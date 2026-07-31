# Danh Sách Nhiệm Vụ & Task Tracker — VONG XUYÊN

Tài liệu quản lý danh sách công việc (Kanban Board Task Tracker) phân chia theo các hạng mục của dự án **Vong Xuyên (Android Release - GDD v4.0)**.

---

## 📋 Kanban Board Summary

| 🔴 To Do | 🟡 In Progress | 🟢 Done |
|---|---|---|
| Mobile Stress Test & ASTC | 🚨 **[TASK-300] Setup Mobile Controls Canvas (Joystick & Skill Button)** | Android Target Platform Refactor |
| Android AAB Build & Signing | | Local Save System (`SaveSystem.cs`) |
| | | Ngũ Hành Damage & Combo System |
| | | Cán Cân Âm Dương (`YinYangManager`) |
| | | Boss AI Dynamic Element (`BossElementController`) |
| | | 12 Pháp Bảo & 5 Yêu Ma Data SOs |
| | | Upgrade Cards Badges & Meta Shop UI |

---

## 🏃 Sprint Tasks Breakdown

### ☯️ Hạng Mục 1: Lõi Cơ Chế Ngũ Hành & Âm Dương (Completed)
- [x] **[TASK-101]** Refactor `DamageUtility.cs` và `DamageData.cs` hỗ trợ tra cứu Tương Khắc Ngũ Hành (+30% Sát thương).
- [x] **[TASK-102]** Tích hợp Element Combo Tracker vào `WeaponManager.cs` (-20% Cooldown khi kích hoạt Tương Sinh trong 3s).
- [x] **[TASK-103]** Xây dựng `YinYangManager.cs` theo dõi trạng thái Âm Dương (0-100) và phát sự kiện `OnYinYangStateChanged`.
- [x] **[TASK-104]** Tích hợp lọc pool thẻ Gacha nâng cấp ngẫu nhiên trong `UpgradeManager.cs` theo `YinYangState`.
- [x] **[TASK-105]** Tạo `BossElementController.cs` cho phép Boss (Ngưu Đầu Mã Diện & Diêm Vương) luân phiên xoay vòng thuộc tính Ngũ Hành theo chu kỳ 10s.

### 🗡️ Hạng Mục 2: Reskin Content & Asset Database (Completed)
- [x] **[TASK-201]** Thêm trường `elementType` vào ScriptableObject `WeaponData.cs` và `EnemyConfig.cs`.
- [x] **[TASK-202]** Tạo `WeaponDataGenerator.cs` tự động sinh/cập nhật dữ liệu `.asset` cho 12 Pháp Bảo MVP và 12 Evolution theo GDD 4.0.
- [x] **[TASK-203]** Tạo `EnemyDataGenerator.cs` tự động sinh/cập nhật dữ liệu `.asset` cho 5 Yêu Ma MVP (Ma Giáp, Ma Trơi, Quỷ Nhập Tràng, Ma Da, Hồ Ly Tinh Nhỏ).
- [x] **[TASK-204]** Chuyển đổi tên gọi đơn vị tiền Meta thành **Cổ Tiền** (tiền xu cổ Việt Nam) trong `MetaCurrencyManager.cs` và `MetaProgressionSaveData.cs`.

### 📱 Hạng Mục 3: Mobile UI Canvas & MVP Systems
- [ ] 🚨 **[TASK-300] [HIGH PRIORITY — ƯU TIÊN HÀNG ĐẦU]** Setup Mobile Controls Canvas trong Scene Unity: Dựng `DynamicVirtualJoystick` (Cần gạt di chuyển) & `SignatureSkillButtonView` + `SignatureSkillPresenter` (Nút bấm Skill chủ động) kết nối với PlayerController.
- [x] **[TASK-301]** Cập nhật `RunHUDView.cs` và `RunHUDPresenter.cs` thêm Slider Cán cân Âm Dương & TMP Text hiển thị thuộc tính Boss.
- [x] **[TASK-302]** Thêm Badge màu hiển thị thuộc tính Ngũ Hành trên thẻ Gacha Nâng cấp (`UpgradeCardView.cs`).
- [x] **[TASK-303]** Xây dựng giao diện `MetaUpgradeShopView.cs` và `MetaUpgradeShopPresenter.cs` cho Cây nâng cấp vĩnh viễn dùng Cổ Tiền.
- [x] **[TASK-304]** Xây dựng UI Chọn Nhân vật theo 3 anh hùng: Thư Sinh (Kim), Đạo Sĩ (Mộc), Võ Tăng (Thổ) qua `CharacterSelectionView/Presenter`.
- [x] **[TASK-305]** Xây dựng màn hình `RunSummaryView.cs` & `RunSummaryPresenter.cs` hiển thị kết quả sau run đấu và tự động cộng Cổ Tiền tích lũy.

### ⚙️ Hạng Mục 4: Performance & Build Release (Planned)
- [ ] **[TASK-401]** Chuyển đổi toàn bộ Sprite Sheets sang Texture Compression **ASTC 6x6** cho Android.
- [ ] **[TASK-402]** Thử nghiệm Stress Test 200 Yêu ma + 100 Projectiles kiểm tra FPS (Target 60 FPS).
- [ ] **[TASK-403]** Cấu hình Build Profile Android IL2CPP ARM64, Target API 33+, xuất file `.aab`.
