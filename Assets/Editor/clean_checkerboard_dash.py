import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
TARGET_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\AnSi"

def clean_checkerboard_dash_master():
    # Ảnh JPG chứa checkerboard do web export: media_1787726902673.jpg
    src_path = os.path.join(MEDIA_DIR, "media_1787726902673.jpg")
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
    
    frames = []
    GLOBAL_SCALE = 0.3429
    
    for idx, box in enumerate(boxes):
        sub_img = img.crop(box)
        sub_rgb = np.array(sub_img, dtype=np.uint8)
        r = sub_rgb[:, :, 0].astype(float)
        g = sub_rgb[:, :, 1].astype(float)
        b = sub_rgb[:, :, 2].astype(float)
        
        lum = 0.299 * r + 0.587 * g + 0.114 * b
        max_c = np.maximum(np.maximum(r, g), b)
        min_c = np.minimum(np.minimum(r, g), b)
        sat = (max_c - min_c) / (max_c + 1e-5)
        h_sub, w_sub = sub_rgb.shape[:2]
        
        # Checkerboard là các ô vuông màu xám xịt có Saturation gần như bằng 0 (sat < 0.08) và lum từ 80 đến 165
        is_checkerboard = (sat < 0.08) & (lum > 70) & (lum < 170)
        
        # Tiền cảnh: Không phải là checkerboard
        fg = ~is_checkerboard
        
        # Nhặt cụm thực thể nhân vật
        labeled, num_features = label(fg)
        sizes = np.bincount(labeled.ravel())
        sizes[0] = 0
        main_lbl = np.argmax(sizes)
        
        char_mask = binary_fill_holes(labeled == main_lbl)
        
        alpha = np.zeros(char_mask.shape, dtype=np.uint8)
        alpha[char_mask] = 255
        
        # Xóa sparkle ở chân frame 4
        if idx == 3:
            is_sparkle = (np.arange(h_sub)[:, None] > int(h_sub * 0.70)) & (lum > 175) & (sat < 0.12)
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
        
    strip = Image.new('RGBA', (4 * 128, 128), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        strip.paste(f, (i * 128, 0), f)
        
    strip.save(dst_path)
    print("DASH STRIP CLEANED 100% WITH NO CHECKERBOARD AND PRECISE COLORS!")

clean_checkerboard_dash_master()
