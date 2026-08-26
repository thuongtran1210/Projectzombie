import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
TARGET_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\AnSi"

def clean_dash_perfect():
    src_path = os.path.join(MEDIA_DIR, "media_1787725940028.png")
    dst_path = os.path.join(TARGET_DIR, "AnSi-Dash.png")
    img = Image.open(src_path).convert('RGB')
    
    w, h = img.size
    half_w, half_h = w // 2, h // 2
    
    # 4 ô tương ứng 4 frame: Top-Left, Top-Right, Bottom-Left, Bottom-Right
    boxes = [
        (0, 0, half_w, half_h),
        (half_w, 0, w, half_h),
        (0, half_h, half_w, h),
        (half_w, half_h, w, h)
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
        
        # 1. Bóc tách tiền cảnh: nhân vật có dist_bg > 35
        fg = dist_bg > 35.0
        labeled, _ = label(fg)
        sizes = np.bincount(labeled.ravel())
        sizes[0] = 0
        
        if idx == 3: # Frame 4: Lấy nhân vật bên phải, bỏ khói bên trái
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
        
        # Xóa nền xám và halo ở ngoài
        core = binary_erosion(char_mask, iterations=2)
        alpha[(dist_bg < 42.0) & (~core)] = 0
        
        # 2. Xóa các vệt đường kẻ tốc độ ngang (Speed trails) sau lưng:
        # Vệt kẻ ngang nằm ở nửa bên trái x < w_sub * 0.45, độ dày rất mỏng 1-2px, có màu xám đen/nền
        if idx in [1, 2]: # Frame 2 và Frame 3 có vệt tốc độ kẻ ngang
            # Vùng phía sau lưng
            behind_area = np.zeros_like(char_mask)
            behind_area[:, :int(w_sub * 0.40)] = True
            
            # Vệt tốc độ là pixel thuộc behind_area mà gần màu nền hoặc màu xám (r < 60, g < 60, b < 60 và không nằm trong hồ lô)
            # Hồ lô có r > 110, g > 75
            is_trail = behind_area & (dist_bg < 60.0) & (~core)
            # Các tia kẻ ngang mỏng xám
            is_thin_trail = behind_area & (lum < 90) & (sat < 0.15) & (~core)
            alpha[is_trail | is_thin_trail] = 0
            
            # Xóa đám bụi nhỏ dưới chân ở frame 2 và 3
            dust_area = np.zeros_like(char_mask)
            dust_area[int(h_sub * 0.65):, :int(w_sub * 0.35)] = True
            alpha[dust_area & (dist_bg < 70.0) & (~core)] = 0
            
        # 3. Xóa bóng đổ xám dưới đất (20% đáy)
        is_bottom = np.zeros_like(char_mask)
        is_bottom[int(h_sub * 0.78):, :] = True
        is_safe_feet = (r < 50) & (g < 50) & (b < 50) | (r > 170) & (g > 110) # nét viền đen hoặc da chân
        alpha[is_bottom & (dist_bg < 65.0) & (sat < 0.16) & (~is_safe_feet)] = 0
        
        # Xóa sparkle
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
    print("Dash Cleaned Perfectly Without Degrading Character Colors!")

clean_dash_perfect()
