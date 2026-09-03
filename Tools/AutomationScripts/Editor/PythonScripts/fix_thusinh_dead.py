import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

SRC_IMAGE = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded\media_1787737860431.png"
OUTPUT_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\ThuSinh"
PNG_PATH = os.path.join(OUTPUT_DIR, "ThuSinh-Dead.png")

GLOBAL_SCALE = 0.3692

img = Image.open(SRC_IMAGE).convert('RGB')
w, h = img.size
mid_y = h // 2 # ~343px

# 6 ô cắt chuẩn:
# Hàng 1: (0..33%), (33%..66%), (66%..100%)
# Hàng 2: (0..33%), (33%..66%), (66%..100%)
BOXES = [
    (0, 0, int(w * 0.33), mid_y),
    (int(w * 0.33), 0, int(w * 0.66), mid_y),
    (int(w * 0.66), 0, w, mid_y),
    (0, mid_y, int(w * 0.33), h),
    (int(w * 0.33), mid_y, int(w * 0.66), h),
    (int(w * 0.66), mid_y, w, h) # Cột 3 hàng 2: linh hồn sách vàng
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
    
    # Ở frame 6 (linh hồn bốc hơi), dùng ngưỡng nhạy hơn để giữ lại các cuốn sách vàng và hào quang
    threshold = 18.0 if idx == 5 else 30.0
    fg = dist_bg > threshold
    labeled, num_features = label(fg)
    sizes = np.bincount(labeled.ravel())
    sizes[0] = 0
    
    top_lbls = [l_i for l_i, sz in enumerate(sizes) if sz > (100 if idx == 5 else 300)]
    char_mask = binary_fill_holes(np.isin(labeled, top_lbls))
    
    alpha = np.zeros(char_mask.shape, dtype=np.uint8)
    alpha[char_mask] = 255
    core = binary_erosion(char_mask, iterations=2)
    
    # 1. Xóa Halo
    alpha[(dist_bg < 30.0) & (~core)] = 0
    
    # 2. Xóa bóng đổ xám dưới đất
    if idx != 5:
        is_bottom = np.zeros_like(char_mask)
        is_bottom[int(h_sub * 0.78):, :] = True
        is_dark = (r < 45) & (g < 45) & (b < 45)
        is_skin = (r > 165) & (g > 105)
        is_blue_robe = (b > 120) & (g > 100)
        is_scroll = (r > 140) & (g > 110) & (b > 70)
        is_safe = is_dark | is_skin | is_blue_robe | is_scroll
        alpha[is_bottom & (dist_bg < 65.0) & (sat < 0.16) & (~is_safe)] = 0
        
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
print("ThuSinh-Dead.png fixed with glowing book spirit!")
