"""Offline authoring/protection checks. Never invokes Unity."""
from pathlib import Path
import re
import json
import hashlib
import yaml

ROOT = Path(__file__).resolve().parents[2]
BASE = Path((ROOT / 'Temp/defense-air-baseline-path.txt').read_text(encoding='utf-8-sig').strip())

class UniqueLoader(yaml.SafeLoader):
    pass

def mapping(loader, node, deep=False):
    value = {}
    for key_node, value_node in node.value:
        key = loader.construct_object(key_node, deep=deep)
        assert key not in value, 'Duplicate YAML key ' + str(key)
        value[key] = loader.construct_object(value_node, deep=deep)
    return value
UniqueLoader.add_constructor(yaml.resolver.BaseResolver.DEFAULT_MAPPING_TAG, mapping)

def blocks(path):
    result = {}
    for kind, identity, body in re.findall(r'--- !u!(\d+) &(-?\d+)(?: stripped)?\n(.*?)(?=--- !u!|\Z)', path.read_text(encoding='utf-8-sig'), re.S):
        assert identity not in result, 'Duplicate fileID ' + identity
        result[identity] = (kind, yaml.load(body, Loader=UniqueLoader))
    return result

def asset(name):
    return blocks(ROOT / ('Assets/Resources/Defense3D/' + name + '.asset'))['11400000'][1]['MonoBehaviour']

def guid(path):
    p = Path(path)
    if not p.exists():
        matches = list((ROOT / 'Assets/Features/Defense3D').rglob(p.name))
        if matches: p = matches[0]
    meta = p.parent / (p.name + '.meta')
    for line in meta.read_text().splitlines():
        if line.startswith('guid: '): return line.split()[1]
    raise AssertionError('No guid in ' + str(meta))

scene_path = Path('Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity')
old, scene = blocks(BASE / scene_path), blocks(ROOT / scene_path)
assert set(old).issubset(scene) and len(scene) == len(old) + 4
allowed = {'8000800021', '8001000002', '256135796'}
for identity in old:
    if identity not in allowed:
        assert old[identity] == scene[identity], 'Unexpected scene mutation ' + identity
assert scene['8001000002'][1]['MonoBehaviour']['_airNavigation']['fileID'] == 8001300002
assert {'fileID': 8001300001} in scene['8000800021'][1]['Transform']['m_Children']
volume = scene['8001300003'][1]['BoxCollider']
assert volume['m_IsTrigger'] == 1 and volume['m_Size'] == {'x': 60, 'y': 5, 'z': 40}
profile = asset('Air_SongQue')
assert profile['_minimumHeight'] <= profile['_cruiseHeight'] <= profile['_maximumHeight']
assert profile['_clearanceRadius'] >= 0.65 and profile['_obstacleMask']['m_Bits'] == 4105
assert scene['8001300002'][1]['MonoBehaviour']['_profile']['guid'] == guid(ROOT / 'Assets/Resources/Defense3D/Air_SongQue.asset')
for name in ['HuyetDoi', 'PhiCuong']:
    path = Path('Assets/Resources/Defense3D/DefenseEnemy_' + name + '.prefab')
    prefab, original = blocks(ROOT / path), blocks(BASE / path)
    assert set(original).issubset(prefab) and len(prefab) == len(original) + 2
    permitted = {'7560621799203833079', '4544474906155788650', '9032791791262103667'}
    for identity in original:
        if identity not in permitted:
            assert original[identity] == prefab[identity], 'Unintended prefab mutation ' + name + identity
    assert prefab['4544474906155788650'][1]['NavMeshAgent']['m_Enabled'] == 0
    root = prefab['7560621799203833079'][1]['GameObject']
    assert root['m_Layer'] == 7
    collider = prefab['9001300002'][1]['SphereCollider']
    assert collider['m_IsTrigger'] == collider['m_Enabled'] == 1
    assert collider['m_Radius'] == 0.5
    attack = prefab['9032791791262103667'][1]['MonoBehaviour']
    assert attack['_useSpatialRange'] == 1 and attack['_lineOfSightMask']['m_Bits'] == 4105
    script = ROOT / 'Assets/Features/Defense3D/DefenseAirMovement3D.cs'
    assert prefab['9001300001'][1]['MonoBehaviour']['m_Script']['guid'] == guid(script)
    assert 'NavMesh.SamplePosition' not in script.read_text()
for name in ['Hero', 'Tower']:
    targeting = asset('Targeting_' + name)
    assert targeting['_targets'] == 3 and targeting['_lineOfSightMask']['m_Bits'] == 4105
assert scene['256135796'][1]['MonoBehaviour']['_targeting']['guid'] == guid(ROOT / 'Assets/Resources/Defense3D/Targeting_Hero.asset')
tower = blocks(ROOT / 'Assets/Resources/Defense3D/DefenseTower3D.prefab')['1005'][1]['MonoBehaviour']
assert tower['_targeting']['guid'] == guid(ROOT / 'Assets/Resources/Defense3D/Targeting_Tower.asset')
allowed_resources = {'Assets/Resources/Defense3D/DefenseEnemy_HuyetDoi.prefab',
    'Assets/Resources/Defense3D/DefenseEnemy_PhiCuong.prefab', 'Assets/Resources/Defense3D/DefenseTower3D.prefab'}
manifest = json.loads((BASE / 'manifest.json').read_text())
protected = 0
for name, expected in manifest.items():
    if name.startswith('Assets/Resources/Defense3D/') and name not in allowed_resources:
        assert hashlib.sha256((ROOT / name).read_bytes()).hexdigest() == expected, 'Protected asset changed ' + name
        protected += 1
print('PASS Flight volume, disabled Air agents, hitboxes, spatial/LOS targeting, scene/prefab preservation')
print('PASS Protected resource hashes:', protected)
print('NOT_RUN Unity import, movement/avoidance, physics LOS, hitbox and camera acceptance')
