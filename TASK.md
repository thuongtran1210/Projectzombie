# 📋 LỘ TRÌNH THỰC THI & QUẢN LÝ NHIỆM VỤ (TASK.md)

Tài liệu này là **Single Source of Truth** theo dõi tiến độ các hạng mục công việc của dự án **Vong Xuyên — 3D Top-Down Survival & Tower Defense (Defense3D)**.

---

## 🚦 TỔNG QUAN TIẾN ĐỘ (ROADMAP STATUS)

| Giai Đoạn (Milestone) | Nội Dung Trọng Tâm | Trạng Thái |
| :--- | :--- | :---: |
| **G0 — Kiểm Kê & Rà Soát** | Phân tích Scene, cô lập DontDestroyOnLoad, kiểm kê hệ thống | **HOÀN THÀNH (100%)** |
| **G1 — Lõi Loadout Thuần C#** | Rules, Selection, Validator, Snapshot, Repository | **HOÀN THÀNH (100%)** |
| **G2 — Catalog & Runtime Adapter** | Defense3DCatalog, Resolver, Shop/HeroKit Adapters | **HOÀN THÀNH (100%)** |
| **G3 — Save Migration & Meta Adapter** | Mapping ID cũ/mới, Cấp quyền Meta, Atomic JSON Save | **HOÀN THÀNH (100%)** |
| **G4 — Luồng Sảnh & Chuẩn Bị Trận** | UI Chuẩn bị trận, Navigation nạp Scene qua AppFlow | **HOÀN THÀNH (100%)** |
| **G5 — Hoàn Thiện Vòng Đời & Kết Quả** | Dịch vụ kết quả, cộng Cổ Tiền, trở về Sảnh an toàn | **HOÀN THÀNH (100%)** |
| **G6 — Mở Rộng Nội Dung & Nạp Động** | Đạo Sĩ, Điếu Cày, Xạ Thủ, Hỏa Long, Dynamic Binding | **HOÀN THÀNH** |
| **G7 — Đánh Bóng & Đóng Gói Android** | Tối ưu 60 FPS, Audio Cổ Phong, Build APK/AAB | **KẾ HOẠCH** |

---

## 📌 CHI TIẾT CÁC HẠNG MỤC CÔNG VIỆC

### ✅ Giai Đoạn Đã Hoàn Thành (Done)

- [x] **[G0-01] Cô Lập DontDestroyOnLoad:**
  - Giải phóng 15 GameObject con của `--- GAME MANAGER ---` trong `GameStateManager.cs` để Unity dọn sạch bộ nhớ khi chuyển cảnh.
- [x] **[G1-01] Lõi Loadout Thuần C#:**
  - Xây dựng `LoadoutRules`, `LoadoutSelection`, `LoadoutValidator`, `DefenseLoadoutSnapshot`.
  - Bộ kiểm thử 13 NUnit tests toàn diện trong `Assets/Tests/Editor/DefenseLoadoutCoreTests.cs`.
- [x] **[G2-01] Danh Mục Catalog & Adapter Runtime:**
  - Xây dựng `Defense3DCatalog` và bộ phân giải `DefenseLoadoutResolver`.
  - Kết nối `DefenseLoadoutRuntimeAdapter` áp dụng dữ liệu đội hình vào `DefenseVillageShop3D` và `DefenseHeroKitSO`.
  - Tách Relic khỏi ràng buộc cứng trong `DefenseHeroKitSO`.
- [x] **[G3-01] Hệ Thống Lưu Trữ & Bộ Chuyển Đổi Meta:**
  - Xây dựng `DefenseLoadoutRepository` lưu file `defense_loadout.json` với cơ chế ghi nguyên tử (Atomic Write `.tmp` -> `.bak`).
  - Xây dựng `DefenseMetaOwnershipAdapter` và `DefenseLoadoutIdMapper` đồng bộ với tiến trình Cấp Sao Tướng/Pháp bảo.
- [x] **[G4-01] Luồng Sảnh & Giao Diện Chuẩn Bị Xuất Trận:**
  - Triển khai `DefensePreparationPresenter` kiểm tra đội hình realtime và điều khiển nút Xuất Trận.
  - Tích hợp `DefenseMatchNavigation` nạp Scene `MapSongQue3D_Prototype` thông qua `AppFlowController`.
- [x] **[G5-01] Kết Quả Trận Đấu & Trở Về Sảnh:**
  - Triển khai `DefenseMatchResultService` tính toán thưởng Cổ Tiền theo số Gỗ thu thập và kỷ lục hoàn thành.
  - Tích hợp `DefenseNavigationService` dọn dẹp bộ nhớ (Clean Pools, Dispose Session) và chuyển cảnh về Sảnh `MainMenu`.
- [x] **[G5-02] Thống Nhất Font Asset TextMesh Pro:**
  - Cấu hình font mặc định `GameFont_Vietnamese_SD` trong `TMP Settings.asset`, khắc phục hoàn toàn lỗi rác bộ nhớ `TMP_SubMeshUI.UpdateMaterial` NullReferenceException.
- [x] **[G5-03] Mở Rộng Menu Xây Tháp 3 Hàng:**
  - Cập nhật `DefenseTowerBuildPresenter` và giao diện Menu Xây Tháp trong `MapSongQue3D_Prototype.unity` lên 3 hàng UI, sửa triệt để lỗi crash `Build menu has fewer rows than tower catalog entries`.

---

### 🟡 Giai Đoạn Đang Triển Khai (In Progress - G6)

- [x] **[G6-01] Mở Rộng Danh Mục Nội Dung Mới:**
  - Thêm Tướng Đạo Sĩ (`hero_daosi`), Pháp Bảo Điếu Cày Khói Lửa (`relic_dieucay`), Lính Xạ Thủ Nỏ Thần (`minion_xathu`), Tháp Hỏa Long (`tower_hoalong`) vào `Defense3DCatalog.CreateExpanded()`.
  - Cấp quyền sở hữu cơ bản trong `DefenseMetaOwnershipAdapter`.
  - Ánh xạ ID đầy đủ trong `DefenseLoadoutIdMapper`.
- [/] **[G6-02] Nạp Động Trong Scene Trận Đấu (Dynamic Scene Binding):**
  - [x] **Hoán đổi Pháp bảo:** Bổ sung phương thức cấu hình động trong `DefenseSkillPresenter3D` để nạp `Skill_DieuCay` và gắn component `DefensePipeRelicEffect3D` với cơ chế Vùng Khói Thiêu Đốt DoT & Slow.
  - [x] **Hoán đổi Tuyệt Kỹ Tướng:** Nối slot Signature với `Skill_DaoSi_Signature` và component `DefenseLightningSignatureEffect3D` khi chọn Đạo Sĩ (hoàn thiện sát thương scale, Stun 1.5s, Slow 40% và Feedback events).
  - [x] **Hoán đổi Lính chiêu mộ:** Cho phép `DefenseVillageShop3D` hoán đổi Definition ô tuyển mộ lính khi người chơi mang Xạ Thủ Nỏ Thần (`Minion_XaThu.asset`, cự ly 8.5m, 40 gỗ, đòn nặng 30 damage kèm Stun và cơ chế phóng tên).
- [x] **[G6-03] Hoán Đổi Mô Hình 3D Nhân Vật (Hero Visual Model Swapping):**
  - Cập nhật `DefenseHeroVisualController3D` cho phép nạp và hiển thị Model 3D tương ứng của Tướng đã chọn thay vì cố định Thư Sinh.

---

### 🔴 Giai Đoạn Kế Hoạch (Backlog - G7: Polish & Android Release)

- [ ] **[G7-01] Tối Ưu Hóa Draw Call & Sprite Atlas:**
  - Đóng gói toàn bộ icon thẻ và UI Cổ Phong vào Sprite Atlas, kiểm soát Draw Call < 40 trên thiết bị di động.
- [ ] **[G7-02] Tinh Chỉnh & Cân Bằng Gameplay Trận Đấu:**
  - Cân đối máu quái, lượng gỗ rơi từ bầy quái Ma Giáp / Thủy Quái và chi phí nâng cấp Tháp theo thời gian 600s.
- [ ] **[G7-03] Kiểm Thử Stress Test & Đóng Gói Android Release:**
  - Stress test 150+ kẻ địch và 80+ viên đạn trên Android để đảm bảo 60 FPS ổn định, không sụt khung hình.
  - Đóng gói file `.aab` (Android App Bundle) với cấu hình IL2CPP ARM64-v8a đạt chuẩn Google Play Store Release.
