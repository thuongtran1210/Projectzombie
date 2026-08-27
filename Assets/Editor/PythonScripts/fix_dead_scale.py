import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
TARGET_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\AnSi"

def fix_dead_global_scale():
    src_path = os.path.join(MEDIA_DIR, "media_1787725660323.png") # Ảnh gốc 6 frame Dead
    dst_path = os.path.join(TARGET_DIR, "AnSi-Dead.png")
    img = Image.open(src_path).convert('RGB')
    
    bg = np.median(np.array(img)[:20, :20], axis=(0, 1))
    arr = np.array(img, dtype=float)
    diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
    
    fg = diff > 40.0
    labeled, _ = label(fg)
    sizes = np.bincount(labeled.ravel())
    sizes[0] = 0
    top_labels = np.argsort(sizes)[::-1][:6]
    char_mask = binary_fill_holes(np.isin(labeled, top_labels))
    
    labeled_chars, _ = label(char_mask)
    slices = find_objects(labeled_chars)
    valid_slices = [s for s in slices if s is not None and np.sum(labeled_chars[s] > 0) > 2000]
    
    row_height = img.height // 2
    valid_slices.sort(key=lambda s: (s[0].start // (row_height - 50), s[1].start))
    
    arr_uint = np.array(img, dtype=np.uint8)
    frames = []
    
    # KHÓA CHẶT GLOBAL SCALE FACTOR = 0.3429 CHO MỌI FRAME DEAD
    # Tuyệt đối không scale riêng theo chiều cao/rộng của dáng ngả nghiêng hay nằm bẹp!
    GLOBAL_SCALE = 0.3429
    
    for idx, sl in enumerate(valid_slices[:6]):
        sy, sx = sl
        sub_rgb = arr_uint[sy, sx].copy()
        sub_mask = (labeled_chars[sy, sx] > 0)
        
        r, g, b = sub_rgb[:, :, 0].astype(float), sub_rgb[:, :, 1].astype(float), sub_rgb[:, :, 2].astype(float)
        dist_bg = np.sqrt((r - bg[0])**2 + (g - bg[1])**2 + (b - bg[2])**2)
        lum = 0.299 * r + 0.587 * g + 0.114 * b
        sat = (np.maximum(np.maximum(r, g), b) - np.minimum(np.minimum(r, g), b)) / (np.maximum(np.maximum(r, g), b) + 1e-5)
        
        alpha = np.zeros(sub_mask.shape, dtype=np.uint8)
        alpha[sub_mask] = 255
        core = binary_erosion(sub_mask, iterations=2)
        
        # Xóa sparkle ở chân frame 6
        h_sub, w_sub = sub_mask.shape
        is_bottom_right = np.zeros_like(sub_mask)
        is_bottom_right[int(h_sub * 0.60):, int(w_sub * 0.70):] = True
        is_sparkle = is_bottom_right & (lum > 170) & (sat < 0.15)
        alpha[is_sparkle] = 0
        
        # Xóa bóng đất xám (không xóa linh hồn bay lên ở góc trên bên phải)
        is_bottom_area = np.zeros_like(sub_mask)
        is_bottom_area[int(h_sub * 0.75):, :] = True
        is_safe = (r < 50) & (g < 50) & (b < 50) | (r > 170) & (g > 110) # viền đen hoặc da
        is_ground_shadow = is_bottom_area & (dist_bg < 65.0) & (sat < 0.16) & (~is_safe)
        alpha[is_ground_shadow] = 0
        
        alpha[(dist_bg < 45.0) & (~core)] = 0
        
        char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
        bbox = char_pil.getbbox()
        if bbox:
            char_pil = char_pil.crop(bbox)
            
        cw, ch = char_pil.size
        
        # ÁP DỤNG ĐỒNG BỘ 1:1 GLOBAL_SCALE
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
    print("AnSi-Dead.png scaled 1:1 perfectly with Global Ratio across all 6 frames!")

fix_dead_global_scale()
