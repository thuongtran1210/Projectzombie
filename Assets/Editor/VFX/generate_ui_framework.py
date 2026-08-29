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
SKILLS_DIR = os.path.join(ASSETS_ROOT, "Skills")
UPGRADE_ICONS_DIR = os.path.join(ASSETS_ROOT, "UpgradeIcons")

os.makedirs(SKILLS_DIR, exist_ok=True)
os.makedirs(UPGRADE_ICONS_DIR, exist_ok=True)

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

def generate_yinyang_fill(output_path):
    """Ruột thanh Âm Dương Thái Cực: Gradient chuyển tiếp giữa Lam Ngọc (Dương) và Mực Nho Tím (Âm)."""
    w, h = 64, 32
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    for x in range(w):
        t = x / float(w)
        # Chuyển sắc từ Xanh Lam Ngọc sang Tím Huyền Bí
        r = int(60 * (1 - t) + 160 * t)
        g = int(220 * (1 - t) + 60 * t)
        b = int(240 * (1 - t) + 220 * t)
        draw.line([(x, 0), (x, h)], fill=(r, g, b, 255))
        
    draw.line([(0, 3), (w, 3)], fill=(255, 255, 255, 220), width=2)
    img.save(output_path, "PNG")
    write_unity_meta(output_path, border=(4, 4, 4, 4))
    print(f"Generated YinYang Fill: {output_path}")

def generate_slider_handle(output_path):
    """Nút con trỏ trượt hình Hạt Linh Châu Đồng Vàng (64x64)."""
    size = 64
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    center = size / 2.0
    r = 24.0
    
    # 1. Glow
    glow_img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    g_draw = ImageDraw.Draw(glow_img)
    g_draw.ellipse([center - r - 4, center - r - 4, center + r + 4, center + r + 4], fill=(255, 215, 0, 160))
    glow_img = glow_img.filter(ImageFilter.GaussianBlur(radius=4))
    img = Image.alpha_composite(img, glow_img)
    draw = ImageDraw.Draw(img)
    
    # 2. Thân hạt linh châu đồng thau
    draw.ellipse([center - r, center - r, center + r, center + r], fill=(240, 200, 100, 255), outline=(120, 90, 40, 255), width=3)
    # 3. Điểm sáng ngọc tâm
    draw.ellipse([center - 10, center - 14, center + 4, center], fill=(255, 255, 255, 230))
    
    img.save(output_path, "PNG")
    write_unity_meta(output_path, border=(0, 0, 0, 0), pivot=(0.5, 0.5))
    print(f"Generated Slider Handle: {output_path}")

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
# 5. LINH CHÂU & HUY HIỆU NGŨ HÀNH (ELEMENT BADGES)
# -------------------------------------------------------------
def generate_element_badge(output_path, element_type="Kim", main_color=(232, 196, 104), glow_color=(255, 243, 196)):
    """
    Sinh Linh Châu Ngũ Hành 256x256 viền kim loại Đông Sơn phát quang:
    - Kim: Lưỡi Kiếm / Hình Thoi
    - Mộc: Chiếc Lá / Mầm Cây
    - Thủy: Giọt Nước
    - Hỏa: Ngọn Lửa
    - Thổ: Khối Núi Đá / Vuông Vát
    """
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    center = size / 2.0
    r = 110.0
    
    # 1. Glow viền ngoài
    glow_img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow_img)
    glow_draw.ellipse([center - r - 6, center - r - 6, center + r + 6, center + r + 6], 
                      fill=(*glow_color, 120))
    glow_img = glow_img.filter(ImageFilter.GaussianBlur(radius=8))
    img = Image.alpha_composite(img, glow_img)
    draw = ImageDraw.Draw(img)
    
    # 2. Vành kim loại đồng thau cổ bên ngoài
    draw.ellipse([center - r, center - r, center + r, center + r], 
                 fill=(25, 22, 28, 240), outline=(201, 168, 106, 255), width=6)
    
    # 3. Vòng tròn rãnh hoa văn phụ
    draw.ellipse([center - r + 14, center - r + 14, center + r - 14, center + r - 14], 
                 outline=(*main_color, 180), width=3)
    
    # 4. Nền ngọc linh khí bên trong
    inner_r = r - 22
    for ring in range(int(inner_r), 0, -2):
        factor = ring / inner_r
        cr = int(main_color[0] * 0.3 + main_color[0] * 0.7 * (1.0 - factor * 0.5))
        cg = int(main_color[1] * 0.3 + main_color[1] * 0.7 * (1.0 - factor * 0.5))
        cb = int(main_color[2] * 0.3 + main_color[2] * 0.7 * (1.0 - factor * 0.5))
        draw.ellipse([center - ring, center - ring, center + ring, center + ring], 
                     fill=(cr, cg, cb, 255))
    
    # 5. Biểu tượng hình học đặc trưng Ngũ Hành ở tâm
    shape_img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    s_draw = ImageDraw.Draw(shape_img)
    
    if element_type == "Kim":
        # Hình Thoi / Lưỡi Kiếm
        points = [(center, center - 60), (center + 45, center), (center, center + 60), (center - 45, center)]
        s_draw.polygon(points, fill=(255, 255, 255, 230), outline=(255, 243, 196, 255))
        # Rãnh kiếm ở giữa
        s_draw.line([(center, center - 45), (center, center + 45)], fill=(180, 150, 60, 255), width=4)
        
    elif element_type == "Moc":
        # Chiếc Lá
        leaf_pts = [
            (center, center - 55),
            (center + 40, center - 15),
            (center + 30, center + 35),
            (center, center + 55),
            (center - 30, center + 35),
            (center - 40, center - 15)
        ]
        s_draw.polygon(leaf_pts, fill=(255, 255, 255, 230), outline=(143, 201, 122, 255))
        # Gân lá
        s_draw.line([(center, center - 40), (center, center + 45)], fill=(40, 100, 30, 255), width=3)
        s_draw.line([(center, center - 10), (center + 20, center - 25)], fill=(40, 100, 30, 255), width=3)
        s_draw.line([(center, center + 15), (center - 20, center)], fill=(40, 100, 30, 255), width=3)
        
    elif element_type == "Thuy":
        # Giọt Nước
        water_pts = [
            (center, center - 55),
            (center + 42, center + 10),
            (center + 30, center + 45),
            (center, center + 55),
            (center - 30, center + 45),
            (center - 42, center + 10)
        ]
        s_draw.polygon(water_pts, fill=(255, 255, 255, 230), outline=(127, 203, 234, 255))
        # Vệt sóng gợn
        s_draw.arc([center - 25, center - 5, center + 25, center + 35], start=30, end=150, fill=(30, 90, 140, 255), width=4)
        
    elif element_type == "Hoa":
        # Ngọn Lửa
        flame_pts = [
            (center, center - 60),
            (center + 25, center - 25),
            (center + 45, center + 15),
            (center + 25, center + 55),
            (center - 25, center + 55),
            (center - 45, center + 15),
            (center - 20, center - 15),
            (center - 5, center - 30)
        ]
        s_draw.polygon(flame_pts, fill=(255, 255, 255, 230), outline=(255, 138, 80, 255))
        # Tâm lửa rực rỡ
        s_draw.polygon([(center, center - 30), (center + 20, center + 35), (center - 20, center + 35)], fill=(255, 220, 100, 255))
        
    elif element_type == "Tho":
        # Khối Núi Đá Vuông Vát
        rock_pts = [
            (center - 35, center - 45),
            (center + 35, center - 45),
            (center + 50, center + 15),
            (center + 35, center + 50),
            (center - 35, center + 50),
            (center - 50, center + 15)
        ]
        s_draw.polygon(rock_pts, fill=(255, 255, 255, 230), outline=(215, 168, 122, 255))
        # Vân rạn nứt đá cổ
        s_draw.line([(center - 20, center - 30), (center, center), (center + 25, center + 30)], fill=(100, 70, 40, 255), width=4)
        s_draw.line([(center, center), (center - 25, center + 25)], fill=(100, 70, 40, 255), width=3)
    
    img = Image.alpha_composite(img, shape_img)
    
    # 6. Highlight điểm sáng trên đỉnh ngọc (Specular Rim)
    draw = ImageDraw.Draw(img)
    draw.arc([center - inner_r + 6, center - inner_r + 6, center + inner_r - 6, center + inner_r - 6], 
             start=200, end=340, fill=(255, 255, 255, 180), width=4)
    
    img.save(output_path, "PNG")
    write_unity_meta(output_path, border=(0, 0, 0, 0), pivot=(0.5, 0.5))
    print(f"Generated Element Badge ({element_type}): {output_path}")

# -------------------------------------------------------------
# 6. BẢNG ÂM DƯƠNG THÁI CỰC & SLOT TRANG BỊ HUD
# -------------------------------------------------------------
def generate_yinyang_board_frame(output_path):
    """
    Sinh Bảng Gỗ Mun Cổ Phong 9-Slice (512x160) cho Thanh Trạng Thái Âm Dương Thái Cực:
    - Viền kim loại đồng thau chạm khắc
    - 2 đầu nẹp hoa văn Đông Sơn
    - Nền giấy dó mực khói
    """
    w, h = 512, 160
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # 1. Glow viền ngoài
    glow_img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow_img)
    glow_draw.rounded_rectangle([6, 6, w - 6, h - 6], radius=24, outline=(255, 215, 0, 90), width=6)
    glow_img = glow_img.filter(ImageFilter.GaussianBlur(radius=6))
    img = Image.alpha_composite(img, glow_img)
    draw = ImageDraw.Draw(img)
    
    # 2. Thân nền gỗ mun trầm sẫm
    draw.rounded_rectangle([12, 12, w - 12, h - 12], radius=18, fill=(21, 19, 26, 235), outline=(201, 168, 106, 255), width=4)
    
    # 3. Viền trang trí rãnh chỉ vàng bên trong
    draw.rounded_rectangle([20, 20, w - 20, h - 20], radius=14, outline=(90, 77, 65, 200), width=2)
    
    # 4. Nẹp hoa văn triện đồng ở 2 bên mép trái & phải
    # Mép trái
    draw.line([(32, 28), (32, h - 28)], fill=(201, 168, 106, 255), width=3)
    draw.polygon([(26, h/2 - 15), (38, h/2), (26, h/2 + 15)], fill=(255, 215, 0, 255))
    
    # Mép phải
    draw.line([(w - 32, 28), (w - 32, h - 28)], fill=(201, 168, 106, 255), width=3)
    draw.polygon([(w - 26, h/2 - 15), (w - 38, h/2), (w - 26, h/2 + 15)], fill=(255, 215, 0, 255))
    
    img.save(output_path, "PNG")
    write_unity_meta(output_path, border=(45, 25, 45, 25), pivot=(0.5, 0.5))
    print(f"Generated YinYang Board Frame: {output_path}")

def generate_weapon_slot_frame(output_path):
    """
    Sinh Khung Slot Vũ Khí HUD (128x128) 9-Slice:
    - Dạng hình thoi vát ngọc bích / đồng thau cổ
    - 4 góc hoa văn triện
    """
    size = 128
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # 1. Outer Glow
    glow_img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow_img)
    glow_draw.rounded_rectangle([4, 4, size - 4, size - 4], radius=16, outline=(77, 238, 234, 110), width=4)
    glow_img = glow_img.filter(ImageFilter.GaussianBlur(radius=4))
    img = Image.alpha_composite(img, glow_img)
    draw = ImageDraw.Draw(img)
    
    # 2. Thân nền
    draw.rounded_rectangle([8, 8, size - 8, size - 8], radius=14, fill=(18, 16, 24, 220), outline=(201, 168, 106, 255), width=3)
    
    # 3. Viền ngọc bên trong
    draw.rounded_rectangle([14, 14, size - 14, size - 14], radius=10, outline=(77, 238, 234, 180), width=2)
    
    # 4. Góc kim loại
    c_len = 12
    draw.line([(8, 8), (8 + c_len, 8)], fill=(255, 215, 0, 255), width=3)
    draw.line([(8, 8), (8, 8 + c_len)], fill=(255, 215, 0, 255), width=3)
    draw.line([(size - 8, 8), (size - 8 - c_len, 8)], fill=(255, 215, 0, 255), width=3)
    draw.line([(size - 8, 8), (size - 8, 8 + c_len)], fill=(255, 215, 0, 255), width=3)
    draw.line([(8, size - 8), (8 + c_len, size - 8)], fill=(255, 215, 0, 255), width=3)
    draw.line([(8, size - 8), (8, size - 8 - c_len)], fill=(255, 215, 0, 255), width=3)
    draw.line([(size - 8, size - 8), (size - 8 - c_len, size - 8)], fill=(255, 215, 0, 255), width=3)
    draw.line([(size - 8, size - 8), (size - 8, size - 8 - c_len)], fill=(255, 215, 0, 255), width=3)
    
    img.save(output_path, "PNG")
    write_unity_meta(output_path, border=(24, 24, 24, 24), pivot=(0.5, 0.5))
    print(f"Generated Weapon Slot Frame: {output_path}")

def generate_action_button_base(output_path, border_color=(201, 168, 106), glow_color=(255, 215, 0), bg_color=(24, 20, 28), inner_ring_color=(255, 215, 0), is_pressed=False):
    """
    Sinh Khung Nền Nút Bấm Tròn Bát Quái / Đồng Thau Cổ Phong 256x256:
    - Viền ngoài đồng thau chạm khắc 8 chấm cung linh khí
    - Nền sẫm ngọc bích / giấy dó
    - Rãnh hào quang phát sáng
    """
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    center = size / 2.0
    r = 110.0 if not is_pressed else 104.0
    
    # 1. Outer Glow
    glow_img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow_img)
    glow_draw.ellipse([center - r - 6, center - r - 6, center + r + 6, center + r + 6], 
                      fill=(*glow_color, 140 if not is_pressed else 90))
    glow_img = glow_img.filter(ImageFilter.GaussianBlur(radius=8))
    img = Image.alpha_composite(img, glow_img)
    draw = ImageDraw.Draw(img)
    
    # 2. Vành kim loại ngoài
    draw.ellipse([center - r, center - r, center + r, center + r], 
                 fill=bg_color + (245,), outline=border_color + (255,), width=6)
    
    # 3. Rãnh chỉ vàng bên trong
    inner_r = r - 12
    draw.ellipse([center - inner_r, center - inner_r, center + inner_r, center + inner_r], 
                 outline=inner_ring_color + (200,), width=3)
    
    # 4. 8 Cung ngọc tròn xung quanh viền
    for i in range(8):
        angle = math.radians(i * 45)
        px = center + (r - 6) * math.cos(angle)
        py = center + (r - 6) * math.sin(angle)
        draw.ellipse([px - 4, py - 4, px + 4, py + 4], fill=glow_color + (255,))
    
    # 5. Gradient lòng nút
    core_r = inner_r - 8
    for ring in range(int(core_r), 0, -2):
        factor = ring / core_r
        cr = int(bg_color[0] * 0.4 + bg_color[0] * 0.6 * (1.0 - factor * 0.4))
        cg = int(bg_color[1] * 0.4 + bg_color[1] * 0.6 * (1.0 - factor * 0.4))
        cb = int(bg_color[2] * 0.4 + bg_color[2] * 0.6 * (1.0 - factor * 0.4))
        draw.ellipse([center - ring, center - ring, center + ring, center + ring], 
                     fill=(cr, cg, cb, 255))
        
    # Specular rim
    draw.arc([center - core_r + 4, center - core_r + 4, center + core_r - 4, center + core_r - 4], 
             start=210, end=330, fill=(255, 255, 255, 160), width=4)
    
    img.save(output_path, "PNG")
    write_unity_meta(output_path, border=(0, 0, 0, 0), pivot=(0.5, 0.5))
    print(f"Generated Action Button Base: {output_path}")

def generate_circle_mask(output_path, size=256):
    """
    Sinh Texture hình tròn hoàn hảo (256x256) màu trắng nguyên bản để làm Sprite Mask cho Cooldown Fill (Radial 360).
    Giúp Unity UI Image khi bật Image.Type.Filled không bị văng ra thành hình vuông.
    """
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    center = size / 2.0
    r = (size / 2.0) - 2.0
    draw.ellipse([center - r, center - r, center + r, center + r], fill=(255, 255, 255, 255))
    img.save(output_path, "PNG")
    write_unity_meta(output_path, border=(0, 0, 0, 0), pivot=(0.5, 0.5))
    print(f"Generated Circle Mask: {output_path}")

def generate_signature_skill_button(output_path):
    """
    Sinh Nút Kỹ Năng Trấn Phái Bát Giác Cổ Phong 256x256:
    - Viền ngoài Bát Giác kim loại đồng vàng Đông Sơn chạm khắc phù chú
    - Nền giấy dó mực nho huyền bí
    - Biểu tượng Bút Phán Quan phát quang ở tâm
    """
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    center = size / 2.0
    r = 112.0
    
    # 1. Tọa độ hình bát giác
    oct_pts = []
    for i in range(8):
        angle = math.radians(i * 45 + 22.5)
        oct_pts.append((center + r * math.cos(angle), center + r * math.sin(angle)))
    
    # 2. Outer Glow
    glow_img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow_img)
    glow_draw.polygon(oct_pts, outline=(255, 215, 0, 160), fill=(255, 215, 0, 40))
    glow_img = glow_img.filter(ImageFilter.GaussianBlur(radius=8))
    img = Image.alpha_composite(img, glow_img)
    draw = ImageDraw.Draw(img)
    
    # 3. Thân bát giác kim loại
    draw.polygon(oct_pts, fill=(24, 20, 28, 245), outline=(201, 168, 106, 255))
    
    # 4. Rãnh chỉ vàng bên trong
    inner_oct_pts = []
    for i in range(8):
        angle = math.radians(i * 45 + 22.5)
        inner_oct_pts.append((center + (r - 12) * math.cos(angle), center + (r - 12) * math.sin(angle)))
    draw.polygon(inner_oct_pts, outline=(255, 215, 0, 220))
    
    # 5. Phù chú 8 cung bát quái ở 8 cạnh
    for i in range(8):
        angle = math.radians(i * 45)
        px = center + (r - 6) * math.cos(angle)
        py = center + (r - 6) * math.sin(angle)
        draw.ellipse([px - 4, py - 4, px + 4, py + 4], fill=(255, 215, 0, 255))
    
    # 6. Biểu tượng Bút Phán Quan phát quang ở tâm
    brush_img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    b_draw = ImageDraw.Draw(brush_img)
    
    # Thân bút lông
    b_draw.polygon([(center - 8, center + 45), (center + 8, center + 45), (center + 12, center - 10), (center - 12, center - 10)], 
                   fill=(180, 140, 70, 255), outline=(255, 215, 0, 255))
    # Đầu ngòi bút lông phát quang
    b_draw.polygon([(center - 12, center - 10), (center + 12, center - 10), (center, center - 55)], 
                   fill=(77, 238, 234, 255), outline=(255, 255, 255, 255))
    # Vệt mực năng lượng
    b_draw.ellipse([center - 6, center - 58, center + 6, center - 46], fill=(255, 255, 255, 255))
    
    img = Image.alpha_composite(img, brush_img)
    
    img.save(output_path, "PNG")
    write_unity_meta(output_path, border=(0, 0, 0, 0), pivot=(0.5, 0.5))
    print(f"Generated Signature Skill Button: {output_path}")

def generate_dash_button(output_path):
    """
    Sinh Nút Lướt Phi Vân Hài 256x256:
    - Vòng tròn đồng thau mây lướt
    - Nền ngọc xanh u linh
    - Biểu tượng tàn ảnh bước chân mây cuộn
    """
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    center = size / 2.0
    r = 108.0
    
    # 1. Glow
    glow_img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow_img)
    glow_draw.ellipse([center - r - 4, center - r - 4, center + r + 4, center + r + 4], 
                      fill=(77, 238, 234, 130))
    glow_img = glow_img.filter(ImageFilter.GaussianBlur(radius=8))
    img = Image.alpha_composite(img, glow_img)
    draw = ImageDraw.Draw(img)
    
    # 2. Vành kim loại
    draw.ellipse([center - r, center - r, center + r, center + r], 
                 fill=(20, 24, 32, 240), outline=(201, 168, 106, 255), width=5)
    
    # 3. Vòng tròn phụ
    draw.ellipse([center - r + 10, center - r + 10, center + r - 10, center + r - 10], 
                 outline=(77, 238, 234, 180), width=2)
    
    # 4. Biểu tượng Phi Vân Lướt (Cloud Dash)
    dash_img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d_draw = ImageDraw.Draw(dash_img)
    
    # 3 vệt gió tốc độ
    d_draw.line([(center - 45, center + 30), (center + 15, center - 30)], fill=(77, 238, 234, 220), width=6)
    d_draw.line([(center - 25, center + 45), (center + 35, center - 15)], fill=(255, 255, 255, 255), width=8)
    d_draw.line([(center - 5, center + 55), (center + 50, center)], fill=(77, 238, 234, 180), width=4)
    
    # Đám mây cuộn tàn ảnh
    d_draw.arc([center - 40, center - 20, center + 10, center + 30], start=120, end=330, fill=(255, 215, 0, 255), width=4)
    d_draw.arc([center + 5, center - 40, center + 45, center], start=180, end=360, fill=(255, 255, 255, 255), width=5)
    
    img = Image.alpha_composite(img, dash_img)
    
    img.save(output_path, "PNG")
    write_unity_meta(output_path, border=(0, 0, 0, 0), pivot=(0.5, 0.5))
    print(f"Generated Dash Button: {output_path}")

# -------------------------------------------------------------
# 8. KHUNG NỀN BANNER GÓC TRÊN (TOP LEFT & TOP RIGHT PANELS)
# -------------------------------------------------------------
def generate_top_scroll_frame(output_path):
    """
    Sinh Khung Cuộn Gỗ Mun Thau Cổ 9-Slice (512x256) cho Panel_TopLeft và Panel_TopRight:
    - Nền giấy dó nhuộm khói bán trong suốt (Alpha 65%)
    - Viền kim loại đồng thau chạm rãnh mây cuộn
    - Bo góc mềm mại, không che khuất tầm nhìn góc trên
    """
    w, h = 512, 256
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # 1. Glow viền ngoài nhẹ
    glow_img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow_img)
    glow_draw.rounded_rectangle([6, 6, w - 6, h - 6], radius=20, outline=(201, 168, 106, 80), width=4)
    glow_img = glow_img.filter(ImageFilter.GaussianBlur(radius=6))
    img = Image.alpha_composite(img, glow_img)
    draw = ImageDraw.Draw(img)
    
    # 2. Thân nền giấy dó khói tối bán trong suốt (Readability First)
    draw.rounded_rectangle([10, 10, w - 10, h - 10], radius=16, fill=(18, 16, 22, 175), outline=(201, 168, 106, 230), width=3)
    
    # 3. Viền trang trí rãnh chỉ vàng bên trong
    draw.rounded_rectangle([18, 18, w - 18, h - 18], radius=12, outline=(90, 77, 65, 140), width=2)
    
    # 4. Hoa văn góc triện đồng Đông Sơn 4 góc
    c_len = 20
    # Góc trên trái
    draw.line([(10, 10), (10 + c_len, 10)], fill=(255, 215, 0, 255), width=3)
    draw.line([(10, 10), (10, 10 + c_len)], fill=(255, 215, 0, 255), width=3)
    # Góc trên phải
    draw.line([(w - 10, 10), (w - 10 - c_len, 10)], fill=(255, 215, 0, 255), width=3)
    draw.line([(w - 10, 10), (w - 10, 10 + c_len)], fill=(255, 215, 0, 255), width=3)
    # Góc dưới trái
    draw.line([(10, h - 10), (10 + c_len, h - 10)], fill=(255, 215, 0, 255), width=3)
    draw.line([(10, h - 10), (10, h - 10 - c_len)], fill=(255, 215, 0, 255), width=3)
    # Góc dưới phải
    draw.line([(w - 10, h - 10), (w - 10 - c_len, h - 10)], fill=(255, 215, 0, 255), width=3)
    draw.line([(w - 10, h - 10), (w - 10, h - 10 - c_len)], fill=(255, 215, 0, 255), width=3)
    
    img.save(output_path, "PNG")
    write_unity_meta(output_path, border=(32, 32, 32, 32), pivot=(0.5, 0.5))
    print(f"Generated Top Scroll Frame: {output_path}")

# -------------------------------------------------------------
# 9. ICON THẺ NÂNG CẤP (VŨ KHÍ, NỘI CÔNG PASSIVES, TIẾN HÓA)
# -------------------------------------------------------------
def make_base_upgrade_icon(bg_color, rim_color):
    size = 128
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    center = size / 2.0
    r = 54.0
    
    glow_img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    g_draw = ImageDraw.Draw(glow_img)
    g_draw.ellipse([center - r - 4, center - r - 4, center + r + 4, center + r + 4], fill=rim_color + (130,))
    glow_img = glow_img.filter(ImageFilter.GaussianBlur(radius=4))
    img = Image.alpha_composite(img, glow_img)
    
    draw = ImageDraw.Draw(img)
    draw.ellipse([center - r, center - r, center + r, center + r], fill=bg_color + (240,), outline=rim_color + (255,), width=4)
    draw.ellipse([center - r + 8, center - r + 8, center + r - 8, center + r - 8], outline=(255, 255, 255, 120), width=2)
    return img, center

def generate_weapon_upgrade_icon(output_path, w_type):
    img, c = make_base_upgrade_icon((20, 24, 35), (77, 238, 234))
    draw = ImageDraw.Draw(img)
    
    if w_type == "crossbow": # Nỏ Thần
        draw.line([(c - 30, c + 10), (c + 30, c + 10)], fill=(255, 215, 0, 255), width=4)
        draw.arc([c - 30, c - 20, c + 30, c + 20], start=180, end=360, fill=(201, 168, 106, 255), width=5)
        draw.line([(c, c - 30), (c, c + 35)], fill=(77, 238, 234, 255), width=4)
        draw.polygon([(c, c - 35), (c + 8, c - 20), (c - 8, c - 20)], fill=(255, 255, 255, 255))
    elif w_type == "brush": # Bút Phán Quan
        draw.polygon([(c - 6, c + 35), (c + 6, c + 35), (c + 8, c - 10), (c - 8, c - 10)], fill=(180, 140, 70, 255))
        draw.polygon([(c - 8, c - 10), (c + 8, c - 10), (c, c - 35)], fill=(77, 238, 234, 255))
        draw.ellipse([c - 4, c - 38, c + 4, c - 30], fill=(255, 255, 255, 255))
    elif w_type == "talisman": # Bùa Trấn Yêu
        draw.rectangle([c - 20, c - 30, c + 20, c + 30], fill=(232, 196, 104, 255), outline=(184, 68, 44, 255), width=2)
        draw.line([(c, c - 20), (c, c + 20)], fill=(184, 68, 44, 255), width=3)
        draw.line([(c - 12, c - 10), (c + 12, c - 10)], fill=(184, 68, 44, 255), width=3)
    elif w_type == "drum": # Trống Đồng
        draw.ellipse([c - 28, c - 28, c + 28, c + 28], fill=(201, 168, 106, 255), outline=(255, 215, 0, 255), width=3)
        draw.ellipse([c - 18, c - 18, c + 18, c + 18], outline=(100, 70, 30, 255), width=2)
        draw.polygon([(c, c - 10), (c + 10, c), (c, c + 10), (c - 10, c)], fill=(255, 215, 0, 255))
    elif w_type == "sword": # Đao / Kiếm
        draw.line([(c - 25, c + 25), (c + 25, c - 25)], fill=(255, 255, 255, 255), width=6)
        draw.line([(c - 30, c + 30), (c - 20, c + 20)], fill=(201, 168, 106, 255), width=8)
        draw.polygon([(c + 25, c - 25), (c + 32, c - 32), (c + 18, c - 25)], fill=(255, 215, 0, 255))
    else: # Phi Tiêu / Pháp Bảo khác
        draw.polygon([(c, c - 30), (c + 30, c), (c, c + 30), (c - 30, c)], fill=(77, 238, 234, 255), outline=(255, 215, 0, 255), width=3)
        draw.ellipse([c - 8, c - 8, c + 8, c + 8], fill=(255, 255, 255, 255))
        
    img.save(output_path, "PNG")
    write_unity_meta(output_path, border=(0, 0, 0, 0), pivot=(0.5, 0.5))
    print(f"Generated Upgrade Icon: {output_path}")

def generate_passive_upgrade_icon(output_path, p_type):
    img, c = make_base_upgrade_icon((25, 20, 30), (255, 215, 0))
    draw = ImageDraw.Draw(img)
    
    if p_type == "damage": # Sát thương
        draw.polygon([(c, c - 35), (c + 25, c + 25), (c, c + 10), (c - 25, c + 25)], fill=(235, 60, 60, 255), outline=(255, 215, 0, 255), width=2)
    elif p_type == "health": # Hồi máu / Chuông
        draw.polygon([(c - 20, c + 15), (c + 20, c + 15), (c + 12, c - 20), (c - 12, c - 20)], fill=(76, 122, 61, 255), outline=(255, 215, 0, 255), width=3)
        draw.ellipse([c - 6, c + 18, c + 6, c + 26], fill=(255, 215, 0, 255))
    elif p_type == "speed": # Tốc độ
        draw.arc([c - 25, c - 25, c + 25, c + 25], start=120, end=330, fill=(77, 238, 234, 255), width=5)
        draw.line([(c - 20, c + 10), (c + 25, c - 15)], fill=(255, 255, 255, 255), width=6)
    elif p_type == "armor": # Giáp
        draw.polygon([(c, c - 30), (c + 25, c - 15), (c + 20, c + 20), (c, c + 35), (c - 20, c + 20), (c - 25, c - 15)], fill=(140, 98, 57, 255), outline=(255, 215, 0, 255), width=3)
    elif p_type == "magnet": # Túi hút hồn
        draw.ellipse([c - 20, c - 25, c + 20, c + 25], fill=(46, 110, 158, 255), outline=(77, 238, 234, 255), width=3)
        draw.line([(c - 15, c - 5), (c + 15, c - 5)], fill=(255, 255, 255, 255), width=4)
    else: # May mắn / Ngọc
        draw.polygon([(c, c - 28), (c + 20, c - 10), (c + 20, c + 15), (c, c + 28), (c - 20, c + 15), (c - 20, c - 10)], fill=(76, 122, 61, 255), outline=(255, 255, 255, 255), width=3)
        
    img.save(output_path, "PNG")
    write_unity_meta(output_path, border=(0, 0, 0, 0), pivot=(0.5, 0.5))
    print(f"Generated Passive Icon: {output_path}")

def generate_evolution_upgrade_icon(output_path):
    img, c = make_base_upgrade_icon((35, 25, 15), (255, 215, 0))
    draw = ImageDraw.Draw(img)
    # Hào quang Bát Quái / Kim Long Thần Thoại
    for i in range(8):
        ang = math.radians(i * 45)
        p1 = (c + 25 * math.cos(ang), c + 25 * math.sin(ang))
        p2 = (c + 42 * math.cos(ang), c + 42 * math.sin(ang))
        draw.line([p1, p2], fill=(255, 215, 0, 255), width=4)
    draw.ellipse([c - 22, c - 22, c + 22, c + 22], fill=(255, 245, 150, 255), outline=(201, 168, 106, 255), width=3)
    draw.polygon([(c, c - 14), (c + 14, c), (c, c + 14), (c - 14, c)], fill=(184, 68, 44, 255))
    
    img.save(output_path, "PNG")
    write_unity_meta(output_path, border=(0, 0, 0, 0), pivot=(0.5, 0.5))
    print(f"Generated Evolution Icon: {output_path}")

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
    
    # 3. HUD Bars & YinYang Board & Weapon Slots & Handles
    generate_hud_bar_frame(os.path.join(HUD_DIR, "Frame_HUD_Bar.png"))
    generate_hud_bar_fill(os.path.join(HUD_DIR, "Fill_HP_ChuSa.png"), 
                          color_top=(235, 60, 60), color_bot=(140, 20, 20), glow_line=(255, 160, 160))
    generate_hud_bar_fill(os.path.join(HUD_DIR, "Fill_EXP_LamNgoc.png"), 
                          color_top=(60, 240, 230), color_bot=(15, 110, 120), glow_line=(180, 255, 250))
    generate_yinyang_fill(os.path.join(HUD_DIR, "Fill_YinYang_Taiji.png"))
    generate_slider_handle(os.path.join(HUD_DIR, "Handle_LinhChau_DongThau.png"))
    generate_yinyang_board_frame(os.path.join(HUD_DIR, "Frame_YinYang_Taiji.png"))
    generate_weapon_slot_frame(os.path.join(HUD_DIR, "Slot_Weapon_Equipped.png"))
    generate_top_scroll_frame(os.path.join(HUD_DIR, "Frame_Panel_Top_Scroll.png"))
    
    # 4. Buttons (Menu & Action Controls)
    generate_primary_button(os.path.join(BUTTONS_DIR, "Btn_Primary_Normal.png"), normal=True)
    generate_primary_button(os.path.join(BUTTONS_DIR, "Btn_Primary_Pressed.png"), normal=False)
    
    # Bộ Action Circle Buttons cho Mobile Controls
    # 4a. Nút Đánh Thường (Đỏ Chu Sa)
    generate_action_button_base(os.path.join(BUTTONS_DIR, "Btn_Circle_Attack.png"), 
                                border_color=(255, 215, 0), glow_color=(255, 80, 80), bg_color=(140, 25, 25), inner_ring_color=(255, 180, 100))
    generate_action_button_base(os.path.join(BUTTONS_DIR, "Btn_Circle_Attack_Pressed.png"), 
                                border_color=(201, 168, 106), glow_color=(200, 50, 50), bg_color=(90, 15, 15), inner_ring_color=(200, 120, 60), is_pressed=True)
    
    # 4b. Nút Pháp Bảo / Relic Skill (Hắc Thạch Lam Ngọc)
    generate_action_button_base(os.path.join(BUTTONS_DIR, "Btn_Circle_Relic.png"), 
                                border_color=(201, 168, 106), glow_color=(77, 238, 234), bg_color=(25, 32, 42), inner_ring_color=(77, 238, 234))
    generate_action_button_base(os.path.join(BUTTONS_DIR, "Btn_Circle_Relic_Pressed.png"), 
                                border_color=(160, 130, 80), glow_color=(50, 180, 180), bg_color=(15, 20, 28), inner_ring_color=(50, 180, 180), is_pressed=True)

    # 4c. Nút Tuyệt Kỹ Nền (Tím Khói Mực)
    generate_action_button_base(os.path.join(BUTTONS_DIR, "Btn_Circle_Skill_Base.png"), 
                                border_color=(255, 215, 0), glow_color=(200, 100, 255), bg_color=(35, 22, 45), inner_ring_color=(255, 215, 0))
    generate_action_button_base(os.path.join(BUTTONS_DIR, "Btn_Circle_Skill_Base_Pressed.png"), 
                                border_color=(201, 168, 106), glow_color=(160, 70, 200), bg_color=(20, 12, 30), inner_ring_color=(201, 168, 106), is_pressed=True)

    # 4d. Nút Lướt Nền (Lam Bạc Phi Vân)
    generate_action_button_base(os.path.join(BUTTONS_DIR, "Btn_Circle_Dash_Base.png"), 
                                border_color=(201, 168, 106), glow_color=(120, 200, 255), bg_color=(20, 28, 40), inner_ring_color=(77, 238, 234))
    generate_action_button_base(os.path.join(BUTTONS_DIR, "Btn_Circle_Dash_Base_Pressed.png"), 
                                border_color=(160, 130, 80), glow_color=(80, 150, 200), bg_color=(12, 18, 28), inner_ring_color=(50, 160, 160), is_pressed=True)
    
    # 4e. Circle Mask cho Cooldown Radial Fill
    generate_circle_mask(os.path.join(BUTTONS_DIR, "Mask_Circle_Solid.png"))
    
    # 5. Bộ 5 Linh Châu Ngũ Hành
    generate_element_badge(os.path.join(BADGES_DIR, "Badge_Element_Kim.png"), 
                           element_type="Kim", main_color=(232, 196, 104), glow_color=(255, 243, 196))
    generate_element_badge(os.path.join(BADGES_DIR, "Badge_Element_Moc.png"), 
                           element_type="Moc", main_color=(76, 122, 61), glow_color=(143, 201, 122))
    generate_element_badge(os.path.join(BADGES_DIR, "Badge_Element_Thuy.png"), 
                           element_type="Thuy", main_color=(46, 110, 158), glow_color=(127, 203, 234))
    generate_element_badge(os.path.join(BADGES_DIR, "Badge_Element_Hoa.png"), 
                           element_type="Hoa", main_color=(184, 68, 44), glow_color=(255, 138, 80))
    generate_element_badge(os.path.join(BADGES_DIR, "Badge_Element_Tho.png"), 
                           element_type="Tho", main_color=(140, 98, 57), glow_color=(215, 168, 122))
    
    # 6. Nút Kỹ Năng Trấn Phái & Nút Lướt
    generate_signature_skill_button(os.path.join(SKILLS_DIR, "Btn_Signature_Skill_PhanQuan.png"))
    generate_dash_button(os.path.join(SKILLS_DIR, "Btn_Dash_PhiVan.png"))
    
    # 7. Bộ Icon Thẻ Nâng Cấp (Upgrades)
    generate_weapon_upgrade_icon(os.path.join(UPGRADE_ICONS_DIR, "Icon_W001_NoThan.png"), "crossbow")
    generate_weapon_upgrade_icon(os.path.join(UPGRADE_ICONS_DIR, "Icon_W002_ButPhanQuan.png"), "brush")
    generate_weapon_upgrade_icon(os.path.join(UPGRADE_ICONS_DIR, "Icon_W003_BuaTranYeu.png"), "talisman")
    generate_weapon_upgrade_icon(os.path.join(UPGRADE_ICONS_DIR, "Icon_W005_TrongDong.png"), "drum")
    generate_weapon_upgrade_icon(os.path.join(UPGRADE_ICONS_DIR, "Icon_W008_DaoCuuVi.png"), "sword")
    generate_weapon_upgrade_icon(os.path.join(UPGRADE_ICONS_DIR, "Icon_W012_PhiTieuBatQuai.png"), "dart")
    
    generate_passive_upgrade_icon(os.path.join(UPGRADE_ICONS_DIR, "Icon_P001_Damage.png"), "damage")
    generate_passive_upgrade_icon(os.path.join(UPGRADE_ICONS_DIR, "Icon_P003_Health.png"), "health")
    generate_passive_upgrade_icon(os.path.join(UPGRADE_ICONS_DIR, "Icon_P006_Speed.png"), "speed")
    generate_passive_upgrade_icon(os.path.join(UPGRADE_ICONS_DIR, "Icon_P007_Armor.png"), "armor")
    generate_passive_upgrade_icon(os.path.join(UPGRADE_ICONS_DIR, "Icon_P010_Magnet.png"), "magnet")
    generate_passive_upgrade_icon(os.path.join(UPGRADE_ICONS_DIR, "Icon_P012_Luck.png"), "luck")
    
    generate_evolution_upgrade_icon(os.path.join(UPGRADE_ICONS_DIR, "Icon_Evolutions_General.png"))
    
    print("=== UI ASSETS & 9-SLICE METAS GENERATED SUCCESSFULLY ===")
