import os
import math
import uuid
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

FRAMES_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\Frames"
HUD_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\HUD"
BUTTONS_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\Buttons"

os.makedirs(FRAMES_DIR, exist_ok=True)
os.makedirs(HUD_DIR, exist_ok=True)
os.makedirs(BUTTONS_DIR, exist_ok=True)

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

def create_column_card_bg(filename, size=(384, 512), radius=28):
    """Khung nền từng cột (Trái/Phải) trong Tàng Bảo Các 9-slice Chibi Arcade 3D"""
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Đổ bóng ngoài
    draw.rounded_rectangle([4, 8, w - 4, h - 4], radius=radius, fill=(8, 10, 14, 255))
    # Viền vàng kim 3D ngoài
    draw.rounded_rectangle([4, 4, w - 4, h - 8], radius=radius, fill=(215, 165, 40, 255), outline=(45, 28, 8, 255), width=3)
    # Lòng trong xanh đen sẫm hiện đại
    draw.rounded_rectangle([10, 10, w - 10, h - 14], radius=radius-4, fill=(24, 28, 38, 255))

    # Highlight trên đỉnh
    draw.arc([16, 12, w - 16, h - 16], start=190, end=350, fill=(255, 255, 255, 100), width=2)

    out_path = os.path.join(FRAMES_DIR, filename)
    img.save(out_path, "PNG")
    write_unity_meta(out_path, border=(32, 32, 32, 32))
    print(f"Created {filename}")

def create_slot_inventory_3d(filename, size=(128, 128), radius=18):
    """Ô slot kho đồ 3D Chibi Arcade"""
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Đổ bóng đáy
    draw.rounded_rectangle([2, 5, w - 2, h - 2], radius=radius, fill=(10, 12, 16, 255))
    # Viền vàng 3D
    draw.rounded_rectangle([2, 2, w - 2, h - 5], radius=radius, fill=(230, 180, 45, 255), outline=(50, 30, 8, 255), width=3)
    # Lòng trong xanh navy tối
    draw.rounded_rectangle([8, 8, w - 8, h - 11], radius=radius-4, fill=(32, 38, 52, 255), outline=(18, 22, 30, 255), width=2)
    # Inner highlight
    draw.arc([10, 10, w - 10, h - 10], start=200, end=340, fill=(255, 255, 255, 120), width=2)

    out_path = os.path.join(FRAMES_DIR, filename)
    img.save(out_path, "PNG")
    write_unity_meta(out_path, border=(20, 20, 20, 20))
    print(f"Created {filename}")

def create_slot_inventory_selected(filename, size=(128, 128), radius=18):
    """Ô slot kho đồ khi được CHỌN (Phát sáng xanh ngọc rực rỡ)"""
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Hào quang phát sáng xanh ngọc ngoài
    for r in range(4, 0, -1):
        alpha = int(60 * r / 4)
        draw.rounded_rectangle([2 - r, 2 - r, w - 2 + r, h - 2 + r], radius=radius + r, outline=(40, 240, 200, alpha), width=2)

    # Đổ bóng đáy
    draw.rounded_rectangle([2, 5, w - 2, h - 2], radius=radius, fill=(10, 12, 16, 255))
    # Viền xanh ngọc 3D
    draw.rounded_rectangle([2, 2, w - 2, h - 5], radius=radius, fill=(40, 220, 180, 255), outline=(10, 60, 50, 255), width=3)
    # Lòng trong
    draw.rounded_rectangle([8, 8, w - 8, h - 11], radius=radius-4, fill=(24, 48, 56, 255), outline=(12, 30, 36, 255), width=2)
    # Highlight
    draw.arc([10, 10, w - 10, h - 10], start=200, end=340, fill=(255, 255, 255, 180), width=2)

    out_path = os.path.join(FRAMES_DIR, filename)
    img.save(out_path, "PNG")
    write_unity_meta(out_path, border=(20, 20, 20, 20))
    print(f"Created {filename}")

def create_stat_gauge(filename_frame, filename_fill_dmg, filename_fill_cd, size=(256, 32), radius=10):
    """Thanh đo chỉ số Sát thương & Hồi chiêu 3D"""
    w, h = size
    
    # 1. Frame
    img_frame = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw_f = ImageDraw.Draw(img_frame)
    draw_f.rounded_rectangle([2, 3, w - 2, h - 2], radius=radius, fill=(10, 12, 16, 255))
    draw_f.rounded_rectangle([2, 2, w - 2, h - 3], radius=radius, fill=(22, 26, 36, 255), outline=(50, 60, 75, 255), width=2)
    
    out_f = os.path.join(HUD_DIR, filename_frame)
    img_frame.save(out_f, "PNG")
    write_unity_meta(out_f, border=(12, 10, 12, 10))

    # 2. Fill Sát Thương (Đỏ Cam Rực)
    img_dmg = Image.new("RGBA", (128, h), (0, 0, 0, 0))
    draw_d = ImageDraw.Draw(img_dmg)
    draw_d.rounded_rectangle([2, 2, 126, h - 2], radius=radius-2, fill=(245, 80, 50, 255))
    draw_d.rectangle([2, 2, 126, h//2], fill=(255, 140, 100, 160)) # Highlight
    
    out_d = os.path.join(HUD_DIR, filename_fill_dmg)
    img_dmg.save(out_d, "PNG")
    write_unity_meta(out_d, border=(10, 8, 10, 8))

    # 3. Fill Hồi Chiêu (Xanh Cyan Băng Lam)
    img_cd = Image.new("RGBA", (128, h), (0, 0, 0, 0))
    draw_c = ImageDraw.Draw(img_cd)
    draw_c.rounded_rectangle([2, 2, 126, h - 2], radius=radius-2, fill=(40, 195, 240, 255))
    draw_c.rectangle([2, 2, 126, h//2], fill=(130, 235, 255, 160)) # Highlight
    
    out_c = os.path.join(HUD_DIR, filename_fill_cd)
    img_cd.save(out_c, "PNG")
    write_unity_meta(out_c, border=(10, 8, 10, 8))

    print(f"Created Stat Gauges: {filename_frame}, {filename_fill_dmg}, {filename_fill_cd}")

if __name__ == "__main__":
    # 1. Khung nền 2 cột Tàng Bảo Các
    create_column_card_bg("Panel_Column_Card_9Slice.png", size=(384, 512), radius=28)

    # 2. Slot Kho Đồ 3D (Normal & Selected Glow)
    create_slot_inventory_3d("Slot_Inventory_Chunky_3D.png", size=(128, 128), radius=18)
    create_slot_inventory_selected("Slot_Inventory_Selected_Glow.png", size=(128, 128), radius=18)

    # 3. Thanh đo chỉ số Sát Thương & Hồi Chiêu
    create_stat_gauge("Gauge_Stat_Bar_Frame.png", "Gauge_Stat_Fill_Damage.png", "Gauge_Stat_Fill_Cooldown.png", size=(256, 30), radius=10)

    print("All Weapon Loadout UI Sprites generated successfully!")
