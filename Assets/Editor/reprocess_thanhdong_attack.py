import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

SRC_IMAGE = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded\media_1787736209426.png"
OUTPUT_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\ThanhDong"
PNG_PATH = os.path.join(OUTPUT_DIR, "ThanhDong-Attack.png")
META_PATH = os.path.join(OUTPUT_DIR, "ThanhDong-Attack.png.meta")
ANIM_PATH = os.path.join(OUTPUT_DIR, "Attack.anim")

GLOBAL_SCALE = 0.4192

# Ảnh gốc có 7 frames rõ ràng:
# Hàng 1 (y: 0..50%): 3 frames (Cột 1, 2, 3)
# Hàng 2 (y: 50%..100%): 4 frames (Cột 1, 2, 3, 4)
# Kích thước ảnh:
img = Image.open(SRC_IMAGE).convert('RGB')
w, h = img.size
mid_y = h // 2
col_w = w // 4 # Mỗi cột rộng khoảng 1/4 ảnh

BOXES = [
    # Hàng 1: 3 frames
    (0, 0, col_w, mid_y),
    (col_w, 0, col_w * 2, mid_y),
    (col_w * 2, 0, col_w * 3, mid_y),
    # Hàng 2: 4 frames
    (0, mid_y, col_w, h),
    (col_w, mid_y, col_w * 2, h),
    (col_w * 2, mid_y, col_w * 3, h),
    (col_w * 3, mid_y, w, h)
]

bg = np.median(np.array(img)[:20, :20], axis=(0, 1))
frames = []

for idx, box in enumerate(BOXES):
    sub_img = img.crop(box)
    sub_rgb = np.array(sub_img, dtype=np.uint8)
    r = sub_rgb[:, :, 0].astype(float)
    g = sub_rgb[:, :, 1].astype(float)
    b = sub_rgb[:, :, 2].astype(float)
    
    dist_bg = np.sqrt((r - bg[0])**2 + (g - bg[1])**2 + (b - bg[2])**2)
    lum = 0.299 * r + 0.587 * g + 0.114 * b
    max_c = np.maximum(np.maximum(r, g), b)
    min_c = np.minimum(np.minimum(r, g), b)
    sat = (max_c - min_c) / (max_c + 1e-5)
    
    h_sub, w_sub = sub_rgb.shape[:2]
    
    fg = dist_bg > 30.0
    labeled, num_features = label(fg)
    sizes = np.bincount(labeled.ravel())
    sizes[0] = 0
    
    # Lấy các cụm lớn > 300px (bao gồm cả dải lụa vòng qua đầu)
    top_lbls = [l_i for l_i, sz in enumerate(sizes) if sz > 300]
    char_mask = binary_fill_holes(np.isin(labeled, top_lbls))
    
    alpha = np.zeros(char_mask.shape, dtype=np.uint8)
    alpha[char_mask] = 255
    core = binary_erosion(char_mask, iterations=2)
    
    # 1. Xóa Halo
    alpha[(dist_bg < 40.0) & (~core)] = 0
    
    # 2. Xóa bóng đổ xám dưới đất (22% đáy)
    is_bottom = np.zeros_like(char_mask)
    is_bottom[int(h_sub * 0.78):, :] = True
    is_dark = (r < 45) & (g < 45) & (b < 45)
    is_skin = (r > 165) & (g > 105)
    is_red = (r > 135) & (g < 65)
    is_green = (g > 80) & (r < 90)
    is_safe = is_dark | is_skin | is_red | is_green
    alpha[is_bottom & (dist_bg < 65.0) & (sat < 0.16) & (~is_safe)] = 0
    
    # 3. Xóa sparkle nếu có ở frame cuối
    if idx == 6:
        is_sparkle_area = np.zeros_like(char_mask)
        is_sparkle_area[int(h_sub * 0.65):, int(w_sub * 0.65):] = True
        alpha[is_sparkle_area & (lum > 165) & (sat < 0.15)] = 0
        
    char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
    bbox = char_pil.getbbox()
    if bbox:
        char_pil = char_pil.crop(bbox)
        
    cw, ch = char_pil.size
    nw, nh = int(round(cw * GLOBAL_SCALE)), int(round(ch * GLOBAL_SCALE))
    resized = char_pil.resize((nw, nh), Image.Resampling.LANCZOS)
    
    res_arr = np.array(resized)
    res_arr[res_arr[:, :, 3] < 100, 3] = 0
    resized = Image.fromarray(res_arr, 'RGBA')
    
    target = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    px = (128 - nw) // 2
    py = max(2, 128 - nh - 8)
    target.paste(resized, (px, py), resized)
    frames.append(target)

# Ghi file PNG 7 frames (7 * 128 = 896px)
strip = Image.new('RGBA', (len(frames) * 128, 128), (0, 0, 0, 0))
for i, f in enumerate(frames):
    strip.paste(f, (i * 128, 0), f)
strip.save(PNG_PATH)
print(f"Baking ThanhDong-Attack.png completed with {len(frames)} frames!")

# Cập nhật .meta và Attack.anim chuẩn 7 frames
guid = "4100000000000000a0b77823fca88d03"
fps = 14
is_loop = 0

sprites_yaml_list = []
name_table_list = []
curve_keys = []
pptr_maps = []

for i in range(len(frames)):
    s_name = f"ThanhDong-Attack_{i}"
    internal_id = 4100000000 + (2 * 100000) + (i * 12345)
    s_id = f"{internal_id:032d}"
    
    s_yaml = (
        f"    - serializedVersion: 2\n"
        f"      name: {s_name}\n"
        f"      rect:\n"
        f"        serializedVersion: 2\n"
        f"        x: {i * 128}\n"
        f"        y: 0\n"
        f"        width: 128\n"
        f"        height: 128\n"
        f"      alignment: 7\n"
        f"      pivot: {{x: 0.5, y: 0}}\n"
        f"      border: {{x: 0, y: 0, z: 0, w: 0}}\n"
        f"      outline: []\n"
        f"      physicsShape: []\n"
        f"      tessellationDetail: 0\n"
        f"      bones: []\n"
        f"      spriteID: {s_id}\n"
        f"      internalID: {internal_id}\n"
        f"      vertices: []\n"
        f"      indices: \n"
        f"      edges: []\n"
        f"      weights: []"
    )
    sprites_yaml_list.append(s_yaml)
    name_table_list.append(f"      {s_name}: {internal_id}")
    
    t = i * (1.0 / fps)
    curve_keys.append(f"    - time: {t:.4f}\n      value: {{fileID: {internal_id}, guid: {guid}, type: 3}}")
    pptr_maps.append(f"    - {{fileID: {internal_id}, guid: {guid}, type: 3}}")

sprites_block = "\n".join(sprites_yaml_list)
names_block = "\n".join(name_table_list)

meta_content = (
    "fileFormatVersion: 2\n"
    f"guid: {guid}\n"
    "TextureImporter:\n"
    "  internalIDToNameTable: []\n"
    "  externalObjects: {}\n"
    "  serializedVersion: 13\n"
    "  mipmaps:\n"
    "    mipMapMode: 0\n"
    "    enableMipMap: 0\n"
    "    sRGBTexture: 1\n"
    "    linearTexture: 0\n"
    "    fadeOut: 0\n"
    "    borderMipMap: 0\n"
    "    mipMapsPreserveCoverage: 0\n"
    "    alphaTestReferenceValue: 0.5\n"
    "    mipMapFadeDistanceStart: 1\n"
    "    mipMapFadeDistanceEnd: 3\n"
    "  bumpmap:\n"
    "    convertToNormalMap: 0\n"
    "    externalNormalMap: 0\n"
    "    heightScale: 0.25\n"
    "    normalMapFilter: 0\n"
    "    flipGreenChannel: 0\n"
    "  isReadable: 1\n"
    "  streamingMipmaps: 0\n"
    "  streamingMipmapsPriority: 0\n"
    "  vTOnly: 0\n"
    "  ignoreMipmapLimit: 0\n"
    "  grayScaleToAlpha: 0\n"
    "  generateCubemap: 6\n"
    "  cubemapConvolution: 0\n"
    "  seamlessCubemap: 0\n"
    "  textureFormat: 1\n"
    "  maxTextureSize: 2048\n"
    "  textureSettings:\n"
    "    serializedVersion: 2\n"
    "    filterMode: 0\n"
    "    aniso: 1\n"
    "    mipBias: 0\n"
    "    wrapU: 1\n"
    "    wrapV: 1\n"
    "    wrapW: 1\n"
    "  nPOTScale: 0\n"
    "  lightmap: 0\n"
    "  compressionQuality: 50\n"
    "  spriteMode: 2\n"
    "  spriteExtrude: 1\n"
    "  spriteMeshType: 1\n"
    "  alignment: 0\n"
    "  spritePivot: {x: 0.5, y: 0}\n"
    "  spritePixelsToUnits: 64\n"
    "  spriteBorder: {x: 0, y: 0, z: 0, w: 0}\n"
    "  spriteGenerateFallbackPhysicsShape: 1\n"
    "  alphaUsage: 1\n"
    "  alphaIsTransparency: 1\n"
    "  spriteTessellationDetail: -1\n"
    "  textureType: 8\n"
    "  textureShape: 1\n"
    "  singleChannelComponent: 0\n"
    "  flipbookRows: 1\n"
    "  flipbookColumns: 1\n"
    "  maxTextureSizeSet: 0\n"
    "  compressionQualitySet: 0\n"
    "  textureFormatSet: 0\n"
    "  ignorePngGamma: 0\n"
    "  applyGammaDecoding: 0\n"
    "  cookieLightType: 0\n"
    "  platformSettings:\n"
    "  - serializedVersion: 3\n"
    "    buildTarget: DefaultTexturePlatform\n"
    "    maxTextureSize: 2048\n"
    "    resizeAlgorithm: 0\n"
    "    textureFormat: -1\n"
    "    textureCompression: 0\n"
    "    compressionQuality: 50\n"
    "    crunchedCompression: 0\n"
    "    allowsAlphaSplitting: 0\n"
    "    overridden: 0\n"
    "    ignorePlatformSupport: 0\n"
    "    androidETC2FallbackOverride: 0\n"
    "    forceMaximumCompressionQuality_BC6H_BC7: 0\n"
    "  spriteSheet:\n"
    "    serializedVersion: 2\n"
    "    sprites:\n"
    f"{sprites_block}\n"
    "    outline: []\n"
    "    physicsShape: []\n"
    "    bones: []\n"
    "    spriteID: 5e97eb03825dee720800000000000006\n"
    "    internalID: 0\n"
    "    vertices: []\n"
    "    indices: \n"
    "    edges: []\n"
    "    weights: []\n"
    "    secondaryTextures: []\n"
    "    nameFileIdTable:\n"
    f"{names_block}\n"
    "  mipmapLimitGroupName: \n"
    "  pSDRemoveMatte: 0\n"
    "  userData: \n"
    "  assetBundleName: \n"
    "  assetBundleVariant: \n"
)
with open(META_PATH, "w", encoding="utf-8") as f:
    f.write(meta_content)

stop_time = len(frames) / fps
curves_block = "\n".join(curve_keys)
pptr_block = "\n".join(pptr_maps)

anim_content = (
    "%YAML 1.1\n"
    "%TAG !u! tag:unity3d.com,2011:\n"
    "--- !u!74 &7400000\n"
    "AnimationClip:\n"
    "  m_ObjectHideFlags: 0\n"
    "  m_CorrespondingSourceObject: {fileID: 0}\n"
    "  m_PrefabInstance: {fileID: 0}\n"
    "  m_PrefabAsset: {fileID: 0}\n"
    "  m_Name: Attack\n"
    "  serializedVersion: 7\n"
    "  m_Legacy: 0\n"
    "  m_Compressed: 0\n"
    "  m_UseHighQualityCurve: 1\n"
    "  m_RotationCurves: []\n"
    "  m_CompressedRotationCurves: []\n"
    "  m_EulerCurves: []\n"
    "  m_PositionCurves: []\n"
    "  m_ScaleCurves: []\n"
    "  m_FloatCurves: []\n"
    "  m_PPtrCurves:\n"
    "  - serializedVersion: 2\n"
    "    curve:\n"
    f"{curves_block}\n"
    "    attribute: m_Sprite\n"
    "    path: \n"
    "    classID: 212\n"
    "    script: {fileID: 0}\n"
    "    flags: 2\n"
    f"  m_SampleRate: {fps}\n"
    "  m_WrapMode: 0\n"
    "  m_Bounds:\n"
    "    m_Center: {x: 0, y: 0, z: 0}\n"
    "    m_Extent: {x: 0, y: 0, z: 0}\n"
    "  m_ClipBindingConstant:\n"
    "    genericBindings:\n"
    "    - serializedVersion: 2\n"
    "      path: 0\n"
    "      attribute: 0\n"
    "      script: {fileID: 0}\n"
    "      typeID: 212\n"
    "      customType: 23\n"
    "      isPPtrCurve: 1\n"
    "      isIntCurve: 0\n"
    "      isSerializeReferenceCurve: 0\n"
    "    pptrCurveMapping:\n"
    f"{pptr_block}\n"
    "  m_AnimationClipSettings:\n"
    "    serializedVersion: 2\n"
    "    m_AdditiveReferencePoseClip: {fileID: 0}\n"
    "    m_AdditiveReferencePoseTime: 0\n"
    "    m_StartTime: 0\n"
    f"    m_StopTime: {stop_time:.4f}\n"
    "    m_OrientationOffsetY: 0\n"
    "    m_Level: 0\n"
    "    m_CycleOffset: 0\n"
    "    m_HasAdditiveReferencePose: 0\n"
    f"    m_LoopTime: {is_loop}\n"
    "    m_LoopBlend: 0\n"
    "    m_LoopBlendOrientation: 0\n"
    "    m_LoopBlendPositionY: 0\n"
    "    m_LoopBlendPositionXZ: 0\n"
    "    m_KeepOriginalOrientation: 0\n"
    "    m_KeepOriginalPositionY: 1\n"
    "    m_KeepOriginalPositionXZ: 0\n"
    "    m_HeightFromFeet: 0\n"
    "    m_Mirror: 0\n"
    "  m_EditorCurves: []\n"
    "  m_EulerEditorCurves: []\n"
    "  m_HasGenericRootTransform: 0\n"
    "  m_HasMotionFloatCurves: 0\n"
    "  m_Events: []\n"
)
with open(ANIM_PATH, "w", encoding="utf-8") as f:
    f.write(anim_content)

print("Updated ThanhDong-Attack.png, .meta and Attack.anim successfully!")
