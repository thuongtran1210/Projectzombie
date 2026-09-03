import os, glob

for p in glob.glob(r"C:\Users\thuon\Unity\Projectzombie\Assets\Art\Madoino\*.anim.meta"):
    with open(p, "r", encoding="utf-8") as f:
        for line in f:
            if "guid:" in line:
                print(os.path.basename(p), line.strip())
