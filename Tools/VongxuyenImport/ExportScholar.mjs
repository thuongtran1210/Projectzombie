/** Export the original Vongxuyen Scholar; source modules are read-only. */
import fs from 'node:fs/promises';
import path from 'node:path';
import { pathToFileURL } from 'node:url';
import { createHash } from 'node:crypto';

const [sourceDirectory, dependencyDirectory, outputFile] = process.argv.slice(2);
if (!outputFile) throw new Error('Usage: node ExportScholar.mjs <Vongxuyen directory> <Three r160 directory> <output.glb>');
const dependency = path.resolve(dependencyDirectory);
const sourceFiles = ['src/entities/PlayerWanderer.js', 'src/entities/HeroVisuals.js', 'src/core/MaterialManager.js'];
const hashes = {};
for (const relative of sourceFiles) {
  const original = await fs.readFile(path.join(sourceDirectory, relative), 'utf8');
  hashes[relative] = createHash('sha256').update(original).digest('hex');
  const patched = original.replaceAll("from 'three'", "from './three.mjs'").replace("from './HeroVisuals.js'", "from './HeroVisuals.mjs'");
  await fs.writeFile(path.join(dependency, path.basename(relative, '.js') + '.mjs'), patched);
}
for (const [name, replacement] of [['GLTFExporter.mjs', "from './TextureUtils.mjs'"], ['TextureUtils.mjs', null]]) {
  const file = path.join(dependency, name);
  let text = (await fs.readFile(file, 'utf8')).replaceAll("from 'three'", "from './three.mjs'");
  if (replacement) text = text.replace("from './../utils/TextureUtils.js'", replacement);
  await fs.writeFile(file, text);
}
// GLTFExporter uses the browser FileReader API for its final Blob.
globalThis.FileReader = class {
  readAsArrayBuffer(blob) { blob.arrayBuffer().then(result => { this.result = result; this.onloadend?.(); }); }
  readAsDataURL(blob) { blob.arrayBuffer().then(result => { this.result = `data:${blob.type};base64,${Buffer.from(result).toString('base64')}`; this.onloadend?.(); }); }
};
const load = name => import(pathToFileURL(path.join(dependency, name)).href);
const THREE = await load('three.mjs');
const { PlayerWanderer } = await load('PlayerWanderer.mjs');
const { MaterialManager } = await load('MaterialManager.mjs');
const { GLTFExporter } = await load('GLTFExporter.mjs');
const mm = new MaterialManager();
const hero = new PlayerWanderer(new THREE.Scene(), mm);
hero.setHeroStyle('scholar');
hero.root.position.set(0, 0, 0);
hero.root.name = 'ThuSinh_Vongxuyen';
let clips = hero.bakeAnimationClips();
hero.evaluatePose('idle', 0);
// Keep Unity's existing hit/skill indicators; don't bake translucent web VFX into the body.
hero.baguaGroup.removeFromParent();
hero.slashArc.removeFromParent();
const prune = object => {
  for (const child of [...object.children]) {
    if (!child.visible) child.removeFromParent();
    else prune(child);
  }
};
prune(hero.root);
const names = new Set();
hero.root.traverse(object => names.add(object.name));
clips = clips.map(clip => new THREE.AnimationClip(clip.name, clip.duration,
  clip.tracks.filter(track => names.has(track.name.slice(0, track.name.lastIndexOf('.'))))));
const materialMap = new Map();
let meshCount = 0;
let triangleCount = 0;
hero.root.traverse(object => {
  object.userData = {};
  if (!object.isMesh) return;
  meshCount++;
  if (object.name.endsWith('_outline')) {
    // Bake the original onBeforeCompile normal extrusion and reverse faces.
    const geometry = object.geometry.clone();
    const positions = geometry.attributes.position;
    const normals = geometry.attributes.normal;
    for (let i = 0; i < positions.count; i++)
      positions.setXYZ(i, positions.getX(i) + normals.getX(i) * .016,
        positions.getY(i) + normals.getY(i) * .016, positions.getZ(i) + normals.getZ(i) * .016);
    if (!geometry.index) throw new Error('Expected indexed source outline geometry.');
    const indices = geometry.index.array;
    for (let i = 0; i < indices.length; i += 3) [indices[i+1], indices[i+2]] = [indices[i+2], indices[i+1]];
    geometry.computeVertexNormals();
    object.geometry = geometry;
  }
  const material = object.material;
  if (!materialMap.has(material)) {
    const converted = new THREE.MeshStandardMaterial({
      color: material.color, roughness: material.roughness ?? .8,
      metalness: material.metalness ?? 0, side: THREE.FrontSide
    });
    converted.name = `Scholar_Color_${material.color.getHexString()}`;
    materialMap.set(material, converted);
  }
  object.material = materialMap.get(material);
  triangleCount += (object.geometry.index?.count ?? object.geometry.attributes.position.count) / 3;
});
hero.root.updateMatrixWorld(true);
const bounds = new THREE.Box3().setFromObject(hero.root);
const result = await new GLTFExporter().parseAsync(hero.root, {binary:true, onlyVisible:true, trs:true, animations:clips});
await fs.mkdir(path.dirname(outputFile), {recursive:true});
await fs.writeFile(outputFile, Buffer.from(result));
const report = {source:'thuongtran1210/Vongxuyen', threeVersion:THREE.REVISION, sourceHashes:hashes,
  meshes:meshCount, triangles:triangleCount, materials:materialMap.size,
  bounds:{min:bounds.min.toArray(),max:bounds.max.toArray()},
  clips:clips.map(clip=>({name:clip.name,duration:clip.duration,tracks:clip.tracks.length})),
  notes:['Original visible Scholar hierarchy and colors; no gameplay changes.',
    'Six procedural poses baked at 30 FPS by the original source baker.',
    'Web toon lighting replaced with Unity material; outline extrusion baked into geometry.',
    'Web combat VFX and hidden Hermit accessories excluded.']};
await fs.writeFile(outputFile.replace(/\.glb$/i,'.source.json'), JSON.stringify(report,null,2)+'\n');
console.log(JSON.stringify(report,null,2));
