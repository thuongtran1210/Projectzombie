# Document Hướng Dẫn Mỹ Thuật & VFX (Art & VFX Style Guide) — Dự Án: VONG XUYÊN

**Phiên bản:** 1.0 (Được trích xuất và chuẩn hóa từ `ProjectZombie_GDD.md` v4.0)  
**Đối tượng sử dụng:** 2D Artist, VFX Artist, UI/UX Designer, Technical Artist (TA)  
**Nền tảng mục tiêu:** Android Mobile (Top-down 2D, Target API 33+, 60 FPS)  

---

## 1. Phong Cách Mỹ Thuật Tổng Thể (Art Direction & World Building)

### 1.1. Tầm Nhìn Mỹ Thuật (Visual Concept)
**Vong Xuyên** xây dựng bối cảnh Âm Ty Việt Nam — nơi truyền thuyết ma quái dân gian trỗi dậy.
*   **Phong cách đồ họa chủ đạo:** Mỹ thuật dân gian Việt Nam (Tranh Đông Hồ / Tranh Hàng Trống cách điệu) kết hợp với đường nét Anime hiện đại và tông màu u linh, huyền bí.
*   **Không khí (Atmosphere):** U uất, ma mị nhưng không quá kinh dị u tối mà mang tính **tương phản rực rỡ (Vibrant HDR)** giữa ánh sáng phép thuật/pháp bảo và nền đất cõi âm u tối.

### 1.2. Quy Chuẩn Màu Sắc Nguyên Tố Ngũ Hành (Elemental Color Palette)
Tất cả VFX đòn đánh, pháp bảo, aura quái vật và giao diện bắt buộc tuân theo bảng mã màu HSL/HEX chuẩn hóa dưới đây:

| Thuộc tính | Mã HEX | Tên Màu | Ứng Dụng Visual & VFX |
|---|---|---|---|
| ✦ **Kim** | `#FFD700` | Vàng Kim / Ánh Thần | Tia sáng thư pháp, nỏ thần, kim khí chói lóa, vết chém chí mạng |
| 🌿 **Mộc** | `#32CD32` | Xanh Lá Cây / Linh Phù | Lá bùa trấn yêu, năng lượng tự nhiên, vòng xoay bùa cửu huyền |
| 🌊 **Thủy** | `#1E90FF` | Xanh Lam / Thủy Cung | Sét nước Long Vương, vũng giếng thiêng, độc Ma Da |
| 🔥 **Hỏa** | `#FF4500` | Đỏ Cam / Hỏa Diệm | Móng vuốt cáo lửa Cửu Vĩ, lựu đạn thần sa, lửa địa ngục |
| 🪨 **Thổ** | `#8B4513` | Nâu Đất / Trầm Trảm | Sóng âm trống đồng Đông Sơn, chấn động đất nứt Võ Tăng |

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
