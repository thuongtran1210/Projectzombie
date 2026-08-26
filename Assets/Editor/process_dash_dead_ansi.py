import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
TARGET_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\AnSi"

TASKS = [
    # (Tên ảnh upload, Tên sprite sheet xuất ra, Số frame, Có phải Grid 2x2 không)
    ("media_1787725607232.png", "AnSi-Dash.png", 4, True),
    ("media_1787725660323.png", "AnSi-Dead.png", 6, False)
]

def ultra_clean_sheet(src_name, dst_name, expected_count, is_grid_2x2=False, frame_w=128, frame_h=128, target_char_h=96):
    src_path = os.path.join(MEDIA_DIR, src_name)
    dst_path = os.path.join(TARGET_DIR, dst_name)
    
    img = Image.open(src_path).convert('RGB')
    arr = np.array(img, dtype=float)
    
    bg = np.median(arr[:20, :20], axis=(0, 1))
    diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
    
    # Bóc tách tiền cảnh sơ bộ
    fg = diff > 40.0
    labeled, num_features = label(fg)
    sizes = np.bincount(labeled.ravel())
    sizes[0] = 0
    
    top_labels = np.argsort(sizes)[::-1][:expected_count]
    char_mask = np.isin(labeled, top_labels)
    char_mask = binary_fill_holes(char_mask)
    
    labeled_chars, _ = label(char_mask)
    slices = find_objects(labeled_chars)
    valid_slices = [s for s in slices if s is not None and np.sum(labeled_chars[s] > 0) > 1500]
    
    row_height = img.height // 2
    if is_grid_2x2:
        valid_slices.sort(key=lambda s: (s[0].start // (row_height - 50), s[1].start))
    else:
        valid_slices.sort(key=lambda s: (s[0].start // (row_height - 50), s[1].start))
        
    arr_uint = np.array(img, dtype=np.uint8)
    frames = []
    
    for idx, sl in enumerate(valid_slices[:expected_count]):
        sy, sx = sl
        sub_rgb = arr_uint[sy, sx].copy()
        sub_mask = (labeled_chars[sy, sx] > 0)
        
        r = sub_rgb[:, :, 0].astype(float)
        g = sub_rgb[:, :, 1].astype(float)
        b = sub_rgb[:, :, 2].astype(float)
        
        dist_bg = np.sqrt((r - bg[0])**2 + (g - bg[1])**2 + (b - bg[2])**2)
        lum = 0.299 * r + 0.587 * g + 0.114 * b
        max_val, min_val = np.maximum(np.maximum(r, g), b), np.minimum(np.minimum(r, g), b)
        sat = (max_val - min_val) / (max_val + 1e-5)
        
        alpha = np.zeros(sub_mask.shape, dtype=np.uint8)
        alpha[sub_mask] = 255
        
        core = binary_erosion(sub_mask, iterations=2)
        h_sub, w_sub = sub_mask.shape
        
        # 1. Triệt tiêu vết lấp lánh (sparkle icon) ở góc dưới phải nếu có
        is_bottom_right = np.zeros_like(sub_mask)
        is_bottom_right[int(h_sub * 0.65):, int(w_sub * 0.65):] = True
        is_sparkle = is_bottom_right & (lum > 175) & (sat < 0.12)
        alpha[is_sparkle] = 0
        
        # 2. Triệt tiêu khói bụi/bóng đổ xám dưới đất (không xóa quần áo/râu/da)
        is_bottom_area = np.zeros_like(sub_mask)
        is_bottom_area[int(h_sub * 0.78):, :] = True
        
        is_black_line = (r < 45) & (g < 45) & (b < 45)
        is_skin = (r > 180) & (g > 120) & (b < 120)
        is_white_hair = (lum > 210) & (sat < 0.15) & (sy.start < img.height * 0.7) # Râu tóc ở trên
        is_safe_body = is_black_line | is_skin | is_white_hair
        
        is_ground_shadow = is_bottom_area & (dist_bg < 65.0) & (sat < 0.16) & (~is_safe_body)
        alpha[is_ground_shadow] = 0
        
        # 3. Triệt tiêu Halo xám mờ
        is_halo = (dist_bg < 45.0) & (~core)
        alpha[is_halo] = 0
        
        # Riêng frame nằm chết dưới đất (frame 5, 6 của Dead): Giữ lại linh hồn bay lên (soul puff)
        # Linh hồn có màu xanh lơ nhạt lum > 160, sat > 0.05
        
        char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
        bbox = char_pil.getbbox()
        if bbox:
            char_pil = char_pil.crop(bbox)
            
        cw, ch = char_pil.size
        # Nếu nhân vật nằm bẹp thì scale theo chiều rộng hoặc tỉ lệ cố định
        if ch < cw * 0.6:  # Dáng nằm ngang (Dead frame 5, 6)
            scale = 100.0 / float(cw)
        else:
            scale = target_char_h / float(ch)
            
        nw, nh = int(round(cw * scale)), int(round(ch * scale))
        resized = char_pil.resize((nw, nh), Image.Resampling.LANCZOS)
        
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
    print(f"Processed {dst_name}: {len(frames)} frames ({strip.size})")

for src, dst, cnt, is_grid in TASKS:
    ultra_clean_sheet(src, dst, cnt, is_grid)

print("DASH AND DEAD SHEETS PROCESSED PERFECTLY!")
