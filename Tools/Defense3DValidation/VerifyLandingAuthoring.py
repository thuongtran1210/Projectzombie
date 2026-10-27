"""Offline cannon/landing wiring validation. Does not invoke Unity."""
from pathlib import Path
import re, json, hashlib, yaml
ROOT = Path(__file__).resolve().parents[2]
BASE = Path((ROOT / 'Temp/defense-landing-baseline-path.txt').read_text(encoding='utf-8-sig').strip())
class UniqueLoader(yaml.SafeLoader): pass
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
def guid(path):
    return re.search(r'guid: (\w+)', Path(str(path)+'.meta').read_text()).group(1)
scene_path = Path('Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity')
old, scene = blocks(BASE / scene_path), blocks(ROOT / scene_path)
assert set(old).issubset(scene) and len(scene) == len(old) + 18
for identity in old:
    if identity not in {'8000800031', '8001000002'}:
        assert old[identity] == scene[identity], 'Unexpected scene edit ' + identity
assert scene['8001000002'][1]['MonoBehaviour']['_cannonBattery']['fileID'] == 8001400002
assert {'fileID': 8001400001} in scene['8000800031'][1]['Transform']['m_Children']
battery = scene['8001400002'][1]['MonoBehaviour']
assert len(battery['_cannons']) == 3
assert battery['_groundMask']['m_Bits'] == 4096
prefab_path = ROOT / 'Assets/Resources/Defense3D/DefenseGhostCannon3D.prefab'
prefab = blocks(prefab_path)
assert len(prefab) == 16
assert prefab['1101'][1]['GameObject']['m_IsActive'] == prefab['1201'][1]['GameObject']['m_IsActive'] == 0
cannon = prefab['1006'][1]['MonoBehaviour']
assert cannon['_projectile']['fileID'] == 1102 and cannon['_canopy']['fileID'] == 1201
assert prefab['1005'][1]['SphereCollider']['m_Enabled'] == 1
ids, anchors = set(), set()
for index, identity in enumerate([8001400100, 8001400200, 8001400300]):
    instance = scene[str(identity)][1]['PrefabInstance']
    assert instance['m_SourcePrefab']['guid'] == guid(prefab_path)
    assert instance['m_Modification']['m_TransformParent']['fileID'] == 8001400001
    changes = {item['propertyPath']: item for item in instance['m_Modification']['m_Modifications']}
    assert changes['m_LocalPosition.x']['value'] > 8.05
    anchor_id = changes['_landingAnchor']['objectReference']['fileID']
    anchor = scene[str(anchor_id)][1]['Transform']
    assert anchor['m_LocalPosition']['x'] < 4.45 and anchor['m_LocalPosition']['y'] == 0
    ids.add(changes['_cannonId']['value']); anchors.add(anchor_id)
    assert battery['_cannons'][index]['fileID'] == identity + 2
assert len(ids) == len(anchors) == 3
quai_path = Path('Assets/Resources/Defense3D/DefenseEnemy_QuaiDu.prefab')
quai, original = blocks(ROOT / quai_path), blocks(BASE / quai_path)
assert set(quai) == set(original)
for identity in original:
    if original[identity][0] == '195':
        agent = quai[identity][1]['NavMeshAgent']
        assert agent['m_Enabled'] == 0
        agent['m_Enabled'] = original[identity][1]['NavMeshAgent']['m_Enabled']
    assert quai[identity] == original[identity], 'Unexpected QuaiDu prefab edit ' + identity
protected = 0
for name, expected in json.loads((BASE / 'manifest.json').read_text()).items():
    if name.startswith('Assets/Resources/Defense3D/') and name != quai_path.as_posix():
        assert hashlib.sha256((ROOT / name).read_bytes()).hexdigest() == expected, 'Protected asset changed ' + name
        protected += 1
for name in ['DefenseCannonBattery3D', 'DefenseWaterNavigation3D']:
    source = (ROOT / ('Assets/Features/Defense3D/' + name + '.cs')).read_text()
    assert not re.search(r'private\s+(?:readonly\s+)?NavMeshPath\s+\w+\s*=\s*new', source)
    assert 'Awake() => _path = new NavMeshPath();' in source
print('PASS Three linked Cannon prefab instances, authored projectile/canopy, West anchors and bindings')
print('PASS Native path creation stays in Awake; landing agent is disabled until owned acquisition')
print('PASS Protected resource hashes:', protected)
print('NOT_RUN Unity import, physics, navigation, rendered flight and gameplay acceptance')
