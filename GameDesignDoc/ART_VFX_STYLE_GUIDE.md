# Document Hướng Dẫn Mỹ Thuật & VFX (Art & VFX Style Guide) — Dự Án: VONG XUYÊN

**Phiên bản:** 1.0 (Được trích xuất và chuẩn hóa từ `ProjectZombie_GDD.md` v4.0)  
**Đối tượng sử dụng:** 2D Artist, VFX Artist, UI/UX Designer, Technical Artist (TA)  
**Nền tảng mục tiêu:** Android Mobile (Top-down 2D, Target API 33+, 60 FPS)  

---

## 1. Phong Cách Mỹ Thuật Tổng Thể (Art Direction & World Building)

### 1.1. Tầm Nhìn Mỹ Thuật & Nguyên Tắc Tương Phản (Visual Concept & Contrast Rules)
**Vong Xuyên** xây dựng bối cảnh Âm Ty Việt Nam — nơi truyền thuyết ma quái dân gian trỗi dậy.
*   **Phong cách đồ họa chủ đạo:** Mỹ thuật dân gian Việt Nam (Tranh Đông Hồ / Tranh Hàng Trống cách điệu) kết hợp với đường nét Anime hiện đại và tông màu u linh, huyền bí.
*   **Nguyên tắc tương phản Nền / Vật thể (Readability First):**
    *   *Nền / Môi trường:* Sử dụng tông màu **Tối và Desaturated** (xám xanh u uất, nâu bùn, tím đen) để truyền tải trọn vẹn chất "u linh" cõi âm ty.
    *   *Vật thể tương tác (Nhân vật / Yêu ma / Đạn bay / VFX):* Phải **Sáng và Saturated hơn nền rõ rệt (chênh lệch 1 – 2 bậc độ sáng / Luminance)**. Đây là quy tắc bắt buộc để mắt người chơi tự động phân biệt ngay "vật thể tương tác được" khỏi nền tĩnh trong loạn chiến đông quái (150-200 Zombie).

### 1.4. Quy Chuẩn Góc Nhìn, Tỉ Lệ & Kích Thước Sprite (Perspective & Sprite Specs)
*   **Góc nhìn (Perspective):** **Top-down 3/4 view** (không phải góc top-down thuần 90° từ trên đỉnh đầu xuống). Góc nhìn 3/4 cho phép thấy rõ mặt trước, biểu cảm và trang phục dân gian Việt Nam (áo the, khăn đóng, áo cà sa, pháp bảo).
*   **Tỉ lệ cơ thể (Body Ratio):** **Chibi hóa nhẹ (Tỉ lệ đầu:thân khoảng 1:3 đến 1:4)**. Giữ silhouette (bóng dáng) dễ đọc khi màn hình cực kỳ đông quái (target 200 enemy đồng thời theo GDD mục 9), đồng thời đủ chỗ vẽ chi tiết họa tiết mà không cần đẩy độ phân giải quá cao.
*   **Kích thước Sprite (Canvas Budget) & PPU:**
    *   *Canvas Nhân vật / Quái thường:* **32×32px đến 48×48px**.
    *   *Tilemap Môi trường:* **16×16px hoặc 32×32px**.
    *   *Pixels Per Unit (PPU):* **`32`** (`1 Unit = 1m = 32px`). Giúp 1 Sprite 32x32px có kích thước đúng 1m x 1m trong World Space, tối ưu va chạm Physics2D và Tilemap Grid.
    *   *Giới hạn:* Tuyệt đối không vượt quá **64px** (ngoại trừ Trùm / Boss) để giữ Sprite Atlas gọn nhẹ, bảo đảm target APK/AAB gói phát hành dưới 60MB.
*   **Cấu hình Camera (Landscape Mode):**
    *   *Màn hình:* Màn hình ngang (Landscape).
    *   *Projection:* **Orthographic**.
    *   *Orthographic Size:* **`6.0`** (Tầm nhìn chiều cao `12m`, tương đương 384px Reference Vertical).
    *   *Pixel Perfect Camera:* Reference Resolution `768 × 432` hoặc `640 × 360`, PPU `32`, Filter Mode `Point (no filter)`.
*   **Viền đen mỏng 1px (Outline Rule):** Vẽ viền đen mỏng 1px bao quanh nhân vật và quái vật để tách biệt hoàn toàn khỏi nền môi trường tối, tăng độ nhận diện (readability) mà không cần đẩy độ sáng của sprite lên quá cao.

### 1.5. Hệ Thống Hướng Di Chuyển — Flip Trái / Phải 2 Hướng (Directional Flip System)

*   **Quyết định Thiết kế:** **Chỉ dùng 2 hướng Trái / Phải** (bỏ thiết kế 4/8 hướng Lên/Xuống riêng biệt). Phù hợp với tiết tấu di chuyển liên tục của trò chơi survivor auto-battler, đồng thời giảm 60-70% khối lượng vẽ cho Art Team.
*   **Triển khai Kỹ thuật:**
    1. Animator chỉ vẽ **1 bộ animation gốc duy nhất theo hướng quay sang Phải (Facing Right)**.
    2. C# Controller sử dụng `SpriteRenderer.flipX = true` (Unity) để tự động lật ngang khi nhân vật di chuyển hoặc ngắm bắn sang Trái.
    3. Khi di chuyển theo hướng Lên/Xuống: Giữ nguyên bộ sprite ngang (Trái hoặc Phải tùy theo hướng nhìn ngang gần nhất trước đó). Chấp nhận compromise *"nhân vật trông ngang khi đi dọc"* — đây là chuẩn thiết kế phổ biến và tối ưu hàng đầu ở thể loại Top-down Roguelite.
*   **Lưu ý Bắt Buộc khi Thiết kế Trang Phục & Pháp Bảo (Anti-Flip Glitch Rules):**
    *   *Thiết kế đối xứng:* Ưu tiên thiết kế trang phục/phụ kiện đối xứng (không có họa tiết thêu lệch một bên hoặc biểu tượng dán cố định bên trái/phải mang ý nghĩa đặc thù) để tránh hình ảnh vô lý khi lật ngang `flipX`.
    *   *Vũ khí/Pháp bảo cầm tay:* Chấp nhận việc tay cầm vũ khí sẽ đổi từ tay phải sang tay trái khi lật `flipX` (không vẽ version cầm tay riêng) để tối ưu thời gian sản xuất.

### 1.2. Quy Chuẩn Bảng Màu Ngũ Hành & Ký Hiệu Hình Khối (Colorblind Accessibility)

Để hỗ trợ khả năng truy cập (Accessibility) và tránh rủi ro nhầm lẫn cho người mù màu (đặc biệt giữa cặp Hỏa/Mộc), **MỖI HỆ NGŨ HÀNH LUÔN CÓ HÌNH KHỐI ĐỘC LẬP ĐI KÈM MÀU SẮC** — không bao giờ dùng màu làm phương tiện phân biệt duy nhất:

| Hệ Ngũ Hành | Mã Màu Chính | Mã Màu Phụ / Glow | Ký Hiệu Hình Khối (Colorblind Icon) | Ứng Dụng Visual & VFX |
|---|---|---|---|---|
| ✦ **Kim** | `#E8C468` | `#FFF3C4` | 🔷 **Hình Thoi / Lưỡi Kiếm** | Tia sáng thư pháp, nỏ thần, kim khí chói lóa, vết chém chí mạng |
| 🌿 **Mộc** | `#4C7A3D` | `#8FC97A` | 🔺 **Hình Lá / Tam Giác Nhọn** | Lá bùa trấn yêu, năng lượng tự nhiên, vòng xoay bùa cửu huyền |
| 🌊 **Thủy** | `#2E6E9E` | `#7FCBEA` | 💧 **Hình Giọt Nước** | Sét nước Long Vương, vũng giếng thiêng, độc Ma Da |
| 🔥 **Hỏa** | `#B8442C` | `#FF8A50` | 🔥 **Hình Ngọn Lửa** | Móng vuốt cáo lửa Cửu Vĩ, lựu đạn thần sa, lửa địa ngục |
| 🪨 **Thổ** | `#8A6A3E` | `#C9A876` | 🟩 **Hình Vuông / Khối Đất** | Sóng âm trống đồng Đông Sơn, chấn động đất nứt Võ Tăng |

---

### 1.3. Palette Mỹ Thuật Văn Hóa Dân Gian Gợi Ý (Vietnamese Folk Art Palette)

*   **Bộ màu truyền thống:** **Đỏ son, Vàng đất, Nâu gụ, Đen mực nho**.
*   **Quy tắc phân bổ họa tiết (Detail Distribution Rule):**
    *   Dồn các họa tiết phức tạp (mô phỏng Tranh Đông Hồ / Tranh Hàng Trống) vào **UI Canvas, Background Arena, và Trùm/Boss** (nơi kích thước Sprite đủ lớn để giữ trọn chi tiết mỹ thuật).
    *   Giữ đơn giản cho **Nhân vật & Quái thường** (pixel size nhỏ khi di chuyển trên màn hình Survival Roguelite để tránh rối mắt và giữ nhịp nhìn snappy).

---

## 2. Quy Chuẩn Thiết Kế Nhân Vật & Signature Skill VFX

### 2.1. Thư Sinh (Vũ khí: Bút Phán Quan — Hệ Kim)
*   **Archetype tham khảo:** Văn nhân / Scribe cổ trang Việt Nam — dáng người gầy, thư sinh, nho nhã, tay cầm Bút Phán Quan cỡ lớn.
*   **Tông màu chủ đạo:** Vàng kim (`#E8C468`) làm điểm nhấn rực rỡ trên nền áo the / khăn đóng màu trung tính (xám nhạt, trắng ngà).
*   **Chi tiết Idle:** Bút Phán Quan phát sáng nhẹ ánh vàng kim ở đầu bút khi đứng yên, thể hiện quyền năng "phán quyết".
*   **Signature Skill VFX — *"Phán Quyết Tiền Định"*:**
    *   *Nét Bút:* Vệt mực thư pháp nhòe (Ink Flow Dissolve) kết hợp ánh sáng Vàng Kim (`#FFD700` / `#E8C468`).
    *   *Biểu Tượng:* Triện Ấn Bát Quái / Chữ Nôm bùng nổ trên đầu trong 1.5s thể hiện hit ảo Tương Sinh.
*   **AI Concept Prompt Mẫu:**  
    `2D top-down game concept art, male Vietnamese scholar scribe, slim build, wearing traditional ao the and khan dong in light gray and ivory, holding a giant illuminated golden calligraphy brush glowing with yellow-gold (#E8C468) energy, mystical Vietnamese folk art style, high contrast on dark background, isolated game asset`

---

### 2.2. Đạo Sĩ (Vũ khí: Bùa Trấn Yêu — Hệ Mộc)
*   **Archetype tham khảo:** Đạo sĩ / Pháp sư cổ trang Việt Nam — áo choàng dài, râu tóc búi đạo gia, tay cầm phất trần hoặc lá bùa thần.
*   **Tông màu chủ đạo:** Xanh lá mộc (`#4C7A3D`) trên áo choàng, phối thêm nâu gụ ở phụ kiện (giỏ đựng bùa, dây lưng vải).
*   **Chi tiết Idle:** Vài lá bùa giấy màu vàng dán chữ đỏ bay lửng lơ quanh người ở trạng thái idle — báo hiệu trực quan cho kỹ năng *"Bát Quái Trận Đồ"*.
*   **Signature Skill VFX — *"Bát Quái Trận Đồ"*:**
    *   *Mặt Đất:* Trận đồ 8 cạnh xoay tròn bán kính 4.5m màu Xanh Mộc (`#32CD32` / `#4C7A3D`).
    *   *Linh Phù:* 8 lá bùa bay lơ lửng tại 8 đỉnh kết nối bằng vệt sáng phong ấn.
    *   *Âm Dương:* 2 luồng khí Hắc Khí (Đen) & Bạch Khí (Trắng) xoáy từ mép cuộn vào tâm.
*   **AI Concept Prompt Mẫu:**  
    `2D top-down game concept art, male Vietnamese Taoist sorcerer exorcist, wearing dark green (#4C7A3D) robes and brown leather accessories, glowing yellow paper talismans floating in orbit around his body, mystical Vietnamese folk art style, vibrant HDR contrast, isolated game asset`

---

### 2.3. Võ Tăng (Vũ khí: Thiền Trượng — Hệ Thổ)
*   **Archetype tham khảo:** Tăng nhân võ thuật / Warrior Monk — dáng người chắc khỏe, cơ bắp cuồn cuộn, tay cầm Thiền Trượng đồng, ngực trần hoặc khoác áo cà sa lệch vai gọn gàng.
*   **Tông màu chủ đạo:** Nâu đất (`#8A6A3E`) chủ đạo, điểm nhấn đỏ son (`#C0392B`) ở dây chuỗi hạt tràng và khăn quấn cổ tay.
*   **Chi tiết Idle:** Tư thế idle hơi khom, thủ thế vững chãi — gợi ý sức mạnh vật lý càn quét, phù hợp với cơ chế hy sinh HP đổi lấy sát thương bộc phát.
*   **Signature Skill VFX — *"Phá Giới Chấn Thế"*:**
    *   *Mặt Đất:* Sprite vết nứt đất 8 hướng bộc phát tỏa rộng (3.0m - 7.0m tùy lượng HP hy sinh).
    *   *Sóng Xung Kích:* Sóng ring wave màu Đỏ Cam / Nâu Đất bộc phát cực nhanh đẩy lùi quái.
*   **AI Concept Prompt Mẫu:**  
    `2D top-down game concept art, muscular Vietnamese warrior monk, shirtless with earthen brown (#8A6A3E) martial trousers, large wooden prayer beads with crimson red accent ribbons, holding a heavy bronze monk spade staff, aggressive battle stance, mystical Vietnamese folk art style, isolated game asset`

### 2.4. Quy Chuẩn Khối Lượng Animation Tối Thiểu per Nhân Vật (Animation Budget)

Để đảm bảo hiệu năng 60 FPS Mobile và tối ưu hóa thời gian sản xuất cho Animator, mọi nhân vật chính MVP được thiết lập số lượng frame tối thiểu cho từng trạng thái:

| Animation State | Số Frame Gợi Ý | Ghi Chú Kỹ Thuật & Tối Ưu |
|---|---|---|
| **Idle** | 2 – 4 frames | Nhịp thở / nhún nhẹ, phát sáng phụ kiện |
| **Walk / Move** | 4 – 6 frames | Dáng bước di chuyển 8 hướng / 4 hướng |
| **Attack / Signature Skill** | 3 – 5 frames | Đòn đánh nhanh (Snappy 0.15s - 0.3s) |
| **Hit-react** | 1 – 2 frames | **Ưu tiên dùng Shader Flash Trắng (`HitFlashShader`)** thay vì tạo animation riêng để tiết kiệm bộ nhớ Sprite Sheet |
| **Death** | 3 – 4 frames | Ngã xuống / tan thành linh hồn |

> [!TIP]
> **Khuyến nghị sản xuất (Production Best Practice):** 
> Sử dụng chung **1 Base Rig / Tỉ lệ cơ thể chuẩn (Base Humanoid Skeleton/Ratio)** cho cả 3 nhân vật MVP (chỉ thay đổi trang phục, phụ kiện và palette màu). Việc này cho phép tái sử dụng toàn bộ Animation Logic C# (`animator.Play()`), loại bỏ Animator Transitions rối rắm và giảm tải 60% khối lượng công việc cho Animator.

---

## 3. Quy Chuẩn Thiết Kế Yêu Ma & Trùm (Enemies & Bosses)

### 3.1. Danh Sách Yêu Ma Dân Gian (MVP)
*   **Ma Giáp (Kim):** Binh lính ma mặc giáp sắt u uất, đi chậm, số lượng đông bao vây.
*   **Ma Trơi (Hỏa):** Đốm lửa lập lòe màu đỏ cam lao nhanh áp sát.
*   **Quỷ Nhập Tràng (Thổ):** Xác chết trâu bò to lớn (Elite Cản Đạn), màu nâu đất trầm, đi chậm cản đạn xuyên.
*   **Ma Da (Thủy):** Ma dưới nước ướt át màu xanh rêu/lam, phun nước độc từ xa.
*   **Hồ Ly Tinh Nhỏ (Hỏa):** Yêu cáo nhỏ rực lửa lao vào tự nổ diện rộng.

### 3.2. Boss & Cán Cân Âm Dương Indicator
*   **Ngưu Đầu Mã Diện (Boss Phút 10):** Tướng quỷ đầu trâu & mặt ngựa u linh. Viền hào quang đổi màu Thổ (Nâu) / Hỏa (Đỏ) mỗi 10s.
*   **Diêm Vương (Final Boss Phút 20):** Trang phục hoàng đế âm ty uy nghiêm. Viền thuộc tính xoay vòng đủ 5 màu Ngũ Hành phía trên thanh HP.

---

## 4. Chuyển Màu Không Khí Màn Chơi (Atmosphere Palette-Swap)

Bản đồ **Bến Đò Vong Xuyên** (60m x 60m) tự động chuyển tông màu môi trường theo 4 giai đoạn trận đấu qua Post-Processing & Color Grading mà 0 tốn bộ nhớ GPU:

1.  `00:00 – 05:00` — **Sương Mờ U Linh:** Tông màu xanh chàm u tối, sương mù bao phủ.
2.  `05:00 – 10:00` — **Âm Phong Hoàng Tuyền:** Tông lá úa sương mờ vàng nhạt.
3.  `10:00 – 19:59` — **Bão Hắc Khí Huyết Nguyệt:** Tông màu đỏ thẫm u uất, trăng máu xuất hiện.
4.  `20:00` — **Hỏa Ngục Địa Môn:** Vùng biên rực cháy tông đỏ cam địa ngục.

---

## 5. Quy Chuẩn Kỹ Thuật VFX & Asset 2D Mobile (Technical Art Rules)

### 5.1. Kiến Trúc 4 Layer Modular VFX (Anti-Ghosting / 0 GC Allocation)
Mọi hiệu ứng kỹ năng / vũ khí bắt buộc phải được tạo bằng Editor Tool `Tools > VFX Generator > Create Modular VFX Hierarchy` và phân tách rõ 4 Category:

1.  **SignatureSkill:** Prefab trọn gói cho kỹ năng diện rộng (`GroundDecal`, `AuraSwirl`, `SkillMain`).
2.  **WeaponAttack:** Prefab vệt chém / vung vũ khí. **TUYỆT ĐỐI KHÔNG** lồng `HitImpact` vào Prefab này.
3.  **BulletProjectile:** Prefab đầu đạn & trail bay. **TUYỆT ĐỐI KHÔNG** lồng `HitImpact` vào Prefab này.
4.  **HitImpact:** Prefab nổ va chạm **ĐỘC LẬP** được spawn tại vị trí trúng quái từ Object Pool tập trung (`GlobalVFXPoolManager`).

> [!IMPORTANT]
> **Quy tắc bắt buộc:** Mọi Root GameObject của VFX phải chứa script **`VFXPoolResetter.cs`** để tự động reset Particle System & TrailRenderer khi thu hồi về Pool, triệt tiêu 100% lỗi dính particle rác.

### 5.2. Pipeline Xử Lý Texture & Material
*   **Sinh & Tách Nền Texture:** Texture VFX sinh bằng AI phải được chạy qua script Python `.agents/skills/unity-image-pipeline/scripts/process_sprites.py` để tách nền trong suốt dạng **Un-premultiplied Glow** (Min Alpha = 0, Max Alpha > 100) và crop sát Bounding Box.
*   **Import Settings:** `Texture Type = Default` (Texture2D), `Alpha Is Transparency = True`, Nén Texture format **ASTC 6x6** cho Android.
*   **Material:** Particle System Renderer **chỉ dùng file Material (`.mat`)** URP Additive / AlphaBlend, tuyệt đối không kéo trực tiếp file ảnh `.png` vào Renderer!

### 5.3. Quy Ước Đặt Tên Assets (Naming Convention)
*   **Prefab VFX:** `PF_Skill_{Name}`, `PF_Weapon_{Name}`, `PF_Bullet_{Name}`, `PF_Impact_{Name}`
*   **Material:** `MAT_Additive_{Element/Effect}.mat`
*   **Texture Sprite:** `SPR_{Skill/Weapon}_{Layer}.png`
*   **Particle Sub-GameObject:** `PS_GroundDecal`, `PS_AuraSwirl`, `PS_SkillMain`, `PS_SparksBurst`, `PS_ImpactBurst`
