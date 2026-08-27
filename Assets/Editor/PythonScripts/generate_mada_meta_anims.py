import os

OUTPUT_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\Mada"
ACTIONS = ["Idle", "Run", "Attack", "Dead"]

guids = {
    "Idle": "5100000000000000a0b77823fca88d01",
    "Run": "5100000000000000a0b77823fca88d02",
    "Attack": "5100000000000000a0b77823fca88d03",
    "Dead": "5100000000000000a0b77823fca88d04"
}

rates = {
    "Idle": (6, 1),
    "Run": (12, 1),
    "Attack": (14, 0),
    "Dead": (8, 0)
}

for action in ACTIONS:
    png_name = f"Mada-{action}.png"
    meta_path = os.path.join(OUTPUT_DIR, f"{png_name}.meta")
    anim_path = os.path.join(OUTPUT_DIR, f"{action}.anim")
    
    guid = guids[action]
    fps, is_loop = rates[action]
    
    sprites_yaml_list = []
    name_table_list = []
    curve_keys = []
    pptr_maps = []
    
    for i in range(6):
        s_name = f"Mada-{action}_{i}"
        internal_id = 5100000000 + (ACTIONS.index(action) * 100000) + (i * 12345)
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
        "    spriteID: 5e97eb03825dee720800000000000008\n"
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
    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(meta_content)
        
    stop_time = 6.0 / fps
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
        f"  m_Name: {action}\n"
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
    with open(anim_path, "w", encoding="utf-8") as f:
        f.write(anim_content)
        
    print(f"Generated {png_name}.meta and {action}.anim successfully!")
