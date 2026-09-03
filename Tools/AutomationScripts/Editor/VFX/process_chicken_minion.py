import os
import numpy as np
from PIL import Image
from scipy.ndimage import label, find_objects, binary_fill_holes, binary_erosion

def extract_chicken_sprites():
    raw_path = r"C:\Users\thuon\.gemini\antigravity-ide\brain\aaa8612e-3b08-4e1f-820b-6de9fb61a391\.user_uploaded\media_1788184380658.jpg"
    out_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\ChickenMinion"
    os.makedirs(out_dir, exist_ok=True)

    img = Image.open(raw_path).convert('RGB')
    arr = np.array(img, dtype=float)

    # Sample background color
    bg_color = np.median(arr[:20, :20], axis=(0, 1))
    diff = np.sqrt(np.sum((arr - bg_color)**2, axis=2))

    # Foreground threshold
    fg = diff > 35.0
    fg_filled = binary_fill_holes(fg)

    labeled, num_features = label(fg_filled)
    slices = find_objects(labeled)
    valid_slices = [s for s in slices if s is not None and np.sum(labeled[s] > 0) > 1000]

    # Sort slices top to bottom (by row buckets of 200px), then left to right
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

        # Defringe near border
        core = binary_erosion(sub_mask, iterations=2)
        is_halo = (dist_bg < 45.0) & (~core)
        alpha[is_halo] = 0

        # Also remove soft dark background fringe
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

        # Alpha thresholding
        res_arr = np.array(resized)
        res_arr[res_arr[:, :, 3] < 100, 3] = 0
        resized = Image.fromarray(res_arr, 'RGBA')

        # Place onto 128x128 canvas, aligned bottom center with 10px padding from bottom
        target = Image.new('RGBA', (frame_w, frame_h), (0, 0, 0, 0))
        px = (frame_w - nw) // 2
        py = max(4, frame_h - nh - 10)
        target.paste(resized, (px, py), resized)
        frames.append(target)

    print(f"Extracted {len(frames)} frames successfully.")

    # 1. Chicken_All.png (all 17 frames in horizontal strip)
    strip_all = Image.new('RGBA', (len(frames) * frame_w, frame_h), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        strip_all.paste(f, (i * frame_w, 0), f)
    strip_all.save(os.path.join(out_dir, "Chicken_All.png"))

    # 2. Chicken_Idle.png (Frames 1, 2, 5, 1) -> 4 frames
    idle_indices = [0, 1, 4, 0]
    strip_idle = Image.new('RGBA', (len(idle_indices) * frame_w, frame_h), (0, 0, 0, 0))
    for i, idx in enumerate(idle_indices):
        strip_idle.paste(frames[idx], (i * frame_w, 0), frames[idx])
    strip_idle.save(os.path.join(out_dir, "Chicken_Idle.png"))

    # 3. Chicken_Run.png (Frames 1, 2, 3, 4, 5, 6, 7, 8) -> 8 frames
    run_indices = [0, 1, 2, 3, 4, 5, 6, 7]
    strip_run = Image.new('RGBA', (len(run_indices) * frame_w, frame_h), (0, 0, 0, 0))
    for i, idx in enumerate(run_indices):
        strip_run.paste(frames[idx], (i * frame_w, 0), frames[idx])
    strip_run.save(os.path.join(out_dir, "Chicken_Run.png"))

    # 4. Chicken_Attack.png (Frames 10, 11, 12, 13, 14, 15) -> 6 frames
    atk_indices = [9, 10, 11, 12, 13, 14]
    strip_atk = Image.new('RGBA', (len(atk_indices) * frame_w, frame_h), (0, 0, 0, 0))
    for i, idx in enumerate(atk_indices):
        strip_atk.paste(frames[idx], (i * frame_w, 0), frames[idx])
    strip_atk.save(os.path.join(out_dir, "Chicken_Attack.png"))

    # Also update VFX Chibi Chicken texture
    vfx_tex_path = r"c:\Users\thuon\Unity\Projectzombie\Assets\VFX\SkillLibrary\Textures\Tex_VFX_Chibi_Chicken_Run.png"
    frames[0].save(vfx_tex_path)
    print(f"Updated {vfx_tex_path} with frame 1.")

if __name__ == "__main__":
    extract_chicken_sprites()
