import os
import math
import uuid
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

FRAMES_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\Frames"
BUTTONS_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\Buttons"

os.makedirs(FRAMES_DIR, exist_ok=True)
os.makedirs(BUTTONS_DIR, exist_ok=True)

def write_unity_meta(filepath, border=(0, 0, 0, 0), pivot=(0.5, 0.5)):
    meta_path = filepath + ".meta"
    guid = uuid.uuid4().hex
    bx, by, bz, bw = border  # Left, Bottom, Right, Top
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

def create_modal_window(filename, size=(512, 512), radius=36):
    """Khung cửa sổ Modal chính 9-slice Chibi Arcade 3D"""
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Đổ bóng ngoài dày
    draw.rounded_rectangle([4, 10, w - 4, h - 4], radius=radius, fill=(8, 10, 14, 255))
    # Viền vàng đồng 3D ngoài
    draw.rounded_rectangle([4, 4, w - 4, h - 10], radius=radius, fill=(215, 165, 40, 255), outline=(50, 30, 8, 255), width=4)
    # Lớp viền trong màu nâu gỗ bóng
    draw.rounded_rectangle([12, 12, w - 12, h - 18], radius=radius-6, fill=(45, 32, 22, 255))
    # Nền trong suốt tối mờ hiện đại
    draw.rounded_rectangle([20, 20, w - 20, h - 26], radius=radius-12, fill=(24, 28, 36, 250))
    
    # Highlight trên đỉnh
    draw.arc([24, 16, w - 24, h - 24], start=200, end=340, fill=(255, 255, 255, 120), width=3)

    out_path = os.path.join(FRAMES_DIR, filename)
    img.save(out_path, "PNG")
    write_unity_meta(out_path, border=(48, 48, 48, 48))
    print(f"Created {filename}")

def create_title_ribbon(filename, size=(512, 128)):
    """Băng rôn tiêu đề đỏ ruby uốn lượn 3D"""
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Đáy bóng
    draw.rounded_rectangle([16, 12, w - 16, h - 8], radius=24, fill=(20, 6, 8, 255))
    # Viền vàng 3D
    draw.rounded_rectangle([12, 8, w - 12, h - 16], radius=24, fill=(245, 195, 45, 255), outline=(60, 15, 10, 255), width=4)
    # Ruột Đỏ Ruby
    draw.rounded_rectangle([20, 16, w - 20, h - 24], radius=18, fill=(195, 28, 36, 255))
    # Highlight bóng trên
    draw.rounded_rectangle([24, 18, w - 24, 18 + (h - 40)//2], radius=14, fill=(240, 75, 85, 160))

    out_path = os.path.join(FRAMES_DIR, filename)
    img.save(out_path, "PNG")
    write_unity_meta(out_path, border=(40, 20, 40, 20))
    print(f"Created {filename}")

def create_upgrade_card_frame(filename, size=(300, 440), radius=28):
    """Thẻ bài nâng cấp 9-slice Chibi Arcade 3D"""
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Đổ bóng đáy thẻ
    draw.rounded_rectangle([4, 8, w - 4, h - 4], radius=radius, fill=(10, 12, 16, 255))
    # Viền vàng 3D ngoài
    draw.rounded_rectangle([4, 4, w - 4, h - 8], radius=radius, fill=(235, 185, 45, 255), outline=(40, 25, 8, 255), width=3)
    # Lớp viền xanh đen sẫm
    draw.rounded_rectangle([10, 10, w - 10, h - 14], radius=radius-4, fill=(28, 34, 44, 255))
    # Thân thẻ nền gradient xanh ngọc tối
    card_box = [16, 16, w - 16, h - 20]
    
    grad_img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    grad_draw = ImageDraw.Draw(grad_img)
    top_c = (38, 48, 64)
    bot_c = (22, 26, 34)
    for y in range(card_box[1], card_box[3]):
        t = (y - card_box[1]) / float(card_box[3] - card_box[1])
        r = int(top_c[0] * (1 - t) + bot_c[0] * t)
        g = int(top_c[1] * (1 - t) + bot_c[1] * t)
        b = int(top_c[2] * (1 - t) + bot_c[2] * t)
        grad_draw.line([(card_box[0], y), (card_box[2], y)], fill=(r, g, b, 255))

    mask = Image.new("L", (w, h), 0)
    mask_draw = ImageDraw.Draw(mask)
    mask_draw.rounded_rectangle(card_box, radius=radius-8, fill=255)
    img.paste(grad_img, (0, 0), mask)

    # Highlight trên đầu thẻ
    draw.arc([20, 16, w - 20, h - 20], start=190, end=350, fill=(255, 255, 255, 90), width=2)

    out_path = os.path.join(FRAMES_DIR, filename)
    img.save(out_path, "PNG")
    write_unity_meta(out_path, border=(32, 32, 32, 32))
    print(f"Created {filename}")

def create_skill_icon_orb(filename, size=(160, 160)):
    """Ô tròn bọc icon kỹ năng 3D"""
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = w // 2, h // 2

    # Đổ bóng
    draw.ellipse([cx - 72, cy - 68, cx + 72, cy + 76], fill=(12, 14, 18, 255))
    # Viền vàng 3D
    draw.ellipse([cx - 70, cy - 70, cx + 70, cy + 70], fill=(245, 195, 45, 255), outline=(50, 30, 8, 255), width=4)
    # Lòng trong xanh navy tối
    draw.ellipse([cx - 58, cy - 58, cx + 58, cy + 58], fill=(24, 30, 42, 255), outline=(15, 18, 26, 255), width=3)
    # Highlight
    draw.arc([cx - 52, cy - 52, cx + 52, cy + 52], start=200, end=340, fill=(255, 255, 255, 160), width=3)

    out_path = os.path.join(FRAMES_DIR, filename)
    img.save(out_path, "PNG")
    write_unity_meta(out_path, border=(0, 0, 0, 0), pivot=(0.5, 0.5))
    print(f"Created {filename}")

if __name__ == "__main__":
    # 1. Khung Modal Chính
    create_modal_window("Frame_Upgrade_Modal_Chunky_3D.png", size=(512, 512), radius=36)

    # 2. Băng Rôn Tiêu Đề
    create_title_ribbon("Banner_Title_Ribbon_3D.png", size=(512, 128))

    # 3. Thẻ Bài Nâng Cấp 3D
    create_upgrade_card_frame("Card_Upgrade_Chunky_9Slice.png", size=(300, 440), radius=28)

    # 4. Ô Tròn Bọc Icon Kỹ Năng
    create_skill_icon_orb("Frame_Skill_Icon_Orb_3D.png", size=(160, 160))

    print("All Upgrade Panel Sprites generated successfully!")
