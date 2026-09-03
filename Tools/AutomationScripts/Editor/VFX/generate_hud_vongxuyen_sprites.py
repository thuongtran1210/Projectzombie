import os
import math
import uuid
from PIL import Image, ImageDraw

DEST_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\VongXuyen"
os.makedirs(DEST_DIR, exist_ok=True)

def write_unity_meta(filepath, border=(0, 0, 0, 0), pivot=(0.5, 0.5)):
    meta_path = filepath + ".meta"
    guid = uuid.uuid4().hex
    bx, by, bz, bw = border
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
    forceMaximumQuality_BC6H_BC7: 0
"""
    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(meta_content)


def generate_player_orb_level():
    """1. Khung Tròn Gỗ Mun Đính 4 Mũi La Bàn Hiển Thị Cấp Độ (Lv.1)"""
    W, H = 140, 140
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    mid = W / 2

    # 4 Mũi La Bàn / Chóp Đồng Khảm 4 Hướng
    pts_n = [(mid, 4), (mid - 8, 22), (mid + 8, 22)]
    pts_s = [(mid, H - 4), (mid - 8, H - 22), (mid + 8, H - 22)]
    pts_w = [(4, mid), (22, mid - 8), (22, mid + 8)]
    pts_e = [(W - 4, mid), (W - 22, mid - 8), (W - 22, mid + 8)]

    for pts in [pts_n, pts_s, pts_w, pts_e]:
        draw.polygon(pts, fill=(225, 175, 75, 255), outline=(28, 18, 12, 255), width=2)

    # Khung Tròn Gỗ Mun Ngoài Cùng
    draw.ellipse([14, 14, W - 14, H - 14], fill=(62, 40, 26, 255), outline=(28, 18, 12, 255), width=4)
    # Vòng hoa văn rãnh xoắn
    draw.ellipse([22, 22, W - 22, H - 22], outline=(135, 95, 60, 255), width=3)
    # Lòng trong gỗ mun tối màu
    draw.ellipse([28, 28, W - 28, H - 28], fill=(42, 26, 18, 255), outline=(175, 130, 75, 255), width=2)

    path = os.path.join(DEST_DIR, "Frame_HUD_Player_Orb_Level.png")
    out.save(path, "PNG")
    write_unity_meta(path)
    print("Generated Frame_HUD_Player_Orb_Level.png")


def generate_bar_frame_9slice():
    """2. Khung Thanh Máu / Exp Vát Góc Nhọn Đuôi 9-Slice"""
    W, H = 220, 36
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    cut = 12
    # Đa giác vát góc nhọn bên phải
    poly_outer = [
        (2, 2), (W - cut, 2),
        (W - 2, H / 2),
        (W - cut, H - 2), (2, H - 2)
    ]
    draw.polygon(poly_outer, fill=(28, 18, 12, 255))

    poly_wood = [
        (4, 4), (W - cut - 2, 4),
        (W - 4, H / 2),
        (W - cut - 2, H - 4), (4, H - 4)
    ]
    draw.polygon(poly_wood, fill=(62, 40, 26, 255), outline=(135, 95, 60, 255), width=2)

    # Lòng trong màu đen khoét rãnh
    poly_inner = [
        (6, 6), (W - cut - 4, 6),
        (W - 7, H / 2),
        (W - cut - 4, H - 6), (6, H - 6)
    ]
    draw.polygon(poly_inner, fill=(18, 12, 10, 255))

    path = os.path.join(DEST_DIR, "Bar_HUD_Frame_VongXuyen_9Slice.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(10, 10, 20, 10))
    print("Generated Bar_HUD_Frame_VongXuyen_9Slice.png")


def generate_bar_fills():
    """3. Ruột Gradient Thanh Máu Đỏ (HP) và Thanh Exp Xanh Ngọc (EXP)"""
    # HP Fill (Đỏ Tươi Sang Đỏ Đậm)
    W, H = 180, 24
    out_hp = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw_hp = ImageDraw.Draw(out_hp)
    draw_hp.rounded_rectangle([1, 1, W - 1, H - 1], radius=4, fill=(225, 45, 45, 255), outline=(255, 120, 100, 255), width=1)
    draw_hp.rectangle([2, 2, W - 2, 6], fill=(255, 140, 120, 180)) # Highlight trên mép
    path_hp = os.path.join(DEST_DIR, "Bar_HUD_Fill_HP.png")
    out_hp.save(path_hp, "PNG")
    write_unity_meta(path_hp, border=(6, 6, 6, 6))

    # EXP Fill (Xanh Ngọc Dạ Quang)
    out_exp = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw_exp = ImageDraw.Draw(out_exp)
    draw_exp.rounded_rectangle([1, 1, W - 1, H - 1], radius=4, fill=(35, 175, 130, 255), outline=(75, 235, 180, 255), width=1)
    draw_exp.rectangle([2, 2, W - 2, 6], fill=(120, 255, 210, 180)) # Highlight trên mép
    path_exp = os.path.join(DEST_DIR, "Bar_HUD_Fill_EXP.png")
    out_exp.save(path_exp, "PNG")
    write_unity_meta(path_exp, border=(6, 6, 6, 6))
    print("Generated Bar Fills (HP & EXP)")


def generate_yin_yang_meter_hud():
    """4. Cán Cân Âm Dương Thái Cực (Panel Trung Tâm Giữa Màn Hình)"""
    W, H = 340, 90
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    # Thân Khung Giấy Da Cổ & Viền Vàng Uốn Lượn 2 Đầu
    draw.rounded_rectangle([40, 8, W - 6, H - 12], radius=16, fill=(28, 18, 12, 255))
    draw.rounded_rectangle([44, 12, W - 10, H - 16], radius=14, fill=(215, 185, 140, 255), outline=(155, 115, 65, 255), width=3)
    draw.rounded_rectangle([48, 16, W - 14, H - 20], radius=12, fill=(238, 220, 185, 255))

    # Vòng Tròn Khung Bát Quái / Thái Cực Bên Trái
    draw.ellipse([6, 6, 84, 84], fill=(62, 40, 26, 255), outline=(28, 18, 12, 255), width=4)
    draw.ellipse([12, 12, 78, 78], outline=(185, 140, 75, 255), width=3)
    draw.ellipse([16, 16, 74, 74], fill=(240, 230, 210, 255)) # Nền Dương (Trắng Ngà)

    # Vẽ Nửa Vòng Tròn Âm (Đen)
    draw.pieslice([16, 16, 74, 74], 90, 270, fill=(32, 28, 30, 255))
    # 2 Vòng tròn uốn Thái Cực S-curve
    draw.ellipse([29, 16, 61, 48], fill=(32, 28, 30, 255))
    draw.ellipse([29, 42, 61, 74], fill=(240, 230, 210, 255))
    # 2 Mắt Thái Cực
    draw.ellipse([42, 28, 48, 34], fill=(240, 230, 210, 255))
    draw.ellipse([42, 54, 48, 60], fill=(32, 28, 30, 255))

    path = os.path.join(DEST_DIR, "Panel_YinYang_Meter_HUD.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(90, 20, 30, 20))
    print("Generated Panel_YinYang_Meter_HUD.png")


def generate_top_right_timer_box():
    """5. Khung Gỗ Mun Khắc Hoa Văn Mây Góc Vàng (Thời Gian & Số Quái Diệt)"""
    W, H = 200, 96
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    # Khung gỗ mun dày
    draw.rounded_rectangle([2, 2, W - 2, H - 2], radius=12, fill=(28, 18, 12, 255))
    draw.rounded_rectangle([5, 5, W - 5, H - 5], radius=10, fill=(72, 45, 28, 255))
    draw.rounded_rectangle([9, 9, W - 9, H - 9], radius=8, fill=(45, 28, 18, 255), outline=(125, 85, 50, 255), width=2)

    # 4 Góc Khảm Hoa Văn Mây Đồng Vàng Cổ
    cw = 18
    # Top-Left
    draw.polygon([(9, 9), (9 + cw, 9), (9, 9 + cw)], fill=(225, 175, 75, 255))
    # Top-Right
    draw.polygon([(W - 9, 9), (W - 9 - cw, 9), (W - 9, 9 + cw)], fill=(225, 175, 75, 255))
    # Bottom-Left
    draw.polygon([(9, H - 9), (9 + cw, H - 9), (9, H - 9 - cw)], fill=(225, 175, 75, 255))
    # Bottom-Right
    draw.polygon([(W - 9, H - 9), (W - 9 - cw, H - 9), (W - 9, H - 9 - cw)], fill=(225, 175, 75, 255))

    path = os.path.join(DEST_DIR, "Frame_HUD_Timer_Kill_Wood.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(24, 24, 24, 24))
    print("Generated Frame_HUD_Timer_Kill_Wood.png")


def main():
    print("=== Generating In-Game HUD Vong Xuyen Vector Sprites ===")
    generate_player_orb_level()
    generate_bar_frame_9slice()
    generate_bar_fills()
    generate_yin_yang_meter_hud()
    generate_top_right_timer_box()
    print("=== Done Generating HUD Sprites! ===")

if __name__ == "__main__":
    main()
