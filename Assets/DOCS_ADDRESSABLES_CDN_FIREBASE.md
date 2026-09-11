# Hướng Dẫn Tích Hợp Firebase Storage CDN Cho Unity Addressables (Dễ Nhất - Kéo Thả Trực Quan)

Tài liệu này hướng dẫn cách sử dụng **Firebase Storage** làm **CDN / DLC** cho Unity Addressables theo phương pháp **100% Giao Diện Web Trực Quan — Không Cần Cài Đặt Node.js, Không Cần Dòng Lệnh**.

---

## 1. Ưu Điểm Của Phương Pháp Kéo & Thả Firebase Storage

* **0 Cài Đặt**: Không cần cài Node.js, không cần gõ lệnh Terminal/PowerShell.
* **0 Xung Đột Code**: Không cần import thêm SDK Firebase vào Unity, tránh lỗi Gradle khi build Android APK.
* **Trực Quan**: Thấy rõ từng file `.bundle` dung lượng bao nhiêu MB trực tiếp trên trình duyệt.
* **Miễn Phí**: Google tặng sẵn 5 GB lưu trữ và 1 GB tải về mỗi ngày (đầy đủ cho nhu cầu phát triển & thử nghiệm).

---

## 2. Quy Trình 3 Bước Thực Hiện

### BƯỚC 1: Tạo Thư Mục Trên Firebase Console (1 Lần Duy Nhất)

1. Mở trình duyệt và truy cập: [Firebase Console](https://console.firebase.google.com/).
2. Chọn Project của bạn (hoặc bấm **Add project** để tạo mới).
3. Ở thanh menu bên trái, chọn **Build > Storage** -> Bấm **Get started**.
4. Chọn chế độ bảo mật:
   - Trong tab **Rules** của Storage, cho phép đọc công khai (để game Android tải được file):
     ```javascript
     rules_version = '2';
     service firebase.storage {
       match /b/{bucket}/o {
         match /{allPaths=**} {
           allow read: if true; // Cho phép game tải file không cần đăng nhập
           allow write: if false;
         }
       }
     }
     ```
   - Bấm **Publish** để lưu luật bảo mật.
5. Quay lại tab **Files**, bấm **Create folder** và đặt tên là: **`Android`**.

---

### BƯỚC 2: Cấu Hình Unity Addressables Profile

1. Trong Unity Editor, mở: `Window > Asset Management > Addressables > Profiles`.
2. Chọn Profile (hoặc tạo mới Profile đặt tên `Firebase_Web`):
   - **`Remote.BuildPath`**: Giữ nguyên mặc định là `ServerData/[BuildTarget]`
   - **`Remote.LoadPath`**: Dán đường link URL của thư mục Firebase Storage vào:
     ```text
     https://firebasestorage.googleapis.com/v0/b/<tên-project-của-bạn>.appspot.com/o/Android%2F{0}?alt=media
     ```
     *(Thay `<tên-project-của-bạn>` bằng ID dự án trên Firebase của bạn)*

---

### BƯỚC 3: Xuất File Bundle & Kéo Thả Lên Web

Mỗi khi bạn muốn cập nhật bản đồ mới, quái vật mới hoặc hiệu ứng VFX:

1. **Build trong Unity**:
   - Mở `Window > Asset Management > Addressables > Groups`.
   - Bấm nút **Build > New Build > Default Build Script**.
   - Unity sẽ nén và xuất các file vào thư mục: `Projectzombie/ServerData/Android/`.
2. **Kéo Thả Lên Web**:
   - Mở thư mục `ServerData/Android/` trên máy tính (sẽ có các file `.bundle`, `catalog.json`, `catalog.hash`).
   - Mở trình duyệt vào thư mục `Android` trên **Firebase Storage**.
   - **Kéo thả toàn bộ các file đó vào trình duyệt**.
3. **Hoàn Tất!** 
   - Mọi máy điện thoại Android khi mở game sẽ tự động đọc `catalog.hash` mới nhất và tải các gói màn chơi mới về máy.

---

## 3. Cách Phân Chia Nhóm Asset (Local vs Remote)

Trong bảng `Addressables Groups`:

* **Nhóm `Local_Core` (Nằm sẵn trong APK)**: 
  - UI cơ bản, Menu chính, Âm thanh nút bấm, Màn 1 (Chapter 1), Nhân vật mặc định.
  - Cấu hình nhóm: `Build & Load Paths` chọn **`Local`**.
* **Nhóm `Remote_DLC` (Tải từ Firebase về máy)**:
  - Bản đồ Màn 2, 3, 4; Quái vật cấp cao; Skin đặc biệt.
  - Cấu hình nhóm: `Build & Load Paths` chọn **`Remote`**.
