import os
import numpy as np
from PIL import Image
from scipy.ndimage import label, binary_fill_holes, binary_erosion

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

def extract_exact_components(img_path, is_dead=False):
    img = Image.open(img_path).convert('RGB')
    arr = np.array(img, dtype=float)
    bg = get_bg_color(arr)
    diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
    
    # Ngưỡng tách nền
    mask = diff > 26.0
    labeled, num = label(mask)
    sizes = np.bincount(labeled.ravel())
    sizes[0] = 0
    
    # Lấy các components lớn (đối với Dead thì lấy cả các cụm nhỏ > 50px)
    min_sz = 50 if is_dead else 500
    valid_lbls = np.where(sizes > min_sz)[0]
    
    # Lấy tọa độ và tâm của từng component
    comp_list = []
    for lbl in valid_lbls:
        pos = np.where(labeled == lbl)
        cy = np.mean(pos[0])
        cx = np.mean(pos[1])
        y_min, y_max = np.min(pos[0]), np.max(pos[0])
        x_min, x_max = np.min(pos[1]), np.max(pos[1])
        comp_list.append({
            'label': lbl,
            'cx': cx,
            'cy': cy,
            'bbox': (x_min, y_min, x_max, y_max),
            'pos': pos,
            'area': sizes[lbl]
        })
        
    return comp_list, arr

# 1. RUN: gồm 3 hàng x 2 cột (6 frames)
# Hàng 1: cy < 300 (2 frames, sort theo cx)
# Hàng 2: 300 <= cy < 500 (2 frames, sort theo cx)
# Hàng 3: cy >= 500 (2 frames, sort theo cx)
def process_run():
    comps, arr = extract_exact_components(os.path.join(MATROI_DIR, 'raw_run.png'))
    # Lọc 6 components lớn nhất
    comps.sort(key=lambda c: c['area'], reverse=True)
    comps = comps[:6]
    
    # Gom theo hàng
    r1 = [c for c in comps if c['cy'] < 300]
    r2 = [c for c in comps if 300 <= c['cy'] < 500]
    r3 = [c for c in comps if c['cy'] >= 500]
    
    r1.sort(key=lambda c: c['cx'])
    r2.sort(key=lambda c: c['cx'])
    r3.sort(key=lambda c: c['cx'])
    
    ordered_run = r1 + r2 + r3
    print(f"Run ordered: {len(ordered_run)} frames")
    return ordered_run, arr

# 2. ATTACK: gồm 2 hàng x 3 cột (6 frames)
# Hàng 1: cy < 380 (3 frames, sort theo cx)
# Hàng 2: cy >= 380 (3 frames, sort theo cx)
def process_attack():
    comps, arr = extract_exact_components(os.path.join(MATROI_DIR, 'raw_attack.png'))
    # Lọc các components chính (bỏ chữ tiếng Việt có area nhỏ)
    comps = [c for c in comps if c['area'] > 2000]
    
    r1 = [c for c in comps if c['cy'] < 380]
    r2 = [c for c in comps if c['cy'] >= 380]
    
    r1.sort(key=lambda c: c['cx'])
    r2.sort(key=lambda c: c['cx'])
    
    ordered_attack = r1 + r2
    print(f"Attack ordered: {len(ordered_attack)} frames")
    return ordered_attack, arr

# 3. IDLE: gồm 2 hàng x 3 cột
def process_idle():
    comps, arr = extract_exact_components(os.path.join(MATROI_DIR, 'raw_idle.png'))
    comps = [c for c in comps if c['area'] > 2000]
    r1 = [c for c in comps if c['cy'] < 380]
    r2 = [c for c in comps if c['cy'] >= 380]
    r1.sort(key=lambda c: c['cx'])
    r2.sort(key=lambda c: c['cx'])
    ordered_idle = r1 + r2
    print(f"Idle ordered: {len(ordered_idle)} frames")
    return ordered_idle, arr

# Bóc tách và render ra dải RGBA
def crop_to_strip(ordered_comps, arr, act_name, global_scale=0.3515):
    strip = Image.new("RGBA", (128 * 6, 128), (0, 0, 0, 0))
    for i, c in enumerate(ordered_comps):
        x_min, y_min, x_max, y_max = c['bbox']
        sub_arr = arr[y_min:y_max+1, x_min:x_max+1]
        
        # Mask
        bg = get_bg_color(arr)
        sub_diff = np.sqrt(np.sum((sub_arr - bg)**2, axis=2))
        mask = sub_diff > 25.0
        mask = binary_fill_holes(mask)
        
        eroded = binary_erosion(mask, iterations=1)
        edge = mask & (~eroded)
        
        alpha = np.zeros(sub_arr.shape[:2], dtype=np.uint8)
        alpha[mask] = 255
        alpha[edge] = 160
        
        rgba = np.dstack([sub_arr.astype(np.uint8), alpha])
        crop_img = Image.fromarray(rgba)
        
        new_w = int(round(crop_img.width * global_scale))
        new_h = int(round(crop_img.height * global_scale))
        
        if new_w > 124:
            s_adj = 124.0 / float(new_w)
            new_w = 124
            new_h = int(round(new_h * s_adj))
        if new_h > 124:
            s_adj = 124.0 / float(new_h)
            new_h = 124
            new_w = int(round(new_w * s_adj))
            
        resized = crop_img.resize((new_w, new_h), Image.Resampling.LANCZOS)
        
        pos_x = i * 128 + (128 - new_w) // 2
        pos_y = (128 - new_h) // 2
        strip.paste(resized, (pos_x, pos_y), resized)
        
    out_path = os.path.join(MATROI_DIR, f"MaTroi-{act_name}.png")
    strip.save(out_path)
    print(f"Generated {out_path} successfully!")

idle_comps, arr_idle = process_idle()
crop_to_strip(idle_comps, arr_idle, "Idle")

run_comps, arr_run = process_run()
crop_to_strip(run_comps, arr_run, "Run")

att_comps, arr_att = process_attack()
crop_to_strip(att_comps, arr_att, "Attack")

print("All 3 main actions processed perfectly!")
