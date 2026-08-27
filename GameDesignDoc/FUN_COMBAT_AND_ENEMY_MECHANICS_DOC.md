# TÀI LIỆU THIẾT KẾ KỸ THUẬT & GAMEPLAY CHI TIẾT: HỆ THỐNG COMBAT & ENEMY SLAPSTICK / BỰA / FUN

> **Dự án:** Vong Xuyên (Project Zombie)  
> **Mã tài liệu:** `DOC_FUN_COMBAT_V1.0`  
> **Trạng thái:** Official Gameplay Feature Specification  
> **Tương thích:** FSM Engine, ScriptableObject Architecture, Unity 2022.3+ LTS URP

---

## 1. TỔNG QUAN & NGUYÊN LÝ THIẾT KẾ (CORE PILLARS)

Để tạo sự khác biệt vượt bậc so với các tựa game Survivor/Hack & Slash thông thường, hệ thống **Combat & Enemy Slapstick/Fun** đưa vào 3 trụ cột thiết kế:

1. **Vật Lý Hỗn Loạn (Ragdoll & Momentum Chaos):** Kẻ địch không chỉ ngã xuống đơn điệu mà có thể bị ném bay, trượt ngã sấp mặt, va vào tường phát ra âm thanh hài hước hoặc va vào nhau gây sát thương chuỗi.
2. **Hiệu Ứng Khống Chế "Khó Đỡ" (Troll Crowd Control):** Trạng thái xấu hổ (*Quê Độ*), buồn ngủ (*Trải Chiếu Nằm Luôn*), say khói (*Phê Thuốc Lào*) hoặc bị mê hoặc nhảy nhót theo điệu nhạc.
3. **Tương Tác Phản Đòn & Tự Bóp (Friendly Fire & Self-Stun):** Kẻ địch và Boss có thể tự gây hại cho nhau hoặc tự đâm đầu vào chướng ngại vật khi người chơi thao tác khéo léo (Dash né đòn).

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          VÒNG LẶP COMBAT HÀI HƯỚC                           │
│                                                                             │
│  [Vũ Khí/Kỹ Năng Bựa] ──► [Kích Hoạt Troll CC] ──► [Vật Lý Hỗn Loạn/Ragdoll]│
│          ▲                                                    │             │
│          │                                                    ▼             │
│  [Hồi Phục & Thưởng Exp/Tiền] ◄── [Quái Bị Bắn Nổ / Tự Đánh Lẫn Nhau]       │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. CHI TIẾT PHÁP BẢO HỘ THÂN BỰA & FUN (SLAPSTICK RELICS)

Mọi trang bị Slapstick đều được phân bổ vào **Slot Pháp Bảo Hộ Thân (Mang theo 1 Pháp Bảo vào trận)**, tự động hỗ trợ nhân vật:

### 2.1. `W_SLIPPER` — Dép Tổ Ong Thần Sa (Vũ Khí Ném Boomerang Slapstick)

* **Hệ Ngũ Hành:** Kim
* **Đặc tính:** Tầm trung, đòn đánh ném bay vòng cung và quay về tay, phát ra âm thanh *"Bẹp! Bẹp!"* giòn giã.
* **Chuỗi Combo 3 Đòn:**
  * **Hit 1 (Dép Trái):** Ném chiếc dép bên trái bay thẳng $4m$, gây $110\%$ Damage.
  * **Hit 2 (Dép Phải):** Ném tiếp chiếc dép bên phải bay chéo, gây $130\%$ Damage.
  * **Hit 3 (Lốc Dép Vạn Năng):** Người chơi quay một vòng quăng cả đôi dép tạo cơn lốc xoáy nhỏ $360^\circ$ bán kính $3m$, gom quái lại gần và vả liên hoàn $4$ hit, tổng $200\%$ Damage.
* **Cơ chế Đột Phá — Hiệu ứng "Quê Độ" (`StatusEffect_Humiliated`):**
  * Quái trúng đòn 3 có $40\%$ tỷ lệ bị *"Quê Độ"* trong $1.5s$: Quái buông vũ khí ôm mặt vì xấu hổ, đồng thời quay sang đấm con quái gần nhất $50$ sát thương.
* **Audio & Visual Cues:**
  * Sprite: Chiếc dép tổ ong màu vàng hoặc trắng ngà có viền đen dân gian.
  * Sound: Tiếng tát cao su cực đanh + tiếng *"Ối dồi ôi"* ngẫu nhiên khi crit.

---

### 2.2. `W_POT` — Nồi Cơm Thạch Sanh (Vũ Khí Gom Quái & Bắn Đạn Đại Bác)

* **Hệ Ngũ Hành:** Thổ
* **Đặc tính:** Cận chiến nặng, kiểm soát đám đông cực mạnh và tương tác vật lý ném quái.
* **Chuỗi Combo 3 Đòn:**
  * **Hit 1 (Gõ Nắp):** Vung nắp nồi đập mạnh xuống đất gây $100\%$ Damage, làm choáng nhẹ $0.3s$ trong góc quạt $90^\circ$.
  * **Hit 2 (Hút Quái):** Mở nắp nồi tạo lốc xoáy chân không bán kính $3.5m$, hút tối đa $3$ quái thường vào trong nồi (khiến quái biến mất tạm thời trong $0.5s$).
  * **Hit 3 (Phóng Quái Đại Bác):** Bắn phọt đám quái đó ra như đạn pháo theo đường thẳng $6m$, gây $240\%$ Sát thương lên toàn bộ kẻ địch trên đường bay.
* **Cơ chế Đột Phá — "Cơm Niêu Vô Tận":**
  * Quái sau khi bị bắn ra chạm đất sẽ nổ tung tạo thành $3$ viên Cơm Nắm phát sáng; nhặt cơm hồi $5\%$ Max HP.
* **Audio & Visual Cues:**
  * Sprite: Nồi đất dân gian bốc khói nghi ngút.
  * Sound: Tiếng kim loại *"Xoong! Keng!"* và tiếng súng cối khi bắn quái ra.

---

### 2.3. `W_PIPE` — Điếu Cày Cửu U (Vũ Khí Phun Khói Gây Lú)

* **Hệ Ngũ Hành:** Hỏa
* **Đặc tính:** Phun vùng khói tầm trung, khống chế quỹ đạo di chuyển của địch.
* **Chuỗi Combo 3 Đòn:**
  * **Hit 1 (Gõ Cán Điếu):** Gõ đầu điếu cày gây $100\%$ Damage, hất lùi quái $1m$.
  * **Hit 2 (Búng Tàn Lửa):** Búng tia tàn thuốc cháy rực ra xa gây $140\%$ Fire DoT trong $2s$.
  * **Hit 3 (Khói Thần Rồng Cuộn):** Rít một hơi dài và nhả đám mây khói thuốc lào dày đặc tồn tại trong $3.5s$.
* **Cơ chế Đột Phá — "Say Thuốc Lào" (`StatusEffect_Stoned`):**
  * Quái đi vào vùng khói bị đảo ngược hướng di chuyển (đi loạng choạng giật lùi hoặc quay vòng tròn), sau $2s$ bị sặc thuốc nổ văng $150\%$ Hỏa sát thương lan sang quái xung quanh.
* **Audio & Visual Cues:**
  * Sound: Tiếng rít điếu cày *"Rọt... rọt... roẹt!"* đặc trưng và tiếng ho sặc sụa hoạt hình.

---

## 3. CHI TIẾT PHÁP BẢO HỘ THÂN (RELICS) FUN & SLAPSTICK

### 3.1. `R007` — Chiếu Trải Hoàng Tuyền (Bẫy Ngủ & Đường Trượt Siêu Tốc)

* **Hệ Ngũ Hành:** Mộc
* **Cơ Chế Kích Hoạt:** Mỗi $8s$, tự động thả một tấm chiếu cói hoa văn dân gian kích thước $3\times 2m$ xuống vị trí người chơi vừa đứng, tồn tại $5s$.
* **Tương Tác 2 Chiều Cực Kỳ Độc Đáo:**
  1. **Đối với Kẻ Địch (Enemy Trap):** Bất kỳ quái nào bước chân vào mép chiếu sẽ ngay lập tức **ngã vật ra ngủ say** trong $3s$ (`StatusEffect_Sleeping`). Đòn đánh đầu tiên đánh thức quái ngủ sẽ nhận $+100\%$ Sát thương bạo kích.
  2. **Đối với Người Chơi (Fast Lane):** Nếu người chơi Dash hoặc bước lên chiếu, người chơi chuyển sang trạng thái **Trượt Ván Siêu Tốc** (+100% Tốc độ di chuyển), miễn nhiễm làm chậm và tông văng đàn quái như bowling.

---

### 3.2. `R008` — Chổi Lông Gà Gia Truyền (Đòn Phạt Tuổi Thơ)

* **Hệ Ngũ Hành:** Kim
* **Cơ Chế Kích Hoạt:** Kích hoạt theo đòn kết thúc Combo (Hit 3) của bất kỳ vũ khí chính nào.
* **Hiệu Ứng Combat:** Triệu hồi một chiếc Chổi Lông Gà khổng lồ giáng thẳng từ trên trời xuống đập bẹp đàn quái:
  * Lực đẩy lùi (Knockback Force): Cực đại ($12m/s$).
  * Nếu quái bị hất văng đập vào mép bản đồ hoặc chướng ngại vật đá, quái bị dính chặt vào tường $1s$ (Wall Splat) và nhận thêm sát thương va đập.

---

## 4. BỘ TUYỆT KỸ NHÂN VẬT ĐẶC BIỆT (SIGNATURE SKILLS)

| Nhân Vật | Tên Kỹ Năng | Cooldown | Mô Tả Kỹ Thuật & Hiệu Ứng Gameplay |
|---|---|---|---|
| **Thư Sinh** | **Bút Sa Gà Chết** | $25s$ | Vẽ một con Gà Chọi Khổng Lồ xuất hiện trong $6s$. Con gà chạy tốc độ cao mổ dồn dập vào mông quái (Single Target DPS cực cao), khiến quái trúng đòn bị hoảng loạn bỏ chạy khỏi người chơi. |
| **Đạo Sĩ** | **Bùa Tráo Hồn** | $20s$ | Bắn một lá bùa tráo đổi vị trí tức thì (`SwapPosition`) với 1 con quái Tinh Anh trong tầm nhìn. Vị trí cũ của người chơi để lại một hình nộm kích nổ khi quái xung quanh lao vào tấn công. |
| **Thanh Đồng** | **Aura Loa Phường** | $28s$ | Triệu hồi chiếc Loa Phóng Thanh phát nhạc hát văn cực lớn trong $5s$: Sóng âm gây Choáng từng đợt mỗi $0.5s$, đồng thời phản xạ toàn bộ đạn đạo của quái bay ngược về kẻ bắn (Bullet Deflection). |
| **Võ Tăng** | **Thiết Đầu Công** | $22s$ | Võ Tăng lao đầu về phía trước với tốc độ cực nhanh trong $0.6s$, phá hủy mọi đạn đạo trên đường đi. Khi tông trúng quái to, phát ra tiếng chuông chùa *"BOONG!"* ngân vang đẩy lùi toàn màn hình. |

---

## 5. THIẾT KẾ KẺ ĐỊCH & BOSS SLAPSTICK

### 5.1. Kẻ Địch Mới: `E_MADOINO` — Ma Đòi Nợ (Debt Collector Ghost)

* **Mục tiêu:** Tạo áp lực vui nhộn và thay đổi độ ưu tiên tiêu diệt quái.
* **Hành vi (AI FSM):**
  * Không tấn công làm giảm HP của người chơi.
  * Tàng hình áp sát người chơi từ phía sau, chạm vào người chơi sẽ "thó" mất **$50$ Cổ Tiền hoặc $20$ Exp Gems** rồi ngay lập tức giơ túi tiền cắm đầu bỏ chạy thật nhanh (+80% Move Speed).
* **Cơ chế Thưởng/Phạt:**
  * Người chơi có **$5$ giây** để đuổi theo và tiêu diệt nó.
  * Nếu diệt thành công: Lấy lại toàn bộ số tiền đã mất + Rơi thêm gấp đôi ($100$ Cổ Tiền / $40$ Exp) kèm rương bảo vật.
  * Nếu để nó chạy thoát khỏi mép màn hình: Mất vĩnh viễn số tiền đó trong run đấu.

### 5.2. Quái Dân Gian Tương Tác Đặc Biệt

1. **Quỷ Nhập Tràng (Mê Nhảy Múa):** Khi ở trong tầm sóng âm của *Trống Đồng Đông Sơn* hoặc *Aura Loa Phường*, quái lập tức ngừng tấn công, giơ 2 tay lên nhảy theo nhịp, biến thành bức tường thịt che đạn cho người chơi.
2. **Ma Da (Trơn Tuột Như Xà Phòng):** Khi bị đánh trúng đòn chí mạng, Ma Da không chết ngay mà bắn vọt ra xa như viên xà phòng trơn tuột, tông trúng quái khác gây sát thương liên hoàn.
3. **Ma Trơi (Say Xỉn):** Quỹ đạo bay hình sin lượn sóng zíc zắc; nếu người chơi né được cú lao của nó, nó sẽ đâm sầm vào đá/tường và tự bất tỉnh $1s$.

### 5.3. Boss Đôi Ngưu Đầu — Mã Diện: Cơ Chế "Đấu Vật Tự Bóp"

* **Tình huống kích hoạt:** Cả hai con Boss chuẩn bị tung chiêu Lướt Húc Càn (*Bull Dash*) từ 2 phía đối diện nhau vào người chơi (Có vệt đỏ cảnh báo cắt nhau hình chữ X).
* **Pha xử lý Pro-play / Slapstick:**
  * Nếu người chơi đứng ngay giao điểm vệt đỏ và bấm **Dash** né đúng vào $0.1s$ trước khi chạm:
  * Hai con Boss sẽ **tông thẳng đầu vào nhau** $\rightarrow$ Tiếng *"Bốp!"* cực to kèm hiệu ứng sao bay trên đầu $\rightarrow$ Hai con Boss tự choáng lẫn nhau trong **$4.0s$** và mất $10\%$ HP tối đa, rơi ra máu và ngọc Exp lớn.

---

## 6. HỆ THỐNG HIỆU ỨNG TRẠNG THÁI MỚI (NEW STATUS EFFECTS MATRIX)

| Mã Trạng Thái | Tên Hiệu Ứng | Thời Gian | Tác Động Lên Quái | Visual FX & Animation |
|---|---|---|---|---|
| `Status_Humiliated` | **Quê Độ** | $1.5s$ | Đứng im ôm mặt hoặc đấm quái bên cạnh $50$ dmg. | Biểu cảm mặt xấu hổ / Giọt mồ hôi hoạt hình. |
| `Status_Sleeping` | **Ngủ Trên Chiếu** | $3.0s$ | Bất động hoàn toàn; đòn đánh thức gây $\times 2.0$ Dmg. | Bong bóng mũi phập phồng chữ *"Zzz"*. |
| `Status_Stoned` | **Say Thuốc Lào** | $3.5s$ | Di chuyển zíc zắc giật lùi; sau $2s$ nổ ho sặc sụa. | Mắt quay vòng tròn $360^\circ$ + Khói cuộn đầu. |
| `Status_RagdollFlight` | **Bị Bắn / Trượt Ngã** | $0.8s$ | Bay hình parabol, đè bẹp quái trên đường bay. | Quái xoay tít trên không + Vệt gió hoạt hình. |
| `Status_Dancing` | **Mê Nhảy Múa** | $4.0s$ | Dừng đánh, giơ tay nhảy nhót, làm bia đỡ đạn. | Nốt nhạc bay quanh người + Lắc lư trái phải. |

---

## 7. KIẾN TRÚC KỸ THUẬT & HƯỚNG DẪN MỞ RỘNG (UNITY IMPLEMENTATION)

### 7.1. Sơ Đồ Cấu Trúc Lớp (Class Architecture)

```
                       ┌────────────────────────┐
                       │       WeaponBase       │
                       └───────────┬────────────┘
                                   │
         ┌─────────────────────────┼─────────────────────────┐
         ▼                         ▼                         ▼
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│  Weapon_Slipper  │     │   Weapon_Pot     │     │   Weapon_Pipe    │
│  (Boomerang CC)  │     │ (Vacuum & Shoot) │     │ (Smoke Area DoT) │
└──────────────────┘     └──────────────────┘     └──────────────────┘

                       ┌────────────────────────┐
                       │   EnemyStatusController│
                       └───────────┬────────────┘
                                   │
         ┌─────────────────────────┼─────────────────────────┐
         ▼                         ▼                         ▼
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│ ApplyHumiliated()│     │  ApplySleeping() │     │   ApplyStoned()  │
└──────────────────┘     └──────────────────┘     └──────────────────┘
```

### 7.2. ScriptableObject Định Nghĩa Vũ Khí Mới (`WeaponData.cs`)

Vũ khí mới được tích hợp liền mạch vào hệ thống ScriptableObject sẵn có mà không làm gãy cấu trúc loadout:
* `WeaponId`: `"W_SLIPPER"`, `"W_POT"`, `"W_PIPE"`
* `ElementalType`: `Metal`, `Earth`, `Fire`
* `BaseDamage`: $25$, $35$, $20$
* `Cooldown`: $1.0s$, $1.4s$, $1.1s$
* `KnockbackForce`: $6.0$, $10.0$, $4.0$

### 7.3. Điểm Móc Nối (Hook Points) Trong Codebase

1. **`Assets/Features/Enemies/EnemyStatusController.cs`:**
   * Bổ sung các phương thức: `ApplyHumiliated()`, `ApplySleeping()`, `ApplyStoned()`, `ApplyDancing()`.
   * Tích hợp flag `IsCrowdControlled` để vô hiệu hóa tạm thời `CombatMovementStrategy` và `AttackStrategy`.
2. **`Assets/Features/Player/Skills/`:**
   * Thêm các script: `ThuSinhSignatureSkill.cs` (Spawns Chicken Pet FSM), `DaoSiSignatureSkill.cs` (Swap position), `ThanhDongSignatureSkill.cs` (Deflect Aura), `VoTangSignatureSkill.cs` (Headbutt Rocket).
3. **`Assets/Features/Weapons/`:**
   * Thêm: `Weapon_Slipper.cs`, `Weapon_Pot.cs`, `Weapon_Pipe.cs`, `Weapon_Mat.cs`.

---

## 8. KẾT LUẬN & BƯỚC TIẾP THEO

Tài liệu này cung cấp toàn bộ đặc tả chi tiết về **Hệ thống Combat & Enemy Slapstick/Fun** cho dự án **Vong Xuyên**. Hệ thống vừa giữ trọn bản sắc văn hóa dân gian Việt Nam, vừa mang lại trải nghiệm chặt chém đã tay, cười sảng khoái và tính lan tỏa cao (viral meme gameplay) trên nền tảng di động.
