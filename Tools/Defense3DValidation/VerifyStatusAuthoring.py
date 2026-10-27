"""Offline status authoring/preservation checks. Does not run Unity."""
from pathlib import Path
import re, json, hashlib, yaml
ROOT = Path(__file__).resolve().parents[2]
BASE = Path((ROOT / 'Temp/defense-status-baseline-path.txt').read_text(encoding='utf-8-sig').strip())
class UniqueLoader(yaml.SafeLoader): pass
def mapping(loader, node, deep=False):
    result = {}
    for key_node, value_node in node.value:
        key = loader.construct_object(key_node, deep=deep)
        assert key not in result, 'Duplicate YAML key ' + str(key)
        result[key] = loader.construct_object(value_node, deep=deep)
    return result
UniqueLoader.add_constructor(yaml.resolver.BaseResolver.DEFAULT_MAPPING_TAG, mapping)
def blocks(path):
    result = {}
    for kind, identity, body in re.findall(r'--- !u!(\d+) &(-?\d+)(?: stripped)?\n(.*?)(?=--- !u!|\Z)', path.read_text(encoding='utf-8-sig'), re.S):
        assert identity not in result, 'Duplicate fileID ' + identity
        result[identity] = (kind, yaml.load(body, Loader=UniqueLoader))
    return result
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
assert set(old) == set(scene)
for identity in old:
    if identity != '8000500004': assert scene[identity] == old[identity], 'Unexpected scene edit ' + identity
profile_path = ROOT / 'Assets/Resources/Defense3D/RelicStatus_SongQue.asset'
assert scene['8000500004'][1]['MonoBehaviour']['_hitStatuses']['guid'] == guid(profile_path)
profile = blocks(profile_path)['11400000'][1]['MonoBehaviour']
assert profile['_slowDuration'] == 2 and profile['_slowStrength'] == 0.35
assert profile['_stunDuration'] == 0.25 and profile['_knockDuration'] == 0.2 and profile['_knockStrength'] == 0.5
names = ['DefenseEnemy3D', 'DefenseEnemy_Magiap', 'DefenseEnemy_ThuyQuai', 'DefenseEnemy_QuaiDu', 'DefenseEnemy_HuyetDoi', 'DefenseEnemy_PhiCuong']
for name in names:
    path = Path('Assets/Resources/Defense3D/' + name + '.prefab')
    prefab, original = blocks(ROOT / path), blocks(BASE / path)
    assert set(original).issubset(prefab) and len(prefab) == len(original) + 1
    root = next(identity for identity, (kind, data) in original.items() if kind == '1')
    for identity in original:
        if identity != root: assert prefab[identity] == original[identity], 'Unexpected actor mutation ' + name + identity
    status = prefab['9001600001'][1]['MonoBehaviour']
    assert status['m_GameObject']['fileID'] == int(root)
    assert {'component': {'fileID': 9001600001}} in prefab[root][1]['GameObject']['m_Component']
    assert status['m_Script']['guid'] == guid(ROOT / 'Assets/Features/Defense3D/DefenseEnemyStatus3D.cs')
    assert status['_maxDuration'] == 10 and status['_maximumKnockDistance'] == 2 and status['_slowCapacity'] == 8
    assert status['_displacementMask']['m_Bits'] == 4105
protected = 0
allowed = {'Assets/Resources/Defense3D/' + name + '.prefab' for name in names}
for name, expected in json.loads((BASE / 'manifest.json').read_text()).items():
    if name.startswith('Assets/Resources/Defense3D/') and name not in allowed:
        assert hashlib.sha256((ROOT / name).read_bytes()).hexdigest() == expected, 'Protected resource changed ' + name
        protected += 1
source = ROOT / 'Assets/Features/Defense3D'
assert 'NavMesh.Raycast(start.position, end.position' in (source / 'DefenseGroundMovement3D.cs').read_text()
assert '_navigation.Contains(transform.position + delta)' in (source / 'DefenseWaterMovement3D.cs').read_text()
assert '_navigation.CanMove(transform.position, transform.position + delta)' in (source / 'DefenseAirMovement3D.cs').read_text()
assert 'enemy.Status.BlocksActions' in (source / 'DefenseEnemyAttack3D.cs').read_text()
for name in ['DefenseWaterNavigation3D', 'DefenseCannonBattery3D']:
    code = (source / (name + '.cs')).read_text()
    assert not re.search(r'private\s+(?:readonly\s+)?NavMeshPath\s+\w+\s*=\s*new', code)
print('PASS Authored status receivers on six ordinary prefabs and post-hit Relic profile binding')
print('PASS Ground/Water/Air displacement guards and attack interruption wiring')
print('PASS Protected resource hashes:', protected)
print('NOT_RUN Unity physics, NavMesh Warp/path restoration, status movement/attack and rendered gameplay')
