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


def generate_settings_banner():
    """1. Băng Rôn Cuộn Giấy Da Cổ (CÀI ĐẶT HỆ THỐNG)"""
    W, H = 480, 84
    out = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw = ImageDraw.Draw(out)

    # Cuộn 2 đầu
    poly_left_tail = [(12, 24), (60, 12), (55, 72), (8, 64), (24, 44)]
    draw.polygon(poly_left_tail, fill=(185, 155, 115, 255), outline=(32, 20, 14, 255), width=3)
    poly_right_tail = [(W - 12, 24), (W - 60, 12), (W - 55, 72), (W - 8, 64), (W - 24, 44)]
    draw.polygon(poly_right_tail, fill=(185, 155, 115, 255), outline=(32, 20, 14, 255), width=3)

    draw.ellipse([42, 10, 70, 74], fill=(210, 185, 145, 255), outline=(32, 20, 14, 255), width=3)
    draw.ellipse([W - 70, 10, W - 42, 74], fill=(210, 185, 145, 255), outline=(32, 20, 14, 255), width=3)

    body_poly = [(54, 8), (W - 54, 8), (W - 54, 76), (54, 76)]
    draw.polygon(body_poly, fill=(242, 222, 185, 255), outline=(32, 20, 14, 255), width=3)
    draw.polygon([(58, 12), (W - 58, 12), (W - 58, 72), (58, 72)], outline=(195, 165, 125, 255), width=2)

    path = os.path.join(DEST_DIR, "Banner_Settings_Parchment.png")
    out.save(path, "PNG")
    write_unity_meta(path, border=(80, 20, 80, 20))
    print("Generated Banner_Settings_Parchment.png")


def generate_slider_sprites():
    """2. Bộ Slider Gỗ Cổ: Rãnh Gỗ Âm 9-Slice, Ruột Hổ Phách & Nút Kéo Ngọc Bích"""
    # Track 9-Slice (200x24)
    W, H = 200, 24
    out_track = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw_t = ImageDraw.Draw(out_track)
    draw_t.rounded_rectangle([1, 1, W - 1, H - 1], radius=10, fill=(28, 18, 12, 255))
    draw_t.rounded_rectangle([3, 3, W - 3, H - 3], radius=8, fill=(58, 36, 24, 255))
    draw_t.rounded_rectangle([5, 5, W - 5, H - 5], radius=6, fill=(20, 14, 10, 255))
    path_track = os.path.join(DEST_DIR, "Slider_Wood_Track_9Slice.png")
    out_track.save(path_track, "PNG")
    write_unity_meta(path_track, border=(16, 8, 16, 8))

    # Fill 9-Slice (180x16)
    W_f, H_f = 180, 16
    out_fill = Image.new("RGBA", (W_f, H_f), (0, 0, 0, 0))
    draw_f = ImageDraw.Draw(out_fill)
    draw_f.rounded_rectangle([1, 1, W_f - 1, H_f - 1], radius=6, fill=(235, 145, 35, 255), outline=(255, 205, 95, 255), width=1)
    draw_f.rectangle([2, 2, W_f - 2, 4], fill=(255, 225, 130, 180)) # Highlight
    path_fill = os.path.join(DEST_DIR, "Slider_Wood_Fill_9Slice.png")
    out_fill.save(path_fill, "PNG")
    write_unity_meta(path_fill, border=(6, 4, 6, 4))

    # Handle Orb (36x36)
    W_h, H_h = 42, 42
    out_h = Image.new("RGBA", (W_h, H_h), (0, 0, 0, 0))
    draw_h = ImageDraw.Draw(out_h)
    draw_h.ellipse([2, 2, W_h - 2, H_h - 2], fill=(62, 40, 26, 255), outline=(28, 18, 12, 255), width=2)
    draw_h.ellipse([5, 5, W_h - 5, H_h - 5], fill=(225, 175, 75, 255), outline=(135, 95, 45, 255), width=2)
    draw_h.ellipse([10, 10, W_h - 10, H_h - 10], fill=(35, 175, 120, 255), outline=(255, 235, 145, 255), width=2)
    path_handle = os.path.join(DEST_DIR, "Slider_Wood_Handle_Orb.png")
    out_h.save(path_handle, "PNG")
    write_unity_meta(path_handle)
    print("Generated Slider Sprites (Track, Fill, Handle)")


def generate_toggle_wood_checkbox():
    """3. Khung Checkbox Gỗ & Dấu Ngọc Bật/Tắt (50x50)"""
    # Background Box (Off)
    W, H = 54, 54
    out_box = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw_b = ImageDraw.Draw(out_box)
    draw_b.rounded_rectangle([2, 2, W - 2, H - 2], radius=10, fill=(28, 18, 12, 255))
    draw_b.rounded_rectangle([5, 5, W - 5, H - 5], radius=8, fill=(65, 42, 28, 255))
    draw_b.rounded_rectangle([8, 8, W - 8, H - 8], radius=6, fill=(35, 22, 16, 255), outline=(135, 95, 55, 255), width=2)
    path_box = os.path.join(DEST_DIR, "Toggle_Wood_Box_Off.png")
    out_box.save(path_box, "PNG")
    write_unity_meta(path_box, border=(14, 14, 14, 14))

    # Checkmark Check Ngọc Bích (On)
    out_chk = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    draw_c = ImageDraw.Draw(out_chk)
    draw_c.ellipse([12, 12, W - 12, H - 12], fill=(35, 185, 130, 255), outline=(245, 220, 110, 255), width=3)
    draw_c.ellipse([18, 18, W - 18, H - 18], fill=(95, 235, 180, 255))
    path_chk = os.path.join(DEST_DIR, "Toggle_Wood_Checkmark_On.png")
    out_chk.save(path_chk, "PNG")
    write_unity_meta(path_chk)
    print("Generated Toggle Sprites (Box & Checkmark)")


def main():
    print("=== Generating UI Settings Vong Xuyen Vector Sprites ===")
    generate_settings_banner()
    generate_slider_sprites()
    generate_toggle_wood_checkbox()
    print("=== Done Generating Settings Sprites! ===")

if __name__ == "__main__":
    main()
