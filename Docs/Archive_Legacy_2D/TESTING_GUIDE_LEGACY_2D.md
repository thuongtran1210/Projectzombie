# 🎮 HƯỚNG DẪN TEST & REVIEW CHI TIẾT HỆ THỐNG PHÁP BẢO MỚI (TESTING_GUIDE.md)

Tài liệu này được biên soạn để bạn có thể **trải nghiệm, kiểm thử và đánh giá từng Pháp Bảo** một cách dễ dàng và trực quan nhất trên Unity Editor.

---

## 🎯 CÁC BƯỚC CHUẨN BỊ TRƯỚC KHI TEST
1. **Mở Unity Editor** và mở Scene Gameplay (hoặc Scene Main Menu).
2. **Vào Tàng Bảo Các / Loadout**: Chọn Pháp Bảo bạn muốn kiểm thử vào **Ô Pháp Bảo Hộ Thân (Slot 2)**.
3. Nhấn nút **Play**.

---

## ⚔️ PHẦN 1: TEST NHÓM PHÁP BẢO CHỦ ĐỘNG (ACTIVE RELICS)

### 1. Dép Tổ Ong Thần Sa (`W_SLIPPER` - Hệ Kim) — *Cơ Chế Recast 2 Giai Đoạn*
* **Thao tác ngắm bắn (Phase 1):**
  - **Đè & Kéo nút Pháp Bảo (Phím `E` / Touch Drag):** Quan sát **Chỉ Dấu Đường Cong Parabol (`CurvedTrajectory`)** uốn lượn theo hướng kéo ngắm, đánh dấu điểm xa nhất chiếc Dép sẽ bay tới.
  - **Nhả tay (Kích hoạt Phase 1):** Chiếc Dép Tổ Ong bay vòng cung xé gió gom quái và kích hoạt Lốc Dép Vạn Năng.
* **Thao tác Tái Kích Hoạt (Phase 2 - Recast):**
  - **Quan sát HUD:** Nút `Btn_RelicSkill` lập tức **bật viền sáng Vàng Kim nhấp nháy (Glow Pulse)** và mở ra cửa sổ đếm ngược 3 giây.
  - **Bấm lần 2 (Phím `E` / Chạm nút):** Tướng lập tức **lướt vụt tới vị trí chiếc Dép đang xoay**, tung cú đá **Song Phi** dẫm nổ Shockwave diện rộng (Đẩy lùi cực mạnh 16m/s và x3.5 sát thương).

---

### 2. Nước Thánh Chùa Hương (`W011` - Hệ Thủy) — *Chỉ Dấu Dựng Tường Trận Địa*
* **Thao tác ngắm bắn:**
  - **Đè & Kéo nút Pháp Bảo:** Quan sát **Chỉ Dấu Thanh Chắn Ngang (`VectorWall`)** hiển thị vuông góc với hướng ngắm.
  - **Nhả tay:** Dựng ngay một **Bức Tường Nước Thánh gồm 4 giếng thiêng** dàn hàng ngang phía trước:
    - Quái vật đi qua bị **làm chậm 50%**.
    - Tướng được **hồi ngay 10% Max HP**.

---

### 3. Nồi Cơm Thạch Sanh (`W_POT` - Hệ Thổ) — *Cắm Nồi Gom Quái Từ Xa*
* **Thao tác ngắm bắn:**
  - **Đè & Kéo nút Pháp Bảo:** Quan sát **Chỉ Dấu Tâm Tròn (`CircleReticle`)** di chuyển linh hoạt từ xa (phạm vi 7 mét).
  - **Nhả tay:** Cắm ngay Nồi Cơm Thạch Sanh tại vị trí chỉ định. Nồi mở nắp hút toàn bộ quái trong bán kính 6m vào tâm trong 2s, sau đó phát nổ hất văng quái 18m/s và rơi ra 3 viên **Cơm Nắm hồi máu (5% HP/viên)**.

---

### 4. Trống Đồng Đông Sơn (`W005` - Hệ Thổ) — *Sóng Âm Nhịp Điệu*
* **Thao tác ngắm bắn:**
  - **Đè nút Pháp Bảo:** Quan sát **Chỉ Dấu Vòng Tròn Nhịp Điệu (`RhythmPulse`)** co giãn nhịp nhàng dưới chân.
  - **Nhả tay:** Dậm 3 đợt sóng âm thần uy 360 độ làm **choáng cứng quái 1.5s** và đẩy lùi toàn bộ quái vật đang vây quanh.

---

### 5. Nỏ Thần (`W001` - Hệ Kim) — *Bão Tiễn Thần Uy Xuyên Thấu Vô Tận*
* **Thao tác ngắm bắn:**
  - **Đè & Kéo nút Pháp Bảo:** Quan sát **Chỉ Dấu Mũi Tên Dài (`LineArrow`)** vươn xa 14 mét.
  - **Nhả tay:** Khai hỏa liên tiếp **3 đợt bão Linh Tiễn Thần Uy**, xuyên thấu 100% mục tiêu trên đường bay (Infinite Pierce) và đẩy lùi bầy quái 8 mét dọn sạch một đường thẳng.

---

### 6. Lựu Đạn Thần Sa (`W006` - Hệ Hỏa) — *Cụm Bom Rải Thảm*
* **Thao tác ngắm bắn:**
  - **Đè & Kéo nút Pháp Bảo:** Chỉ dấu `CircleReticle` vùng nổ rộng 8.5 mét.
  - **Nhả tay:** Quăng chùm 3 quả Lựu Đạn Thần Sa nổ liên hoàn tạo bão lửa thiêu rụi toàn bộ quái vật trong khu vực chỉ định.

---

### 7. Đao Cửu Vĩ (`W008` - Hệ Hỏa) — *Hỏa Long Bộc Phát*
* **Thao tác ngắm bắn:**
  - **Đè & Kéo nút Pháp Bảo:** Chỉ dấu hình quạt góc rộng 120 độ (`ConeSector`).
  - **Nhả tay:** Kích hoạt trạng thái thần uy trong 5s: Tăng 35% tốc độ chém, liên tục phóng ra các vệt trảm **Hỏa Long 8 hướng** thiêu rụi quái vật xung quanh.

---

### 8. Điếu Cày Cửu U (`W_PIPE` - Hệ Hỏa) — *Bão Khói Tương Tác Địa Hình*
* **Thao tác ngắm bắn:**
  - **Đè & Kéo nút Pháp Bảo:** Chỉ dấu `VectorWall` dựng tường khói dài 6 mét.
  - **Nhả tay:** Rít hơi dài nhả bức tường bão khói thuốc lào rồng cuộn. Quái bước vào khói bị đi giật lùi, say thuốc và liên tục chịu sát thương thiêu đốt.

---

## 🛡️ PHẦN 2: TEST NHÓM PHÁP BẢO BỊ ĐỘNG (PASSIVE RELICS)

*(Khi trang bị nhóm này: Nút Kỹ Năng trên HUD tự động ẩn để giữ màn hình gọn gàng)*

| Pháp Bảo | Cách Kiểm Thử | Hiện Tượng Cần Quan Sát |
| :--- | :--- | :--- |
| **Chiếu Trải Hoàng Tuyền (`R007`)** | Đứng yên hoặc di chuyển, đợi 8s | Chiếu tự trải dưới đất. Quái bước vào mép chiếu lăn ra ngủ say 3s (x2 Crit). Khi bạn điều khiển Tướng bước/Dash lên chiếu $\rightarrow$ **Kích hoạt trượt ván siêu tốc**, ủi bay quái như chơi bowling! |
| **Chổi Lông Gà Gia Truyền (`R008`)** | Dùng nút Đánh thường chém Combo 1 ➔ 2 ➔ 3 | Ngay khi kết thúc Nhát chém số 3, một cây **Chổi Lông Gà khổng lồ** từ trên trời giáng xuống dập tắt quái, đẩy lùi 12m/s và gây Choáng 0.8s. |
| **Cửu Vĩ Hồ Trảo (`W004`)** | Cho quái đánh tụt máu xuống **dưới 35% HP** | Đàn dơi hồ ly tự động **tăng gấp đôi số lượng** và tăng **gấp 2.5 lần hiệu suất Hút Máu (Lifesteal)** giúp Tướng lội ngược dòng sinh tử! |
| **Trượng Long Vương (`W009`)** | Đứng trước một đám đông quái vật | Cầu sét nước tự động bắn ra và **nảy chuỗi liên hoàn qua 6 mục tiêu**, giật điện gây choáng 0.5s. |
| **Bút Phán Quan (`W002`)** | Chém trúng quái vật | Tự động xuất hiện vệt mực Chu Sa phán quyết bồi thêm sát thương xuyên giáp. |

---

## 🧹 PHẦN 3: TEST CLEANUP KHI THOÁT TRẬN (ZERO-LEAK)
1. Trong lúc trận đấu đang diễn ra ác liệt (ném Dép, dậm Trống, bắn Nỏ, rải bão khói).
2. Nhấn nút **Pause ➔ Thoát Trận (Quit to Menu)** để trở về **Sảnh Chờ (Meta Hub)**.
3. **Tiêu chí đạt chuẩn:**
   - Mọi thực thể đạn bay, quả cầu xoay, đốm lửa và hiệu ứng hạt VFX **biến mất 100% tức thì**.
   - Không còn bất kỳ vòng Decal hay icon kỹ năng nào bị đè lên giao diện Sảnh Chờ.
   - Nhấn **Vào Trận Mới**: Trận đấu mới bắt đầu sạch sẽ, mượt mà ở 60 FPS.
