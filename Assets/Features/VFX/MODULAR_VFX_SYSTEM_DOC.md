# 🎆 Báo Cáo & Mô Tả Kiến Trúc Hệ Thống Modular VFX & Global Pool

**Dự án:** Projectzombie (Vong Xuyên — Top-down Survival Roguelite 2D URP)  
**Phiên bản:** 4.0  
**Tác giả:** Antigravity AI (DeepMind Team)  

---

## 1. 📌 Tổng Quan Hệ Thống

Hệ thống VFX được xây dựng nhằm giải quyết triệt để vấn đề **rác bộ nhớ (GC Spikes)** và **lỗi dư hiệu ứng (Visual Ghosting/Leak)** khi spawn hàng trăm đòn đánh, đạn bay và hiệu ứng kỹ năng đồng thời trên Android Mobile (60 FPS).

### ✅ 3 Trụ Cột Kiến Trúc Chính:
1. **[GlobalVFXPoolManager.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Shared/VFX/GlobalVFXPoolManager.cs):** Singleton Object Pool tập trung quản lý cả `ParticleSystem` đơn lẻ lẫn `GameObject Modular VFX Prefab` (0 GC Allocation).
2. **[VFXPoolResetter.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/VFX/VFXPoolResetter.cs):** Component tự động dọn dẹp và reset trạng thái của tất cả `ParticleSystem` và `TrailRenderer` con mỗi khi GameObject được thu hồi về Pool.
3. **[VFXHierarchyGenerator.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Editor/VFX/VFXHierarchyGenerator.cs):** Unity Editor Tool 1-Click tự động sinh cấu trúc Hierarchy phân bóc 4 Category riêng biệt.

---

## 2. 🗂️ Phân Loại 4 Category Modular VFX

| Category | Mô Tả | Cấu Trúc Layers | Cách Spawn |
|---|---|---|---|
| ☯️ **SignatureSkill** | Kỹ năng chủ động diện rộng của 3 nhân vật chính | `PS_GroundDecal`<br>`PS_AuraSwirl`<br>`PS_SkillMain`<br>`PS_SparksBurst` | Spawn qua Script Skill / `GlobalVFXPoolManager` |
| 🗡️ **WeaponAttack** | Vệt chém / vung của vũ khí cận chiến | `PS_MuzzleSwing`<br>`PS_SlashArc`<br>`PS_SlashGlow`<br>`PS_SparksBurst` | `GlobalVFXPoolManager.Instance.PlayEffect()` |
| 🔫 **BulletProjectile** | Lõi đạn và vệt bay đuôi đạn (KHÔNG chứa Nổ) | `PS_BulletCore`<br>`PS_BulletTrail`<br>`PS_MuzzleFlash` | Qua `ProjectileSystem` / `GlobalVFXPoolManager` |
| 💥 **HitImpact** | Nổ va chạm độc lập trên thân Zombie | `PS_ImpactBurst`<br>`PS_ImpactSparks`<br>`PS_ImpactSmoke` | `GlobalVFXPoolManager.Instance.PlayEffect()` tại tọa độ va chạm |

---

## 3. ☯️ Thiết Kế Hiệu Ứng Signature Skill 3 Nhân Vật Chính

### 3.1. Đạo Sĩ — *"Bát Quái Trận Đồ"* (`PF_Skill_BatQuaiTranDo`)
- **Visual:** Mặt đất bùng lên Trận Đồ Bát Quái 8 cạnh (bán kính 4.5m, màu Xanh Mộc `#32CD32`), 8 lá Bùa Trấn Yêu bay lơ lửng tại 8 đỉnh bát giác kết nối bằng dây linh phù, 2 luồng Âm/Dương xoáy từ mép vào tâm.
- **Cơ chế:** Khóa pathing quái + ép `yinYangValue` về 50 trong 4s.

### 3.2. Thư Sinh — *"Phán Quyết Tiền Định"* (`PF_Skill_PhanQuyetTienDinh`)
- **Visual:** Vệt mực thư pháp nhòe dưới chân (màu Vàng Kim `#FFD700`), trail mực đen xoay tròn quanh Bút Phán Quan, Triện Ấn Bát Quái/Chữ Nôm bùng nổ rực rỡ trên đầu trong 1.5s.
- **Cơ chế:** Chèn Virtual Element Hit vào Queue Tương Sinh.

### 3.3. Võ Tăng — *"Phá Giới Chấn Thế"* (`PF_Skill_PhaGioiChanThe`)
- **Visual:** Võ Tăng đấm xuống đất hy sinh 30% HP $\rightarrow$ Nứt đất 8 hướng tỏa rộng (3m-7m), Sóng xung kích (Shockwave Ring) đỏ cam chói lóa quét ra cực nhanh Knockback 8m/s + Choáng 1.2s.
- **Cơ chế:** Cộng thẳng +25 điểm Dương vào `yinYangValue` + Rung màn hình (Camera Shake).

---

## 4. 💻 Hướng Dẫn Sử Dụng Code API

### Gọi Spawn VFX Đơn Giản từ C# Script:

```csharp
using ProjectZombie.Features.Shared.VFX;

// 1. Spawn Hit Impact tại điểm va chạm
GlobalVFXPoolManager.Instance.PlayEffect(hitImpactPrefab, hitPosition, Quaternion.identity, 0.4f);

// 2. Spawn Signature Skill Prefab tại chân Nhân vật
GlobalVFXPoolManager.Instance.PlayEffect(skillZonePrefab, playerPos, Quaternion.identity, 4.0f);
```

---

## 🛠️ Hướng Dẫn Sinh Prefab Tự Động Trong Editor

1. Mở menu: **`Tools > VFX Generator > Create Modular VFX Hierarchy`**.
2. Chọn Category (`SignatureSkill`, `WeaponAttack`, `BulletProjectile`, `HitImpact`).
3. Tích chọn `Attach VFXPoolResetter`.
4. Nhấn **`🚀 TẠO HIERARCHY VFX MODULAR`**. Tool sẽ tự tạo GameObject Tree, gán Material URP Additive và gán sẵn script reset tự động.
