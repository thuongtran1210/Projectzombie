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

def extract_raw_crops(img_path, is_dead=False, is_attack=False):
    img = Image.open(img_path).convert('RGB')
    arr = np.array(img, dtype=float)
    h, w, _ = arr.shape
    
    bg = get_bg_color(arr)
    diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
    
    row_h = h // 2
    col_w = w // 3
    
    crops = []
    
    for r in range(2):
        for c in range(3):
            # Với frame F6 của Dead (r=1, c=2), tạo frame trống
            if is_dead and r == 1 and c == 2:
                crops.append(None)
                continue
                
            y1, y2 = r * row_h, (r + 1) * row_h
            x1, x2 = c * col_w, (c + 1) * col_w
            
            # Cắt bớt phần text chú thích ở dưới nếu có
            y2_clean = y2 - int(row_h * 0.16)
            
            sub_arr = arr[y1:y2_clean, x1:x2]
            sub_diff = diff[y1:y2_clean, x1:x2]
            
            # Ngưỡng tách nền
            th = 20.0 if is_dead else 28.0
            mask = sub_diff > th
            
            # Xóa các thành phần nhỏ góc viền
            labeled, num = label(mask)
            if num > 0:
                sizes = np.bincount(labeled.ravel())
                sizes[0] = 0
                if not is_dead and not is_attack:
                    max_lbl = np.argmax(sizes)
                    mask = (labeled == max_lbl)
                    mask = binary_fill_holes(mask)
                elif is_attack:
                    # Lấy các mảng > 40px
                    valid_lbls = np.where(sizes > 40)[0]
                    mask = np.isin(labeled, valid_lbls)
                    mask = binary_fill_holes(mask)
                else: # is_dead
                    valid_lbls = np.where(sizes > 10)[0]
                    mask = np.isin(labeled, valid_lbls)
            
            # Tạo kênh RGBA
            sub_uint8 = sub_arr.astype(np.uint8)
            alpha = np.zeros((sub_arr.shape[0], sub_arr.shape[1]), dtype=np.uint8)
            alpha[mask] = 255
            
            # Defringing 1 pixel ở rìa ngoài để loại bỏ viền xám
            eroded = binary_erosion(mask, iterations=1)
            edge = mask & (~eroded)
            # Giảm alpha ở viền
            alpha[edge] = np.clip(alpha[edge] * 0.5, 0, 255).astype(np.uint8)
            
            rgba = np.dstack([sub_uint8, alpha])
            
            if np.sum(mask) > 0:
                pos = np.where(mask)
                min_y, max_y = np.min(pos[0]), np.max(pos[0])
                min_x, max_x = np.min(pos[1]), np.max(pos[1])
                char_crop = Image.fromarray(rgba[min_y:max_y+1, min_x:max_x+1])
            else:
                char_crop = None
                
            crops.append(char_crop)
            
    return crops

# 1. Trích xuất Idle để tính Master Height
idle_crops = extract_raw_crops(os.path.join(MATROI_DIR, "raw_idle.png"))
# Frame 1 của Idle
f0 = idle_crops[0]
idle_master_h = f0.height
print(f"Idle Master Height: {idle_master_h} px, Width: {f0.width} px")

# Target nhân vật Chibi trong canvas 128x128 là ~88px chiều cao (để chừa đuôi lửa bay)
GLOBAL_SCALE = 84.0 / float(idle_master_h)
print(f"GLOBAL_SCALE: {GLOBAL_SCALE:.4f}")

actions_info = [
    ("Idle", "raw_idle.png", False, False),
    ("Run", "raw_run.png", False, False),
    ("Attack", "raw_attack.png", False, True),
    ("Dead", "raw_dead.png", True, False)
]

for act_name, raw_file, is_d, is_att in actions_info:
    crops = extract_raw_crops(os.path.join(MATROI_DIR, raw_file), is_dead=is_d, is_attack=is_att)
    
    strip = Image.new("RGBA", (128 * 6, 128), (0, 0, 0, 0))
    
    for i, crop in enumerate(crops):
        if crop is None:
            continue
            
        new_w = max(1, int(round(crop.width * GLOBAL_SCALE)))
        new_h = max(1, int(round(crop.height * GLOBAL_SCALE)))
        
        # Resample Lanczos
        resized = crop.resize((new_w, new_h), Image.Resampling.LANCZOS)
        
        # Đặt vào tâm canvas 128x128
        # Đối với Ma Trơi (bay lơ lửng), đặt tâm Y ở giữa (y = 64)
        pos_x = i * 128 + (128 - new_w) // 2
        # Căn chỉnh Y bồng bềnh
        pos_y = (128 - new_h) // 2
        
        strip.paste(resized, (pos_x, pos_y), resized)
        
    out_path = os.path.join(MATROI_DIR, f"MaTroi-{act_name}.png")
    strip.save(out_path)
    print(f"Generated {out_path} successfully!")

print("All 4 MaTroi strips generated perfectly!")
