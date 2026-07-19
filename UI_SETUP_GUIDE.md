# Hướng Dẫn Thiết Lập UI (UI Setup Guide) - Projectzombie

Tài liệu này cung cấp các bước chuẩn để Game Designer và Developer thiết lập UI trong Unity, đảm bảo tuân thủ kiến trúc **MVP (Model-View-Presenter)** và tối ưu hiệu năng theo `CODING_GUIDELINES.md`.

---

## 1. Yêu Cầu Canvas Cơ Bản

Mỗi hệ thống UI (HUD, Main Menu, Pause Menu) cần một `Canvas` chuẩn.

*   **Render Mode:** Tùy thuộc vào yêu cầu (thường là `Screen Space - Overlay`).
*   **Canvas Scaler:** 
    *   **UI Scale Mode:** Bắt buộc chọn `Scale With Screen Size`.
    *   **Reference Resolution:** Thường là `1920 x 1080` (Tùy dự án, hãy thống nhất 1 kích thước chuẩn).
    *   **Screen Match Mode:** `Match Width Or Height` (Match = 0.5 để cân bằng hoặc tùy theo thiết kế ngang/dọc).
*   **Graphic Raycaster:** Chỉ giữ lại trên các Canvas **thực sự cần tương tác chuột/touch** (Click button). Nếu Canvas chỉ hiển thị máu/chỉ số (HUD) và không thể click, hãy **tắt (Disable)** Graphic Raycaster để tiết kiệm CPU.

---

## 2. Quy Chuẩn Kéo / Gắn Component (Hierarchy)

### 2.1. Cấu trúc Object
Tách biệt rõ ràng các lớp layer trong UI:
```text
Canvas_PlayerHUD
├── UI_HealthBar (Panel)
│   ├── FillArea
│   └── Background
├── UI_SkillList (Horizontal Layout)
└── UI_RunStats (Panel)
    ├── Txt_Timer
    └── Txt_KillCount
```

### 2.2. Gắn Script MVP
Kiến trúc MVP yêu cầu 2 script chính trên UI: **View** và **Presenter**.

1.  **View Script (Vd: `PlayerHUDView.cs`):**
    *   Gắn trên Root GameObject của thành phần UI đó (vd: Gắn trên `Canvas_PlayerHUD` hoặc Panel chính).
    *   **Nhiệm vụ của Designer:** Kéo thả đúng các thành phần UI (Slider, TextMeshProUGUI, Image) vào các biến `[SerializeField]` trong Inspector.
    *   *Lưu ý:* Bắt buộc tắt `Raycast Target` trên các Text/Image không cần click để tối ưu.

2.  **Presenter Script (Vd: `PlayerInfoUIPresenter.cs`):**
    *   Có thể gắn chung GameObject với View HOẶC gắn ở một Game Manager riêng.
    *   **Nhiệm vụ của Designer:** Kéo GameObject chứa View vào biến `_view` của Presenter. Kéo các Models (vd: `PlayerStats`, `HealthSystem`) từ Player vào Presenter.

> [!WARNING]
> **Tuyệt đối không:** Gắn logic game, xử lý sát thương hay xử lý dữ liệu vào file View. View chỉ nhận string để hiển thị.

---

## 3. Quy Chuẩn TextMeshPro (TMP)

Dự án **KHÔNG SỬ DỤNG** Text Legacy của Unity.
*   **Component:** Bắt buộc dùng `TextMeshProUGUI`.
*   **Màu sắc / Định dạng:** Dùng **Rich Text** thay vì đổi màu bằng Code.
    *   *Ví dụ trong C#:* `text.text = "<color=#FF0000>HP Low</color>";`
*   **Font Asset:** Đảm bảo sử dụng Font Asset chuẩn đã được tạo trong thư mục `Assets/Art/Fonts/`. Tránh dùng font mặc định của TextMeshPro (LiberationSans) nếu không phải đồ nháp.

---

## 4. Animation & Time.timeScale

Rất nhiều màn hình UI (Level Up, Game Over, Pause) sẽ xuất hiện khi game đã dừng thời gian (`Time.timeScale = 0`).

*   **Animator:** Nếu UI Panel có component Animator để tạo hiệu ứng Fade in/out, **BẮTLLO BUỘC** phải set `Update Mode` thành `Unscaled Time` trong Inspector.
*   *Lưu ý cho Coder:* Hãy gọi cấu hình này ở `Awake()` của View để backup nếu Designer quên set trong Inspector:
    ```csharp
    GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
    ```

---

## 5. UI Layout Groups & Rebuilding (Tối ưu Hiệu năng)

*   Hạn chế sử dụng `VerticalLayoutGroup`, `HorizontalLayoutGroup`, `ContentSizeFitter` trên các UI thay đổi nội dung liên tục (mỗi frame). Quá trình "Layout Rebuild" của Unity rất ngốn CPU.
*   Nếu danh sách ít khi thay đổi (như danh sách Vũ khí đang sở hữu, Bảng Stats), Layout Group là hoàn toàn phù hợp.
*   Nếu là danh sách thay đổi liên tục, cân nhắc tự code tính toán vị trí thay vì phụ thuộc vào Layout Group.

---

## 6. Null-guard (Bảo vệ tham chiếu)

Trong quá trình phát triển, UI thường thay đổi liên tục. Designer có thể xóa nhầm một Text hoặc quên kéo vào Inspector.
*   Các file View bắt buộc phải có `if (_element == null) return;` trước khi gán text để game không bị văng lỗi (NullReferenceException) làm hỏng luồng chạy của logic khác.
