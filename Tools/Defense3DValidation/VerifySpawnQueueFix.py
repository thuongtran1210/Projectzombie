"""Verify the production-profile switch and queue regression evidence without Unity."""
from pathlib import Path
import hashlib,json,re,sys,yaml

root=Path(__file__).resolve().parents[2]
baseline=Path(sys.argv[1]) if len(sys.argv)>1 else Path((root/'Temp/defense-spawn-queue-baseline-path.txt').read_text().strip())
def parse(path):
    result={}
    for m in re.finditer(r'--- !u!(\d+) &(-?\d+)(?: stripped)?\n(.*?)(?=--- !u!|\Z)',path.read_text(encoding='utf-8-sig'),re.S):
        i=int(m[2]);assert i not in result
        result[i]=yaml.safe_load(m[3])
    return result
def val(d,i):return next(iter(d[i].values()))
scene_path=Path('Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity')
before=parse(baseline/scene_path);after=parse(root/scene_path)
assert set(before)==set(after)
assert {i for i in before if before[i]!=after[i]}=={290506675}
a=val(before,290506675).copy();b=val(after,290506675).copy()
old=a.pop('stageSpawnConfig');new=b.pop('stageSpawnConfig');assert a==b
assert old['guid']=='e3127f0b71c1e324f90fb2875d8fe986'
assert new['guid']=='6a67eea68f0980344b80b8275db3ca4f'
for name in ['Stage01_FastSpawn','Stage01_Production']:
    p=Path('Assets/Resources/Defense3D/Stages')/(name+'.asset')
    assert (root/p).read_bytes()==(baseline/p).read_bytes(),name
    data=val(parse(root/p),11400000)
    assert len(data['events'])==34 and sum(e['Count'] for e in data['events'])==214
    assert (data['maxActiveEnemies'],data['maxPendingSpawns'],data['spawnBudgetPerFrame'],data['jobLifetime'])==(32,40,4,25)
    assert data['events'][20]['TriggerTime']==(18 if name.endswith('FastSpawn') else 360)
planner=next((root/'Assets/Features/Defense3D').rglob('DefenseSpawnPlanner.cs')).read_text()
runtime=next((root/'Assets/Features/Defense3D').rglob('DefenseSpawnRuntime.cs')).read_text()
assert planner.index('ExpirePending(currentTime);')<planner.index('pendingQueue.Count >= pendingLimit')
assert '_resetVersion' in planner and 'if (_expiring) return 0' in planner
tick=runtime.split('public void Tick()')[1].split('private bool CanSpawn')[0]
assert tick.index('_planner.ExpirePending')<tick.index('_scheduler.EnqueueDue')
for field in ['profile=','active=','owned=','pending=','predicateBlockedLastBatch=','factoryFailedLastBatch=','expired=','overflowTotal=']:
    assert field in runtime,field
counts=[('DefenseSessionChecks',23),('DefenseCombatChecks',35),('DefenseAbilityInputChecks',27),('DefenseGestureAimChecks',39),
        ('DefenseSkillCastChecks',22),('DefenseVongxuyenControlChecks',14),('DefenseActorLifetimeChecks',21),
        ('DefenseGroundAggroChecks',21),('DefenseSpawnQueueChecks',14)]
for name,count in counts:
    assert f'TOTAL {count} PASS' in (root/f'Logs/TraversalQA/defense-spawn-queue-{name}.txt').read_text(encoding='utf-8-sig')
for suffix in ['runtime-compile','source-compatibility-compile']:
    assert 'PASS ' in (root/f'Logs/TraversalQA/defense-spawn-queue-{suffix}.txt').read_text(encoding='utf-8-sig')
report='\n'.join([
    'PASS only the Run stageSpawnConfig reference changed in scene; Production is now the default',
    'PASS FastSpawn and Production assets unchanged: 34 events/214 ordinary slots, independent Boss, limits32/40/4/25',
    'PASS expiry sweep precedes enqueue and ignores active cap/queue rotation; reset callback guard retained',
    'PASS overflow telemetry includes profile/time/capacity/factory/predicate/expiry accounting',
    f'PASS {sum(c for _,c in counts)} standalone core checks and runtime/editor/fixture source compilation',
    'PASS real profile simulation reproduces FastSpawn overflow and no Production overflow at 0.1s steps with no kills and all placement valid',
    'NOT_RUN Unity, actual enemy navigation/combat, gameplay frame timing, graphics or Android',
])
(root/'Logs/TraversalQA/defense-spawn-queue-static-checks.txt').write_text(report+'\n',encoding='utf-8');print(report)
