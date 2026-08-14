# Danh Sách Nhiệm Vụ & Task Tracker — VONG XUYÊN

Tài liệu quản lý danh sách công việc (Kanban Board Task Tracker) phân chia theo các hạng mục của dự án **Vong Xuyên (Android Release - GDD v4.0)**.
*Tài liệu tham chiếu chi tiết:* 🎨 **[UI_ART_DIRECTION_GUIDE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/UI_ART_DIRECTION_GUIDE.md)** | 📖 **[GAMEPLAY_PRODUCTION_GUIDE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/GAMEPLAY_PRODUCTION_GUIDE.md)** | 📐 **[SYSTEM_ARCHITECTURE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/SYSTEM_ARCHITECTURE.md)**

---

## 📋 Kanban Board Summary

| 🔴 To Do | 🟡 In Progress | 🟢 Done |
|---|---|---|
| 🚨 **[TASK-316] Hệ Thống UI Trực Quan Ngũ Hành Cho Người Chơi Mới (MVP & Clean Code)** | 🚨 **[TASK-300] Setup Mobile Controls Canvas (Joystick & Skill/Dash Buttons)** | 🚨 **[TASK-315] Setup Vietnamese TMP Dynamic Fallback Font** |
| 🚨 **[TASK-GP-01] Hit Flash Shader & Knockback Physics** | 🚨 **[TASK-VFX-01] Dynamic Slash VFX Placement (`Bút Phán Quan` W002)** | 🚨 **[TASK-EXP-01] Tối Ưu & Nâng Cấp Hệ Thống Hạt EXP & Magnet** |
| 🚨 **[TASK-GP-02] Screen Shake & Critical Damage Feedback** | 🚨 **[TASK-VFX-02] Trail & Orbit Polish (`Bùa Trấn Yêu` W003)** | 🚨 **[TASK-106] Tích Hợp Sát Thương Ngũ Hành & Tương Sinh** |
| 🚨 **[TASK-GP-03] Treasure Chest Gacha Popup & Evolution Ceremony** | 🚨 **[TASK-VFX-03] Shockwave & Ground Decals (`Trống Đồng` W005 & Võ Tăng)** | Master-Worker Spawner Refactoring |
| 🚨 **[TASK-GP-04] Circle Swarm Spawner Events (Min 3, 7, 12)** | 🚨 **[TASK-VFX-04] Hit Impact & Hit Flash Shader (`Hit Impact` & Zombie)** | Android Target Platform Refactor |
| 🚨 **[TASK-GP-05] Polish Audio Feedback Layers (Hit/Gem/LevelUp)** | 🚨 **[TASK-314] Mobile Resolution & Anchor Layout Adaptation** | Local Save System (`SaveSystem.cs`) |
| Mobile Stress Test (200 Mobs 60 FPS) | | Ngũ Hành Damage & Combo System |
| Texture Compression ASTC & Sprite Atlas (Tối ưu cuối) | | Cán Cân Âm Dương (`YinYangManager`) |
| Android AAB Build & Signing (Google Play Release) | | Boss AI Dynamic Element (`BossElementController`) |
| | | 12 Pháp Bảo & 5 Yêu Ma Data SOs |
| | | Enemy Prefabs & Boss Data ([TASK-E01] -> [TASK-E14]) |
| | | 🚨 **[TASK-312] Refactor & Refine In-Game HUD Visuals** |
| | | 🚨 **[TASK-313] Refine Upgrade Cards Gacha UI (Stat Diff & Badges)** |

---

## 🏃 Sprint Tasks Breakdown

### ☯️ Hạng Mục 1: Lõi Cơ Chế Ngũ Hành & Âm Dương
- [x] **[TASK-101]** Refactor `DamageUtility.cs` và `DamageData.cs` hỗ trợ tra cứu Tương Khắc Ngũ Hành (+30% Sát thương).
- [x] **[TASK-102]** Tích hợp Element Combo Tracker vào `WeaponManager.cs` (-20% Cooldown khi kích hoạt Tương Sinh trong 3s).
- [x] **[TASK-103]** Xây dựng `YinYangManager.cs` theo dõi trạng thái Âm Dương (0-100) và phát sự kiện `OnYinYangStateChanged`.
- [x] **[TASK-104]** Tích hợp lọc pool thẻ Gacha nâng cấp ngẫu nhiên trong `UpgradeManager.cs` theo `YinYangState`.
- [x] **[TASK-105]** Tạo `BossElementController.cs` cho phép Boss (Ngưu Đầu Mã Diện & Diêm Vương) luân phiên xoay vòng thuộc tính Ngũ Hành theo chu kỳ 10s.
- [x] 🚨 **[TASK-106] Tích Hợp Toàn Diện Sát Thương Ngũ Hành & Vòng Tương Sinh (Clean Architecture & SOLID)**:
  - [x] **[TASK-106.1] Chuẩn Hóa IDamageDealer & WeaponBase (Attacker Element Pipeline):**
    - Cập nhật `WeaponBase` cung cấp `AttackElement = this.element` và `CreateDamageData()`.
    - Refactor các lớp vũ khí con (`Weapon_Crossbow`, `Weapon_Boomerang`, `Weapon_DualSlash`, `Weapon_Flamethrower`, `Weapon_MeleeBase`, `PetController`,...) truyền đúng `this.element` khi tạo `DamageData` / spawn Projectile.
  - [x] **[TASK-106.2] Chuẩn Hóa IDamageable & Hit Resolution (Defender Element & Counter Multiplier):**
    - Cung cấp `CurrentElement` trên `Enemy` và `BossElementController`.
    - Tại điểm va chạm (`ProjectileCollision.cs` & `Weapon_MeleeBase.DealDamageInArea`), áp dụng tính toán tương khắc 1 chiều qua `DamageUtility.CalculateHitDamage(attackerElement, defenderElement)` (nhân `×1.3` nếu khắc hệ).
    - Đánh dấu cờ `IsCounter` trong `DamageData` và `DamageReport` để phục vụ visual feedback.
  - [x] **[TASK-106.3] Tích Hợp Vòng Tương Sinh Theo Event-Driven Decoupled (`ElementCycleManager`):**
    - Gọi `ElementCycleManager.Instance.RegisterHit(attackerElement, sourceWeapon)` khi đòn đánh trúng kẻ địch.
    - Đảm bảo cơ chế cooldown refund 20% trừ trực tiếp trên `WeaponBase.ReduceCurrentCooldown()` mượt mà, zero-allocation và tôn trọng giới hạn 1 proc / 3s.
  - [x] **[TASK-106.4] Visual & Audio Feedback Tương Khắc / Tương Sinh (Game Feel):**
    - Kích hoạt màu Vàng Kim rực rỡ (`#FFD700`) + ký hiệu `✦` trên Floating Damage Text khi đòn đánh đạt điều kiện Tương Khắc trong `DamageTextManager` và `DamageTextStyleConfig`.
    - Phát âm thanh *Ting!* + hiệu ứng khi proc Tương Sinh theo đúng GDD v4.0.

### 🗡️ Hạng Mục 2: Reskin Content & Asset Database (Completed)
- [x] **[TASK-201]** Thêm trường `elementType` vào ScriptableObject `WeaponData.cs` và `EnemyConfig.cs`.
- [x] **[TASK-202]** Tạo `WeaponDataGenerator.cs` tự động sinh/cập nhật dữ liệu `.asset` cho 12 Pháp Bảo MVP và 12 Evolution theo GDD 4.0.
- [x] **[TASK-203]** Tạo `EnemyDataGenerator.cs` tự động sinh/cập nhật dữ liệu `.asset` cho 5 Yêu Ma MVP (Ma Giáp, Ma Trơi, Quỷ Nhập Tràng, Ma Da, Hồ Ly Tinh Nhỏ).
- [x] **[TASK-204]** Chuyển đổi tên gọi đơn vị tiền Meta thành **Cổ Tiền** (tiền xu cổ Việt Nam) trong `MetaCurrencyManager.cs` và `MetaProgressionSaveData.cs`.
- [x] **[TASK-205]** Tạo `WeaponEvolutionManager.cs` kiểm tra điều kiện ghép 12 Vũ Khí Tiến Hóa theo ma trận GDD v4.0.
- [x] **[TASK-206]** Tạo `ProjectilePrefabGenerator.cs` & `ProjectileBehaviorDataGenerator.cs` sinh 12 Prefabs đạn và gán Behaviors SOs.
- [x] **[TASK-207]** Tạo `WaveDataGenerator.cs` tự động sinh 15 Wave Config ScriptableObjects bám sát Timeline 15 Phút GDD v4.0.

### 🧟 Hạng Mục 3: AI Yêu Ma & Boss Mechanics (Completed)
- [x] **[TASK-306]** Tích hợp cơ chế **Cản Đạn (`Heavy Armor Bullet Sponge` - GDD 5.1)** cho Quỷ Nhập Tràng (`E_QUYNHAPTRANG`) trong `PierceBehavior.cs`.
- [x] **[TASK-307]** Viết `SuicideExplodeBehavior.cs` cho Hồ Ly Tinh Nhỏ (`E_HOALYTINH`) áp sát nổ AoE 50 Damage.
- [x] **[TASK-308]** Xây dựng `BossStateMachine.cs`, `BullDashSkill.cs` (Ngưu Xung Thiên), `GroundSlamSkill.cs` (Địa Chấn Âm Ty) cho Boss 1 Ngưu Đầu Mã Diện.
- [x] **[TASK-309]** Tích hợp **Anti-Cornering Guard (GDD 7.0)** cho `EnemySpawner.cs` dồn 70% quái hướng về trung tâm khi Player sát tường $<5\text{m}$.
- [x] **[TASK-310]** Tái cấu trúc Spawner System quy về mô hình Master-Worker chuẩn Data-Driven (`SpawnManager` & `EnemySpawner`).
- [x] **[TASK-311]** Tách biệt 100% AI Boss bằng `BossMovementStrategy.cs` và `BossMeleeAttackStrategy.cs` độc lập với quái thường.

### 📱 Hạng Mục 4: Mobile UI Canvas & MVP Systems
- [x] 🚨 **[TASK-300] [HIGH PRIORITY — ƯU TIÊN HÀNG ĐẦU]** Setup Mobile Controls Canvas trong Scene Unity: Dựng `DynamicVirtualJoystick` (OnScreenControl), `SignatureSkillButtonView` + `SignatureSkillPresenter` (Nút bấm Skill chủ động) & `DashButtonView` + `DashButtonPresenter` chuẩn New Input System & MVP.
- [x] 🚨 **[TASK-312] Refactor & Refine In-Game HUD Visuals (`RunHUDView.cs` & `RunHUDPresenter.cs`)**:
  - [x] **[TASK-312.1] Visual & Contrast cho Health Bar, EXP Bar & Level Text:**
    - Cấu hình màu sắc Đỏ Chu Sa (`#D13838`) cho `_hpSlider`, cảnh báo nháy đỏ khi HP $<25\%$.
    - Cấu hình Xanh Ngọc Bích (`#4DEEEA`) cho `_expSlider` và Hoàng Kim (`#FFD700`) cho `_levelText` với TMP Rich Text.
  - [x] **[TASK-312.2] Redesign Slider Cán Cân Âm Dương (`CharacterGaugeWidgetView.cs` & `TaoistYinYangTracker.cs`):**
    - Thêm Fill Color động theo 3 trạng thái: Âm Thịnh (Tím Ma Mị `#9B51E0`), Cân Bằng (Trắng Xanh/Vàng Kim `#E0F7FA`/`#FFD700`), Dương Thịnh (Cam Thái Dương `#FF8C00`).
    - Cập nhật chuỗi tiêu đề trạng thái có biểu tượng ☯ chuẩn Art Guide.
  - [x] **[TASK-312.3] Hiển Thị Thuộc Tính Ngũ Hành Trên Đầu Boss (Overhead Boss Element Badge):**
    - Thiết kế World-Space UI / Overhead Indicator gắn trực tiếp phía trên đầu Boss trong `BossElementController.cs` (thay vì đặt trên HUD/Panel Âm Dương gây rối mắt).
    - Hiển thị TextMeshPro / Badge với biểu tượng hình khối & màu sắc 5 hệ chuẩn Art Guide: Kim 🔷 (`#E8C468`), Mộc 🌿 (`#4C7A3D`), Thủy 💧 (`#29B6F6`), Hỏa 🔥 (`#FF5722`), Thổ 🟫 (`#D7A87A`).
  - [x] **[TASK-312.4] Tối Ưu Tinh Gọn HUD & Phân Định Rõ Ràng Trách Nhiệm UI:**
    - Tách biệt hoàn toàn Panel Âm Dương (`CharacterGaugeWidgetView.cs`) chỉ hiển thị cơ chế Cán Cân Nhân Vật.
    - Loại bỏ trường `_bossElementText` dư thừa khỏi `RunHUDView.cs` và `RunHUDPresenter.cs`, giữ Top HUD thanh thoát, tập trung vào HP, EXP, Timer & Kills.
- [x] 🚨 **[TASK-313] Refine Upgrade Cards Gacha UI (Stat Diff & Badges - MVP & Clean Architecture)**:
  - [x] **[TASK-313.1] Xây Dựng Bộ Định Dạng Chỉ Số Thẻ Nâng Cấp (`IUpgradeStatFormatter` & `UpgradeStatFormatter`):**
    - Thiết kế `IUpgradeStatFormatter` độc lập để format `WeaponStatModifier` và `PlayerStatModifier` thành Rich Text trực quan (ví dụ: `<color=#4DEEEA>+15% Sát Thương</color>`, `<color=#FFD700>+1 Số Tia</color>`).
    - Dễ bảo trì và mở rộng khi thêm các loại modifier hoặc thẻ kỹ năng mới trong tương lai.
  - [x] **[TASK-313.2] Chuẩn Hóa Bảng Màu & Badge Ngũ Hành (`ElementVisualHelper`):**
    - Tạo helper tập trung cung cấp format TMP Rich Text, mã màu phong thủy (Kim 🔷, Mộc 🌿, Thủy 💧, Hỏa 🔥, Thổ 🟫) dùng chung cho Card, HUD và Hệ thống.
  - [x] **[TASK-313.3] Nâng Cấp Passive View Thẻ Nâng Cấp (`UpgradeCardView.cs`):**
    - Bổ sung trường `_statDiffText` (hoặc `_statDiffContainer`) độc lập với `_descriptionText` để phân tách rõ Mô Tả Truyền Thuyết và Chỉ Số Thay Đổi.
    - Bổ sung các phương thức cập nhật thụ động `SetStatDiff(string)` và cập nhật lại signature `Setup(...)` kèm null-guard.
    - Đảm bảo `Animator.updateMode = AnimatorUpdateMode.UnscaledTime` khi hiển thị popup dừng game (`Time.timeScale = 0`).
  - [x] **[TASK-313.4] Tích Hợp Điều Phối Presenter (`UpgradeUIPresenter.cs`):**
    - Kết nối `UpgradeStatFormatter` và `ElementVisualHelper` vào luồng `PopulateUpgradeScreen()` để truyền dữ liệu đã format sang `UpgradeCardView`.
  - [x] **[TASK-313.5] Dynamic Card Container & Object Pooling (`UpgradeUIView.cs` & `UpgradeUIPresenter.cs`):**
    - Chuyển đổi cơ chế mảng cố định `_upgradeCards` sang `Transform _cardsContainer` + `UpgradeCardView _cardPrefab` kết hợp Object Pooling nội bộ.
    - `UpgradeUIView` cung cấp API `IReadOnlyList<UpgradeCardView> GetOrCreateCardViews(int requiredCount)` tự động tái sử dụng các instance đã có hoặc instantiate thêm khi cần (Zero GC rác, không Destroy).
    - `UpgradeUIPresenter` hỗ trợ số lượng thẻ cấu hình động `_defaultChoiceCount = 3` (dễ mở rộng khi có Meta Upgrade tăng lựa chọn hoặc Rương Báu 1-3-5 thẻ).
    - Tương thích ngược: Tự động nhận diện các Card View đã kéo sẵn trong Scene lúc `Awake()` vào Pool.
    - Tạo Editor Tool `UpgradeUIHierarchyOptimizer.cs` (`Tools/ProjectZombie/UI/Optimize UpgradeUI Hierarchy`) tự động chuẩn hóa cấu trúc `Upgrade_Panel`, `Cards_Container`, `Header_Title`, `Footer_Controls` (Button Reroll/Skip) và tự động gán reference trong Inspector.
- [ ] 🚨 **[TASK-314] Mobile Screen Resolution & Anchor Layout Adaptation**:
  - Cấu hình chuẩn `CanvasScaler`: `Scale With Screen Size` (1920x1080, `Match Width Or Height` = 0.5).
  - Tinh chỉnh Anchor Points của tất cả Element UI (HUD Top Left/Right, Controls Bottom Left/Right, Overlay Cards Center) tránh bị lệch hoặc tràn lề trên các tỉ lệ màn hình mobile (16:9, 19.5:9, 20:9).
- [x] 🚨 **[TASK-315] Setup Vietnamese TMP Dynamic Fallback Font Asset (Unicode Range & Fallback List)**:
  - Cấu hình `LiberationSans SDF - Fallback.asset` (Dynamic Mode) vào `m_fallbackFontAssets` trong `TMP Settings.asset`.
  - Tạo Editor Tool `TMPVietnameseFontSetupTool.cs` (`Tools/ProjectZombie/Font/Setup Vietnamese TMP Fallbacks`) tự động sửa lỗi thiếu glyph Unicode tiếng Việt (`0x1EA0-0x1EF9`) như `Ủ` (`\u1EE6`) và biểu tượng `☯` (`\u262F`) trên toàn bộ UI/HUD.
- [ ] 🚨 **[TASK-316] Hệ Thống UI Trực Quan Ngũ Hành Cho Người Chơi Mới (MVP & Clean Architecture)**:
  - [ ] **[TASK-316.1] Gợi Ý Tương Sinh (Synergy Glow & Hint) Trên Thẻ Gacha (`UpgradeCardView` & `UpgradeUIPresenter`):**
    - `UpgradeUIPresenter` kiểm tra vũ khí người chơi đang sở hữu, nếu thẻ nâng cấp xuất hiện thuộc tính Tương Sinh với vũ khí hiện có $\rightarrow$ format chuỗi gợi ý `💡 Tương Sinh: -20% Hồi chiêu!` và kích hoạt viền sáng xanh ngọc `SynergyGlow`.
    - Gọi `cardView.SetElementBadge()` nạp badge thuộc tính chuẩn màu sắc và hình khối Ngũ Hành.
  - [ ] **[TASK-316.2] HUD Quick Reference Wheel - Vòng Bát Quái Tra Cứu Nhanh (`ElementWheelWidgetView/Presenter`):**
    - Dựng Component `ElementWheelWidgetView` gắn trên Top HUD với nút icon Bát Quái.
    - Hỗ trợ cơ chế **Hold Touch (Chạm giữ 1 giây)**: Khi ngón tay đè vào icon $\rightarrow$ phóng to vòng đồ hình Ngũ Hành thể hiện rõ mũi tên Tương Khắc (Đỏ) và Tương Sinh (Xanh); thả tay $\rightarrow$ tự động ẩn, không pause game, không cản trở di chuyển.
  - [ ] **[TASK-316.3] Progressive Onboarding Banner Tutorial (Run 1 First-Time Feedback):**
    - Tạo `OnboardingBannerView` hiển thị toast banner mờ 3 giây ở mép trên màn hình khi lần đầu tiên người chơi kích hoạt đòn đánh Khắc hệ hoặc proc Tương sinh trong trận đầu tiên (lưu cờ vào `PlayerPrefs` / `SaveData` để chỉ hiện đúng 1 lần).
  - [ ] **[TASK-316.4] Tab Sơ Đồ Ngũ Hành Trong Pause Menu & Sách Yêu Ma (Codex):**
    - Tích hợp trang đồ họa Ngũ Hành Cổ Phong vào Pause Menu cho phép người chơi tra cứu chi tiết thông số và quy luật khi tạm dừng trận đấu.
- [x] **[TASK-301]** Cập nhật `RunHUDView.cs` và `RunHUDPresenter.cs` thêm Slider Cán cân Âm Dương & TMP Text hiển thị thuộc tính Boss.
- [x] **[TASK-302]** Thêm Badge màu hiển thị thuộc tính Ngũ Hành trên thẻ Gacha Nâng cấp (`UpgradeCardView.cs`).
- [x] **[TASK-303]** Xây dựng giao diện `MetaUpgradeShopView.cs` và `MetaUpgradeShopPresenter.cs` cho Cây nâng cấp vĩnh viễn dùng Cổ Tiền.
- [x] **[TASK-304]** Xây dựng UI Chọn Nhân vật theo 3 anh hùng: Thư Sinh (Kim), Đạo Sĩ (Mộc), Võ Tăng (Thổ) qua `CharacterSelectionView/Presenter`.
- [x] **[TASK-305]** Xây dựng màn hình `RunSummaryView.cs` & `RunSummaryPresenter.cs` hiển thị kết quả sau run đấu và tự động cộng Cổ Tiền tích lũy.

### ✨ Hạng Mục 7: Modular VFX & Visual Polish (High Priority)
- [ ] 🚨 **[TASK-VFX-01] [HIGH PRIORITY]** Tối ưu Vệt Chém Dynamic (`Bút Phán Quan` W002 & `Weapon_DualSlash`): Instantiate/Pool Crescent Slash VFX xoay theo góc chém thực tế + Thêm `Ink Splash` khi trúng quái.
- [x] 🚨 **[TASK-VFX-02] [HIGH PRIORITY]** Tối ưu Vệt Linh Khí Orbit (`Bùa Trấn Yêu` W003): Xoay tiếp tuyến góc lá bùa mềm mại + hiệu ứng đẩy lùi (Knockback Repulsion Pulse) khi chạm yêu ma.
- [ ] 🚨 **[TASK-VFX-03] [HIGH PRIORITY]** Tối ưu Sóng Xung Kích & Nứt Đất (`Trống Đồng` W005 & Skill Võ Tăng): Tạo URP Shader vòng sóng xung kích (Shockwave Ring) + Vết nứt đất `Ground Decal Fade` 0.5s.
- [ ] 🚨 **[TASK-VFX-04] [HIGH PRIORITY]** Tối ưu Hit Impact & Hit Flash Shader: Tạo Prefab `HitImpact` (Sparks + Blood Burst) spawn tại tọa độ va chạm (`hitPoint`) + Shader nháy trắng (Hit Flash) 0.05s trên Zombie khi nhận sát thương.

### 👾 Hạng Mục 5: Enemy Prefabs, Boss Data & 20-Min Wave Completion (Completed)
- [x] **[TASK-E01]** Tạo ScriptableObject Data cho Boss **Ngưu Đầu Mã Diện** (HP Base 5000, Speed 2.2, Hệ Thổ/Hỏa).
- [x] **[TASK-E02]** Tạo ScriptableObject Data cho Trùm Cuối **Diêm Vương** (HP Base 15000, Speed 1.8, Luân phiên 5 hệ).
- [x] **[TASK-E03]** Tạo 5 ScriptableObjects `Wave_Minute_16` đến `Wave_Minute_20` hoàn thiện Timeline 20 Phút.
- [x] **[TASK-E04]** Cập nhật `WavePhaseGenerator.cs` để Phase 3 kéo dài từ Phút 10 đến Phút 20 và nạp đủ 20 Waves.
- [x] **[TASK-E05]** Dựng Prefab `E_MAGIAP.prefab` (Ma Giáp - Kim).
- [x] **[TASK-E06]** Dựng Prefab `E_MATROI.prefab` (Ma Trơi - Hỏa).
- [x] **[TASK-E07]** Dựng Prefab `E_MADA.prefab` (Ma Da - Thủy).
- [x] **[TASK-E08]** Dựng Prefab `E_HOALYTINH.prefab` (Hồ Ly Tinh Nhỏ - Hỏa AoE Nổ).
- [x] **[TASK-E09]** Dựng Prefab `E_QUYNHAPTRANG.prefab` (Quỷ Nhập Tràng - Thổ Elite 1).
- [x] **[TASK-E10]** Dựng Prefab `Boss_NguuDauMaDien.prefab` (Ngưu Đầu Mã Diện - Boss 10:00).
- [x] **[TASK-E11]** Dựng Prefab `Boss_DiemVuong.prefab` (Diêm Vương - Final Boss 20:00).
- [x] **[TASK-E12]** Dựng Prefabs Rương Phần Thưởng (`Chest_UMinh.prefab` & `Chest_DauThai.prefab`).
- [x] **[TASK-E13]** Chạy Tool `WavePhaseGenerator` tự động nạp Prefabs chuẩn vào 3 Phase và 20 WaveConfig SOs.
- [x] **[TASK-E14]** Kiểm thử toàn bộ trận đấu 20 phút (00:00 -> 20:00).

### 🎮 Hạng Mục 8: Gameplay Production Readiness & Game Feel (High Priority)
*Tài liệu tham chiếu chi tiết:* 📖 **[GAMEPLAY_PRODUCTION_GUIDE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/GAMEPLAY_PRODUCTION_GUIDE.md)**
- [ ] 🚨 **[TASK-GP-01] Hit Flash URP Shader & Knockback Feedback**:
  - Tích hợp Shader URP nháy trắng (Flash White 0.05s) trên Sprite quái vật khi nhận sát thương trong `Damageable.cs` / `ZombieHealth.cs`.
  - Bổ sung lực đẩy lùi vật lý `_knockbackForce` cho các đòn đánh nặng (Trống Đồng W005, Cọc Gỗ) tạo không gian thở khi bị quái áp sát.
- [ ] 🚨 **[TASK-GP-02] Screen Shake & Critical Damage Feedback**:
  - Tích hợp Camera Shake (`0.08s`, biên độ `0.15`) khi kích hoạt đòn đánh Chí Mạng (Critical) hoặc Nổ AoE.
  - Nâng cấp Damage Popup: Phân biệt rõ rệt Sát thương Thường (Trắng), Tương Khắc (Vàng Kim `#FFD700` + Biểu tượng `✦`), Chí Mạng (Đỏ Cam Scale Punch).
- [ ] 🚨 **[TASK-GP-03] Treasure Chest Gacha Popup (1-3-5 Items) & Evolution Ceremony**:
  - Xây dựng Popup mở rương báu rơi từ Elite/Boss (Rương Gỗ: 1 món, Rương Bạc: 3 món, Rương U Minh: 5 món + Instant Evolution).
  - Tích hợp hiệu ứng sóng xung kích linh khí quét sạch quái cận chiến (Cleanse Blast) khi hoàn thành ghép Pháp Bảo Tối Thượng (Evolution).
- [ ] 🚨 **[TASK-GP-04] Circle Swarm Spawner Events (Tăng nhịp độ căng thẳng)**:
  - Bổ sung Event quái bao vây 360 độ siết chặt tại các mốc: Phút 03:00 (Ma Trơi bao vây), Phút 07:00 (Đàn Ma Da xoắn ốc), Phút 12:00 (Tường chắn Quỷ Nhập Tràng).
  - Buộc người chơi phải sử dụng cơ chế `Dash` (với i-frame 0.2s) để đục thủng vòng vây.
- [ ] 🚨 **[TASK-GP-05] Polish Audio Feedback Layers (Âm thanh đa tầng)**:
  - Tích hợp âm thanh phản hồi va chạm (Hit SFX), âm thanh quái tan biến (Death Pop) và chuỗi chuông ngân tăng dần khi hút hạt EXP (Gem Vacuum Chime).
- [x] 🚨 **[TASK-EXP-01] Tối Ưu & Nâng Cấp Toàn Diện Hệ Thống Hạt EXP & Magnet (Performance & Game Feel)**:
  - [x] **[TASK-EXP-01.1] Kích Hoạt & Đồng Bộ Bán Kính Hút (`PickupRange` Dynamic Magnet):**
    - Thiết lập trigger collider (`CircleCollider2D` IsTrigger) trên Player qua `PlayerMagnetTrigger.cs` chuyên trách bắt vùng Magnet, tự động cập nhật bán kính theo chỉ số `PlayerStats.PickupRange` khi người chơi nhận buff (như `P010_TúiHútHồn` +30% Bán kính nhặt).
    - Đồng bộ sự kiện kích hoạt trạng thái hút (`StartMagnetEffect`) mượt mà, không phụ thuộc vào collider cố định nhỏ của ExpGem.
  - [x] **[TASK-EXP-01.2] Tối Ưu Hiệu Năng Homing & Triệt Tiêu GC Alloc (Math Interpolation thay thế DOTween per Gem):**
    - Chuyển đổi cơ chế bay gia tốc và hiệu ứng pop-up từ việc khởi tạo nhiều Tween độc lập (`DOScale`, `DOMove`, `DOTween.To`) sang tính toán nội suy toán học (`Mathf.Lerp` / `Vector2.MoveTowards` với gia tốc tốc độ cục bộ).
    - Đảm bảo khi hàng trăm hạt Gem bay cùng lúc, hệ thống đạt Zero-Allocation (0 byte GC rác) và không làm nghẽn hàng đợi DOTween.
  - [x] **[TASK-EXP-01.3] Cơ Chế Nén & Gộp Hạt Kinh Nghiệm Tự Động (Gem Compression / Merging):**
    - Giới hạn ngưỡng số lượng hạt Gem active đồng thời trên sân (Threshold: 150 hạt).
    - Khi vượt ngưỡng, `ExpGemPoolManager` tự động tìm và gộp các hạt nhỏ xa Player nhất thành hạt cấp cao hơn (Tier 2/3/4) với giá trị bằng tổng exp các hạt được gộp, giúp giữ vững 60 FPS trong các đợt quái đông.
  - [x] **[TASK-EXP-01.4] Phân Cấp Hình Ảnh & Hiệu Ứng Âm Thanh Nhặt Liên Hoàn (Visual Tiering & Combo Pitch):**
    - Phân cấp màu sắc / Sprite hiển thị của ExpGem theo giá trị exp:
      - *Thường (10 - 25 EXP)*: Lam Ngọc Bích (`#4DEEEA`).
      - *Trung (50 - 100 EXP)*: Lục Bảo Ma Mị (`#50E3C2`).
      - *Boss / Tinh Anh (200+ EXP)*: Hoàng Kim Lấp Lánh (`#FFD700`) kèm hào quang nhẹ.
    - Tích hợp Pitch Scaling trong `AudioManager` khi người chơi nhặt liên tục chuỗi Gem (`Pitch: 1.0f -> 1.5f`, step `+0.05f`), tự reset về `1.0f` sau `0.4s` nghỉ, tạo cảm giác âm thanh thăng hoa chuẩn Roguelite.

### ⚙️ Hạng Mục 6: Performance & Build Release (Final Polish & Release)
- [ ] **[TASK-401]** Gom toàn bộ UI/Sprites vào Sprite Atlas 2048x2048 và chuyển đổi Texture Compression sang **ASTC 6x6** cho Android.
- [ ] **[TASK-402]** Thử nghiệm Stress Test 200 Yêu ma + 100 Projectiles kiểm tra FPS (Target 60 FPS).
- [ ] **[TASK-403]** Cấu hình Build Profile Android IL2CPP ARM64, Target API 33+, xuất file `.aab` có ký số phát hành Google Play Store.
