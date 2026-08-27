import os

OUTPUT_DIR = r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\QuyNhapTrang"
ACTIONS = ["Idle", "Run", "Attack", "Dead"]

anim_guids = {
    "Idle": "8100000000000000a0b77823fca88c01",
    "Run": "8100000000000000a0b77823fca88c02",
    "Attack": "8100000000000000a0b77823fca88c03",
    "Dead": "8100000000000000a0b77823fca88c04"
}

for act, g in anim_guids.items():
    meta_path = os.path.join(OUTPUT_DIR, f"{act}.anim.meta")
    content = (
        "fileFormatVersion: 2\n"
        f"guid: {g}\n"
        "NativeFormatImporter:\n"
        "  externalObjects: {}\n"
        "  mainObjectFileID: 7400000\n"
        "  userData: \n"
        "  assetBundleName: \n"
        "  assetBundleVariant: \n"
    )
    with open(meta_path, "w", encoding="utf-8") as f:
        f.write(content)

print("Created anim.meta for QuyNhapTrang!")
