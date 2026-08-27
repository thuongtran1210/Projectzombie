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
    forceMaximumCompressionQuality_BC6H_BC7: 0
"""
    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(meta_content)


def generate_header_bar():
    """Vẽ thanh gỗ mun đỉnh màn hình uốn lượn có tấm phù điêu chạm khắc mây núi ở giữa"""
    W, H = 1024, 88
    img = Image.open(r"C:\Users\thuon\.gemini\antigravity-ide\brain\a0caa716-0c54-40e4-a25a-54134669c143\.user_uploaded\media_1787827193679.png").convert("RGBA")
    # Vẽ đồ họa vector sạch bóng dựa trên tỷ lệ
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)
    
    # 1. Khung gỗ chính uốn cong đáy
    poly = [
        (0, 0), (W, 0), 
        (W, 42), (W - 80, 52), (W * 0.72, 54),
        (W * 0.68, 64), (W * 0.64, 82), (W * 0.36, 82), (W * 0.32, 64),
        (W * 0.28, 54), (80, 52), (0, 42)
    ]
    # Viền đen dày (Thick outline)
    draw.polygon(poly, fill=(28, 20, 16, 255))
    
    # Lớp gỗ nâu trầm bên trong
    inner_poly = [
        (4, 0), (W - 4, 0),
        (W - 4, 38), (W - 80, 48), (W * 0.72, 50),
        (W * 0.67, 60), (W * 0.63, 76), (W * 0.37, 76), (W * 0.33, 60),
        (W * 0.28, 50), (80, 48), (4, 38)
    ]
    draw.polygon(inner_poly, fill=(58, 38, 28, 255))

    # Tấm phù điêu chạm khắc mây núi ở giữa
    center_poly = [
        (W * 0.35, 8), (W * 0.65, 8),
        (W * 0.62, 70), (W * 0.38, 70)
    ]
    draw.polygon(center_poly, fill=(35, 24, 18, 255), outline=(130, 95, 60, 255), width=3)
    
    # Đường vân mây vàng đồng bên trong tấm phù điêu
    mid_x, mid_y = W / 2, 38
    draw.ellipse([mid_x - 18, mid_y - 18, mid_x + 18, mid_y + 18], outline=(180, 135, 75, 255), width=2)
    draw.arc([mid_x - 140, mid_y - 15, mid_x - 25, mid_y + 15], 0, 180, fill=(180, 135, 75, 255), width=2)
    draw.arc([mid_x + 25, mid_y - 15, mid_x + 140, mid_y + 15], 0, 180, fill=(180, 135, 75, 255), width=2)

    path = os.path.join(DEST_DIR, "Header_Wood_Bar_VongXuyen.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(120, 10, 120, 10))
    print("Generated Header_Wood_Bar_VongXuyen.png")


def generate_btn_nav():
    """Vẽ nút thẻ gỗ nâu viền chỉ khâu may du mục cổ trang 9-slice"""
    W, H = 160, 68
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    # 1. Viền ngoài đen dày Thick Outline bo góc
    draw.rounded_rectangle([2, 2, W - 2, H - 2], radius=10, fill=(28, 18, 14, 255))
    # 2. Khối gỗ 3D đáy (Shadow Bevel)
    draw.rounded_rectangle([4, 8, W - 4, H - 4], radius=8, fill=(45, 28, 20, 255))
    # 3. Mặt nút gỗ nổi
    draw.rounded_rectangle([4, 4, W - 4, H - 10], radius=8, fill=(78, 52, 38, 255))
    # 4. Viền chỉ khâu đan chéo / rãnh khắc gỗ
    draw.rounded_rectangle([10, 10, W - 10, H - 16], radius=5, outline=(130, 92, 65, 255), width=2)
    
    # 4 góc đan chỉ
    for x in (14, W - 14):
        for y in (14, H - 20):
            draw.line([(x - 3, y - 3), (x + 3, y + 3)], fill=(200, 160, 110, 255), width=2)
            draw.line([(x - 3, y + 3), (x + 3, y - 3)], fill=(200, 160, 110, 255), width=2)

    path = os.path.join(DEST_DIR, "Btn_Nav_Wood_Stitched.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(24, 24, 24, 24))
    print("Generated Btn_Nav_Wood_Stitched.png")


def generate_btn_battle():
    """Vẽ nút XUẤT TRẬN lục giác kéo dài nằm ngang bọc ngọc hổ phách cam rực rỡ và đính sao sáng"""
    W, H = 260, 100
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    cut = 32
    # Poly lục giác ngoài cùng (Viền đen dày)
    poly_outer = [
        (cut, 4), (W - cut, 4),
        (W - 4, H / 2),
        (W - cut, H - 4), (cut, H - 4),
        (4, H / 2)
    ]
    draw.polygon(poly_outer, fill=(28, 16, 10, 255))

    # Viền gỗ bọc bên ngoài
    poly_wood = [
        (cut + 3, 7), (W - cut - 3, 7),
        (W - 7, H / 2),
        (W - cut - 3, H - 7), (cut + 3, H - 7),
        (7, H / 2)
    ]
    draw.polygon(poly_wood, fill=(90, 48, 22, 255))

    # Lõi Ngọc Hổ Phách Cam Vàng Rực Lửa
    poly_amber = [
        (cut + 8, 14), (W - cut - 8, 14),
        (W - 14, H / 2),
        (W - cut - 8, H - 14), (cut + 8, H - 14),
        (14, H / 2)
    ]
    # Gradient ngọc từ cam sang đỏ cam
    draw.polygon(poly_amber, fill=(215, 95, 20, 255))

    # Lớp sáng bóng trên nửa mặt ngọc (Highlight)
    poly_shine = [
        (cut + 10, 16), (W - cut - 10, 16),
        (W - 20, H / 2 - 4),
        (20, H / 2 - 4)
    ]
    draw.polygon(poly_shine, fill=(255, 175, 45, 255))

    # Viền vàng kim loại bên trong
    draw.polygon(poly_amber, outline=(255, 215, 100, 255), width=3)

    # Đính Ngôi Sao 4 Cánh Kim Cương Trên Đỉnh Nút
    star_x, star_y = W / 2, 8
    star_poly = [
        (star_x, star_y - 8), (star_x + 6, star_y),
        (star_x, star_y + 8), (star_x - 6, star_y)
    ]
    draw.polygon(star_poly, fill=(255, 245, 180, 255), outline=(120, 70, 20, 255), width=2)
    draw.ellipse([star_x - 2, star_y - 2, star_x + 2, star_y + 2], fill=(255, 255, 255, 255))

    # Đính Ngôi Sao 4 Cánh Đáy Nút
    star_b_y = H - 8
    star_b_poly = [
        (star_x, star_b_y - 8), (star_x + 6, star_b_y),
        (star_x, star_b_y + 8), (star_x - 6, star_b_y)
    ]
    draw.polygon(star_b_poly, fill=(255, 245, 180, 255), outline=(120, 70, 20, 255), width=2)

    path = os.path.join(DEST_DIR, "Btn_Battle_Hex_Amber_Glow.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(45, 30, 45, 30))
    print("Generated Btn_Battle_Hex_Amber_Glow.png")


def generate_tray_loadout():
    """Vẽ khay gỗ mun góc dưới bên trái 9-slice bo góc viền gỗ dày"""
    W, H = 256, 140
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    # Viền đen dày ngoài cùng
    draw.rounded_rectangle([2, 2, W - 2, H - 2], radius=14, fill=(24, 16, 12, 255))
    # Viền gỗ mun dày dặn
    draw.rounded_rectangle([5, 5, W - 5, H - 5], radius=11, fill=(65, 42, 30, 255))
    # Mặt lõi nền tối bên trong để chứa 2 ô slot
    draw.rounded_rectangle([12, 12, W - 12, H - 12], radius=8, fill=(32, 22, 18, 255), outline=(95, 65, 45, 255), width=2)
    # Khắc góc cổ
    for x in (16, W - 16):
        for y in (16, H - 16):
            draw.ellipse([x - 2, y - 2, x + 2, y + 2], fill=(160, 120, 80, 255))

    path = os.path.join(DEST_DIR, "Tray_Loadout_Wood_Frame.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(32, 32, 32, 32))
    print("Generated Tray_Loadout_Wood_Frame.png")


def generate_currency_pill():
    """Vẽ viên thuốc tài nguyên gỗ bo tròn 9-slice"""
    W, H = 160, 48
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    draw.rounded_rectangle([2, 2, W - 2, H - 2], radius=22, fill=(26, 18, 14, 255))
    draw.rounded_rectangle([4, 4, W - 4, H - 4], radius=20, fill=(58, 38, 28, 255))
    draw.rounded_rectangle([8, 8, W - 8, H - 8], radius=16, fill=(35, 24, 18, 255), outline=(110, 80, 55, 255), width=2)

    path = os.path.join(DEST_DIR, "Pill_Currency_Wood.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(24, 16, 24, 16))
    print("Generated Pill_Currency_Wood.png")


def generate_pedestal_hexagon():
    """Vẽ Bục Đá Lục Giác 2.5D Cổ Khảm Viên Hổ Phách Giọt Nước"""
    W, H = 380, 220
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    # 1. Chân đế đá xám phong rêu dưới cùng
    base_poly = [
        (80, 120), (300, 120),
        (370, 165), (310, 210),
        (70, 210), (10, 165)
    ]
    draw.polygon(base_poly, fill=(60, 68, 65, 255), outline=(25, 30, 28, 255), width=4)

    # 2. Thân bục bọc gỗ mun khắc hoa văn
    mid_poly = [
        (85, 90), (295, 90),
        (355, 140), (295, 190),
        (85, 190), (25, 140)
    ]
    draw.polygon(mid_poly, fill=(75, 48, 32, 255), outline=(32, 20, 14, 255), width=4)

    # Mặt trước bục gỗ có ô chạm khắc
    front_poly = [(110, 120), (270, 120), (270, 180), (110, 180)]
    draw.polygon(front_poly, fill=(50, 32, 22, 255), outline=(130, 90, 55, 255), width=2)

    # Viên Ngọc Hổ Phách Giọt Nước Trung Tâm Bục
    gem_x, gem_y = 190, 150
    draw.ellipse([gem_x - 14, gem_y - 10, gem_x + 14, gem_y + 18], fill=(235, 140, 25, 255), outline=(255, 220, 120, 255), width=2)
    draw.polygon([(gem_x, gem_y - 20), (gem_x - 14, gem_y), (gem_x + 14, gem_y)], fill=(235, 140, 25, 255))
    draw.ellipse([gem_x - 4, gem_y - 4, gem_x + 4, gem_y + 4], fill=(255, 245, 180, 255))

    # 3. Mặt trên bục đá lục giác (Mặt sàn đứng của nhân vật)
    top_poly = [
        (110, 40), (270, 40),
        (340, 90), (270, 140),
        (110, 140), (40, 90)
    ]
    draw.polygon(top_poly, fill=(160, 125, 85, 255), outline=(40, 28, 20, 255), width=4)

    # Các vòng vân lục giác đồng tâm trên mặt bục
    top_inner = [
        (130, 55), (250, 55),
        (300, 90), (250, 125),
        (130, 125), (80, 90)
    ]
    draw.polygon(top_inner, fill=(140, 108, 72, 255), outline=(100, 75, 50, 255), width=2)

    path = os.path.join(DEST_DIR, "Pedestal_Hexagon_2_5D_WoodStone.png")
    out.save(path, "PNG")
    write_unity_meta(path, pivot=(0.5, 0.4))
    print("Generated Pedestal_Hexagon_2_5D_WoodStone.png")


def generate_settings_gear_button():
    """Vẽ Nút Bánh Răng Cài Đặt Gỗ Mun Bát Quái Cổ Phong 2.5D (64x64)"""
    W, H = 64, 64
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    cx, cy = W // 2, H // 2
    r_outer = 28
    r_inner = 20

    # Răng cưa bánh răng 8 cánh (Bát Quái Gỗ)
    for i in range(8):
        angle = i * (math.pi / 4)
        gx = cx + int((r_outer + 2) * math.cos(angle))
        gy = cy + int((r_outer + 2) * math.sin(angle))
        draw.ellipse([gx - 5, gy - 5, gx + 5, gy + 5], fill=(32, 20, 14, 255))
        draw.ellipse([gx - 3, gy - 3, gx + 3, gy + 3], fill=(195, 145, 65, 255))

    # Viền bánh răng chính
    draw.ellipse([cx - r_outer, cy - r_outer, cx + r_outer, cy + r_outer], fill=(32, 20, 14, 255))
    draw.ellipse([cx - r_outer + 2, cy - r_outer + 2, cx + r_outer - 2, cy + r_outer - 2], fill=(68, 44, 30, 255))
    draw.ellipse([cx - r_inner - 2, cy - r_inner - 2, cx + r_inner + 2, cy + r_inner + 2], fill=(215, 165, 75, 255), outline=(135, 95, 45, 255), width=2)
    draw.ellipse([cx - r_inner + 4, cy - r_inner + 4, cx + r_inner - 4, cy + r_inner - 4], fill=(28, 18, 12, 255))

    # Lỗ trung tâm đính ngọc
    draw.ellipse([cx - 5, cy - 5, cx + 5, cy + 5], fill=(35, 185, 130, 255), outline=(255, 235, 145, 255), width=1)

    path = os.path.join(DEST_DIR, "Btn_Settings_Gear_Wood.png")
    out.save(path, "PNG")
    write_unity_meta(path)
    print("Generated Btn_Settings_Gear_Wood.png")


def main():
    print("=== Generating Full 2.5D Vector UI Sprites for Vong Xuyen ===")
    generate_header_bar()
    generate_btn_nav()
    generate_btn_battle()
    generate_tray_loadout()
    generate_currency_pill()
    generate_pedestal_hexagon()
    generate_settings_gear_button()
    print("=== Done Generating Sprites! ===")

if __name__ == "__main__":
    main()
