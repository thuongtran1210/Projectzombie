import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, binary_erosion

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

# Định nghĩa tọa độ ROI chính xác cho từng frame trong ảnh 1024 x 765
# (x1, y1, x2, y2)
ROI_CONFIGS = {
    'Run': [
        # Hàng 1: Frame 1 (ngắn), Frame 2 (dài)
        (130, 40, 390, 330),
        (530, 80, 930, 330),
        # Hàng 2: Frame 3 (rất dài), Frame 4 (dài vừa)
        (30, 370, 490, 620),
        (530, 370, 950, 620),
        # Hàng 3 (ảnh layout 3 hàng x 2 cột): Frame 5 (dài), Frame 6 (thu lại)
        (70, 680, 480, 765),
        (660, 660, 930, 765)
    ],
    'Attack': [
        # 2 hàng x 3 cột (Cắt bỏ phần text tiếng Việt ở đáy y > 330 và y > 690)
        (40, 70, 340, 400),
        (350, 110, 600, 400),
        (670, 120, 950, 400),
        (40, 520, 340, 700),
        (340, 520, 680, 700),
        (680, 540, 950, 700)
    ]
}

def extract_custom_rois(img_name, rois, is_attack=False):
    img = Image.open(os.path.join(MATROI_DIR, img_name)).convert('RGB')
    arr = np.array(img, dtype=float)
    bg = get_bg_color(arr)
    diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
    
    crops = []
    for (x1, y1, x2, y2) in rois:
        sub_arr = arr[y1:y2, x1:x2]
        sub_diff = diff[y1:y2, x1:x2]
        
        # Ngưỡng tách
        th = 28.0
        mask = sub_diff > th
        
        # Với frame Attack 5, có vòng tròn tâm ngắm nhỏ hoặc text lọt vào, ta fill holes và xóa góc
        if is_attack:
            mask = binary_fill_holes(mask)
            
        # Tạo alpha
        alpha = np.zeros(sub_arr.shape[:2], dtype=np.uint8)
        alpha[mask] = 255
        
        # Defringe 1px
        eroded = binary_erosion(mask, iterations=1)
        edge = mask & (~eroded)
        alpha[edge] = 160
        
        rgba = np.dstack([sub_arr.astype(np.uint8), alpha])
        
        if np.sum(mask) > 0:
            pos = np.where(mask)
            min_y, max_y = np.min(pos[0]), np.max(pos[0])
            min_x, max_x = np.min(pos[1]), np.max(pos[1])
            char_crop = Image.fromarray(rgba[min_y:max_y+1, min_x:max_x+1])
        else:
            char_crop = None
        crops.append(char_crop)
    return crops

# Xử lý Run & Attack
GLOBAL_SCALE = 0.3515

for act, rois in ROI_CONFIGS.items():
    raw_f = f"raw_{act.lower()}.png" if act == 'Attack' else "raw_run.png"
    crops = extract_custom_rois(raw_f, rois, is_attack=(act=='Attack'))
    
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
        
        pos_x = i * 128 + (128 - new_w) // 2
        pos_y = (128 - new_h) // 2
        strip.paste(resized, (pos_x, pos_y), resized)
        
    out_path = os.path.join(MATROI_DIR, f"MaTroi-{act}.png")
    strip.save(out_path)
    print(f"Saved custom cropped {out_path}")

print("Run and Attack processed perfectly!")
