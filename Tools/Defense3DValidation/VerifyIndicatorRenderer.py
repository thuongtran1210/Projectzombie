"""Regress pass compatibility with the project's configured URP 2D renderer, using its installed source."""
from pathlib import Path
import hashlib
import json
import re
import sys
import yaml

root=Path(__file__).resolve().parents[2]
snapshot=Path(sys.argv[1]) if len(sys.argv)>1 else Path(
    (root/'Temp/defense-indicator-renderer-baseline-path.txt').read_text().strip())

def load(path):
    text=path.read_text(encoding='utf-8-sig')
    text=re.sub(r'^%.*\n','',text,flags=re.M)
    text=re.sub(r'--- !u!\d+ &-?\d+(?: stripped)?','---',text)
    return list(yaml.safe_load_all(text))

graphics=load(root/'ProjectSettings/GraphicsSettings.asset')[0]['GraphicsSettings']
pipeline=load(root/'Assets/Settings/URP_2D_Asset.asset')[0]['MonoBehaviour']
pipeline_guid=re.search(r'^guid: (\w+)',(root/'Assets/Settings/URP_2D_Asset.asset.meta').read_text(),re.M)[1]
assert graphics['m_CustomRenderPipeline']['guid']==pipeline_guid
renderer_guid=re.search(r'^guid: (\w+)',(root/'Assets/Settings/URP_2D_Renderer.asset.meta').read_text(),re.M)[1]
assert pipeline['m_RendererDataList'][pipeline['m_DefaultRendererIndex']]['guid']==renderer_guid
scene=load(root/'Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity')
cameras=[d['MonoBehaviour'] for d in scene if d and 'MonoBehaviour' in d and 'm_RendererIndex' in d['MonoBehaviour']]
assert len(cameras)==1 and cameras[0]['m_RendererIndex']==-1

package=root/'Library/PackageCache/com.unity.render-pipelines.universal@14.0.12'
renderer_source=(package/'Runtime/2D/Passes/Render2DLightingPass.cs').read_text()
accepted=set(re.findall(r'ShaderTagId\("([^"]+)"\)',renderer_source))
assert 'SRPDefaultUnlit' in accepted and 'UniversalForward' not in accepted
shader=(root/'Assets/Shaders/VFX/DefenseAimOverlay.shader').read_text()
passes=re.findall(r'"LightMode"\s*=\s*"([^"]+)"',shader)
assert passes and all(p in accepted for p in passes),f'Invisible shader passes: {passes}; accepted: {accepted}'
before=(snapshot/'DefenseAimOverlay.shader').read_text(encoding='utf-8-sig')
old_passes=re.findall(r'"LightMode"\s*=\s*"([^"]+)"',before)
assert old_passes and not any(p in accepted for p in old_passes),'Baseline must reproduce the rejected shader pass.'
assert 'ZTest Always' in shader and 'ZWrite Off' in shader and '_Shape' in shader
shader_guid=re.search(r'^guid: (\w+)',(root/'Assets/Shaders/VFX/DefenseAimOverlay.shader.meta').read_text(),re.M)[1]
materials=list((root/'Assets/Resources/Defense3D').glob('DefenseAim*.mat'))
for path in materials:
    material=load(path)[0]['Material']
    assert material['m_Shader']['guid']==shader_guid,path

for item in json.loads((snapshot/'manifest.json').read_text()):
    assert hashlib.sha256((root/item['path']).read_bytes()).hexdigest()==item['sha256'],item['path']

report='\n'.join([
    'PASS default camera uses configured URP 2D renderer',
    'PASS installed URP 14.0.12 2D renderer accepts SRPDefaultUnlit and excludes UniversalForward',
    'PASS pre-fix shader fails the same renderer-pass compatibility check',
    'PASS indicator shader now uses an accepted unlit pass for world meshes/LineRenderers',
    f'PASS {len(materials)} indicator/foot/UI materials resolve the corrected shader GUID',
    'PASS scene, indicator/foot prefabs, shader GUID and global rendering settings unchanged SHA256',
    'NOT_RUN Unity, shader compilation, rendered pixels or input; this verifies source-level pass compatibility',
])
(root/'Logs/TraversalQA/defense-indicator-renderer-fix-checks.txt').write_text(report+'\n',encoding='utf-8')
print(report)
