# 📋 KẾ HOẠCH NÂNG CẤP TOÀN DIỆN HỆ THỐNG PHÁP BẢO & CHỈ DẤU (TASK.md)

Tài liệu này đóng vai trò là **Lộ trình thực thi (Action Roadmap)** phân tách rõ ràng từng hạng mục nhiệm vụ, độ ưu tiên, sự phụ thuộc (dependencies) và giải pháp kỹ thuật nhằm nâng cấp toàn bộ 17 Pháp Bảo Cổ Phong từ đơn điệu sang hệ thống chiến thuật hành động đỉnh cao.

---

## 🎯 PHÂN TÍCH HIỆN TRẠNG & MỤC TIÊU CỐT LÕI

| Hạng mục | Hiện Trạng (Cũ) | Mục Tiêu Sau Nâng Cấp (Mới) |
| :--- | :--- | :--- |
| **Hệ Thống Chỉ Dấu (Aiming)** | Chỉ có Line/Cone/Circle đơn giản. | Bổ sung **VectorWall** (Tường trận), **CurvedTrajectory** (Cong Parabol), **RhythmPulse** (Sóng nhịp điệu). *(Đã triển khai core)* |
| **Cơ Chế Kích Hoạt (Casting)** | Bấm 1 lần xả chiêu $\rightarrow$ Vào Cooldown. | Hỗ trợ **Recast 2 Giai Đoạn**, **Phản Đòn (Parry Timing)**, **Đặt Cọc/Triệu Hồi (Turret Placement)**. |
| **Tương Tác Tướng & Pháp Bảo** | Tướng đánh riêng, Pháp bảo nổ riêng. | Tương tác môi trường: Tướng Dash/Chém vào pháp bảo kích hoạt phản ứng dây chuyền (Synergy Combo). |
| **Trải Nghiệm Thị Giác (VFX/SFX)** | Hiệu ứng tĩnh, thiếu độ nảy giật (Impact). | Đạt chuẩn **Art Master DNA Cổ Phong**: Decal nở trận, Shockwave phản lực, camera shake nhẹ. |

---

## 🏗️ DANH SÁCH NHIỆM VỤ CHI TIẾT (TASK LIST)

### 📌 Giai Đoạn 1: Hoàn Thiện & Tinh Chỉnh 4 Pháp Bảo Đột Phá Tiêu Biểu (Priority: HIGH)

- [x] **Task 1.1: Core Indicator & Recast Engine**
  - Đã tích hợp `VectorWall`, `CurvedTrajectory`, `RhythmPulse` vào `SkillAimIndicatorController.cs`.
  - Đã nâng cấp `WeaponBase.cs` hỗ trợ `RelicCastPhase` (Ready $\rightarrow$ RecastReady $\rightarrow$ Cooldown).
- [x] **Task 1.2: Dép Tổ Ong Thần Sa (`W_SLIPPER`)**
  - *Phase 1:* Ném Boomerang bay vòng cung gom quái (`CurvedTrajectory`).
  - *Phase 2 (Recast):* Tướng lướt vụt tới tung cước Song Phi dẫm nổ Shockwave (`DashLine`).
- [x] **Task 1.3: Nước Thánh Chùa Hương (`W011`)**
  - Chỉ dấu `VectorWall` dựng Bức Tường Nước Thánh 4 giếng thiêng phong tỏa quái và hồi máu.
- [x] **Task 1.4: Nồi Cơm Thạch Sanh (`W_POT`)**
  - Chỉ dấu `CircleReticle` thả cắm Nồi Cơm từ xa, tạo lốc hút quái diện rộng và đẻ Cơm Nắm.
- [x] **Task 1.5: HUD Button Recast Feedback**
  - Đã tích hợp `SetRecastGlow()` và kết nối sự kiện `OnRelicPhaseChanged` trong `RelicSkillPresenter.cs` & `RelicSkillButtonView.cs`: Nút `Btn_RelicSkill` tự động phát xung viền sáng vàng kim khi bước vào Phase 2.

---

### 📌 Giai Đoạn 2: Nâng Cấp 5 Pháp Bảo Chủ Động Còn Lại (Active Relics) (Priority: MEDIUM)

- [x] **Task 2.1: Nỏ Thần (`W001` - Kim)**
  - *Cơ chế mới:* **Linh Tiễn Thần Uy Xuyên Thấu Vô Tận (Infinite Pierce & Knockback Barrage)**.
  - *Chỉ dấu:* `LineArrow` siêu dài 14m. Khai hỏa 3 đợt bão tên thần uy xuyên thấu 100% mục tiêu và đẩy lùi bầy quái 8m.
- [x] **Task 2.2: Lựu Đạn Thần Sa (`W006` - Hỏa)**
  - *Cơ chế mới:* **Cụm Bom Rải Thảm & Nổ Kích Hoạt Dây Chuyền**.
  - *Chỉ dấu:* `CircleReticle` hiển thị vùng nổ. Quăng chùm 3 hạt Thần Sa nổ tung liên hoàn bão lửa diện rộng.
- [x] **Task 2.3: Đao Cửu Vĩ (`W008` - Hỏa)**
  - *Cơ chế mới:* **Hỏa Long Bộc Phát (8-Direction Dragon Slash Stance)**.
  - *Chỉ dấu:* `ConeSector` góc rộng 120 độ. Tăng 35% tốc đánh, trảm quét Hỏa Long 8 hướng liên tục trong 5s.
- [x] **Task 2.4: Điếu Cày Cửu U (`W_PIPE` - Hỏa)**
  - *Cơ chế mới:* **Bão Khói Tương Tác Địa Hình (VectorWall Terrain Cloud)**.
  - Khói thuốc rồng cuộn diện rộng 6m làm chậm 60% quái và thiêu đốt DoT liên tục.

---

### 📌 Giai Đoạn 3: Nâng Cấp Chiều Sâu 9 Pháp Bảo Bị Động (Passive Relics) (Priority: MEDIUM)

- [x] **Task 3.1: Bút Phán Quan (`W002` - Kim)**
  - *Cơ chế mới:* Kế thừa `Weapon_OnHitRelicBase`: Khi Hero chém trúng quái, tự động vung nhát bút mực Chu Sa phán quyết gây sát thương xuyên giáp.
- [x] **Task 3.2: Bùa Trấn Yêu (`W003` - Mộc)**
  - *Cơ chế mới:* Kế thừa `Weapon_Orbit`: Vòng lá bùa thần bảo hộ xoay quanh cản phá quái áp sát và đẩy lùi.
- [x] **Task 3.3: Cửu Vĩ Hồ Trảo (`W004` - Hỏa)**
  - *Cơ chế mới:* **Cửu Vĩ Cuồng Nộ (Low HP Berserk)**. Khi máu Tướng dưới 35%, tự động nhân đôi đàn dơi hồ ly và x2.5 hiệu suất Hút Máu (Lifesteal) cứu nguy.
- [x] **Task 3.4: Trượng Long Vương (`W009` - Thủy)**
  - *Cơ chế mới:* **Sét Thủy Long Lan Truyền (Chain Lightning)**. Bắn cầu sét tự động nảy chuỗi qua 6 mục tiêu.
- [x] **Task 3.5: Chiếu Trải Hoàng Tuyền (`R007` - Mộc)**
  - *Cơ chế mới:* Thả chiếu khiến quái ngủ say (x2 Crit). Khi Tướng bước lên chiếu sẽ trượt lướt ván lướt nhanh hất văng quái như chơi bowling và gây sát thương va chạm!
- [x] **Task 3.6: Chổi Lông Gà Gia Truyền (`R008` - Kim)**
  - *Cơ chế mới:* **Đòn Phạt Tuổi Thơ (Combo 3 Giant Smash)**. Tự động giáng Chổi Lông Gà khổng lồ khi kết thúc Combo 3, Knockback 12m/s và gây Choáng 0.8s.

---

### 📌 Giai Đoạn 4: Đánh Bóng Visual VFX, SFX & Cân Bằng Gameplay (Priority: LOW)

- [x] **Task 4.1: VFX Decal & Impact Shaders**
  - Tích hợp Decal vòng quay Cổ Phong cho Bát Quái Trận và Bão Khói Thuốc Lào.
- [x] **Task 4.2: FMOD / Audio Trọng Lực**
  - Gắn tiếng nện đá Song Phi, tiếng chuông Trống Đồng và tiếng vung Chổi Lông Gà đanh thép.
- [x] **Task 4.3: Tinh Chỉnh Stats & Cân Bằng Cooldown**
  - Đồng bộ bảng chỉ số trong các ScriptableObject `Assets/_Data/Weapons/...`.
