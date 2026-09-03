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


def generate_modal_wood_frame():
    """1. Khung Modal Gỗ Mun Khắc Hoa Văn Cổ 9-Slice (Frame lớn bao quanh Tàng Bảo Các)"""
    W, H = 512, 512
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    # Viền đen dày ngoài cùng
    draw.rounded_rectangle([4, 4, W - 4, H - 4], radius=24, fill=(28, 18, 14, 255))
    # Khối khung gỗ mun 3D
    draw.rounded_rectangle([8, 8, W - 8, H - 8], radius=20, fill=(72, 46, 32, 255))
    # Rãnh khắc hoa văn mây núi / viền chỉ
    draw.rounded_rectangle([18, 18, W - 18, H - 18], radius=14, outline=(42, 26, 18, 255), width=4)
    draw.rounded_rectangle([24, 24, W - 24, H - 24], radius=10, fill=(50, 32, 22, 255))
    
    # 4 Góc khắc đinh tán đồng
    for cx in (28, W - 28):
        for cy in (28, H - 28):
            draw.ellipse([cx - 5, cy - 5, cx + 5, cy + 5], fill=(180, 135, 75, 255), outline=(30, 20, 12, 255), width=2)

    path = os.path.join(DEST_DIR, "Frame_Modal_TangBaoCac_9Slice.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(48, 48, 48, 48))
    print("Generated Frame_Modal_TangBaoCac_9Slice.png")


def generate_scroll_parchment_banner():
    """2. Cuộn Giấy Da Cuộn Mép 2 Đầu (Banner Tiêu Đề: TÀNG BẢO CÁC - Kho Pháp Bảo)"""
    W, H = 420, 90
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    # 2 Đầu cuộn mép phía sau
    # Mép trái
    draw.polygon([(8, 20), (50, 20), (35, 75), (0, 75)], fill=(175, 145, 110, 255), outline=(35, 25, 18, 255), width=3)
    # Mép phải
    draw.polygon([(W - 50, 20), (W - 8, 20), (W, 75), (W - 35, 75)], fill=(175, 145, 110, 255), outline=(35, 25, 18, 255), width=3)

    # Thân cuộn giấy chính diện (Parchment)
    body_poly = [
        (35, 12), (W - 35, 12),
        (W - 45, 78), (45, 78)
    ]
    # Viền đen dày
    draw.polygon([(32, 9), (W - 32, 9), (W - 42, 81), (42, 81)], fill=(35, 25, 18, 255))
    # Nền giấy da vàng ngà ấm áp
    draw.polygon(body_poly, fill=(238, 220, 186, 255))

    # Viền chỉ rãnh trong giấy da
    draw.polygon([(42, 18), (W - 42, 18), (W - 50, 72), (50, 72)], outline=(190, 165, 130, 255), width=2)

    path = os.path.join(DEST_DIR, "Banner_Parchment_Scroll.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(60, 20, 60, 20))
    print("Generated Banner_Parchment_Scroll.png")


def generate_parchment_detail_card():
    """3. Tờ Giấy Da Cổ Cột Phải (Bảng Chi Tiết Soi Chỉ Số Pháp Bảo) 9-Slice"""
    W, H = 360, 360
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    # Viền đen ngoài cùng
    draw.rounded_rectangle([4, 4, W - 4, H - 4], radius=16, fill=(35, 25, 18, 255))
    # Nền giấy da cổ
    draw.rounded_rectangle([8, 8, W - 8, H - 8], radius=12, fill=(236, 218, 184, 255))
    # Rãnh viền trang trí cổ trang
    draw.rounded_rectangle([16, 16, W - 16, H - 16], radius=8, outline=(195, 170, 135, 255), width=2)

    path = os.path.join(DEST_DIR, "Card_Parchment_Detail_9Slice.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(32, 32, 32, 32))
    print("Generated Card_Parchment_Detail_9Slice.png")


def generate_inventory_slot():
    """4. Ô Vật Phẩm Gỗ Rãnh Âm (Inventory Grid Slot 4x5) 9-Slice"""
    W, H = 84, 84
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    # Viền ngoài
    draw.rounded_rectangle([2, 2, W - 2, H - 2], radius=10, fill=(28, 18, 14, 255))
    # Thân gỗ lõm
    draw.rounded_rectangle([4, 4, W - 4, H - 4], radius=8, fill=(45, 28, 20, 255))
    # Lòng ô tối màu
    draw.rounded_rectangle([8, 8, W - 8, H - 8], radius=6, fill=(22, 14, 10, 255), outline=(65, 42, 30, 255), width=2)

    path = os.path.join(DEST_DIR, "Slot_Inventory_Wood_9Slice.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(16, 16, 16, 16))
    print("Generated Slot_Inventory_Wood_9Slice.png")


def generate_slot_selected_gold_glow():
    """5. Khung Chọn Ô Phát Sáng Viền Vàng Hoàng Kim (Slot Highlight)"""
    W, H = 84, 84
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    # Viền sáng xanh ngọc/vàng nổi bật
    draw.rounded_rectangle([2, 2, W - 2, H - 2], radius=10, outline=(0, 255, 180, 255), width=4)
    # Lớp lót vàng cam 4 góc
    for cx in (6, W - 6):
        for cy in (6, H - 6):
            draw.rectangle([cx - 3, cy - 3, cx + 3, cy + 3], fill=(255, 220, 50, 255))

    path = os.path.join(DEST_DIR, "Slot_Inventory_Selected_Glow.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(16, 16, 16, 16))
    print("Generated Slot_Inventory_Selected_Glow.png")


def generate_btn_back_wood():
    """6. Nút Quay Lại Gỗ Mun Cổ Chữ Vàng (< QUAY LẠI)"""
    W, H = 140, 52
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    draw.rounded_rectangle([2, 2, W - 2, H - 2], radius=12, fill=(28, 18, 14, 255))
    draw.rounded_rectangle([4, 4, W - 4, H - 4], radius=10, fill=(65, 42, 28, 255))
    draw.rounded_rectangle([8, 8, W - 8, H - 8], radius=7, fill=(45, 28, 18, 255), outline=(130, 95, 65, 255), width=2)

    path = os.path.join(DEST_DIR, "Btn_Back_Wood_9Slice.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(18, 18, 18, 18))
    print("Generated Btn_Back_Wood_9Slice.png")


def generate_orb_avatar_frame():
    """7. Khung Tròn Ngọc Hổ Phách Soi Vũ Khí Cột Phải (Orb Icon Frame)"""
    W, H = 110, 110
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    # 1. Viền đen dày ngoài cùng
    draw.ellipse([4, 4, W - 4, H - 4], fill=(28, 18, 14, 255))
    # 2. Vành kim loại vàng đồng chạm khắc
    draw.ellipse([7, 7, W - 7, H - 7], fill=(185, 140, 65, 255))
    # 3. Lõi nền tròn tối màu bên trong để đặt icon
    draw.ellipse([14, 14, W - 14, H - 14], fill=(24, 16, 12, 255), outline=(255, 215, 100, 255), width=3)
    # 4. Đính ngôi sao tam giác trên đỉnh và đáy vành tròn
    mid = W / 2
    draw.polygon([(mid, 2), (mid - 8, 12), (mid + 8, 12)], fill=(255, 235, 140, 255), outline=(40, 25, 15, 255), width=2)

    path = os.path.join(DEST_DIR, "Frame_Weapon_Orb_Gold.png")
    out.save(path, "PNG")
    write_unity_meta(path)
    print("Generated Frame_Weapon_Orb_Gold.png")


def generate_gauge_bars():
    """8. Thanh Gauge Đo Chỉ Số Sát Thương & Hồi Chiêu 9-Slice"""
    W, H = 200, 28
    # Frame rãnh
    out_f = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d_f = ImageDraw.Draw(out_f)
    d_f.rounded_rectangle([2, 2, W - 2, H - 2], radius=12, fill=(28, 18, 14, 255))
    d_f.rounded_rectangle([4, 4, W - 4, H - 4], radius=10, fill=(35, 24, 18, 255), outline=(100, 75, 50, 255), width=2)
    path_f = os.path.join(DEST_DIR, "Gauge_Stat_Bar_Frame.png")
    out_f.save(path_f, "PNG")
    write_unity_meta(path_f, border=(14, 10, 14, 10))

    # Fill Sát thương (Đỏ cam)
    out_d = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d_d = ImageDraw.Draw(out_d)
    d_d.rounded_rectangle([4, 4, W - 4, H - 4], radius=10, fill=(225, 95, 25, 255))
    d_d.rounded_rectangle([6, 6, W - 6, H / 2], radius=6, fill=(255, 175, 75, 255))
    path_d = os.path.join(DEST_DIR, "Gauge_Stat_Fill_Damage.png")
    out_d.save(path_d, "PNG")
    write_unity_meta(path_d, border=(10, 8, 10, 8))

    # Fill Hồi chiêu (Xanh ngọc / Cyan)
    out_c = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d_c = ImageDraw.Draw(out_c)
    d_c.rounded_rectangle([4, 4, W - 4, H - 4], radius=10, fill=(0, 180, 215, 255))
    d_c.rounded_rectangle([6, 6, W - 6, H / 2], radius=6, fill=(120, 245, 255, 255))
    path_c = os.path.join(DEST_DIR, "Gauge_Stat_Fill_Cooldown.png")
    out_c.save(path_c, "PNG")
    write_unity_meta(path_c, border=(10, 8, 10, 8))
    print("Generated Gauge Bar Frames & Fills")


def main():
    print("=== Generating Tang Bao Cac 2.5D Vector UI Sprites ===")
    generate_modal_wood_frame()
    generate_scroll_parchment_banner()
    generate_parchment_detail_card()
    generate_inventory_slot()
    generate_slot_selected_gold_glow()
    generate_btn_back_wood()
    generate_orb_avatar_frame()
    generate_gauge_bars()
    print("=== Done Generating Tang Bao Cac Sprites! ===")

if __name__ == "__main__":
    main()
