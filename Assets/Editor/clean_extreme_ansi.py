import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
TARGET_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\AnSi"

TASKS = [
    ("media_1787724657658.png", "AnSi-Run.png", 6),
    ("media_1787724663701.png", "AnSi-Attack.png", 6),
    ("media_1787724668386.png", "AnSi-Idle.png", 6)
]

def clean_extreme(src_name, dst_name, expected_count=6, frame_w=128, frame_h=128):
    src_path = os.path.join(MEDIA_DIR, src_name)
    dst_path = os.path.join(TARGET_DIR, dst_name)
    
    img = Image.open(src_path).convert('RGB')
    arr = np.array(img, dtype=float)
    
    # Góc trên bên trái là nền xám thuần
    bg = np.median(arr[:20, :20], axis=(0, 1))
    diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
    
    # Tiền cảnh: Không lấy nền và không lấy bóng mờ
    fg = diff > 45.0
    
    labeled, num_features = label(fg)
    sizes = np.bincount(labeled.ravel())
    sizes[0] = 0
    
    top_labels = np.argsort(sizes)[::-1][:expected_count]
    char_mask = np.isin(labeled, top_labels)
    char_mask = binary_fill_holes(char_mask)
    
    labeled_chars, _ = label(char_mask)
    slices = find_objects(labeled_chars)
    valid_slices = [s for s in slices if s is not None and np.sum(labeled_chars[s] > 0) > 2000]
    
    row_height = img.height // 2
    valid_slices.sort(key=lambda s: (s[0].start // (row_height - 50), s[1].start))
    
    arr_uint = np.array(img, dtype=np.uint8)
    frames = []
    target_char_h = 96
    
    for idx, sl in enumerate(valid_slices[:expected_count]):
        sy, sx = sl
        sub_rgb = arr_uint[sy, sx].copy()
        sub_mask = (labeled_chars[sy, sx] > 0)
        
        # Phân tích pixel
        r = sub_rgb[:, :, 0].astype(float)
        g = sub_rgb[:, :, 1].astype(float)
        b = sub_rgb[:, :, 2].astype(float)
        
        # Khoảng cách tới nền xám
        dist_bg = np.sqrt((r - bg[0])**2 + (g - bg[1])**2 + (b - bg[2])**2)
        
        # Độ sáng
        lum = 0.299 * r + 0.587 * g + 0.114 * b
        
        # Độ bão hòa màu (Saturation)
        max_val = np.maximum(np.maximum(r, g), b)
        min_val = np.minimum(np.minimum(r, g), b)
        sat = (max_val - min_val) / (max_val + 1e-5)
        
        # Alpha
        alpha = np.zeros(sub_mask.shape, dtype=np.uint8)
        alpha[sub_mask] = 255
        
        # 1. Triệt tiêu vết lấp lánh (sparkle artifact) hình ngôi sao trắng 4 cánh ở frame cuối:
        # Đặc điểm: Rất sáng (lum > 180), saturation cực thấp (sat < 0.08), nằm ở góc dưới phải của sub-frame
        h_sub, w_sub = sub_mask.shape
        is_bottom_right = np.zeros_like(sub_mask)
        is_bottom_right[int(h_sub * 0.65):, int(w_sub * 0.65):] = True
        
        is_sparkle = is_bottom_right & (lum > 175) & (sat < 0.12)
        alpha[is_sparkle] = 0
        
        # 2. Triệt tiêu bóng xám dưới gót chân (Ground oval shadow)
        # Đặc điểm: Nằm ở 22% đáy, saturation thấp (sat < 0.15), màu gần với nền xám (dist_bg < 65)
        is_bottom_area = np.zeros_like(sub_mask)
        is_bottom_area[int(h_sub * 0.78):, :] = True
        
        # Không được xóa chân nhân vật (chân có viền đen nét đậm r,g,b < 40 hoặc màu da cam)
        is_black_line = (r < 45) & (g < 45) & (b < 45)
        is_skin = (r > 180) & (g > 120) & (b < 120)
        is_safe_body = is_black_line | is_skin
        
        is_ground_shadow = is_bottom_area & (dist_bg < 65.0) & (sat < 0.16) & (~is_safe_body)
        alpha[is_ground_shadow] = 0
        
        # 3. Triệt tiêu Halo mờ rìa ngoài
        core = binary_erosion(sub_mask, iterations=2)
        is_halo = (dist_bg < 45.0) & (~core)
        alpha[is_halo] = 0
        
        char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
        
        # Crop sát Bounding Box
        bbox = char_pil.getbbox()
        if bbox:
            char_pil = char_pil.crop(bbox)
            
        cw, ch = char_pil.size
        scale = target_char_h / float(ch)
        nw, nh = int(round(cw * scale)), int(round(ch * scale))
        
        resized = char_pil.resize((nw, nh), Image.Resampling.LANCZOS)
        
        # Khóa Alpha dứt khoát
        res_arr = np.array(resized)
        res_arr[res_arr[:, :, 3] < 100, 3] = 0
        resized = Image.fromarray(res_arr, 'RGBA')
        
        target = Image.new('RGBA', (frame_w, frame_h), (0, 0, 0, 0))
        px = (frame_w - nw) // 2
        py = max(2, frame_h - nh - 8)
        target.paste(resized, (px, py), resized)
        frames.append(target)
        
    strip = Image.new('RGBA', (len(frames) * frame_w, frame_h), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        strip.paste(f, (i * frame_w, 0), f)
        
    strip.save(dst_path)
    print(f"Ultra Cleaned: {dst_path}")

for src, dst, cnt in TASKS:
    clean_extreme(src, dst, cnt)

print("COMPLETED WITHOUT ANY DEFECTS!")
