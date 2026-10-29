# Hướng Dẫn Đóng Gói (Build) & Phát Hành Android — Projectzombie

> **Dự án:** `Southern Myth Relic Guardians` (Thủ Thành 3D Sông Quê)  
> **Nền tảng:** Android (Google Play Store Release)  
> **Phiên bản Unity:** Unity 2022.3 LTS | **Render Pipeline:** Universal Render Pipeline (URP)

---

## 1. Yêu Cầu Môi Trường & SDK

Để build thành công gói cài đặt Android (APK / AAB), Unity Hub cần cài đặt đủ 3 thành phần Android Build Support:
* **Android Build Support**
* **Android SDK & NDK Tools** (Được quản lý tự động bởi Unity)
* **OpenJDK** (Được quản lý tự động bởi Unity)

> [!TIP]
> Kiểm tra tại Unity Editor: `Edit > Preferences > External Tools` ➔ Đảm bảo các mục **JDK**, **SDK**, **NDK** và **Gradle** đều được tích chọn *"Installed with Unity (recommended)"*.

---

## 2. Tiêu Chuẩn Kỹ Thuật Bắt Buộc (Google Play Requirements)

Theo chính sách phát hành mới nhất của Google Play Store:
1. **Kiến Trúc 64-bit (ARM64):** Bắt buộc hỗ trợ `ARM64` để chạy mượt mà trên các thiết bị Android thế hệ mới.
2. **Scripting Backend:** Bắt buộc sử dụng `IL2CPP` (thay vì Mono) để tối ưu hiệu năng C++ và hỗ trợ ARM64.
3. **Target API Level:** Bắt buộc tối thiểu `API Level 34` (Android 14) hoặc `API Level 35`.
4. **Định dạng phát hành:** Bắt buộc xuất file `.aab` (Android App Bundle) thay cho file `.apk` truyền thống để giảm 30-50% dung lượng tải về cho người dùng.

---

## 3. Quy Trình 1-Click Build Trong Unity Editor

Dự án đã tích hợp sẵn công cụ tự động hóa tại thanh Menu:

```text
ProjectZombie/
└── 4. 🤖 Android Build & Deploy/
    ├── 1. Kiểm Tra & Tối Ưu Cấu Hình Android (PlayerSettings)
    ├── 2. Xuất APK Test (Development Build)
    ├── 3. Pre-Build Asset Validator
    └── 4. Xuất Google Play App Bundle (.aab)
```

### 🔹 Bước 1: Chuẩn Hóa Cấu Hình (Chạy 1 lần hoặc trước khi build)
* Chọn menu: `ProjectZombie > 4. 🤖 Android Build & Deploy > 1. Kiểm Tra & Tối Ưu Cấu Hình Android`.
* Hệ thống sẽ tự động:
  - Chuyển Backend sang `IL2CPP`.
  - Bật kiến trúc `ARM64` + `ARMv7`.
  - Cập nhật `Target SDK = 34`, `Min SDK = 24`.
  - Khóa màn hình ngang (`Landscape`).
  - Đồng bộ tài nguyên Resources và Shaders URP.

### 🔹 Bước 2: Xuất File Cài Đặt
* **Để Test trên điện thoại thực tế:**
  - Chọn menu: `ProjectZombie > 4. 🤖 Android Build & Deploy > 2. Xuất APK Test (Development Build)`.
  - File `.apk` sẽ được tạo tại thư mục `Builds/Android/`.
* **Để Tải lên Google Play Store:**
  - Chọn menu: `ProjectZombie > 4. 🤖 Android Build & Deploy > 4. Xuất Google Play App Bundle (.aab)`.
  - File `.aab` sẽ được tạo tại thư mục `Builds/Android/`.

---

## 4. Cài Đặt File APK Lên Điện Thoại Qua ADB (Android Debug Bridge)

Nếu bạn có điện thoại Android cắm dây USB vào máy tính (đã bật *USB Debugging* trong *Developer Options*):

```powershell
# 1. Kiểm tra thiết bị đã kết nối thành công:
adb devices

# 2. Cài đặt file APK trực tiếp vào điện thoại:
adb install -r Builds/Android/ProjectZombie_Dev_<timestamp>.apk

# 3. Xem log thời gian thực của game khi chơi trên điện thoại:
adb logcat -s Unity
```

---

## 5. Hướng Dẫn Ký Chữ Ký Phát Hành (Keystore Signing) Cho Bản Release

Trước khi phát hành chính thức lên Google Play, ứng dụng phải được ký bằng Keystore bảo mật:

1. Mở `Project Settings > Player > Android > Publishing Settings`.
2. Tích chọn **Keystore Manager** ➔ Chọn **Create New** (hoặc chọn Keystore có sẵn của studio).
3. Lưu file keystore vào một nơi an toàn (tuyệt đối không chia sẻ công khai).
4. Điền Password, Alias name và Alias Password.
5. Khi bấm xuất bản `.aab`, Unity sẽ tự động ký chữ ký số này vào gói phát hành.

---

## 6. Checklist Kiểm Tra Cuối Trước Khi Release

- [x] Danh sách Scene trong Build: `Assets/Scenes/MainMenu.unity` (Scene 0) và `Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity` (Scene 1).
- [x] SRP Batcher được bật trên `URP_2D_Asset.asset` (`m_UseSRPBatcher: 1`).
- [x] Không còn rác chuỗi (0 GC Allocations) trong combat loop.
- [x] Nam châm hút gỗ hoạt động mượt mà trong bán kính 3.8m.
- [x] Virtual Joystick và cụm nút chiêu thức hỗ trợ cảm ứng đa điểm, có rung phản hồi xúc giác.
- [x] Định dạng phát hành là `.aab` với Scripting Backend `IL2CPP` và kiến trúc `ARM64`.
