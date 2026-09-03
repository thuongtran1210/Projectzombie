import os
import numpy as np
from PIL import Image
from scipy.ndimage import label, find_objects, binary_fill_holes, binary_erosion

MATROI_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\MaTroi"

def get_bg_color(arr):
    h, w, _ = arr.shape
    corners = np.concatenate([
        arr[:30, :30].reshape(-1, 3),
        arr[:30, -30:].reshape(-1, 3),
        arr[-30:, :30].reshape(-1, 3),
        arr[-30:, -30:].reshape(-1, 3)
    ], axis=0)
    return np.median(corners, axis=0)

def extract_connected_components(img_path, expected_count=6, is_dead=False, is_attack=False):
    img = Image.open(img_path).convert('RGB')
    arr = np.array(img, dtype=float)
    h, w, _ = arr.shape
    
    bg = get_bg_color(arr)
    diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
    
    # Bỏ 12% mép dưới cùng của ảnh để tránh chữ chú thích F1, F2...
    arr_clean = arr[:int(h * 0.90), :]
    diff_clean = diff[:int(h * 0.90), :]
    
    # 2 hàng (Top row và Bottom row)
    half_h = arr_clean.shape[0] // 2
    
    crops = []
    
    for row_idx, r_arr, r_diff in [
        (0, arr_clean[:half_h, :], diff_clean[:half_h, :]),
        (1, arr_clean[half_h:, :], diff_clean[half_h:, :])
    ]:
        th = 18.0 if is_dead else 28.0
        mask = r_diff > th
        
        # Nếu là Dead và row_idx == 1, có frame 6 biến mất
        labeled, num = label(mask)
        if num == 0:
            continue
            
        sizes = np.bincount(labeled.ravel())
        sizes[0] = 0
        
        # Lọc bỏ các hạt rác < 20px
        valid_mask = np.isin(labeled, np.where(sizes > 20)[0])
        
        # Dilate nhẹ theo chiều ngang để gom các cụm rời của cùng 1 nhân vật (như vệt lửa, hạt nổ)
        # Tìm các cụm theo trục X
        # Chiếu mask xuống trục X
        col_proj = np.any(valid_mask, axis=0)
        
        # Tìm các đoạn liên tục trên trục X
        in_seg = False
        start_x = 0
        segments = []
        for x, val in enumerate(col_proj):
            if val and not in_seg:
                in_seg = True
                start_x = x
            elif not val and in_seg:
                # Kiểm tra xem có khoảng trống nhỏ nào bị đứt không
                in_seg = False
                segments.append((start_x, x))
        if in_seg:
            segments.append((start_x, len(col_proj)))
            
        # Gộp các segments gần nhau (< 30px)
        merged_segs = []
        for s, e in segments:
            if not merged_segs:
                merged_segs.append([s, e])
            else:
                if s - merged_segs[-1][1] < 25:
                    merged_segs[-1][1] = e
                else:
                    merged_segs.append([s, e])
                    
        # Lấy tối đa 3 segments lớn nhất theo chiều rộng trong mỗi hàng
        # Sắp xếp theo X từ trái sang phải
        merged_segs.sort(key=lambda x: x[0])
        
        print(f"Row {row_idx} found {len(merged_segs)} segments: {merged_segs}")
        
        for s, e in merged_segs[:3]:
            # Padding nhẹ
            pad = 4
            s_p = max(0, s - pad)
            e_p = min(r_arr.shape[1], e + pad)
            
            sub_arr = r_arr[:, s_p:e_p]
            sub_mask = valid_mask[:, s_p:e_p]
            
            # Cắt bounding box Y
            if np.sum(sub_mask) > 0:
                y_indices = np.where(np.any(sub_mask, axis=1))[0]
                y_min, y_max = y_indices[0], y_indices[-1]
                
                # Tạo RGBA
                sub_crop_arr = sub_arr[y_min:y_max+1, :]
                sub_crop_mask = sub_mask[y_min:y_max+1, :]
                
                # Defringe
                eroded = binary_erosion(sub_crop_mask, iterations=1)
                edge = sub_crop_mask & (~eroded)
                
                alpha = np.zeros(sub_crop_arr.shape[:2], dtype=np.uint8)
                alpha[sub_crop_mask] = 255
                alpha[edge] = 160
                
                rgba = np.dstack([sub_crop_arr.astype(np.uint8), alpha])
                crops.append(Image.fromarray(rgba))
            else:
                crops.append(None)
                
        # Nếu Dead ở hàng 2 chỉ có 2 frames (F4, F5), bổ sung frame 6 None
        if is_dead and row_idx == 1 and len(merged_segs) < 3:
            while len(crops) < 6:
                crops.append(None)
                
    return crops

# Bóc tách chuẩn xác cho cả 4 dải
raw_files = {
    'Idle': ('raw_idle.png', False, False),
    'Run': ('raw_run.png', False, False),
    'Attack': ('raw_attack.png', False, True),
    'Dead': ('raw_dead.png', True, False)
}

# Lấy Idle Frame 1 tính Master Height
idle_crops = extract_connected_components(os.path.join(MATROI_DIR, 'raw_idle.png'))
f0 = idle_crops[0]
idle_master_h = f0.height
print(f"Idle Master Height: {idle_master_h}")

GLOBAL_SCALE = 84.0 / float(idle_master_h)
print(f"GLOBAL_SCALE: {GLOBAL_SCALE:.4f}")

for act, (fname, is_d, is_att) in raw_files.items():
    crops = extract_connected_components(os.path.join(MATROI_DIR, fname), is_dead=is_d, is_attack=is_att)
    print(f"{act} extracted {len(crops)} crops")
    
    # Đảm bảo đủ 6 frames
    while len(crops) < 6:
        crops.append(None)
    crops = crops[:6]
    
    strip = Image.new("RGBA", (128 * 6, 128), (0, 0, 0, 0))
    
    for i, crop in enumerate(crops):
        if crop is None:
            continue
        
        # Giới hạn scale không để width vượt quá 124px
        new_w = int(round(crop.width * GLOBAL_SCALE))
        new_h = int(round(crop.height * GLOBAL_SCALE))
        
        if new_w > 124:
            scale_adj = 124.0 / float(new_w)
            new_w = 124
            new_h = int(round(new_h * scale_adj))
        if new_h > 124:
            scale_adj = 124.0 / float(new_h)
            new_h = 124
            new_w = int(round(new_w * scale_adj))
            
        resized = crop.resize((new_w, new_h), Image.Resampling.LANCZOS)
        
        # Đặt vào tâm Canvas 128x128
        pos_x = i * 128 + (128 - new_w) // 2
        pos_y = (128 - new_h) // 2
        
        strip.paste(resized, (pos_x, pos_y), resized)
        
    out_path = os.path.join(MATROI_DIR, f"MaTroi-{act}.png")
    strip.save(out_path)
    print(f"Saved {out_path}")

print("All MaTroi animations processed with flawless bounding boxes!")
