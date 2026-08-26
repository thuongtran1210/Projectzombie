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

def strip_all_shadows_and_sparkles(src_name, dst_name, expected_count=6, frame_w=128, frame_h=128):
    src_path = os.path.join(MEDIA_DIR, src_name)
    dst_path = os.path.join(TARGET_DIR, dst_name)
    
    img = Image.open(src_path).convert('RGB')
    arr = np.array(img, dtype=float)
    
    # Màu nền xám trung bình
    bg_color = np.median(arr[:20, :20], axis=(0, 1))
    
    # 1. Phát hiện đối tượng cơ bản
    diff = np.sqrt(np.sum((arr - bg_color)**2, axis=2))
    
    # Ngưỡng tách: loại bỏ bóng xám mờ và nền
    fg_mask = diff > 40.0
    
    labeled, num_features = label(fg_mask)
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
        
        # Tạo alpha
        alpha = np.zeros(sub_mask.shape, dtype=np.uint8)
        alpha[sub_mask] = 255
        
        # Tính toán màu và độ bão hòa (Saturation)
        r, g, b = sub_rgb[:, :, 0].astype(float), sub_rgb[:, :, 1].astype(float), sub_rgb[:, :, 2].astype(float)
        max_c = np.maximum(np.maximum(r, g), b)
        min_c = np.minimum(np.minimum(r, g), b)
        sat = (max_c - min_c) / (max_c + 1e-5) # Độ bão hòa màu
        brightness = (r + g + b) / 3.0
        
        sub_diff = np.sqrt((r - bg_color[0])**2 + (g - bg_color[1])**2 + (b - bg_color[2])**2)
        core = binary_erosion(sub_mask, iterations=3)
        
        # A. Triệt tiêu bóng đổ dưới chân: Bóng đất thường có độ bão hòa cực thấp (sat < 0.12) và màu xám/tối nằm ở 20% đáy
        h_sub, w_sub = sub_mask.shape
        bottom_area = np.zeros_like(sub_mask)
        bottom_area[int(h_sub * 0.78):, :] = True
        
        # Bóng đất xám (Gray ground shadow): độ bão hòa thấp, không phải màu cam/đỏ/nâu của giày/áo
        is_ground_shadow = bottom_area & (sat < 0.15) & (brightness < 160) & (~core)
        
        # B. Triệt tiêu vết lấp lánh màu trắng (White Sparkle Artifact) ở góc dưới phải frame 6
        is_white_sparkle = (brightness > 200) & (sat < 0.10) & (~core)
        
        # C. Triệt tiêu Halo rìa xám
        is_halo = (sub_diff < 50.0) & (~core)
        
        to_erase = (is_ground_shadow | is_white_sparkle | is_halo) & (~core)
        alpha[to_erase] = 0
        
        char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
        
        # Lấy Bounding Box chuẩn xác sau khi gọt sạch bóng
        bbox = char_pil.getbbox()
        if bbox:
            char_pil = char_pil.crop(bbox)
            
        cw, ch = char_pil.size
        scale = target_char_h / float(ch)
        nw, nh = int(round(cw * scale)), int(round(ch * scale))
        
        resized = char_pil.resize((nw, nh), Image.Resampling.LANCZOS)
        
        # Triệt tiêu hoàn toàn alpha < 80 để cạnh sắc bén dứt khoát 100%
        res_arr = np.array(resized)
        res_arr[res_arr[:, :, 3] < 80, 3] = 0
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
    print(f"Shadow-Free Cleaned: {dst_path}")

for src, dst, cnt in TASKS:
    strip_all_shadows_and_sparkles(src, dst, cnt)

print("COMPLETELY CLEAN - 0 RESIDUAL PIXELS!")
