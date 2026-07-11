# HƯỚNG DẪN EXPORT & UPLOAD GAME WEBGL

Tài liệu này hướng dẫn chi tiết cho Nhà phát triển (Developer) cách xuất bản (Export) game từ Unity và dành cho Quản trị viên (Admin) cách tải lên (Upload) game vào hệ thống Storage của dự án.

---

## 1. HƯỚNG DẪN EXPORT WEBGL TỪ UNITY

Để đảm bảo hiệu năng tải game nhanh nhất và tương thích tốt nhất với hệ thống Cloud Save + Leaderboard, hãy cấu hình Build Settings trong Unity như sau:

### 1.1. Cấu hình Player Settings (Unity)
Vào **Edit > Project Settings > Player**, chọn tab **WebGL** và thiết lập:

1.  **Publishing Settings:**
    *   **Compression Format:** Chọn **Brotli** (khuyên dùng để giảm 30% dung lượng tải) hoặc **Gzip**.
    *   **Decompression Fallback:** Bật (**Checked**) để trình duyệt tự động giải nén nếu server chưa cấu hình header nén tương thích.
    *   **Data Caching:** Bật (**Checked**) để kích hoạt lưu trữ tạm cache trình duyệt.
2.  **Other Settings:**
    *   **Strip Engine Code:** Bật (**Checked**) để giảm kích thước của file `.wasm`.
    *   **Managed Stripping Level:** Đặt ở mức **Medium** hoặc **High**.

### 1.2. Cấu hình JS Bridge
Để Unity có thể giao tiếp với ReactJS, hãy copy file `SupabaseBridge.jslib` vào thư mục dự án Unity theo đúng đường dẫn:
`Assets/Plugins/WebGL/SupabaseBridge.jslib`

---

## 2. CẤU TRÚC THƯ MỤC WEBGL BUILD SAU KHI EXPORT

Sau khi Unity build xong, bạn sẽ nhận được một thư mục (ví dụ tên là `Binh_Ngo_Dai_Chien`) có cấu trúc như sau:

```text
Binh_Ngo_Dai_Chien/
├── index.html
├── TemplateData/
│   ├── style.css
│   └── ...
└── Build/
    ├── Binh_Ngo_Dai_Chien.loader.js
    ├── Binh_Ngo_Dai_Chien.data.br (hoặc .data.gz / .data)
    ├── Binh_Ngo_Dai_Chien.framework.js.br (hoặc .framework.js.gz)
    └── Binh_Ngo_Dai_Chien.wasm.br (hoặc .wasm.gz / .wasm)
```

> **💡 Lưu ý:** Bạn không cần phải đổi tên các file build thành `Build.*`. Hệ thống của chúng tôi sẽ tự động quét, nhận dạng các file có hậu tố đặc trưng và định cấu hình linh hoạt.

---

## 3. QUY TRÌNH UPLOAD GAME TRÊN TRANG ADMIN DASHBOARD

### 3.1. Các bước thực hiện
1.  Đăng nhập bằng tài khoản có cờ **Admin** (`profiles.is_admin = true`).
2.  Truy cập vào trang quản trị `/admin` (Tab **Upload game**).
3.  Điền các thông tin của game:
    *   **Tên game:** Tên hiển thị (Ví dụ: `Bình Ngô Đại Chiến`).
    *   **Slug:** Định danh URL (Ví dụ: `binh-ngo-dai-chien`).
    *   *Lưu ý: Bạn phải điền Slug trước khi tải file lên.*
4.  Tại phần **Upload thư mục WebGL build**:
    *   Bấm chọn thư mục và chọn trực tiếp thư mục build tổng của game (thư mục chứa file `index.html` và thư mục con `Build/`).
    *   Hệ thống sẽ quét toàn bộ các file, tự động loại bỏ tên thư mục cha để tránh lệch cấu trúc đường dẫn tương đối.
5.  **Cơ chế cấu hình tự động (game-config.json):**
    *   Trình duyệt sẽ phân tích và xác định các file cốt lõi của WebGL (`.loader.js`, `.data`, `.framework.js`, `.wasm`).
    *   Một file cấu hình tên là `game-config.json` sẽ tự động được tạo và đẩy lên thư mục game trên Storage cùng với các asset khác.
    *   Nội dung file `game-config.json` mẫu:
        ```json
        {
          "loaderUrl": "Build/Binh_Ngo_Dai_Chien.loader.js",
          "dataUrl": "Build/Binh_Ngo_Dai_Chien.data.br",
          "frameworkUrl": "Build/Binh_Ngo_Dai_Chien.framework.js.br",
          "codeUrl": "Build/Binh_Ngo_Dai_Chien.wasm.br"
        }
        ```
6.  Sau khi upload hoàn tất, hệ thống tự điền link public vào ô **Build URL**.
7.  Nhấn nút **Lưu game** để hoàn tất cập nhật metadata lên Database.

---

## 4. KIỂM TRA & KHẮC PHỤC SỰ CỐ
*   **Game không load được (Màn hình đen):**
    *   F12 mở tab Console xem có lỗi 404 file build không.
    *   Kiểm tra xem file `game-config.json` đã được upload thành công lên thư mục game trên Supabase Storage chưa.
*   **Lỗi phân quyền RLS khi upload:**
    *   Đảm bảo tài khoản đăng nhập của bạn có cờ `is_admin = true` trong bảng `profiles` trên Supabase database.
