import os, glob, uuid
from PIL import Image
import numpy as np

def clean_alpha(img_rgba, threshold=22):
    data = np.array(img_rgba)
    r, g, b = data[:, :, 0], data[:, :, 1], data[:, :, 2]
    max_rgb = np.maximum(np.maximum(r, g), b)
    alpha = np.clip((max_rgb.astype(float) - threshold) / (threshold * 1.5), 0.0, 1.0) * 255.0
    data[:, :, 3] = alpha.astype(np.uint8)
    return Image.fromarray(data, mode="RGBA")

def make_meta(png_path):
    meta_path = png_path + ".meta"
    guid = uuid.uuid4().hex
    content = f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  textureSettings:
    filterMode: 1
    aniso: 1
    wrapU: 1
    wrapV: 1
  spriteMode: 1
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  textureType: 8
  textureShape: 1
  platformSettings:
  - serializedVersion: 3
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    textureCompression: 1
    crunchedCompression: 0
  - serializedVersion: 3
    buildTarget: Android
    maxTextureSize: 2048
    textureFormat: 4
    textureCompression: 0
    overridden: 1
"""
    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(content)

# Lấy file mới nhất trong user_uploaded
base_dir = r"C:\Users\thuon\.gemini\antigravity-ide\brain\335cc8b0-53e6-4c93-9562-5f9b22e5ec04\.user_uploaded"
files = glob.glob(os.path.join(base_dir, "*.*"))
files.sort(key=os.path.getmtime)
latest_file = files[-1]
print(f"Latest uploaded file: {latest_file}")

img = Image.open(latest_file).convert("RGBA")
cleaned = clean_alpha(img, threshold=20)
bbox = cleaned.getbbox()
if bbox:
    cleaned = cleaned.crop(bbox)

out_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\HUD"
os.makedirs(out_dir, exist_ok=True)
out_png = os.path.join(out_dir, "Meter_Taiji_YinYang_DongSon.png")
cleaned.save(out_png, "PNG")
make_meta(out_png)

print(f"Processed Taiji Meter: {cleaned.size} -> {out_png}")
