import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
TARGET_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\AnSi"

# 1. Đo chiều cao chuẩn của nhân vật ở tư thế đứng (Idle) từ ảnh gốc
idle_img = Image.open(os.path.join(MEDIA_DIR, "media_1787724668386.png")).convert('RGB')
idle_arr = np.array(idle_img, dtype=float)
bg_idle = np.median(idle_arr[:20, :20], axis=(0, 1))
fg_idle = np.sqrt(np.sum((idle_arr - bg_idle)**2, axis=2)) > 40.0
lab_idle, _ = label(fg_idle)
sizes_idle = np.bincount(lab_idle.ravel())
sizes_idle[0] = 0
top_lbl = np.argsort(sizes_idle)[::-1][:6]
slices_idle = find_objects(lab_idle)
# Lấy chiều cao Bounding Box của Frame 1 Idle làm thước đo Master
idle_f1_slice = [s for s in slices_idle if s is not None and sizes_idle[lab_idle[s][0, 0] if lab_idle[s][0, 0] > 0 else 1] > 2000][0]
master_idle_raw_h = float(idle_f1_slice[0].stop - idle_f1_slice[0].start)
print(f"Master Idle Raw Height: {master_idle_raw_h} px")

# Khóa cứng hệ số scale toàn cục (GLOBAL_SCALE_FACTOR) dựa trên chiều cao đứng chuẩn 96px
# Bất kể động tác cúi người, lướt hay ngả nghiêng, luôn nhân đúng hệ số GLOBAL_SCALE_FACTOR này!
GLOBAL_SCALE_FACTOR = 96.0 / master_idle_raw_h
print(f"LOCKED GLOBAL_SCALE_FACTOR = {GLOBAL_SCALE_FACTOR:.4f}")

def process_all_with_global_scale():
    # A. Xử lý Dash với Global Scale cố định
    dash_path = os.path.join(MEDIA_DIR, "media_1787725607232.png")
    dash_img = Image.open(dash_path).convert('RGB')
    w, h = dash_img.size
    boxes = [
        (0, 0, w // 2, h // 2),
        (w // 2, 0, w, h // 2),
        (0, h // 2, w // 2, h),
        (w // 2, h // 2, w, h)
    ]
    bg_dash = np.median(np.array(dash_img)[:20, :20], axis=(0, 1))
    frames_dash = []
    
    for idx, box in enumerate(boxes):
        sub_img = dash_img.crop(box)
        sub_rgb = np.array(sub_img, dtype=np.uint8)
        r, g, b = sub_rgb[:, :, 0].astype(float), sub_rgb[:, :, 1].astype(float), sub_rgb[:, :, 2].astype(float)
        dist_bg = np.sqrt((r - bg_dash[0])**2 + (g - bg_dash[1])**2 + (b - bg_dash[2])**2)
        lum = 0.299 * r + 0.587 * g + 0.114 * b
        sat = (np.maximum(np.maximum(r, g), b) - np.minimum(np.minimum(r, g), b)) / (np.maximum(np.maximum(r, g), b) + 1e-5)
        
        fg = dist_bg > 40.0
        labeled, _ = label(fg)
        sizes = np.bincount(labeled.ravel())
        sizes[0] = 0
        
        if idx == 3: # Frame 4: Lấy nhân vật đứng bên phải, bỏ khói bên trái
            slices = find_objects(labeled)
            rightmost = -1
            max_x = -1
            for l_idx, sl in enumerate(slices):
                if sl is not None and sizes[l_idx + 1] > 1500:
                    if sl[1].start > max_x:
                        max_x = sl[1].start
                        rightmost = l_idx + 1
            char_mask = binary_fill_holes(labeled == rightmost)
        else:
            char_mask = binary_fill_holes(labeled == np.argmax(sizes))
            
        alpha = np.zeros(char_mask.shape, dtype=np.uint8)
        alpha[char_mask] = 255
        core = binary_erosion(char_mask, iterations=2)
        h_sub, w_sub = char_mask.shape
        
        # Xóa sparkle và shadow
        is_bottom = np.zeros_like(char_mask)
        is_bottom[int(h_sub * 0.78):, :] = True
        is_safe = (r < 50) & (g < 50) & (b < 50) | (r > 170) & (g > 110)
        alpha[is_bottom & (dist_bg < 65.0) & (sat < 0.16) & (~is_safe)] = 0
        alpha[(dist_bg < 45.0) & (~core)] = 0
        alpha[(lum > 170) & (sat < 0.12) & (sub_rgb[:, :, 0] > 180) & (~core)] = 0
        
        char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
        bbox = char_pil.getbbox()
        if bbox:
            char_pil = char_pil.crop(bbox)
            
        cw, ch = char_pil.size
        # ÁP DỤNG HỆ SỐ GLOBAL SCALE THAY VÌ SCALE RIÊNG THEO CHIỀU CAO CÚI
        nw, nh = int(round(cw * GLOBAL_SCALE_FACTOR)), int(round(ch * GLOBAL_SCALE_FACTOR))
        resized = char_pil.resize((nw, nh), Image.Resampling.LANCZOS)
        
        res_arr = np.array(resized)
        res_arr[res_arr[:, :, 3] < 100, 3] = 0
        resized = Image.fromarray(res_arr, 'RGBA')
        
        target = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
        px = (128 - nw) // 2
        py = max(2, 128 - nh - 8)
        target.paste(resized, (px, py), resized)
        frames_dash.append(target)
        
    strip_dash = Image.new('RGBA', (len(frames_dash) * 128, 128), (0, 0, 0, 0))
    for i, f in enumerate(frames_dash):
        strip_dash.paste(f, (i * 128, 0), f)
    strip_dash.save(os.path.join(TARGET_DIR, "AnSi-Dash.png"))
    print("AnSi-Dash.png scaled 1:1 with Global Ratio!")

process_all_with_global_scale()
