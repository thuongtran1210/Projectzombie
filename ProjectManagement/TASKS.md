# Danh Sách Nhiệm Vụ & Task Tracker — VONG XUYÊN

Tài liệu quản lý danh sách công việc (Kanban Board Task Tracker) phân chia theo các hạng mục của dự án **Vong Xuyên (Android Release - GDD v5.0 Action RPG Roguelite)**.  
*Tài liệu tham chiếu chi tiết:* 🎮 **[ProjectZombie_GDD.md](file:///c:/Users/thuon/Unity/Projectzombie/GameDesignDoc/ProjectZombie_GDD.md)** | 🎨 **[UI_ART_DIRECTION_GUIDE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/UI_ART_DIRECTION_GUIDE.md)** | 📐 **[SYSTEM_ARCHITECTURE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/SYSTEM_ARCHITECTURE.md)**

---

## 📋 Kanban Board Summary

| 🔴 Backlog (Sắp Tới / Xử Lý Sau) | 🟡 In Progress (Đang Triển Khai - ARPG & Slapstick) | 🟢 Done (Đã Hoàn Thành) |
|---|---|---|
| 📱 **[TASK-314] Mobile UI Responsive & Safe Area** | 👊 **[TASK-FEEL-01] Hệ Thống Đánh Đã Tay (Lunge & Crunchy Hit)** | ⚔️ **[TASK-ARPG-01] Tái Cấu Trúc Lõi Vũ Khí & 1 Relic** |
| 🪓 **[TASK-316] Hệ Thống UI Trực Quan Ngũ Hành (Bát Quái / Glow)** | ✨ **[TASK-VFX-05] Bộ VFX Đòn Đánh Riêng Biệt 4 Nhân Vật** | ☯️ **[Hạng Mục 1] Lõi Ngũ Hành, Âm Dương & 0-Alloc Combat** |
| 💥 **[TASK-GP-03] Treasure Chest Gacha Popup (1-3-5 Items)** | 🏃 **[TASK-ARPG-02] Trạng Thái Nhân Vật & Combat Controls** | 📦 **[Hạng Mục 2] Database 12 Pháp Bảo, 12 Tiến Hóa & Cổ Tiền** |
| 🗜️ **[TASK-401] Texture Compression ASTC & Sprite Atlas** | 🎴 **[TASK-ARPG-03] Hệ Thống Nâng Cấp Trong Trận & Đột Phá** | 🧟 **[Hạng Mục 3] AI 5 Yêu Ma, 2 Boss & 20-Min Wave Timeline** |
| 🧪 **[TASK-402] Mobile Stress Test (60 FPS ARPG)** | 🤪 **[TASK-FUN-01 -> 06] Combat & Enemy Slapstick/Bựa/Fun** | 📱 **[TASK-300] Mobile Controls (Joystick, Dash, Skill Buttons)** |
| 🚀 **[TASK-403] Android AAB Build & Signing Release** | 🎨 **[TASK-FUN-07] Art Sprites & Icons (Dép, Nồi, Điếu, Chiếu, Chổi)** | 📊 **[TASK-312 & 313] In-Game HUD Visuals & Upgrade Cards UI** |
| | ✨ **[TASK-FUN-08] VFX Particle Prefabs (Lốc Dép, Khói, Đại Bác)** | 💎 **[TASK-EXP-01] Hệ Thống Hạt EXP Zero-Alloc & Magnet** |
| | 🏛️ **[TASK-ARPG-05] UI Sảnh Chờ Chọn Loadout Vũ Khí & Relic** | 🎨 **[TASK-VFX-01 -> 04] Bộ VFX Vệt Chém, Orbit & Shockwave** |
| | | 📦 **[TASK-FUN-06] Database 5 Slapstick SOs & 25 Upgrade Cards** |

---

## 🏃 Pipeline Thực Thi Chi Tiết (Execution Pipeline)

```mermaid
graph TD
    A["✅ Phase 1: Core Systems, Data & Waves (DONE)"] --> B["✅ Phase 2: Mobile UI MVP & Controls (DONE)"]
    B --> C["✅ Phase 3: Shaders & VFX Foundation (DONE)"]
    C --> D["🟡 Phase 4: Action RPG Hack & Slash Transition (CURRENT SPRINT)"]
    D --> D2["🤪 Phase 4.5: Combat & Enemy Slapstick / Fun (DOC_FUN_COMBAT)"]
    D2 --> E["🔴 Phase 5: UI Polish, Performance & Store Release (FINAL)"]
```

---

### 🟡 Phase 4: Chuyển Đổi Sang Action RPG Roguelite (CURRENT SPRINT — GDD v5.0)

Mục tiêu: Chuyển đổi cơ chế chiến đấu từ Tự Động Đánh (Survivor) sang **Chặt Chém Chủ Động (Hack & Slash Action RPG)** lấy **Vũ Khí Chính (3-Hit Combo)** và **Kỹ Năng Nhân Vật** làm gốc, biến đổi các vũ khí phụ thành **Pháp Bảo Hộ Thân (Relics)**.

---

#### ⚔️ [TASK-ARPG-01] Hệ Thống Đòn Đánh Nhân Vật & 1 Pháp Bảo Hộ Thân (Character Combat & Single Relic)
*Mục tiêu:* Đòn đánh thường gắn liền với bản thể Tướng (Melee Slash / Ranged Projectile + Combo 1-2-3), trang bị mang vào rút gọn thành 1 Pháp bảo hộ thân tự động.
* **Dependencies:** `CharacterCombat.cs`, `CharacterSelectionData.cs`, `WeaponManager.cs`, `AttackButtonPresenter.cs`.

- [x] **[TASK-ARPG-01.1] Xây Dựng `CharacterCombat.cs` (Đòn Đánh Bản Thể Nhân Vật):**
  - Tích hợp `CharacterAttackConfig` trực tiếp trong `CharacterSelectionData.cs` (Loại đòn MeleeSlash/RangedProjectile, VFX Slash/Đạn, Sát thương, Tốc đánh, Combo 1-2-3).
  - Quản lý `CurrentComboStep` ($1 \rightarrow 2 \rightarrow 3$), `MaxComboSteps = 3`, `comboResetWindow = 1.0s`, `Tap Buffer Window = 0.18s`.
  - Tự động điều phối Animation Attack, lực đầm Slowdown, Smart Soft-Lock Auto-Aim, Game Feel (`CameraShake`, `HitStop`) và phát sự kiện `OnHitEnemy`.
- [x] **[TASK-ARPG-01.2] Tối Ưu Hóa `WeaponManager.cs` Về Chế Độ 1 Pháp Bảo Duy Nhất:**
  - Thiết lập `MAX_WEAPONS = 1`: Chỉ mang 1 Pháp Bảo Hộ Thân vào trận.
  - Chế độ vận hành 100% tự động: Mọi Pháp bảo khi trang bị được đặt `isPrimaryActiveWeapon = false` để tự động kích hoạt/xoay quanh qua `Tick()`.
- [x] **[TASK-ARPG-01.3] Đồng Bộ Nút Tấn Công (`AttackButtonPresenter.cs`):**
  - Kết nối trực tiếp vào `CharacterCombat` của Player.
  - Tự động đổi Icon và hiển thị thanh Cooldown theo đòn đánh cơ bản của nhân vật.
- [x] **[TASK-ARPG-01.4] Tái Cấu Trúc Giao Diện Tàng Bảo Các (`WeaponLoadoutUI`):**
  - Cột trái: Tab duy nhất `[ KHO PHÁP BẢO HỘ THÂN (CHỌN 1) ]`.
  - Cột phải: 2 Ô Xuất Trận gồm **[Đòn Đánh Tướng]** (Cố định) và **[1 Pháp Bảo Hộ Thân]** (Tùy chọn).

---

#### 👊 [TASK-FEEL-01] Hệ Thống Đánh Đã Tay & Game Feel Đậm Lực (Crunchy Combat Feel & Motion Lunge)
*Mục tiêu:* Nâng cấp cảm giác chém quái cực kỳ đã tay qua cơ chế dấn người (Lunge Impulse), khựng hình bậc thang (HitStop) và phản hồi va chạm (Hit-Reaction).
* **Dependencies:** `CharacterCombat.cs`, `PlayerController.cs`, `GameJuiceEvents.cs`.

- [x] **[TASK-FEEL-01.1] Lực Dấn Người Tới Trước (Attack Motion Lunge):**
  - Khi tung đòn đánh thường, nhân vật tự động lướt nhẹ tới trước theo nhát chém:
    - *Nhát 1:* Dấn nhẹ `1.8 m/s`.
    - *Nhát 2:* Dấn vừa `2.5 m/s`.
    - *Nhát 3 (Finisher):* Nhảy vút tới trước `4.0 m/s` càn quét quái.
  - Tự động bám dính mục tiêu (Sticky Melee Tracking) tránh chém hụt.
- [x] **[TASK-FEEL-01.2] Dynamic HitStop & Camera Shake Theo Bậc Thang Combo:**
  - *Nhát 1:* HitStop $0.025s$, Shake nhẹ $0.05$.
  - *Nhát 2:* HitStop $0.045s$, Shake vừa $0.09$.
  - *Nhát 3 (Finisher):* HitStop **$0.08s$** tạo lực đầm ngàn cân, Shake mạnh **$0.18$** + Knockback $\times 1.6$.
- [x] **[TASK-FEEL-01.3] Phản Hồi Va Chạm (Hit Impact Reaction):**
  - Tự động spawn tia lửa va chạm (`PS_ImpactSparks`) ngay tại điểm tiếp xúc giữa kiếm và quái.
  - Quái vật chớp trắng (`HitFlash`) rõ nét khi nhận đòn.

---

#### ✨ [TASK-VFX-05] Bộ Hiệu Ứng VFX Đòn Đánh Riêng Biệt Cho 4 Nhân Vật
*Mục tiêu:* Xây dựng 4 bộ VFX Effect đặc trưng 3 lớp (Vệt chém/Đạn + Lõi phát sáng Core + Hạt bụi năng lượng) cho 4 vị tướng.
* **Dependencies:** `CharacterSelectionData.asset`, `Assets/VFX/SkillLibrary/Prefabs/`, `URP_VFX_Slash_Additive.shader`.

- [x] **[TASK-VFX-05.1] Thư Sinh (C001 - Hệ Kim):** `VFX_ThuSinh_InkSlash`
  - Vệt chém mực đen Thư Pháp viền vàng Kim Additive, bốc khói bụi mực chữ triện cổ. Tích hợp Prefab `VFX_ThuSinh_InkSlash.prefab` và gán tự động vào `CharacterSelectionData.asset`.
- [x] **[TASK-VFX-05.2] Đạo Sĩ (C002 - Hệ Mộc / Âm Dương):** `VFX_DaoSi_TalismanShot`
  - Đạn Linh Phù Tiên Đạo bay thẳng viền tia chớp xanh ngọc lục bảo. Tích hợp Prefab `Projectile_DaoSi_TalismanShot.prefab` (Vòng Bát Quái Xoay + Đuôi Sét Xanh Ngọc) và gán tự động vào `CharacterSelectionData.asset`.
- [x] **[TASK-VFX-05.3] Thanh Đồng (C003 - Hệ Thủy / Tứ Phủ):** `VFX_ThanhDong_FlameSlash`
  - Vệt quét Đuốc Lửa Tứ Phủ quét góc rộng $140^\circ$, bốc tàn lửa than hồng rực rỡ mang phong cách lễ nghi Hầu Đồng. Tích hợp Prefab `VFX_ThanhDong_FlameSlash.prefab` và gán tự động vào `CharacterSelectionData.asset`.
- [x] **[TASK-VFX-05.4] Ẩn Sĩ Sơn Lâm (C004 - Hệ Thổ):** `VFX_AnSi_EarthImpactSlash`
  - Vệt chém quyền cước nâu hổ phách chấn địa, bốc bụi đất sỏi chấn động nứt mặt đất. Tích hợp Prefab `VFX_AnSi_EarthImpactSlash.prefab` và gán tự động vào `CharacterSelectionData.asset`.
- [x] **[TASK-VFX-05.5] Liên Kết Tự Động Vào CharacterSelectionData:**
  - Cung cấp MenuItem `Tools/VFX Generator/⚡ Build All 4 Character Basic Attack VFX (1-Click)` tự động liên kết toàn bộ 4 vị tướng.
- [ ] **[TASK-ARPG-01.4] Chuyển Đổi Các Pháp Bảo Phụ Sang Cơ Chế Relic:**
  - *Bùa Trấn Yêu (`Weapon_Orbit.cs`):* Giữ nguyên vòng xoay bảo vệ nhưng chuyển trọng tâm sang Đẩy lùi (Knockback) bảo vệ sau lưng.
  - *Cửu Vĩ Hồ Trảo:* Lắng nghe `PrimaryWeapon.OnHitEnemy` $\rightarrow$ Đính kèm đòn cào lửa và hút máu On-Hit.
  - *Lựu Đạn Thần Sa:* Lắng nghe đòn Combo thứ 3 của Primary Weapon $\rightarrow$ Phóng lựu đạn phát nổ.

---

#### 🏃 [TASK-ARPG-02] Trạng Thái Nhân Vật, Kỹ Năng Lướt & Combat Controls
*Mục tiêu:* Đem lại cảm giác điều khiển mượt mà, đầm tay, hỗ trợ né đòn phản xạ và tự động ngắm mục tiêu.
* **Dependencies:** `PlayerController.cs`, `AttackButtonPresenter.cs`, `MobileControlsSetupTool.cs`.

- [x] **[TASK-ARPG-02.1] Trạng Thái Vung Kiếm (Action State & Movement Slowdown):**
  - Trong `PlayerController.cs`: Khi đang vung đòn chém ($0.1s$ windup), giảm tốc độ chạy còn $40\%$ để tạo lực đầm cho nhát chém, khôi phục tốc độ ngay sau khi kết thúc đòn.
- [x] **[TASK-ARPG-02.2] Cơ Chế Dash Cancel (Hủy Hoạt Ảnh Bằng Nút Lướt):**
  - Khi người chơi bấm nút **Dash** trong lúc đang chém $\rightarrow$ Lập tức hủy trạng thái vung đòn, kích hoạt Lướt né đòn khẩn cấp với $0.15s$ I-frame bất tử.
- [x] **[TASK-ARPG-02.3] Tự Động Ngắm Thông Minh (Smart Soft-Lock):**
  - Khi bấm nút Attack: Nếu người chơi thả Joystick $\rightarrow$ `PlayerController` tự động xoay mặt về phía kẻ địch gần nhất trong hình nón $90^\circ$ và bán kính $5m$ phía trước.
- [ ] **[TASK-ARPG-02.4] Nâng Cấp `PlayerAnimator.cs`:**
  - Bổ sung các Trigger gọi Animation: `Attack_1`, `Attack_2`, `Attack_3`, `Dash` đồng bộ theo nhịp bấm.
- [x] **[TASK-ARPG-02.5] Nâng Cấp `AttackButtonPresenter.cs` & `AttackButtonView.cs`:**
  - Bổ sung **Tap Buffer**: Nhận diện nhịp bấm liên tục của người chơi mà không bị nuốt lệnh khi vũ khí vừa kết thúc nhát chém trước.

---

#### 🎴 [TASK-ARPG-03] Hệ Thống Nâng Cấp Trong Trận (In-Run Upgrades & Breakthrough)
*Mục tiêu:* Cải biến giao diện Lên Cấp, tập trung biến hóa chuỗi đòn chém và thức tỉnh Pháp bảo đã chọn.
* **Dependencies:** `UpgradeData.cs`, `UpgradeManager.cs`, `UpgradeUIPresenter.cs`.

- [x] **[TASK-ARPG-03.1] Mở Rộng Schema Dữ Liệu `UpgradeData.cs`:**
  - Thêm `UpgradeCategory`: `ComboAugment` (Biến hóa đòn chém), `RelicAwakening` (Thức tỉnh Pháp bảo), `DashTrait` (Cường hóa lướt), `ConditionalPassive` (Nội tại tình huống), `BreakthroughUltimate` (Bí tịch tuyệt kỹ).
- [x] **[TASK-ARPG-03.2] Nâng Cấp Bộ Lọc Gacha `UpgradeManager.cs`:**
  - Chỉ cho phép xuất hiện thẻ thuộc về: (1) Vũ Khí Chính đang cầm, (2) Các Pháp bảo đã trang bị trong Loadout, (3) Thẻ Lướt & Chỉ số.
  - Triệt tiêu hoàn toàn việc roll ra các vũ khí lạ chưa được trang bị ngoài sảnh.
- [x] **[TASK-ARPG-03.3] Cơ Chế Đột Phá Tuyệt Kỹ (Breakthrough System):**
  - Tại các mốc Level 5 và Level 10 (hoặc sau khi hạ Boss 1): Ép hiển thị 3 thẻ **Bí Tịch Tuyệt Kỹ** làm thay đổi hoàn toàn hình thái chiến đấu (VD: *Bát Quái Kiếm Trận, Hóa Thần Nhập Ma, Thái Cực Hộ Mệnh*).
- [x] **[TASK-ARPG-03.4] Tạo Bộ Thẻ ScriptableObjects Mẫu Cho 4 Nhóm:**
  - Tạo Editor Tool `ActionRPGUpgradeGenerator.cs`: Sinh tự động các thẻ Kiếm Khí Trảm, Trảm Phong Liên Hoàn, Tàn Ảnh Kiếm, Lướt Phản Đòn, Bát Quái Kiếm Trận, Hóa Thần Nhập Ma.

---

#### 🧟 [TASK-ARPG-04] Cân Bằng Enemy Waves (30-50 Mob) & Chỉ Báo Báo Đòn (Telegraphing)
*Mục tiêu:* Giảm mật độ quái để có không gian lướt và chém combo, tăng chất lượng thử thách của quái vật.
* **Dependencies:** `WavePhase.cs`, `SpawnManager.cs`, `Enemy.cs`.

- [x] **[TASK-ARPG-04.1] Điều Chỉnh Mật Độ Spawn:**
  - Trong `WavePhase.cs` & `SpawnManager.cs`: Giảm `maxEnemyCap` đồng thời từ 150–200 xuống **30 – 50 quái/wave**.
- [x] **[TASK-ARPG-04.2] Tăng HP & Độ Bền Của Quái Vật:**
  - Tăng HP cơ bản của toàn bộ Yêu ma lên $2.5\times$ trong `EnemyDataGenerator.cs` (Ma Giáp 100 HP, Quỷ Nhập Tràng 350 HP) và chuẩn hóa `WaveDataGenerator.cs`.
- [x] **[TASK-ARPG-04.3] Hệ Thống Chỉ Báo Báo Đòn (Telegraph Warning System):**
  - Tạo component `EnemyAttackTelegraph.cs` và tích hợp vào `EnemyAttackState.cs`: Hiển thị vệt đỏ cảnh báo $0.4s$ trước khi Quái Tinh Anh và Boss ra đòn giúp người chơi kịp bấm Dash né đòn.

---

#### 🏛️ [TASK-ARPG-05] UI Sảnh Chờ Chọn Loadout (Meta Hub Integration)
*Mục tiêu:* Cho phép người chơi chuẩn bị Nhân vật + Vũ khí chính + Pháp bảo trước khi bắt đầu trận đấu.
* **Dependencies:** `CharacterSelectionPresenter.cs`, `MainHubPresenter.cs`, `MetaUIManager.cs`.

- [x] **[TASK-ARPG-05.1] Mở Rộng Dữ Liệu Loadout Trận Đấu (`RunLoadoutState.cs`):**
  - Lưu thông tin: `SelectedCharacter`, `SelectedPrimaryWeapon`, `List<WeaponData> SelectedRelics (tối đa 3)`.
- [x] **[TASK-ARPG-05.2] Nâng Cấp UI Sảnh Chờ `CharacterSelectionPresenter.cs`:**
  - Tự động cấu hình Loadout gồm Vũ Khí Chính và 2-3 Pháp Bảo Hộ Thân khi chọn nhân vật.
- [x] **[TASK-ARPG-05.3] Khởi Tạo Gameplay Từ Loadout:**
  - Khi Scene Gameplay load: `WeaponManager.cs` tự động đọc `RunLoadoutState` để spawn chính xác Vũ Khí Chính và các Pháp bảo đã chọn.

---

#### ✨ [TASK-VFX-05] Tích Hợp Game Feel & Hit Impact Polish Cho Đòn Chém
- [ ] **Hit-Stop Toàn Diện:** Tích hợp `GameJuiceEvents.RequestHitStop(0.04f)` cho mọi đòn chém trúng quái (Chí mạng dừng `0.08f`).
- [ ] **Camera Shake Theo Nhịp Combo:** Nhát chém 1-2 rung nhẹ ($0.03$), nhát chém thứ 3 rung mạnh ($0.1s$, biên độ $0.15$).
- [ ] **VFX Vệt Chém Theo Chiều Vung Kiếm:** Tích hợp vệt chém uốn lượn uốn cong theo hướng chém của nhân vật.

---

### 🤪 Phase 4.5: Hệ Thống Combat & Kẻ Địch Slapstick / Bựa / Fun (DOC_FUN_COMBAT_V1.0)

Mục tiêu: Đưa các cơ chế chiến đấu và quái vật dân gian hài hước, bựa, độc đáo vào game mà vẫn duy trì kiến trúc chuẩn FSM, ScriptableObject và tối ưu hiệu năng 60 FPS Zero-Alloc trên Mobile.

---

#### 🤪 [TASK-FUN-01] Core Slapstick Status System & Visual Feedback
* **Dependencies:** `EnemyStatusController.cs`, `EnemyStatusVisuals.cs`, `Enemy.cs`.

- [x] **[TASK-FUN-01.1] Mở rộng Enum & Logic Trạng Thái trong `EnemyStatusController.cs`:**
  - Bổ sung vào enum `StatusEffectType`: `Humiliated` (Quê độ), `Sleeping` (Ngủ ngáy), `Stoned` (Say thuốc lào), `Dancing` (Mê nhảy múa), `RagdollFlight` (Văng parabol).
  - Logic `Humiliated`: Tắt tấn công người chơi; cho quái $40\%$ tỷ lệ quay sang đấm quái đồng minh gần nhất.
  - Logic `Sleeping`: Bất động hoàn toàn; đòn đánh đầu tiên trúng quái ngủ tự động nhận $\times 2.0$ sát thương (Wake-up Crit).
  - Logic `Stoned`: Đảo ngược input di chuyển; sau $2s$ nổ đám khói sặc sụa lan sát thương xung quanh.
  - Logic `Dancing`: Quái giơ tay đứng nhảy nhót, đóng vai trò làm vật cản / bia đỡ đạn đạo cho Player.
- [x] **[TASK-FUN-01.2] Kinematic Ragdoll Flight & Wall Impact:**
  - Thêm hàm `ApplyRagdollLaunch(Vector2 direction, float speed, float airborneDuration)`.
  - Quái bay hình parabol xoay tròn Sprite $Z$-axis, nổ sát thương chuỗi khi va đập vào tường hoặc quái khác.
- [x] **[TASK-FUN-01.3] Visual Cues & Audio Hooks (`EnemyStatusVisuals.cs`):**
  - Tích hợp Sprite Icons: Giọt mồ hôi xấu hổ (*Quê*), bong bóng mũi phập phồng (*Zzz*), mắt quay vòng tròn ($360^\circ$), nốt nhạc bay.

---

#### 🩴 [TASK-FUN-02] Bộ 3 Vũ Khí Chính Slapstick (Primary Weapons)
* **Dependencies:** `WeaponBase.cs`, `WeaponData.cs`, `ProjectileSpawner.cs`.

- [x] **[TASK-FUN-02.1] `Weapon_Slipper.cs` (Dép Tổ Ong Thần Sa - Hệ Kim):**
  - *Hit 1 (Dép Trái):* Ném dép bay thẳng $4m$, quay về tay (Boomerang trajectory), gây $110\%$ DMG.
  - *Hit 2 (Dép Phải):* Ném dép bay chéo góc $30^\circ$, gây $130\%$ DMG.
  - *Hit 3 (Lốc Dép Vạn Năng):* Xoay vòng $360^\circ$ quăng đôi dép tạo lốc xoáy gom quái và vả liên hoàn 4 hit ($200\%$ DMG), kích hoạt hiệu ứng `Humiliated` (Quê Độ).
- [x] **[TASK-FUN-02.2] `Weapon_Pot.cs` (Nồi Cơm Thạch Sanh - Hệ Thổ):**
  - *Hit 1 (Gõ Nắp):* Gõ nắp nồi gây choáng $0.3s$ trong góc quạt $90^\circ$.
  - *Hit 2 (Hút Quái):* Tạo lực hút chân không gom tối đa 3 quái thường vào trong lòng nồi.
  - *Hit 3 (Bắn Đại Bác):* Phóng quái bay ra như đạn pháo gây $240\%$ DMG; chạm đất nổ rơi $3$ viên Cơm Nắm hồi $5\%$ Max HP.
- [x] **[TASK-FUN-02.3] `Weapon_Pipe.cs` (Điếu Cày Cửu U - Hệ Hỏa):**
  - *Hit 1 (Gõ Cán Điếu):* Đập đầu điếu cày gây $100\%$ DMG + Đẩy lùi $1m$.
  - *Hit 2 (Búng Tàn Lửa):* Bắn tia lửa thiêu đốt $140\%$ Fire DoT trong $2s$.
  - *Hit 3 (Khói Thần Rồng Cuộn):* Phun luồng khói dày đặc $3.5s$, quái đi qua dính trạng thái `Stoned` (Say Thuốc Lào).

---

#### 🧘 [TASK-FUN-03] Bộ Pháp Bảo Hộ Thân (Relics) Slapstick
* **Dependencies:** `Weapon_MeleeBase.cs`, `RunLoadoutState.cs`.

- [x] **[TASK-FUN-03.1] `Relic_SleepingMat.cs` (Chiếu Trải Hoàng Tuyền - Hệ Mộc):**
  - Mỗi $8s$ tự động trải tấm chiếu hoa văn $3\times 2m$ tại vị trí người chơi trong $5s$.
  - *Quái bước vào:* Ngã vật ra ngủ say $3s$ (`Sleeping`).
  - *Người chơi bước vào / Dash qua:* Trạng thái **Trượt Ván Siêu Tốc** ($+100\%$ Move Speed), ủi văng đàn quái.
- [x] **[TASK-FUN-03.2] `Relic_ChickenFeatherBroom.cs` (Chổi Lông Gà Gia Truyền - Hệ Kim):**
  - Kích hoạt theo đòn Combo Hit 3 của vũ khí chính.
  - Triệu hồi Chổi Lông Gà khổng lồ giáng từ trời xuống, Knockback cực đại $12m/s$ và găm dính quái vào tường (Wall Splat).

---

#### 🐓 [TASK-FUN-04] Tuyệt Kỹ Nhân Vật Bựa (Signature Skills)
* **Dependencies:** `PlayerSkillBase.cs`, `ThuSinhSignatureSkill.cs`, `PetController.cs`.

- [x] **[TASK-FUN-04.1] Thư Sinh — `Bút Sa Gà Chết`:**
  - Triệu hồi Gà Chọi Khổng Lồ chạy tốc độ cao mổ liên hoàn vào mông quái, khiến mục tiêu hoảng loạn bỏ chạy.
- [x] **[TASK-FUN-04.2] Đạo Sĩ — `Bùa Tráo Hồn`:**
  - Bắn bùa tráo đổi vị trí tức thì với quái Tinh Anh, để lại Hình Nộm phát nổ thu hút kẻ địch.
- [x] **[TASK-FUN-04.3] Thanh Đồng — `Aura Loa Phường`:**
  - Triệu hồi Loa Phóng Thanh phát sóng âm choáng $0.5s$/nhịp, phản xạ $100\%$ đạn đạo của quái bay ngược lại.
- [x] **[TASK-FUN-04.4] Võ Tăng — `Thiết Đầu Công`:**
  - Húc đầu tên lửa lao thẳng về phía trước, phá hủy đạn đạo và phát tiếng chuông chùa *"BOONG!"* đẩy lùi toàn màn hình.

---

#### 👻 [TASK-FUN-05] AI Kẻ Địch & Boss Slapstick
* **Dependencies:** `Enemy.cs`, `EnemyAttackState.cs`, `DropManager.cs`.

- [x] **[TASK-FUN-05.1] Kẻ Địch Mới: `E_MADOINO` (Ma Đòi Nợ):**
  - AI FSM: Tàng hình áp sát từ sau lưng $\rightarrow$ Thó $50$ Cổ Tiền hoặc $20$ Exp $\rightarrow$ Ôm bao tiền chạy thục mạng trong $5s$.
  - Diệt kịp: Lấy lại tiền + nhân đôi thưởng ($100$ Cổ Tiền / $40$ Exp); Thoát màn hình: Mất vĩnh viễn số tiền đó.
- [x] **[TASK-FUN-05.2] Quái Dân Gian Tương Tác Vui Nhộn:**
  - *Ma Da Trơn Tuột:* Khi nhận sát thương kết liễu, bắn vọt ra xa như viên xà phòng đè bẹp quái khác.
  - *Ma Trơi Say Xỉn:* Bay zíc zắc lượn sóng, nếu né được sẽ đâm sầm vào chướng ngại vật tự choáng $1s$.
- [x] **[TASK-FUN-05.3] Cơ Chế "Tự Húc Đầu Vào Nhau" của Boss Đôi (Ngưu Đầu — Mã Diện):**
  - Khi 2 Boss cùng bật đường chạy húc chữ X: Nếu Dash né đúng lúc $\rightarrow$ 2 Boss tông đầu vào nhau $\rightarrow$ Cùng Choáng $4.0s$, mất $10\%$ Max HP và rơi lượng lớn Exp Gems.

---

#### 🛠️ [TASK-FUN-06] Generator & Tích Hợp Hệ Thống Tự Động
* **Dependencies:** `UpgradeDataGenerator.cs`, `CharacterSelectionPresenter.cs`, `WeaponEvolutionManager.cs`.

- [x] **[TASK-FUN-06.1] `FunCombatDataGenerator.cs` (Editor Tool):**
  - Tự động tạo `WeaponData` cho 3 vũ khí mới (`W_SLIPPER`, `W_POT`, `W_PIPE`) và 2 Relics (`R007`, `R008`).
  - Tự động tạo các thẻ `UpgradeData` nâng cấp / đột phá tương ứng.
- [x] **[TASK-FUN-06.2] Tích hợp Loadout & Gameplay:**
  - Đồng bộ các vũ khí/pháp bảo mới vào UI Sảnh Chờ và hệ thống Drop trong trận.

---

#### 🎨 [TASK-FUN-07] Bộ Hình Ảnh Art (Sprites & Icons) Cho Vũ Khí & Pháp Bảo Slapstick
* **Dependencies:** `Assets/Art/Weapons/`, `generate_image`.

- [ ] **[TASK-FUN-07.1] Bộ 5 Sprite Icons HUD / Card 256x256 Pixel Art Cổ Phong Trong Suốt:**
  - *Icon Dép Tổ Ong:* Màu vàng ngà viền đen dân gian, có lỗ tổ ong đặc trưng.
  - *Icon Nồi Cơm Thạch Sanh:* Nồi đất dân gian bốc khói nghi ngút phát sáng.
  - *Icon Điếu Cày Cửu U:* Điếu tre trúc bọc đồng phát tàn lửa rực sáng.
  - *Icon Chiếu Trải Hoàng Tuyền:* Chiếu cói hoa văn rồng cuộn cổ truyền.
  - *Icon Chổi Lông Gà Gia Truyền:* Cán gỗ nghiến lông ngũ sắc.
- [ ] **[TASK-FUN-07.2] Bộ Sprite Vật Thể & Đạn Đạo In-Game:**
  - Sprite chiếc Dép bay ném Boomerang (xoay quanh trục).
  - Sprite Chiếu Cói trải trên sàn $3\times 2m$ hoa văn dân gian sắc nét.
  - Sprite Hạt Cơm Nắm phát sáng hồi máu.
- [ ] **[TASK-FUN-07.3] Sprite Nhân Vật Kẻ Địch `E_MADOINO` (Ma Đòi Nợ):**
  - Tạo Sprite Ma Đòi Nợ tàng hình nửa trong suốt, vác bao tải tiền cắm đầu chạy.

---

#### ✨ [TASK-FUN-08] Bộ Hiệu Ứng VFX Prefabs & Particle Systems Cho Slapstick Combat
* **Dependencies:** `Assets/VFX/`, `UniversalRenderPipeline`, `GlobalVFXPoolManager.cs`.

- [ ] **[TASK-FUN-08.1] VFX Lốc Xoáy Dép & Hiệu Ứng Vả Mặt:**
  - Particle System lốc xoáy quét $360^\circ$ hút quái vào tâm + Vệt tát cao su bép bép.
- [ ] **[TASK-FUN-08.2] VFX Nồi Cơm Chân Không & Đạn Pháo Nổ Cơm Nắm:**
  - Vòng xoáy hút chân không vào miệng nồi + Vệt khói phóng quái đại bác + Vụ nổ bung cơm nắm phát sáng.
- [ ] **[TASK-FUN-08.3] VFX Đám Mây Khói Thuốc Lào Tiên Lãng:**
  - Khói tím cuộn hình rồng dày đặc tồn tại $3.5s$ + Vòng khói bay quanh đầu quái dính Say Thuốc.
- [ ] **[TASK-FUN-08.4] VFX Đường Trượt Bowling & Chổi Lông Gà Giáng Trời:**
  - Vệt gió lướt tốc độ cao phía sau người chơi khi trượt trên chiếu.
  - Chổi Lông Gà khổng lồ giáng từ đỉnh màn hình xuống kèm sóng xung kích chấn động ($12m/s$).

---

---

### 🔴 Phase 5: UI Polish, Performance & Đóng Gói Phát Hành (FINAL PHASE)

- [ ] 🚨 **[TASK-314] Tối Ưu Hóa Toàn Diện Mobile UI Responsive, Safe Area & Multi-Resolution Adaptation**
- [ ] 🚨 **[TASK-316] Hệ Thống UI Trực Quan Ngũ Hành Cho Người Chơi Mới (MVP)**

#### 🗜️ [TASK-401] Tối Ưu Hóa Bộ Nhớ & Draw Calls UI Bằng Hệ Thống Sprite Atlas v2 (ASTC 6x6)
*Mục tiêu:* Gom 85+ file PNG UI nhỏ lẻ về 3 tấm Sprite Atlas theo vòng đời màn hình, giảm Draw Calls UI từ ~35 xuống còn 1 - 2 Batches/frame, tiết kiệm VRAM và duy trì 60 FPS ổn định trên Android.
* **Dependencies:** `Assets/Art/UI/`, `Unity 2D Sprite Atlas v2`, `generate_ui_framework.py`.

- [ ] **[TASK-401.1] Phân Nhóm 3 Sprite Atlas Theo Vòng Đời Màn Hình (Multi-Atlas Architecture):**
  - **`Atlas_UI_InGame_HUD` ($1024 \times 1024$):** Gom toàn bộ Joystick, Cụm Nút Đánh/Lướt/Signature, Thanh Máu HP, Thanh EXP, Khung Avatar Mini, Khung Timer/Kill, Cán Cân Âm Dương. *Luôn nạp trong Combat Loop.*
  - **`Atlas_UI_MainHub` ($1024 \times 1024$):** Gom Khung Gỗ Tàng Bảo Các, Nút Chọn Tướng, Nút Xuất Trận, Thanh Tab Cài Đặt, Bảng Nâng Cấp Miếu Cổ. *Chỉ nạp tại Sảnh Chờ.*
  - **`Atlas_UI_UpgradeCards` ($1024 \times 1024$):** Gom 3 Khung Thẻ Bài 9-Slice (Common/Rare/Evolution), toàn bộ 30+ Icon Pháp Bảo (W001-W012) & Passive (P001-P012). *Chỉ nạp khi mở Level Up Modal hoặc Tủ Đồ.*
- [ ] **[TASK-401.2] Cấu Hình Sprite Atlas v2 & Bảo Toàn 9-Slice Borders:**
  - Thiết lập thuộc tính `Include in Build = true`, `Allow Rotation = false`, `Tight Packing = false`, `Padding = 4px` (chống lem viền / Texture Bleeding).
  - Tự động kế thừa và bảo toàn 100% thông số `spriteBorder` (9-Slice) của từng Sprite con mà không làm méo hoa văn góc Mây Cuộn / Triện Đông Sơn.
- [ ] **[TASK-401.3] Chuẩn Hóa Nén Texture ASTC 6x6 Cho Android Platform:**
  - Thiết lập Default Override Android: Format `ASTC 6x6 block` (giảm 75% dung lượng VRAM so với RGBA 32-bit mà chất lượng hiển thị sắc nét 99%).
  - Kiểm thử Draw Calls trên *Frame Debugger*: Xác nhận toàn bộ In-Game HUD chỉ tốn đúng **1 duy nhất Draw Call** khi render.

- [ ] **[TASK-402] Mobile Stress Test (60 FPS ARPG)**
- [ ] **[TASK-403] Android AAB Build & Signing (Google Play Release)**

