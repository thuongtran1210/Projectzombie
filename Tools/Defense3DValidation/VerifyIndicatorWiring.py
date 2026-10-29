from pathlib import Path
import re,json,hashlib,yaml,sys
ROOT=Path(__file__).resolve().parents[2]
class UniqueLoader(yaml.SafeLoader):pass
def mapping(loader,node,deep=False):
 result={}
 for k,v in node.value:
  key=loader.construct_object(k,deep=deep)
  assert key not in result,('Duplicate YAML key',key)
  result[key]=loader.construct_object(v,deep=deep)
 return result
UniqueLoader.add_constructor(yaml.resolver.BaseResolver.DEFAULT_MAPPING_TAG,mapping)
def read(p):return p.read_text(encoding='utf-8-sig')
def blocks(s):
 result={}
 for m in re.finditer(r'--- !u!(\d+) &(-?\d+)(?: stripped)?\n(.*?)(?=--- !u!|\Z)',s,re.S):
  key=int(m[2]);assert key not in result
  result[key]=(int(m[1]),m[0],next(iter(yaml.load(m[3],Loader=UniqueLoader).values())))
 return result
def guid(p):return re.search(r'guid: (\w+)',read(Path(str(p)+'.meta')))[1]
def check(cond,name):assert cond,name;print('PASS '+name)
def refs(b):
 for i,(_,s,d) in b.items():
  for r in re.findall(r'\{fileID: (-?\d+)\}',s):assert int(r)==0 or int(r) in b,('Dangling reference',i,r)
base=Path(sys.argv[1]) if len(sys.argv)>1 else Path(read(ROOT/'Temp/defense-indicator-baseline-path.txt').strip())
manifest=json.loads(read(base/'manifest.json'));R=ROOT/'Assets/Resources/Defense3D';F=ROOT/'Assets/Features/Defense3D'
allowed={'Assets/Resources/Defense3D/DefenseHeroAttackIndicator3D.prefab'}
protected=[p for p in manifest if (p.startswith('Assets/Resources/Defense3D/') or p.startswith('Assets/Shaders/')) and p not in allowed]
check(all(hashlib.sha256((ROOT/p).read_bytes()).hexdigest()==manifest[p] for p in protected),str(len(protected))+' protected resources/shader unchanged, including Stage, Controls, HeroFoot and generic Aim')
scene='Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity';b=blocks(read(ROOT/scene));old=blocks(read(base/scene))
check(set(old).issubset(b) and len(b)-len(old)==3,'All old scene IDs retained; only three warning instance blocks added')
check({i for i in old if old[i][1]!=b[i][1]}=={290506675,8000400002,9223372036854775807},'Scene changes limited to Run warning binding, composition and roots')
refs(b);check(True,'Scene local references resolve')
check(b[290506675][2]['_spawnWarnings']['fileID']==8001900002 and b[8001900002][2]['m_CorrespondingSourceObject']['fileID']==1004,'Run supplies the same compiled stage to authored warning presenter')
comp=b[8000400002][2];check(len(comp['_sceneParticipants'])==21 and len(comp['_skillPresenters'])==4,'Composition has twenty-one participants and four Hero presenters')
check(any(x['fileID']==8001900000 for x in b[9223372036854775807][2]['m_Roots']),'Warnings are an authored scene root')
mods=b[8001900000][2]['m_Modification']['m_Modifications'];check(any(x['propertyPath']=='_camera' and x['objectReference']['fileID']==1990408784 for x in mods),'Warnings use actual main camera binding')
c=blocks(read(R/'DefenseSpawnWarningsCanvas.prefab'));refs(c)
check(len(c)==133 and len(c[1001][2]['m_Children'])==32,'Thirty-two fully authored vector badge slots; no runtime object pool creation')
check(c[1002][2]['m_RenderMode']==0 and c[1002][2]['m_SortingOrder']==85,'Overlay warning layer draws below action Controls')
check(c[1004][2]['_leadSeconds']==5 and c[1004][2]['_screenMargin']>=32 and c[1004][2]['_spacing']>=44,'Five-second lead and safe badge/arrow margins authored')
for view in c[1004][2]['_views']:
 local=view['fileID'];d=c[local][2];go=c[d['m_GameObject']['fileID']][2]
 assert d['m_Script']['guid']==guid(F/'UI/DefenseSpawnWarningGraphic.cs') and d['m_RaycastTarget']==0 and go['m_IsActive']==0
check(True,'Every badge starts inactive and cannot block joystick/skill pointer input')
scriptguids={x[2].get('m_Script',{}).get('guid') for x in c.values()}
check('dc42784cf147c0c48a680349fa168899' not in scriptguids,'Warning canvas has no GraphicRaycaster')
a=blocks(read(R/'DefenseHeroAttackIndicator3D.prefab'));refs(a)
check(a[1002][2]['_sectorBorder']['fileID']==8002 and a[8002][2]['m_UseWorldSpace']==0 and len(a[8002][2]['m_Positions'])==34,'Authored player sector outline has center and 33 arc points')
check(a[8002][2]['m_Enabled']==0,'Sector outline remains hidden before presentation')
# Validate production schedule fits the fixed badge count, including simultaneous boss warnings.
stage=blocks(read(R/'Stages/Stage01_Production.asset'))[11400000][2]
events=[(x['TriggerTime'],x['Count']) for x in stage['events']]+[(stage['bossSpawnTime'],1)]
max_count=max(sum(n for t,n in events if 0<t-now<=5) for now,_ in events for now in [max(0,now-5),max(0,now-.001)])
check(max_count<=32,'Production warning peak '+str(max_count)+' fits 32 authored slots')
run=read(F/'DefenseRun6003D.cs');pres=read(F/'UI/DefenseSpawnWarningPresenter3D.cs');graphic=read(F/'UI/DefenseSpawnWarningGraphic.cs');foot=read(F/'DefenseHeroFootIndicator3D.cs');aim=read(F/'UI/DefenseAimIndicator3D.cs')
check(run.count('stageSpawnConfig.Compile()')==1 and '_spawnWarnings?.Configure(plan, sceneInfo.EastEnemySpawn)' in run,'Warning view does not compile a second stage schedule')
check(not re.search(r'Enqueue|ProcessSpawns|TrySpawn|NavMesh|Instantiate\(|new GameObject|new Material|FindObject',pres),'Warning presenter observes only; cannot consume/change spawn jobs')
check('Screen.safeArea' in pres and '_camera.pixelRect' in pres and 'WorldToViewportPoint' in pres,'Projection respects camera rectangle and device safe area')
check('GameplayTime' in pres and 'Time.deltaTime' not in pres and 'Time.time' not in pres,'Countdown uses session gameplay clock')
check('OnGameplayGateChanged' in pres and 'OnGameplayCleanup' in pres and 'OnGameplayReset' in pres and 'OnDisable' in pres,'Warnings hide on gate, cleanup, reset and disable')
check('OnPopulateMesh' in graphic and 'new Mesh' not in graphic and 'new Material' not in graphic and 'Texture2D' not in graphic,'Existing UGUI Graphic renders vector badges without emoji fonts or explicit mesh/material creation')
check('_heroCollider.bounds.min.y' in foot and 'else SetVisible(false)' in foot,'HeroFoot uses collider-foot fallback when surface probe misses')
check('_sectorBorder' in aim and 'ground.point.y' in aim and 'showTarget' in aim,'Aim has sector outline, ground-at-origin range ring and optional duplicate-target suppression')
for p in [F/'DefenseIndicatorProjection.cs',F/'UI/DefenseSpawnWarningGraphic.cs',F/'UI/DefenseSpawnWarningPresenter3D.cs',R/'DefenseSpawnWarningsCanvas.prefab']:assert Path(str(p)+'.meta').exists()
check(True,'All new scripts and prefab have explicit GUID metadata')
print('PASS OFFLINE ONLY: Unity import/render/input/device acceptance NOT_RUN; warnings predict authored schedule, not delayed queue placement')
