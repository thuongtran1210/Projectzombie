import os
import shutil
import numpy as np
from PIL import Image, ImageDraw

SRC_IMG = r"C:\Users\thuon\.gemini\antigravity-ide\brain\a0caa716-0c54-40e4-a25a-54134669c143\.user_uploaded\media_1787827193679.png"
DEST_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\VongXuyen"
os.makedirs(DEST_DIR, exist_ok=True)

def write_unity_meta(filepath, border=(0, 0, 0, 0), pivot=(0.5, 0.5)):
    meta_path = filepath + ".meta"
    import uuid
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
  isReadable: 1
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

def process():
    print(f"Reading src image: {SRC_IMG}")
    img = Image.open(SRC_IMG).convert("RGBA")
    W, H = img.size
    print(f"Image dimensions: {W} x {H}")

    # 1. Background Vọng Xuyên Full Scene (1024 x 576 -> resize standard)
    bg_out = os.path.join(DEST_DIR, "BG_VongXuyen_Forest_Hub.png")
    img.save(bg_out, "PNG")
    write_unity_meta(bg_out)
    print("Saved BG_VongXuyen_Forest_Hub.png")

    # 2. Extract Top Header Bar (Khung gỗ đỉnh)
    # y: 0 -> 100
    top_bar = img.crop((0, 0, W, int(H * 0.16)))
    top_out = os.path.join(DEST_DIR, "Header_Wood_Bar_VongXuyen.png")
    top_bar.save(top_out, "PNG")
    write_unity_meta(top_out, border=(100, 0, 100, 0))
    print("Saved Header_Wood_Bar_VongXuyen.png")

    # 3. Extract Bục Đá Lục Giác 2.5D (Pedestal)
    # Bục nằm ở giữa: x around 380 -> 640, y around 250 -> 400
    pedestal = img.crop((int(W * 0.38), int(H * 0.44), int(W * 0.63), int(H * 0.68)))
    ped_out = os.path.join(DEST_DIR, "Pedestal_Hexagon_2_5D_WoodStone.png")
    pedestal.save(ped_out, "PNG")
    write_unity_meta(ped_out, pivot=(0.5, 0.25))
    print("Saved Pedestal_Hexagon_2_5D_WoodStone.png")

    # 4. Extract Nút Xuất Trận Lục Giác Ngọc Hổ Phách (Btn Battle)
    # Góc dưới phải: x 820 -> 990, y 470 -> 560
    btn_battle = img.crop((int(W * 0.83), int(H * 0.81), int(W * 0.99), int(H * 0.98)))
    btn_out = os.path.join(DEST_DIR, "Btn_Battle_Hex_Amber_Glow.png")
    btn_battle.save(btn_out, "PNG")
    write_unity_meta(btn_out, border=(30, 20, 30, 20))
    print("Saved Btn_Battle_Hex_Amber_Glow.png")

    # 5. Extract Nút Thẻ Gỗ Khâu Chỉ (Navigation Buttons)
    # 3 nút nằm ở giữa đáy: x 370 -> 650, y 500 -> 560
    btn_nav = img.crop((int(W * 0.37), int(H * 0.88), int(W * 0.465), int(H * 0.97)))
    nav_out = os.path.join(DEST_DIR, "Btn_Nav_Wood_Stitched.png")
    btn_nav.save(nav_out, "PNG")
    write_unity_meta(nav_out, border=(16, 16, 16, 16))
    print("Saved Btn_Nav_Wood_Stitched.png")

    # 6. Extract Khay Loadout Gỗ Góc Dưới Trái
    # x 10 -> 215, y 470 -> 565
    tray_loadout = img.crop((int(W * 0.01), int(H * 0.82), int(W * 0.22), int(H * 0.985)))
    tray_out = os.path.join(DEST_DIR, "Tray_Loadout_Wood_Frame.png")
    tray_loadout.save(tray_out, "PNG")
    write_unity_meta(tray_out, border=(24, 24, 24, 24))
    print("Saved Tray_Loadout_Wood_Frame.png")

    # 7. Extract Thẻ Bài Pháp Bảo Nan Quạt (Card Fan Deck)
    # x 30 -> 195, y 385 -> 490
    card_deck = img.crop((int(W * 0.03), int(H * 0.68), int(W * 0.20), int(H * 0.86)))
    deck_out = os.path.join(DEST_DIR, "Card_Relic_Fan_Deck.png")
    card_deck.save(deck_out, "PNG")
    write_unity_meta(deck_out)
    print("Saved Card_Relic_Fan_Deck.png")

    # 8. Extract Currency Pill Bar (Tiền Vàng & Phù Lục)
    # x 750 -> 870, y 10 -> 45
    pill_curr = img.crop((int(W * 0.74), int(H * 0.02), int(W * 0.86), int(H * 0.08)))
    curr_out = os.path.join(DEST_DIR, "Pill_Currency_Wood.png")
    pill_curr.save(curr_out, "PNG")
    write_unity_meta(curr_out, border=(18, 12, 18, 12))
    print("Saved Pill_Currency_Wood.png")

    print("\nAll Vọng Xuyên UI Assets Extracted & Meta Generated Successfully!")

if __name__ == "__main__":
    process()
