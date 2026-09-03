import os, uuid
from PIL import Image
import numpy as np

def clean_alpha(img_rgba, threshold=24):
    data = np.array(img_rgba)
    r, g, b = data[:, :, 0], data[:, :, 1], data[:, :, 2]
    max_rgb = np.maximum(np.maximum(r, g), b)
    alpha = np.clip((max_rgb.astype(float) - threshold) / (threshold * 1.5), 0.0, 1.0) * 255.0
    data[:, :, 3] = alpha.astype(np.uint8)
    return Image.fromarray(data, mode="RGBA")

def make_meta(png_path, border=(50, 40, 50, 40)):
    meta_path = png_path + ".meta"
    guid = uuid.uuid4().hex
    L, B, R, T = border
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
  spriteBorder: {{x: {L}, y: {B}, z: {R}, w: {T}}}
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

src_path = r"C:\Users\thuon\.gemini\antigravity-ide\brain\335cc8b0-53e6-4c93-9562-5f9b22e5ec04\.user_uploaded\media_1787818070779.png"
raw_img = Image.open(src_path).convert("RGBA")
W, H = raw_img.size
print(f"Original Buttons Image size: {W} x {H}")

# 3 nút bấm xếp ngang: Đỏ, Xanh Ngọc, Vàng Đồng
boxes = {
    "Btn_Action_CinnabarRed": (0.01, 0.30, 0.33, 0.65, (60, 40, 60, 40)),
    "Btn_Action_JadeGreen": (0.34, 0.30, 0.66, 0.65, (60, 40, 60, 40)),
    "Btn_Action_DragonGold": (0.67, 0.30, 0.99, 0.65, (60, 40, 60, 40)),
}

out_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\Buttons"
os.makedirs(out_dir, exist_ok=True)

for name, (x1, y1, x2, y2, border) in boxes.items():
    crop_rect = (int(x1 * W), int(y1 * H), int(x2 * W), int(y2 * H))
    cropped = raw_img.crop(crop_rect)
    cleaned = clean_alpha(cropped, threshold=24)
    bbox = cleaned.getbbox()
    if bbox:
        cleaned = cleaned.crop(bbox)
    
    out_png = os.path.join(out_dir, f"{name}.png")
    cleaned.save(out_png, "PNG")
    make_meta(out_png, border)
    print(f"Saved {name}: {cleaned.size} -> {out_png}")

print("ALL ACTION BUTTONS EXTRACTED & 9-SLICE META CREATED!")
