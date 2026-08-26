import os
import numpy as np
from PIL import Image

MEDIA_DIR = r"C:\Users\thuon\.gemini\antigravity-ide\brain\b3d411d8-f012-497c-ace6-450430cca8a8\.user_uploaded"
files = [
    "media_1787739790904.png",
    "media_1787739797637.png",
    "media_1787739803441.png",
    "media_1787739809190.png"
]

for f in files:
    path = os.path.join(MEDIA_DIR, f)
    if os.path.exists(path):
        img = Image.open(path)
        print(f"{f}: size={img.size}, mode={img.mode}")
