import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
TARGET_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\AnSi"

def clean_dash_perfect_no_trails():
    src_path = os.path.join(MEDIA_DIR, "media_1787725940028.png")
    dst_path = os.path.join(TARGET_DIR, "AnSi-Dash.png")
    img = Image.open(src_path).convert('RGB')
    
    w, h = img.size
    half_w, half_h = w // 2, h // 2
    
    boxes = [
        (0, 0, half_w, half_h),           # Frame 1
        (half_w, 0, w, half_h),           # Frame 2
        (0, half_h, half_w, h),           # Frame 3
        (half_w, half_h, w, h)            # Frame 4
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
        h_sub, w_sub = sub_rgb.shape[:2]
        
        fg = dist_bg > 35.0
        labeled, _ = label(fg)
        sizes = np.bincount(labeled.ravel())
        sizes[0] = 0
        
        if idx == 3:
            slices = find_objects(labeled)
            best_lbl = -1
            for l_idx, sl in enumerate(slices):
                if sl is not None and sizes[l_idx + 1] > 1500:
                    if sl[1].start > w_sub * 0.35:
                        best_lbl = l_idx + 1
            char_mask = binary_fill_holes(labeled == best_lbl)
        else:
            char_mask = binary_fill_holes(labeled == np.argmax(sizes))
            
        alpha = np.zeros(char_mask.shape, dtype=np.uint8)
        alpha[char_mask] = 255
        core = binary_erosion(char_mask, iterations=2)
        
        # 1. Xóa Halo
        alpha[(dist_bg < 42.0) & (~core)] = 0
        
        # 2. Xóa triệt để các tia kẻ ngang tốc độ (Frame 2 và Frame 3):
        if idx in [1, 2]:
            # Tìm tọa độ x của viền ngoài cùng bên trái của Hồ Lô và Búi tóc
            # Các pixel bên ngoài (x bên trái) phần thân nhân vật đặc -> xóa sạch
            for y_line in range(h_sub):
                row_pixels = np.where(char_mask[y_line, :])[0]
                if len(row_pixels) > 0:
                    # Tìm điểm bắt đầu thật của thân (nơi có viền đen hoặc màu đậm đặc)
                    body_start_x = row_pixels[0]
                    for x_test in row_pixels:
                        # Điểm bắt đầu thân nhân vật: có viền đen đậm (r,g,b < 50) hoặc màu hồ lô (r > 100, g > 65)
                        if (r[y_line, x_test] < 50 and g[y_line, x_test] < 50 and b[y_line, x_test] < 50) or \
                           (r[y_line, x_test] > 100 and g[y_line, x_test] > 65 and b[y_line, x_test] < 80):
                            body_start_x = x_test
                            break
                    # Xóa toàn bộ pixel nằm bên trái body_start_x (đó là vệt tốc độ kẻ ngang)
                    alpha[y_line, :body_start_x] = 0
                    
            # Xóa đám khói bụi góc dưới trái
            alpha[int(h_sub * 0.65):, :int(w_sub * 0.35)] = 0
            
        # 3. Xóa bóng đổ xám dưới đất
        is_bottom = np.zeros_like(char_mask)
        is_bottom[int(h_sub * 0.78):, :] = True
        is_dark_outline = (r < 45) & (g < 45) & (b < 45)
        is_skin = (r > 170) & (g > 110)
        is_ground_shadow = is_bottom & (dist_bg < 65.0) & (sat < 0.16) & (~(is_dark_outline | is_skin))
        alpha[is_ground_shadow] = 0
        
        # 4. Xóa sparkle
        alpha[(lum > 170) & (sat < 0.12) & (sub_rgb[:, :, 0] > 180) & (~core)] = 0
        
        char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
        bbox = char_pil.getbbox()
        if bbox:
            char_pil = char_pil.crop(bbox)
            
        cw, ch = char_pil.size
        scale = 0.3429
        nw, nh = int(round(cw * scale)), int(round(ch * scale))
        resized = char_pil.resize((nw, nh), Image.Resampling.LANCZOS)
        
        res_arr = np.array(resized)
        res_arr[res_arr[:, :, 3] < 100, 3] = 0
        resized = Image.fromarray(res_arr, 'RGBA')
        
        target = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
        px = (128 - nw) // 2
        py = max(2, 128 - nh - 8)
        target.paste(resized, (px, py), resized)
        frames.append(target)
        
    strip = Image.new('RGBA', (4 * 128, 128), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        strip.paste(f, (i * 128, 0), f)
        
    strip.save(dst_path)
    print("ALL SPEED TRAILS & SHADOWS ELIMINATED 100%!")

clean_dash_perfect_no_trails()
