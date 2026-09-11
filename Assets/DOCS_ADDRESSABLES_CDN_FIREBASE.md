# Hướng Dẫn Tích Hợp Firebase Hosting CDN Cho Unity Addressables (Android)

Tài liệu này mô tả chi tiết quy trình thiết lập, đóng gói và vận hành hệ thống **Content Delivery Network (CDN) & Downloadable Content (DLC)** bằng **Firebase Hosting / Storage** cho dự án di động **Projectzombie** (Unity 2022).

---

## 1. Tại Sao Chọn Firebase Hosting Cho Unity Addressables?

1. **Miễn Phí & Tốc Độ Cao (Google Global CDN Edge Network)**:
   - Miễn phí 10 GB lưu trữ.
   - Miễn phí 360 MB/ngày truyền tải (đối với gói Spark miễn phí) hoặc tính tiền theo mức dùng cực rẻ ($0.15/GB đối với gói Blaze).
   - Tự động cấp chứng chỉ bảo mật HTTPS bắt buộc trên Android.
2. **Cập Nhật 0 Giây (Instant Invalidation)**:
   - Khi bạn deploy bundle mới bằng lệnh `firebase deploy`, toàn bộ mạng lưới CDN của Google cập nhật dữ liệu trong vòng vài giây trên toàn cầu.
3. **Không cần code Server backend**:
   - Máy client Android chỉ cần gọi các URL tĩnh (`https://<project-id>.web.app/Android/...`).

---

## 2. Quy Trình Cài Đặt Firebase CLI (1 Lần Duy Nhất)

### Bước 1: Cài đặt Node.js & Firebase Tools
Mở PowerShell trên máy tính và chạy lệnh:
```bash
npm install -g firebase-tools
```

### Bước 2: Đăng nhập Firebase
```bash
firebase login
```

### Bước 3: Khởi tạo thư mục CDN cho Unity
1. Tạo một thư mục riêng bên ngoài dự án Unity, ví dụ: `D:/Projectzombie_CDN/`
2. Mở terminal tại thư mục đó và chạy:
```bash
firebase init hosting
```
* Chọn dự án Firebase của bạn (VD: `projectzombie-app`).
* Chọn thư mục public: `public` (Mặc định).
* Configure as a single-page app? Chọn **`N`** (No).
* Set up automatic builds with GitHub? Chọn **`N`** (No).

File cấu hình `firebase.json` tạo ra sẽ có dạng:
```json
{
  "hosting": {
    "public": "public",
    "ignore": [
      "firebase.json",
      "**/.*",
      "**/node_modules/**"
    ],
    "headers": [
      {
        "source": "**/*.@(bundle|hash|json)",
        "headers": [
          {
            "key": "Cache-Control",
            "value": "max-age=3600"
          },
          {
            "key": "Access-Control-Allow-Origin",
            "value": "*"
          }
        ]
      }
    ]
  }
}
```

---

## 3. Cấu Hình Unity Addressables Profile Trỏ Vào Firebase

Trong cửa sổ Unity: `Window > Asset Management > Addressables > Profiles`:

1. Tạo một Profile mới đặt tên: **`Firebase_Production`**
2. Cấu hình các biến đường dẫn:
   * **`Remote.BuildPath`**: `ServerData/[BuildTarget]` (Thư mục Unity xuất file bundle)
   * **`Remote.LoadPath`**: `https://<your-firebase-project-id>.web.app/[BuildTarget]`

> [!TIP]
> Biến `[BuildTarget]` trong Unity sẽ tự động thay bằng `Android` khi bạn chuyển nền tảng sang Android, hoặc `StandaloneWindows64` khi ở PC.

---

## 4. Phân Nhóm Asset (Addressables Groups Architecture)

Trong `Addressables Groups`, tổ chức các nhóm như sau:

| Tên Nhóm (Group Name) | Build & Load Path | Chứa Tài Nguyên Gì? | Mục Đích |
| :--- | :--- | :--- | :--- |
| **`Local_Core`** | `Local.BuildPath`<br>`Local.LoadPath` | Core UI, Splash, Font TMP, Màn 1 (Chapter 1), Hero Cơ Bản. | Nằm sẵn trong file cài APK, mở game chơi được ngay không cần mạng. |
| **`Remote_Chapters`** | `Remote.BuildPath`<br>`Remote.LoadPath` | Bản đồ Màn 2, 3, 4 (Tilemaps, BGM, Decor Props). | Chỉ tải về khi người chơi vượt qua Màn 1. |
| **`Remote_Enemies`** | `Remote.BuildPath`<br>`Remote.LoadPath` | Quái tinh anh, Boss các Chapter sau, Âm thanh gầm rú. | Tiết kiệm 40% dung lượng bộ nhớ cho người chơi mới. |
| **`Remote_VFX_Weapons`**| `Remote.BuildPath`<br>`Remote.LoadPath` | Vũ khí đặc biệt, Skin Pháp Bảo cao cấp, Hào quang. | Cập nhật cân bằng VFX từ xa không cần nộp lại Google Play. |

---

## 5. Quy Trình Xuất Bản & Deploy Asset (Build Pipeline)

Mỗi khi thêm quái mới, map mới hoặc sửa hiệu ứng VFX:

1. **Build Bundles trong Unity**:
   * Mở `Window > Asset Management > Addressables > Groups`.
   * Chọn `Build > New Build > Default Build Script`.
   * Unity sẽ xuất các file `.bundle`, `catalog.json`, `catalog.hash` vào thư mục `ServerData/Android/`.
2. **Copy file vào thư mục Firebase**:
   * Copy toàn bộ thư mục `ServerData/Android` vào `D:/Projectzombie_CDN/public/Android/`.
3. **Đẩy lên CDN toàn cầu**:
   ```bash
   cd D:/Projectzombie_CDN/
   firebase deploy --only hosting
   ```
4. **Kết quả**: Tất cả thiết bị Android của người chơi khi mở game sẽ tự động nhận diện `catalog.hash` mới và cập nhật nội dung tức thì!

---

## 6. Xử Lý Bộ Nhớ Đệm & Offline Mode Trên Android

* Khi tải về từ Firebase CDN, Addressables tự động lưu vào thư mục Cache trên điện thoại:
  `Application.persistentDataPath/com.unity.addressables/`
* Nếu người chơi không có mạng (Offline), hệ thống sẽ tự động dùng dữ liệu đã cache trong máy để tiếp tục chơi bình thường.
