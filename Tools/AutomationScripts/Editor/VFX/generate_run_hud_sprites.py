import os
import math
import uuid
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

HUD_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\HUD"
BADGES_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\Badges"

os.makedirs(HUD_DIR, exist_ok=True)
os.makedirs(BADGES_DIR, exist_ok=True)

def write_unity_meta(filepath, border=(0, 0, 0, 0), pivot=(0.5, 0.5)):
    meta_path = filepath + ".meta"
    guid = uuid.uuid4().hex
    bx, by, bz, bw = border  # Left, Bottom, Right, Top
    px, py = pivot
    meta_content = f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: {px}, y: {py}}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: {bx}, y: {by}, z: {bz}, w: {bw}}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 3
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
"""
    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(meta_content)

def create_bar_frame(filename, size=(256, 48), radius=16, bg_color=(20, 24, 30, 240), border_color=(35, 42, 54, 255)):
    """Khung thanh máu/EXP 3D Chibi bo góc"""
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Đổ bóng đáy
    draw.rounded_rectangle([2, 4, w - 2, h - 2], radius=radius, fill=(10, 12, 16, 255))
    # Nền trong
    draw.rounded_rectangle([3, 3, w - 3, h - 3], radius=radius, fill=bg_color)
    # Viền ngoài
    draw.rounded_rectangle([2, 2, w - 2, h - 4], radius=radius, outline=(50, 60, 75, 255), width=3)
    # Inner dark shadow
    draw.rounded_rectangle([5, 5, w - 5, h - 5], radius=radius-2, outline=(10, 14, 18, 200), width=2)

    out_path = os.path.join(HUD_DIR, filename)
    img.save(out_path, "PNG")
    write_unity_meta(out_path, border=(18, 18, 18, 18))
    print(f"Created {filename}")

def create_bar_fill(filename, size=(128, 48), top_color=(245, 60, 60), bot_color=(175, 20, 25), radius=12):
    """Ruột thanh Máu/EXP gradient bóng"""
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    
    grad_img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    grad_draw = ImageDraw.Draw(grad_img)
    for y in range(h):
        t = y / float(h)
        r = int(top_color[0] * (1 - t) + bot_color[0] * t)
        g = int(top_color[1] * (1 - t) + bot_color[1] * t)
        b = int(top_color[2] * (1 - t) + bot_color[2] * t)
        grad_draw.line([(0, y), (w, y)], fill=(r, g, b, 255))
    
    # Highlight trên
    hl_mask = Image.new("L", (w, h), 0)
    hl_draw = ImageDraw.Draw(hl_mask)
    hl_draw.rectangle([0, 0, w, h // 2], fill=70)
    hl_layer = Image.new("RGBA", (w, h), (255, 255, 255, 255))
    grad_img.paste(hl_layer, (0, 0), hl_mask)

    mask = Image.new("L", (w, h), 0)
    mask_draw = ImageDraw.Draw(mask)
    mask_draw.rounded_rectangle([2, 2, w - 2, h - 2], radius=radius, fill=255)
    img.paste(grad_img, (0, 0), mask)

    out_path = os.path.join(HUD_DIR, filename)
    img.save(out_path, "PNG")
    write_unity_meta(out_path, border=(14, 14, 14, 14))
    print(f"Created {filename}")

def create_level_badge(filename, size=(128, 128)):
    """Huy hiệu level tròn 3D Chibi sao vàng"""
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = w // 2, h // 2

    # Đổ bóng đáy
    draw.ellipse([cx - 56, cy - 52, cx + 56, cy + 60], fill=(15, 12, 8, 255))
    # Viền vàng 3D ngoài
    draw.ellipse([cx - 54, cy - 54, cx + 54, cy + 54], fill=(245, 195, 45, 255), outline=(90, 60, 10, 255), width=4)
    # Lớp trong màu nâu gỗ sẫm / xanh navy
    draw.ellipse([cx - 44, cy - 44, cx + 44, cy + 44], fill=(28, 34, 46, 255), outline=(255, 230, 120, 200), width=3)
    # Highlight vòng trên
    draw.arc([cx - 40, cy - 40, cx + 40, cy + 40], start=190, end=350, fill=(255, 255, 255, 180), width=3)

    out_path = os.path.join(BADGES_DIR, filename)
    img.save(out_path, "PNG")
    write_unity_meta(out_path, border=(0, 0, 0, 0), pivot=(0.5, 0.5))
    print(f"Created {filename}")

def create_taiji_orb(filename, size=(160, 160)):
    """Viên ngọc Thái Cực Âm Dương 3D phát sáng Băng/Lửa"""
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = w // 2, h // 2

    # Hào quang mờ 2 màu
    for r in range(74, 58, -3):
        alpha = int(45 * (r - 58) / 16)
        draw.chord([cx - r, cy - r, cx + r, cy + r], start=90, end=270, fill=(40, 170, 240, alpha))
        draw.chord([cx - r, cy - r, cx + r, cy + r], start=270, end=90, fill=(240, 90, 40, alpha))

    # Viền kim loại 3D
    draw.ellipse([cx - 60, cy - 58, cx + 60, cy + 62], fill=(20, 15, 10, 255))
    draw.ellipse([cx - 58, cy - 58, cx + 58, cy + 58], fill=(225, 180, 50, 255), outline=(50, 30, 8, 255), width=3)
    
    # Nửa Trắng (Dương / Băng Lam)
    draw.chord([cx - 50, cy - 50, cx + 50, cy + 50], start=90, end=270, fill=(230, 245, 255, 255))
    # Nửa Đen (Âm / Hắc Ám)
    draw.chord([cx - 50, cy - 50, cx + 50, cy + 50], start=270, end=90, fill=(25, 28, 38, 255))
    
    # 2 Vòng xoắn Thái Cực
    draw.ellipse([cx - 25, cy - 50, cx + 25, cy], fill=(230, 245, 255, 255))
    draw.ellipse([cx - 25, cy, cx + 25, cy + 50], fill=(25, 28, 38, 255))
    
    # 2 Mắt Âm Dương
    draw.ellipse([cx - 8, cy - 33, cx + 8, cy - 17], fill=(25, 28, 38, 255))
    draw.ellipse([cx - 8, cy + 17, cx + 8, cy + 33], fill=(230, 245, 255, 255))

    # Highlight bóng ngọc
    draw.arc([cx - 46, cy - 46, cx + 46, cy + 46], start=200, end=340, fill=(255, 255, 255, 150), width=3)

    out_path = os.path.join(HUD_DIR, filename)
    img.save(out_path, "PNG")
    write_unity_meta(out_path, border=(0, 0, 0, 0), pivot=(0.5, 0.5))
    print(f"Created {filename}")

def create_yinyang_bar_bg(filename, size=(240, 36), radius=14):
    """Thanh cân bằng Âm Dương nền capsule"""
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    draw.rounded_rectangle([2, 4, w - 2, h - 2], radius=radius, fill=(12, 15, 20, 255))
    draw.rounded_rectangle([3, 2, w - 3, h - 4], radius=radius, fill=(24, 28, 38, 255), outline=(220, 180, 50, 255), width=2)
    # Vạch chia trung tâm
    cx = w // 2
    draw.line([(cx, 6), (cx, h - 8)], fill=(255, 230, 120, 180), width=2)

    out_path = os.path.join(HUD_DIR, filename)
    img.save(out_path, "PNG")
    write_unity_meta(out_path, border=(16, 12, 16, 12))
    print(f"Created {filename}")

def create_run_stats_pill(filename, size=(220, 84), radius=22):
    """Khung Top-Right hiển thị Thời Gian & Số Quái diệt"""
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    draw.rounded_rectangle([2, 4, w - 2, h - 2], radius=radius, fill=(12, 14, 18, 255))
    draw.rounded_rectangle([3, 2, w - 3, h - 4], radius=radius, fill=(28, 32, 42, 240), outline=(245, 195, 45, 255), width=3)
    draw.arc([8, 6, w - 8, h - 8], start=190, end=350, fill=(255, 255, 255, 100), width=2)

    out_path = os.path.join(HUD_DIR, filename)
    img.save(out_path, "PNG")
    write_unity_meta(out_path, border=(24, 24, 24, 24))
    print(f"Created {filename}")

if __name__ == "__main__":
    # 1. Khung & Ruột Máu HP (Đỏ Dâu 3D)
    create_bar_frame("Bar_HP_Chunky_Frame.png", size=(256, 44), radius=16)
    create_bar_fill("Bar_HP_Chunky_Fill.png", size=(128, 44), top_color=(255, 75, 75), bot_color=(175, 20, 25), radius=12)

    # 2. Khung & Ruột EXP (Vàng Kim / Ngọc Lục Bảo 3D)
    create_bar_frame("Bar_EXP_Chunky_Frame.png", size=(256, 32), radius=12)
    create_bar_fill("Bar_EXP_Chunky_Fill.png", size=(128, 32), top_color=(255, 215, 55), bot_color=(195, 140, 15), radius=10)

    # 3. Huy hiệu Level Chibi
    create_level_badge("Badge_Level_Chibi_Star.png", size=(128, 128))

    # 4. Ngọc Thái Cực & Thanh Trượt Âm Dương
    create_taiji_orb("Meter_Taiji_Orb_Chibi.png", size=(160, 160))
    create_yinyang_bar_bg("Gauge_YinYang_Bar_Chibi.png", size=(240, 36), radius=14)

    # 5. Khung Run Stats Top-Right
    create_run_stats_pill("Panel_RunStats_Pill_3D.png", size=(220, 84), radius=22)

    print("All Run HUD Sprites generated successfully!")
