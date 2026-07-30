# Danh Sách Nhiệm Vụ & Task Tracker — VONG XUYÊN

Tài liệu quản lý danh sách công việc (Kanban Board Task Tracker) phân chia theo các hạng mục của dự án **Vong Xuyên (Android Release - GDD v4.0)**.

---

## 📋 Kanban Board Summary

| 🔴 To Do | 🟡 In Progress | 🟢 Done |
|---|---|---|
| UpgradeCardView Element Badges | 12 Pháp Bảo Asset Refactoring | Android Target Platform Refactor |
| Meta Upgrade Shop Cổ Tiền UI | 5 Yêu Ma Config Asset Refactoring | Local Save System (`SaveSystem.cs`) |
| Character Selection UI (3 Heroes) | | Ngũ Hành Damage & Combo System |
| Mobile Stress Test & ASTC | | Cán Cân Âm Dương (`YinYangManager`) |
| Android AAB Build & Signing | | Boss AI Dynamic Element (`BossElementController`) |
| | | RunHUD MVP Update (Âm Dương Slider & Boss Text) |
| | | GDD 4.0 & Scenario Guide Single Source of Truth |

---

## 🏃 Sprint Tasks Breakdown

### ☯️ Hạng Mục 1: Lõi Cơ Chế Ngũ Hành & Âm Dương (Completed)
- [x] **[TASK-101]** Refactor `DamageUtility.cs` và `DamageData.cs` hỗ trợ tra cứu Tương Khắc Ngũ Hành (+30% Sát thương).
- [x] **[TASK-102]** Tích hợp Element Combo Tracker vào `WeaponManager.cs` (-20% Cooldown khi kích hoạt Tương Sinh trong 3s).
- [x] **[TASK-103]** Xây dựng `YinYangManager.cs` theo dõi trạng thái Âm Dương (0-100) và phát sự kiện `OnYinYangStateChanged`.
- [x] **[TASK-104]** Tích hợp lọc pool thẻ Gacha nâng cấp ngẫu nhiên trong `UpgradeManager.cs` theo `YinYangState`.
- [x] **[TASK-105]** Tạo `BossElementController.cs` cho phép Boss (Ngưu Đầu Mã Diện & Diêm Vương) luân phiên xoay vòng thuộc tính Ngũ Hành theo chu kỳ 10s.

### 🗡️ Hạng Mục 2: Reskin Content & Asset Database (In Progress)
- [x] **[TASK-201]** Thêm trường `elementType` vào ScriptableObject `WeaponData.cs` và `EnemyConfig.cs`.
- [/] **[TASK-202]** Refactor dữ liệu `.asset` cho 12 Pháp Bảo MVP (Nỏ Thần, Bút Phán Quan, Bùa Trấn Yêu, Trống Đồng...).
- [/] **[TASK-203]** Refactor dữ liệu `.asset` cho 5 Yêu Ma (Ma Giáp, Ma Trơi, Quỷ Nhập Tràng, Ma Da, Hồ Ly Tinh Nhỏ).
- [x] **[TASK-204]** Chuyển đổi tên gọi đơn vị tiền Meta thành **Cổ Tiền** (tiền xu cổ Việt Nam) trong `MetaCurrencyManager.cs` và `MetaProgressionSaveData.cs`.

### 📱 Hạng Mục 3: Mobile UI Canvas & MVP Systems (In Progress)
- [x] **[TASK-301]** Cập nhật `RunHUDView.cs` và `RunHUDPresenter.cs` thêm Slider Cán cân Âm Dương & TMP Text hiển thị thuộc tính Boss.
- [ ] **[TASK-302]** Thêm Badge màu hiển thị thuộc tính Ngũ Hành trên thẻ Gacha Nâng cấp (`UpgradeCardView.cs`).
- [ ] **[TASK-303]** Xây dựng giao diện `MetaUpgradeShopView.cs` và `MetaUpgradeShopPresenter.cs` cho Cây nâng cấp vĩnh viễn dùng Cổ Tiền.
- [ ] **[TASK-304]** Xây dựng UI Chọn Nhân vật theo 3 anh hùng: Thư Sinh (Kim), Đạo Sĩ (Mộc), Võ Tăng (Thổ).
- [ ] **[TASK-305]** Xây dựng màn hình `RunSummaryView.cs` hiển thị kết quả sau run đấu và cộng Cổ Tiền tích lũy.

### ⚙️ Hạng Mục 4: Performance & Build Release (Planned)
- [ ] **[TASK-401]** Chuyển đổi toàn bộ Sprite Sheets sang Texture Compression **ASTC 6x6** cho Android.
- [ ] **[TASK-402]** Thử nghiệm Stress Test 200 Yêu ma + 100 Projectiles kiểm tra FPS (Target 60 FPS).
- [ ] **[TASK-403]** Cấu hình Build Profile Android IL2CPP ARM64, Target API 33+, xuất file `.aab`.
