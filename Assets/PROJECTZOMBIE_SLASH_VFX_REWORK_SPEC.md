# Projectzombie — Slash VFX Rework Specification

**Project:** Projectzombie / Vong Xuyên  
**Engine:** Unity 2022.3.62f3  
**Render Pipeline:** URP 14.0.12  
**Target:** 2D Top-down Action Roguelite / Mobile 60 FPS  
**Document Type:** Technical Design / Implementation Specification  
**Status:** Proposed

---

# 1. Mục tiêu

Tài liệu này mô tả các thay đổi cần thực hiện để nâng cấp **VFX vết chém cận chiến** từ dạng sprite tĩnh “bật lên rồi biến mất” thành hiệu ứng có chuyển động, có nhịp, có hướng và phù hợp với art direction hiện tại của Projectzombie.

Mục tiêu chính:

- Giữ nguyên kiến trúc combat hiện tại.
- Không thay đổi logic damage / hitbox nếu không cần thiết.
- Tận dụng hệ thống VFX Pool hiện có.
- Biến một texture slash đơn thành VFX động bằng:
  - Reveal
  - Stretch
  - Peak
  - Dissolve
  - After-image
  - Brush fragments
- Tách vị trí hiển thị VFX khỏi vị trí hitbox.
- Chuẩn hóa slash theo combo 1–2–3.
- Giữ performance phù hợp Android 60 FPS.
- Không tạo GC runtime không cần thiết.
- Phù hợp Visual DNA:
  - 2D stylized
  - chibi-compatible
  - brush-painted
  - East Asian / Vietnamese folklore
  - controlled glow
  - low visual noise

---

# 2. Hiện trạng

## 2.1 Combat flow hiện tại

File chính:

```text
Assets/Features/Player/CharacterCombat.cs
```

Flow hiện tại:

```text
TriggerAttack()
    ↓
ExecuteAttack()
    ↓
PlayerAnimator.ChangeAnimationState(Attack)
    ↓
ExecuteMeleeSlashRoutine()
    ↓
Wind-up Delay
    ↓
Spawn slashVfxPrefab
    ↓
OverlapBoxNonAlloc
    ↓
Damage
    ↓
Hit Sparks
    ↓
Knockback
    ↓
Camera Shake / Hit Stop
```

Kiến trúc combat này được giữ lại.

---

## 2.2 VFX spawn hiện tại

Hiện slash được spawn một lần tại thời điểm impact:

```csharp
if (attackConfig.slashVfxPrefab != null)
{
    float life = attackConfig.vfxDuration > 0
        ? attackConfig.vfxDuration
        : 0.45f;

    VFXPoolManager.SpawnVFX(
        attackConfig.slashVfxPrefab,
        center,
        Quaternion.Euler(0, 0, angle),
        life
    );
}
```

Vấn đề không nằm ở lệnh spawn.

Vấn đề xuất hiện nếu prefab chỉ có:

```text
VFX_Slash
└── SpriteRenderer
```

Kết quả nhìn thấy:

```text
Invisible
→ Full Slash Sprite
→ Fade
→ Invisible
```

Hiệu ứng vì vậy giống decal / icon hơn là chuyển động của một nhát kiếm.

---

# 3. Nguyên tắc thiết kế mới

Slash không được xem như một ảnh tĩnh.

Slash phải được xem như:

> Một nét cọ năng lượng được vẽ ra cực nhanh theo hướng chuyển động của vũ khí.

Một slash hoàn chỉnh có bốn pha:

```text
Anticipation
    ↓
Attack / Reveal
    ↓
Peak / Impact
    ↓
Breakup / Dissipation
```

Với basic attack, toàn bộ visual chính nên kết thúc trong khoảng:

```text
0.18 – 0.28 giây
```

Không nên để full slash đứng yên 0.3–0.5 giây.

---

# 4. Kiến trúc VFX đề xuất

Prefab mới:

```text
PF_VFX_BasicSlash
│
├── MainSlash
│   └── SpriteRenderer
│
├── CoreSlash
│   └── SpriteRenderer
│
├── AfterImage
│   └── SpriteRenderer
│
├── BrushFragments
│   └── ParticleSystem
│
├── SlashVFXAnimator
│
└── VFXPoolResetter
```

Phiên bản tối giản:

```text
PF_VFX_BasicSlash
│
├── MainSlash
├── AfterImage
├── BrushFragments
├── SlashVFXAnimator
└── VFXPoolResetter
```

---

# 5. Vai trò từng layer

## 5.1 MainSlash

Texture chính của vết chém.

Nguồn:

- AI generated slash texture
- texture hand-painted đã cleanup
- alpha sạch
- không background

Nhiệm vụ:

- cung cấp silhouette chính;
- reveal theo hướng chém;
- đạt peak cực nhanh;
- dissolve sau impact.

Không dùng MainSlash như sprite tĩnh.

---

## 5.2 CoreSlash

Optional.

Một phiên bản:

- nhỏ hơn;
- sáng hơn;
- mỏng hơn;
- chỉ tồn tại gần peak.

Mục đích:

- tăng cảm giác tốc độ;
- tạo “hot core”;
- không tăng quá nhiều particle.

Có thể bỏ layer này ở effect L1 nếu muốn tối ưu.

---

## 5.3 AfterImage

Có thể dùng chính texture MainSlash.

Thiết lập:

```text
Alpha:
0.15 – 0.35

Scale X:
1.03 – 1.12

Delay:
0.03 – 0.05s sau MainSlash
```

Mục đích:

- tạo motion persistence;
- làm slash có hướng;
- tăng cảm giác tốc độ mà không cần nhiều sprite frame.

---

## 5.4 BrushFragments

Particle System phụ.

Số lượng:

```text
2 – 4 particles cho basic slash
```

Không dùng 20–30 particles.

Particle nên có dạng:

- tapered shard;
- brush chip;
- diamond shard;
- tiny slash fragment.

Motion:

```text
bay theo hướng slash
+
random spread rất nhỏ
```

Không burst 360°.

---

# 6. Timing chuẩn

Basic slash đề xuất:

```text
0.000s
Attack animation bắt đầu

0.000 – 0.080s
Wind-up / anticipation

0.080s
Slash bắt đầu reveal

0.105s
Slash khoảng 50–70%

0.130s
Peak visual
Damage active
Hitbox active
Lunge
Slash sound

0.145s
Brush fragment burst

0.160s
Main shape bắt đầu breakup

0.180 – 0.260s
After-image + dissolve

0.260 – 0.300s
Effect kết thúc
```

---

# 7. Animation curve đề xuất

## Reveal phase

Duration:

```text
0.045 – 0.065s
```

Properties:

```text
Reveal:
0 → 1

Scale X:
0.82 → 1.08

Scale Y:
0.90 → 1.00

Alpha:
0.35 → 1.00
```

Easing:

```text
EaseOutCubic
```

Slash phải “phóng ra”, không pop nguyên hình.

---

## Peak phase

Duration:

```text
0.025 – 0.050s
```

Properties:

```text
Reveal:
1

Scale X:
1.08 → 1.00

Brightness:
1.15 → 1.00
```

Có overshoot nhẹ.

---

## Dissolve phase

Duration:

```text
0.08 – 0.14s
```

Properties:

```text
Dissolve:
0 → 1

Alpha:
1 → 0

Scale X:
1.00 → 1.08
```

Visual phải có cảm giác tiếp tục di chuyển trước khi tan.

---

# 8. Shader Reveal / Dissolve

## 8.1 Mục tiêu

Một texture tĩnh phải có khả năng:

```text
ẩn
→ được “vẽ ra”
→ đầy đủ
→ bị phá vỡ
→ biến mất
```

Shader cần tối thiểu:

```text
_Reveal
_Dissolve
_EdgeWidth
_EdgeIntensity
```

Optional:

```text
_NoiseTex
_RevealDirection
_DissolveSoftness
_HighlightColor
```

---

## 8.2 Reveal logic

Không dùng clip thẳng hoàn toàn theo UV.

Concept:

```text
revealMask =
    uv.x
    + noise * noiseStrength
```

Sau đó:

```text
revealMask <= _Reveal
    → visible
```

Noise nhẹ giúp mép reveal giống brush stroke thay vì wipe UI.

---

## 8.3 Material handling

Không dùng runtime:

```csharp
spriteRenderer.material.SetFloat(...)
```

vì có nguy cơ tạo Material instance.

Bắt buộc ưu tiên:

```csharp
MaterialPropertyBlock
```

---

# 9. Component mới — SlashVFXAnimator

Tạo file:

```text
Assets/Features/VFX/Slash/SlashVFXAnimator.cs
```

Trách nhiệm:

- reset trạng thái khi object được lấy từ pool;
- chạy Reveal;
- chạy Peak;
- chạy Dissolve;
- animate MainSlash;
- animate AfterImage;
- trigger fragment burst nếu cần;
- không xử lý damage;
- không xử lý hitbox;
- không xử lý target detection.

Component này chỉ chịu trách nhiệm Presentation.

---

# 10. Không dùng Coroutine cho visual animation

Project đang ưu tiên:

- pooling;
- zero-GC;
- mobile performance.

Do đó animation VFX ngắn có thể chạy bằng:

```csharp
Update()
```

với state/timer nội bộ.

Ví dụ state:

```text
Reveal
Peak
Dissolve
Completed
```

Khi GameObject bị disable bởi pool:

```text
Update() tự ngừng
```

Khi spawn lại:

```text
OnEnable()
→ ResetState()
```

---

# 11. Tách Hitbox Center và Visual Center

## Hiện trạng

Hiện combat dùng:

```csharp
Vector2 center =
    (Vector2)transform.position
    + direction * offset;
```

`center` được dùng đồng thời cho:

- hitbox;
- slash VFX.

Điều này làm slash có nguy cơ trông như đang “bay bên cạnh nhân vật”.

---

## Thay đổi

Tạo hai center:

```csharp
Vector2 hitCenter =
    (Vector2)transform.position
    + direction * hitOffset;

Vector2 visualCenter =
    (Vector2)transform.position
    + direction * visualOffset;
```

Đề xuất:

```text
meleeOffset:
~1.2m

slashVisualOffset:
~0.65 – 0.85m
```

Hitbox vẫn giữ gameplay range.

Visual nằm gần tay / vũ khí hơn.

---

# 12. Thay đổi CharacterAttackConfig

File:

```text
Assets/Features/Player/CharacterEntry.cs
```

Bổ sung:

```csharp
[Header("Melee Slash Visual Settings")]

[Tooltip("Khoảng cách tâm visual slash so với nhân vật")]
public float slashVisualOffset = 0.8f;

[Tooltip("Scale visual cơ bản của slash")]
public Vector2 slashVisualScale = Vector2.one;

[Tooltip("Bù góc visual cho texture slash")]
public float slashRotationOffset = 0f;

[Tooltip("Có mirror texture khi quay trái hay không")]
public bool mirrorSlashOnLeft = false;
```

Optional:

```csharp
public float slashRevealDuration = 0.055f;
public float slashPeakDuration = 0.04f;
public float slashDissolveDuration = 0.10f;
```

Tuy nhiên nếu mọi slash cùng timing thì nên để timing trong prefab thay vì CharacterAttackConfig.

---

# 13. Thay đổi CharacterCombat

File:

```text
Assets/Features/Player/CharacterCombat.cs
```

Thay vì:

```csharp
Vector2 center =
    transform.position
    + direction * offset;
```

Dùng:

```csharp
Vector2 hitCenter =
    (Vector2)transform.position
    + direction * offset;

Vector2 visualCenter =
    (Vector2)transform.position
    + direction * attackConfig.slashVisualOffset;
```

Damage:

```csharp
Physics2D.OverlapBoxNonAlloc(
    hitCenter,
    boxSize,
    angle,
    ...
);
```

Visual:

```csharp
Quaternion visualRotation =
    Quaternion.Euler(
        0f,
        0f,
        angle + attackConfig.slashRotationOffset
    );

GameObject slash =
    VFXPoolManager.SpawnVFX(
        attackConfig.slashVfxPrefab,
        visualCenter,
        visualRotation,
        attackConfig.vfxDuration
    );
```

---

# 14. Combo Visual Scaling

Hiện `CharacterCombat` đã có:

```text
comboStep 1
comboStep 2
comboStep 3
```

Visual nên phản ánh combo.

Không cần ba prefab hoàn toàn riêng.

---

## Combo 1

```text
Scale:
0.90 – 0.95

Fragments:
2

AfterImage:
nhẹ

Peak brightness:
0.9 – 1.0
```

Cảm giác:

```text
nhanh
gọn
nhẹ
```

---

## Combo 2

```text
Scale:
1.00

Fragments:
3

AfterImage:
trung bình

Peak brightness:
1.0
```

Cảm giác:

```text
mạnh hơn combo 1
```

---

## Combo 3

```text
Scale:
1.10 – 1.20

Fragments:
4 – 5

AfterImage:
rõ hơn

Optional:
small shockwave
```

Cảm giác:

```text
finisher
nặng
```

Không đổi toàn bộ hue chỉ để biểu hiện combo mạnh hơn.

---

# 15. Hit Effect phải tách khỏi Slash

Slash:

```text
chuyển động của vũ khí
```

Hit spark:

```text
phản hồi va chạm với enemy
```

Project hiện đã có:

```text
SpawnHitImpactSparks()
```

Giữ nguyên nguyên tắc này.

Không nhúng hit spark vào slash prefab.

Lý do:

```text
đánh hụt
→ vẫn có slash

đánh trúng
→ slash + hit spark
```

---

# 16. Game Feel hiện tại

`CharacterCombat` đang có:

```text
Attack Lunge
Knockback
Hit Spark
Camera Shake
Hit Stop
Critical Feedback
```

Các hệ thống này giữ nguyên.

Slash rework chỉ cần đồng bộ peak visual với thời điểm damage.

---

# 17. Đồng bộ Peak với Damage

Rule:

```text
Slash visual peak
==
Damage active frame
==
Hitbox query
==
Main attack sound
```

Không nên:

```text
damage xảy ra trước slash
```

hoặc:

```text
slash peak xong mới damage
```

Mục tiêu là người chơi cảm nhận:

```text
nhìn thấy impact
=
nghe impact
=
enemy bị damage
```

---

# 18. PlayerAnimator — điểm cần kiểm tra

File:

```text
Assets/Features/Player/PlayerAnimator.cs
```

Hiện có:

```csharp
if (_currentState == newState) return;
```

Đối với locomotion:

```text
Idle
Run
```

đây là behavior hợp lý.

Nhưng Attack có tính chất event.

Nếu combo tiếp theo xảy ra trong lúc state vẫn là:

```text
Attack
```

thì animation có thể không restart.

Đề xuất thêm API:

```csharp
public void PlayAttack()
{
    if (animator == null) return;

    int hash = _stateHashes[PlayerAnimationState.Attack];

    animator.Play(
        hash,
        0,
        0f
    );

    _currentState = PlayerAnimationState.Attack;
}
```

Sau đó CharacterCombat gọi:

```csharp
playerAnimator.PlayAttack();
```

Thay vì dùng chung logic state change.

Phần này cần test trước khi merge vì còn phụ thuộc `PlayerController`.

---

# 19. VFX Pool Reset

File:

```text
Assets/Features/VFX/VFXPoolResetter.cs
```

Hiện component reset:

- ParticleSystem
- TrailRenderer

Đề xuất bổ sung reset transform state nếu prefab có runtime scale animation.

Cache:

```csharp
private Vector3 _originalLocalScale;
private Quaternion _originalLocalRotation;
```

Trong Awake / Cache:

```csharp
_originalLocalScale = transform.localScale;
_originalLocalRotation = transform.localRotation;
```

Reset:

```csharp
transform.localScale = _originalLocalScale;
transform.localRotation = _originalLocalRotation;
```

Nếu child SpriteRenderer được animate riêng, `SlashVFXAnimator` chịu trách nhiệm reset child.

---

# 20. Rủi ro pooled scale

`GlobalVFXPoolManager.PlayEffect()` hiện chỉ set scale nếu:

```csharp
scale.HasValue
```

Nếu một instance từng được spawn bằng scale khác, nhưng lần sau không truyền scale và component không reset, scale cũ có thể tồn tại.

Slash prefab mới phải đảm bảo:

```text
mọi runtime-modified transform
→ reset khi respawn
```

---

# 21. Weapon_DualSlash — palette cần chuẩn hóa

File:

```text
Assets/Features/Weapons/Weapon_DualSlash.cs
```

Hiện VFX thay màu theo level:

```text
Lv1–2:
#00FF66

Lv3–4:
#FF4500

Lv5–6:
#8A2BE2
```

Điều này không phù hợp với Visual DNA mới.

Đề xuất:

```text
Element palette cố định.
```

Ví dụ Hỏa:

```text
Dominant:
Deep Vermilion

Secondary:
Orange

Highlight:
Warm Ivory
```

Level tăng bằng:

```text
scale
particle count
secondary layer
shockwave
brightness
duration nhẹ
```

Không đổi từ xanh → đỏ → tím.

---

# 22. Texture slash production rule

AI image chỉ là source asset.

Không dùng raw output trực tiếp nếu chưa cleanup.

Pipeline:

```text
AI Slash
↓
Remove Background
↓
Clean Alpha
↓
Crop
↓
Normalize Canvas
↓
Simplify Tiny Noise
↓
Color Normalize
↓
Production PNG
↓
Unity Material
↓
Slash Prefab
```

Naming:

```text
AI_SLASH_Brush_001_RAW.png
```

Production:

```text
TX_VFX_Slash_Brush_A.png
```

Prefab:

```text
PF_VFX_BasicSlash_Brush
```

Material:

```text
MAT_VFX_Slash_BrushReveal
```

---

# 23. Sorting

Theo quy chuẩn hiện có:

```text
Slash / Impact / Foreground VFX:
Sorting Layer = Skill
Order = khoảng 10 – 15
```

MainSlash:

```text
Order 12
```

CoreSlash:

```text
Order 13
```

Fragments:

```text
Order 14
```

AfterImage:

```text
Order 11
```

Có thể tinh chỉnh theo prefab.

---

# 24. Recommended world size

Theo guideline hiện tại:

```text
Basic melee slash:
~1.2m – 1.8m visual diameter / coverage
```

Không để basic attack che nhiều màn hình.

Finisher có thể lớn hơn:

```text
~10 – 20%
```

không nên gấp đôi chỉ vì combo 3.

---

# 25. Performance Budget

Basic Slash L1:

```text
Main Sprite:
1

AfterImage:
1

Optional Core:
0–1

Particles:
2–4

Materials:
shared

Runtime Instantiate:
0

Runtime Destroy:
0

Pooling:
required
```

Target:

```text
Android
60 FPS
```

Không dùng:

- nhiều material instance;
- trail quá dài;
- nhiều translucent layer overlap;
- particle burst lớn;
- texture 2K cho basic slash.

Texture thường:

```text
256x256
hoặc
512x512
```

tùy crop và gameplay scale.

---

# 26. Acceptance Criteria

Slash rework được chấp nhận khi đạt:

## Visual

```text
[ ] Không còn cảm giác sprite bật lên tĩnh.
[ ] Có hướng chuyển động rõ.
[ ] Leading edge và trailing edge phân biệt được.
[ ] Slash xuất hiện nhanh hơn thời gian fade.
[ ] Có breakup / dissolve.
[ ] Không neon quá mức.
[ ] Phù hợp chibi character.
[ ] Không che silhouette nhân vật quá lâu.
```

## Combat

```text
[ ] Peak visual trùng thời điểm damage.
[ ] Hitbox không thay đổi ngoài chủ đích.
[ ] Attack range không bị thay đổi do visual.
[ ] Đánh hụt vẫn chỉ có slash.
[ ] Đánh trúng mới sinh hit spark.
[ ] Combo 1–2–3 đọc được khác nhau.
```

## Technical

```text
[ ] Slash dùng VFX pool.
[ ] Không Instantiate/Destroy mỗi hit.
[ ] Không tạo Material runtime instance.
[ ] Pool respawn không giữ scale/alpha cũ.
[ ] Particle được Clear khi return pool.
[ ] Không có ghost trail.
[ ] Không phát sinh GC đáng kể mỗi slash.
```

## Performance

```text
[ ] Stable khi spam attack.
[ ] Stable khi nhiều enemy cùng lúc.
[ ] Stable trên Android target.
[ ] Không overdraw quá lớn.
```

---

# 27. Test Cases

## TC01 — Basic attack spam

```text
Spam attack liên tục 30 giây.
```

Kiểm tra:

- slash reset đúng;
- không alpha ghost;
- không scale sai;
- không particle tồn dư.

---

## TC02 — Combo 1–2–3

Kiểm tra:

```text
Combo 1:
nhẹ

Combo 2:
trung bình

Combo 3:
finisher
```

Không được:

```text
ba slash nhìn giống hệt nhau.
```

---

## TC03 — Left / Right Direction

Attack:

```text
Right
Left
Diagonal Up
Diagonal Down
```

Kiểm tra:

- rotation đúng;
- reveal direction hợp lý;
- texture không bị lật sai;
- slash không tách khỏi nhân vật.

---

## TC04 — Attack Miss

Không có enemy.

Kỳ vọng:

```text
Slash
+
Slash sound
```

Không có:

```text
Hit spark
Hit stop mạnh
Enemy knockback
```

---

## TC05 — Attack Hit

Có enemy.

Kỳ vọng:

```text
Slash
+
Hit spark
+
Damage
+
Knockback
+
Camera feedback
```

Peak phải đồng bộ.

---

## TC06 — Critical Hit

Kỳ vọng:

```text
feedback mạnh hơn normal hit
```

Không cần tạo slash texture khác.

---

## TC07 — Pool reuse

Spawn cùng prefab:

```text
100+ lần
```

với scale/intensity khác nhau.

Kỳ vọng:

```text
không inherit state cũ.
```

---

# 28. Implementation Plan

## Phase 1 — Prototype

Tạo:

```text
TX_VFX_Slash_Brush_A
MAT_VFX_Slash_BrushReveal
PF_VFX_BasicSlash_Brush
SlashVFXAnimator
```

Chỉ test một character.

---

## Phase 2 — Combat Integration

Thay đổi:

```text
CharacterAttackConfig
CharacterCombat
```

Tách:

```text
hitCenter
visualCenter
```

---

## Phase 3 — Pool Safety

Review:

```text
VFXPoolResetter
GlobalVFXPoolManager
SlashVFXAnimator
```

Đảm bảo reset:

- scale;
- alpha;
- shader property;
- particles.

---

## Phase 4 — Combo Polish

Thêm:

```text
Combo 1 intensity
Combo 2 intensity
Combo 3 intensity
```

Không tạo duplicate prefab trừ khi art direction thật sự cần.

---

## Phase 5 — Art Bible Pass

Chuẩn hóa:

- palette;
- brush fragments;
- glow;
- slash thickness;
- size;
- timing.

Tạo Golden Reference:

```text
REF_01_BasicSlash
```

---

# 29. Files dự kiến thay đổi

## Existing files

```text
Assets/Features/Player/CharacterCombat.cs
Assets/Features/Player/CharacterEntry.cs
Assets/Features/Player/PlayerAnimator.cs
Assets/Features/VFX/VFXPoolResetter.cs
Assets/Features/Weapons/Weapon_DualSlash.cs
```

`PlayerAnimator.cs` và `Weapon_DualSlash.cs` có thể xử lý ở phase riêng.

---

## New files

```text
Assets/Features/VFX/Slash/SlashVFXAnimator.cs

Assets/VFX/Library/Textures/Trails/
    TX_VFX_Slash_Brush_A.png

Assets/VFX/Library/Materials/
    MAT_VFX_Slash_BrushReveal.mat

Assets/VFX/Prefabs/Common/
    PF_VFX_BasicSlash_Brush.prefab
```

Optional:

```text
Assets/VFX/Library/Shaders/
    SH_VFX_SpriteBrushReveal.shadergraph
```

---

# 30. Definition of Done

Feature được xem là hoàn thành khi:

```text
[ ] Basic slash dùng modular prefab.
[ ] Một texture slash có thể tạo cảm giác chuyển động.
[ ] Có reveal → peak → dissolve.
[ ] Có after-image.
[ ] Có 2–4 fragment.
[ ] Visual center tách hitbox center.
[ ] Combo 1–2–3 có visual intensity khác nhau.
[ ] Hit spark vẫn độc lập.
[ ] Pool reset hoàn chỉnh.
[ ] Không Material instance runtime.
[ ] Art style phù hợp gameplay hiện tại.
[ ] Test spam attack không lỗi.
[ ] Android performance đạt yêu cầu.
```

---

# 31. Kết luận

Thay đổi này không yêu cầu viết lại combat system.

Kiến trúc mục tiêu:

```text
CharacterCombat
    │
    ├── Gameplay
    │      ├── timing
    │      ├── hitbox
    │      ├── damage
    │      └── combo
    │
    └── Spawn VFX
           ↓
       Slash Prefab
           │
           ├── Main Brush
           ├── AfterImage
           ├── Fragments
           └── SlashVFXAnimator
```

Nguyên tắc quan trọng nhất:

> Slash không còn là “một ảnh xuất hiện”, mà là “một nét cọ được sinh ra, tăng tốc, đạt đỉnh và tan đi”.

Điều này cho phép Projectzombie tiếp tục dùng asset AI dưới dạng **source texture**, trong khi Unity kiểm soát chuyển động, timing, game feel và performance.
