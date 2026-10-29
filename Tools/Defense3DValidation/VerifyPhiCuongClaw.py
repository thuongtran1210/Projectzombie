"""Offline Claw wiring/shader preservation checks. No Unity invocation."""
from pathlib import Path
import re, json, hashlib, yaml
ROOT = Path(__file__).resolve().parents[2]
BASE = Path((ROOT / 'Temp/defense-claw-baseline-path.txt').read_text(encoding='utf-8-sig').strip())
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
path = Path('Assets/Resources/Defense3D/DefenseEnemy_PhiCuong.prefab')
prefab, old = blocks(ROOT / path), blocks(BASE / path)
assert set(old).issubset(prefab) and len(prefab) == len(old) + 6
allowed = {'7560621799203833079', '2235734807898763793', '9032791791262103667'}
for identity in old:
    if identity not in allowed: assert prefab[identity] == old[identity], 'Unexpected prefab mutation ' + identity
assert prefab['9032791791262103667'][1]['MonoBehaviour']['_coneAttack']['fileID'] == 9001700001
assert prefab['9001700001'][1]['MonoBehaviour']['_view']['fileID'] == 9001700002
view = prefab['9001700002'][1]['MonoBehaviour']
assert view['_indicator']['fileID'] == 9001700011 and view['_fill']['fileID'] == 9001700013
assert prefab['9001700010'][1]['GameObject']['m_IsActive'] == 0
assert prefab['9001700013'][1]['MeshRenderer']['m_Enabled'] == 1
assert prefab['9001700012'][1]['MeshFilter']['m_Mesh']['fileID'] == 10210
profile_path = ROOT / 'Assets/Resources/Defense3D/PhiCuong_Claw.asset'
profile = blocks(profile_path)['11400000'][1]['MonoBehaviour']
assert profile['_windup'] == 0.65 and profile['_radius'] == 2.5 and profile['_angle'] == 120
assert profile['_lineOfSightMask']['m_Bits'] == 4105
assert prefab['9001700001'][1]['MonoBehaviour']['_profile']['guid'] == guid(profile_path)
material_path = ROOT / 'Assets/Resources/Defense3D/PhiCuong_Telegraph.mat'
mat = blocks(material_path)['2100000'][1]['Material']
values = {key: value for item in mat['m_SavedProperties']['m_Floats'] for key, value in item.items()}
assert values['_Shape'] == 5 and values['_SectorCos'] == 0.5 and values['_UseVertexColor'] == 0
assert mat['m_Shader']['guid'] == guid(ROOT / 'Assets/Shaders/VFX/DefenseAimOverlay.shader')
assert prefab['9001700013'][1]['MeshRenderer']['m_Materials'][0]['guid'] == guid(material_path)
shader_path = Path('Assets/Shaders/VFX/DefenseAimOverlay.shader')
shader = (ROOT / shader_path).read_text(encoding='utf-8-sig')
assert '"LightMode" = "SRPDefaultUnlit"' in shader and 'ZTest Always' in shader and 'ZWrite Off' in shader
legacy = shader.replace('Arrow,3,Triangle,4,Sector,5)', 'Arrow,3,Triangle,4)')
legacy = legacy.replace('        _SectorCos("Sector Half Angle Cosine", Range(-1,1)) = 0.5\n', '')
legacy = legacy.replace('                float _SectorCos;\n', '')
sector = """                    else if (_Shape > 4.5)
                    {
                        float directionCos = p.y / max(radius,0.0001);
                        float angularAA = max(fwidth(directionCos),0.001);
                        float angular = radius < 0.001 ? 1.0 : smoothstep(_SectorCos-angularAA,_SectorCos+angularAA,directionCos);
                        coverage = (1.0-smoothstep(1.0-aa,1.0,radius)) * angular;
                    }
"""
legacy = legacy.replace(sector, '')
assert legacy == (BASE / shader_path).read_text(encoding='utf-8-sig'), 'Unexpected change to existing indicator shader paths'
protected = 0
for name, expected in json.loads((BASE / 'manifest.json').read_text()).items():
    if name.startswith('Assets/Resources/Defense3D/') and name != path.as_posix():
        assert hashlib.sha256((ROOT / name).read_bytes()).hexdigest() == expected, 'Protected resource changed ' + name
        protected += 1
scene = Path('Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity')
assert (ROOT / scene).read_bytes() == (BASE / scene).read_bytes()
source = ROOT / 'Assets/Features/Defense3D'
attack = (source / 'DefenseEnemyAttack3D.cs').read_text()
assert attack.index('if (IsWindingUp)') < attack.index('if (GameplayTime < nextAttackTime)')
assert 'if (_coneAttack.TryBegin(target, _targetCollider)) nextAttackTime' in attack
assert 'CancelPendingAttack' in (source / 'DefenseEnemyStatus3D.cs').read_text()
assert 'if (_attack.IsWindingUp)' in (source / 'DefenseAirMovement3D.cs').read_text()
cone_source = (source / 'DefenseConeAttack3D.cs').read_text()
assert 'ReferenceEquals(_enemy.Lifetime, lifetime)' in cone_source and 'nextAttackTime' not in cone_source
for name in ['DefenseConeAttack3D', 'DefenseConeTelegraphView3D']:
    code = (source / (name + '.cs')).read_text()
    for forbidden in ['new GameObject(', 'new Material(', 'new Mesh(']: assert forbidden not in code
print('PASS Authored Phi Cuong cone/profile/view/material refs and single cooldown owner wiring')
print('PASS Existing indicator shader paths and SRPDefaultUnlit rendering pass preserved')
print('PASS Scene unchanged; protected resource hashes:', protected)
print('NOT_RUN Shader compilation/rendering, Unity lifecycle, actual LOS/cone damage, interruption and cooldown')
