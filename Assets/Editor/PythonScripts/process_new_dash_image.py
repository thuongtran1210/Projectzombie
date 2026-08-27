import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
TARGET_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\AnSi"

def clean_dash_perfect_from_transparent_input():
    # Ảnh mới tải lên: media_1787726516119.png (978,355 bytes)
    src_path = os.path.join(MEDIA_DIR, "media_1787726516119.png")
    dst_path = os.path.join(TARGET_DIR, "AnSi-Dash.png")
    
    img = Image.open(src_path).convert('RGBA')
    w, h = img.size
    half_w, half_h = w // 2, h // 2
    
    # 4 ô tương ứng 4 góc:
    # 1. Top-Left: Cúi người thủ thế
    # 2. Top-Right: Lao phóng gậy
    # 3. Bottom-Left: Lướt tốc độ
    # 4. Bottom-Right: Tiếp đất
    boxes = [
        (0, 0, half_w, half_h),
        (half_w, 0, w, half_h),
        (0, half_h, half_w, h),
        (half_w, half_h, w, h)
    ]
    
    frames = []
    # Khóa cứng hệ số scale chuẩn
    GLOBAL_SCALE = 0.3429
    
    for idx, box in enumerate(boxes):
        sub_img = img.crop(box)
        arr = np.array(sub_img)
        r, g, b, a = arr[:, :, 0].astype(float), arr[:, :, 1].astype(float), arr[:, :, 2].astype(float), arr[:, :, 3]
        
        h_sub, w_sub = arr.shape[:2]
        
        # Nhận diện phần nhân vật thực sự qua màu sắc nét viền/quần áo/da:
        # 1. Viền đen nét đậm: r,g,b < 50
        is_dark_outline = (r < 50) & (g < 50) & (b < 50) & (a > 50)
        # 2. Da mặt/tay/chân: r > 165, g > 105
        is_skin = (r > 165) & (g > 105) & (a > 50)
        # 3. Áo nâu: r: 80..155, g: 60..125, b < 105
        is_cloth = (r > 80) & (r < 155) & (g > 60) & (g < 125) & (b < 105) & (a > 50)
        # 4. Đai đỏ: r > 140, g < 60, b < 60
        is_red = (r > 140) & (g < 60) & (b < 60) & (a > 50)
        # 5. Râu trắng: lum > 195
        lum = 0.299 * r + 0.587 * g + 0.114 * b
        is_beard = (lum > 190) & (a > 50)
        # 6. Bình hồ lô: r > 110, g > 70, b < 75
        is_gourd = (r > 105) & (g > 65) & (b < 80) & (a > 50)
        # 7. Gậy tre: r > 135, g > 110, b < 95
        is_bamboo = (r > 135) & (g > 110) & (b < 95) & (a > 50)
        
        # Tổng hợp mọi pixel thuộc thân nhân vật
        is_body = is_dark_outline | is_skin | is_cloth | is_red | is_beard | is_gourd | is_bamboo
        
        # Ở frame 4 (Bottom-Right): Chỉ lấy phần nhân vật bên phải (x > w_sub * 0.40), triệt tiêu hoàn toàn đám khói bên trái
        if idx == 3:
            is_body[:, :int(w_sub * 0.42)] = False
            
        # Lấp kín ruột nhân vật
        char_solid_mask = binary_fill_holes(is_body)
        
        # Tạo kênh Alpha mới: Chỉ cho phép hiển thị các pixel thuộc thân nhân vật đặc
        new_alpha = np.zeros((h_sub, w_sub), dtype=np.uint8)
        new_alpha[char_solid_mask] = 255
        
        # Dọn sạch các pixel nền checkerboard/xám giả mạo nếu có
        # Độ bão hòa màu:
        max_c = np.maximum(np.maximum(r, g), b)
        min_c = np.minimum(np.minimum(r, g), b)
        sat = (max_c - min_c) / (max_c + 1e-5)
        
        # Xóa sparkle ở frame 4
        is_sparkle = (lum > 170) & (sat < 0.12) & (r > 180)
        new_alpha[is_sparkle] = 0
        
        # Xóa bóng đổ dưới đáy
        is_bottom = np.zeros_like(char_solid_mask)
        is_bottom[int(h_sub * 0.78):, :] = True
        is_ground_shadow = is_bottom & (sat < 0.16) & (~(is_dark_outline | is_skin))
        new_alpha[is_ground_shadow] = 0
        
        out_sub_arr = np.dstack((arr[:, :, :3], new_alpha))
        char_pil = Image.fromarray(out_sub_arr, 'RGBA')
        
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
    print("DASH STRIP CREATED FLAWLESSLY FROM NEW INPUT!")

clean_dash_perfect_from_transparent_input()
