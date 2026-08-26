import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

DASH_SRC = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded\media_1787725607232.png"
OUTPUT_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\AnSi"
PNG_PATH = os.path.join(OUTPUT_DIR, "AnSi-Dash.png")

GLOBAL_SCALE = 0.3429

img = Image.open(DASH_SRC).convert('RGB')
w, h = img.size
half_w, half_h = w // 2, h // 2
bg = np.median(np.array(img)[:20, :20], axis=(0, 1))

boxes = [
    (0, 0, half_w, half_h),
    (half_w, 0, w, half_h),
    (0, half_h, half_w, h),
    (half_w, half_h, w, h)
]

frames = []
for idx, box in enumerate(boxes):
    sub_img = img.crop(box)
    sub_rgb = np.array(sub_img, dtype=np.uint8)
    r, g, b = sub_rgb[:, :, 0].astype(float), sub_rgb[:, :, 1].astype(float), sub_rgb[:, :, 2].astype(float)
    dist_bg = np.sqrt((r - bg[0])**2 + (g - bg[1])**2 + (b - bg[2])**2)
    
    fg = dist_bg > 35.0
    labeled, _ = label(fg)
    sizes = np.bincount(labeled.ravel())
    sizes[0] = 0
    
    if idx == 3:
        # Ở ô góc dưới bên phải (box 4), chỉ lấy cụm nhân vật (đứng bên phải), không lấy cụm khói trắng ở bên trái
        slices = find_objects(labeled)
        char_lbl = -1
        max_x = -1
        for l_idx, sl in enumerate(slices):
            if sl is not None and sizes[l_idx + 1] > 2000:
                if sl[1].start > max_x:
                    max_x = sl[1].start
                    char_lbl = l_idx + 1
        char_mask = binary_fill_holes(labeled == char_lbl)
    else:
        top_lbl = np.argsort(sizes)[::-1][:1]
        char_mask = binary_fill_holes(np.isin(labeled, top_lbl))
        
    alpha = np.zeros(char_mask.shape, dtype=np.uint8)
    alpha[char_mask] = 255
    core = binary_erosion(char_mask, iterations=2)
    alpha[(dist_bg < 35.0) & (~core)] = 0
    
    # Xóa bóng đất
    h_sub = sub_rgb.shape[0]
    is_bottom = np.zeros_like(char_mask)
    is_bottom[int(h_sub * 0.80):, :] = True
    is_dark = (r < 45) & (g < 45) & (b < 45)
    is_skin = (r > 165) & (g > 105)
    is_brown = (r > 100) & (g > 60) & (b < 60)
    is_safe = is_dark | is_skin | is_brown
    alpha[is_bottom & (dist_bg < 65.0) & (~is_safe)] = 0
    
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

strip = Image.new('RGBA', (len(frames) * 128, 128), (0, 0, 0, 0))
for i, f in enumerate(frames):
    strip.paste(f, (i * 128, 0), f)
strip.save(PNG_PATH)
print("AnSi-Dash.png 4 frames cleaned perfectly!")
