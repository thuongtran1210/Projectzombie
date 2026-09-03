import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

SRC_IMAGE = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded\media_1787796100188.png"
OUTPUT_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\QuyNhapTrang"
PNG_PATH = os.path.join(OUTPUT_DIR, "QuyNhapTrang-Attack.png")

GLOBAL_SCALE = 0.2945

img = Image.open(SRC_IMAGE).convert('RGB')
w, h = img.size
bg = np.median(np.array(img)[:20, :20], axis=(0, 1))

half_h = h // 2
w1, w2 = int(w * 0.333), int(w * 0.666)
boxes = [
    (0, 0, w1, half_h),
    (w1, 0, w2, half_h),
    (w2, 0, w, half_h),
    (0, half_h, w1, h),
    (w1, half_h, w2, h),
    (w2, half_h, w, h)
]

frames = []
for idx, box in enumerate(boxes):
    sub_img = img.crop(box)
    sub_rgb = np.array(sub_img, dtype=np.uint8)
    r, g, b = sub_rgb[:, :, 0].astype(float), sub_rgb[:, :, 1].astype(float), sub_rgb[:, :, 2].astype(float)
    dist_bg = np.sqrt((r - bg[0])**2 + (g - bg[1])**2 + (b - bg[2])**2)
    lum = 0.299 * r + 0.587 * g + 0.114 * b
    max_c = np.maximum(np.maximum(r, g), b)
    min_c = np.minimum(np.minimum(r, g), b)
    sat = (max_c - min_c) / (max_c + 1e-5)
    h_sub, w_sub = sub_rgb.shape[:2]
    
    fg = dist_bg > 35.0
    labeled, _ = label(fg)
    sizes = np.bincount(labeled.ravel())
    sizes[0] = 0
    
    slices = find_objects(labeled)
    valid_lbls = []
    for l_i, sl in enumerate(slices):
        if sl is not None:
            sz = sizes[l_i + 1]
            if sz > 300:
                valid_lbls.append(l_i + 1)
                
    char_mask = binary_fill_holes(np.isin(labeled, valid_lbls))
    core = binary_erosion(char_mask, iterations=2)
    alpha = np.zeros(char_mask.shape, dtype=np.uint8)
    alpha[char_mask] = 255
    alpha[(dist_bg < 48.0) & (~core)] = 0
    
    # Xóa bóng đất (22% đáy)
    is_bottom = np.zeros_like(char_mask)
    is_bottom[int(h_sub * 0.78):, :] = True
    is_dark = (r < 45) & (g < 45) & (b < 45)
    is_skin = (r > 160) & (g > 180) & (b > 190)
    is_robe = (b > 60) & (r < 70)
    is_talisman = (r > 170) & (g > 120) & (b < 80)
    is_safe = is_dark | is_skin | is_robe | is_talisman
    alpha[is_bottom & (dist_bg < 68.0) & (sat < 0.16) & (~is_safe)] = 0
    
    # Xóa sparkle ở góc dưới phải frame 6
    if idx == 5:
        is_sparkle_area = np.zeros_like(char_mask)
        is_sparkle_area[int(h_sub * 0.65):, int(w_sub * 0.65):] = True
        is_sparkle = is_sparkle_area & (lum > 165) & (sat < 0.15)
        alpha[is_sparkle] = 0
        
    char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
    bbox = char_pil.getbbox()
    if bbox:
        char_pil = char_pil.crop(bbox)
    cw, ch = char_pil.size
    nw, nh = int(round(cw * GLOBAL_SCALE)), int(round(ch * GLOBAL_SCALE))
    resized = char_pil.resize((nw, nh), Image.Resampling.LANCZOS)
    res_arr = np.array(resized)
    res_arr[res_arr[:, :, 3] < 125, 3] = 0
    resized = Image.fromarray(res_arr, 'RGBA')
    target = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    px = (128 - nw) // 2
    py = max(2, 128 - nh - 8)
    target.paste(resized, (px, py), resized)
    frames.append(target)

strip = Image.new('RGBA', (len(frames) * 128, 128), (0, 0, 0, 0))
for i, f in enumerate(frames):
    strip.paste(f, (i * 128, 0), f)
strip.save(PNG_PATH)

print("QuyNhapTrang-Attack.png updated without VFX background contamination!")
