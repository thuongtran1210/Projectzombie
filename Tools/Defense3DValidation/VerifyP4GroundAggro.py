"""Static checks for the Ground/aggro slice of RF3D-021, without Unity."""
import hashlib,json,re,sys
from pathlib import Path
import yaml

root=Path(__file__).resolve().parents[2]
baseline=Path(sys.argv[1]) if len(sys.argv)>1 else Path((root/'Temp/defense-p4-ground-baseline-path.txt').read_text().strip())
def parse(path):
    result={}
    for m in re.finditer(r'--- !u!(\d+) &(-?\d+)(?: stripped)?\n(.*?)(?=--- !u!|\Z)',path.read_text(encoding='utf-8-sig'),re.S):
        i=int(m[2]);assert i not in result,f'Duplicate ID {i}'
        result[i]=yaml.safe_load(m[3])
    return result
def val(d,i):return next(iter(d[i].values()))
def refs(v):
    if isinstance(v,dict):
        if 'fileID' in v:yield v
        else:
            for a in v.values():yield from refs(a)
    elif isinstance(v,list):
        for a in v:yield from refs(a)
def guid(path):
    p = Path(path)
    if not p.exists():
        matches = list((ROOT / 'Assets/Features/Defense3D').rglob(p.name))
        if matches: p = matches[0]
    meta = p.parent / (p.name + '.meta')
    for line in meta.read_text().splitlines():
        if line.startswith('guid: '): return line.split()[1]
    raise AssertionError('No guid in ' + str(meta))

folder=Path('Assets/Features/Defense3D')
aggro_guid=guid(folder/'DefenseGroundAggro3D.cs')
attack_guid=guid(folder/'DefenseEnemyAttack3D.cs')
profile_path=Path('Assets/Resources/Defense3D/GroundNavigation_SongQue.asset')
profile_guid=guid(profile_path)
for name in ['DefenseEnemy3D','DefenseEnemy_Magiap']:
    p=Path('Assets/Resources/Defense3D')/(name+'.prefab');old=parse(baseline/p);new=parse(root/p)
    assert set(new)-set(old)=={9001100001}
    aggro=val(new,9001100001)
    assert aggro['m_Script']['guid']==aggro_guid and aggro['_profile']['guid']==profile_guid
    actor=aggro['_enemy']['fileID'];movement=aggro['_movement']['fileID'];attack=aggro['_attack']['fileID'];gid=aggro['m_GameObject']['fileID']
    assert val(new,actor)['m_GameObject']['fileID']==gid and val(new,movement)['m_GameObject']['fileID']==gid
    assert val(new,attack)['m_Script']['guid']==attack_guid and val(new,attack)['_lineOfSightMask']['m_Bits']==25
    assert val(new,gid)['m_Component']==val(old,gid)['m_Component']+[{'component':{'fileID':9001100001}}]
    old_attack=val(old,attack).copy();new_attack=val(new,attack).copy();new_attack.pop('_lineOfSightMask');assert new_attack==old_attack
    for i in old:
        if i not in [gid,attack]:assert old[i]==new[i],f'{name}: unrelated prefab change {i}'
    for i,v in new.items():
        for ref in refs(v):assert not ref['fileID'] or 'guid' in ref or ref['fileID'] in new,f'{name}: bad ref {i}'

profile=val(parse(root/profile_path),11400000)
assert (profile['_acquireRange'],profile['_retainRange'],profile['_targetLockSeconds'],profile['_repathSeconds'])==(4.2,7,0.75,0.25)
assert (profile['_westEdge'],profile['_eastEdge'],profile['_bridgeHalfWidth'])==(4.45,8.05,1.4)
allowed={
    'Assets/Features/Defense3D/'+n+'.cs' for n in ['DefenseGroundMovement3D','DefenseEnemyAttack3D','DefenseEnemyBridgeWalker3D',
        'DefenseBridgeRoute3D','DefenseSpawnPlacement3D','DefenseRun6003D']
}
allowed|={'Assets/Resources/Defense3D/DefenseEnemy3D.prefab','Assets/Resources/Defense3D/DefenseEnemy_Magiap.prefab',
          'Tools/Defense3DValidation/RunCoreChecks.ps1','Tools/Defense3DValidation/README.md'}
preserved=0
for item in json.loads((baseline/'manifest.json').read_text()):
    if item['path'] in allowed:continue
    p=root/item['path'];assert p.is_file() and hashlib.sha256(p.read_bytes()).hexdigest()==item['sha256'],item['path']
    preserved+=1
movement=(root/folder/'DefenseGroundMovement3D.cs').read_text()
assert 'agent.SetPath(_path)' in movement and 'CanReach(Vector3 target)' in movement
assert 'index==_routeCount-1' in movement and 'if (agent==null) agent = GetComponent' in movement
attack=(root/folder/'DefenseEnemyAttack3D.cs').read_text()
assert 'ClosestPoint' in attack and 'Physics.Linecast' in attack and '_groundRules.SameBank' in attack
aggro=(root/folder/'DefenseGroundAggro3D.cs').read_text()
assert 'DefenseEnemyLocomotionType.GroundNavMesh' in aggro and '_enemy.Stats.IsBoss' in aggro
assert '_movement.FollowBuffer' in aggro and '_movement.Pursue' in aggro
assert 'Physics2D' not in aggro and 'FindObjectOfType' not in aggro
route=(root/folder/'DefenseBridgeRoute3D.cs').read_text()
assert 'route[route.Length-1]=destination' in route and 'origin.x<=4.45f' in route
assert 'UnityEngine' not in (root/folder/'DefenseGroundNavigationRules.cs').read_text()
assert '"LightMode" = "SRPDefaultUnlit"' in (root/'Assets/Shaders/VFX/DefenseAimOverlay.shader').read_text()
counts=[('DefenseSessionChecks',23),('DefenseCombatChecks',35),('DefenseAbilityInputChecks',27),('DefenseGestureAimChecks',39),
        ('DefenseSkillCastChecks',22),('DefenseVongxuyenControlChecks',14),('DefenseActorLifetimeChecks',21),('DefenseGroundAggroChecks',21)]
for name,count in counts:
    assert f'TOTAL {count} PASS' in (root/f'Logs/TraversalQA/defense-p4-ground-{name}.txt').read_text(encoding='utf-8-sig')
for suffix in ['runtime-compile','source-compatibility-compile']:
    assert 'PASS ' in (root/f'Logs/TraversalQA/defense-p4-ground-{suffix}.txt').read_text(encoding='utf-8-sig')
report='\n'.join([
    'PASS Ground profile and two authored Ground prefab compositions have valid references',
    'PASS actual Base endpoint, West direct route, committed bridge resume and allocation-free route buffers',
    'PASS aggro hysteresis/provocation policy; Ground-only/non-Boss binding and Hero reachability guards',
    'PASS collider-surface melee range, per-hit same-bank guard and configured LOS mask',
    f'PASS {preserved} protected files unchanged SHA256, including scene/NavMesh/other enemy prefabs/skill indicators',
    f'PASS {sum(c for _,c in counts)} standalone core checks and runtime/editor/fixture source compilation',
    'NOT_MIGRATED Water/Air/Landing/Support/GhostCannon/Boss policies; legacy navigation remains for those actors',
    'NOT_RUN Unity/Awake/NavMesh/LOS/physics/aggro gameplay/rendering/Android',
])
(root/'Logs/TraversalQA/defense-p4-ground-static-checks.txt').write_text(report+'\n',encoding='utf-8');print(report)
