"""Verify the MapSongQue hierarchy consolidation against its saved baseline, without Unity."""
import copy
import hashlib
import json
from pathlib import Path
import re
import sys
import yaml

root = Path(__file__).resolve().parents[2]
baseline = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(
    (root/'Temp/defense-hierarchy-baseline-path.txt').read_text().strip())
scene_path = Path('Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity')

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
    docs, raw = {}, {}
    for m in re.finditer(r'--- !u!(\d+) &(-?\d+)(?: stripped)?\n(.*?)(?=--- !u!|\Z)', text, re.S):
        identifier = int(m[2])
        assert identifier not in docs, f'Duplicate fileID: {identifier}'
        docs[identifier] = yaml.load(m[3], Loader=UniqueLoader)
        raw[identifier] = m[3]
    return docs, raw

def value(docs, identifier):
    doc = docs[identifier]
    return doc[next(iter(doc))]

def refs(obj):
    if isinstance(obj, dict):
        if 'fileID' in obj: yield obj
        else:
            for v in obj.values(): yield from refs(v)
    elif isinstance(obj, list):
        for v in obj: yield from refs(v)

old, old_raw = parse(baseline/scene_path)
new, new_raw = parse(root/scene_path)
assert set(old) <= set(new), 'Existing IDs were removed.'
added = {8000800000,8000800001,8000800010,8000800011,8000800020,8000800021,8000800030,8000800031,8000800051}
assert set(new) - set(old) == added
for identifier, doc in new.items():
    for ref in refs(doc):
        if ref['fileID'] and 'guid' not in ref:
            assert ref['fileID'] in new, f'Broken local reference {identifier}: {ref}'

root_transform = value(new,620079490)
assert value(new,620079488)['m_Name'] == 'MapSongQue'
assert root_transform['m_LocalPosition'] == {'x':0,'y':0,'z':0}
assert root_transform['m_LocalRotation'] == {'x':0,'y':0,'z':0,'w':1}
assert root_transform['m_LocalScale'] == {'x':1,'y':1,'z':1}
assert root_transform['m_Children'] == [{'fileID':i} for i in [8000800001,8000800011,8000800021,8000800031,201442793]]
groups = {
    8000800001: ('Visuals',[8000800051,1311251764]),
    8000800011: ('Collision',[1907199476,2118810462,2076928680,2052115105,1982675315,2146724231]),
    8000800021: ('GameplayMarkers',[1090862958,1228108700,455539703,1376125754,156792712,959971192,
                                   1834248032,1445426050,1518703982,1632943169,1531786202]),
    8000800031: ('Structures',[873313247,8000200021]),
}
reparented = {201442793}
for tid, (name, children) in groups.items():
    transform = value(new,tid)
    assert value(new,transform['m_GameObject']['fileID'])['m_Name'] == name
    assert transform['m_Father']['fileID'] == 620079490
    for field in ('m_LocalPosition','m_LocalRotation','m_LocalScale'):
        assert transform[field] == root_transform[field], f'Non-identity group: {name}'
    assert transform['m_Children'] == [{'fileID':i} for i in children]
    for child in children:
        if child == 8000800051: continue
        assert value(new,child)['m_Father']['fileID'] == tid
        reparented.add(child)

assert value(new,201442791)['m_Name'] == 'Navigation'
assert value(new,201442793)['m_Father']['fileID'] == 620079490
model = value(new,8000800051)
assert model['m_CorrespondingSourceObject'] == {
    'fileID':6555043479735266583,'guid':'75295593cbbbf914785cdcbb1ddf3f70','type':3}
assert model['m_PrefabInstance']['fileID'] == 831596297
assert value(new,831596297)['m_Modification']['m_TransformParent']['fileID'] == 8000800001
old_model = copy.deepcopy(value(old,831596297))
new_model = copy.deepcopy(value(new,831596297))
new_model['m_Modification']['m_TransformParent'] = old_model['m_Modification']['m_TransformParent']
assert old_model == new_model, 'GLB import/rotation/position/component overrides changed.'

allowed = reparented | {620079488,620079490,201442791,831596297,9223372036854775807}
for identifier in old:
    if identifier not in allowed:
        assert old_raw[identifier] == new_raw[identifier], f'Unexpected change to block {identifier}'
for identifier in reparented | {620079490}:
    a, b = copy.deepcopy(value(old,identifier)), copy.deepcopy(value(new,identifier))
    b['m_Father'] = a['m_Father']
    if identifier == 620079490: b['m_Children'] = a['m_Children']
    assert a == b, f'Transform data changed beyond parent/children: {identifier}'
for identifier in [620079488,201442791]:
    a, b = copy.deepcopy(value(old,identifier)), copy.deepcopy(value(new,identifier))
    b['m_Name'] = a['m_Name']
    assert a == b, f'Object data changed beyond name: {identifier}'
old_roots = value(old,9223372036854775807)['m_Roots']
new_roots = value(new,9223372036854775807)['m_Roots']
assert new_roots == [v for v in old_roots if v['fileID'] not in [831596297,201442793,8000200021]]

# Identity groups and unchanged local TRS prove existing world poses are unchanged.
for identifier in reparented:
    old_parent = value(old,identifier)['m_Father']['fileID']
    assert old_parent in (0,620079490)
assert value(old,620079490)['m_LocalPosition'] == root_transform['m_LocalPosition']
assert value(old,620079490)['m_LocalRotation'] == root_transform['m_LocalRotation']
assert value(old,620079490)['m_LocalScale'] == root_transform['m_LocalScale']

protected = 0
for item in json.loads((baseline/'manifest.json').read_text()):
    if item['path'] == scene_path.as_posix(): continue
    path = root/item['path']
    assert path.is_file() and hashlib.sha256(path.read_bytes()).hexdigest() == item['sha256'], f'Protected file changed: {path}'
    protected += 1

report = '\n'.join([
    f'PASS {len(new)} unique scene blocks, all existing IDs retained and all local references resolve',
    'PASS MapSongQue root has Visuals / Collision / GameplayMarkers / Structures / Navigation',
    'PASS GLB remains a linked prefab with identical rotation, position and component overrides',
    'PASS old local TRS unchanged; identity parent groups preserve world poses for map, markers, Base, towers and navigation',
    'PASS all existing script, collider, renderer, NavMesh, skill/UI/session bindings unchanged byte-for-byte',
    f'PASS {protected} protected source/resource/model/NavMesh/metadata files unchanged by SHA256',
    'PASS Hero, camera, session, UI, lights and skill indicators remain outside the static map root',
    'NOT_RUN Unity import, rendering, physics, NavMesh bake or Unity tests; this is hierarchy organization only',
])
(root/'Logs/TraversalQA/defense-map-hierarchy-static-checks.txt').write_text(report+'\n',encoding='utf-8')
print(report)
