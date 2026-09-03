import os
import math
import uuid
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

UI_ROOT = "Assets/Art/UI"
FRAMES_DIR = os.path.join(UI_ROOT, "Frames")
BUTTONS_DIR = os.path.join(UI_ROOT, "Buttons")
BADGES_DIR = os.path.join(UI_ROOT, "Badges")
HUD_DIR = os.path.join(UI_ROOT, "HUD")

for d in [FRAMES_DIR, BUTTONS_DIR, BADGES_DIR, HUD_DIR]:
    os.makedirs(d, exist_ok=True)

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
    buildTarget: Standalone
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
    spriteID: 
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
    with open(meta_path, 'w', encoding='utf-8') as f:
        f.write(meta_content)

def create_panel_dongson():
    # 256x256, 9-slice border 48,48,48,48
    w, h = 256, 256
    img = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # 1. Dark aged dó paper background
    draw.rounded_rectangle([12, 12, w-13, h-13], radius=16, fill=(24, 21, 29, 245))
    
    # Inner parchment accent
    draw.rounded_rectangle([18, 18, w-19, h-19], radius=12, fill=(30, 27, 36, 235), outline=(55, 48, 62, 180), width=1)

    # 2. Bronze border double stroke
    draw.rounded_rectangle([12, 12, w-13, h-13], radius=16, outline=(201, 168, 106, 255), width=3)
    draw.rounded_rectangle([8, 8, w-9, h-9], radius=20, outline=(138, 108, 62, 160), width=1)
    draw.rounded_rectangle([16, 16, w-17, h-17], radius=12, outline=(245, 218, 140, 180), width=1)

    # 3. 4 Corners Dong Son Solar Star & Chim Lạc motifs
    corner_size = 32
    corners = [(12, 12), (w-12-corner_size, 12), (12, h-12-corner_size), (w-12-corner_size, h-12-corner_size)]
    for cx, cy in corners:
        scx, scy = cx + corner_size // 2, cy + corner_size // 2
        draw.ellipse([scx-6, scy-6, scx+6, scy+6], fill=(235, 195, 105, 255), outline=(130, 95, 45, 255))
        draw.ellipse([scx-3, scy-3, scx+3, scy+3], fill=(255, 240, 180, 255))
        for angle in range(0, 360, 45):
            rad = math.radians(angle)
            rx1 = scx + math.cos(rad) * 6
            ry1 = scy + math.sin(rad) * 6
            rx2 = scx + math.cos(rad) * 11
            ry2 = scy + math.sin(rad) * 11
            draw.line([(rx1, ry1), (rx2, ry2)], fill=(245, 210, 120, 255), width=2)

    # 4. Top/Bottom Center engraved accent
    mid_x = w // 2
    draw.line([(mid_x - 36, 12), (mid_x + 36, 12)], fill=(255, 225, 145, 255), width=3)
    draw.polygon([(mid_x, 6), (mid_x - 8, 12), (mid_x + 8, 12)], fill=(235, 195, 105, 255))
    
    draw.line([(mid_x - 36, h-13), (mid_x + 36, h-13)], fill=(255, 225, 145, 255), width=3)
    draw.polygon([(mid_x, h-7), (mid_x - 8, h-13), (mid_x + 8, h-13)], fill=(235, 195, 105, 255))

    out_path = os.path.join(FRAMES_DIR, "Panel_DongSon_GameOver.png")
    img.save(out_path)
    write_unity_meta(out_path, border=(48, 48, 48, 48))
    print("Created:", out_path)

def create_button_sonmai(is_pressed=False):
    # 192x64, 9-slice border 24,24,24,24
    w, h = 192, 64
    img = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    if not is_pressed:
        c_top = (195, 45, 45, 255)
        c_bot = (130, 20, 20, 255)
        c_border = (245, 215, 125, 255)
    else:
        c_top = (130, 20, 20, 255)
        c_bot = (90, 10, 10, 255)
        c_border = (205, 165, 85, 255)

    pad = 4
    for y in range(pad, h - pad):
        factor = (y - pad) / float(h - 2 * pad)
        r = int(c_top[0] * (1 - factor) + c_bot[0] * factor)
        g = int(c_top[1] * (1 - factor) + c_bot[1] * factor)
        b = int(c_top[2] * (1 - factor) + c_bot[2] * factor)
        draw.line([(pad + 6, y), (w - pad - 7, y)], fill=(r, g, b, 255))

    draw.rounded_rectangle([pad, pad, w - pad - 1, h - pad - 1], radius=8, outline=c_border, width=2)
    draw.rounded_rectangle([pad + 3, pad + 3, w - pad - 4, h - pad - 4], radius=6, outline=(255, 240, 170, 120 if not is_pressed else 60), width=1)

    studs = [(pad + 6, pad + 6), (w - pad - 7, pad + 6), (pad + 6, h - pad - 7), (w - pad - 7, h - pad - 7)]
    for sx, sy in studs:
        draw.ellipse([sx-2, sy-2, sx+2, sy+2], fill=(255, 235, 150, 255), outline=(120, 85, 30, 255))

    fname = "Btn_SonMai_ChuSa_Pressed.png" if is_pressed else "Btn_SonMai_ChuSa.png"
    out_path = os.path.join(BUTTONS_DIR, fname)
    img.save(out_path)
    write_unity_meta(out_path, border=(24, 24, 24, 24))
    print("Created:", out_path)

def create_button_gomun(is_pressed=False):
    # 192x64, 9-slice border 24,24,24,24
    w, h = 192, 64
    img = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    if not is_pressed:
        c_top = (52, 48, 60, 255)
        c_bot = (28, 25, 33, 255)
        c_border = (165, 158, 178, 255)
    else:
        c_top = (35, 32, 42, 255)
        c_bot = (18, 16, 22, 255)
        c_border = (120, 115, 130, 255)

    pad = 4
    for y in range(pad, h - pad):
        factor = (y - pad) / float(h - 2 * pad)
        r = int(c_top[0] * (1 - factor) + c_bot[0] * factor)
        g = int(c_top[1] * (1 - factor) + c_bot[1] * factor)
        b = int(c_top[2] * (1 - factor) + c_bot[2] * factor)
        draw.line([(pad + 6, y), (w - pad - 7, y)], fill=(r, g, b, 255))

    draw.rounded_rectangle([pad, pad, w - pad - 1, h - pad - 1], radius=8, outline=c_border, width=2)
    draw.rounded_rectangle([pad + 3, pad + 3, w - pad - 4, h - pad - 4], radius=6, outline=(220, 215, 235, 100 if not is_pressed else 40), width=1)

    studs = [(pad + 6, pad + 6), (w - pad - 7, pad + 6), (pad + 6, h - pad - 7), (w - pad - 7, h - pad - 7)]
    for sx, sy in studs:
        draw.ellipse([sx-2, sy-2, sx+2, sy+2], fill=(200, 195, 215, 255), outline=(70, 65, 80, 255))

    fname = "Btn_GoMun_Dark_Pressed.png" if is_pressed else "Btn_GoMun_Dark.png"
    out_path = os.path.join(BUTTONS_DIR, fname)
    img.save(out_path)
    write_unity_meta(out_path, border=(24, 24, 24, 24))
    print("Created:", out_path)

def create_cotien_coin():
    size = 128
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    cx, cy = size // 2, size // 2
    r_outer = 56
    r_inner = 16

    draw.ellipse([cx - r_outer, cy - r_outer, cx + r_outer, cy + r_outer], fill=(215, 175, 80, 255), outline=(130, 95, 35, 255), width=3)
    draw.ellipse([cx - r_outer + 4, cy - r_outer + 4, cx + r_outer - 4, cy + r_outer - 4], outline=(255, 235, 145, 220), width=2)

    for i in range(12):
        rad = math.radians(i * 30)
        px = cx + math.cos(rad) * (r_outer - 10)
        py = cy + math.sin(rad) * (r_outer - 10)
        draw.ellipse([px-2, py-2, px+2, py+2], fill=(255, 245, 180, 255))

    draw.rectangle([cx - r_inner, cy - r_inner, cx + r_inner, cy + r_inner], fill=(0, 0, 0, 0), outline=(130, 95, 35, 255), width=3)
    draw.rectangle([cx - r_inner + 2, cy - r_inner + 2, cx + r_inner - 2, cy + r_inner - 2], outline=(255, 235, 145, 200), width=1)

    gold_mark = (150, 110, 45, 255)
    draw.line([(cx, cy - r_outer + 12), (cx, cy - r_inner - 6)], fill=gold_mark, width=3)
    draw.line([(cx - 6, cy - r_outer + 18), (cx + 6, cy - r_outer + 18)], fill=gold_mark, width=2)
    draw.line([(cx, cy + r_inner + 6), (cx, cy + r_outer - 12)], fill=gold_mark, width=3)
    draw.line([(cx - 8, cy + r_outer - 18), (cx + 8, cy + r_outer - 18)], fill=gold_mark, width=2)
    draw.line([(cx - r_outer + 12, cy), (cx - r_inner - 6, cy)], fill=gold_mark, width=3)
    draw.line([(cx + r_inner + 6, cy), (cx + r_outer - 12, cy)], fill=gold_mark, width=3)

    out_path = os.path.join(BADGES_DIR, "Icon_CoTien_VongXuyen.png")
    img.save(out_path)
    write_unity_meta(out_path, border=(0, 0, 0, 0))
    print("Created:", out_path)

def create_stat_sub_card():
    # 256x72, 9-slice border 16,16,16,16
    w, h = 256, 72
    img = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Nền gỗ mun tối mờ
    pad = 2
    draw.rounded_rectangle([pad, pad, w - pad - 1, h - pad - 1], radius=8, fill=(28, 20, 16, 230), outline=(130, 95, 55, 255), width=2)
    draw.rounded_rectangle([pad + 3, pad + 3, w - pad - 4, h - pad - 4], radius=6, outline=(220, 180, 110, 80), width=1)

    # Đinh tán vàng đồng 4 góc
    studs = [(pad + 6, pad + 6), (w - pad - 7, pad + 6), (pad + 6, h - pad - 7), (w - pad - 7, h - pad - 7)]
    for sx, sy in studs:
        draw.ellipse([sx-2, sy-2, sx+2, sy+2], fill=(235, 195, 115, 255), outline=(75, 50, 25, 255))

    out_path = os.path.join(FRAMES_DIR, "Card_Stat_Sub_Bg.png")
    img.save(out_path)
    write_unity_meta(out_path, border=(16, 16, 16, 16))
    print("Created:", out_path)

def create_gameover_banner(is_victory=False):
    # 440x90, 9-slice border 32,32,32,32
    w, h = 440, 90
    img = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    c_top = (210, 175, 80, 255) if is_victory else (180, 35, 30, 255)
    c_bot = (140, 105, 35, 255) if is_victory else (100, 15, 15, 255)
    c_border = (255, 235, 150, 255) if is_victory else (240, 190, 100, 255)

    pad = 4
    for y in range(pad, h - pad):
        factor = (y - pad) / float(h - 2 * pad)
        r = int(c_top[0] * (1 - factor) + c_bot[0] * factor)
        g = int(c_top[1] * (1 - factor) + c_bot[1] * factor)
        b = int(c_top[2] * (1 - factor) + c_bot[2] * factor)
        draw.line([(pad + 8, y), (w - pad - 9, y)], fill=(r, g, b, 255))

    # Viền ngoài hoa văn
    draw.rounded_rectangle([pad, pad, w - pad - 1, h - pad - 1], radius=12, outline=c_border, width=3)
    draw.rounded_rectangle([pad + 4, pad + 4, w - pad - 5, h - pad - 5], radius=9, outline=(255, 255, 200, 140), width=1)

    # Chấm ngọc góc
    studs = [(pad + 8, pad + 8), (w - pad - 9, pad + 8), (pad + 8, h - pad - 9), (w - pad - 9, h - pad - 9)]
    for sx, sy in studs:
        draw.ellipse([sx-3, sy-3, sx+3, sy+3], fill=(255, 245, 180, 255), outline=(90, 60, 20, 255))

    fname = "Banner_GameOver_Victory.png" if is_victory else "Banner_GameOver_Defeat.png"
    out_path = os.path.join(BADGES_DIR, fname)
    img.save(out_path)
    write_unity_meta(out_path, border=(32, 32, 32, 32))
    print("Created:", out_path)

def create_stat_icons():
    # 1. Icon Thời Gian (Đồng hồ cát cổ)
    img_time = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    d_time = ImageDraw.Draw(img_time)
    d_time.polygon([(16, 12), (48, 12), (32, 32), (48, 52), (16, 52), (32, 32)], fill=(245, 215, 130, 255), outline=(120, 85, 30, 255))
    d_time.rectangle([14, 10, 50, 14], fill=(210, 165, 70, 255))
    d_time.rectangle([14, 50, 50, 54], fill=(210, 165, 70, 255))
    out_time = os.path.join(BADGES_DIR, "Icon_Stat_Time.png")
    img_time.save(out_time)
    write_unity_meta(out_time)

    # 2. Icon Diệt Quái (Đầu lâu ma / Sát quái)
    img_kill = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    d_kill = ImageDraw.Draw(img_kill)
    d_kill.ellipse([14, 12, 50, 44], fill=(235, 230, 240, 255), outline=(70, 55, 75, 255), width=2)
    d_kill.rectangle([22, 40, 42, 54], fill=(235, 230, 240, 255), outline=(70, 55, 75, 255), width=2)
    d_kill.ellipse([20, 26, 28, 36], fill=(50, 35, 55, 255))
    d_kill.ellipse([36, 26, 44, 36], fill=(50, 35, 55, 255))
    d_kill.polygon([(32, 36), (29, 42), (35, 42)], fill=(50, 35, 55, 255))
    d_kill.line([(27, 48), (27, 54)], fill=(70, 55, 75, 255), width=2)
    d_kill.line([(32, 48), (32, 54)], fill=(70, 55, 75, 255), width=2)
    d_kill.line([(37, 48), (37, 54)], fill=(70, 55, 75, 255), width=2)
    out_kill = os.path.join(BADGES_DIR, "Icon_Stat_Kill.png")
    img_kill.save(out_kill)
    write_unity_meta(out_kill)

    # 3. Icon Cấp Độ (Lệnh bài sao vàng)
    img_lvl = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    d_lvl = ImageDraw.Draw(img_lvl)
    d_lvl.polygon([(32, 10), (52, 22), (52, 48), (32, 56), (12, 48), (12, 22)], fill=(225, 175, 60, 255), outline=(110, 75, 20, 255), width=2)
    d_lvl.polygon([(32, 18), (36, 28), (47, 28), (38, 34), (41, 44), (32, 38), (23, 44), (26, 34), (17, 28), (28, 28)], fill=(255, 245, 180, 255), outline=(130, 90, 25, 255))
    out_lvl = os.path.join(BADGES_DIR, "Icon_Stat_Level.png")
    img_lvl.save(out_lvl)
    write_unity_meta(out_lvl)

    # 4. Icon Sát Thương (Kiếm kích song đao)
    img_dmg = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    d_dmg = ImageDraw.Draw(img_dmg)
    d_dmg.line([(14, 50), (46, 14)], fill=(230, 220, 240, 255), width=4)
    d_dmg.line([(18, 54), (50, 18)], fill=(200, 185, 215, 255), width=2)
    d_dmg.line([(50, 50), (18, 14)], fill=(240, 120, 60, 255), width=4)
    d_dmg.ellipse([28, 28, 36, 36], fill=(255, 230, 140, 255), outline=(130, 80, 20, 255))
    out_dmg = os.path.join(BADGES_DIR, "Icon_Stat_Damage.png")
    img_dmg.save(out_dmg)
    write_unity_meta(out_dmg)
    print("Created Stat Icons!")

if __name__ == '__main__':
    create_panel_dongson()
    create_button_sonmai(False)
    create_button_sonmai(True)
    create_button_gomun(False)
    create_button_gomun(True)
    create_cotien_coin()
    create_stat_sub_card()
    create_gameover_banner(False)
    create_gameover_banner(True)
    create_stat_icons()
    print("All Vietnamese Folklore UI Assets successfully generated!")
