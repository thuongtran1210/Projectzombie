# Điểm Bắt Đầu — Tiến Độ Chuyển Gameplay Sang Defense3D

**Thời điểm cập nhật:** 30/10/2026  
**Chủ đề đã hoàn thành:**
- **G0 — Kiểm kê & Cô lập phụ thuộc:** Hoàn thành 100%.
- **G1 — Lõi Loadout thuần C# & Unit Tests:** Hoàn thành 100%.
- **G2 — Catalog Defense3D & Adapter Runtime:** Hoàn thành 100%.
- **G3 — Save Migration & Meta Adapter:** Hoàn thành 100%.
- **G4 — Luồng Sảnh & UI Chuẩn Bị Trận (AppFlow):** Hoàn thành 100%.
- **G5 — Kết quả, trở về sảnh & hoàn thiện vòng đời:** Hoàn thành 100%.
- **Sửa lỗi nền tảng:**
  - Thống nhất Font Asset TMP sang `GameFont_Vietnamese_SD` (triệt tiêu `TMP_SubMeshUI` NullReferenceException).
  - Mở rộng Menu Xây Tháp trong `MapSongQue3D_Prototype.unity` lên 3 hàng UI (triệt tiêu lỗi crash `Build menu has fewer rows than tower catalog entries`).
  - Mở rộng Catalog nội dung mới (Đạo Sĩ, Điếu Cày Khói Lửa, Xạ Thủ Nỏ Thần, Tháp Hỏa Long).

**Chủ đề đang tiến hành:** **G6 — Mở Rộng Nội Dung & Nạp Động Scene Trận Đấu (Dynamic Scene Binding)**.

---

## 📖 Đọc Theo Thứ Tự Ở Phiên Tiếp Theo

1. `.agents/AGENTS.md`: Quy tắc chuẩn mực dự án (MVP, TextMeshPro, quy chuẩn 60 FPS mobile).
2. `README.md`: Toàn cảnh kiến trúc, vòng lặp trận đấu và hệ thống gameplay Defense3D.
3. `TESTING_GUIDE.md`: Hướng dẫn kiểm thử chi tiết trong Unity Editor và tự động qua PowerShell.
4. `TASK.md`: Bảng theo dõi tiến độ các hạng mục công việc.
5. `Docs/DEFENSE_3D_ARCHITECTURE_GUIDE_VI.md`: Sổ tay 5 trụ cột kiến trúc trong `MapSongQue3D_Prototype.unity`.

---

## 📌 Quyết Định Đang Có Hiệu Lực (Active Decisions)

- **Defense3D là gameplay chính thức và duy nhất của dự án:** Tái sử dụng UI Vong Xuyên (MVP), hệ thống Lưu trữ (SaveSystem), và dịch vụ nền tảng.
- **Cấu trúc Đội Hình (Loadout):**
  - 1 Tướng (`hero_thusinh`, `hero_daosi`, `hero_thanhdong`, `hero_ansi`).
  - 1 Pháp Bảo (`relic_deptoong`, `relic_dieucay`).
  - 3 Lính Làng (`minion_danbinh`, `minion_xathu`, `minion_phutapdich`, `minion_thaylang`).
  - 3 Tháp Phòng Thủ (`tower_cungtien`, `tower_bangha`, `tower_hoalong`).
- **Nguồn Lưu Trữ Chính:** Lưu trữ tại `defense_loadout.json` với cơ chế ghi Atomic an toàn (.tmp -> .bak -> replace) và kiểm soát Versioning.
- **Bộ Kiểm Thử Tự Động Hóa:** Vận hành qua script `Tools/Invoke-UnityBridge.ps1` (`validate-app-architecture`, `validate-towers`, `validate-card-progression`). Toàn bộ 269 unit tests hiện tại đều đạt trạng thái **PASS**.

---

## 📊 Bảng Trạng Thái Chi Tiết Các Cột Mốc

| Cột Mốc | Trạng Thái | Số Lượng Test NUnit | Ghi Chú Kỹ Thuật |
| :--- | :---: | :---: | :--- |
| **G0 — Kiểm Kê** | Đã xong | — | Cô lập `DontDestroyOnLoad` cho `--- GAME MANAGER ---`. |
| **G1 — Lõi Loadout** | Đã xong | 13 Tests PASS | Triển khai các lớp thuần C# `LoadoutRules`, `LoadoutSelection`, `LoadoutValidator`. |
| **G2 — Catalog & Adapter** | Đã xong | 30 Checks PASS | Triển khai `Defense3DCatalog`, `DefenseLoadoutResolver`, `DefenseLoadoutRuntimeAdapter`. |
| **G3 — Save & Meta** | Đã xong | 11 Tests PASS | `DefenseLoadoutRepository`, `DefenseMetaOwnershipAdapter`, `DefenseLoadoutIdMapper`. |
| **G4 — Luồng Sảnh & UI** | Đã xong | 7 Tests PASS | `DefensePreparationPresenter`, `DefenseMatchNavigation`, nạp Scene qua `AppFlowController`. |
| **G5 — Vòng Đời Trận Đấu** | Đã xong | 7 Tests PASS | `DefenseMatchResultService`, `DefenseNavigationService`, dọn sạch bộ nhớ khi về Sảnh. |
| **G6 — Dynamic Binding** | Đang làm | Đang cập nhật | Nạp động Pháp bảo Điếu Cày, Tuyệt kỹ Đạo Sĩ, Lính Xạ Thủ trong Scene 3D. |
