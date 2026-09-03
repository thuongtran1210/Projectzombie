import os
import numpy as np
from PIL import Image
from scipy.ndimage import label, find_objects, binary_fill_holes, binary_erosion

def export_gachoi_art():
    raw_path = r"C:\Users\thuon\.gemini\antigravity-ide\brain\aaa8612e-3b08-4e1f-820b-6de9fb61a391\.user_uploaded\media_1788184380658.jpg"
    gachoi_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\GaChoi"
    os.makedirs(gachoi_dir, exist_ok=True)

    img = Image.open(raw_path).convert('RGB')
    arr = np.array(img, dtype=float)

    # 1. Background color detection
    bg_color = np.median(arr[:20, :20], axis=(0, 1))
    diff = np.sqrt(np.sum((arr - bg_color)**2, axis=2))

    fg = diff > 35.0
    fg_filled = binary_fill_holes(fg)

    labeled, num_features = label(fg_filled)
    slices = find_objects(labeled)
    valid_slices = [s for s in slices if s is not None and np.sum(labeled[s] > 0) > 1000]

    # Sort slices top to bottom (row buckets of 200px), then left to right
    valid_slices.sort(key=lambda s: (s[0].start // 200, s[1].start))

    arr_uint = np.array(img, dtype=np.uint8)
    frames = []

    frame_w, frame_h = 128, 128
    target_char_h = 96

    for idx, sl in enumerate(valid_slices):
        sy, sx = sl
        sub_rgb = arr_uint[sy, sx].copy()
        sub_mask = (labeled[sy, sx] > 0)

        r = sub_rgb[:, :, 0].astype(float)
        g = sub_rgb[:, :, 1].astype(float)
        b = sub_rgb[:, :, 2].astype(float)
        dist_bg = np.sqrt((r - bg_color[0])**2 + (g - bg_color[1])**2 + (b - bg_color[2])**2)

        alpha = np.zeros(sub_mask.shape, dtype=np.uint8)
        alpha[sub_mask] = 255

        # Defringe near outer border
        core = binary_erosion(sub_mask, iterations=2)
        is_halo = (dist_bg < 45.0) & (~core)
        alpha[is_halo] = 0

        # Remove soft dark background edge
        is_bg_fringe = (dist_bg < 30.0)
        alpha[is_bg_fringe] = 0

        char_pil = Image.fromarray(np.dstack((sub_rgb, alpha)), 'RGBA')
        bbox = char_pil.getbbox()
        if bbox:
            char_pil = char_pil.crop(bbox)

        cw, ch = char_pil.size
        scale = target_char_h / float(ch)
        nw, nh = int(round(cw * scale)), int(round(ch * scale))
        resized = char_pil.resize((nw, nh), Image.Resampling.LANCZOS)

        res_arr = np.array(resized)
        res_arr[res_arr[:, :, 3] < 100, 3] = 0
        resized = Image.fromarray(res_arr, 'RGBA')

        # Place onto 128x128 canvas, bottom center aligned with 10px bottom margin
        target = Image.new('RGBA', (frame_w, frame_h), (0, 0, 0, 0))
        px = (frame_w - nw) // 2
        py = max(4, frame_h - nh - 10)
        target.paste(resized, (px, py), resized)
        frames.append(target)

    print(f"Successfully processed {len(frames)} frames from uploaded image.")

    # 1. GaChoi-Idle.png (3 frames: 384x128) -> Frames [0, 1, 4]
    idle_idx = [0, 1, 4]
    idle_img = Image.new('RGBA', (3 * frame_w, frame_h), (0, 0, 0, 0))
    for i, idx in enumerate(idle_idx):
        idle_img.paste(frames[idx], (i * frame_w, 0), frames[idx])
    idle_img.save(os.path.join(gachoi_dir, "GaChoi-Idle.png"))

    # 2. GaChoi-Run.png (4 frames: 512x128) -> Frames [1, 2, 3, 4]
    run_idx = [1, 2, 3, 4]
    run_img = Image.new('RGBA', (4 * frame_w, frame_h), (0, 0, 0, 0))
    for i, idx in enumerate(run_idx):
        run_img.paste(frames[idx], (i * frame_w, 0), frames[idx])
    run_img.save(os.path.join(gachoi_dir, "GaChoi-Run.png"))

    # 3. GaChoi-Attack.png (4 frames: 512x128) -> Frames [9, 10, 11, 12] (Peck & Wing strike)
    atk_idx = [9, 10, 11, 12]
    atk_img = Image.new('RGBA', (4 * frame_w, frame_h), (0, 0, 0, 0))
    for i, idx in enumerate(atk_idx):
        atk_img.paste(frames[idx], (i * frame_w, 0), frames[idx])
    atk_img.save(os.path.join(gachoi_dir, "GaChoi-Attack.png"))

    # 4. GaChoi-All.png (8 frames: 1024x128)
    all_idx = [0, 1, 2, 3, 4, 9, 10, 11]
    all_img = Image.new('RGBA', (8 * frame_w, frame_h), (0, 0, 0, 0))
    for i, idx in enumerate(all_idx):
        all_img.paste(frames[idx], (i * frame_w, 0), frames[idx])
    all_img.save(os.path.join(gachoi_dir, "GaChoi-All.png"))

    # 5. Update Tex_VFX_Chibi_Chicken_Run.png for particle effects
    vfx_tex_path = r"c:\Users\thuon\Unity\Projectzombie\Assets\VFX\SkillLibrary\Textures\Tex_VFX_Chibi_Chicken_Run.png"
    frames[1].save(vfx_tex_path)

    print("GaChoi Sprite Sheets updated successfully!")

if __name__ == "__main__":
    export_gachoi_art()
