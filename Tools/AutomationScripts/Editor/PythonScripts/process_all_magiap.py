import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
OUTPUT_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\Magiap"
os.makedirs(OUTPUT_DIR, exist_ok=True)

ACTIONS = {
    "Idle": "media_1787744210559.png",
    "Run": "media_1787744215895.png",
    "Attack": "media_1787744515567.png",
    "Dead": "media_1787744558768.png"
}

# 1. Đo Idle Master Height (Frame 1) để lấy thước đo Master Scale
idle_img = Image.open(os.path.join(MEDIA_DIR, ACTIONS["Idle"])).convert('RGB')
idle_arr = np.array(idle_img, dtype=float)
bg_idle = np.median(idle_arr[:20, :20], axis=(0, 1))
lab_idle, _ = label(np.sqrt(np.sum((idle_arr - bg_idle)**2, axis=2)) > 35.0)
sizes_idle = np.bincount(lab_idle.ravel())
sizes_idle[0] = 0
slices_idle = find_objects(lab_idle)
valid_idle = [s for s in slices_idle if s is not None and np.sum(lab_idle[s] > 0) > 1500]
master_idle_raw_h = float(valid_idle[0][0].stop - valid_idle[0][0].start)

# GLOBAL_SCALE chuẩn cho Ma Giáp = 96px
GLOBAL_SCALE = 96.0 / master_idle_raw_h
print(f"Magiap Master Idle Raw Height: {master_idle_raw_h:.2f} px, LOCKED GLOBAL_SCALE = {GLOBAL_SCALE:.4f}")

def process_action_magiap(action_name, filename):
    src_path = os.path.join(MEDIA_DIR, filename)
    dst_path = os.path.join(OUTPUT_DIR, f"Magiap-{action_name}.png")
    
    img = Image.open(src_path).convert('RGB')
    w, h = img.size
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
    valid_slices = [s for s in slices if s is not None and np.sum(labeled_chars[s] > 0) > 1000]
    
    # Sắp xếp 6 frame theo Grid 2 hàng 3 cột
    row_height = h // 2
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
        if action_name != "Dead":
            is_bottom = np.zeros_like(sub_mask)
            is_bottom[int(h_sub * 0.80):, :] = True
            is_dark = (r < 45) & (g < 45) & (b < 45) # viền đen
            is_bone = (r > 170) & (g > 165) & (b > 150) # xương sọ
            is_bronze = (r > 70) & (g > 60) & (b < 70) # giáp đồng
            is_spectral = (g > 130) & (b > 130) # luồng ma khí xanh nhạt dưới chân
            is_safe = is_dark | is_bone | is_bronze | is_spectral
            alpha[is_bottom & (dist_bg < 65.0) & (sat < 0.16) & (~is_safe)] = 0
            
        # 3. Xóa số thứ tự F1..F6 hoặc 1..6 ở góc trên nếu bị dính
        is_top_corner = np.zeros_like(sub_mask)
        is_top_corner[:int(h_sub * 0.15), :int(w_sub * 0.20)] = True
        # Nếu có chữ số nhỏ rời rạc
        # (nhưng vì label đã lấy cụm lớn > 1000px nên số thường tự bị lọc ra)
        
        # 4. Xóa sparkle ở góc dưới bên phải frame 6 (trừ Dead)
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
    print(f"Master Clean: Magiap-{action_name}.png ({len(frames)} frames) done!")

for act, file_name in ACTIONS.items():
    process_action_magiap(act, file_name)
