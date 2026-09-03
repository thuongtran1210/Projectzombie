import os
import shutil
from PIL import Image

SRC_BG = r"C:\Users\thuon\.gemini\antigravity-ide\brain\a0caa716-0c54-40e4-a25a-54134669c143\.user_uploaded\media_1787828332523.png"
DEST_FILE = r"c:\Users\thuon\Unity\Projectzombie\Assets\Art\UI\VongXuyen\BG_VongXuyen_Forest_Hub.png"

def update_background():
    print(f"Copying clean background image from: {SRC_BG}")
    img = Image.open(SRC_BG).convert("RGBA")
    print(f"Original size: {img.size}")
    
    # Save directly to Unity assets
    img.save(DEST_FILE, "PNG")
    print(f"Successfully updated clean background to: {DEST_FILE}")

if __name__ == "__main__":
    update_background()
