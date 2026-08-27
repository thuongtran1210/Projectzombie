import os
import numpy as np
from PIL import Image
from scipy.ndimage import label, binary_fill_holes, binary_erosion

TARGET_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\HoaHoly"

def get_bg_color(arr):
    h, w, _ = arr.shape
    corners = np.concatenate([
        arr[:30, :30].reshape(-1, 3),
        arr[:30, -30:].reshape(-1, 3),
        arr[-30:, :30].reshape(-1, 3),
        arr[-30:, -30:].reshape(-1, 3)
    ], axis=0)
    return np.median(corners, axis=0)

# Kích thước chiều cao chuẩn của nhân vật trong canvas 128x128 là ~88px (để chừa đuôi và tai)
# Idle Frame 1 nguyên gốc có full height = 228px
GLOBAL_SCALE = 88.0 / 228.0  # = 0.38596
print(f"Master GLOBAL_SCALE: {GLOBAL_SCALE:.4f}")

def process_action_strip(raw_filename, act_name, is_dead=False):
    img_path = os.path.join(TARGET_DIR, raw_filename)
    img = Image.open(img_path).convert('RGB')
    arr = np.array(img, dtype=float)
    h, w, _ = arr.shape
    bg = get_bg_color(arr)
    diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
    
    # 6 ROIs chính xác cho 2 hàng x 3 cột (Layout 16:9)
    # Hàng 1: y: 30..280 | Hàng 2: y: 290..550
    # Cột 1: x: 40..330 | Cột 2: x: 340..660 | Cột 3: x: 670..980
    rois = [
        (40, 30, 330, 280),
        (340, 30, 660, 280),
        (670, 30, 980, 280),
        (40, 290, 340, 550),
        (340, 290, 660, 550),
        (670, 290, 980, 550)
    ]
    
    crops = []
    for idx, (x1, y1, x2, y2) in enumerate(rois):
        sub_arr = arr[y1:y2, x1:x2]
        sub_diff = diff[y1:y2, x1:x2]
        
        mask = sub_diff > 25.0
        
        # Xóa sparkle ở góc dưới phải frame 6 nếu có
        if idx == 5:
            # Xóa cụm pixel độc lập ở góc ngoài
            labeled, num = label(mask)
            if num > 0:
                sizes = np.bincount(labeled.ravel())
                sizes[0] = 0
                if not is_dead:
                    # Lấy component lớn nhất là thân Hồ Ly
                    max_lbl = np.argmax(sizes)
                    mask = (labeled == max_lbl)
                else:
                    # Với Dead frame 6 có bóng ma nhỏ bay lên ở x: 853..916
                    # Giữ cả thân nằm và hồn ma (các cụm > 50px)
                    valid_lbls = np.where(sizes > 50)[0]
                    mask = np.isin(labeled, valid_lbls)
        else:
            labeled, num = label(mask)
            if num > 0:
                sizes = np.bincount(labeled.ravel())
                sizes[0] = 0
                valid_lbls = np.where(sizes > 50)[0]
                mask = np.isin(labeled, valid_lbls)
                
        mask = binary_fill_holes(mask)
        
        # Defringing nhẹ 1px biên
        eroded = binary_erosion(mask, iterations=1)
        edge = mask & (~eroded)
        
        alpha = np.zeros(sub_arr.shape[:2], dtype=np.uint8)
        alpha[mask] = 255
        alpha[edge] = 160
        
        rgba = np.dstack([sub_arr.astype(np.uint8), alpha])
        
        if np.sum(mask) > 0:
            pos = np.where(mask)
            min_y, max_y = np.min(pos[0]), np.max(pos[0])
            min_x, max_x = np.min(pos[1]), np.max(pos[1])
            crops.append(Image.fromarray(rgba[min_y:max_y+1, min_x:max_x+1]))
        else:
            crops.append(None)
            
    # Tạo dải strip 768 x 128 (6 ô 128x128)
    strip = Image.new("RGBA", (128 * 6, 128), (0, 0, 0, 0))
    for i, crop in enumerate(crops):
        if crop is None:
            continue
            
        new_w = int(round(crop.width * GLOBAL_SCALE))
        new_h = int(round(crop.height * GLOBAL_SCALE))
        
        if new_w > 124:
            s_adj = 124.0 / float(new_w)
            new_w = 124
            new_h = int(round(new_h * s_adj))
        if new_h > 124:
            s_adj = 124.0 / float(new_h)
            new_h = 124
            new_w = int(round(new_w * s_adj))
            
        resized = crop.resize((new_w, new_h), Image.Resampling.LANCZOS)
        
        # Bám chân đất (Bottom-aligned): đặt chân tại y_bottom = 120 (cách đáy 8px)
        pos_x = i * 128 + (128 - new_w) // 2
        pos_y = 120 - new_h
        if pos_y < 2:
            pos_y = 2
            
        strip.paste(resized, (pos_x, pos_y), resized)
        
    out_path = os.path.join(TARGET_DIR, f"HoaLyTinh-{act_name}.png")
    strip.save(out_path)
    print(f"Generated {out_path}")
    
    # Xuất ra IndividualFrames
    out_frames_dir = os.path.join(TARGET_DIR, "IndividualFrames")
    os.makedirs(out_frames_dir, exist_ok=True)
    for i in range(6):
        frame = strip.crop((i * 128, 0, (i + 1) * 128, 128))
        frame.save(os.path.join(out_frames_dir, f"{act_name}_{i}.png"))

actions = [
    ("raw_idle.png", "Idle", False),
    ("raw_run.png", "Run", False),
    ("raw_attack.png", "Attack", False),
    ("raw_dead.png", "Dead", True)
]

for raw_f, act, is_d in actions:
    process_action_strip(raw_f, act, is_dead=is_d)

print("All 4 HoaLyTinh action strips processed flawlessly!")
