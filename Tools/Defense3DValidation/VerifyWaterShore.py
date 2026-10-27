"""Offline authored-water wiring checks. Does not run Unity or validate baked navigation."""
from pathlib import Path
import re
import yaml

ROOT = Path(__file__).resolve().parents[2]
SCENE = ROOT / 'Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity'
BASELINE = Path((ROOT / 'Temp/defense-water-baseline-path.txt').read_text(encoding='utf-8-sig').strip())

def blocks(path):
    result = {}
    for kind, identity, body in re.findall(r'--- !u!(\d+) &(-?\d+)(?: stripped)?\n(.*?)(?=--- !u!|\Z)', path.read_text(encoding='utf-8-sig'), re.S):
        assert identity not in result, 'Duplicate fileID ' + identity
        result[identity] = (kind, yaml.safe_load(body))
    return result

scene = blocks(SCENE)
old = blocks(BASELINE / SCENE.relative_to(ROOT))
assert set(old).issubset(scene), 'Removed existing scene object'
assert len(scene) == len(old) + 15, 'Expected seven GameObjects, seven Transforms and navigation component'
allowed = {'8000800021', '8001000002'}
for identity in old:
    if identity not in allowed:
        assert old[identity] == scene[identity], 'Unexpected scene change ' + identity
markers = scene['8000800021'][1]['Transform']
assert {'fileID': 8001200001} in markers['m_Children']
roots = scene['8001000002'][1]['MonoBehaviour']
assert roots['_waterNavigation']['fileID'] == 8001200002
navigation = scene['8001200002'][1]['MonoBehaviour']
assert navigation['_waterBounds']['fileID'] == 2146724234
assert navigation['_groundMask']['m_Bits'] == 4096
assert len(navigation['_exits']) == 3
for exit in navigation['_exits']:
    water = scene[str(exit['Water']['fileID'])][1]['Transform']
    shore = scene[str(exit['Shore']['fileID'])][1]['Transform']
    assert water['m_Father']['fileID'] == shore['m_Father']['fileID'] == 8001200001
    w, s = water['m_LocalPosition'], shore['m_LocalPosition']
    assert 4.45 < w['x'] < 8.05 and s['x'] < 4.45 and w['z'] == s['z']
prefab = blocks(ROOT / 'Assets/Resources/Defense3D/DefenseEnemy_ThuyQuai.prefab')
assert len(prefab) == len(blocks(BASELINE / 'Assets/Resources/Defense3D/DefenseEnemy_ThuyQuai.prefab')) + 1
agents = [value['NavMeshAgent'] for kind, value in prefab.values() if kind == '195']
assert len(agents) == 1 and agents[0]['m_Enabled'] == 0
assert '9001200001' in prefab
for name in ['DefenseWaterNavigation3D', 'DefenseWaterMovement3D']:
    source = ROOT / ('Assets/Features/Defense3D/' + name + '.cs')
    guid = re.search(r'guid: (\w+)', source.with_suffix('.cs.meta').read_text()).group(1)
    assert any(guid in str(value) for _, value in list(scene.values()) + list(prefab.values()))
print('PASS Water exits, scene preservation, disabled agent and authored component references')
print('NOT VERIFIED: Unity movement, collision, baked shore path and visual acceptance')
