import os
import numpy as np
from PIL import Image

def generate_perfect_6frame_thusinh():
    src_path = 'Assets/Art/ThuSinh/ThuSinh_Master_DNA.png'
    master_img = Image.open(src_path).convert('RGBA')
    
    # Target height 100px on 128x128 canvas
    mw, mh = master_img.size
    target_h = 100
    scale = target_h / float(mh)
    target_w = int(round(mw * scale))
    char_resized = master_img.resize((target_w, target_h), Image.Resampling.BILINEAR)
    
    # 1. IDLE (4 frames) - 512x128
    idle_strip = Image.new('RGBA', (512, 128), (0, 0, 0, 0))
    for i in range(4):
        f = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
        dy = int(round(np.sin(i * np.pi / 2.0) * 2.0))
        cur_h = target_h + dy
        cur_w = target_w - dy // 2
        f_char = char_resized.resize((cur_w, cur_h), Image.Resampling.BILINEAR)
        px = (128 - cur_w) // 2
        py = 128 - cur_h - 14
        f.paste(f_char, (px, py), f_char)
        idle_strip.paste(f, (i * 128, 0), f)
    idle_strip.save('Assets/Art/ThuSinh/ThuSinh-Idle.png')
    
    # 2. RUN (6 frames) - 768x128 (EXACTLY MATCHING Run.anim 6 FRAMES!)
    run_strip = Image.new('RGBA', (768, 128), (0, 0, 0, 0))
    angles = [5, 2, -4, -6, -2, 4]
    bobs = [0, 4, 1, 0, 4, 1]
    for i in range(6):
        f = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
        rot_char = char_resized.rotate(angles[i], resample=Image.Resampling.BICUBIC, expand=True)
        rw, rh = rot_char.size
        dy = bobs[i]
        px = (128 - rw) // 2
        py = 128 - rh - 14 - dy
        f.paste(rot_char, (px, py), rot_char)
        run_strip.paste(f, (i * 128, 0), f)
    run_strip.save('Assets/Art/ThuSinh/ThuSinh-Run.png')
    
    # 3. ATTACK (4 frames) - 512x128
    attack_strip = Image.new('RGBA', (512, 128), (0, 0, 0, 0))
    # Frame 0: Windup
    f0 = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    c0 = char_resized.rotate(-8, resample=Image.Resampling.BICUBIC, expand=True)
    f0.paste(c0, ((128 - c0.width) // 2 - 4, 128 - c0.height - 14), c0)
    attack_strip.paste(f0, (0, 0), f0)
    
    # Frame 1: Lunge/Thrust
    f1 = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    c1 = char_resized.rotate(10, resample=Image.Resampling.BICUBIC, expand=True)
    f1.paste(c1, ((128 - c1.width) // 2 + 8, 128 - c1.height - 14), c1)
    attack_strip.paste(f1, (128, 0), f1)
    
    # Frame 2: Impact
    f2 = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    c2 = char_resized.rotate(4, resample=Image.Resampling.BICUBIC, expand=True)
    f2.paste(c2, ((128 - c2.width) // 2 + 4, 128 - c2.height - 14), c2)
    attack_strip.paste(f2, (256, 0), f2)
    
    # Frame 3: Recover
    f3 = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    f3.paste(char_resized, ((128 - char_resized.width) // 2, 128 - char_resized.height - 14), char_resized)
    attack_strip.paste(f3, (384, 0), f3)
    attack_strip.save('Assets/Art/ThuSinh/ThuSinh-Attack.png')
    
    print("Exported 6-frame Run strip (768x128), 4-frame Idle strip (512x128), and 4-frame Attack strip (512x128)!")

if __name__ == '__main__':
    generate_perfect_6frame_thusinh()
