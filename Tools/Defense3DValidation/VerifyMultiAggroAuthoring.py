"""Offline wiring/preservation checks for shared aggro; never invokes Unity."""
from pathlib import Path
import re, json, hashlib, yaml
ROOT = Path(__file__).resolve().parents[2]
BASE = Path((ROOT / 'Temp/defense-multi-aggro-baseline-path.txt').read_text(encoding='utf-8-sig').strip())
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
def guid(path): return re.search(r'guid: (\w+)', Path(str(path)+'.meta').read_text()).group(1)
scene_path = Path('Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity')
old, scene = blocks(BASE / scene_path), blocks(ROOT / scene_path)
assert set(old).issubset(scene) and len(scene) == len(old) + 3
for identity in old:
    if identity not in {'8000800021', '8001000002'}:
        assert scene[identity] == old[identity], 'Unexpected scene mutation ' + identity
assert scene['8001000002'][1]['MonoBehaviour']['_friendlyTargets']['fileID'] == 8001500002
assert {'fileID': 8001500001} in scene['8000800021'][1]['Transform']['m_Children']
roster = scene['8001500002'][1]['MonoBehaviour']
assert roster['_hero']['fileID'] == 256135798 and roster['_supports'] == []
assert roster['m_Script']['guid'] == guid(ROOT / 'Assets/Features/Defense3D/DefenseFriendlyTargets3D.cs')
profile_path = ROOT / 'Assets/Resources/Defense3D/Aggro_SongQue.asset'
profile = blocks(profile_path)['11400000'][1]['MonoBehaviour']
assert profile['_acquireRange'] == 4.2 and profile['_retainRange'] == 7
assert profile['_lockSeconds'] == 0.75 and profile['_thinkSeconds'] == 0.25 and profile['_provokedSeconds'] == 3
profile_guid = guid(profile_path)
names = ['DefenseEnemy3D', 'DefenseEnemy_Magiap', 'DefenseEnemy_ThuyQuai', 'DefenseEnemy_QuaiDu', 'DefenseEnemy_HuyetDoi', 'DefenseEnemy_PhiCuong']
for name in names:
    path = Path('Assets/Resources/Defense3D/' + name + '.prefab')
    prefab, original = blocks(ROOT / path), blocks(BASE / path)
    component = '9001300001' if name in names[4:] else '9001500001' if name in names[2:4] else '9001100001'
    assert prefab[component][1]['MonoBehaviour']['_aggroProfile']['guid'] == profile_guid
    assert set(original).issubset(prefab)
    permitted = {'7560621799203833079', '9032791791262103667'} if name in names[2:4] else {component}
    assert len(prefab) == len(original) + (1 if name in names[2:4] else 0)
    for identity in original:
        if identity not in permitted:
            assert prefab[identity] == original[identity], 'Unintended prefab change ' + name + identity
    if name in names[2:4]:
        assert prefab['9032791791262103667'][1]['MonoBehaviour']['_lineOfSightMask']['m_Bits'] == 4105
        assert prefab['4544474906155788650'][1]['NavMeshAgent']['m_Enabled'] == 0
protected = 0
allowed = {'Assets/Resources/Defense3D/' + name + '.prefab' for name in names}
for name, expected in json.loads((BASE / 'manifest.json').read_text()).items():
    if name.startswith('Assets/Resources/Defense3D/') and name not in allowed:
        assert hashlib.sha256((ROOT / name).read_bytes()).hexdigest() == expected, 'Protected resource changed ' + name
        protected += 1
source = ROOT / 'Assets/Features/Defense3D'
run = (source / 'DefenseRun6003D.cs').read_text()
assert run.index('FriendlyTargets.Bind(session)') < run.index('new DefenseEnemySessionFactory3D')
water = (source / 'DefenseWaterMovement3D.cs').read_text()
assert water.index('CurrentPhase = Phase.Ground') < water.index('_groundedAggro.Configure(1, _target, null, true)')
assert '_aggroProfile' in (source / 'DefenseAirMovement3D.cs').read_text()
for name in ['DefenseWaterNavigation3D', 'DefenseCannonBattery3D']:
    code = (source / (name + '.cs')).read_text()
    assert not re.search(r'private\s+(?:readonly\s+)?NavMeshPath\s+\w+\s*=\s*new', code)
print('PASS Shared aggro profile on six prefabs, grounded-only Water handoff and injected Hero/Support roster')
print('PASS Authored Support roster is empty; no unrequested units were added')
print('PASS Protected resource hashes:', protected)
print('NOT_RUN Unity lifecycle, navigation, combat, Support integration and rendered gameplay')
