import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
TARGET_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\AnSi"

os.makedirs(TARGET_DIR, exist_ok=True)

# Map các file ảnh upload với hành động tương ứng:
# media_1787724657658.png -> Run (781,036 bytes)
# media_1787724663701.png -> Attack (774,251 bytes)
# media_1787724668386.png -> Idle (720,474 bytes)

TASKS = [
    ("media_1787724657658.png", "AnSi-Run.png", 6),
    ("media_1787724663701.png", "AnSi-Attack.png", 6),
    ("media_1787724668386.png", "AnSi-Idle.png", 6)
]

def process_grid_to_strip(src_name, dst_name, expected_count=6, frame_w=128, frame_h=128):
    src_path = os.path.join(MEDIA_DIR, src_name)
    dst_path = os.path.join(TARGET_DIR, dst_name)
    
    if not os.path.exists(src_path):
        print(f"File not found: {src_path}")
        return
        
    img = Image.open(src_path).convert('RGB')
    arr = np.array(img, dtype=float)
    
    # Lấy màu nền từ 4 góc ảnh để tính màu nền trung bình
    corners = np.array([arr[0, 0], arr[0, -1], arr[-1, 0], arr[-1, -1]])
    bg = np.median(corners, axis=0)
    
    # Tính khoảng cách màu
    diff = np.sqrt(np.sum((arr - bg) ** 2, axis=2))
    is_bg = diff < 20.0  # Ngưỡng màu nền xám
    is_fg = binary_fill_holes(~is_bg)
    
    # Phân cụm các đối tượng nhân vật (Connected Components)
    labeled, num_features = label(is_fg)
    sizes = np.bincount(labeled.ravel())
    sizes[0] = 0  # Bỏ nền
    
    # Lấy top các cụm lớn nhất tương ứng với số frame
    top_labels = np.argsort(sizes)[::-1][:expected_count]
    clean_mask = binary_fill_holes(np.isin(labeled, top_labels))
    labeled_clean, _ = label(clean_mask)
    slices = find_objects(labeled_clean)
    
    # Lọc slices hợp lệ
    valid_slices = [s for s in slices if s is not None and np.sum(labeled_clean[s] > 0) > 2000]
    
    # Sắp xếp theo thứ tự đọc: từ trên xuống dưới, từ trái sang phải
    img_h, img_w = img.height, img.width
    row_height = img_h // 2
    
    # Sắp xếp: chia theo hàng trước, sau đó sắp xếp theo cột x
    valid_slices.sort(key=lambda s: (s[0].start // (row_height - 50), s[1].start))
    
    print(f"Processing {src_name} -> {dst_name}: Found {len(valid_slices)} frames (expected {expected_count})")
    
    arr_uint = np.array(img, dtype=np.uint8)
    frames = []
    
    # Scale đồng bộ theo chiều cao nhân vật chuẩn
    target_char_h = 96
    
    for idx, sl in enumerate(valid_slices[:expected_count]):
        sy, sx = sl
        sub_rgb = arr_uint[sy, sx]
        sub_mask = (labeled_clean[sy, sx] > 0).astype(np.uint8) * 255
        char_pil = Image.fromarray(np.dstack((sub_rgb, sub_mask)), 'RGBA')
        
        cw, ch = char_pil.size
        scale = target_char_h / float(ch)
        nw, nh = int(round(cw * scale)), int(round(ch * scale))
        resized = char_pil.resize((nw, nh), Image.Resampling.NEAREST)
        
        # Đặt vào khung frame 128x128 và ghim gót chân y = 8px tính từ đáy
        target = Image.new('RGBA', (frame_w, frame_h), (0, 0, 0, 0))
        px = (frame_w - nw) // 2
        py = max(2, frame_h - nh - 8)
        target.paste(resized, (px, py), resized)
        frames.append(target)
        
    # Ghép thành dải Strip nằm ngang
    actual_count = len(frames)
    strip = Image.new('RGBA', (actual_count * frame_w, frame_h), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        strip.paste(f, (i * frame_w, 0), f)
        
    strip.save(dst_path)
    print(f"Saved: {dst_path} ({strip.size})")

for src, dst, cnt in TASKS:
    process_grid_to_strip(src, dst, cnt)

print("ALL PROCESSING COMPLETED SUCCESSFULLY!")
