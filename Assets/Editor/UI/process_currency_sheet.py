import os
import sys
import uuid
from PIL import Image

def process_currency_sheet(sheet_path="Assets/Art/UI/VongXuyen/UI_Currency_Header_Sheet.png", out_dir="Assets/Art/UI/VongXuyen"):
    if not os.path.exists(sheet_path):
        print(f"Error: File not found at {sheet_path}")
        return False

    os.makedirs(out_dir, exist_ok=True)
    img = Image.open(sheet_path)
    
    # Check dimensions
    w, h = img.size
    print(f"Loaded {sheet_path} ({w}x{h}, mode={img.mode})")

    import numpy as np
    alpha = np.array(img.split()[-1])
    from scipy.ndimage import label, find_objects

    mask = alpha > 15
    labeled, num_features = label(mask)
    slices = find_objects(labeled)

    regions = []
    for i, s in enumerate(slices):
        ymin, ymax = s[0].start, s[0].stop
        xmin, xmax = s[1].start, s[1].stop
        rw = xmax - xmin
        rh = ymax - ymin
        if rw * rh > 200: # filter out tiny noise
            regions.append((xmin, ymin, xmax, ymax, rw, rh))

    # Sort regions by Y coordinate then X coordinate
    regions.sort(key=lambda r: (r[1] // 50, r[0]))
    print(f"Detected {len(regions)} elements in currency sheet:")
    for i, r in enumerate(regions):
        print(f"  Element {i+1}: bbox=({r[0]}, {r[1]}, {r[2]}, {r[3]}), size={r[4]}x{r[5]}")

    meta_template = '''fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 12
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    showMipMapValue: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMasterTextureLimit: 0
  globalScale: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: {border_l}, y: {border_b}, z: {border_r}, w: {border_t}}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookProjectis: 0
  textureFormat: 1
  maxTextureSize: 2048
  masterLOD: 0
  compressionQuality: 50
  cramTextureCompression: 1
  textureSubtype: 0
  cramFormat: 0
  ignorePngGamma: 0
  fileName: {filename}
  platformSettings:
  - serializedVersion: 3
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    cramFormat: 0
    cramTextureCompression: 1
    allowsAlphaSplitting: 0
    overridden: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_Internal: 0
  - serializedVersion: 3
    buildTarget: Android
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: 50
    textureCompression: 1
    compressionQuality: 100
    cramFormat: 0
    cramTextureCompression: 1
    allowsAlphaSplitting: 0
    overridden: 1
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_Internal: 0
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
  spritePackingTag: 
  pSN: 
  userData: 
  assetBundleName: 
  assetBundleVariant: 
'''

    # Map detected elements to target file names and 9-slice borders
    # Top row: 3 icons (Coin, Soul Flame, Diamond Gem)
    # Bottom row: Pill Frame
    icon_names = ["Icon_Currency_Coin_Gold.png", "Icon_Currency_Soul_Blue.png", "Icon_Currency_Gem_Yellow.png"]
    frame_names = ["Pill_Currency_Frame_Gold.png"]

    saved_count = 0
    for i, r in enumerate(regions):
        bbox = (r[0], r[1], r[2], r[3])
        cropped = img.crop(bbox)

        if r[5] < 150 and r[4] < 200 and i < len(icon_names):
            fname = icon_names[i]
            border = (0, 0, 0, 0)
        else:
            fname = "Pill_Currency_Frame_Gold.png"
            border = (40, 15, 40, 15) # 9-slice border for pill frame

        out_path = os.path.join(out_dir, fname)
        cropped.save(out_path)

        meta_path = out_path + ".meta"
        if not os.path.exists(meta_path):
            guid_str = str(uuid.uuid4()).replace("-", "")
            meta_content = meta_template.format(
                guid=guid_str,
                filename=fname,
                border_l=border[0],
                border_b=border[1],
                border_r=border[2],
                border_t=border[3]
            )
            with open(meta_path, "w", encoding="utf-8") as f:
                f.write(meta_content)

        print(f"Saved: {fname} (border={border})")
        saved_count += 1

    return True

if __name__ == "__main__":
    path = sys.argv[1] if len(sys.argv) > 1 else "Assets/Art/UI/VongXuyen/UI_Currency_Header_Sheet.png"
    process_currency_sheet(path)
