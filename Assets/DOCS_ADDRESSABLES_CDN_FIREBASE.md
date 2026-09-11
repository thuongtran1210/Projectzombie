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

## 3. Kiến Trúc Phân Chia Nhóm Asset (Local Core vs Remote CDN Groups)

Trong bảng `Addressables Groups` (`Window > Asset Management > Addressables > Groups`), toàn bộ tài nguyên trong dự án được tổ chức thành **6 Nhóm Chiến Lược** như sau:

| Tên Nhóm (Group Name) | Build & Load Paths | Bundle Mode | Mục đích & Danh mục Asset chứa bên trong |
|---|---|---|---|
| **`Group_Core_Preload`** | **`Local`** (Trong APK) | Pack Together | UI Sảnh chính, Font Chữ TMP, Hệ thống Bootstrapper, Âm thanh UI cơ bản, Đạo Sĩ mặc định. |
| **`Group_Enemies_Stage1`** | **`Local`** (Trong APK) | Pack Together | Quái vật Màn 1 (Cương thi thường, Thủy quái cơ bản, Chuột ma) để người chơi vào game là chơi được ngay. |
| **`Group_Weapons_Tier1`** | **`Local`** (Trong APK) | Pack Together | Bộ 4 vũ khí khởi đầu & đạn cơ bản (Bình Bát, Dép Tổ Ong, Chổi Lông Gà, Trảo Cửu Vĩ). |
| **`Group_DLC_Stages_Remote`** | **`Remote`** (CDN Firebase) | **Pack Separately** (Từng Asset) | Tilemap Prefab các màn nâng cao (`Map_AncientCitadel`, `Map_CinnabarSwamp`, `Map_UnderworldGate`). |
| **`Group_DLC_Enemies_Remote`**| **`Remote`** (CDN Firebase) | Pack Together | Quái vật cấp cao Màn 2-3-4, Quỷ tướng tinh anh, Boss các chương sau. |
| **`Group_DLC_Audio_Remote`**  | **`Remote`** (CDN Firebase) | Pack Separately (Từng Asset) | Nhạc nền BGM chất lượng cao của từng Ải (`BGM_AncientCitadel.mp3`, `BGM_CinnabarSwamp.mp3`). |

---

## 4. Bảng Quy Chuẩn Cấu Hình Nhóm Trong Inspector

Khi chọn một Group trong Addressables Groups Window:

### A. Với các nhóm `Local_*`:
- **`Build & Load Paths`**: Chọn **`Local`** (Mặc định `[UnityEngine.AddressableAssets.Addressables.BuildPath]`).
- **`Bundle Mode`**: `Pack Together` (Gom chung thành 1 file .bundle duy nhất để nén dung lượng APK).
- **`Compression`**: `LZ4` (Tối ưu tốc độ giải nén siêu tốc trên Android).

### B. Với các nhóm `Group_DLC_*_Remote`:
- **`Build & Load Paths`**: Chọn **`Remote`** (Trỏ về Profile `Firebase_Web`).
- **`Bundle Mode`**: 
  - Chọn **`Pack Separately`** cho Map & BGM để **tải riêng từng ải theo nhu cầu** (Chơi ải nào tải ải đó, không bắt người chơi tải cả cụm 100MB).
  - Chọn **`Pack Together`** cho Quái vật theo từng Chapter.
- **`Compression`**: `LZ4` hoặc `LZMA` (LZMA giúp file tải trên mạng nhẹ nhất có thể).
