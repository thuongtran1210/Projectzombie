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

def clean_and_process_sheet(src_name, dst_name, expected_count=6, frame_w=128, frame_h=128):
    src_path = os.path.join(MEDIA_DIR, src_name)
    dst_path = os.path.join(TARGET_DIR, dst_name)
    
    img = Image.open(src_path).convert('RGB')
    arr = np.array(img, dtype=float)
    
    # 1. Thu thập màu nền chuẩn xác từ viền 4 cạnh của ảnh
    border_pixels = np.concatenate([
        arr[0:15, :].reshape(-1, 3),
        arr[-15:, :].reshape(-1, 3),
        arr[:, 0:15].reshape(-1, 3),
        arr[:, -15:].reshape(-1, 3)
    ])
    bg_median = np.median(border_pixels, axis=0)
    bg_std = np.std(border_pixels, axis=0) + 1e-5
    
    # 2. Tính khoảng cách màu chuẩn hóa (Mahalanobis / Euclidean mở rộng)
    diff = np.sqrt(np.sum((arr - bg_median)**2, axis=2))
    
    # Bóng đổ dưới chân có màu xám đậm hơn nền một chút hoặc viền nền mờ
    # Ngưỡng màu nền: Các pixel có màu gần với nền xám (kể cả bóng xám nhạt)
    is_bg = diff < 38.0
    
    # Tìm vùng tiền cảnh sơ bộ
    fg_mask = ~is_bg
    
    # 3. Phân cụm để lấy đúng các nhân vật chính
    labeled, num_features = label(fg_mask)
    sizes = np.bincount(labeled.ravel())
    sizes[0] = 0
    
    top_labels = np.argsort(sizes)[::-1][:expected_count]
    raw_char_mask = np.isin(labeled, top_labels)
    
    # Lấp kín thân người bên trong
    filled_mask = binary_fill_holes(raw_char_mask)
    
    # Xử lý khử viền vi mô (Defringe / Alpha Matting)
    # Lấy các lát cắt từng nhân vật
    labeled_chars, _ = label(filled_mask)
    slices = find_objects(labeled_chars)
    valid_slices = [s for s in slices if s is not None and np.sum(labeled_chars[s] > 0) > 2000]
    
    img_h, img_w = img.height, img.width
    row_height = img_h // 2
    valid_slices.sort(key=lambda s: (s[0].start // (row_height - 50), s[1].start))
    
    print(f"Refining {src_name} -> {dst_name}: {len(valid_slices)} frames")
    
    arr_uint = np.array(img, dtype=np.uint8)
    frames = []
    target_char_h = 96
    
    for idx, sl in enumerate(valid_slices[:expected_count]):
        sy, sx = sl
        sub_rgb = arr_uint[sy, sx].copy()
        sub_mask = (labeled_chars[sy, sx] > 0)
        
        # Cắt bỏ bóng đổ xám dưới đất (các pixel có độ bão hòa màu thấp ở đáy)
        # Khử viền trắng/xám mờ (Defringe border pixels)
        sub_diff = np.sqrt(np.sum((sub_rgb.astype(float) - bg_median)**2, axis=2))
        
        # Tính alpha mượt và triệt tiêu halo
        alpha = np.zeros(sub_mask.shape, dtype=np.uint8)
        alpha[sub_mask] = 255
        
        # Nếu pixel nằm ở rìa ngoài và gần màu nền hoặc màu trắng nhạt -> xóa bỏ
        border_noise = (sub_diff < 48.0) | ((sub_rgb[:, :, 0] > 200) & (sub_rgb[:, :, 1] > 200) & (sub_rgb[:, :, 2] > 200) & (sub_diff < 70.0))
        # Không xóa pixel bên trong thân (dùng erosion để bảo vệ ruột)
        inner_core = binary_erosion(sub_mask, iterations=2)
        should_remove = border_noise & (~inner_core)
        alpha[should_remove] = 0
        
        char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
        
        # Cắt sát Bounding Box thật sau khi khử viền
        bbox = char_pil.getbbox()
        if bbox:
            char_pil = char_pil.crop(bbox)
            
        cw, ch = char_pil.size
        scale = target_char_h / float(ch)
        nw, nh = int(round(cw * scale)), int(round(ch * scale))
        
        # Resize với Lanczos để giữ cạnh sắc nét không bị vệt mờ răng cưa
        resized = char_pil.resize((nw, nh), Image.Resampling.LANCZOS)
        
        # Dọn sạch alpha yếu < 30
        res_arr = np.array(resized)
        res_arr[res_arr[:, :, 3] < 40, 3] = 0
        resized = Image.fromarray(res_arr, 'RGBA')
        
        target = Image.new('RGBA', (frame_w, frame_h), (0, 0, 0, 0))
        px = (frame_w - nw) // 2
        py = max(2, frame_h - nh - 8)
        target.paste(resized, (px, py), resized)
        frames.append(target)
        
    actual_count = len(frames)
    strip = Image.new('RGBA', (actual_count * frame_w, frame_h), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        strip.paste(f, (i * frame_w, 0), f)
        
    strip.save(dst_path)
    print(f"High Quality Saved: {dst_path}")

for src, dst, cnt in TASKS:
    clean_and_process_sheet(src, dst, cnt)

print("PERFECT CLEANUP COMPLETED!")
