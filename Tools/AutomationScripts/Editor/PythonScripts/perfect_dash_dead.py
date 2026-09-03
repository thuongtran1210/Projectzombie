import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
TARGET_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\AnSi"

def clean_dash_perfect():
    src_path = os.path.join(MEDIA_DIR, "media_1787725607232.png")
    dst_path = os.path.join(TARGET_DIR, "AnSi-Dash.png")
    img = Image.open(src_path).convert('RGB')
    
    # 4 góc tương ứng 4 frame của Dash (Grid 2x2)
    w, h = img.size
    half_w, half_h = w // 2, h // 2
    
    # Thứ tự 4 frame: Top-Left, Top-Right, Bottom-Left, Bottom-Right
    boxes = [
        (0, 0, half_w, half_h),
        (half_w, 0, w, half_h),
        (0, half_h, half_w, h),
        (half_w, half_h, w, h)
    ]
    
    bg = np.median(np.array(img)[:20, :20], axis=(0, 1))
    frames = []
    
    for idx, box in enumerate(boxes):
        crop_img = img.crop(box)
        sub_rgb = np.array(crop_img, dtype=np.uint8)
        r, g, b = sub_rgb[:, :, 0].astype(float), sub_rgb[:, :, 1].astype(float), sub_rgb[:, :, 2].astype(float)
        
        dist_bg = np.sqrt((r - bg[0])**2 + (g - bg[1])**2 + (b - bg[2])**2)
        lum = 0.299 * r + 0.587 * g + 0.114 * b
        max_val, min_val = np.maximum(np.maximum(r, g), b), np.minimum(np.minimum(r, g), b)
        sat = (max_val - min_val) / (max_val + 1e-5)
        
        # Tiền cảnh
        fg = dist_bg > 35.0
        labeled, _ = label(fg)
        sizes = np.bincount(labeled.ravel())
        sizes[0] = 0
        main_label = np.argmax(sizes)
        char_mask = binary_fill_holes(labeled == main_label)
        
        alpha = np.zeros(char_mask.shape, dtype=np.uint8)
        alpha[char_mask] = 255
        
        core = binary_erosion(char_mask, iterations=2)
        h_sub, w_sub = char_mask.shape
        
        # Xóa khói bụi lớn và sparkle ở frame 4
        # Khói bụi có màu nâu nhạt/xám, sat thấp, nằm ở góc dưới bên trái
        is_smoke = (dist_bg < 60.0) | ((lum > 140) & (sat < 0.20) & (r < 180))
        # Không xóa phần thân nhân vật
        is_safe = (r < 50) & (g < 50) & (b < 50) # nét viền đen
        is_safe |= (r > 170) & (g > 110) & (b < 120) # da
        is_safe |= (r > 90) & (r < 140) & (g > 70) & (g < 110) # áo nâu
        
        # Xóa đám khói độc lập ở frame 4
        if idx == 3: # Frame 4
            # Chỉ giữ lại nhân vật đứng bên phải, xóa đám khói bên trái x < w_sub * 0.55
            is_left_smoke = np.zeros_like(char_mask)
            is_left_smoke[:, :int(w_sub * 0.55)] = True
            alpha[is_left_smoke & (~is_safe)] = 0
            
        alpha[(dist_bg < 45.0) & (~core)] = 0
        
        # Xóa sparkle ở chân
        is_sparkle = (lum > 170) & (sat < 0.12) & (sub_rgb[:, :, 0] > 180) & (~core)
        alpha[is_sparkle] = 0
        
        char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
        bbox = char_pil.getbbox()
        if bbox:
            char_pil = char_pil.crop(bbox)
            
        cw, ch = char_pil.size
        scale = 96.0 / float(ch)
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
        
    strip = Image.new('RGBA', (len(frames) * 128, 128), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        strip.paste(f, (i * 128, 0), f)
    strip.save(dst_path)
    print("Dash Strip Cleaned 100% Perfectly!")

def clean_dead_perfect():
    src_path = os.path.join(MEDIA_DIR, "media_1787725660323.png")
    dst_path = os.path.join(TARGET_DIR, "AnSi-Dead.png")
    img = Image.open(src_path).convert('RGB')
    
    bg = np.median(np.array(img)[:20, :20], axis=(0, 1))
    arr = np.array(img, dtype=float)
    diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
    
    fg = diff > 35.0
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
    
    for idx, sl in enumerate(valid_slices[:6]):
        sy, sx = sl
        sub_rgb = arr_uint[sy, sx].copy()
        sub_mask = (labeled_chars[sy, sx] > 0)
        
        r, g, b = sub_rgb[:, :, 0].astype(float), sub_rgb[:, :, 1].astype(float), sub_rgb[:, :, 2].astype(float)
        dist_bg = np.sqrt((r - bg[0])**2 + (g - bg[1])**2 + (b - bg[2])**2)
        lum = 0.299 * r + 0.587 * g + 0.114 * b
        max_val, min_val = np.maximum(np.maximum(r, g), b), np.minimum(np.minimum(r, g), b)
        sat = (max_val - min_val) / (max_val + 1e-5)
        
        alpha = np.zeros(sub_mask.shape, dtype=np.uint8)
        alpha[sub_mask] = 255
        core = binary_erosion(sub_mask, iterations=2)
        
        # Xóa sparkle ở chân frame 6
        h_sub, w_sub = sub_mask.shape
        is_bottom_right = np.zeros_like(sub_mask)
        is_bottom_right[int(h_sub * 0.65):, int(w_sub * 0.75):] = True
        is_sparkle = is_bottom_right & (lum > 175) & (sat < 0.12)
        alpha[is_sparkle] = 0
        
        # Xóa bóng xám dưới đất (không xóa linh hồn bay màu xanh lơ ở góc trên)
        is_bottom_area = np.zeros_like(sub_mask)
        is_bottom_area[int(h_sub * 0.80):, :] = True
        is_safe = (r < 50) & (g < 50) & (b < 50) | (r > 170) & (g > 110) # viền đen hoặc da
        is_ground_shadow = is_bottom_area & (dist_bg < 60.0) & (sat < 0.16) & (~is_safe)
        alpha[is_ground_shadow] = 0
        
        # Xóa halo
        alpha[(dist_bg < 45.0) & (~core)] = 0
        
        char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
        bbox = char_pil.getbbox()
        if bbox:
            char_pil = char_pil.crop(bbox)
            
        cw, ch = char_pil.size
        if ch < cw * 0.65: # Nằm bẹp
            scale = 100.0 / float(cw)
        else:
            scale = 96.0 / float(ch)
            
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
        
    strip = Image.new('RGBA', (len(frames) * 128, 128), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        strip.paste(f, (i * 128, 0), f)
    strip.save(dst_path)
    print("Dead Strip Cleaned 100% Perfectly!")

clean_dash_perfect()
clean_dead_perfect()
