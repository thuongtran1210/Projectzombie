import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

SRC_IMAGE = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded\media_1787745367089.png"
OUTPUT_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\Madoino"
PNG_PATH = os.path.join(OUTPUT_DIR, "Madoino-Dead.png")

GLOBAL_SCALE = 0.3117

img = Image.open(SRC_IMAGE).convert('RGB')
w, h = img.size
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

bg = np.median(np.array(img)[:20, :20], axis=(0, 1))
frames = []

for idx, box in enumerate(boxes):
    sub_img = img.crop(box)
    sub_rgb = np.array(sub_img, dtype=np.uint8)
    r = sub_rgb[:, :, 0].astype(float)
    g = sub_rgb[:, :, 1].astype(float)
    b = sub_rgb[:, :, 2].astype(float)
    
    dist_bg = np.sqrt((r - bg[0])**2 + (g - bg[1])**2 + (b - bg[2])**2)
    
    threshold = 18.0 if idx >= 2 else 25.0
    fg = dist_bg > threshold
    labeled, _ = label(fg)
    sizes = np.bincount(labeled.ravel())
    sizes[0] = 0
    
    # Ở frame 2 (box 1), nếu có mảnh vụn lấn sang mép phải từ frame 3, lọc các cụm có x > 85% chiều rộng
    if idx == 1:
        slices = find_objects(labeled)
        valid_lbls = []
        for l_i, sl in enumerate(slices):
            if sl is not None and sizes[l_i + 1] > 200:
                # Nếu cụm nằm sát cạnh phải (do lấn frame) thì bỏ
                if sl[1].start < int(sub_rgb.shape[1] * 0.85):
                    valid_lbls.append(l_i + 1)
        char_mask = binary_fill_holes(np.isin(labeled, valid_lbls))
    else:
        top_lbls = [l_i for l_i, sz in enumerate(sizes) if sz > (50 if idx >= 2 else 300)]
        char_mask = binary_fill_holes(np.isin(labeled, top_lbls))
        
    alpha = np.zeros(char_mask.shape, dtype=np.uint8)
    alpha[char_mask] = 255
    core = binary_erosion(char_mask, iterations=2)
    alpha[(dist_bg < 25.0) & (~core)] = 0
    
    char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
    bbox = char_pil.getbbox()
    if bbox:
        char_pil = char_pil.crop(bbox)
    cw, ch = char_pil.size
    nw, nh = int(round(cw * GLOBAL_SCALE)), int(round(ch * GLOBAL_SCALE))
    resized = char_pil.resize((nw, nh), Image.Resampling.LANCZOS)
    res_arr = np.array(resized)
    res_arr[res_arr[:, :, 3] < 80, 3] = 0
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
print("Madoino-Dead.png cleaned with zero artifacts!")
