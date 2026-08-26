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

def perfect_clean_sprite_strip(src_name, dst_name, expected_count=6, frame_w=128, frame_h=128):
    src_path = os.path.join(MEDIA_DIR, src_name)
    dst_path = os.path.join(TARGET_DIR, dst_name)
    
    img = Image.open(src_path).convert('RGB')
    arr = np.array(img, dtype=float)
    
    # Lấy mẫu màu nền xám chuẩn xác từ các góc
    bg_color = np.median(arr[:20, :20], axis=(0, 1))
    
    # Tính khoảng cách Euclidean tới màu nền
    diff = np.sqrt(np.sum((arr - bg_color)**2, axis=2))
    
    # 1. Phát hiện đối tượng: Nền xám và bóng đổ mờ có diff thấp
    is_bg_or_shadow = diff < 30.0
    fg_mask = ~is_bg_or_shadow
    
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
        
        # Tạo alpha channel
        alpha = np.zeros(sub_mask.shape, dtype=np.uint8)
        alpha[sub_mask] = 255
        
        # Triệt tiêu Halo / Defringe:
        # Các pixel ở rìa nếu có màu gần với xám hoặc màu trắng sáng nhấp nháy ở góc phải dưới
        sub_diff = np.sqrt(np.sum((sub_rgb.astype(float) - bg_color)**2, axis=2))
        
        # Vùng lõi an toàn của nhân vật (không bao giờ bị cắt vào)
        core = binary_erosion(sub_mask, iterations=3)
        
        # Xóa các pixel xám rìa ngoài và các tia sáng lấp lánh (sparkle icon) ở góc frame
        halo_pixels = (sub_diff < 55.0) & (~core)
        
        # Xóa bóng xám dưới chân (vùng dưới cùng có màu xám đục)
        h_sub, w_sub = sub_mask.shape
        feet_area = np.zeros_like(sub_mask)
        feet_area[int(h_sub * 0.85):, :] = True
        is_gray_shadow = (sub_diff < 75.0) & feet_area & (~core)
        
        # Xóa ngôi sao sáng lấp lánh ở frame cuối (sparkle artifact)
        is_sparkle = (sub_rgb[:, :, 0] > 210) & (sub_rgb[:, :, 1] > 210) & (sub_rgb[:, :, 2] > 210) & (~core)
        
        remove_mask = (halo_pixels | is_gray_shadow | is_sparkle) & (~core)
        alpha[remove_mask] = 0
        
        char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
        
        # Cắt sát Bounding Box thật sự
        bbox = char_pil.getbbox()
        if bbox:
            char_pil = char_pil.crop(bbox)
            
        cw, ch = char_pil.size
        scale = target_char_h / float(ch)
        nw, nh = int(round(cw * scale)), int(round(ch * scale))
        
        resized = char_pil.resize((nw, nh), Image.Resampling.LANCZOS)
        
        # Dọn sạch alpha mềm mờ
        res_arr = np.array(resized)
        res_arr[res_arr[:, :, 3] < 60, 3] = 0
        resized = Image.fromarray(res_arr, 'RGBA')
        
        # Đặt vào Canvas 128x128 ghim chân tại y=8px
        target = Image.new('RGBA', (frame_w, frame_h), (0, 0, 0, 0))
        px = (frame_w - nw) // 2
        py = max(2, frame_h - nh - 8)
        target.paste(resized, (px, py), resized)
        frames.append(target)
        
    strip = Image.new('RGBA', (len(frames) * frame_w, frame_h), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        strip.paste(f, (i * frame_w, 0), f)
        
    strip.save(dst_path)
    print(f"Cleaned Perfectly: {dst_path}")

for src, dst, cnt in TASKS:
    perfect_clean_sprite_strip(src, dst, cnt)

print("ALL DONE WITHOUT WHITE PIXELS OR SHADOWS!")
