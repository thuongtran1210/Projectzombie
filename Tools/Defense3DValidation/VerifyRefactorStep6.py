"""Check authored skill composition, prefab overrides, local refs and the pre-step-6 asset boundary.

Requires PyYAML. Does not load Unity or run physics, shaders or EventSystem callbacks.
"""
from pathlib import Path
import hashlib
import json
import re
import sys
import yaml

root = Path(__file__).resolve().parents[2]
snapshot = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(
    (root/'Temp/defense-refactor-step6-baseline-path.txt').read_text().strip())
scene_path = Path('Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity')
resources = Path('Assets/Resources/Defense3D')

class UniqueLoader(yaml.SafeLoader):
    pass

def mapping(loader, node, deep=False):
    result = {}
    for key_node, value_node in node.value:
        key = loader.construct_object(key_node, deep=deep)
        assert key not in result, f'Duplicate YAML key: {key}'
        result[key] = loader.construct_object(value_node, deep=deep)
    return result

UniqueLoader.add_constructor(yaml.resolver.BaseResolver.DEFAULT_MAPPING_TAG, mapping)

def parse(path):
    text = path.read_text(encoding='utf-8-sig')
    result = {}
    for m in re.finditer(r'--- !u!(\d+) &(\d+)(?: stripped)?\n(.*?)(?=--- !u!|\Z)', text, re.S):
        identifier = int(m[2])
        assert identifier not in result, f'Duplicate YAML ID: {path}: {identifier}'
        result[identifier] = yaml.load(m[3], Loader=UniqueLoader)
    assert result, path
    return result

def data(docs, identifier):
    block = docs[identifier]
    return block[next(iter(block))]

def refs(value):
    if isinstance(value, dict):
        if 'fileID' in value:
            yield value
        else:
            for v in value.values(): yield from refs(v)
    elif isinstance(value, list):
        for v in value: yield from refs(v)

def validate_local(docs):
    for identifier, value in docs.items():
        for ref in refs(value):
            if ref.get('fileID') and 'guid' not in ref:
                assert ref['fileID'] in docs, f'Broken local reference: {identifier} -> {ref}'

scene = parse(root/scene_path)
before = parse(snapshot/scene_path)
validate_local(scene)
changed = {i for i in before if before[i] != scene.get(i)}
assert changed == {256135789, 8000200001, 8000400002, 9223372036854775807}, changed
assert len(scene) - len(before) == 17
for i in changed - {9223372036854775807}:
    assert data(before, i)['m_Script'] == data(scene, i)['m_Script'] if 'm_Script' in data(before, i) else True

manifest = json.loads((snapshot/'manifest.json').read_text())
asset_count = meta_count = 0
for item in manifest:
    relative = Path(item['path'])
    path = root/relative
    assert path.is_file(), f'Missing baseline: {path}'
    if relative.suffix == '.meta' or relative.as_posix().startswith(resources.as_posix()+'/'):
        assert hashlib.sha256(path.read_bytes()).hexdigest() == item['sha256'], f'Changed baseline asset/meta: {relative}'
        if relative.suffix == '.meta': meta_count += 1
        else: asset_count += 1

prefab_path = resources/'DefenseSkillControlsCanvas.prefab'
prefab = parse(root/prefab_path)
validate_local(prefab)
assert len(prefab) == 75
assert data(prefab, 1002)['m_SortingOrder'] < 100
assert data(prefab, 1003)['m_ReferenceResolution'] == {'x': 1920, 'y': 1080}

# Resolve every prefab target referenced by an instance/stripped object, including all scene-specific overrides.
catalog = {}
for path in (root/resources).glob('*'):
    if path.suffix in ('.prefab', '.asset'):
        meta = Path(str(path)+'.meta')
        catalog[re.search(r'^guid: (\w+)', meta.read_text(), re.M)[1]] = parse(path)
for identifier, value in scene.items():
    block = data(scene, identifier)
    for ref in refs(value):
        if ref.get('guid') in catalog:
            assert ref['fileID'] == 100100000 or ref['fileID'] in catalog[ref['guid']], f'Broken prefab/asset target: {ref}'

modifications = data(scene, 8000600000)['m_Modification']['m_Modifications']
overrides = {(m['target']['fileID'], m['propertyPath']): m['objectReference']['fileID'] for m in modifications}
for base, effect, indicator in [(2000,8000500003,8000700002),(3000,8000500004,8000700012)]:
    assert overrides[(base+4, '_projector')] == 8000500001
    assert overrides[(base+4, '_heroHealth')] == 256135798
    for field, target in [('_health',256135798),('_origin',256135790),('_effect',effect),('_indicator',indicator)]:
        assert overrides[(base+6,field)] == target
    assert data(prefab,base+4)['_cancelZone']['fileID'] == base+601
    assert data(prefab,base+5)['_cooldownFill']['fileID'] == base+203
    assert data(prefab,base+5)['_cooldownText']['fileID'] == base+303
    assert data(prefab,base+5)['_lockedOverlay']['fileID'] == base+400
    assert data(prefab,base+600)['m_IsActive'] == 0
    assert data(prefab,base+601)['m_Father']['fileID'] == 1001
    assert data(prefab,base+3)['m_RaycastTarget'] == 1
    assert data(prefab,base+203)['m_Type'] == 3
    for off in [103,203,303,403,503,603,613]:
        assert data(prefab,base+off)['m_RaycastTarget'] == 0
    assert all(m['objectReference']['fileID'] or m['propertyPath']=='m_Name' for m in modifications)

composition = data(scene,8000400002)
assert len(composition['_sceneParticipants']) == 17
assert composition['_sceneParticipants'][:15] == data(before,8000400002)['_sceneParticipants']
assert composition['_skillPresenters'] == [{'fileID':8000600002},{'fileID':8000600003}]
assert data(scene,8000500001)['_camera']['fileID'] == 1990408784
assert data(scene,8000500001)['_groundMask']['m_Bits'] == 4096
assert data(scene,8000500002)['_obstacleMask']['m_Bits'] == 25
assert data(scene,8000500002)['_requireNavMesh'] == 1
assert data(scene,8000500004)['_impactView']['fileID'] == 8000700022
assert data(scene,256135794)['m_Direction'] == 1 and data(scene,256135789)['m_Layer'] == 6
for modification in data(scene,8000200001)['m_Modification']['m_Modifications']:
    if modification['propertyPath'].startswith('m_LocalScale.'):
        assert modification['value'] == 1 or str(modification['value']) == '1'
roots = data(scene,9223372036854775807)['m_Roots']
for identifier in [8000600000,8000700000,8000700010,8000700020]: assert {'fileID':identifier} in roots

dash = data(parse(root/resources/'Skill_Dash.asset'),11400000)
relic = data(parse(root/resources/'Skill_DepToOng.asset'),11400000)
assert (dash['_range'],dash['_cooldown'],dash['_duration']) == (4.5,3,0.25)
assert (relic['_range'],relic['_radius'],relic['_damage'],relic['_cooldown']) == (10,3.5,180,12)
for name in ['DefenseDashTraversal3D','DefenseDashSkillEffect3D','DefenseRelicSkillEffect3D','UI/DefenseSkillPresenter3D','UI/DefenseSkillButtonView']:
    text = (root/'Assets/Features/Defense3D'/(name+'.cs')).read_text()
    assert not re.search(r'\b(?:Instantiate|Destroy)\s*\(', text), name
    for forbidden in ('new GameObject', 'AddComponent', 'new Material', 'Physics2D.', 'FindObjectOfType'):
        assert forbidden not in text, f'{name}: {forbidden}'
    assert Path(str(root/'Assets/Features/Defense3D'/(name+'.cs'))+'.meta').exists()

total = 0
for name, count in [('DefenseSessionChecks',23),('DefenseCombatChecks',35),('DefenseAbilityInputChecks',27),
                    ('DefenseGestureAimChecks',39),('DefenseSkillCastChecks',22)]:
    log = (root/f'Logs/TraversalQA/defense-refactor-step6-{name}.txt').read_text(encoding='utf-8-sig')
    assert f'TOTAL {count} PASS' in log
    total += count
for suffix in ['runtime-compile','source-compatibility-compile']:
    assert 'PASS ' in (root/f'Logs/TraversalQA/defense-refactor-step6-{suffix}.txt').read_text(encoding='utf-8-sig')

report = '\n'.join([
    f'PASS {asset_count} existing resource assets and {meta_count} metadata files unchanged byte-for-byte',
    'PASS scene changes limited to Hero components, composition bindings, Canvas scale overrides and new roots',
    'PASS 75-block authored controls prefab: local refs, separate cancel zones, radial fills, TMP labels and decorative raycast flags',
    'PASS scene prefab overrides resolve both controls to Hero/projector/effect/indicator; 17 participants and 2 presenters bound',
    'PASS 3 authored indicator instances for Dash aim, Dép aim and Dép impact; no runtime UI/VFX object creation',
    'PASS Ground/Obstacle/Water masks, capsule and baked NavMesh requirement serialized',
    'PASS Dash/Dep definition assets preserve prototype parameters and are snapshotted for runtime',
    f'PASS {total} standalone core checks and runtime/editor/fixture source compilation',
    'NOT_RUN Unity import/test runner, physical dash/raycast/NavMesh, touch/visuals/shaders and Android',
])
(root/'Logs/TraversalQA/defense-refactor-step6-static-checks.txt').write_text(report+'\n',encoding='utf-8')
print(report)
