"""Static Vongxuyen skill-control/foot authoring checks; never invokes Unity."""
from pathlib import Path
import hashlib
import json
import re
import sys
import yaml

root = Path(__file__).resolve().parents[2]
baseline = Path(sys.argv[1]) if len(sys.argv)>1 else Path(
    (root/'Temp/defense-vongxuyen-controls-baseline-path.txt').read_text().strip())
resources = root/'Assets/Resources/Defense3D'

class UniqueLoader(yaml.SafeLoader):
    pass

def mapping(loader,node,deep=False):
    result={}
    for kn,vn in node.value:
        k=loader.construct_object(kn,deep=deep)
        assert k not in result, f'Duplicate YAML key {k}'
        result[k]=loader.construct_object(vn,deep=deep)
    return result

UniqueLoader.add_constructor(yaml.resolver.BaseResolver.DEFAULT_MAPPING_TAG,mapping)

def parse(path):
    result={}
    for m in re.finditer(r'--- !u!(\d+) &(-?\d+)(?: stripped)?\n(.*?)(?=--- !u!|\Z)',path.read_text(encoding='utf-8-sig'),re.S):
        i=int(m[2]);assert i not in result,f'Duplicate YAML ID {path}: {i}'
        result[i]=yaml.load(m[3],Loader=UniqueLoader)
    return result

def val(d,i): return next(iter(d[i].values()))

def refs(value):
    if isinstance(value,dict):
        if 'fileID' in value:yield value
        else:
            for v in value.values():yield from refs(v)
    elif isinstance(value,list):
        for v in value:yield from refs(v)

def local_refs(d):
    for i,v in d.items():
        for ref in refs(v):
            assert not ref.get('fileID') or 'guid' in ref or ref['fileID'] in d,f'Broken local ref {i}: {ref}'

scene_path=Path('Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity')
scene=parse(root/scene_path);old=parse(baseline/scene_path)
controls=parse(resources/'DefenseSkillControlsCanvas.prefab')
indicator=parse(resources/'DefenseAimIndicator3D.prefab')
foot=parse(resources/'DefenseHeroFootIndicator3D.prefab')
for doc in (scene,controls,indicator,foot):local_refs(doc)
assert len(controls)==101 and len(indicator)==21 and len(foot)==17
assert {i for i in old if old[i]!=scene.get(i)}=={8000400002,9223372036854775807}
assert len(scene)-len(old)==3
assert len(val(scene,8000400002)['_sceneParticipants'])==18
assert val(scene,8000400002)['_sceneParticipants'][:-1]==val(old,8000400002)['_sceneParticipants']
assert val(scene,8000400002)['_sceneParticipants'][-1]=={'fileID':8000900002}
assert val(scene,9223372036854775807)['m_Roots'][-1]=={'fileID':8000900000}
mods=val(scene,8000900000)['m_Modification']['m_Modifications']
assert {(m['propertyPath'],m['objectReference']['fileID']) for m in mods}=={('_hero',256135790),('_health',256135798)}
assert val(foot,7002)['_visual']['fileID']==7101
assert val(foot,7002)['_groundMask']['m_Bits']==4096
assert val(foot,7100)['m_IsActive']==0
assert val(foot,7201)['m_LocalScale']['x']==0.88
assert val(foot,7301)['m_LocalScale']['x']==0.72
assert val(foot,7401)['m_LocalPosition']['z']==0.44

for base,pad,x in [(2000,4800,-315),(3000,5800,-120)]:
    b=val(controls,base+4)
    assert b['_dragThreshold']==6 and b['_maxDragDistance']==52 and b['_minimumDragDistance']==1.5
    assert b['_stick']['fileID']==base+7
    stick=val(controls,base+7)
    assert stick['_pad']['fileID']==pad+1 and stick['_thumb']['fileID']==pad+11
    assert stick['_padGraphic']['fileID']==pad+23
    assert val(controls,pad)['m_IsActive']==0
    assert val(controls,pad+1)['m_Father']['fileID']==1001
    assert val(controls,pad+21)['m_AnchorMin']=={'x':0,'y':0}
    assert val(controls,pad+21)['m_AnchorMax']=={'x':1,'y':1}
    for i in [pad+3,pad+13,pad+23,base+603,base+613]:assert val(controls,i)['m_RaycastTarget']==0
    cancel=val(controls,base+601)
    assert cancel['m_SizeDelta']=={'x':96,'y':96}
    assert cancel['m_AnchoredPosition']=={'x':x,'y':282}
    assert val(controls,base+613)['m_text']=='×\nHỦY'

for field,i in [('_rangeRing',4003),('_rangeFill',5003),('_arrow',6003)]:
    assert val(indicator,1002)[field]['fileID']==i
    assert val(indicator,i)['m_Enabled']==0
shader=(root/'Assets/Shaders/VFX/DefenseAimOverlay.shader').read_text()
assert 'ZTest Always' in shader and '_UseVertexColor' in shader and 'fwidth' in shader
for p in resources.glob('DefenseAim_*.mat'):
    material=val(parse(p),2100000)
    names=[next(iter(x)) for x in material['m_SavedProperties']['m_Floats']]
    assert len(names)==len(set(names)),f'Duplicate material property: {p}'
    assert '_Shape' in names
    assert Path(str(p)+'.meta').is_file()

allowed={
    'Assets/Features/UI/Controls/SkillPointerGesture.cs',
    'Assets/Features/Defense3D/DefenseSkillGestureSession.cs',
    'Assets/Features/Defense3D/DefenseAimResolver.cs',
    'Assets/Features/Defense3D/DefenseAimProjector3D.cs',
    'Assets/Features/Defense3D/DefenseHeroMotor3D.cs',
    'Assets/Features/Defense3D/UI/DefenseSkillAimButton3D.cs',
    'Assets/Features/Defense3D/UI/DefenseSkillPresenter3D.cs',
    'Assets/Features/Defense3D/UI/DefenseAimIndicator3D.cs',
    'Assets/Resources/Defense3D/DefenseSkillControlsCanvas.prefab',
    'Assets/Resources/Defense3D/DefenseAimIndicator3D.prefab',
    scene_path.as_posix(), 'Assets/Shaders/VFX/DefenseAimOverlay.shader'
}
preserved=0
for item in json.loads((baseline/'manifest.json').read_text()):
    if item['path'] in allowed:continue
    p=root/item['path'];assert p.is_file() and hashlib.sha256(p.read_bytes()).hexdigest()==item['sha256'],item['path']
    preserved+=1
total=0
for name,count in [('DefenseSessionChecks',23),('DefenseCombatChecks',35),('DefenseAbilityInputChecks',27),
                   ('DefenseGestureAimChecks',39),('DefenseSkillCastChecks',22),('DefenseVongxuyenControlChecks',14)]:
    assert f'TOTAL {count} PASS' in (root/f'Logs/TraversalQA/defense-vongxuyen-controls-{name}.txt').read_text(encoding='utf-8-sig')
    total+=count

report='\n'.join([
    'PASS authored thumb pads, circular cancel controls and decorative raycast flags',
    'PASS authored Hero range ring/fill/arrow and persistent 0.44m foot ring with facing marker',
    'PASS 18 participants, foot prefab Hero bindings and unchanged map/skill/gameplay scene blocks',
    'PASS mesh/UI shape materials, unique local refs, no duplicate material properties',
    f'PASS {preserved} existing baseline assets/sources/metadata unchanged SHA256',
    f'PASS {total} standalone checks, including 14 Vongxuyen tap/drag/reach cases',
    'NOT_RUN Unity import, touch, rendering/shader compilation, physics/NavMesh and Android',
])
(root/'Logs/TraversalQA/defense-vongxuyen-controls-static-checks.txt').write_text(report+'\n',encoding='utf-8')
print(report)
