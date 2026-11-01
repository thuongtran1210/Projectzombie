# 🎮 HƯỚNG DẪN TEST & KIỂM THỬ THỦ THÀNH 3D (TESTING_GUIDE.md)

Tài liệu này hướng dẫn chi tiết cách thức **trải nghiệm, kiểm thử chức năng và đánh giá hiệu năng** của chế độ chơi **Thủ Thành 3D (Defense3D — Map Sông Quê)** trên Unity Editor và thiết bị Android thật.

---

## 🎯 CÁC BƯỚC KHỞI ĐỘNG TRẬN ĐẤU (GETTING STARTED)

### Cách 1: Khởi Động Chuẩn Theo Luồng Ứng Dụng (Khuyên Dùng)
1. Trong Unity Editor, mở Scene: `Assets/Scenes/Bootstrap.unity`.
2. Nhấn nút **Play** (`Ctrl + P`).
3. Hệ thống sẽ tự động khởi tạo các dịch vụ nền và chuyển tới **Sảnh Chính (`MainMenu.unity`)**.
4. Chọn **Tàng Bảo Các (Card Codex)** để xem danh mục thẻ và trang bị Đội Hình (Tướng, Pháp Bảo, Lính, Tháp).
5. Quay lại Sảnh chính, chọn Ải (Stage) và nhấn **Xuất Trận**. Hệ thống chuyển cảnh mượt mà vào `MapSongQue3D_Prototype.unity`.

### Cách 2: Chạy Thử Trực Tiếp Scene Trận Đấu (Quick Dev Testing)
1. Mở trực tiếp Scene: `Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity`.
2. Nhấn nút **Play**.
3. `DefenseSceneCompositionRoot` sẽ tự động phát hiện chế độ Direct Play và nạp cấu hình mặc định để bạn test ngay lập tức các cơ chế di chuyển, tháp và đợt quái.

---

## ⚔️ PHẦN 1: KIỂM THỬ ĐIỀU KHIỂN & KỸ NĂNG TƯỚNG (HERO COMBAT)

### 1. Di Chuyển & Đánh Thường (Movement & Basic Attack)
- **Bàn phím:** Sử dụng các phím `W`, `A`, `S`, `D` để di chuyển Tướng. Nhấn hoặc giữ phím `J` (hoặc click nút Đánh thường trên HUD) để vung đòn.
- **Chỉ dấu dưới chân (`HeroFootIndicator`):** Quan sát vòng tròn và mũi tên chỉ hướng luôn bám sát mặt đất theo chân Tướng.
- **Vùng đòn đánh (`AttackSector`):** Khi giữ nút đánh thường, hình quạt 210° (bán kính 2.05m) xuất hiện, tự động xoay hướng bắt mục tiêu quái gần nhất (Auto-Aim). Khi nhả tay, Tướng tung nhát vung quét trúng toàn bộ quái trong quạt.

### 2. Kỹ Năng Lướt (Dash)
- **Thao tác:** Nhấn phím `Space` hoặc nút Lướt trên màn hình.
- **Hiện tượng cần kiểm tra:**
  - Tướng lướt nhanh về phía trước một khoảng 4.5m trong 0.25s.
  - Vệt bóng mờ (Ghost Trail) xuất hiện phía sau.
  - Thao tác lướt có khả năng ngắt ngay lập tức các hành động đánh thường hoặc tuyệt kỹ đang chờ xuất chiêu.

### 3. Tuyệt Kỹ Tướng (Signature Skill)
- **Thư Sinh (`Skill_ScholarSignature`):**
  - Nhấn phím `K` hoặc nút Tuyệt Kỹ: Xuất hiện vòng chỉ dấu Bút Mực Chu Sa bán kính 6m dưới chân.
  - Phát nổ sát thương diện rộng sau 0.38s, đồng thời giảm ngay **20% thời gian hồi chiêu còn lại** của Pháp Bảo.
- **Đạo Sĩ (`Skill_DaoSi_Signature`):**
  - Nhấn nút Tuyệt Kỹ: Gọi thiên lôi giáng tia sét sấm sét bộc phá diện rộng 4.5m, gây sát thương cực lớn lên bầy quái.

### 4. Kỹ Năng Pháp Bảo (Relic Active Skill)
- **Dép Tổ Ong Thần Sa (`Skill_DepToOng`):**
  - Nhấn phím `Q` hoặc nút Pháp Bảo: Ném chiếc Dép Tổ Ong bay thẳng gom quái và phát nổ lốc xoáy.
  - Nút chiêu tự động phát xung viền sáng kích hoạt Cước Song Phi lướt theo dẫm nổ Shockwave đẩy lùi bầy quái.
- **Điếu Cày Khói Lửa (`Skill_DieuCay`):**
  - Nhấn nút Pháp Bảo: Rít hơi thuốc lào phun ra bức tường bão khói rồng cuộn rộng 4m.
  - Quái bước vào vùng khói chịu sát thương thiêu đốt liên tục (DoT) và bị giảm tốc chạy rõ rệt.

---

## 🛡️ PHẦN 2: KIỂM THỬ KINH TẾ GỖ & THÁP PHÒNG THỦ (TOWERS & WOOD)

### 1. Thu Thập Cổ Mộc (Wood Drops)
1. Tiêu diệt quái vật Ma Giáp hoặc Thủy Quái.
2. Quan sát các khối Gỗ Cổ Mộc 3D rơi ra tại vị trí quái chết.
3. Điều khiển Tướng lại gần trong phạm vi hút gỗ: Khối gỗ tự động bay về phía Tướng, số lượng gỗ trên thanh HUD tăng lên tương ứng.

### 2. Xây Dựng & Nâng Cấp Tháp Tại 10 Điểm Cắm (Sockets)
1. Điều khiển Tướng di chuyển tới 1 trong 10 cọc thổ địa cắm tháp trên bản đồ.
2. Khi bước vào bán kính tương tác (`1.7m`), nút **Xây Tháp (Build Button)** trên giao diện sẽ sáng lên.
3. Bấm nút để mở Menu Xây Dựng:
   - **Tháp Nỏ Thần:** Tiêu tốn 25 Gỗ. Bắn tỉa mũi tên tầm xa 8m, tốc bắn nhanh.
   - **Tháp Băng Hà:** Tiêu tốn 30 Gỗ. Phóng tinh thể băng làm chậm 40% quái vật diện rộng.
   - **Tháp Hỏa Long:** Tiêu tốn 35 Gỗ. Khai hỏa đạn pháo lửa nổ lan diện rộng và thiêu đốt.
4. Khi đã xây xong, tiếp tục đứng gần tháp để nâng cấp lên Cấp 2 và Cấp 3 nhằm gia tăng tầm bắn và uy lực.

---

## 👨‍🌾 PHẦN 3: KIỂM THỬ LỀU DÂN LÀNG & CHIÊU MỘ LÍNH (MINIONS)

1. Di chuyển Tướng về khu vực Lều Dân Làng ở hậu phương Bờ Tây.
2. Bước vào vòng tương tác của lều (`1.6m`): Menu tuyển mộ lính kích hoạt.
3. Kiểm tra chiêu mộ các vai trò lính theo đội hình đã trang bị:
   - **Dân Binh:** Lính cận chiến cầm giáo cảm tử, xông ra chặn đầu các đợt quái qua cầu.
   - **Xạ Thủ Nỏ Thần:** Lính tầm xa đứng sau xả linh tiễn yểm trợ.
   - **Phu Tạp Dịch:** Tự động tỏa ra các cây cầu gom nhặt các khối gỗ rơi rớt mang về kho.
   - **Thầy Lang:** Đứng gần Căn Cứ và Tướng để truyền máu hồi phục sinh lực.

---

## 👹 PHẦN 4: KIỂM THỬ ĐỢT QUÁI & BOSS DẠ XOA

1. **Đợt Quái Bộ (Cầu Bắc, Cầu Giữa, Cầu Nam):** Ma Giáp và Quái Dữ tiến quân theo các nhánh đường dẫn tới 3 cây cầu.
2. **Đợt Quái Thủy:** Thủy Quái bơi xuyên qua lòng kênh sông, vượt chướng ngại vật đánh tạt sườn.
3. **Đợt Quái Bay:** Huyết Dơi và Quỷ Phi Cương bay thẳng qua vách đá. Quỷ Phi Cương hiển thị hình quạt báo đòn đỏ cam (Telegraph 0.65s) trước khi tung trảo cào.
4. **Boss Dạ Xoa (Phút 8:30 — 510s):**
   - Boss xuất hiện tại khu vực Bến Đò.
   - Tướng bước vào phạm vi sẽ kích hoạt Vòng Đấu Trường Arena (45s).
   - Boss tung đòn dậm đất nổ vòng lửa (Bán kính 5m). Sau giai đoạn Arena, Boss sẽ chuyển sang lộ trình tiến công càn quét Căn Cứ Bờ Tây.

---

## 🧪 PHẦN 5: CHẠY BỘ KIỂM THỬ TỰ ĐỘNG HÓA (AUTOMATED SUITES)

Trước khi đóng gói hoặc commit tính năng mới, hãy chạy bộ test tự động qua PowerShell:

```powershell
# 1. Kiểm tra kiến trúc tổng thể ứng dụng (115 tests NUnit)
powershell -ExecutionPolicy Bypass -File Tools/Invoke-UnityBridge.ps1 -Command validate-app-architecture -TimeoutSeconds 30

# 2. Kiểm tra danh mục và logic vận hành Tháp (51 tests NUnit)
powershell -ExecutionPolicy Bypass -File Tools/Invoke-UnityBridge.ps1 -Command validate-towers -TimeoutSeconds 30

# 3. Kiểm tra logic Thẻ Loadout & Cấp Sao Meta (103 tests NUnit)
powershell -ExecutionPolicy Bypass -File Tools/Invoke-UnityBridge.ps1 -Command validate-card-progression -TimeoutSeconds 30
```
> **Tiêu chuẩn đạt:** Tất cả các lệnh trên phải trả về `Result: PASS` và không có bất kỳ compile error nào.
