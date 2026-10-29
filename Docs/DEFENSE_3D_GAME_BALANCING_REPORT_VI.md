# Báo Cáo Thẩm Định & Cân Bằng Gameplay — Thủ Thành 3D Sông Quê (Projectzombie)

> **Phiên bản:** `stage01-production-v1` | **Môi trường:** `MapSongQue3D_Prototype.unity` | **Thời lượng trận:** `600 giây` (10 phút)

---

## 1. Tổng Quan Nhịp Độ Trận Đấu 600s (Session Pacing & Wave Phases)

Trận chiến phòng thủ Căn Cứ Bờ Tây được chia thành **4 Giai Đoạn Chiến Thuật**:

| Giai Đoạn | Khoảng Thời Gian | Đặc Trưng Quái Vật | Mục Tiêu Người Chơi | Lượng Gỗ Tích Lũy |
|---|---|---|---|---|
| **Phase 1: Khởi Động** | `0s – 120s` (2 phút đầu) | Ma Giáp, Quái Dù, Thủy Quái (nhóm nhỏ 4-5 con) | Tự thủ bằng Tướng, nhặt gỗ cắm 3-4 Tháp Nỏ Thần/Hàn Băng | `~750 Gỗ` (kèm 100 gỗ khởi điểm) |
| **Phase 2: Mở Rộng** | `120s – 300s` (Phút 2 – 5) | Đợt quái dồn dập từ cả 3 Lane (Đông, Nam, Kênh nước), xuất hiện Ma Giáp Thống Lĩnh | Phủ kín 8-10 Socket tháp, bắt đầu nâng cấp Tháp lên Level 2 | `~1.800 Gỗ` |
| **Phase 3: Áp Lực Cao** | `300s – 510s` (Phút 5 – 8.5) | Mật độ quái tối đa (7-8 quái/đợt), kết hợp quái bay và thủy quái vượt kênh | Nâng cấp toàn bộ Tháp lên Level 3, tích luỹ EXP nâng cấp Tướng | `~3.200 Gỗ` |
| **Phase 4: Đại Chiến Boss** | `510s – 600s` (Phút 8.5 – 10) | **Boss Dạ Xoa (3200 HP)** giáng lâm + Đợt quái tổng lực cuối cùng | Dồn toàn bộ hỏa lực Tướng & Tháp chặn Dạ Xoa phá vỡ Căn Cứ | `~4.000 Gỗ` |

---

## 2. Bảng Phân Tích Chỉ Số 7 Loại Kẻ Địch (Enemy Matrix)

| Kẻ Địch | Loại Di Chuyển | Máu (HP) | Tốc Độ | Sát Thương | DPS Lên Base | Gỗ Rơi | EXP | Vai Trò & Điểm Yếu |
|---|---|---|---|---|---|---|---|---|
| **Ma Giáp** | Bộ (Đường bờ) | `130` | `1.45 m/s` | `16` | `8.0 DPS` | `10` | `5` | Lực lượng chủ lực, dễ bị Tháp Nỏ Thần dọn dẹp |
| **Quái Dù** | Vượt cản / Nhảy | `90` | `1.75 m/s` | `14` | `7.0 DPS` | `15` | `8` | Tấn công bất ngờ từ hướng Bắc (North Canopy) |
| **Thủy Quái** | Bơi dưới kênh | `95` | `2.05 m/s` | `12` | `6.0 DPS` | `12` | `6` | Bơi thẳng qua kênh nước, bỏ qua cầu bộ hành |
| **Huyết Dơi** | Bay trên không | `50` | `3.10 m/s` | `8` | `4.0 DPS` | `8` | `4` | Tốc độ cực nhanh, cần Tháp Băng làm chậm |
| **Phi Cương** | Cương thi bay | `260` | `1.65 m/s` | `18` | `9.0 DPS` | `25` | `15` | Quái trâu trên không, áp sát nhanh |
| **Ma Giáp Thống Lĩnh** | Quái Đầu Đàn (Elite) | `700` | `1.18 m/s` | `24` | `12.0 DPS` | `80` | `50` | Máu dày, rớt lượng gỗ và EXP đột biến |
| **Dạ Xoa (Boss)** | Đại Boss Phút 8:30 | `3200` | `1.35 m/s` | `38` | `19.0 DPS` | `300` | `200` | Chiêu thức càn quét, kiểm tra ngưỡng sức mạnh cuối |

---

## 3. Bảng Cân Bằng Tháp Canh (Tower Progression Matrix)

Hệ thống có **10 Điểm Cắm Tháp (Sockets)** cố định trên bản đồ với 2 nhánh tháp bổ trợ tương hỗ:

### 🏹 Tháp Nỏ Thần (Nỏ Thần Bắn Tỉa — Sát Thương Đơn Mục Tiêu)
* **Cơ chế:** Bắn tên theo đường cong parabol, sát thương vật lý dồn mục tiêu quái đi đầu.
* **Level 1:** Chi phí `25 Gỗ` | Sát thương `22` | Tốc độ bắn `1.1 phát/s` ➔ **DPS: 24.2** | Tầm: `8.0m`
* **Level 2:** Chi phí `40 Gỗ` | Sát thương `35` | Tốc độ bắn `1.4 phát/s` ➔ **DPS: 49.0** | Tầm: `9.5m`
* **Level 3:** Chi phí `60 Gỗ` | Sát thương `55` | Tốc độ bắn `1.8 phát/s` ➔ **DPS: 99.0** | Tầm: `11.0m`
> *Hiệu quả:* 1 Tháp Nỏ Thần Level 3 tiêu diệt 1 Ma Giáp (130 HP) chỉ trong **1.3 giây**!

### ❄️ Tháp Hàn Băng (Hàn Băng Tháp — Khống Chế & Làm Chậm Diện Rộng)
* **Cơ chế:** Bắn tinh thể băng, áp hiệu ứng `Slow 40%` trong 3 giây.
* **Level 1:** Chi phí `30 Gỗ` | Sát thương `15` | Tốc độ bắn `0.9 phát/s` ➔ **DPS: 13.5** | Tầm: `7.5m`
* **Level 2:** Chi phí `50 Gỗ` | Sát thương `25` | Tốc độ bắn `1.1 phát/s` ➔ **DPS: 27.5** | Tầm: `8.5m`
* **Level 3:** Chi phí `75 Gỗ` | Sát thương `40` | Tốc độ bắn `1.3 phát/s` ➔ **DPS: 52.0** | Tầm: `10.0m`
> *Hiệu quả:* Làm chậm bước chân kẻ địch trên 3 cây cầu từ 15s lên 25s, tăng gấp đôi thời gian cho Tháp Nỏ Thần và Tướng dồn đòn.

---

## 4. Phân Tích Cân Bằng Kinh Tế Gỗ (Wood Economy Analysis)

1. **Ngân sách Khởi điểm:** `100 Gỗ` — Đủ xây ngay 3 Tháp Nỏ Thần (`25x3 = 75 Gỗ`) hoặc 2 Nỏ Thần + 1 Hàn Băng (`25x2 + 30 = 80 Gỗ`) ngay tại giây thứ 0.
2. **Chi phí Phủ 10 Tháp Cấp 1:**
   - 6 Tháp Nỏ Thần (`150 Gỗ`) + 4 Tháp Hàn Băng (`120 Gỗ`) = **`270 Gỗ`**.
   - Người chơi hoàn thành mốc này dễ dàng ở **khoảng giây 90 – 120** (cuối Phase 1).
3. **Chi phí Tối Đa Hóa (Full Max Level 3 cho cả 10 Tháp):**
   - 6 Tháp Nỏ Thần Lv.3: `6 x (25 + 40 + 60) = 750 Gỗ`.
   - 4 Tháp Hàn Băng Lv.3: `4 x (30 + 50 + 75) = 620 Gỗ`.
   - **Tổng chi phí Full Tháp:** **`1.370 Gỗ`**.
4. **Tổng Gỗ Rơi Từ 34 Wave:** **`~4.000 Gỗ`**.
   - Thừa đủ ngân sách để người chơi Full Max toàn bộ 10 Tháp trước khi Boss Dạ Xoa xuất hiện (510s), phần gỗ dôi dư dùng để nâng cấp các tính năng hỗ trợ hoặc dự phòng.

---

## 5. Đánh Giá Độ Bền Căn Cứ Bờ Tây (Base Survivability)

* **Máu Căn Cứ:** `1000 HP`.
* **Cơ Chế Thua Trận:** Trận đấu chỉ thất bại khi Căn Cứ bị đánh sập (`HP = 0`). Tướng bị hạ gục sẽ tự động hồi sinh sau `10 giây`.
* **Thời gian phá nhà khi bị quái tràn qua:**
  - Nếu lọt **1 con Ma Giáp lẻ loi:** Cần tới **125 giây** liên tục mới phá được Base (không gây nguy hiểm đột ngột).
  - Nếu lọt **1 đợt 8 con Ma Giáp cùng lúc:** Tổng DPS là `64 DPS` ➔ Phá sập Base trong **15.6 giây**!
  - ➔ *Kết luận:* Ngưỡng cân bằng này cực kỳ kịch tính, buộc người chơi phải ưu tiên tiêu diệt các cụm quái số đông hoặc dùng Tháp Băng khống chế phân tán đội hình quái.

---

## 6. Khuyến Nghị Cho Game Designer (Tuning Guide)

1. **Nếu muốn tăng độ thử thách (Hard Mode):**
   - Tăng số lượng quái ở các wave giữa trận (benque-15 đến benque-25) từ 6 lên 8 con.
   - Thêm 1 quái Elite `magiap_boss` vào Wave 20 (342s) để tạo mốc thử thách giữa trận.
2. **Nếu muốn trải nghiệm thư giãn hơn (Casual Mode):**
   - Tăng bán kính nam châm hút gỗ trong `DefenseWoodDrop3D` từ `3.8m` lên `4.5m`.
   - Giảm chi phí nâng cấp Tháp Nỏ Thần Lv.2 từ 40 xuống 30 gỗ.
