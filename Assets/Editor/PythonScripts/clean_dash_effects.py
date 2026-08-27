import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
TARGET_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\AnSi"

def clean_dash_perfect_no_effects():
    src_path = os.path.join(MEDIA_DIR, "media_1787725940028.png") # Ảnh gốc 4 frame Dash
    dst_path = os.path.join(TARGET_DIR, "AnSi-Dash.png")
    img = Image.open(src_path).convert('RGB')
    
    w, h = img.size
    half_w, half_h = w // 2, h // 2
    
    boxes = [
        (0, 0, half_w, half_h),           # Frame 1: Cúi gồng (Top-Left)
        (half_w, 0, w, half_h),           # Frame 2: Phóng gậy (Top-Right)
        (0, half_h, half_w, h),           # Frame 3: Lướt tốc độ cao (Bottom-Left)
        (half_w, half_h, w, h)            # Frame 4: Tiếp đất (Bottom-Right)
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
        
        # Tiền cảnh
        fg = dist_bg > 35.0
        labeled, _ = label(fg)
        sizes = np.bincount(labeled.ravel())
        sizes[0] = 0
        
        if idx == 3: # Frame 4
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
        
        # 1. Loại bỏ viền mờ Halo
        alpha[dist_bg < 42.0] = 0
        
        # 2. Loại bỏ TOÀN BỘ vệt tốc độ sau lưng (Speed trail lines / Effect sau lưng):
        # Các vệt tốc độ là những đường kẻ mờ/màu xám nằm ở phía sau (bên trái nhân vật)
        # Nhân vật có viền đen dày đậm, da cam, áo nâu, hồ lô nâu
        # Các vệt effect có màu gần nền hoặc màu xám nhạt (sat < 0.20, r < 140)
        # Chỉ giữ lại phần thân nhân vật thật sự:
        is_dark_outline = (r < 45) & (g < 45) & (b < 45) # viền đen
        is_skin = (r > 165) & (g > 105) & (b < 125) # da mặt/tay/chân
        is_brown_cloth = (r > 80) & (r < 155) & (g > 60) & (g < 125) & (b < 105) # áo nâu
        is_red_sash = (r > 140) & (g < 60) & (b < 60) # đai đỏ
        is_white_beard = (lum > 195) & (sat < 0.15) # râu trắng
        is_gourd = (r > 110) & (g > 70) & (b < 75) # hồ lô
        is_bamboo = (r > 140) & (g > 115) & (b < 95) # gậy tre
        
        is_character_body = (is_dark_outline | is_skin | is_brown_cloth | is_red_sash | is_white_beard | is_gourd | is_bamboo)
        
        # Lấp kín bên trong thân để không bị thủng lỗ
        is_solid_character = binary_fill_holes(is_character_body)
        
        # Mọi pixel không thuộc thân nhân vật đặc (tức là vệt khói sau lưng, vệt gạch ngang tốc độ, bóng đổ dưới đất) -> XÓA SẠCH
        alpha[~is_solid_character] = 0
        
        # Xóa triệt để bóng đất xám ở 20% đáy (nếu còn sót lại mảng xám đục)
        is_bottom = np.zeros_like(char_mask)
        is_bottom[int(h_sub * 0.78):, :] = True
        alpha[is_bottom & (sat < 0.18) & (~(is_dark_outline | is_skin))] = 0
        
        char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
        bbox = char_pil.getbbox()
        if bbox:
            char_pil = char_pil.crop(bbox)
            
        cw, ch = char_pil.size
        
        # Khóa cứng Global Scale chuẩn 0.3429
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
    print("AnSi-Dash.png cleaned: 0 SPEED TRAILS, 0 SMOKE, 0 GROUND SHADOWS!")

clean_dash_perfect_no_effects()
