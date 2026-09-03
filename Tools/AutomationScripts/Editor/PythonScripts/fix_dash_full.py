import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
TARGET_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\AnSi"

def fix_dash_perfect():
    src_path = os.path.join(MEDIA_DIR, "media_1787725940028.png") # Ảnh gốc Dash do AI sinh ra
    dst_path = os.path.join(TARGET_DIR, "AnSi-Dash.png")
    img = Image.open(src_path).convert('RGB')
    
    w, h = img.size
    half_w, half_h = w // 2, h // 2
    
    # 4 góc chính xác của 4 frame trong ảnh AI
    boxes = [
        (0, 0, half_w, half_h),           # Frame 1: Cúi gồng mình chuẩn bị lướt (Top-Left)
        (half_w, 0, w, half_h),           # Frame 2: Phóng gậy lao về phía trước (Top-Right)
        (0, half_h, half_w, h),           # Frame 3: Lướt tốc độ cao tối đa (Bottom-Left)
        (half_w, half_h, w, h)            # Frame 4: Tiếp đất sau cú lướt (Bottom-Right)
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
        lum = 0.299 * r + 0.587 * g + 0.114 * b
        sat = (np.maximum(np.maximum(r, g), b) - np.minimum(np.minimum(r, g), b)) / (np.maximum(np.maximum(r, g), b) + 1e-5)
        
        # Tiền cảnh
        fg = dist_bg > 35.0
        labeled, num_features = label(fg)
        sizes = np.bincount(labeled.ravel())
        sizes[0] = 0
        
        h_sub, w_sub = sub_rgb.shape[:2]
        
        # Tìm nhân vật chính xác
        if idx == 3: # Frame 4: Có đám khói bên trái và nhân vật bên phải
            slices = find_objects(labeled)
            # Chọn cụm nằm ở nửa bên phải (x > w_sub * 0.4)
            best_lbl = -1
            for l_idx, sl in enumerate(slices):
                if sl is not None and sizes[l_idx + 1] > 1500:
                    if sl[1].start > w_sub * 0.35:
                        best_lbl = l_idx + 1
            char_mask = binary_fill_holes(labeled == best_lbl)
        else:
            main_label = np.argmax(sizes)
            char_mask = binary_fill_holes(labeled == main_label)
            
        alpha = np.zeros(char_mask.shape, dtype=np.uint8)
        alpha[char_mask] = 255
        
        core = binary_erosion(char_mask, iterations=2)
        
        # Xóa nền xám và halo
        alpha[(dist_bg < 45.0) & (~core)] = 0
        
        # Xóa bóng đất xám ở đáy
        is_bottom = np.zeros_like(char_mask)
        is_bottom[int(h_sub * 0.78):, :] = True
        is_safe = (r < 50) & (g < 50) & (b < 50) | (r > 170) & (g > 110) # viền đen hoặc da
        alpha[is_bottom & (dist_bg < 65.0) & (sat < 0.16) & (~is_safe)] = 0
        
        # Xóa sparkle nếu có
        alpha[(lum > 170) & (sat < 0.12) & (sub_rgb[:, :, 0] > 180) & (~core)] = 0
        
        # Riêng frame 4: Xóa triệt để tàn dư khói bên trái x < w_sub * 0.45
        if idx == 3:
            left_smoke = np.zeros_like(char_mask)
            left_smoke[:, :int(w_sub * 0.42)] = True
            alpha[left_smoke] = 0
            
        char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
        bbox = char_pil.getbbox()
        if bbox:
            char_pil = char_pil.crop(bbox)
            
        cw, ch = char_pil.size
        
        # Scale chuẩn theo Global Scale 0.3429
        scale = 0.3429
        nw, nh = int(round(cw * scale)), int(round(ch * scale))
        resized = char_pil.resize((nw, nh), Image.Resampling.LANCZOS)
        
        res_arr = np.array(resized)
        res_arr[res_arr[:, :, 3] < 100, 3] = 0
        resized = Image.fromarray(res_arr, 'RGBA')
        
        # Đặt vào Canvas 128x128 ghim chân cố định y=8px
        target = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
        px = (128 - nw) // 2
        py = max(2, 128 - nh - 8)
        target.paste(resized, (px, py), resized)
        frames.append(target)
        
    strip = Image.new('RGBA', (4 * 128, 128), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        strip.paste(f, (i * 128, 0), f)
        
    strip.save(dst_path)
    print("Fixed AnSi-Dash.png with 4 full intact frames!")

fix_dash_perfect()
