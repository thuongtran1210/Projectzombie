# Danh Sách Nhiệm Vụ & Task Tracker — VONG XUYÊN

Tài liệu quản lý danh sách công việc (Kanban Board Task Tracker) phân chia theo các hạng mục của dự án **Vong Xuyên (Android Release - GDD v4.0)**.
*Tài liệu tham chiếu chi tiết:* 🎨 **[UI_ART_DIRECTION_GUIDE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/UI_ART_DIRECTION_GUIDE.md)** | 📖 **[GAMEPLAY_PRODUCTION_GUIDE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/GAMEPLAY_PRODUCTION_GUIDE.md)** | 📐 **[SYSTEM_ARCHITECTURE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/SYSTEM_ARCHITECTURE.md)**

---

## 📋 Kanban Board Summary

| 🔴 To Do | 🟡 In Progress | 🟢 Done |
|---|---|---|
| 🚨 **[TASK-GP-01] Hit Flash Shader & Knockback Physics** | 🚨 **[TASK-300] Setup Mobile Controls Canvas (Joystick & Skill/Dash Buttons)** | Master-Worker Spawner Refactoring |
| 🚨 **[TASK-GP-02] Screen Shake & Critical Damage Feedback** | 🚨 **[TASK-VFX-01] Dynamic Slash VFX Placement (`Bút Phán Quan` W002)** | Android Target Platform Refactor |
| 🚨 **[TASK-GP-03] Treasure Chest Gacha Popup & Evolution Ceremony** | 🚨 **[TASK-VFX-02] Trail & Orbit Polish (`Bùa Trấn Yêu` W003)** | Local Save System (`SaveSystem.cs`) |
| 🚨 **[TASK-GP-04] Circle Swarm Spawner Events (Min 3, 7, 12)** | 🚨 **[TASK-VFX-03] Shockwave & Ground Decals (`Trống Đồng` W005 & Võ Tăng)** | Ngũ Hành Damage & Combo System |
| 🚨 **[TASK-GP-05] Polish Audio Feedback Layers (Hit/Gem/LevelUp)** | 🚨 **[TASK-VFX-04] Hit Impact & Hit Flash Shader (`Hit Impact` & Zombie)** | Cán Cân Âm Dương (`YinYangManager`) |
| Mobile Stress Test (200 Mobs 60 FPS) | 🚨 **[TASK-312] Refactor & Refine In-Game HUD Visuals** | Boss AI Dynamic Element (`BossElementController`) |
| Texture Compression ASTC & Sprite Atlas (Tối ưu cuối) | 🚨 **[TASK-313] Refine Upgrade Cards Gacha UI (Stat Diff & Badges)** | 12 Pháp Bảo & 5 Yêu Ma Data SOs |
| Android AAB Build & Signing (Google Play Release) | 🚨 **[TASK-314] Mobile Resolution & Anchor Layout Adaptation** | Enemy Prefabs & Boss Data ([TASK-E01] -> [TASK-E14]) |
| | | Upgrade Cards Badges & Meta Shop UI |

---

## 🏃 Sprint Tasks Breakdown

### ☯️ Hạng Mục 1: Lõi Cơ Chế Ngũ Hành & Âm Dương (Completed)
- [x] **[TASK-101]** Refactor `DamageUtility.cs` và `DamageData.cs` hỗ trợ tra cứu Tương Khắc Ngũ Hành (+30% Sát thương).
- [x] **[TASK-102]** Tích hợp Element Combo Tracker vào `WeaponManager.cs` (-20% Cooldown khi kích hoạt Tương Sinh trong 3s).
- [x] **[TASK-103]** Xây dựng `YinYangManager.cs` theo dõi trạng thái Âm Dương (0-100) và phát sự kiện `OnYinYangStateChanged`.
- [x] **[TASK-104]** Tích hợp lọc pool thẻ Gacha nâng cấp ngẫu nhiên trong `UpgradeManager.cs` theo `YinYangState`.
- [x] **[TASK-105]** Tạo `BossElementController.cs` cho phép Boss (Ngưu Đầu Mã Diện & Diêm Vương) luân phiên xoay vòng thuộc tính Ngũ Hành theo chu kỳ 10s.

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
- [ ] 🚨 **[TASK-300] [HIGH PRIORITY — ƯU TIÊN HÀNG ĐẦU]** Setup Mobile Controls Canvas trong Scene Unity: Dựng `DynamicVirtualJoystick` (Cần gạt di chuyển), `SignatureSkillButtonView` + `SignatureSkillPresenter` (Nút bấm Skill chủ động) & `DashButtonView` kết nối trực tiếp với PlayerController.
- [ ] 🚨 **[TASK-312] Refactor & Refine In-Game HUD Visuals (`RunHUDView.cs` & `RunHUDPresenter.cs`)**:
  - Nâng cấp tương phản & màu sắc cho Thanh Máu (`_hpSlider`), EXP (`_expSlider`) và Text Cấp độ `Lv.X`.
  - Thiết kế lại Slider Cán cân Âm Dương (phân định rõ màu sắc Âm / Cân Bằng / Dương & hiệu ứng trực quan).
  - Tối ưu hiển thị Thuộc tính Boss (`_bossElementText`) dùng Rich Text màu sắc chuẩn 5 hệ (Kim - Vàng, Mộc - Xanh Lá, Thủy - Xanh Dương, Hỏa - Đỏ, Thổ - Nâu/Vàng Kim).
- [ ] 🚨 **[TASK-313] Refine Upgrade Cards Gacha UI (`UpgradeUIView.cs` & `UpgradeCardView.cs`)**:
  - Tối ưu bố cục Thẻ Nâng Cấp (Card Layout): Tên pháp bảo/kỹ năng, cấp độ mới, mô tả hiệu ứng, badge màu thuộc tính Ngũ Hành.
  - Hiển thị rõ ràng sự thay đổi chỉ số (Stat Diff) trực quan dễ đọc.
  - Đảm bảo `Animator` đặt `updateMode = AnimatorUpdateMode.UnscaledTime` để hoạt ảnh xuất hiện mượt mà khi dừng game (`Time.timeScale = 0`).
- [ ] 🚨 **[TASK-314] Mobile Screen Resolution & Anchor Layout Adaptation**:
  - Cấu hình chuẩn `CanvasScaler`: `Scale With Screen Size` (1920x1080, `Match Width Or Height` = 0.5).
  - Tinh chỉnh Anchor Points của tất cả Element UI (HUD Top Left/Right, Controls Bottom Left/Right, Overlay Cards Center) tránh bị lệch hoặc tràn lề trên các tỉ lệ màn hình mobile (16:9, 19.5:9, 20:9).
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

### ⚙️ Hạng Mục 6: Performance & Build Release (Final Polish & Release)
- [ ] **[TASK-401]** Gom toàn bộ UI/Sprites vào Sprite Atlas 2048x2048 và chuyển đổi Texture Compression sang **ASTC 6x6** cho Android.
- [ ] **[TASK-402]** Thử nghiệm Stress Test 200 Yêu ma + 100 Projectiles kiểm tra FPS (Target 60 FPS).
- [ ] **[TASK-403]** Cấu hình Build Profile Android IL2CPP ARM64, Target API 33+, xuất file `.aab` có ký số phát hành Google Play Store.
