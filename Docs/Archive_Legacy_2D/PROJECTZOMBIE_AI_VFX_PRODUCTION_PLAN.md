# Projectzombie — AI-assisted VFX Production Plan

**Project:** Vong Xuyên / Projectzombie  
**Target:** Unity 2022.3 LTS, 2D Top-down Survivor-like, Android 60 FPS  
**Purpose:** Chuẩn hóa cách dùng AI Image Generation để tạo asset cho VFX, đảm bảo asset đồng nhất về art direction, dễ tích hợp vào Unity, dễ kiểm soát chất lượng và không phá gameplay readability.

---

# 1. Kết luận quan trọng

AI Image Generation **không nên tạo VFX final để đưa thẳng vào game**.

Vai trò đúng của AI là tạo:

- concept visual;
- texture source;
- shape source;
- brush stroke;
- smoke/flame silhouette;
- magic rune/decal;
- impact burst;
- particle sprite;
- noise/mask source;
- visual variations.

Sau đó asset phải đi qua:

```text
VFX Brief
  ↓
AI Concept / Source Image
  ↓
Art Direction Review
  ↓
Cleanup / Simplification
  ↓
Technical Asset Preparation
  ↓
Unity Material / Shader
  ↓
Particle Composition
  ↓
Animation & Timing
  ↓
Gameplay Readability Test
  ↓
Performance Test
  ↓
Approved VFX Prefab
```

AI tạo **nguyên liệu**.

Unity VFX/Particle System quyết định **chuyển động, timing, scale, layer, gameplay readability và performance**.

---

# 2. Vị trí của AI-to-IMG trong pipeline

## Stage 0 — Gameplay Intent

Trước khi tạo hình, phải biết effect dùng để truyền tải điều gì.

Ví dụ:

```text
Effect: Fire Hit Impact
Purpose: xác nhận projectile Hỏa đã trúng enemy
Gameplay Priority: Medium
Duration: 0.25–0.4s
World Size: khoảng 0.4–0.8m
Element: Fire
Intensity: L1
```

Không generate image khi chưa xác định gameplay intent.

---

## Stage 1 — VFX Brief

Mỗi VFX cần một brief ngắn.

Template:

```text
VFX ID:
Category:
Gameplay Event:
Element:
Yin/Yang:
Intensity:
World Size:
Duration:
Primary Shape:
Secondary Shapes:
Motion:
Palette:
Must Read:
Must Avoid:
Reference VFX:
```

Ví dụ:

```text
VFX ID: VFX_HIT_FIRE_SMALL

Category:
HitImpact

Gameplay Event:
Fire projectile hits normal enemy

Element:
Fire

Intensity:
L1

World Size:
0.6m

Duration:
0.3s

Primary Shape:
Sharp radial flame burst

Secondary:
4–8 embers

Motion:
Fast outward burst

Palette:
Deep red → orange → pale yellow

Must Read:
Instant fire damage

Must Avoid:
Large smoke cloud
Blue flame
Purple magic
Realistic explosion
High-detail background
```

---

# 3. Stage 2 — AI Image Generation

Đây là nơi AI-to-IMG được sử dụng.

AI không được tự quyết toàn bộ visual.

Input của AI phải được khóa bằng:

1. VFX Style Bible.
2. Element Style Rules.
3. Intensity Level.
4. Shape Language.
5. Palette.
6. Technical Output Requirement.
7. Negative Constraints.
8. Golden Reference Set.

---

# 4. AI nên tạo loại asset nào?

## 4.1 Shape Texture

Ví dụ:

- slash arc;
- shockwave;
- radial burst;
- magic circle;
- brush stroke;
- ground crack.

Ưu tiên ảnh:

```text
single centered shape
transparent or clean solid background
high silhouette readability
minimal detail
no environment
no character
no text
```

---

## 4.2 Particle Sprite

Ví dụ:

- ember;
- spark;
- leaf;
- ash;
- water drop;
- stone shard;
- talisman;
- ink particle.

Mỗi sprite cần có silhouette rõ khi thu nhỏ.

---

## 4.3 Smoke / Flame Source

AI có thể tạo:

- 4–8 biến thể smoke puff;
- fire tongue;
- ink cloud;
- spirit mist.

Không dùng nguyên cả ảnh explosion làm một VFX final.

Nên cắt thành primitive nhỏ và animate trong Unity.

---

## 4.4 Decal / Rune

Đây là nhóm AI đặc biệt hữu ích:

- Bát Quái;
- phù chú;
- trận pháp;
- vòng Đông Sơn cách điệu;
- họa tiết âm dương;
- ground magic pattern.

Sau AI generation cần vectorize/cleanup hoặc tạo mask sạch.

---

## 4.5 Concept Frame

AI có thể tạo concept frame để quyết định:

```text
Effect trông ra sao ở peak frame?
```

Concept frame không phải asset runtime.

Artist/dev dùng frame này để recreate bằng Particle System + shader + primitive library.

---

# 5. Không nên dùng AI-to-IMG cho gì?

Không nên coi AI-generated sprite sheet là final nếu chưa kiểm tra kỹ.

AI thường không ổn định ở:

- frame-to-frame consistency;
- alpha edge;
- exact geometry;
- exact pivot;
- animation timing;
- hitbox correspondence;
- gameplay readability;
- visual hierarchy;
- texture compression behavior.

Không nên yêu cầu:

```text
"Generate complete 12-frame fire animation for Unity"
```

và import thẳng.

Có thể dùng nó làm reference, nhưng production asset cần được kiểm soát.

---

# 6. VFX Visual DNA của Projectzombie

Mọi AI output phải bám theo DNA chung.

## Core style

```text
2D stylized
Vietnamese / East Asian spiritual folklore
brush-painted
graphic silhouette
controlled glow
low visual noise
mobile readable
chibi-compatible
```

## Không dùng

```text
photorealistic
cinematic realistic explosion
3D rendered particle look
Western high-fantasy magic
sci-fi hologram
rainbow neon
overly detailed anime aura
excessive bloom
lens flare
complex background
```

---

# 7. Shape Language

## Linh khí

```text
Curved
Circular
Spiral
Soft-flowing
```

## Physical damage

```text
Sharp
Directional
Angular
Fast
```

## Magic / spell

```text
Radial
Symmetric
Rune-based
Layered circles
```

## Dangerous enemy attack

```text
High contrast
Sharp boundary
Readable telegraph
Simple geometry
```

---

# 8. Ngũ Hành Visual Grammar

Màu sắc không được là cách duy nhất để phân biệt element.

## Kim

```text
Shape:
sharp, thin, straight

Motion:
fast, piercing

Texture:
metal spark / thin shard

Feeling:
precise, crisp
```

## Mộc

```text
Shape:
curved, branching, organic

Motion:
growth, spiral

Texture:
leaf, vine, pollen

Feeling:
living, expanding
```

## Thủy

```text
Shape:
smooth, wave, circular

Motion:
flowing, trailing

Texture:
droplet, mist

Feeling:
continuous
```

## Hỏa

```text
Shape:
triangular, rising, irregular

Motion:
burst, flicker

Texture:
ember, flame

Feeling:
aggressive
```

## Thổ

```text
Shape:
blocky, heavy, radial

Motion:
slow outward impact

Texture:
rock, dust

Feeling:
weight
```

---

# 9. Yin / Yang Visual Grammar

## Âm

```text
dark
soft
inward
smoke
ink
slow swirl
low-frequency movement
```

## Dương

```text
bright
sharp
outward
spark
flash
fast burst
```

## Balanced Yin-Yang

```text
two opposing flows
clean circular geometry
controlled contrast
balanced visual weight
```

---

# 10. Palette System

Mỗi element phải có palette cố định.

Không để AI tự chọn palette.

Mỗi VFX dùng tối đa:

```text
1 dominant color
1 secondary color
1 highlight
```

Tỷ lệ gợi ý:

```text
70% dominant
25% secondary
5% highlight
```

Ví dụ Fire:

```text
Dominant:
Deep Vermilion

Secondary:
Orange

Highlight:
Warm Ivory
```

Không thêm purple, cyan hoặc rainbow chỉ để hiệu ứng trông "magic" hơn.

---

# 11. Intensity Levels

## L1 — Basic

Dùng cho:

- basic hit;
- normal projectile;
- normal weapon attack.

Budget:

```text
1 primary shape
1 secondary particle group
0–1 flash
short duration
small world coverage
```

---

## L2 — Skill

Dùng cho:

- weapon active;
- medium AOE;
- elite attacks.

Budget:

```text
1 major shape
2 secondary layers
small shockwave allowed
medium duration
```

---

## L3 — Signature

Dùng cho:

- character signature skill;
- elite special;
- important phase skill.

Budget:

```text
2 major visual layers
2–3 supporting layers
ground decal allowed
controlled camera feedback
```

---

## L4 — Ultimate / Boss

Dùng cho:

- ultimate;
- boss phase attack;
- rare high-impact moment.

Có thể dùng:

```text
large decal
shockwave
screen flash
camera shake
large particle burst
```

Nhưng gameplay indicators vẫn phải đọc được.

---

# 12. Prompt Template cho AI-to-IMG

Prompt không nên chỉ mô tả subject.

Prompt cần mô tả:

```text
[Asset type]
+ [Project style]
+ [Element grammar]
+ [Shape]
+ [Palette]
+ [Composition]
+ [Technical requirements]
+ [Things to avoid]
```

Template:

```text
Create a 2D stylized game VFX source texture for a top-down mobile
survivor-like game inspired by Vietnamese spiritual folklore.

Asset:
[TYPE]

Element:
[ELEMENT]

Primary silhouette:
[SHAPE]

Visual language:
[STYLE RULES]

Palette:
[COLORS]

Composition:
single isolated centered element,
strong silhouette,
minimal internal detail,
readable at small size.

Technical:
transparent background or clean flat background,
no character,
no environment,
no typography,
no UI,
no perspective scene.

Avoid:
photorealism,
3D-rendered look,
rainbow colors,
heavy bloom,
lens flare,
complex background,
excessive micro-detail.
```

---

# 13. Prompt Example — Fire Hit

```text
Create a 2D stylized fire impact sprite for a top-down mobile
survivor-like game inspired by Vietnamese spiritual folklore.

A compact radial burst made of sharp brush-painted flame shapes.

Palette:
deep vermilion, orange, warm ivory highlight.

The silhouette should feel fast and aggressive.
Use 5–8 strong flame spikes with a clean central impact.

Single isolated centered effect.
Minimal internal detail.
Readable when displayed very small.

Transparent background.
No character.
No environment.
No text.

Avoid:
photorealistic explosion,
smoke cloud,
purple or blue flames,
rainbow colors,
3D rendering,
lens flare,
excessive glow.
```

---

# 14. Prompt Example — Mộc

```text
Create a 2D stylized Wood-element spirit particle for a top-down
Vietnamese folklore mobile game.

Organic curved leaf-like energy shape painted with bold brush strokes.

Palette:
deep moss green, jade green, pale yellow-green highlight.

The shape should suggest growth and spiral movement.

Single isolated sprite.
Strong silhouette.
Minimal detail.
Transparent background.

Avoid:
realistic leaf photography,
Western fantasy magic,
neon green,
complex background,
text,
3D rendering.
```

---

# 15. Golden Reference Set

Trước khi generate hàng trăm asset, project phải tạo 5–8 effect chuẩn.

Đề xuất:

```text
REF_01 Basic Slash
REF_02 Normal Hit Impact
REF_03 Fire Burst
REF_04 Water Wave
REF_05 Wood Spirit
REF_06 Yin Mist
REF_07 Yang Impact
REF_08 Boss Ultimate
```

Bộ này trở thành chuẩn visual.

Mọi VFX mới phải được so sánh với Golden Reference.

Câu hỏi:

```text
Nếu đặt effect mới cạnh 8 reference,
nó có trông như cùng một game không?
```

Nếu không, reject hoặc rework.

---

# 16. Generate Variations, không generate Final ngay

Cho mỗi asset:

```text
Generate 4–8 variations
        ↓
Select top 2
        ↓
Cleanup
        ↓
Test in Unity
        ↓
Choose final
```

Không chọn output đầu tiên chỉ vì nó "đẹp".

Tiêu chí chọn:

1. silhouette;
2. readability;
3. style consistency;
4. ease of cleanup;
5. usefulness as reusable primitive.

---

# 17. AI Asset Review Gate

Mỗi AI image phải qua review trước khi import.

## Visual

```text
[ ] Đúng style
[ ] Đúng palette
[ ] Đúng shape language
[ ] Không quá nhiều detail
[ ] Không có random color
[ ] Silhouette rõ
[ ] Phù hợp intensity level
```

## Technical

```text
[ ] Có thể tách alpha sạch
[ ] Edge không bị artifact nặng
[ ] Không chứa background khó remove
[ ] Tâm/pivot có thể xác định rõ
[ ] Có thể scale xuống mà vẫn đọc được
[ ] Có thể dùng lại trong Particle System
```

Nếu fail, regenerate thay vì cố cứu asset kém.

---

# 18. Cleanup Pipeline

AI output được xem là raw source.

Pipeline:

```text
Raw_AI/
    ↓
Crop
    ↓
Remove background
    ↓
Clean alpha
    ↓
Simplify shape
    ↓
Normalize palette
    ↓
Remove tiny details
    ↓
Normalize canvas
    ↓
Export Production PNG
```

Không import trực tiếp folder Raw_AI vào Addressables/runtime.

---

# 19. Folder Structure

Đề xuất:

```text
Assets/VFX/

├── _AI_Source/
│   ├── Fire/
│   ├── Water/
│   ├── Wood/
│   ├── Metal/
│   ├── Earth/
│   ├── YinYang/
│   └── References/
│
├── Library/
│   ├── Textures/
│   │   ├── Shapes/
│   │   ├── Particles/
│   │   ├── Trails/
│   │   ├── Noise/
│   │   └── Decals/
│   │
│   ├── Materials/
│   ├── Shaders/
│   └── Primitives/
│
├── Prefabs/
│   ├── Common/
│   ├── Weapons/
│   ├── Skills/
│   ├── Enemies/
│   └── Boss/
│
├── Runtime/
└── Editor/
```

`_AI_Source` không được dùng trực tiếp trong gameplay prefab.

---

# 20. Naming Convention

Raw AI:

```text
AI_FIRE_Burst_001_RAW.png
AI_FIRE_Burst_002_RAW.png
```

Approved production texture:

```text
TX_VFX_FireBurst_A.png
TX_VFX_FireBurst_B.png
```

Material:

```text
MAT_VFX_Fire_Additive
```

Prefab:

```text
PF_VFX_Hit_Fire_Small
```

Profile:

```text
VFX_HIT_FIRE_SMALL
```

---

# 21. Primitive Library

Không tạo texture mới cho mọi skill.

Xây library khoảng 20–30 primitives.

## Shape

```text
CircleSoft
RingHard
SlashArc
BrushArc
RadialBurst
Shockwave
GroundCrack
```

## Particle

```text
Spark
Ember
Dust
Smoke
Leaf
WaterDrop
RockShard
Talisman
InkDrop
```

## Trail

```text
BrushTrail
EnergyTrail
SmokeTrail
WeaponTrail
```

## Decal

```text
BatQuai
DongSonCircle
YinYang
TalismanCircle
InkCircle
```

VFX mới ưu tiên composition từ library trước khi generate asset mới.

---

# 22. Composition Rule

Một effect thường gồm:

```text
PRIMARY
+
SECONDARY
+
TERTIARY
```

Ví dụ Fire Impact:

```text
Primary:
Radial flame burst

Secondary:
Embers

Tertiary:
Tiny smoke / flash
```

Một layer phải là dominant.

Không để mọi layer đều có visual weight giống nhau.

---

# 23. Unity Assembly

Sau khi texture được approved:

```text
Texture
  ↓
Material
  ↓
Particle System
  ↓
Modular Layers
  ↓
Prefab
  ↓
VFXPoolResetter
  ↓
GlobalVFXPoolManager
```

AI không quyết định:

- emission curve;
- velocity;
- particle lifetime;
- timing;
- sorting;
- VFX duration;
- max instances;
- pool behavior.

Các phần này được định nghĩa trong Unity.

---

# 24. Timing Standard

Một effect có impact nên có các phase:

```text
Anticipation
Impact
Expansion
Dissipation
```

Ví dụ Hit:

```text
0.00  anticipation
0.03  flash
0.06  impact
0.12  expansion
0.25  particles dissipate
0.40  fully gone
```

Ví dụ Explosion:

```text
Flash       0.05s
Core burst  0.10s
Shockwave   0.20s
Debris      0.30s
Smoke       0.60s
```

Không spawn tất cả layer cùng một thời điểm rồi fade cùng lúc.

---

# 25. Gameplay Readability Gate

VFX đẹp nhưng làm người chơi không đọc được combat là fail.

Mỗi effect phải kiểm tra:

```text
[ ] Player silhouette vẫn nhìn rõ
[ ] Enemy silhouette vẫn đủ rõ
[ ] Danger telegraph không bị che
[ ] Hitbox và visual tương ứng
[ ] Ground decal không che indicator
[ ] Ultimate không giữ màn hình sáng quá lâu
```

Priority:

```text
Danger Indicator
>
Gameplay Entity
>
Important Hit Feedback
>
Decorative VFX
```

---

# 26. Test ở kích thước gameplay thật

Không đánh giá asset bằng preview 1024×1024.

Test ở:

```text
actual camera distance
actual sprite scale
actual Android resolution
actual enemy density
```

Một image nhìn đẹp ở full resolution có thể trở thành noise khi còn 40–80 pixels trên màn hình.

---

# 27. VFX Test Arena

Tạo:

```text
Scenes/VFX_TestArena.unity
```

Scene gồm:

```text
Player dummy
Small enemy
Normal enemy
Elite
Boss

Bright floor
Dark floor

VFX selector

Spawn:
x1
x10
x25
x50

Speed:
0.25x
1x
2x
```

Có camera giống gameplay.

---

# 28. Side-by-side Review

Khi review VFX mới, luôn hiển thị cùng Golden Reference.

Ví dụ:

```text
Reference Fire Impact
New Fire Impact

Reference Signature
New Signature
```

Không review asset mới hoàn toàn độc lập.

---

# 29. VFXProfileSO

Nên bổ sung data abstraction.

Ví dụ:

```csharp
VFXProfileSO
{
    string id;

    VFXCategory category;
    ElementType element;
    VFXIntensity intensity;

    GameObject prefab;

    float duration;
    float baseScale;

    int maxInstances;

    bool allowCameraShake;
    bool allowHitStop;

    VFXQuality minimumQuality;
}
```

Gameplay gọi:

```csharp
VFXService.Play(VFXId.FireHit, context);
```

thay vì trực tiếp biết prefab.

---

# 30. Quality Tier

Mobile nên có:

```text
Low
Medium
High
```

Ví dụ:

High:

```text
Shockwave
20 sparks
Smoke
Secondary glow
```

Medium:

```text
Shockwave
12 sparks
Smoke
```

Low:

```text
Shockwave
6 sparks
```

Gameplay không thay đổi.

---

# 31. AI Prompt Versioning

Prompt dùng để generate asset production cần được lưu.

Ví dụ:

```text
VFXPrompts/
├── Fire/
│   ├── FireHit_v01.md
│   └── FireHit_v02.md
```

Mỗi approved asset ghi:

```text
Source Prompt:
FireHit_v02

Generation Batch:
2026-09-FIRE-03

Selected Variation:
04

Cleanup:
Yes

Approved By:
Art Lead
```

Mục tiêu là reproducibility.

---

# 32. Không để mỗi developer tự prompt theo ý mình

Cần Prompt Preset.

Ví dụ:

```text
Preset:
VFX_SOURCE_TEXTURE

Preset:
VFX_RUNE_DECAL

Preset:
VFX_PARTICLE_SPRITE

Preset:
VFX_CONCEPT_FRAME
```

Developer chỉ điền:

```text
element
shape
purpose
intensity
```

Style portion được giữ cố định.

---

# 33. Acceptance Criteria cho AI-generated asset

Asset chỉ được dùng production khi đạt:

## Art

```text
Same visual family as Golden Reference
Correct element language
Controlled palette
Strong silhouette
No accidental AI artifacts
```

## Gameplay

```text
Readable at gameplay scale
Does not hide important telegraphs
Impact strength matches gameplay importance
```

## Technical

```text
Clean alpha
Reasonable texture size
Correct import settings
Reusable material
Pooling-compatible prefab
```

## Performance

```text
No unexpected material instance spam
Particle count within tier
No uncontrolled overdraw
Mobile test passed
```

---

# 34. Implementation Roadmap

## P0 — Freeze uncontrolled generation

Ngay lập tức:

```text
Không thêm production VFX mới theo prompt tự do.
```

AI-generated assets mới chỉ được đặt vào `_AI_Source`.

---

## P1 — Create VFX Style Bible

Tạo:

```text
VFX_STYLE_GUIDE.md
```

Bao gồm:

- Core visual DNA;
- palettes;
- Ngũ Hành grammar;
- Yin/Yang grammar;
- shape language;
- intensity;
- do/don't examples.

---

## P2 — Golden Reference Set

Làm lại 8 effect chuẩn.

Không cần sửa toàn game.

Đảm bảo bộ này đạt chất lượng cuối cùng mong muốn.

---

## P3 — Prompt Presets

Tạo 4 prompt chuẩn:

```text
Source Texture
Particle Sprite
Rune / Decal
Concept Frame
```

---

## P4 — Primitive Library

Tạo 20–30 assets reusable.

Các VFX tương lai phải ưu tiên reuse library.

---

## P5 — Asset Pipeline

Thiết lập:

```text
_AI_Source
→ Review
→ Clean
→ Production Texture
→ Material
→ Prefab
```

---

## P6 — Test Arena

Tạo scene VFX review.

---

## P7 — VFXProfile / VFXService

Tách gameplay code khỏi prefab.

---

## P8 — Existing VFX Migration

Migration theo thứ tự:

```text
Basic Hit
→ Basic Weapon Attack
→ Projectile
→ Element Effects
→ Character Skills
→ Signature Skills
→ Elite
→ Boss
```

Không migrate toàn bộ cùng lúc.

---

# 35. Team Workflow cho một VFX mới

```text
Developer / Designer
        ↓
Create VFX Brief
        ↓
Choose existing primitive?
      ↙       ↘
    YES        NO
     ↓          ↓
Compose      AI Generation
     ↓          ↓
     └── Review / Cleanup
              ↓
        Add to Library
              ↓
       Unity Composition
              ↓
        Test Arena Review
              ↓
      Gameplay Integration
              ↓
       Mobile Performance
              ↓
           Approved
```

---

# 36. Pull Request Requirement

PR có VFX mới phải ghi:

```text
VFX ID:
Category:
Element:
Intensity:
New AI Source:
New Production Texture:
New Material:
New Prefab:
Golden Reference Compared:
Test Arena Result:
Android Performance Result:
```

Nếu AI-generated source được thêm:

```text
Prompt file phải được commit cùng hoặc lưu theo quy trình team.
```

---

# 37. Definition of Done

Một VFX chỉ Done khi:

```text
[ ] Brief có đầy đủ
[ ] Đúng Style Guide
[ ] So với Golden Reference
[ ] AI source đã cleanup
[ ] Production texture đúng naming
[ ] Material dùng chuẩn
[ ] Prefab modular
[ ] Pool reset đúng
[ ] Intensity đúng
[ ] Gameplay readability đạt
[ ] Test concurrency đạt
[ ] Android performance đạt
[ ] PR được review
```

---

# 38. Nguyên tắc cuối

```text
AI does not define the art style.
The project defines the art style.

AI does not create the final effect.
AI creates controlled visual ingredients.

Unity defines:
motion,
timing,
impact,
gameplay readability,
performance.

Golden References define quality.
Style Guide defines consistency.
Review Gate protects production.
```

---

# 39. Recommended First Sprint

Nếu bắt đầu ngay, sprint đầu chỉ làm:

### Task 1
Tạo `VFX_STYLE_GUIDE.md`.

### Task 2
Chọn và hoàn thiện 8 Golden Reference VFX.

### Task 3
Tạo palette + shape grammar Ngũ Hành/Yin-Yang.

### Task 4
Tạo 4 AI Prompt Presets.

### Task 5
Tạo `_AI_Source` và Production Library folder convention.

### Task 6
Tạo VFX Test Arena.

### Task 7
Làm Primitive Library v1 khoảng 20 asset.

Sau sprint này mới bắt đầu regenerate/migrate VFX diện rộng.

---

**Document owner:** Art Lead + Technical Lead  
**Applies to:** VFX Artist, Technical Artist, Gameplay Developer, AI-assisted asset pipeline  
**Review rule:** Mọi thay đổi lớn về visual direction phải cập nhật Golden Reference và Style Guide trước khi scale ra toàn game.
