import os, uuid

def make_meta(png_path, border=(100, 100, 100, 100)):
    meta_path = png_path + ".meta"
    guid = uuid.uuid4().hex
    L, B, R, T = border
    content = f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  textureSettings:
    filterMode: 1
    aniso: 1
    wrapU: 1
    wrapV: 1
  spriteMode: 1
  spritePixelsToUnits: 100
  spriteBorder: {{x: {L}, y: {B}, z: {R}, w: {T}}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  textureType: 8
  textureShape: 1
  platformSettings:
  - serializedVersion: 3
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    textureCompression: 1
    crunchedCompression: 0
  - serializedVersion: 3
    buildTarget: Android
    maxTextureSize: 2048
    textureFormat: 4
    textureCompression: 0
    overridden: 1
"""
    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"Created meta: {meta_path} with border {border}")

card_png = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\Frames\Frame_Card_Upgrade_DongSon.png"
modal_png = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\Frames\Frame_Modal_Window_DongSon.png"

# Card size: (633, 898) -> corner jade brackets are ~110px
make_meta(card_png, border=(115, 115, 115, 115))

# Modal size: (747, 457) -> corner cinnabar cloud brackets are ~120px
make_meta(modal_png, border=(125, 125, 125, 125))
