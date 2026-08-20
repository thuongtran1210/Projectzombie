# Document Hướng Dẫn Mỹ Thuật & VFX (Art & VFX Style Guide) — Dự Án: VONG XUYÊN

**Phiên bản:** 1.0 (Được trích xuất và chuẩn hóa từ `ProjectZombie_GDD.md` v4.0)  
**Đối tượng sử dụng:** 2D Artist, VFX Artist, UI/UX Designer, Technical Artist (TA)  
**Nền tảng mục tiêu:** Android Mobile (Top-down 2D, Target API 33+, 60 FPS)  

---

## 1. Phong Cách Mỹ Thuật Tổng Quan & Visual DNA

### 1.1. Tầm Nhìn Mỹ Thuật & Art Master DNA (Visual Concept & Art DNA)
**Vong Xuyên** xây dựng bối cảnh Âm Ty Việt Nam — nơi truyền thuyết ma quái dân gian trỗi dậy.
*   **Art Master DNA chủ đạo:** **2D Stylized Vector Cartoon / Cutout Chibi** (Lấy cảm hứng từ phong cách kinh điển của *Kingdom Rush*, *Castle Crashers*, *Brawl Stars*) kết hợp họa tiết & bản sắc văn hóa dân gian Việt Nam (Đông Hồ, Hàng Trống, Cổ Phong Đông Sơn).
*   **Nguyên tắc viền đen đậm (Thick Solid Dark Outlines):** Đường viền ngoài dày $3\text{px} - 6\text{px}$ màu nâu đen ấm/than củi bao trọn toàn bộ silhouette nhân vật và quái vật để tách bạch $100\%$ khỏi nền cõi âm, triệt tiêu hoàn toàn hiện tượng chìm hình khi đông quái.
*   **Đổ bóng 2-Tone Cell-Shading:** 1 màu gốc (Base Color) + 1 lớp bóng tối (Shadow Tone) dạng mảng phẳng dứt khoát, kết hợp điểm xuyết vệt sáng Highlight tròn, không dùng gradient mờ.
*   **Nguyên tắc tương phản Nền / Vật thể (Readability First):**
    *   *Nền / Môi trường:* Tông màu **Tối và Desaturated** (xám xanh u uất, nâu bùn, tím đen).
    *   *Nhân vật / Yêu ma / VFX:* **Sáng, tương phản cao, saturated rõ rệt** với viền đen bao quanh.

### 1.2. Phân Tích Chi Tiết Art Style Bộ Nhân Vật Thực Tế (Character Art Style Breakdown)
Dựa trên kho 72 nhân vật thực tế đã chuẩn hóa tại `Assets/Art/Extracted_Characters/`:
1.  **Tỉ lệ cơ thể & Khối hình (Anatomy & Proportions):**
    *   *Tỉ lệ Chibi 1:1.5 đến 1:2 (Head-Dominant):* Đầu chiếm ~50% chiều cao tổng thể, có dạng hình hộp chữ nhật bo tròn mềm mại ("Squircle").
    *   *Thân & Tay Chân:* Thân hình trụ/thang nhỏ gọn. Tay chân dạng ống ngắn thon (Nub limbs), không vẽ ngón chân giúp tối ưu visual và giảm độ phức tạp cho animation.
2.  **Đặc trưng khuôn mặt & Biểu cảm (Facial Features):**
    *   *Nhân vật người / Dân gian / Anh hùng:* Mắt hạt tiêu (Dot/Bead eyes) tối giản, mang nét ngây ngô, bình dị nhưng kiên nghị.
    *   *Yêu ma / Quái vật:* Mắt xếch góc cạnh, tròng trắng phát sáng, miệng rộng lộ răng nhọn hung dữ tạo sự tương phản rõ nét với phe người.
3.  **Bản sắc văn hóa & Archetypes phân tầng:**
    *   *Dân gian & Lao động:* Áo bà ba, nón lá, khăn rằn Nam Bộ, đòn gánh gạo/lúa, mái chèo sông nước, đàn nguyệt.
    *   *Nghệ thuật truyền thống:* Mặt nạ Hát Bội / Tuồng cổ, Đầu Lân Sư Rồng, trống cơm, cành trúc.
    *   *Thần thoại & Tâm linh:* Ma Lai rút ruột, Ngưu Đầu Mã Diện, Thủy quỷ ma da tóc rêu, Cương thi quấn vải liệm, Thần đất, Hỏa ma.
    *   *Cung đình & Quân binh:* Mũ cánh chuồn quan lại, áo giao lĩnh/nhật bình hoàng tộc, giáp trụ lính vệ binh thời phong kiến.

### 1.3. Quy Chuẩn Bảng Màu Ngũ Hành & Ký Hiệu Hình Khối (Colorblind Accessibility)

Để hỗ trợ khả năng truy cập (Accessibility) và tránh rủi ro nhầm lẫn cho người mù màu (đặc biệt giữa cặp Hỏa/Mộc), **MỖI HỆ NGŨ HÀNH LUÔN CÓ HÌNH KHỐI ĐỘC LẬP ĐI KÈM MÀU SẮC** — không bao giờ dùng màu làm phương tiện phân biệt duy nhất:

| Hệ Ngũ Hành | Mã Màu Chính | Mã Màu Phụ / Glow | Ký Hiệu Hình Khối (Colorblind Icon) | Ứng Dụng Visual & VFX |
|---|---|---|---|---|
| ✦ **Kim** | `#E8C468` | `#FFF3C4` | 🔷 **Hình Thoi / Lưỡi Kiếm** | Tia sáng thư pháp, nỏ thần, kim khí chói lóa, vết chém chí mạng |
| 🌿 **Mộc** | `#4C7A3D` | `#8FC97A` | 🔺 **Hình Lá / Tam Giác Nhọn** | Lá bùa trấn yêu, năng lượng tự nhiên, vòng xoay bùa cửu huyền |
| 🌊 **Thủy** | `#2E6E9E` | `#7FCBEA` | 💧 **Hình Giọt Nước** | Sét nước Long Vương, vũng giếng thiêng, độc Ma Da |
| 🔥 **Hỏa** | `#B8442C` | `#FF8A50` | 🔥 **Hình Ngọn Lửa** | Móng vuốt cáo lửa Cửu Vĩ, lựu đạn thần sa, lửa địa ngục |
| 🪨 **Thổ** | `#8A6A3E` | `#C9A876` | 🟩 **Hình Vuông / Khối Đất** | Sóng âm trống đồng Đông Sơn, chấn động đất nứt Võ Tăng |

---

### 1.4. Palette Mỹ Thuật Văn Hóa Dân Gian & Nguyên Tắc Phân Bổ (Vietnamese Folk Art Palette)

*   **Bộ màu truyền thống:** **Đỏ son, Vàng đất, Nâu gụ, Đen mực nho, Xanh chàm**.
*   **Quy tắc phân bổ họa tiết (Detail Distribution Rule):**
    *   Dồn các họa tiết phức tạp (mô phỏng Tranh Đông Hồ / Tranh Hàng Trống) vào **UI Canvas, Background Arena, và Trùm/Boss** (nơi kích thước Sprite đủ lớn để giữ trọn chi tiết mỹ thuật).
    *   Giữ đơn giản cho **Nhân vật & Quái thường** (pixel size nhỏ khi di chuyển trên màn hình Survival Roguelite để tránh rối mắt và giữ nhịp nhìn snappy).

### 1.5. Quy Chuẩn Tỉ Lệ Cơ Thể, Góc Nhìn & Cấu Trúc Khối (Anatomy & Perspective)
*   **Tỉ lệ cơ thể (Body Ratio):** **Chibi $1:1.5$ đến $1:2$ (Head-Dominant)**:
    *   *Đầu (Head):* Chiếm **$45\% - 50\%$** tổng chiều cao nhân vật. Tập trung nhận diện đặc trưng (mắt to/hạt tiêu, biểu cảm cá tính, khăn đóng, tóc búi, mũ chầu, râu).
    *   *Thân (Torso):* Chiếm **$30\%$**, hình khối trụ / hình thang bo tròn đơn giản.
    *   *Tay & Chân (Limbs):* Ngắn, dạng ống bo tròn ở các khớp nối (**Ball-joint Modular Structure**), bàn chân phẳng tiếp đất vững chãi.
*   **Góc nhìn (Perspective):** **Frontal 3/4 Flat View (Chính diện chếch 3/4 phẳng)**:
    *   Thấy rõ toàn bộ mặt trước, biểu cảm, trang phục và hai chân đứng trên cùng một đường nằm ngang (**Ground Baseline**).
    *   Hai tay tách rời sang hai bên thân ở trạng thái trung tính, tối ưu hóa $100\%$ cho hệ thống 2D Sprite Rigging / Cutout Animation.
*   **Kích thước Sprite (Canvas Budget) & PPU:**
    *   *Pixels Per Unit (PPU):* **`64`** (hoặc `32`), Filter Mode: `Point (no filter)`.
    *   *Camera Orthographic Size:* **`6.0`**, Resolution Reference: `768 × 432` / `640 × 360`.

### 1.6. Hệ Thống Hướng Di Chuyển — Flip Trái / Phải 2 Hướng (Directional Flip System)
*   **Quy chuẩn vẽ gốc & Asset Export:** Toàn bộ sprite nhân vật đã bóc tách được xuất ở chuẩn **Facing Right (Quay mặt sang Phải)**.
*   **Lật hướng Runtime:** C# Controller lật toàn bộ `transform.localScale = new Vector3(-1, 1, 1)` trên Root Bone (hoặc `SpriteRenderer.flipX`) khi di chuyển sang Trái.
*   **Lưu ý Bắt Buộc khi Thiết kế Trang Phục & Pháp Bảo (Anti-Flip Glitch Rules):**
    *   *Thiết kế đối xứng:* Ưu tiên thiết kế trang phục/phụ kiện đối xứng để tránh hình ảnh vô lý khi lật ngang `flipX`.
    *   *Vũ khí/Pháp bảo cầm tay:* Chấp nhận việc tay cầm vũ khí sẽ đổi từ tay phải sang tay trái khi lật `flipX` để tối ưu thời gian sản xuất.

---

## 2. Quy Chuẩn Thiết Kế Nhân Vật & Signature Skill VFX

### 2.1. Thư Sinh (Vũ khí: Bút Phán Quan — Hệ Kim)
*   **Archetype tham khảo:** Hiền sĩ / Nho sinh bản địa Việt Nam — dáng người gầy, thư sinh, tay cầm Bút Phán Quan. Được anh linh sông núi & Đức Thánh Trần điểm hóa, dùng bút lệnh phán định tà ma.
*   **Tông màu chủ đạo:** Vàng kim (`#E8C468`) làm điểm nhấn rực rỡ trên nền áo the / khăn đóng màu xám nhạt hoặc trắng ngà.
*   **Chi tiết Idle:** Đầu Bút Phán Quan phát ra vệt khói thiêng nhè nhẹ ánh vàng kim khi đứng yên.
*   **Signature Skill VFX — *"Phán Quyết Tiền Định" / "Khí Thiêng Sông Núi" phát động:**
    *   *Nét Bút:* Vệt khói thiêng cuộn tròn (Spirit Smoke Dissolve) kết hợp ký tự Nôm/Nho rực cháy màu Vàng Kim (`#FFD700` / `#E8C468`).
    *   *Biểu Tượng:* Chữ Nôm bùng cháy thành luồng khói thiêng bay lơ lửng trên đầu trong 1.5s thể hiện hit ảo Tương Sinh.
*   **AI Concept Prompt Mẫu:**  
    `2D top-down game concept art, male Vietnamese scholar scribe, slim build, wearing traditional ao the and khan dong in light gray and ivory, holding a giant illuminated golden calligraphy brush with spirit smoke flowing, glowing yellow-gold (#E8C468) energy, mystical Vietnamese folk art style, high contrast on dark background, isolated game asset`

---

### 2.2. Thanh Đồng / Cô Đồng (Vũ khí: Bùa Trấn Yêu — Hệ Mộc)
*   **Archetype tham khảo:** Thầy Pháp / Bà Đồng hầu đồng Tứ Phủ — sắc phục rực rỡ, khăn chầu áo ngự, tay cầm Bùa Trấn Yêu / Chuỗi Bùa Tứ Phủ.
*   **Tông màu chủ đạo:** Biến đổi theo cõi Tứ Phủ được thỉnh nhập: Thiên Phủ (Đỏ son `#B8442C`), Nhạc Phủ (Xanh mộc `#4C7A3D`), Thoải Phủ (Trắng/Lam `#2E6E9E`), Địa Phủ (Vàng đất `#8A6A3E`).
*   **Chi tiết Idle:** Vài lá bùa Tứ Phủ thêu hoa văn cổ bay lơ lửng quanh người ở trạng thái idle.
*   **Signature Skill VFX — *"Giá Đồng" (Nhập Tứ Phủ):*
    *   *Múa Bóng:* Dáng múa bóng / múa mồi 2–3 frame đặc trưng kèm hiệu ứng dải lụa mồi lửa xoay quanh người.
    *   *Hào Quang Tứ Phủ:* Bùng nổ hào quang bán kính 4.5m đổi màu sắc rực rỡ đại diện cho cõi Tứ Phủ thỉnh nhập (Đỏ / Xanh / Lam / Vàng).
*   **AI Concept Prompt Mẫu:**  
    `2D top-down game concept art, female Vietnamese Hau Dong ritual priestess medium, wearing vibrant traditional ceremonial silk robes with ornate headdress, glowing talismans and multicolored mystic silk ribbons floating around her body, mystical Vietnamese folk art style, vibrant HDR contrast, isolated game asset`

---

### 2.3. Ẩn Sĩ Sơn Lâm (Vũ khí: Thiền Trượng / Gậy Sơn Lâm — Hệ Thổ)
*   **Archetype tham khảo:** Kỳ nhân ẩn dật / Đạo nhân núi — dáng người khỏe khoắn chững chạc, tay cầm Thiền Trượng đồng / Gậy Sơn Lâm bọc đồng, áo vải thô phong trần dật. Tự thiền định chốn đại ngàn, hòa hợp làm một với sinh khí đất trời.
*   **Tông màu chủ đạo:** Nâu đất (`#8A6A3E`) chủ đạo, điểm nhấn đỏ gụ (`#C0392B`) ở quấn tay và đai lưng.
*   **Chi tiết Idle:** Tư thế idle hơi khom, thủ thế vững chãi, gậy nạm đồng cắm nhẹ xuống đất.
*   **Signature Skill VFX — *"Thập Phương Chấn Thế" (Sơn Lâm Bộc Phát):*
    *   *Mặt Đất:* Sprite vết nứt đá đất núi ngàn tỏa rộng (3.0m - 7.0m tùy lượng HP hy sinh).
    *   *Sóng Xung Kích:* Luồng địa khí sơn lâm cuồn cuộn màu Nâu Đất / Vàng Đất bộc phát cực nhanh đẩy lùi quái.
*   **AI Concept Prompt Mẫu:**  
    `2D top-down game concept art, muscular Vietnamese mountain hermit sage, wearing rough earthen brown (#8A6A3E) robes with crimson red waist sash, holding a heavy copper-headed wooden mountain staff, grounded martial stance, earth energy bursting around feet, mystical Vietnamese folk art style, isolated game asset`

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
