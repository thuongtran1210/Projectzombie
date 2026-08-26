import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
OUTPUT_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\Mada"
os.makedirs(OUTPUT_DIR, exist_ok=True)

ACTIONS = {
    "Idle": "media_1787739790904.png",
    "Run": "media_1787739797637.png",
    "Attack": "media_1787739803441.png",
    "Dead": "media_1787739809190.png"
}

# 1. Tìm Master Height của Idle (Frame 1) để khóa cứng Global Scale Factor 1:1
idle_img = Image.open(os.path.join(MEDIA_DIR, ACTIONS["Idle"])).convert('RGB')
idle_arr = np.array(idle_img, dtype=float)
bg_idle = np.median(idle_arr[:20, :20], axis=(0, 1))
fg_idle = np.sqrt(np.sum((idle_arr - bg_idle)**2, axis=2)) > 35.0
lab_idle, _ = label(fg_idle)
sizes_idle = np.bincount(lab_idle.ravel())
sizes_idle[0] = 0
slices_idle = find_objects(lab_idle)
valid_idle = [s for s in slices_idle if s is not None and np.sum(lab_idle[s] > 0) > 1500]
master_idle_raw_h = float(valid_idle[0][0].stop - valid_idle[0][0].start)
print(f"Mada Master Idle Raw Height: {master_idle_raw_h:.2f} px")

# Khóa cứng hệ số scale toàn cục (GLOBAL_SCALE) cho Ma Da = 96px chuẩn
GLOBAL_SCALE = 96.0 / master_idle_raw_h
print(f"LOCKED GLOBAL_SCALE FOR MADA = {GLOBAL_SCALE:.4f}")

def process_action_mada(action_name, filename):
    src_path = os.path.join(MEDIA_DIR, filename)
    dst_path = os.path.join(OUTPUT_DIR, f"Mada-{action_name}.png")
    
    img = Image.open(src_path).convert('RGB')
    arr = np.array(img, dtype=float)
    bg = np.median(arr[:20, :20], axis=(0, 1))
    
    diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
    fg = diff > 30.0
    labeled, num_features = label(fg)
    sizes = np.bincount(labeled.ravel())
    sizes[0] = 0
    top_labels = np.argsort(sizes)[::-1][:6]
    char_mask = binary_fill_holes(np.isin(labeled, top_labels))
    
    labeled_chars, _ = label(char_mask)
    slices = find_objects(labeled_chars)
    valid_slices = [s for s in slices if s is not None and np.sum(labeled_chars[s] > 0) > 1200]
    
    # Sắp xếp 6 frame theo Grid 2 hàng 3 cột (hàng 1: 3 frame, hàng 2: 3 frame)
    row_height = img.height // 2
    valid_slices.sort(key=lambda s: (s[0].start // (row_height - 30), s[1].start))
    
    arr_uint = np.array(img, dtype=np.uint8)
    frames = []
    
    for idx, sl in enumerate(valid_slices[:6]):
        sy, sx = sl
        sub_rgb = arr_uint[sy, sx].copy()
        sub_mask = (labeled_chars[sy, sx] > 0)
        
        r, g, b = sub_rgb[:, :, 0].astype(float), sub_rgb[:, :, 1].astype(float), sub_rgb[:, :, 2].astype(float)
        dist_bg = np.sqrt((r - bg[0])**2 + (g - bg[1])**2 + (b - bg[2])**2)
        lum = 0.299 * r + 0.587 * g + 0.114 * b
        max_c = np.maximum(np.maximum(r, g), b)
        min_c = np.minimum(np.minimum(r, g), b)
        sat = (max_c - min_c) / (max_c + 1e-5)
        
        h_sub, w_sub = sub_mask.shape
        
        alpha = np.zeros(sub_mask.shape, dtype=np.uint8)
        alpha[sub_mask] = 255
        core = binary_erosion(sub_mask, iterations=2)
        
        # 1. Xóa Halo
        alpha[(dist_bg < 35.0) & (~core)] = 0
        
        # 2. Xóa bóng đổ xám dưới đất (22% đáy)
        is_bottom = np.zeros_like(sub_mask)
        is_bottom[int(h_sub * 0.80):, :] = True
        is_dark = (r < 45) & (g < 45) & (b < 45) # viền đen / tóc đen
        is_cyan_skin = (g > 100) & (b > 120) # da ma nước xanh
        is_teal_robe = (g > 60) & (b > 70) & (r < 75) # áo ma rách chàm tối
        is_water_drops = (b > 130) & (r < 100) # giọt nước
        is_safe = is_dark | is_cyan_skin | is_teal_robe | is_water_drops
        
        is_ground_shadow = is_bottom & (dist_bg < 65.0) & (sat < 0.16) & (~is_safe)
        alpha[is_ground_shadow] = 0
        
        # 3. Xóa sparkle ở góc dưới bên phải frame 6 (trừ Dead)
        if idx == 5 and action_name != "Dead":
            is_sparkle_area = np.zeros_like(sub_mask)
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
        
    strip.save(dst_path)
    print(f"Master Clean: Mada-{action_name}.png ({len(frames)} frames) done!")

for act, file_name in ACTIONS.items():
    process_action_mada(act, file_name)
