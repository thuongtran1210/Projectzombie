import os
import numpy as np
from PIL import Image
from scipy.ndimage import binary_fill_holes, label, find_objects, binary_erosion

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
OUTPUT_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\Madoino"

ACTIONS = {
    "Idle": "media_1787745188145.png",
    "Run": "media_1787745259142.png",
    "Attack": "media_1787745317621.png",
    "Dead": "media_1787745367089.png"
}

GLOBAL_SCALE = 0.3117

def clean_perfect_madoino(action_name, filename):
    src_path = os.path.join(MEDIA_DIR, filename)
    dst_path = os.path.join(OUTPUT_DIR, f"Madoino-{action_name}.png")
    
    img = Image.open(src_path).convert('RGB')
    w, h = img.size
    bg = np.median(np.array(img)[:20, :20], axis=(0, 1))
    
    if action_name == "Dead":
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
            
            threshold = 28.0 if idx >= 2 else 35.0
            fg = dist_bg > threshold
            labeled, _ = label(fg)
            sizes = np.bincount(labeled.ravel())
            sizes[0] = 0
            
            if idx == 1:
                slices = find_objects(labeled)
                valid_lbls = []
                for l_i, sl in enumerate(slices):
                    if sl is not None and sizes[l_i + 1] > 200:
                        if sl[1].start < int(sub_rgb.shape[1] * 0.85):
                            valid_lbls.append(l_i + 1)
                char_mask = binary_fill_holes(np.isin(labeled, valid_lbls))
            else:
                top_lbls = [l_i for l_i, sz in enumerate(sizes) if sz > (50 if idx >= 2 else 300)]
                char_mask = binary_fill_holes(np.isin(labeled, top_lbls))
                
            # Xóa sạch viền hào quang xám mờ (Gọt chặt viền 2 pixel)
            core = binary_erosion(char_mask, iterations=2)
            alpha = np.zeros(char_mask.shape, dtype=np.uint8)
            alpha[char_mask] = 255
            alpha[(dist_bg < 45.0) & (~core)] = 0
            
            char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
            bbox = char_pil.getbbox()
            if bbox:
                char_pil = char_pil.crop(bbox)
            cw, ch = char_pil.size
            nw, nh = int(round(cw * GLOBAL_SCALE)), int(round(ch * GLOBAL_SCALE))
            resized = char_pil.resize((nw, nh), Image.Resampling.LANCZOS)
            res_arr = np.array(resized)
            res_arr[res_arr[:, :, 3] < 120, 3] = 0
            resized = Image.fromarray(res_arr, 'RGBA')
            target = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
            px = (128 - nw) // 2
            py = max(2, 128 - nh - 8)
            target.paste(resized, (px, py), resized)
            frames.append(target)
            
    else:
        # Idle, Run, Attack
        arr = np.array(img, dtype=float)
        diff = np.sqrt(np.sum((arr - bg)**2, axis=2))
        
        # Ngưỡng bắt đầu tách tiền cảnh
        fg = diff > 38.0
        labeled, _ = label(fg)
        sizes = np.bincount(labeled.ravel())
        sizes[0] = 0
        top_labels = np.argsort(sizes)[::-1][:6]
        char_mask = binary_fill_holes(np.isin(labeled, top_labels))
        
        labeled_chars, _ = label(char_mask)
        slices = find_objects(labeled_chars)
        valid_slices = [s for s in slices if s is not None and np.sum(labeled_chars[s] > 0) > 1000]
        
        row_height = h // 2
        valid_slices.sort(key=lambda s: (s[0].start // (row_height - 30), s[1].start))
        
        arr_uint = np.array(img, dtype=np.uint8)
        frames = []
        for idx, sl in enumerate(valid_slices[:6]):
            sy, sx = sl
            sub_rgb = arr_uint[sy, sx].copy()
            sub_mask = (labeled_chars[sy, sx] > 0)
            r, g, b = sub_rgb[:, :, 0].astype(float), sub_rgb[:, :, 1].astype(float), sub_rgb[:, :, 2].astype(float)
            dist_bg = np.sqrt((r - bg[0])**2 + (g - bg[1])**2 + (b - bg[2])**2)
            lum = 0.299 * r + 0.587 * g + 0.114 * b
            max_c = np.maximum(np.maximum(r, g), b)
            min_c = np.minimum(np.minimum(r, g), b)
            sat = (max_c - min_c) / (max_c + 1e-5)
            h_sub, w_sub = sub_mask.shape
            
            # Gọt sạch viền ngoài (Erosion 2 vòng)
            core = binary_erosion(sub_mask, iterations=2)
            
            alpha = np.zeros(sub_mask.shape, dtype=np.uint8)
            alpha[sub_mask] = 255
            
            # 1. Gọt sạch toàn bộ vầng sáng hào quang xanh nhạt/xám mờ ngoài nét viền đen
            # Bất cứ pixel nào gần màu nền hoặc có độ chênh lệch màu nền < 50 mà nằm ở rìa ngoài -> Xóa 100%
            alpha[(dist_bg < 50.0) & (~core)] = 0
            
            # 2. Xóa bóng đổ xám dưới đất (22% đáy)
            is_bottom = np.zeros_like(sub_mask)
            is_bottom[int(h_sub * 0.78):, :] = True
            is_dark = (r < 45) & (g < 45) & (b < 45)
            is_mint = (g > 140) & (b > 130) # chỉ giữ màu xanh ngọc thật đặc
            is_sack = (r > 120) & (g > 90) & (b < 80)
            is_coin = (r > 150) & (g > 120) & (b < 90)
            is_safe = is_dark | is_mint | is_sack | is_coin
            alpha[is_bottom & (dist_bg < 70.0) & (sat < 0.18) & (~is_safe)] = 0
            
            # 3. Xóa sparkle ở góc dưới bên phải frame 6
            if idx == 5:
                is_sparkle_area = np.zeros_like(sub_mask)
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
            # Alpha cut-off chặt chẽ 130 để gọt sạch mọi pixel mờ ở mép
            res_arr[res_arr[:, :, 3] < 130, 3] = 0
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
    print(f"Ultra Clean Perfect: Madoino-{action_name}.png ({len(frames)} frames) done!")

for act, file_name in ACTIONS.items():
    clean_perfect_madoino(act, file_name)
