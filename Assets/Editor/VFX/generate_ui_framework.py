"""
Pipeline Tự Động Hóa Sinh UI Sprites (Cổ Phong Đông Sơn - Anime URP)
Dự án: Projectzombie (Vong Xuyên)
"""

import os
import math
import uuid
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

ASSETS_ROOT = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI"
FRAMES_DIR = os.path.join(ASSETS_ROOT, "Frames")
JOYSTICK_DIR = os.path.join(ASSETS_ROOT, "Joystick")
HUD_DIR = os.path.join(ASSETS_ROOT, "HUD")
BADGES_DIR = os.path.join(ASSETS_ROOT, "Badges")
BUTTONS_DIR = os.path.join(ASSETS_ROOT, "Buttons")

def write_unity_meta(filepath, border=(0, 0, 0, 0), pivot=(0.5, 0.5)):
    meta_path = filepath + ".meta"
    if os.path.exists(meta_path):
        return
    guid = uuid.uuid4().hex
    bx, by, bz, bw = border  # L, B, R, T
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
  - serializedVersion: 3
    buildTarget: Android
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
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: 5e97eb03825dee720800000000000000
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(meta_content)

# -------------------------------------------------------------
# 1. TẠO THẺ BÀI LÊN CẤP 9-SLICE (UPGRADE CARD FRAMES)
# -------------------------------------------------------------
def generate_upgrade_card_frame(output_path, border_color, glow_color, bg_color=(25, 20, 26, 245)):
    """Sinh Thẻ bài Lệnh bài Đạo Gia 9-Slice với 4 góc hoa văn mây cuộn và viền đồng cổ."""
    w, h = 256, 384
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # 1. Nền thẻ gỗ mun / giấy dó u tối (Bo góc 18px)
    margin = 8
    draw.rounded_rectangle([margin, margin, w - margin, h - margin], radius=18, fill=bg_color)
    
    # 2. Hào quang phát sáng ở viền ngoài (Outer Glow)
    draw.rounded_rectangle([margin, margin, w - margin, h - margin], radius=18, outline=glow_color + (140,), width=3)
    
    # 3. Viền chính bằng đồng thau cổ
    draw.rounded_rectangle([margin + 2, margin + 2, w - margin - 2, h - margin - 2], radius=16, outline=border_color + (255,), width=2)
    
    # 4. Viền chỉ phụ bên trong
    draw.rounded_rectangle([margin + 7, margin + 7, w - margin - 7, h - margin - 7], radius=12, outline=border_color + (90,), width=1)
    
    # 5. Họa tiết 4 góc Vân Mây Cuộn / Hồi Văn Triện Cổ
    corner_size = 28
    corners = [
        (margin + 5, margin + 5, 1, 1),            # Top-Left
        (w - margin - 5, margin + 5, -1, 1),       # Top-Right
        (margin + 5, h - margin - 5, 1, -1),       # Bottom-Left
        (w - margin - 5, h - margin - 5, -1, -1)   # Bottom-Right
    ]
    
    for cx, cy, sx, sy in corners:
        # Góc triện vuông
        draw.line([(cx, cy), (cx + sx * corner_size, cy)], fill=border_color + (255,), width=2)
        draw.line([(cx, cy), (cx, cy + sy * corner_size)], fill=border_color + (255,), width=2)
        draw.line([(cx + sx * 6, cy + sy * 6), (cx + sx * (corner_size - 6), cy + sy * 6)], fill=glow_color + (200,), width=1)
        draw.line([(cx + sx * 6, cy + sy * 6), (cx + sx * 6, cy + sy * (corner_size - 6))], fill=glow_color + (200,), width=1)
        # Chấm ngọc phong thủy ở góc
        draw.ellipse([cx + sx * 8 - 3, cy + sy * 8 - 3, cx + sx * 8 + 3, cy + sy * 8 + 3], fill=glow_color + (255,))

    img.save(output_path, "PNG")
    write_unity_meta(output_path, border=(36, 36, 36, 36))
    print(f"Generated 9-Slice Card Frame: {output_path}")

# -------------------------------------------------------------
# 2. TẠO CẦN ĐIỀU KHIỂN CẢM ỨNG (JOYSTICK BASE & KNOB)
# -------------------------------------------------------------
def generate_joystick_base(output_path):
    """Vòng tròn Trống Đồng Đông Sơn lồng Bát Quái trận đồ (Alpha bán trong suốt 35%)."""
    w, h = 256, 256
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = w // 2, h // 2
    r_outer = 118
    
    # Vòng nền mờ
    draw.ellipse([cx - r_outer, cy - r_outer, cx + r_outer, cy + r_outer], fill=(18, 15, 24, 75))
    
    # Viền ngoài đôi kim loại đồng thau
    draw.ellipse([cx - r_outer, cy - r_outer, cx + r_outer, cy + r_outer], outline=(201, 168, 106, 120), width=2)
    draw.ellipse([cx - r_outer + 6, cy - r_outer + 6, cx + r_outer - 6, cy + r_outer - 6], outline=(201, 168, 106, 70), width=1)
    
    # 8 Cung Bát Quái phân nhánh
    for i in range(8):
        angle = i * (math.pi / 4)
        x1 = cx + (r_outer - 22) * math.cos(angle)
        y1 = cy + (r_outer - 22) * math.sin(angle)
        x2 = cx + (r_outer - 6) * math.cos(angle)
        y2 = cy + (r_outer - 6) * math.sin(angle)
        draw.line([(x1, y1), (x2, y2)], fill=(255, 215, 0, 110), width=2)
        
    # Vòng tròn tâm Mặt Trời Đông Sơn 12 tia
    r_sun = 32
    draw.ellipse([cx - r_sun, cy - r_sun, cx + r_sun, cy + r_sun], outline=(201, 168, 106, 140), width=1)
    for i in range(12):
        angle = i * (math.pi / 6)
        tx = cx + (r_sun + 8) * math.cos(angle)
        ty = cy + (r_sun + 8) * math.sin(angle)
        draw.line([(cx, cy), (tx, ty)], fill=(255, 235, 150, 90), width=1)
        
    img = img.filter(ImageFilter.GaussianBlur(radius=0.5))
    img.save(output_path, "PNG")
    write_unity_meta(output_path)
    print(f"Generated Joystick Base: {output_path}")

def generate_joystick_knob(output_path):
    """Viên Thái Cực Ngọc Châu bát giác viền vàng đồng sáng bóng."""
    w, h = 128, 128
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = w // 2, h // 2
    r = 54
    
    # 1. Nền ngọc bích phát quang
    draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(20, 45, 45, 235))
    draw.ellipse([cx - r + 4, cy - r + 4, cx + r - 4, cy + r - 4], fill=(40, 110, 100, 245))
    
    # 2. Hào quang viền đồng vàng
    draw.ellipse([cx - r, cy - r, cx + r, cy + r], outline=(255, 215, 0, 255), width=3)
    draw.ellipse([cx - r + 5, cy - r + 5, cx + r - 5, cy + r - 5], outline=(255, 245, 180, 200), width=1)
    
    # 3. Biểu tượng Thái Cực Âm Dương ở tâm
    draw.ellipse([cx - 18, cy - 18, cx + 18, cy + 18], fill=(240, 245, 255, 255), outline=(10, 10, 15, 255), width=1)
    draw.pieslice([cx - 18, cy - 18, cx + 18, cy + 18], start=90, end=270, fill=(15, 18, 25, 255))
    draw.ellipse([cx - 9, cy - 18, cx + 9, cy], fill=(15, 18, 25, 255))
    draw.ellipse([cx - 9, cy, cx + 9, cy + 18], fill=(240, 245, 255, 255))
    draw.ellipse([cx - 3, cy - 11, cx + 3, cy - 5], fill=(240, 245, 255, 255))
    draw.ellipse([cx - 3, cy + 5, cx + 3, cy + 11], fill=(15, 18, 25, 255))
    
    img = img.filter(ImageFilter.GaussianBlur(radius=0.4))
    img.save(output_path, "PNG")
    write_unity_meta(output_path)
    print(f"Generated Joystick Knob: {output_path}")

# -------------------------------------------------------------
# 3. TẠO THANH HP CHU SA & THANH EXP LAM NGỌC (HUD)
# -------------------------------------------------------------
def generate_hud_bar_frame(output_path):
    """Khung thanh máu/EXP 9-slice dạng ống đồng cổ phong."""
    w, h = 256, 48
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Nền rãnh trượt tối
    draw.rounded_rectangle([2, 2, w - 2, h - 2], radius=10, fill=(15, 12, 18, 240), outline=(90, 77, 65, 255), width=2)
    # Khung viền đồng sáng ấm
    draw.rounded_rectangle([4, 4, w - 4, h - 4], radius=8, outline=(201, 168, 106, 255), width=2)
    # Khóa ngọc 2 đầu
    draw.rectangle([2, 6, 12, h - 6], fill=(201, 168, 106, 255))
    draw.rectangle([w - 12, 6, w - 2, h - 6], fill=(201, 168, 106, 255))
    
    img.save(output_path, "PNG")
    write_unity_meta(output_path, border=(18, 12, 18, 12))
    print(f"Generated HUD Bar Frame: {output_path}")

def generate_hud_bar_fill(output_path, color_top, color_bot, glow_line):
    """Ruột thanh máu (Đỏ Chu Sa) hoặc EXP (Lam Ngọc) với hiệu ứng gradient phát sáng."""
    w, h = 64, 32
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    for y in range(h):
        t = y / float(h)
        r = int(color_top[0] * (1 - t) + color_bot[0] * t)
        g = int(color_top[1] * (1 - t) + color_bot[1] * t)
        b = int(color_top[2] * (1 - t) + color_bot[2] * t)
        draw.line([(0, y), (w, y)], fill=(r, g, b, 255))
        
    # Vệt sáng highlight ở nửa trên
    draw.line([(0, 3), (w, 3)], fill=glow_line + (220,), width=2)
    
    img.save(output_path, "PNG")
    write_unity_meta(output_path, border=(4, 4, 4, 4))
    print(f"Generated HUD Bar Fill: {output_path}")

# -------------------------------------------------------------
# 4. TẠO NÚT BẤM LỆNH BÀI (BUTTONS)
# -------------------------------------------------------------
def generate_primary_button(output_path, normal=True):
    """Thẻ lệnh bài nút bấm 9-Slice."""
    w, h = 192, 64
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    bg = (35, 27, 24, 250) if normal else (55, 20, 20, 255)
    border = (201, 168, 106) if normal else (255, 215, 0)
    
    draw.rounded_rectangle([3, 3, w - 3, h - 3], radius=12, fill=bg, outline=border + (255,), width=3)
    draw.rounded_rectangle([7, 7, w - 7, h - 7], radius=8, outline=(255, 240, 180, 120), width=1)
    
    # Triện góc nút
    draw.rectangle([5, 5, 14, 14], fill=border + (255,))
    draw.rectangle([w - 14, 5, w - 5, 14], fill=border + (255,))
    draw.rectangle([5, h - 14, 14, h - 5], fill=border + (255,))
    draw.rectangle([w - 14, h - 14, w - 5, h - 5], fill=border + (255,))
    
    img.save(output_path, "PNG")
    write_unity_meta(output_path, border=(20, 20, 20, 20))
    print(f"Generated Primary Button: {output_path}")

# -------------------------------------------------------------
# MAIN GENERATOR PIPELINE
# -------------------------------------------------------------
if __name__ == "__main__":
    print("=== START GENERATING DONG SON ANIME UI ASSETS ===")
    
    # 1. Thẻ bài nâng cấp 3 cấp độ
    generate_upgrade_card_frame(os.path.join(FRAMES_DIR, "Frame_Card_Common.png"), 
                                border_color=(168, 162, 158), glow_color=(210, 210, 210))
    generate_upgrade_card_frame(os.path.join(FRAMES_DIR, "Frame_Card_Rare.png"), 
                                border_color=(77, 238, 234), glow_color=(120, 255, 245))
    generate_upgrade_card_frame(os.path.join(FRAMES_DIR, "Frame_Card_Evolution.png"), 
                                border_color=(255, 215, 0), glow_color=(255, 245, 150))
    
    # 2. Joystick
    generate_joystick_base(os.path.join(JOYSTICK_DIR, "Joystick_Base_DongSon.png"))
    generate_joystick_knob(os.path.join(JOYSTICK_DIR, "Joystick_Knob_Taiji.png"))
    
    # 3. HUD Bars
    generate_hud_bar_frame(os.path.join(HUD_DIR, "Frame_HUD_Bar.png"))
    generate_hud_bar_fill(os.path.join(HUD_DIR, "Fill_HP_ChuSa.png"), 
                          color_top=(235, 60, 60), color_bot=(140, 20, 20), glow_line=(255, 160, 160))
    generate_hud_bar_fill(os.path.join(HUD_DIR, "Fill_EXP_LamNgoc.png"), 
                          color_top=(60, 240, 230), color_bot=(15, 110, 120), glow_line=(180, 255, 250))
    
    # 4. Buttons
    generate_primary_button(os.path.join(BUTTONS_DIR, "Btn_Primary_Normal.png"), normal=True)
    generate_primary_button(os.path.join(BUTTONS_DIR, "Btn_Primary_Pressed.png"), normal=False)
    
    print("=== UI ASSETS & 9-SLICE METAS GENERATED SUCCESSFULLY ===")
