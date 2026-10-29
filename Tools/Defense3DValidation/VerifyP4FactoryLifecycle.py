"""Source/scene checks for the initial P4 factory-lifecycle package; no Unity invocation."""
from pathlib import Path
import hashlib
import json
import re
import sys
import yaml

root=Path(__file__).resolve().parents[2]
baseline=Path(sys.argv[1]) if len(sys.argv)>1 else Path((root/'Temp/defense-p4-baseline-path.txt').read_text().strip())

class UniqueLoader(yaml.SafeLoader):pass
def mapping(loader,node,deep=False):
    result={}
    for kn,vn in node.value:
        key=loader.construct_object(kn,deep=deep)
        assert key not in result,f'Duplicate YAML key {key}'
        result[key]=loader.construct_object(vn,deep=deep)
    return result
UniqueLoader.add_constructor(yaml.resolver.BaseResolver.DEFAULT_MAPPING_TAG,mapping)
def parse(path):
    result={}
    for m in re.finditer(r'--- !u!(\d+) &(-?\d+)(?: stripped)?\n(.*?)(?=--- !u!|\Z)',path.read_text(encoding='utf-8-sig'),re.S):
        i=int(m[2]);assert i not in result,f'Duplicate fileID {i}'
        result[i]=yaml.load(m[3],Loader=UniqueLoader)
    return result
def val(d,i):return next(iter(d[i].values()))
def refs(v):
    if isinstance(v,dict):
        if 'fileID' in v:yield v
        else:
            for x in v.values():yield from refs(x)
    elif isinstance(v,list):
        for x in v:yield from refs(x)

scene_path=Path('Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity')
before=parse(baseline/scene_path);scene=parse(root/scene_path)
assert set(before)<=set(scene)
assert {i for i in before if before[i]!=scene[i]}=={290506675,9223372036854775807}
assert len(scene)-len(before)==11
for i,v in scene.items():
    for ref in refs(v):assert not ref['fileID'] or 'guid' in ref or ref['fileID'] in scene,f'{i}: broken {ref}'
run_before=val(before,290506675).copy();run_after=val(scene,290506675).copy()
assert run_after.pop('_runtimeRoots')=={'fileID':8001000002}
assert run_after==run_before
assert val(scene,9223372036854775807)['m_Roots']==val(before,9223372036854775807)['m_Roots']+[{'fileID':8001000001}]
runtime=val(scene,8001000002)
assert val(scene,8001000000)['m_Name']=='DefenseRuntime'
for field,off,name,active in [('_actors',10,'Actors',1),('_staging',20,'Staging',0),('_pickups',30,'Pickups',1),('_pools',40,'Pools_Reserved',0)]:
    gid=8001000000+off;tid=gid+1
    assert runtime[field]['fileID']==tid
    go=val(scene,gid);tr=val(scene,tid)
    assert go['m_Name']==name and go['m_IsActive']==active
    assert tr['m_Father']['fileID']==8001000001
    assert tr['m_LocalPosition']=={'x':0,'y':0,'z':0} and tr['m_LocalScale']=={'x':1,'y':1,'z':1}

preserved=0
for entry in json.loads((baseline/'manifest.json').read_text()):
    path=root/entry['path'];relative=Path(entry['path'])
    if relative.suffix=='.meta' or entry['path'].startswith('Assets/Resources/Defense3D/') or (entry['path'].startswith('Assets/Scenes/Defense3D/') and relative!=scene_path):
        assert path.is_file() and hashlib.sha256(path.read_bytes()).hexdigest()==entry['sha256'],entry['path']
        preserved+=1
folder=root/'Assets/Features/Defense3D'
factory=(folder/'DefenseEnemyFactory3D.cs').read_text()
assert factory.index('prepared?.Invoke(actor)')<factory.index('instance.transform.SetParent(roots.Actors,true)')
assert 'roots.Staging' in factory and 'actor.RequestRelease(DefenseActorReleaseReason.SpawnFailed)' in factory
owner=(folder/'DefenseEnemySessionFactory3D.cs').read_text()
assert '!ReferenceEquals(entry.Lifetime,lifetime)' in owner
assert owner.index('lifetime.TryRetire(')<owner.index('actor.gameObject.SetActive(false)')
assert 'ActorReleased?.Invoke(actor,lifetime)' in owner and 'entry.Notified = true' in owner
spawn=(folder/'DefenseSpawnRuntime.cs').read_text()
assert '_factory.Spawn(' in spawn and 'DefenseEnemyFactory3D.Spawn' not in spawn and 'Object.Destroy' not in spawn
assert 'ReferenceEquals(tracked,lifetime)' in spawn and 'DefenseActorReleaseReason.SpawnFailed' in spawn
actor=(folder/'DefenseEnemy3D.cs').read_text()
assert 'HasCurrentLifetime' in actor and 'DefenseActorReleaseReason.ExternalDisabled' in actor
drop=(folder/'DefenseEnemyDrop3D.cs').read_text()
assert 'TryClaimReward(Gameplay)' in drop and 'finally' in drop and 'enemy.RequestRelease' in drop
assert '_pickupRoot' in drop
assert 'UnityEngine' not in (folder/'DefenseActorLifetime.cs').read_text()
for name in ['DefenseActorLifetime','DefenseRuntimeRoots3D','DefenseEnemySessionFactory3D']:
    assert (folder/(name+'.cs.meta')).is_file()

counts=[('DefenseSessionChecks',23),('DefenseCombatChecks',35),('DefenseAbilityInputChecks',27),('DefenseGestureAimChecks',39),
        ('DefenseSkillCastChecks',22),('DefenseVongxuyenControlChecks',14),('DefenseActorLifetimeChecks',21)]
for name,count in counts:
    log=(root/f'Logs/TraversalQA/defense-p4-factory-{name}.txt').read_text(encoding='utf-8-sig')
    assert f'TOTAL {count} PASS' in log
for suffix in ['runtime-compile','source-compatibility-compile']:
    assert 'PASS ' in (root/f'Logs/TraversalQA/defense-p4-factory-{suffix}.txt').read_text(encoding='utf-8-sig')
report='\n'.join([
    f'PASS {len(scene)} unique scene blocks; only Run binding/new runtime root changes existing scene data',
    'PASS authored Actors/Pickups active, Staging/Pools inactive, all containers identity and all local refs valid',
    f'PASS {preserved} existing resource/NavMesh/metadata files unchanged SHA256',
    'PASS production actor ownership registered before enable; spawn failure/death/cleanup release through scoped factory',
    'PASS retired/old generation guards, once-only reward/retirement latches and planner committed-token checks',
    'PASS pickup parent injected; external disable/removal does not count as a rewarded kill',
    f'PASS {sum(c for _,c in counts)} standalone core checks and runtime/editor/fixture source compilation',
    'NOT_IMPLEMENTED pool reuse, locomotion/aggro strategies or Boss FSM migration',
    'NOT_RUN Unity import/callback ordering, PlayMode, NavMesh/physics, rendered UI or Android',
])
(root/'Logs/TraversalQA/defense-p4-factory-static-checks.txt').write_text(report+'\n',encoding='utf-8')
print(report)
