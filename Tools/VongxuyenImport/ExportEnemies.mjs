import fs from 'node:fs/promises';
import path from 'node:path';
import {pathToFileURL} from 'node:url';
import {createHash} from 'node:crypto';
const [sourceDirectory,dependencyDirectory,outputDirectory,selection]=process.argv.slice(2);
if (!outputDirectory) throw new Error('Usage: node ExportEnemies.mjs <Vongxuyen> <Three r160 dependency directory> <output directory>');
const dependency=path.resolve(dependencyDirectory);
const sourceHashes={};
for(const relative of ['src/entities/EnemyFactory.js','src/entities/YakshaBossMesh.js','src/entities/MinionFactory.js','src/core/MaterialManager.js']) {
 const source=await fs.readFile(path.join(sourceDirectory,relative),'utf8');
 sourceHashes[relative]=createHash('sha256').update(source).digest('hex');
 await fs.writeFile(path.join(dependency,path.basename(relative,'.js')+'.mjs'),source.replaceAll("from 'three'","from './three.mjs'"));
}
globalThis.FileReader=class {readAsArrayBuffer(blob){blob.arrayBuffer().then(result=>{this.result=result;this.onloadend?.();});}};
const load=name=>import(pathToFileURL(path.join(dependency,name)).href);
const THREE=await load('three.mjs');
const {EnemyFactory}=await load('EnemyFactory.mjs');
const {buildYakshaBossMesh}=await load('YakshaBossMesh.mjs');
const {MinionFactory}=await load('MinionFactory.mjs');
const {MaterialManager}=await load('MaterialManager.mjs');
const {GLTFExporter}=await load('GLTFExporter.mjs');
await fs.mkdir(outputDirectory,{recursive:true});
const ids=selection?selection.split(','):['magiap','thuyquai','huyetdoi','quaidu','phicuong','magiap_boss'];
for(const id of ids) {
 const isMinion=['danbinh','phutapdich','thaylang'].includes(id);
 if(!isMinion&&!['magiap','thuyquai','huyetdoi','quaidu','phicuong','magiap_boss','yaksha_boss'].includes(id))throw new Error('Unknown actor: '+id);
 const mm=new MaterialManager();
 const enemy=isMinion?new MinionFactory(new THREE.Scene(),mm).spawnMinion(id,0,0):id==='yaksha_boss'?buildYakshaBossMesh(mm):new EnemyFactory(new THREE.Scene(),mm).spawnEnemy(id,0,0);
 enemy.root.name=id+'_Vongxuyen';
 enemy.bodyPivot.name=isMinion?'MinionBody':'EnemyBody';
 if(isMinion)enemy.legs?.forEach((leg,index)=>{leg.name='MinionLeg_'+index;});
 enemy.hpBar?.group.removeFromParent();
 enemy.auraRing?.removeFromParent();
 enemy.groundAura?.removeFromParent();
 enemy.bodyHalo?.removeFromParent();
 const remove=[];
 enemy.root.traverse(object=>{if(object.isLight || !object.visible || (object.isMesh && object.material.transparent))remove.push(object);});
 for(const object of remove)object.removeFromParent();
 const materials=new Map();let meshes=0,triangles=0;
 enemy.root.traverse(object=>{
  object.userData={};
  if(!object.isMesh)return;
  if(object.name.endsWith('_outline')) {
   const geometry=object.geometry.clone(),positions=geometry.attributes.position,normals=geometry.attributes.normal;
   for(let i=0;i<positions.count;i++)positions.setXYZ(i,positions.getX(i)+normals.getX(i)*.016,positions.getY(i)+normals.getY(i)*.016,positions.getZ(i)+normals.getZ(i)*.016);
   const indices=geometry.index.array;
   for(let i=0;i<indices.length;i+=3)[indices[i+1],indices[i+2]]=[indices[i+2],indices[i+1]];
   geometry.computeVertexNormals();object.geometry=geometry;
  }
  const source=object.material;
  if(!materials.has(source)) {
   const material=new THREE.MeshStandardMaterial({color:source.color,roughness:source.roughness??.8,metalness:source.metalness??0,side:source.side===THREE.DoubleSide?THREE.DoubleSide:THREE.FrontSide});
   material.name=`Enemy_Color_${source.color.getHexString()}_${material.side}`;
   materials.set(source,material);
  }
  object.material=materials.get(source);meshes++;triangles+=(object.geometry.index?.count??object.geometry.attributes.position.count)/3;
 });
 enemy.root.updateMatrixWorld(true);
 const bounds=new THREE.Box3().setFromObject(enemy.root);
 const result=await new GLTFExporter().parseAsync(enemy.root,{binary:true,onlyVisible:true,trs:true});
 await fs.writeFile(path.join(outputDirectory,id+'.glb'),Buffer.from(result));
 const manifest={source:'thuongtran1210/Vongxuyen',id,threeVersion:THREE.REVISION,sourceHashes,meshes,triangles,materials:materials.size,bounds:{min:bounds.min.toArray(),max:bounds.max.toArray()},bodyPivot:enemy.bodyPivot.position.toArray(),leftWing:enemy.leftWing?.name??null,rightWing:enemy.rightWing?.name??null,notes:['Original geometry, rigid hierarchy, colors and baked outline extrusion.','Health bars, transparent web auras and lights excluded; Unity retains gameplay ownership.','Locomotion poses are driven by the imported EnemyBody and wings in Unity, not animation clips.']};
 await fs.writeFile(path.join(outputDirectory,id+'.source.json'),JSON.stringify(manifest,null,2)+'\n');
 console.log(`${id}: ${meshes} meshes, ${triangles} triangles`);
}
