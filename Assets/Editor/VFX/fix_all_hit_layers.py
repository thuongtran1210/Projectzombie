import os
import re

DATA_DIR = r"c:\Users\thuon\Unity\Projectzombie\Assets\_Data\Projectiles\Data"

def fix_all_hit_layers():
    files = [f for f in os.listdir(DATA_DIR) if f.endswith(".asset") and f.startswith("Proj_W")]
    for filename in files:
        filepath = os.path.join(DATA_DIR, filename)
        with open(filepath, "r", encoding="utf-8") as f:
            content = f.read()
        
        # Replace m_Bits: <number> under HitLayer with 4294967295 (Everything)
        new_content = re.sub(
            r"(HitLayer:\s*\n\s*serializedVersion:\s*\d+\s*\n\s*m_Bits:\s*)\d+",
            r"\g<1>4294967295",
            content
        )
        
        if new_content != content:
            with open(filepath, "w", encoding="utf-8") as f:
                f.write(new_content)
            print("Updated HitLayer in: " + filename.encode('ascii', 'replace').decode('ascii'))
        else:
            print("Already correct: " + filename.encode('ascii', 'replace').decode('ascii'))

if __name__ == "__main__":
    fix_all_hit_layers()
