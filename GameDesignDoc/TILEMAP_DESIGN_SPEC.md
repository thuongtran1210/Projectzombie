# Tilemap Design Spec — Dự Án VONG XUYÊN

**Phiên bản:** 1.0 (Official Single Source of Truth)  
**Cơ sở:** Được điều chỉnh từ `Document Hướng Dẫn Mỹ Thuật & VFX v1.0`, cập nhật theo thông số **PPU = 64** (thay vì PPU = 32 như bản gốc)  
**Đối tượng sử dụng:** 2D Artist, Technical Artist (TA), Level Designer  
**Nền tảng mục tiêu:** Android Mobile (Top-down 2D, Target API 33+, 60 FPS)  

---

## 0. Ghi Chú Quy Đổi Thông Số (PPU 32 → PPU 64)

Vì nhân vật đang dùng **PPU = 64** (`1 Unit = 1m = 64px`), toàn bộ thông số liên quan đến pixel trong tài liệu Art gốc (vốn tính theo PPU 32) cần nhân đôi để giữ đúng tỷ lệ 1m = 1 world unit giữa nhân vật và tilemap. Nếu tilemap vẫn dùng PPU 32 trong khi nhân vật dùng PPU 64, nhân vật sẽ bị "nét đôi" (sprite mịn hơn hẳn nền) — vỡ tương quan độ chi tiết. **Tilemap bắt buộc phải đồng bộ PPU = 64 với nhân vật.**

| Thông số | PPU 32 (gốc) | PPU 64 (áp dụng) |
|---|---|---|
| 1 Tile chuẩn | 32×32px | **64×64px** |
| Canvas nhân vật/quái thường | 32–48px | **64–96px** |
| Giới hạn Sprite tối đa (trừ Boss) | 64px | **128px** |
| Orthographic Size (giữ nguyên khung hình 12m) | 6.0 | **6.0 (không đổi)** |
| Reference Resolution (Pixel Perfect Camera) | 768×432 / 640×360 | **1536×864 / 1280×720** |

> Lưu ý: Orthographic Size giữ nguyên 6.0 vì đây là thông số world-space (mét), không phụ thuộc PPU — chỉ có độ phân giải pixel quy đổi ra là tăng gấp đôi.

---

## 1. Grid & Kích Thước Tile

*   **Tile chính (Ground/Base Layer):** **64×64px**, khớp PPU = 64 → 1 tile = 1m × 1m.
*   **Tile phụ (Decoration/Detail Layer):** có thể dùng **32×32px** làm sub-tile chồng lên lớp Ground để tăng độ phong phú (đá vụn, cỏ dại, xương, vết nứt) mà không phải vẽ lại toàn bộ atlas 64px cho các chi tiết nhỏ.
*   **Grid trong Unity:** `Grid Cell Size = 1,1,0` (world units), khớp Tilemap với Pixel Perfect Camera PPU = 64.
*   **Không trộn PPU khác nhau trong cùng 1 Tilemap Atlas** — mọi tile trong cùng layer phải đồng nhất 64px để tránh lệch pixel khi Pixel Perfect Camera snap.

---

## 2. Nguyên Tắc Tương Phản Nền/Vật Thể (bắt buộc giữ nguyên, không đổi theo PPU)

Đây là rule quan trọng nhất kế thừa từ tài liệu Art gốc, **áp dụng y nguyên bất kể PPU**:

*   **Tilemap = Tối & Desaturated:** xám xanh u uất, nâu bùn, tím đen.
*   **Nhân vật/Quái/Vật thể tương tác = Sáng & Saturated hơn nền 1–2 bậc Luminance.**
*   Gợi ý mốc đo cụ thể để Art Team áp dụng nhất quán:
    *   Tilemap: **Luminance 15–35%**
    *   Nhân vật/Quái/VFX: **Luminance 45–65%+**
*   Việc tăng PPU lên 2x sẽ khiến tile hiển thị chi tiết rõ hơn (không còn bị "vỡ" ở khoảng cách gần) — cần cẩn thận không để chi tiết mới lộ ra làm tile bị nổi bật quá mức so với rule Luminance ở trên.

---

## 3. Cấu Trúc Layer Tilemap (Unity)

Tối thiểu 3 layer, tách riêng theo chức năng và cách tối ưu batching:

| Layer | Nội dung | Collider | Ghi chú |
|---|---|---|---|
| **Ground** | Nền đất, đường mòn, mặt nước tĩnh | Không | Dùng Rule Tile / Auto-tiling để giảm khối lượng vẽ tay cho map 60×60m |
| **Decoration** | Cỏ, lá bùa rơi, xương, rêu, vết nứt (sub-tile 32px) | Không | Không chặn di chuyển/đạn, chỉ tăng chi tiết thị giác |
| **Collision** | Bia mộ, cây khô, đá, vật cản địa hình | Có (`Tilemap Collider2D` + `Composite Collider2D`) | Gộp Composite Collider để tối ưu physics, tránh hàng trăm collider rời rạc |

---

## 4. Đồng Bộ Với 4 Giai Đoạn Atmosphere (Palette-Swap)

Theo mục 4 tài liệu Art gốc (Post-Processing Color Grading, không tốn GPU):

*   Chỉ cần vẽ **1 bộ tile nền trung tính (neutral base)** ở PPU 64 — không vẽ riêng 4 bộ theo từng giai đoạn.
*   Bắt buộc **test bộ tile gốc dưới cả 4 lớp màu** trước khi duyệt asset final:
    1.  `00:00–05:00` Sương Mờ U Linh (xanh chàm)
    2.  `05:00–10:00` Âm Phong Hoàng Tuyền (vàng úa nhạt)
    3.  `10:00–19:59` Bão Hắc Khí Huyết Nguyệt (đỏ thẫm)
    4.  `20:00+` Hỏa Ngục Địa Môn (đỏ cam)
*   Vùng biên bản đồ (map edge) ở giai đoạn 4 nên có **tile riêng bật/tắt bằng code** (không chỉ dựa Color Grading) để thể hiện rõ hiệu ứng "rực cháy" mà không làm mất chi tiết nền do bị đẩy tông quá gắt.

---

## 5. Phân Bổ Độ Chi Tiết Họa Tiết

Kế thừa nguyên tắc mục 1.3 tài liệu gốc:

*   **Vùng combat trung tâm:** tile càng đơn giản càng tốt, tránh texture noise tần số cao — vùng này luôn có 150–200 quái/VFX chuyển động, chi tiết rườm rà sẽ đánh lừa mắt người chơi.
*   **Vùng viền/background xa (ngoài tầm tương tác):** có thể đưa họa tiết Tranh Đông Hồ/Hàng Trống cách điệu (hoa văn sóng nước, mây, họa tiết dân gian) vì không cạnh tranh readability với gameplay.

---

## 6. Technical Import & Naming Convention

*   **Import Settings:** `Texture Type = Default`, `Alpha Is Transparency = True`, nén **ASTC 6x6** cho Android — đồng bộ với quy chuẩn VFX/Sprite trong tài liệu gốc.
*   **Naming convention đề xuất cho tile assets:**
    *   Tileset atlas: `TS_{Zone}_{Layer}.png` (VD: `TS_BenDo_Ground.png`)
    *   Rule Tile asset: `RT_{Zone}_{Type}` (VD: `RT_BenDo_DirtPath`)
    *   Tile riêng lẻ: `T_{Zone}_{Layer}_{Index}` (VD: `T_BenDo_Deco_003`)

---

## 7. Cảnh Báo Ngân Sách APK/AAB

PPU 64 đồng nghĩa mỗi tile/sprite chiếm diện tích pixel gấp ~4 lần so với PPU 32 (do tăng cả chiều rộng lẫn chiều cao). Điều này ảnh hưởng trực tiếp đến mục tiêu **APK/AAB dưới 60MB** đã đặt ra trong tài liệu gốc:

*   Ưu tiên dùng **Rule Tile/Auto-tiling** thay vì vẽ tay từng tile để giảm số lượng texture unique cần lưu.
*   Cân nhắc gộp chung 1 Tile Atlas theo từng zone thay vì theo từng loại tile riêng lẻ, giảm overhead file.
*   Nếu ngân sách APK bị áp lực, có thể cân nhắc giữ **Decoration layer ở PPU 32** (không cần độ nét cao vì là chi tiết phụ, không tương tác) trong khi Ground/Collision layer giữ PPU 64 để đồng bộ với nhân vật — đánh đổi hợp lý giữa chất lượng và dung lượng.
