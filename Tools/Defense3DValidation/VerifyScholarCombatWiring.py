from pathlib import Path
import re,json,hashlib,yaml,sys
ROOT=Path(__file__).resolve().parents[2]
class UniqueLoader(yaml.SafeLoader): pass
def mapping(loader,node,deep=False):
 result={}
 for k,v in node.value:
  key=loader.construct_object(k,deep=deep)
  if key in result:raise AssertionError('Duplicate YAML key: '+str(key))
  result[key]=loader.construct_object(v,deep=deep)
 return result
UniqueLoader.add_constructor(yaml.resolver.BaseResolver.DEFAULT_MAPPING_TAG,mapping)
def read(p):return p.read_text(encoding='utf-8-sig')
def blocks(s):
 result={}
 for m in re.finditer(r'--- !u!(\d+) &(-?\d+)(?: stripped)?\n(.*?)(?=--- !u!|\Z)',s,re.S):
  key=int(m[2]);assert key not in result,('Duplicate fileID',key)
  result[key]=(int(m[1]),m[0],next(iter(yaml.load(m[3],Loader=UniqueLoader).values())))
 return result
def guid(p):return re.search(r'guid: (\w+)',read(Path(str(p)+'.meta')))[1]
def check(cond,name):
 assert cond,name
 print('PASS '+name)
def local_refs(bs):
 for i,(_,s,_) in bs.items():
  for ref in re.findall(r'\{fileID: (-?\d+)\}',s):assert int(ref)==0 or int(ref) in bs,('Dangling local reference',i,ref)
R=ROOT/'Assets/Resources/Defense3D';F=ROOT/'Assets/Features/Defense3D';scene=ROOT/'Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity'
base=Path(sys.argv[1]) if len(sys.argv)>1 else Path(read(ROOT/'Temp/defense-scholar-baseline-path.txt').strip())
manifest=json.loads(read(base/'manifest.json'))
protected=[p for p in manifest if p.startswith('Assets/Resources/Defense3D/') and p!='Assets/Resources/Defense3D/DefenseSkillControlsCanvas.prefab']
check(all(hashlib.sha256((ROOT/p).read_bytes()).hexdigest()==manifest[p] for p in protected),str(len(protected))+' original resource files unchanged')
b=blocks(read(scene));old=blocks(read(base/'Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity'))
check(set(old).issubset(b),'All original scene IDs retained')
changed={i for i in old if old[i][1]!=b[i][1]};check(changed=={256135789,256135796,8000400002,8000600000,9223372036854775807},'Scene changes limited to Hero components, manual mode, composition, UI bindings and roots')
check(len(set(b)-set(old))==16,'Exactly sixteen new scene blocks')
local_refs(b);check(True,'Scene local references resolve')
kit=blocks(read(R/'HeroKit_ThuSinh.asset'))[11400000][2]
for name,kind,ran,rad,cd in [('Skill_BasicAttack',2,2.05,2.05,.65),('Skill_ScholarSignature',3,6,6,30)]:
 d=blocks(read(R/(name+'.asset')))[11400000][2]
 check((d['_kind'],d['_range'],d['_radius'],d['_cooldown'])==(kind,ran,rad,cd),name+' prototype geometry and cooldown')
check(all(kit[k]['guid']==guid(R/(n+'.asset')) for k,n in [('_attack','Skill_BasicAttack'),('_dash','Skill_Dash'),('_signature','Skill_ScholarSignature'),('_relic','Skill_DepToOng')]),'Hero kit maps all four authored definitions')
c=blocks(read(R/'DefenseSkillControlsCanvas.prefab'));local_refs(c)
check(True,'Four-button prefab local references resolve')
for baseid,skill in [(2000,'Skill_Dash'),(3000,'Skill_DepToOng'),(12000,'Skill_BasicAttack'),(23000,'Skill_ScholarSignature')]:
 p=c[baseid+6][2];a=c[baseid+4][2];stick=c[baseid+7][2]
 check(p['_definition']['guid']==guid(R/(skill+'.asset')) and p['_button']['fileID']==baseid+4 and a['_stick']['fileID']==baseid+7,skill+' owns its control and definition')
 cancel=a['_cancelZone']['fileID'];pad=stick['_pad']['fileID']
 check(cancel in c and pad in c and c[c[cancel][2]['m_GameObject']['fileID']][2]['m_IsActive']==0,skill+' owns authored inactive cancel/pad')
 # Only the actual button accepts pointer raycasts.
 for cid,(_,_,d) in c.items():
  if d.get('m_GameObject',{}).get('fileID')==baseid and 'm_RaycastTarget' in d:check(d['m_RaycastTarget']==1,skill+' button receives raycasts')
root=c[1001][2]
check(len(root['m_Children'])==12,'Canvas has four authored buttons, four cancel zones and four thumb pads')
centers=[]
for t in [2001,3001,12001,23001]:
 d=c[t][2];centers.append((d['m_AnchoredPosition']['x'],d['m_AnchoredPosition']['y'],d['m_SizeDelta']['x']/2))
check(all((x-u)**2+(y-v)**2>(r+s)**2 for n,(x,y,r) in enumerate(centers) for u,v,s in centers[n+1:]),'Four action buttons do not overlap at reference resolution')
check(c[1005][2]['_attack']['fileID']==12006 and c[1005][2]['_signature']['fileID']==23006,'Authored keyboard adapter targets same presenters')
comp=b[8000400002][2]
check(len(comp['_sceneParticipants'])==20 and len(comp['_skillPresenters'])==4 and comp['_heroKit']['guid']==guid(R/'HeroKit_ThuSinh.asset'),'Composition binds twenty participants and four kit presenters')
check(b[256135796][2]['_autoAttackEnabled']==0,'Production scene uses manual attack')
mods=b[8000600000][2]['m_Modification']['m_Modifications'];lookup={(x['target']['fileID'],x['propertyPath']):x['objectReference']['fileID'] for x in mods}
for source,view,effect in [(12006,8001800102,8001800001),(23006,8001800112,8001800002)]:
 check(lookup[(source,'_indicator')]==view and lookup[(source,'_effect')]==effect and lookup[(source,'_health')]==256135798 and lookup[(source,'_origin')]==256135790,'New presenter '+str(source)+' has complete Hero/effect/indicator overrides')
check(b[8001800001][2]['_attack']['fileID']==256135796 and b[8001800002][2]['_relic']['fileID']==8000600003,'Effects reference sole attack owner and existing relic presenter')
roots={x['fileID'] for x in b[9223372036854775807][2]['m_Roots']}
check({8001800100,8001800110,8001800120,8001800130,8000900000}.issubset(roots),'New views and original Hero foot indicator present in scene roots')
a=blocks(read(R/'DefenseHeroAttackIndicator3D.prefab'));local_refs(a)
check(a[1002][2]['_sector']['fileID']==7003 and a[7003][2]['m_Materials'][0]['guid']==guid(R/'HeroAttack_Sector.mat'),'Attack indicator uses authored sector renderer/material')
check(a[7002][2]['m_Mesh']['fileID']==10210,'Sector uses existing native quad mesh')
mat=blocks(read(R/'HeroAttack_Sector.mat'))[2100000][2];floats={k:v for x in mat['m_SavedProperties']['m_Floats'] for k,v in x.items()}
check(floats['_Shape']==5 and abs(floats['_SectorCos']+.258819)<.000001,'Sector material matches 210-degree attack arc')
for p in [F/'DefenseHeroKitSO.cs',F/'DefenseHeroAttackEffect3D.cs',F/'DefenseScholarSignatureEffect3D.cs',F/'UI/DefenseHeroCombatHotkeys3D.cs']:
 s=read(p);check(not re.search(r'new GameObject|AddComponent|FindObject|Physics2D|Elemental|WuXing|NguHanh',s),p.name+' has no runtime object creation, global combat lookup or elemental/2D coupling')
newassets=[R/x for x in ['Skill_BasicAttack.asset','Skill_ScholarSignature.asset','HeroKit_ThuSinh.asset','HeroAttack_Sector.mat','DefenseHeroAttackIndicator3D.prefab']]
check(all(Path(str(p)+'.meta').exists() for p in newassets),'New authored assets retain explicit GUIDs')
print('PASS OFFLINE AUTHORING ONLY; Unity rendering, physics and input acceptance NOT_RUN')
