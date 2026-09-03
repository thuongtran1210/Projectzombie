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
    
    # 4. DEAD (6 frames) - 768x128 (Stagger, Collapse, Flat, Soul Rising & Ascending)
    dead_strip = Image.new('RGBA', (768, 128), (0, 0, 0, 0))
    ghost_img = char_resized.copy()
    r, g, b, a = ghost_img.split()
    ghost_r = r.point(lambda p: int(p * 0.4))
    ghost_g = g.point(lambda p: int(min(255, p * 1.3 + 50)))
    ghost_b = b.point(lambda p: int(min(255, p * 1.5 + 80)))
    ghost_a = a.point(lambda p: int(p * 0.75))
    ghost_tinted = Image.merge('RGBA', (ghost_r, ghost_g, ghost_b, ghost_a))

    # Frame 0: Stagger back (-12 deg)
    f0 = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    c0 = char_resized.rotate(-12, resample=Image.Resampling.BICUBIC, expand=True)
    f0.paste(c0, ((128 - c0.width) // 2 - 8, 128 - c0.height - 18), c0)
    dead_strip.paste(f0, (0, 0), f0)

    # Frame 1: Falling down (-45 deg)
    f1 = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    c1 = char_resized.rotate(-45, resample=Image.Resampling.BICUBIC, expand=True)
    f1.paste(c1, ((128 - c1.width) // 2 - 14, 128 - c1.height - 10), c1)
    dead_strip.paste(f1, (128, 0), f1)

    # Frame 2: Hitting ground (-80 deg)
    f2 = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    c2 = char_resized.rotate(-80, resample=Image.Resampling.BICUBIC, expand=True)
    f2.paste(c2, ((128 - c2.width) // 2 - 8, 128 - c2.height - 4), c2)
    dead_strip.paste(f2, (256, 0), f2)

    # Frame 3: Flat on ground (-90 deg)
    f3 = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    c3 = char_resized.rotate(-90, resample=Image.Resampling.BICUBIC, expand=True)
    f3.paste(c3, ((128 - c3.width) // 2, 128 - c3.height - 4), c3)
    dead_strip.paste(f3, (384, 0), f3)

    # Frame 4: Flat body + Ghost rising
    f4 = f3.copy()
    f4.paste(ghost_tinted, ((128 - ghost_tinted.width) // 2, 128 - ghost_tinted.height - 36), ghost_tinted)
    dead_strip.paste(f4, (512, 0), f4)

    # Frame 5: Fading body + Ghost ascending high
    f5 = Image.new('RGBA', (128, 128), (0, 0, 0, 0))
    body_dim = c3.copy()
    br, bg, bb, ba = body_dim.split()
    body_dim = Image.merge('RGBA', (br, bg, bb, ba.point(lambda p: int(p * 0.6))))
    f5.paste(body_dim, ((128 - body_dim.width) // 2, 128 - body_dim.height - 4), body_dim)

    ghost_high = ghost_tinted.resize((int(ghost_tinted.width * 1.05), int(ghost_tinted.height * 1.05)), Image.Resampling.BILINEAR)
    gr, gg, gb, ga = ghost_high.split()
    ghost_high = Image.merge('RGBA', (gr, gg, gb, ga.point(lambda p: int(p * 0.9))))
    f5.paste(ghost_high, ((128 - ghost_high.width) // 2, 128 - ghost_high.height - 62), ghost_high)
    dead_strip.paste(f5, (640, 0), f5)
    dead_strip.save('Assets/Art/ThuSinh/ThuSinh-Dead.png')

    print("Exported 6-frame Run strip (768x128), 4-frame Idle strip (512x128), 4-frame Attack strip (512x128), and 6-frame Dead strip (768x128)!")

if __name__ == '__main__':
    generate_perfect_6frame_thusinh()
