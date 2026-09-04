import os
from PIL import Image

art_dir = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\VongXuyen"
files = [
    "Frame_Card_Wood_9Slice.png",
    "Frame_Card_Jade_9Slice.png",
    "Frame_Card_Evolution_Gold_9Slice.png",
    "Frame_Card_Synergy_9Slice.png"
]

for f in files:
    p = os.path.join(art_dir, f)
    if not os.path.exists(p):
        continue
    img = Image.open(p).convert("RGBA")
    bbox = img.getbbox()
    if bbox:
        # Cắt sát mép viền (Auto crop transparent margins)
        cropped = img.crop(bbox)
        # Resize về chuẩn kích thước HD thống nhất: 512 x 768 (tỉ lệ 2:3)
        resized = cropped.resize((512, 768), Image.Resampling.LANCZOS)
        resized.save(p, "PNG")
        print(f"Cropped and normalized {f} to 512x768 (Original bbox was {bbox})")

print("All frames cropped tightly to borders!")
