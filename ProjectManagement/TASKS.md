# Danh Sách Nhiệm Vụ & Task Tracker — VONG XUYÊN

Tài liệu quản lý danh sách công việc (Kanban Board Task Tracker) phân chia theo các hạng mục của dự án **Vong Xuyên (Android Release - GDD v5.0 Action RPG Roguelite)**.  
*Tài liệu tham chiếu chi tiết:* 🎮 **[ProjectZombie_GDD.md](file:///c:/Users/thuon/Unity/Projectzombie/GameDesignDoc/ProjectZombie_GDD.md)** | 🎨 **[UI_ART_DIRECTION_GUIDE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/UI_ART_DIRECTION_GUIDE.md)** | 📐 **[SYSTEM_ARCHITECTURE.md](file:///c:/Users/thuon/Unity/Projectzombie/.agents/references/SYSTEM_ARCHITECTURE.md)**

---

## 📋 Kanban Board Summary

| 🔴 Backlog (Sắp Tới / Xử Lý Sau) | 🟡 In Progress (Đang Triển Khai - ARPG & Slapstick) | 🟢 Done (Đã Hoàn Thành) |
|---|---|---|
| 📱 **[TASK-314] Mobile UI Responsive & Safe Area** | ⚔️ **[TASK-ARPG-01] Tái Cấu Trúc Lõi Vũ Khí & 3-Hit Combo** | ☯️ **[Hạng Mục 1] Lõi Ngũ Hành, Âm Dương & 0-Alloc Combat** |
| 🪓 **[TASK-316] Hệ Thống UI Trực Quan Ngũ Hành (Bát Quái / Glow)** | 🏃 **[TASK-ARPG-02] Trạng Thái Nhân Vật & Combat Controls** | 📦 **[Hạng Mục 2] Database 12 Pháp Bảo, 12 Tiến Hóa & Cổ Tiền** |
| 💥 **[TASK-GP-03] Treasure Chest Gacha Popup (1-3-5 Items)** | 🎴 **[TASK-ARPG-03] Hệ Thống Nâng Cấp Trong Trận & Đột Phá** | 🧟 **[Hạng Mục 3] AI 5 Yêu Ma, 2 Boss & 20-Min Wave Timeline** |
| 🗜️ **[TASK-401] Texture Compression ASTC & Sprite Atlas** | 🤪 **[TASK-FUN-01 -> 06] Combat & Enemy Slapstick/Bựa/Fun** | 📱 **[TASK-300] Mobile Controls (Joystick, Dash, Skill Buttons)** |
| 🧪 **[TASK-402] Mobile Stress Test (60 FPS ARPG)** | 🏛️ **[TASK-ARPG-05] UI Sảnh Chờ Chọn Loadout Vũ Khí & Relic** | 📊 **[TASK-312 & 313] In-Game HUD Visuals & Upgrade Cards UI** |
| 🚀 **[TASK-403] Android AAB Build & Signing Release** | ✨ **[TASK-VFX-05] Hit Sparks, Hit Stop 0.04s & Shake Polish** | 💎 **[TASK-EXP-01] Hệ Thống Hạt EXP Zero-Alloc & Magnet** |
| | 🎵 **[TASK-GP-05] Audio Feedback Layers (Hit/Death/Chime)** | 🎨 **[TASK-VFX-01 -> 04] Bộ VFX Vệt Chém, Orbit & Shockwave** |

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

#### ⚔️ [TASK-ARPG-01] Tái Cấu Trúc Lõi Vũ Khí & Chuỗi 3-Hit Combo (Primary vs Relics)
*Mục tiêu:* Phân tách rõ ràng giữa 1 Vũ khí chính đánh tay và tối đa 3 Pháp bảo hộ thân.
* **Dependencies:** `DamageUtility.cs`, `TargetingUtility.cs`, `ElementCycleManager.cs`.

- [x] **[TASK-ARPG-01.1] Nâng Cấp `WeaponBase.cs` Hỗ Trợ Chuỗi Combo:**
  - Thêm thuộc tính `CurrentComboStep` ($1 \rightarrow 2 \rightarrow 3$), `MaxComboSteps = 3`, `ComboResetTime = 1.0s`.
  - Bổ sung hàm `TriggerActiveComboAttack(int step)` và cơ chế reset combo về nhát 1 nếu quá thời gian chờ.
  - Cung cấp event `public event Action<DamageData, Collider2D> OnHitEnemy` để các Pháp bảo và kỹ năng lắng nghe kích hoạt hiệu ứng On-Hit.
- [x] **[TASK-ARPG-01.2] Đa Hình Hóa Hitbox & Sát Thương Trong `Weapon_MeleeBase.cs`:**
  - Mỗi bước Combo có thông số riêng:
    - *Nhát 1:* Hitbox hẹp phía trước, Damage hệ số $1.0\times$.
    - *Nhát 2:* Hitbox quét quạt ngang rộng, Damage hệ số $1.2\times$.
    - *Nhát 3:* Hitbox mở rộng / đâm sâu, Damage hệ số $1.8\times$, Knockback force mạnh $+50\%$ và kích hoạt Camera Shake.
  - Cập nhật [Weapon_DualSlash.cs](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/Weapons/Weapon_DualSlash.cs) / `Weapon_MeleeBase` tuân thủ chuỗi 3 nhát này.
- [x] **[TASK-ARPG-01.3] Tái Cấu Trúc `WeaponManager.cs` Theo Mô Hình Loadout:**
  - Định nghĩa rõ: `public WeaponBase PrimaryWeapon { get; }` (Chỉ duy nhất 1 vũ khí slot chính).
  - Định nghĩa danh sách: `public IReadOnlyList<WeaponBase> RelicWeapons { get; }` (Tối đa 3 Pháp bảo mang theo).
  - Sửa đổi hàm `TriggerPrimaryAttack()`: Kích hoạt `PrimaryWeapon.TriggerActiveComboAttack()` mượt mà.
  - Vòng lặp `Tick()`: Chỉ chạy cho các `RelicWeapons` (nếu là dạng Aura hộ thân) hoặc đăng ký On-Hit event của `PrimaryWeapon`.
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

### 🔴 Phase 5: UI Polish, Performance & Đóng Gói Phát Hành (FINAL PHASE - XỬ LÝ SAU)

- [ ] 🚨 **[TASK-314] Tối Ưu Hóa Toàn Diện Mobile UI Responsive, Safe Area & Multi-Resolution Adaptation**
- [ ] 🚨 **[TASK-316] Hệ Thống UI Trực Quan Ngũ Hành Cho Người Chơi Mới (MVP)**
- [ ] **[TASK-401] Texture Compression ASTC & Sprite Atlas (ASTC 6x6)**
- [ ] **[TASK-402] Mobile Stress Test (60 FPS ARPG)**
- [ ] **[TASK-403] Android AAB Build & Signing (Google Play Release)**

