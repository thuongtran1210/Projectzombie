import os
import math
import uuid
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

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


def generate_ribbon_banner_amber():
    """1. Băng Rôn Đỏ Cam Viền Vàng 3D Uốn Lượn 2 Đầu (Banner: CHỌN ANH HÙNG XUẤT TRẬN)"""
    W, H = 480, 90
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    # 2 Đầu Băng Rôn Đuôi Nheo Phía Sau
    # Đuôi trái
    poly_left_tail = [(12, 30), (70, 18), (65, 78), (8, 68), (30, 48)]
    draw.polygon(poly_left_tail, fill=(145, 42, 22, 255), outline=(28, 16, 12, 255), width=3)
    # Đuôi phải
    poly_right_tail = [(W - 12, 30), (W - 70, 18), (W - 65, 78), (W - 8, 68), (W - 30, 48)]
    draw.polygon(poly_right_tail, fill=(145, 42, 22, 255), outline=(28, 16, 12, 255), width=3)

    # Thân Băng Rôn Chính Diện (Uốn lượn hình vòng cung)
    body_poly = [
        (55, 12), (W - 55, 12),
        (W - 40, 72), (40, 72)
    ]
    # Viền đen dày
    draw.polygon([(52, 9), (W - 52, 9), (W - 37, 75), (37, 75)], fill=(28, 16, 12, 255))
    # Nền Đỏ Cam Hổ Phách Rực Lửa
    draw.polygon(body_poly, fill=(205, 68, 32, 255))
    # Vành sáng trên mặt băng rôn
    draw.polygon([(58, 15), (W - 58, 15), (W - 46, 42), (46, 42)], fill=(235, 105, 48, 255))
    # Viền Vàng Hoàng Kim Chạy Dọc Mép
    draw.polygon(body_poly, outline=(255, 215, 110, 255), width=3)

    path = os.path.join(DEST_DIR, "Banner_Ribbon_Hero_Amber.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(80, 20, 80, 20))
    print("Generated Banner_Ribbon_Hero_Amber.png")


def generate_totem_avatar_frame():
    """2. Khung Tròn Gỗ Mun Chạm Totem Sừng Thú & Dây Leo Cổ Trang (Cột Trái Avatar Tướng)"""
    W, H = 320, 320
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    mid = W / 2

    # 1. Cặp Sừng Trâu Cổ Thú 2 Bên
    # Sừng trái
    draw.polygon([(40, 90), (12, 50), (35, 30), (75, 70)], fill=(195, 175, 145, 255), outline=(32, 20, 14, 255), width=3)
    # Sừng phải
    draw.polygon([(W - 40, 90), (W - 12, 50), (W - 35, 30), (W - 75, 70)], fill=(195, 175, 145, 255), outline=(32, 20, 14, 255), width=3)

    # 2. Vòng Tròn Gỗ Mun Dày Ngoài Cùng
    draw.ellipse([30, 30, W - 30, H - 30], fill=(68, 44, 30, 255), outline=(28, 18, 12, 255), width=6)
    
    # Rãnh hoa văn dây leo bện đan xoắn
    draw.ellipse([44, 44, W - 44, H - 44], outline=(125, 88, 58, 255), width=4)

    # Vòng trong bọc đồng cổ
    draw.ellipse([58, 58, W - 58, H - 58], fill=(45, 28, 20, 255), outline=(175, 130, 70, 255), width=3)

    # 3. Lòng tròn tối màu chứa Avatar Nhân Vật
    draw.ellipse([68, 68, W - 68, H - 68], fill=(22, 16, 14, 255))

    # 4 Hạt Ngọc Hổ Phách Khảm 4 Góc (Đông Tây Nam Bắc)
    for px, py in [(mid, 36), (mid, H - 36), (36, mid), (W - 36, mid)]:
        draw.ellipse([px - 6, py - 6, px + 6, py + 6], fill=(235, 150, 30, 255), outline=(255, 220, 120, 255), width=2)

    path = os.path.join(DEST_DIR, "Frame_Avatar_Totem_Wood.png")
    out.save(path, "PNG")
    write_unity_meta(path)
    print("Generated Frame_Avatar_Totem_Wood.png")


def generate_btn_nav_arrow():
    """3. Nút Chuyển Tướng Gỗ 2 Đầu Vát Góc Khảm Hổ Phách (< và >) 9-Slice"""
    W, H = 100, 52
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    cut = 14
    poly_outer = [
        (cut, 2), (W - cut, 2),
        (W - 2, H / 2),
        (W - cut, H - 2), (cut, H - 2),
        (2, H / 2)
    ]
    draw.polygon(poly_outer, fill=(28, 16, 12, 255))

    poly_wood = [
        (cut + 2, 4), (W - cut - 2, 4),
        (W - 4, H / 2),
        (W - cut - 2, H - 4), (cut + 2, H - 4),
        (4, H / 2)
    ]
    draw.polygon(poly_wood, fill=(78, 48, 28, 255))

    poly_inner = [
        (cut + 4, 7), (W - cut - 4, 7),
        (W - 8, H / 2),
        (W - cut - 4, H - 7), (cut + 4, H - 7),
        (8, H / 2)
    ]
    draw.polygon(poly_inner, fill=(52, 32, 20, 255), outline=(160, 115, 65, 255), width=2)

    path = os.path.join(DEST_DIR, "Btn_Nav_Arrow_Hex_Wood.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(20, 15, 20, 15))
    print("Generated Btn_Nav_Arrow_Hex_Wood.png")


def generate_skill_box_wood():
    """4. Khung Kỹ Năng Gỗ Vuông Vát Đỉnh Chứa Icon Kỹ Năng (Signature & Passive Skill) 9-Slice"""
    W, H = 84, 84
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    draw.rounded_rectangle([2, 2, W - 2, H - 2], radius=10, fill=(28, 16, 12, 255))
    draw.rounded_rectangle([4, 4, W - 4, H - 4], radius=8, fill=(72, 46, 30, 255))
    draw.rounded_rectangle([8, 8, W - 8, H - 8], radius=6, fill=(35, 22, 16, 255), outline=(150, 110, 65, 255), width=2)
    
    # Chóp đính hạt vàng trên đỉnh
    mid = W / 2
    draw.polygon([(mid, 3), (mid - 5, 10), (mid + 5, 10)], fill=(245, 195, 80, 255))

    path = os.path.join(DEST_DIR, "Box_Skill_Icon_Wood_9Slice.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(16, 16, 16, 16))
    print("Generated Box_Skill_Icon_Wood_9Slice.png")


def generate_hero_name_badge():
    """5. Biển Tên Tướng Gỗ Khắc Hoa Văn Cổ (Thư Sinh / Ẩn Sĩ / Đạo Sĩ)"""
    W, H = 220, 48
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    draw.rounded_rectangle([2, 2, W - 2, H - 2], radius=12, fill=(28, 16, 12, 255))
    draw.rounded_rectangle([4, 4, W - 4, H - 4], radius=10, fill=(62, 40, 26, 255))
    draw.rounded_rectangle([8, 8, W - 8, H - 8], radius=7, fill=(42, 26, 18, 255), outline=(145, 105, 65, 255), width=2)

    path = os.path.join(DEST_DIR, "Badge_Hero_Name_Wood.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(20, 16, 20, 16))
    print("Generated Badge_Hero_Name_Wood.png")


def main():
    print("=== Generating Character Select 2.5D Vector UI Sprites ===")
    generate_ribbon_banner_amber()
    generate_totem_avatar_frame()
    generate_btn_nav_arrow()
    generate_skill_box_wood()
    generate_hero_name_badge()
    print("=== Done Generating Character Select Sprites! ===")

if __name__ == "__main__":
    main()
