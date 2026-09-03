import os
import math
import uuid
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

ASSETS_ROOT = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI"
BUTTONS_DIR = os.path.join(ASSETS_ROOT, "Buttons")
HUD_DIR = os.path.join(ASSETS_ROOT, "HUD")
FRAMES_DIR = os.path.join(ASSETS_ROOT, "Frames")

os.makedirs(BUTTONS_DIR, exist_ok=True)
os.makedirs(HUD_DIR, exist_ok=True)
os.makedirs(FRAMES_DIR, exist_ok=True)

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

def create_chunky_button(filename, top_color, bot_color, outline_color=(25, 15, 10, 255), size=(256, 128), radius=28, bevel_h=12):
    """
    Sinh nút 3D Chunky Bevel phong cách Kingdom Rush / Survivor.io
    """
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # 1. Shadow / Bottom Lip (Phần nổi đáy 3D)
    draw.rounded_rectangle([2, 4, w - 2, h - 2], radius=radius, fill=outline_color)
    draw.rounded_rectangle([4, 6, w - 4, h - 4], radius=radius-2, fill=(bot_color[0]//2, bot_color[1]//2, bot_color[2]//2, 255))

    # 2. Main Body (Nút bấm)
    btn_box = [4, 4, w - 4, h - bevel_h - 4]
    
    # Tạo gradient cho thân nút
    grad_img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    grad_draw = ImageDraw.Draw(grad_img)
    for y in range(btn_box[1], btn_box[3]):
        t = (y - btn_box[1]) / float(btn_box[3] - btn_box[1])
        r = int(top_color[0] * (1 - t) + bot_color[0] * t)
        g = int(top_color[1] * (1 - t) + bot_color[1] * t)
        b = int(top_color[2] * (1 - t) + bot_color[2] * t)
        grad_draw.line([(btn_box[0], y), (btn_box[2], y)], fill=(r, g, b, 255))
    
    # Mask bo góc
    mask = Image.new("L", (w, h), 0)
    mask_draw = ImageDraw.Draw(mask)
    mask_draw.rounded_rectangle(btn_box, radius=radius-2, fill=255)
    
    img.paste(grad_img, (0, 0), mask)

    # 3. Top Highlight (Viền sáng bóng phía trên)
    hl_box = [8, 8, w - 8, 8 + (btn_box[3] - btn_box[1]) // 2]
    hl_mask = Image.new("L", (w, h), 0)
    hl_draw = ImageDraw.Draw(hl_mask)
    hl_draw.rounded_rectangle(hl_box, radius=radius-6, fill=80)
    
    hl_layer = Image.new("RGBA", (w, h), (255, 255, 255, 255))
    img.paste(hl_layer, (0, 0), hl_mask)

    # 4. Viền nét ngoài (Outer Outline)
    draw.rounded_rectangle([2, 2, w - 2, h - 2], radius=radius, outline=outline_color, width=4)

    out_path = os.path.join(BUTTONS_DIR, filename)
    img.save(out_path, "PNG")
    # 9-slice: Left, Bottom, Right, Top
    write_unity_meta(out_path, border=(32, 32, 32, 32))
    print(f"Created {filename}")

def create_pill_bar(filename, bg_color=(35, 40, 48, 240), border_color=(240, 180, 40, 255), size=(320, 64), radius=30):
    """
    Sinh thanh tài nguyên Pill Bar Capsule
    """
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Đổ bóng
    draw.rounded_rectangle([2, 4, w - 2, h - 2], radius=radius, fill=(15, 18, 22, 200))
    # Nền trong
    draw.rounded_rectangle([4, 2, w - 4, h - 4], radius=radius, fill=bg_color)
    # Viền vàng kim 3D
    draw.rounded_rectangle([4, 2, w - 4, h - 4], radius=radius, outline=border_color, width=3)
    
    # Highlight trên
    draw.arc([10, 4, w - 10, h - 4], start=190, end=350, fill=(255, 255, 255, 120), width=2)

    out_path = os.path.join(HUD_DIR, filename)
    img.save(out_path, "PNG")
    write_unity_meta(out_path, border=(32, 20, 32, 20))
    print(f"Created {filename}")

def create_slot_card(filename, size=(128, 128), radius=20):
    """
    Sinh khung Slot trang bị 9-slice
    """
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Đáy tối
    draw.rounded_rectangle([2, 4, w - 2, h - 2], radius=radius, fill=(18, 22, 28, 255))
    # Nền slot
    draw.rounded_rectangle([4, 4, w - 4, h - 8], radius=radius-2, fill=(45, 52, 64, 255))
    # Viền vàng sáng
    draw.rounded_rectangle([2, 2, w - 2, h - 6], radius=radius, outline=(230, 175, 45, 255), width=4)
    # Inner shadow
    draw.rounded_rectangle([8, 8, w - 8, h - 12], radius=radius-6, outline=(25, 30, 38, 180), width=2)

    out_path = os.path.join(FRAMES_DIR, filename)
    img.save(out_path, "PNG")
    write_unity_meta(out_path, border=(24, 24, 24, 24))
    print(f"Created {filename}")

def create_pedestal_circle(filename, size=(512, 512)):
    """
    Sinh bục trận pháp ánh sáng ma pháp dưới chân nhân vật
    """
    w, h = size
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = w // 2, h // 2

    # Vòng hào quang phát sáng ngoài
    for r in range(230, 160, -8):
        alpha = int(40 * (r - 160) / 70)
        draw.ellipse([cx - r, cy - r*0.45, cx + r, cy + r*0.45], fill=(80, 200, 240, alpha))

    # Bục đá tròn vát phối cảnh 2.5D
    draw.ellipse([cx - 180, cy - 75, cx + 180, cy + 85], fill=(30, 36, 46, 255), outline=(50, 180, 220, 255), width=6)
    draw.ellipse([cx - 165, cy - 65, cx + 165, cy + 75], fill=(22, 26, 34, 255), outline=(240, 190, 60, 200), width=3)

    # Ký tự trận pháp / Bát quái tâm
    draw.ellipse([cx - 60, cy - 25, cx + 60, cy + 25], outline=(80, 220, 255, 230), width=3)
    draw.line([cx - 120, cy, cx + 120, cy], fill=(80, 220, 255, 180), width=2)

    out_path = os.path.join(HUD_DIR, filename)
    img.save(out_path, "PNG")
    write_unity_meta(out_path, border=(0, 0, 0, 0), pivot=(0.5, 0.5))
    print(f"Created {filename}")

if __name__ == "__main__":
    # 1. Nút Xuất Trận 3D Đỏ Rực (Chunky Battle Button)
    create_chunky_button("Btn_Battle_3D_Red.png", top_color=(245, 75, 75), bot_color=(170, 25, 30), outline_color=(50, 10, 12, 255), size=(300, 110), radius=32, bevel_h=16)
    create_chunky_button("Btn_Battle_3D_Red_Pressed.png", top_color=(190, 35, 40), bot_color=(130, 15, 20), outline_color=(40, 8, 10, 255), size=(300, 110), radius=32, bevel_h=6)

    # 2. Nút Menu Xanh Ngọc 3D (Chunky Nav Button - Anh Hùng / Tàng Bảo Các / Miếu Cổ)
    create_chunky_button("Btn_Nav_3D_Teal.png", top_color=(42, 180, 160), bot_color=(20, 105, 95), outline_color=(12, 45, 40, 255), size=(200, 90), radius=24, bevel_h=10)
    create_chunky_button("Btn_Nav_3D_Gold.png", top_color=(250, 195, 55), bot_color=(190, 125, 20), outline_color=(60, 40, 8, 255), size=(200, 90), radius=24, bevel_h=10)
    create_chunky_button("Btn_Nav_3D_Purple.png", top_color=(165, 95, 225), bot_color=(105, 45, 155), outline_color=(45, 15, 70, 255), size=(200, 90), radius=24, bevel_h=10)

    # 3. HUD Pill Bar
    create_pill_bar("PillBar_Resource_HUD.png", bg_color=(28, 32, 40, 245), border_color=(245, 195, 50, 255), size=(260, 60), radius=28)

    # 4. Khung Slot Trang Bị
    create_slot_card("Slot_Card_Equipment_9Slice.png", size=(128, 128), radius=20)
    create_slot_card("Card_Loadout_Summary_BG.png", size=(360, 160), radius=24)

    # 5. Bục Trận Pháp
    create_pedestal_circle("Pedestal_Magic_Array_2.5D.png", size=(512, 512))

    print("All Chibi Arcade UI Sprites generated successfully!")
