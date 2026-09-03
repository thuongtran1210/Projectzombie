import os
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


def generate_parchment_banner():
    """1. Băng Rôn Cuộn Giấy Da Cổ (LỰA CHỌN PHÁP BẢO)"""
    W, H = 500, 84
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    # 2 Đầu cuộn mép phía sau
    # Cuộn trái
    poly_left_tail = [(15, 24), (65, 12), (60, 72), (10, 64), (28, 44)]
    draw.polygon(poly_left_tail, fill=(185, 155, 115, 255), outline=(32, 20, 14, 255), width=3)
    # Cuộn phải
    poly_right_tail = [(W - 15, 24), (W - 65, 12), (W - 60, 72), (W - 10, 64), (W - 28, 44)]
    draw.polygon(poly_right_tail, fill=(185, 155, 115, 255), outline=(32, 20, 14, 255), width=3)

    # Cuộn tròn 2 bên mép
    draw.ellipse([45, 10, 75, 74], fill=(210, 185, 145, 255), outline=(32, 20, 14, 255), width=3)
    draw.ellipse([W - 75, 10, W - 45, 74], fill=(210, 185, 145, 255), outline=(32, 20, 14, 255), width=3)

    # Thân giấy da chính diện
    body_poly = [(58, 8), (W - 58, 8), (W - 58, 76), (58, 76)]
    draw.polygon(body_poly, fill=(242, 222, 185, 255), outline=(32, 20, 14, 255), width=3)
    # Rãnh sáng bên trong
    draw.polygon([(62, 12), (W - 62, 12), (W - 62, 72), (62, 72)], outline=(195, 165, 125, 255), width=2)

    path = os.path.join(DEST_DIR, "Banner_Upgrade_Parchment.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(80, 20, 80, 20))
    print("Generated Banner_Upgrade_Parchment.png")


def generate_card_upgrade_wood_totem():
    """2. Thẻ Nâng Cấp Gỗ Mun Dày Chạm Hoa Văn Dây Leo Cổ 9-Slice"""
    W, H = 260, 360
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    # Viền ngoài cùng gỗ mun dày
    draw.rounded_rectangle([2, 2, W - 2, H - 2], radius=16, fill=(28, 18, 12, 255))
    draw.rounded_rectangle([5, 5, W - 5, H - 5], radius=13, fill=(65, 42, 28, 255))

    # Hoa văn rãnh khắc dây leo bên trong mép gỗ
    draw.rounded_rectangle([12, 12, W - 12, H - 12], radius=10, outline=(125, 88, 55, 255), width=3)
    draw.rounded_rectangle([18, 18, W - 18, H - 18], radius=8, fill=(42, 26, 18, 255))

    path = os.path.join(DEST_DIR, "Card_Upgrade_Wood_Totem_9Slice.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(32, 32, 32, 32))
    print("Generated Card_Upgrade_Wood_Totem_9Slice.png")


def generate_badge_pill_wood():
    """3. Thẻ Gỗ Đính Đỉnh Thẻ (Rarity / Lv.1/5) 9-Slice"""
    W, H = 140, 38
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    draw.rounded_rectangle([2, 2, W - 2, H - 2], radius=10, fill=(28, 18, 12, 255))
    draw.rounded_rectangle([4, 4, W - 4, H - 4], radius=8, fill=(215, 185, 140, 255))
    draw.rounded_rectangle([7, 7, W - 7, H - 7], radius=6, fill=(238, 218, 180, 255), outline=(155, 120, 80, 255), width=1)

    path = os.path.join(DEST_DIR, "Badge_Upgrade_Pill_Wood_9Slice.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(18, 12, 18, 12))
    print("Generated Badge_Upgrade_Pill_Wood_9Slice.png")


def generate_btn_sub_wood():
    """4. Cặp Nút Phụ Bọc Gỗ Dây Leo Cổ 9-Slice (Làm Mới & Bỏ Qua)"""
    W, H = 180, 52
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    draw.rounded_rectangle([2, 2, W - 2, H - 2], radius=14, fill=(28, 18, 12, 255))
    draw.rounded_rectangle([4, 4, W - 4, H - 4], radius=12, fill=(65, 40, 26, 255))
    draw.rounded_rectangle([7, 7, W - 7, H - 7], radius=10, fill=(225, 198, 155, 255), outline=(145, 105, 65, 255), width=2)
    # Họa tiết đan bện 2 đầu
    draw.rectangle([10, 10, 22, H - 10], fill=(195, 165, 125, 255))
    draw.rectangle([W - 22, 10, W - 10, H - 10], fill=(195, 165, 125, 255))

    path = os.path.join(DEST_DIR, "Btn_Upgrade_Wood_Sub_9Slice.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(28, 16, 28, 16))
    print("Generated Btn_Upgrade_Wood_Sub_9Slice.png")


def main():
    print("=== Generating In-Game Level Up Upgrade UI Sprites ===")
    generate_parchment_banner()
    generate_card_upgrade_wood_totem()
    generate_badge_pill_wood()
    generate_btn_sub_wood()
    print("=== Done Generating Upgrade Sprites! ===")

if __name__ == "__main__":
    main()
