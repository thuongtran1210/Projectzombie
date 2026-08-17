# Danh Sách Nhiệm Vụ & Task Tracker — VONG XUYÊN

Tài liệu quản lý danh sách công việc (Kanban Board Task Tracker) phân chia theo các hạng mục của dự án **Vong Xuyên (Android Release - GDD v4.0)**.
*Tài liệu tham chiếu chi tiết:* 🎨 **[UI_ART_DIRECTION_GUIDE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/UI_ART_DIRECTION_GUIDE.md)** | 📖 **[GAMEPLAY_PRODUCTION_GUIDE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/GAMEPLAY_PRODUCTION_GUIDE.md)** | 📐 **[SYSTEM_ARCHITECTURE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/SYSTEM_ARCHITECTURE.md)**

---

## 📋 Kanban Board Summary

| 🔴 Backlog (Sắp Tới) | 🟡 In Progress (Đang Triển Khai) | 🟢 Done (Đã Hoàn Thành) |
|---|---|---|
| 📱 **[TASK-314] Mobile Resolution & Anchor Layout Adaptation** | 🎨 **[TASK-VFX-Core] Bộ 4 Shader Graph URP Cho 2D Mobile** | ☯️ **[Hạng Mục 1] Lõi Ngũ Hành, Âm Dương & 0-Alloc Combat** |
| 🪓 **[TASK-316] Hệ Thống UI Trực Quan Ngũ Hành (Bát Quái / Glow)** | ⚔️ **[TASK-VFX-01] Vệt Chém Bút Phán Quan & Luồng Lửa Đao Cửu Vĩ** | 📦 **[Hạng Mục 2] Database 12 Pháp Bảo, 12 Tiến Hóa & Cổ Tiền** |
| 💥 **[TASK-GP-03] Treasure Chest Gacha Popup (1-3-5 Items)** | 🌀 **[TASK-VFX-02] Orbit Trail Bùa Trấn Yêu & Phi Tiêu Bát Quái** | 🧟 **[Hạng Mục 3] AI 5 Yêu Ma, 2 Boss & 20-Min Wave Timeline** |
| 🌊 **[TASK-GP-04] Circle Swarm Spawner Events (Phút 3, 7, 12)** | 🔔 **[TASK-VFX-03] Sóng Âm Trống Đồng & Địa Chấn Lựu Đạn** | 📱 **[TASK-300] Mobile Controls (Joystick, Dash, Skill Buttons)** |
| 🗜️ **[TASK-401] Texture Compression ASTC & Sprite Atlas** | ⚡ **[TASK-VFX-04] Đạn Bay Nỏ Thần, Sét Long Vương & Khói Ma Da** | 📊 **[TASK-312 & 313] In-Game HUD Visuals & Upgrade Cards UI** |
| 🧪 **[TASK-402] Mobile Stress Test (200 Mobs 60 FPS)** | ✨ **[TASK-VFX-05] Hit Sparks, Hit Stop & Sorting Layer Polish** | 💎 **[TASK-EXP-01] Hệ Thống Hạt EXP Zero-Alloc, Magnet & Audio Pitch** |
| 🚀 **[TASK-403] Android AAB Build & Signing Release** | ⚡ **[TASK-GP-01] Hit Flash URP Shader & Knockback Feedback** | 🇻🇳 **[TASK-315] Vietnamese TMP Dynamic Fallback Font** |
| | 📳 **[TASK-GP-02] Screen Shake & Damage Popup Scale Polish** | 🏛️ **[TASK-303, 304, 305] Meta Shop, Hero Select & Run Summary** |
| | 🎵 **[TASK-GP-05] Audio Feedback Layers (Hit/Death/Chime)** | |

---

## 🏃 Pipeline Thực Thi Chi Tiết (Execution Pipeline)

```mermaid
graph TD
    A["✅ Phase 1: Core Systems, Data & Waves (DONE)"] --> B["✅ Phase 2: Mobile UI MVP & Controls (DONE)"]
    B --> C["🟡 Phase 3: Shaders, VFX & Visual Polish (CURRENT)"]
    C --> D["🟡 Phase 4: Game Feel & Audio Juice (NEXT)"]
    D --> E["🔴 Phase 5: UI Polish, Performance & Store Release (FINAL)"]
```

---

### ✅ Phase 1: Hệ Thống Cốt Lõi, Database & AI Wave (Completed)

#### ☯️ Hạng Mục 1.1: Lõi Cơ Chế Ngũ Hành & Âm Dương
- [x] **[TASK-101]** Refactor `DamageUtility.cs` và `DamageData.cs` hỗ trợ tra cứu Tương Khắc Ngũ Hành (+30% Sát thương).
- [x] **[TASK-102]** Tích hợp Element Combo Tracker vào `WeaponManager.cs` (-20% Cooldown khi kích hoạt Tương Sinh trong 3s).
- [x] **[TASK-103]** Xây dựng `YinYangManager.cs` theo dõi trạng thái Âm Dương (0-100) và phát sự kiện `OnYinYangStateChanged`.
- [x] **[TASK-104]** Tích hợp lọc pool thẻ Gacha nâng cấp ngẫu nhiên trong `UpgradeManager.cs` theo `YinYangState`.
- [x] **[TASK-105]** Tạo `BossElementController.cs` cho phép Boss (Ngưu Đầu Mã Diện & Diêm Vương) luân phiên xoay vòng thuộc tính Ngũ Hành theo chu kỳ 10s.
- [x] **[TASK-106] Tích Hợp Toàn Diện Sát Thương Ngũ Hành & Vòng Tương Sinh (Clean Architecture & SOLID):**
  - [x] **[TASK-106.1] Chuẩn Hóa IDamageDealer & WeaponBase (Attacker Element Pipeline):**
    - Cập nhật `WeaponBase` cung cấp `AttackElement = this.element` và `CreateDamageData()`.
    - Refactor các lớp vũ khí con (`Weapon_Crossbow`, `Weapon_Boomerang`, `Weapon_DualSlash`, `Weapon_Flamethrower`, `Weapon_MeleeBase`, `PetController`,...) truyền đúng `this.element` khi tạo `DamageData` / spawn Projectile.
  - [x] **[TASK-106.2] Chuẩn Hóa IDamageable & Hit Resolution (Defender Element & Counter Multiplier):**
    - Cung cấp `CurrentElement` trên `Enemy` và `BossElementController`.
    - Tại điểm va chạm (`ProjectileCollision.cs` & `Weapon_MeleeBase.DealDamageInArea`), áp dụng tính toán tương khắc 1 chiều qua `DamageUtility.CalculateHitDamage(attackerElement, defenderElement)` (nhân `×1.3` nếu khắc hệ).
    - Đánh dấu cờ `IsCounter` trong `DamageData` và `DamageReport` để phục vụ visual feedback.
  - [x] **[TASK-106.3] Tích Hợp Single Source of Truth Vòng Tương Sinh (`ElementCycleManager`):**
    - Dọn dẹp logic combo thủ công trong `WeaponManager.cs`, quy tụ về `ElementCycleManager`.
    - Gọi `ElementCycleManager.Instance.RegisterHit(attackerElement, sourceWeapon)` khi đòn đánh trúng kẻ địch.
    - Đảm bảo cơ chế cooldown refund 20% trừ trực tiếp trên `WeaponBase.ReduceCurrentCooldown()` mượt mà, zero-allocation và tôn trọng giới hạn 1 proc / 3s.
    - Bổ sung sự kiện `OnElementSynergyTriggered` kích hoạt -20% Cooldown cho vũ khí trúng đòn và phát tín hiệu cho UI/VFX/Audio.
  - [x] **[TASK-106.4] Visual & Audio Feedback Tương Khắc / Tương Sinh (Game Feel):**
    - Kích hoạt màu Vàng Kim rực rỡ (`#FFD700`) + ký hiệu `✦` trên Floating Damage Text khi đòn đánh đạt điều kiện Tương Khắc trong `DamageTextManager` và `DamageTextStyleConfig`.
    - Phát âm thanh *Ting!* + hiệu ứng khi proc Tương Sinh theo đúng GDD v4.0.
  - [x] **[TASK-OPT-01] Tối Ưu Hóa Toàn Diện Physics & Combat Hot-path (Zero-Alloc & LayerMask Filtering):**
    - Xây dựng `TargetingUtility.cs` với cache `EnemyLayerMask` (Bitwise O(1)) và Reservoir Sampling 0-Alloc.
    - Refactor 8 vũ khí tầm xa, `Weapon_MeleeBase`, `ExplosionBehavior`, `HomingBehavior`, `VampiricBehavior`, `PetController`, `SuicideExplodeBehavior`, `BatQuaiTranZone`, `VoTangSignatureSkill`.
    - Triệt tiêu 100% việc gọi `Physics2D.OverlapCircleAll` tạo mảng rác trên Heap và bỏ qua `CompareTag` dư thừa khi đã lọc LayerMask.

#### 🗡️ Hạng Mục 1.2: Reskin Content, Database & Kinh Tế Cổ Tiền
- [x] **[TASK-201]** Thêm trường `elementType` vào ScriptableObject `WeaponData.cs` và `EnemyConfig.cs`.
- [x] **[TASK-202]** Tạo `WeaponDataGenerator.cs` tự động sinh/cập nhật dữ liệu `.asset` cho 12 Pháp Bảo MVP và 12 Evolution theo GDD 4.0.
- [x] **[TASK-203]** Tạo `EnemyDataGenerator.cs` tự động sinh/cập nhật dữ liệu `.asset` cho 5 Yêu Ma MVP (Ma Giáp, Ma Trơi, Quỷ Nhập Tràng, Ma Da, Hồ Ly Tinh Nhỏ).
- [x] **[TASK-204]** Chuyển đổi tên gọi đơn vị tiền Meta thành **Cổ Tiền** (tiền xu cổ Việt Nam) trong `MetaCurrencyManager.cs` và `MetaProgressionSaveData.cs`.
- [x] **[TASK-205]** Tạo `WeaponEvolutionManager.cs` kiểm tra điều kiện ghép 12 Vũ Khí Tiến Hóa theo ma trận GDD v4.0.
- [x] **[TASK-206]** Tạo `ProjectilePrefabGenerator.cs` & `ProjectileBehaviorDataGenerator.cs` sinh 12 Prefabs đạn và gán Behaviors SOs.
- [x] **[TASK-207]** Tạo `WaveDataGenerator.cs` tự động sinh 15 Wave Config ScriptableObjects bám sát Timeline 15 Phút GDD v4.0.

#### 🧟 Hạng Mục 1.3: AI Yêu Ma, Boss Mechanics & 20-Min Wave Timeline
- [x] **[TASK-306]** Tích hợp cơ chế **Cản Đạn (`Heavy Armor Bullet Sponge` - GDD 5.1)** cho Quỷ Nhập Tràng (`E_QUYNHAPTRANG`) trong `PierceBehavior.cs`.
- [x] **[TASK-307]** Viết `SuicideExplodeBehavior.cs` cho Hồ Ly Tinh Nhỏ (`E_HOALYTINH`) áp sát nổ AoE 50 Damage.
- [x] **[TASK-308]** Xây dựng `BossStateMachine.cs`, `BullDashSkill.cs` (Ngưu Xung Thiên), `GroundSlamSkill.cs` (Địa Chấn Âm Ty) cho Boss 1 Ngưu Đầu Mã Diện.
- [x] **[TASK-309]** Tích hợp **Anti-Cornering Guard (GDD 7.0)** cho `EnemySpawner.cs` dồn 70% quái hướng về trung tâm khi Player sát tường $<5\text{m}$.
- [x] **[TASK-310]** Tái cấu trúc Spawner System quy về mô hình Master-Worker chuẩn Data-Driven (`SpawnManager` & `EnemySpawner`).
- [x] **[TASK-311]** Tách biệt 100% AI Boss bằng `BossMovementStrategy.cs` và `BossMeleeAttackStrategy.cs` độc lập với quái thường.
- [x] **[TASK-E01] -> [TASK-E14]** Hoàn thiện 5 Yêu ma, 2 Boss Prefabs/SOs, 2 Rương báu và toàn bộ 20 Waves cho trận đấu 20 phút.

---

### ✅ Phase 2: Mobile UI MVP Architecture & Core Controls (Completed)

- [x] 🚨 **[TASK-300] Setup Mobile Controls Canvas**:
  - Dựng `DynamicVirtualJoystick.cs` (OnScreenControl), `SignatureSkillButtonView` + `SignatureSkillPresenter` (Nút bấm Skill chủ động) & `DashButtonView` + `DashButtonPresenter` chuẩn New Input System & MVP.
  - Xây dựng Editor Tool `MobileControlsSetupTool.cs` tự động dựng và wire toàn bộ Mobile Controls vào Scene.
- [x] 🚨 **[TASK-312] Refactor & Refine In-Game HUD Visuals (`RunHUDView.cs` & `RunHUDPresenter.cs`)**:
  - [x] Visual & Contrast cho Health Bar (`#D13838`), EXP Bar (`#4DEEEA`) & Level Text (`#FFD700`).
  - [x] Redesign Slider Cán Cân Âm Dương (`CharacterGaugeWidgetView.cs` & `TaoistYinYangTracker.cs`) với 3 trạng thái Âm Thịnh / Cân Bằng / Dương Thịnh.
  - [x] Overhead Boss Element Indicator: Hiển thị Badge ngũ hành trực tiếp trên đầu Boss.
  - [x] Tinh gọn Top HUD, phân định rõ rệt giữa HUD chính và Panel Âm Dương.
- [x] 🚨 **[TASK-313] Refine Upgrade Cards Gacha UI (Stat Diff & Badges)**:
  - [x] Xây dựng bộ format chỉ số `IUpgradeStatFormatter` & `UpgradeStatFormatter` (Rich Text).
  - [x] Chuẩn hóa Badge Ngũ Hành qua `ElementVisualHelper.cs`.
  - [x] View thụ động `UpgradeCardView.cs` hỗ trợ `_statDiffText` và `UnscaledTime`.
  - [x] Dynamic Card Container & Object Pooling trong `UpgradeUIView.cs` (Zero-Alloc, tái sử dụng thẻ linh hoạt 1-3-5 cards).
  - [x] Tool tự động hóa `UpgradeUIHierarchyOptimizer.cs`.
- [x] 🚨 **[TASK-315] Setup Vietnamese TMP Dynamic Fallback Font Asset**:
  - Cấu hình Dynamic Font Fallback `LiberationSans SDF - Fallback.asset` hỗ trợ full dải ký tự tiếng Việt (`0x1EA0-0x1EF9`) và biểu tượng `☯` (`\u262F`).
  - Tool Editor `TMPVietnameseFontSetupTool.cs`.
- [x] **[TASK-301] - [TASK-305] Các Màn Hình UI Phụ Trợ (MVP Pattern):**
  - [x] `MetaUpgradeShopView/Presenter.cs`: Giao diện Cây nâng cấp vĩnh viễn dùng Cổ Tiền.
  - [x] `CharacterSelectionView/Presenter.cs`: Giao diện Chọn Tướng (Thư Sinh, Đạo Sĩ, Võ Tăng).
  - [x] `RunSummaryView/Presenter.cs`: Giao diện Tổng kết trận đấu và cộng Cổ Tiền tích lũy.
- [x] 🚨 **[TASK-EXP-01] Hệ Thống Hạt EXP & Magnet Tối Ưu (Performance & Game Feel)**:
  - [x] `PlayerMagnetTrigger.cs`: Bắt vùng hút động theo chỉ số `PickupRange`.
  - [x] Tối ưu homing toán học Zero-Alloc (`ExpGemPoolManager`), không dùng DOTween per gem.
  - [x] Cơ chế tự động gộp hạt (Gem Compression/Merging) khi vượt ngưỡng 150 hạt trên sân.
  - [x] Phân cấp màu sắc Gem (Lam Ngọc, Lục Bảo, Hoàng Kim) và Pitch Scaling Audio (`1.0f -> 1.5f`).

---

### 🟡 Phase 3: Shaders, Modular VFX & Vũ Khí Polish (CURRENT FOCUS)
*Tài liệu tham chiếu chi tiết:* 🎨 **[UI_ART_DIRECTION_GUIDE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/UI_ART_DIRECTION_GUIDE.md)** | 🥞 **[SORTING_LAYERS_GUIDE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/SORTING_LAYERS_GUIDE.md)** | 💥 **[weapon_dual_slash_vfx_suggestions.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/weapon_dual_slash_vfx_suggestions.md)**

- [ ] 🚨 **[TASK-VFX-Core] Xây Dựng Bộ Shader Graph URP Chuẩn Cho VFX 2D Mobile**:
  - [ ] **[TASK-VFX-Core.1] Shader Graph Vệt Chém / Kiếm Khí (`Shader_Slash_Additive`):** Polar UV Scrolling + Dissolve Noise + Rim Glow.
  - [ ] **[TASK-VFX-Core.2] Shader Graph Sóng Xung Kích / Biến Dạng (`Shader_Distortion_Shockwave`):** Scene Color + Normal Wave cho đòn nổ/AoE.
  - [ ] **[TASK-VFX-Core.3] Shader Graph Vết Nứt Đất (`Shader_GroundDecal_Dissolve`):** Dissolve Burn Edge trên nền đất 0.5s.
  - [ ] **[TASK-VFX-Core.4] Shader Graph Hit Flash Siêu Nhẹ Cho Quái Vật (`Shader_Sprite_HitFlash`):** Nháy trắng/vàng kim 0.05s trực tiếp trên Sprite Shader (0 GC).

- [ ] 🚨 **[TASK-VFX-01] [W002 & W008] Vệt Chém Đa Hướng & Luồng Lửa (`Bút Phán Quan` & `Đao Cửu Vĩ`)**:
  - [ ] **Dynamic Slash Placement:** Spawn Crescent Slash VFX tại `hitCenter` xoay góc Z theo hướng đánh thực tế.
  - [ ] **Multi-layer Slash Effect:** Tạo Prefab 3 lớp (`Core_White`, `Slash_Arc_Glow`, `Ink_Splash_Sparks`).
  - [ ] **Visual Progression:** Nâng cấp visual theo Level 1 -> 4 -> Tiến Hóa `E002` (Vòng xoáy Âm Dương).
  - [ ] **Đao Cửu Vĩ (W008):** Luồng rồng lửa phun liên tục dạng nón xoắn ốc kèm tàn lửa.

- [ ] 🚨 **[TASK-VFX-02] [W003 & W012] Vệt Linh Khí Orbit & Phi Tiêu Bát Quái (`Bùa Trấn Yêu` & `Phi Tiêu Bát Quái`)**:
  - [x] Tangent Rotation: Xoay tiếp tuyến lá bùa theo chiều quay.
  - [ ] Trail & Glow Ribbon: Gắn Trail Renderer Shader phát sáng theo sau lá bùa và luồng gió xoáy phi tiêu.
  - [ ] Impact Pulse: Sóng đẩy lùi tí hon (Micro Repulsion Pulse) tại điểm va chạm quái.

- [ ] 🚨 **[TASK-VFX-03] [W005, W006, W011] Sóng Âm Trấn Quốc & Địa Chấn (`Trống Đồng`, `Lựu Đạn`, `Nước Thánh`)**:
  - [ ] **Trống Đồng Đông Sơn (W005):** Sóng âm hoa văn Trống Đồng dập xuống mặt đất + Vết nứt đất `GroundDecal`.
  - [ ] **Lựu Đạn Thần Sa (W006):** Hiệu ứng nổ bão lửa 3 tầng (Khói nén -> Cột lửa -> Tàn lửa).
  - [ ] **Nước Thánh Chùa Hương (W011):** Bãi nước thiêng bốc sương trắng gợn sóng (Sorting Layer `VFX_Bottom`).

- [ ] 🚨 **[TASK-VFX-04] [W001, W004, W007, W009, W010, W013] Đạn Bay, Sét Dây & Triệu Hồi Linh Thú**:
  - [ ] **Nỏ Thần (W001) & Cung Thạch Sanh (W007):** Mũi tên ánh sáng vệt đuôi dài + vòng xé gió (Wind Pierce Ring).
  - [ ] **Cửu Vĩ Hồ Trảo (W004):** Móng vuốt cáo lửa cào xé + vệt sinh khí xanh ngọc bay ngược về Player.
  - [ ] **Trượng Long Vương (W009):** Chuỗi sét nước `LineRenderer` giật nối 6 mục tiêu kèm tóe nước điện tích.
  - [ ] **Linh Phù Ma Da (W010) & Cờ Âm Binh (W013):** Hiệu ứng khói sương đầm lầy và làn sương đen âm ty triệu hồi.

- [ ] 🚨 **[TASK-VFX-05] Tích Hợp Game Feel & Hit Impact Polish Cho Toàn Bộ Vũ Khí**:
  - [ ] **Hit Sparks Pool:** Pool Particle Prefab tóe tia sáng/mảnh vụn theo 5 nguyên tố.
  - [ ] **Hit Stop (Frame Freeze):** Dừng `Time.timeScale = 0.05f` trong `0.04s` khi Crit / Proc Evolution.
  - [ ] **Chuẩn Hóa 3 Tầng Sorting Layer:** `VFX_Bottom` (Decal), `VFX_Middle` (Đạn/Slash/Orbit), `VFX_Top` (Hit Sparks/Nổ).

---

### 🟡 Phase 4: Gameplay Production Feel & Dynamic Audio (NEXT PHASE)
*Tài liệu tham chiếu chi tiết:* 📖 **[GAMEPLAY_PRODUCTION_GUIDE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/GAMEPLAY_PRODUCTION_GUIDE.md)** | 🔊 **[AUDIO_SYSTEM_GUIDE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/AUDIO_SYSTEM_GUIDE.md)**

- [ ] 🚨 **[TASK-GP-01] Hit Flash URP Shader & Knockback Feedback**:
  - Nháy trắng (`Flash White 0.05s`) trên Sprite quái vật khi nhận damage trong `ZombieHealth.cs`.
  - Lực đẩy lùi vật lý `_knockbackForce` cho các đòn đánh nặng (Trống Đồng W005, Cọc Gỗ) tạo khoảng thở.
- [ ] 🚨 **[TASK-GP-02] Screen Shake & Damage Popup Feedback**:
  - Tích hợp Camera Shake (`0.08s`, biên độ `0.15`) khi đòn đánh Chí Mạng hoặc Nổ AoE.
  - Nâng cấp Damage Popup: Phân biệt rõ Sát thương Thường (Trắng), Tương Khắc (Vàng Kim `#FFD700` + `✦`), Chí Mạng (Đỏ Cam Scale Punch).
- [ ] 🚨 **[TASK-GP-03] Treasure Chest Gacha Popup (1-3-5 Items) & Evolution Ceremony**:
  - Popup mở rương báu rơi từ Elite/Boss (Rương Gỗ: 1 món, Rương Bạc: 3 món, Rương U Minh: 5 món + Instant Evolution).
  - Hiệu ứng Cleanse Blast (quét sạch quái cận chiến) khi hoàn thành ghép Pháp Bảo Tối Thượng.
- [ ] 🚨 **[TASK-GP-04] Circle Swarm Spawner Events (Tăng nhịp độ căng thẳng)**:
  - Bổ sung Event quái bao vây 360 độ siết chặt tại: Phút 03:00 (Ma Trơi), Phút 07:00 (Ma Da), Phút 12:00 (Quỷ Nhập Tràng).
  - Buộc người chơi dùng cơ chế `Dash` (i-frame 0.2s) để đục thủng vòng vây.
- [ ] 🚨 **[TASK-GP-05] Polish Audio Feedback Layers**:
  - Tích hợp âm thanh phản hồi va chạm (Hit SFX), âm thanh quái tan biến (Death Pop) và chuỗi chuông hút ngọc EXP.

---

### 🔴 Phase 5: Onboarding UI, Performance & Đóng Gói Phát Hành (FINAL PHASE)

- [ ] 🚨 **[TASK-314] Mobile Screen Resolution & Anchor Layout Adaptation**:
  - Cấu hình `CanvasScaler`: `Scale With Screen Size` (1920x1080, `Match Width Or Height` = 0.5).
  - Tinh chỉnh Anchor Points tránh tràn viền trên các tỉ lệ màn hình dài (16:9, 19.5:9, 20:9).
- [ ] 🚨 **[TASK-316] Hệ Thống UI Trực Quan Ngũ Hành Cho Người Chơi Mới (MVP)**:
  - [ ] **[TASK-316.1] Gợi Ý Tương Sinh (Synergy Glow & Hint) Trên Thẻ Gacha:** Viền sáng xanh ngọc + tooltip gợi ý khi xuất hiện thẻ tương sinh.
  - [ ] **[TASK-316.2] HUD Quick Reference Wheel (Vòng Bát Quái Tra Cứu Nhanh):** Hold touch 1s để soi sơ đồ tương sinh tương khắc.
  - [ ] **[TASK-316.3] Progressive Onboarding Banner Tutorial:** Toast hướng dẫn 3s khi lần đầu kích hoạt tương khắc / tương sinh trong Run 1.
  - [ ] **[TASK-316.4] Tab Sơ Đồ Ngũ Hành Trong Pause Menu & Codex.**
- [ ] **[TASK-401] Texture Compression ASTC & Sprite Atlas**:
  - Gom toàn bộ UI/Sprites vào Sprite Atlas 2048x2048, nén **ASTC 6x6** cho Android (Draw Calls < 30).
- [ ] **[TASK-402] Mobile Stress Test (200 Mobs 60 FPS)**:
  - Thử nghiệm 200 Yêu ma + 100 Projectiles kiểm tra FPS và GC Spikes (Target 60 FPS).
- [ ] **[TASK-403] Android AAB Build & Signing (Google Play Release)**:
  - Cấu hình Build Profile IL2CPP ARM64, Target API 33+, xuất file `.aab` có ký số phát hành Google Play Store.
