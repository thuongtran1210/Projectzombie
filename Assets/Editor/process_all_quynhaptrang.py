import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
OUTPUT_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\QuyNhapTrang"
os.makedirs(OUTPUT_DIR, exist_ok=True)

ACTIONS = {
    "Idle": "media_1787795132352.png",
    "Run": "media_1787795156821.png",
    "Attack": "media_1787795208038.png",
    "Dead": "media_1787795347356.png"
}

# 1. Đo Idle Master Height (Frame 1) để lấy thước đo Master Scale
idle_img = Image.open(os.path.join(MEDIA_DIR, ACTIONS["Idle"])).convert('RGB')
idle_arr = np.array(idle_img, dtype=float)
bg_idle = np.median(idle_arr[:20, :20], axis=(0, 1))
lab_idle, _ = label(np.sqrt(np.sum((idle_arr - bg_idle)**2, axis=2)) > 35.0)
sizes_idle = np.bincount(lab_idle.ravel())
sizes_idle[0] = 0
slices_idle = find_objects(lab_idle)
valid_idle = [s for s in slices_idle if s is not None and np.sum(lab_idle[s] > 0) > 1500]
master_idle_raw_h = float(valid_idle[0][0].stop - valid_idle[0][0].start)

# GLOBAL_SCALE chuẩn cho Quỷ Nhập Tràng = 96px
GLOBAL_SCALE = 96.0 / master_idle_raw_h
print(f"QuyNhapTrang Master Idle Raw Height: {master_idle_raw_h:.2f} px, LOCKED GLOBAL_SCALE = {GLOBAL_SCALE:.4f}")

def clean_quynhaptrang_action(action_name, filename):
    src_path = os.path.join(MEDIA_DIR, filename)
    dst_path = os.path.join(OUTPUT_DIR, f"QuyNhapTrang-{action_name}.png")
    
    img = Image.open(src_path).convert('RGB')
    w, h = img.size
    bg = np.median(np.array(img)[:20, :20], axis=(0, 1))
    
    # Chia 6 box lưới 2 hàng 3 cột để đảm bảo không bị dính chữ cái / chữ số ở viền trên/dưới
    half_h = h // 2
    w1, w2 = int(w * 0.333), int(w * 0.666)
    boxes = [
        (0, 0, w1, half_h),
        (w1, 0, w2, half_h),
        (w2, 0, w, half_h),
        (0, half_h, w1, h),
        (w1, half_h, w2, h),
        (w2, half_h, w, h)
    ]
    
    frames = []
    for idx, box in enumerate(boxes):
        sub_img = img.crop(box)
        sub_rgb = np.array(sub_img, dtype=np.uint8)
        r, g, b = sub_rgb[:, :, 0].astype(float), sub_rgb[:, :, 1].astype(float), sub_rgb[:, :, 2].astype(float)
        dist_bg = np.sqrt((r - bg[0])**2 + (g - bg[1])**2 + (b - bg[2])**2)
        lum = 0.299 * r + 0.587 * g + 0.114 * b
        max_c = np.maximum(np.maximum(r, g), b)
        min_c = np.minimum(np.minimum(r, g), b)
        sat = (max_c - min_c) / (max_c + 1e-5)
        h_sub, w_sub = sub_rgb.shape[:2]
        
        # Ngưỡng tách nhân vật
        fg = dist_bg > 35.0
        
        # Lọc bỏ phần chữ chú thích (REAR BACK, CLAW SLASH, ...) ở 12% đỉnh và 12% đáy
        is_text_zone = np.zeros(fg.shape, dtype=bool)
        is_text_zone[:int(h_sub * 0.13), :] = True
        is_text_zone[int(h_sub * 0.88):, :] = True
        # Chỉ áp dụng lọc text nếu là vùng text đen mỏng rời rạc
        
        labeled, _ = label(fg)
        sizes = np.bincount(labeled.ravel())
        sizes[0] = 0
        
        # Lấy các cụm lớn của nhân vật / khói (bỏ qua cụm chữ nhỏ)
        slices = find_objects(labeled)
        valid_lbls = []
        for l_i, sl in enumerate(slices):
            if sl is not None:
                sz = sizes[l_i + 1]
                # Nếu cụm nằm hoàn toàn trong text zone và kích thước nhỏ -> Bỏ
                if sl[0].start < int(h_sub * 0.13) and sl[0].stop < int(h_sub * 0.14) and sz < 800:
                    continue
                if sl[0].start > int(h_sub * 0.88) and sz < 800:
                    continue
                if sz > (100 if action_name == "Dead" and idx == 5 else 400):
                    valid_lbls.append(l_i + 1)
                    
        char_mask = binary_fill_holes(np.isin(labeled, valid_lbls))
        
        # Gọt sạch 100% vầng sáng hào quang quanh mép
        core = binary_erosion(char_mask, iterations=2)
        alpha = np.zeros(char_mask.shape, dtype=np.uint8)
        alpha[char_mask] = 255
        alpha[(dist_bg < 48.0) & (~core)] = 0
        
        # Xóa bóng đổ xám dưới đất (22% đáy)
        if not (action_name == "Dead" and idx == 5):
            is_bottom = np.zeros_like(char_mask)
            is_bottom[int(h_sub * 0.78):, :] = True
            is_dark = (r < 45) & (g < 45) & (b < 45)
            is_skin = (r > 160) & (g > 180) & (b > 190) # da xanh tái
            is_robe = (b > 60) & (r < 70) # áo xanh chùng
            is_talisman = (r > 170) & (g > 120) & (b < 80) # bùa vàng
            is_fire_smoke = (r > 150) | (sat > 0.25) # lửa/khói
            is_safe = is_dark | is_skin | is_robe | is_talisman | is_fire_smoke
            alpha[is_bottom & (dist_bg < 68.0) & (sat < 0.16) & (~is_safe)] = 0
            
        # Xóa sparkle góc dưới phải frame 6
        if idx == 5 and action_name != "Dead":
            is_sparkle_area = np.zeros_like(char_mask)
            is_sparkle_area[int(h_sub * 0.65):, int(w_sub * 0.65):] = True
            is_sparkle = is_sparkle_area & (lum > 165) & (sat < 0.15)
            alpha[is_sparkle] = 0
            
        char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
        bbox = char_pil.getbbox()
        if bbox:
            char_pil = char_pil.crop(bbox)
        cw, ch = char_pil.size
        nw, nh = int(round(cw * GLOBAL_SCALE)), int(round(ch * GLOBAL_SCALE))
        resized = char_pil.resize((nw, nh), Image.Resampling.LANCZOS)
        res_arr = np.array(resized)
        res_arr[res_arr[:, :, 3] < 125, 3] = 0
        resized = Image.fromarray(res_arr, 'RGBA')
        target = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
        px = (128 - nw) // 2
        py = max(2, 128 - nh - 8)
        target.paste(resized, (px, py), resized)
        frames.append(target)
        
    strip = Image.new('RGBA', (len(frames) * 128, 128), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        strip.paste(f, (i * 128, 0), f)
        
    strip.save(dst_path)
    print(f"Master Clean: QuyNhapTrang-{action_name}.png ({len(frames)} frames) done!")

for act, file_name in ACTIONS.items():
    clean_quynhaptrang_action(act, file_name)
