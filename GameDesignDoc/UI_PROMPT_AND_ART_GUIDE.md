# Sổ Tay Prompt Sinh Ảnh & Quy Chuẩn Mỹ Thuật UI (UI Prompt & Art Generation Guide)

**Dự án:** VONG XUYÊN (Project Zombie)  
**Mục đích:** Cung cấp bộ Prompt AI chuẩn hóa $100\%$ cùng quy trình khóa phong cách (Style Consistency), giúp mọi AI Agent hoặc Artist sinh ra các màn hình UI, Background và Sprite 9-Slice đồng nhất tuyệt đối về phong cách mỹ thuật, nét vẽ và bảng màu.

---

## 🔒 1. Quy Tắc Cốt Lõi: Khóa Phong Cách Bất Biến (Style Anchor Rules)

Để tránh hiện tượng lệch phong cách giữa các lần sinh ảnh (Style Drift), **BẮT BUỘC PHẢI CHÈN ĐOẠN STYLE ANCHOR BLOCK VÀ NEGATIVE CONSTRAINTS DƯỚI ĐÂY VÀO MỌI PROMPT**:

### 🛡️ Đoạn Style Anchor Block (Không Được Thay Đổi)
```text
[STYLE ANCHOR - DO NOT CHANGE]:
Art style strictly identical to 2D stylized vector game art, Kingdom Rush chibi aesthetic, thick solid 4px dark charcoal outlines (#1A1615), 2-tone flat cell-shading, matte finish, ancient Vietnamese Dong Son bronze and carved dark mahogany wood UI frames, exact color palette of antique gold (#E8C468), cinnabar red (#B8442C), emerald jade (#4C7A3D) and dark mahogany (#5C3A21). Strictly 2D flat vector, NO 3D render, NO realism, NO soft airbrush gradients, high contrast on dark underworld background.
```

### 🚫 Bộ Từ Khóa Loại Trừ (Negative Constraints)
```text
Exclude / Negative prompt: 3D render, hyperrealistic, CGI, glossy plastic, soft gradient blur, photorealism, modern futuristic UI, Western sci-fi, bevel embossed text, distorted anatomy, messy composition, low contrast.
```

---

## 🖼️ 2. Bộ Prompt Chi Tiết Từng Màn Hình Giao Diện (Full-Screen UI Prompts)

---

### 🏠 2.1. Màn Hình Sảnh Chính (Sảnh Hoàng Tuyền — Main Hub UI)
*   **Bố cục:** 16:9 Landscape. Header (Cổ Tiền/Linh Hồn) trên đỉnh, Tướng Chibi đứng trên bục đá Đông Sơn ở trung tâm, khay trang bị góc trái, 4 nút gỗ điều hướng ở giữa đáy, nút `XUẤT TRẬN` lục giác hổ phách phát sáng ở góc phải.

```text
2D mobile game Main Hub UI interface screenshot, top-down 2.5D perspective, 16:9 landscape orientation. 

Atmospheric mythical Vietnamese underworld riverbank background at twilight with dark weeping willow silhouettes, misty ghostly blue water, and tiny floating green spirit wisps. 

In the center: a cute chibi Vietnamese hero (1:2 body proportion, bold thick dark outline, 2-tone cell-shading, traditional gray ao the robe) standing on an ancient hexagonal stone pedestal engraved with Dong Son bronze drum sun patterns. Above the hero is an amber golden silk ribbon banner displaying decorative ancient text.

Top header: dark mahogany wood status bars showing golden coin currency and cyan soul gems, with an ancient wooden gear settings icon on the top right.

Bottom bar: four modular carved dark wood navigation buttons in the center, a small 2-slot wooden relic loadout tray on the bottom-left, and a large glowing hexagonal amber-gold battle button reading "XUẤT TRẬN" on the bottom-right with an crossed sword icon.

[STYLE ANCHOR - DO NOT CHANGE]:
Art style strictly identical to 2D stylized vector game art, Kingdom Rush chibi aesthetic, thick solid 4px dark charcoal outlines (#1A1615), 2-tone flat cell-shading, matte finish, ancient Vietnamese Dong Son bronze and carved dark mahogany wood UI frames, exact color palette of antique gold (#E8C468), cinnabar red (#B8442C), emerald jade (#4C7A3D) and dark mahogany (#5C3A21). Strictly 2D flat vector, NO 3D render, NO realism, NO soft airbrush gradients, high contrast on dark underworld background.
```

---

### 🌲 2.2. Nền Sảnh Chờ Tách Rời (Background Arena — Không Chứa UI)
*   **Dùng làm Asset:** `BG_VongXuyen_Forest_Hub.png`

```text
2D stylized mobile game background environment, 16:9 landscape view, mythical ancient Vietnamese underworld realm named Vong Xuyen riverbank. 

Dark misty forest with gnarled ancient banyan and willow trees, calm ethereal dark teal-gray river flowing quietly in the background, faint glowing jade-green will-o'-the-wisps floating in the air. In the foreground center, an ancient round ritual stone platform carved with Dong Son bronze drum geometric sun engravings, illuminated by warm subtle ground lanterns. 

[STYLE ANCHOR - DO NOT CHANGE]:
Art style strictly identical to 2D stylized vector game art, Kingdom Rush chibi aesthetic, thick solid 4px dark charcoal outlines (#1A1615), 2-tone flat cell-shading, matte finish, ancient Vietnamese Dong Son bronze and carved dark mahogany wood UI frames, exact color palette of antique gold (#E8C468), cinnabar red (#B8442C), emerald jade (#4C7A3D) and dark mahogany (#5C3A21). Strictly 2D flat vector, NO 3D render, NO realism, NO soft airbrush gradients, high contrast on dark underworld background. No characters, no text.
```

---

### 🗡️ 2.3. Màn Hình Chọn Trang Bị (Tàng Bảo Các — Weapon / Relic Loadout)
*   **Bố cục:** Chia 2 Cột. Cột Trái là lưới 12-17 ô vuông trang bị gỗ mộc; Cột Phải là bảng giấy điệp chi tiết chỉ số (Sát thương, Hồi chiêu) và nút Xuất Trận.

```text
2D mobile game Weapon Loadout and Armory UI screen, top-down 2.5D view, 16:9 landscape orientation. 

Atmospheric ancient Vietnamese imperial armory vault with dark lacquered wood walls and subtle glowing red lanterns. 

Left column: a neat 3x4 inventory grid of dark wooden square item frames with Dong Son brass borders, holding stylized mythical weapons and slapstick relics (golden crossbow, glowing talisman scroll, flaming fox claws, bronze drum, honeycomb sandals, bamboo smoking pipe). The selected slot has an intense glowing amber border.

Right column: an ancient parchment scroll detail inspection panel displaying a large illustrated icon of the Honeycomb Sandals relic surrounded by swirling golden speedlines, Vietnamese calligraphy title, two antique brass stat gauges with glowing red and gold fill bars, and crisp stat text. At the bottom-right is a large glowing hexagonal battle button reading "XUẤT TRẬN".

[STYLE ANCHOR - DO NOT CHANGE]:
Art style strictly identical to 2D stylized vector game art, Kingdom Rush chibi aesthetic, thick solid 4px dark charcoal outlines (#1A1615), 2-tone flat cell-shading, matte finish, ancient Vietnamese Dong Son bronze and carved dark mahogany wood UI frames, exact color palette of antique gold (#E8C468), cinnabar red (#B8442C), emerald jade (#4C7A3D) and dark mahogany (#5C3A21). Strictly 2D flat vector, NO 3D render, NO realism, NO soft airbrush gradients, high contrast on dark underworld background.
```

---

### 🧑 2.4. Màn Hình Chọn Anh Hùng (Điện Anh Hùng — Hero Selection)
*   **Bố cục:** Sân khấu Tướng Chibi 1:2 đứng trên bục đá ở bên trái/giữa kèm 2 nút mũi tên chuyển tướng; Bảng thuộc tính RPG, tuyệt kỹ chủ động và nút `CHỌN ANH HÙNG` ở bên phải.

```text
2D mobile game Character Selection UI screen, 16:9 landscape layout, top-down 2.5D perspective. 

Left side: a large center stage showing a cute chibi Vietnamese Hau Dong priestess (Thanh Dong) in an ornate red and gold silk ceremonial robe, holding a pink bamboo folding fan and a flowing green silk ribbon, standing atop an ancient Dong Son bronze drum pedestal. Two carved wooden arrow buttons flank the character for switching heroes. Above her head is a floating golden ribbon banner with Vietnamese calligraphy.

Right side: a vertical dark carved wood parchment panel framed with Vietnamese mythical cloud motifs, displaying RPG character stats with colorful elemental icons, a glowing active skill box titled "Loan Phượng Trảm Tà" with skill description, and a prominent golden-amber button at the bottom reading "CHỌN ANH HÙNG".

[STYLE ANCHOR - DO NOT CHANGE]:
Art style strictly identical to 2D stylized vector game art, Kingdom Rush chibi aesthetic, thick solid 4px dark charcoal outlines (#1A1615), 2-tone flat cell-shading, matte finish, ancient Vietnamese Dong Son bronze and carved dark mahogany wood UI frames, exact color palette of antique gold (#E8C468), cinnabar red (#B8442C), emerald jade (#4C7A3D) and dark mahogany (#5C3A21). Strictly 2D flat vector, NO 3D render, NO realism, NO soft airbrush gradients, high contrast on dark underworld background.
```

---

### ⚡ 2.5. Màn Hình Chọn Thẻ Nâng Cấp Trong Trận (In-Game Level-Up Modal)
*   **Bố cục:** Pop-up dừng thời gian giữa trận, hiển thị 3 Thẻ Nâng Cấp Totem gỗ mộc đặt song song kèm nút `ĐỔ LẠI` và `BỎ QUA`.

```text
2D mobile game Level-Up Upgrade Selection UI modal, 16:9 landscape format, centered pop-up over a blurred dark moody combat arena background. 

Header: an ancient golden parchment banner on top reading "THIÊN CƠ ĐỘT PHÁ" with floating spiritual golden spark embers. 

Center: 3 large vertical wooden tarot-style upgrade cards arranged side by side. Each card has an intricate dark mahogany wood border with engraved Dong Son bronze drum geometric patterns, a glowing circular badge on top (New Weapon / Stat Augment / Ultimate Evolution), a colorful stylized Chibi icon in the upper half, bold Vietnamese title, 5-star level progression bar, and crisp effect description text. 

Bottom: two smaller rectangular carved wood utility buttons for "REROLL" with a dice icon and "SKIP" with an arrow icon.

[STYLE ANCHOR - DO NOT CHANGE]:
Art style strictly identical to 2D stylized vector game art, Kingdom Rush chibi aesthetic, thick solid 4px dark charcoal outlines (#1A1615), 2-tone flat cell-shading, matte finish, ancient Vietnamese Dong Son bronze and carved dark mahogany wood UI frames, exact color palette of antique gold (#E8C468), cinnabar red (#B8442C), emerald jade (#4C7A3D) and dark mahogany (#5C3A21). Strictly 2D flat vector, NO 3D render, NO realism, NO soft airbrush gradients, high contrast on dark underworld background.
```

---

### ⛩️ 2.6. Màn Hình Miếu Tứ Bất Tử (Cây Thiên Phú Vĩnh Viễn — Meta Talent Tree)
*   **Bố cục:** 3 Tab đền miếu (Tản Viên, Phù Đổng, Liễu Hạnh), cây chòm sao kỹ năng nối bằng xích đồng cổ phát quang, bảng chi phí và nút Nâng Cấp màu ngọc bích.

```text
2D mobile game Meta Talent Tree UI screen named "Miếu Tứ Bất Tử", 16:9 landscape orientation. 

Top header: 3 ornate wooden shrine tabs with traditional Vietnamese pagoda roofs representing the Three Immortals (Tan Vien God of Attack, Phu Dong God of Defense, Lieu Hanh Goddess of Fortune). 

Center canvas: an interactive constellation skill tree with circular brass talisman nodes interconnected by glowing golden spirit lines on dark parchment. Unlocked nodes glow with radiant golden and cyan elemental light, while locked nodes are dark iron silhouettes. 

Bottom bar: currency counter showing golden ancient coins, a detail pop-up card on the right showing the selected talent cost, and a large green jade "NÂNG CẤP" upgrade button.

[STYLE ANCHOR - DO NOT CHANGE]:
Art style strictly identical to 2D stylized vector game art, Kingdom Rush chibi aesthetic, thick solid 4px dark charcoal outlines (#1A1615), 2-tone flat cell-shading, matte finish, ancient Vietnamese Dong Son bronze and carved dark mahogany wood UI frames, exact color palette of antique gold (#E8C468), cinnabar red (#B8442C), emerald jade (#4C7A3D) and dark mahogany (#5C3A21). Strictly 2D flat vector, NO 3D render, NO realism, NO soft airbrush gradients, high contrast on dark underworld background.
```

---

### 🗡️ 2.7. Màn Hình Bách Bảo Các & Sơ Đồ Ngũ Hành (Codex / Elemental Chart)
*   **Bố cục:** Vòng tròn Bát Quái Ngũ Hành tương sinh tương khắc ở bên trái; Danh mục thẻ bài Pháp bảo và công thức Tiến Hóa ở bên phải.

```text
2D mobile game Codex and Elemental Lore UI screen, 16:9 landscape orientation. 

Left side: a large glowing Yin-Yang Bagua elemental wheel showing the 5 Vietnamese elements (Golden Metal, Emerald Wood, Blue Water, Fiery Fire, Earthen Stone) connected by glowing green generative cycles (Tương Sinh) and red destructive cycle arrows (Tương Khắc). 

Right side: a scrollable codex grid displaying cards of all 17 unlocked Mythological and Slapstick Relics with detailed lore, evolution recipes, and discovery status. 

[STYLE ANCHOR - DO NOT CHANGE]:
Art style strictly identical to 2D stylized vector game art, Kingdom Rush chibi aesthetic, thick solid 4px dark charcoal outlines (#1A1615), 2-tone flat cell-shading, matte finish, ancient Vietnamese Dong Son bronze and carved dark mahogany wood UI frames, exact color palette of antique gold (#E8C468), cinnabar red (#B8442C), emerald jade (#4C7A3D) and dark mahogany (#5C3A21). Strictly 2D flat vector, NO 3D render, NO realism, NO soft airbrush gradients, high contrast on dark underworld background.
```

---

### 📊 2.8. Màn Hình Tổng Kết Trận Đấu (Run Summary / Game Over)
*   **Bố cục:** Biểu tượng Chiến Thắng / Thất Thủ trên đỉnh, bảng cuộn thống kê thời gian sống sót, số quái diệt, vàng nhận được và nút `TRỞ VỀ SẢNH`.

```text
2D mobile game Victory / Defeat Run Summary UI screen, 16:9 landscape format, centered over a misty dark underworld battlefield. 

Top: a grand ceremonial wooden crest reading "ĐẠI THẮNG QUỶ QUÂN" (Victory) adorned with golden phoenix wings and red silk tassels. 

Center: a detailed parchment report scroll displaying run statistics (Survival Time, Total Kills, Damage Dealt, Ancient Coins Earned) with small colorful icons, and a row of icons showing all weapons evolved during the run. 

Bottom: two large carved wooden buttons for "RETRY" and "RETURN TO HUB" in glowing green and amber colors.

[STYLE ANCHOR - DO NOT CHANGE]:
Art style strictly identical to 2D stylized vector game art, Kingdom Rush chibi aesthetic, thick solid 4px dark charcoal outlines (#1A1615), 2-tone flat cell-shading, matte finish, ancient Vietnamese Dong Son bronze and carved dark mahogany wood UI frames, exact color palette of antique gold (#E8C468), cinnabar red (#B8442C), emerald jade (#4C7A3D) and dark mahogany (#5C3A21). Strictly 2D flat vector, NO 3D render, NO realism, NO soft airbrush gradients, high contrast on dark underworld background.
```

---

### ⏳ 2.9. Màn Hình Loading & Khởi Động / Chuyển Cảnh (Loading Screen & Scene Transition)
*   **Bối cảnh:** Cánh cổng Địa Phủ / Cửa Hoàng Tuyền uy nghiêm hé mở giữa làn sương khói cõi âm.
*   **Bố cục:** 16:9 Landscape. Chính giữa phía dưới là thanh tiến trình năng lượng ngũ hành khảm đồng Đông Sơn (`Bar_HUD_Frame_VongXuyen_9Slice`), bánh xe Bát Quái / Trống Đồng xoay tròn linh động, % tiến trình và dòng chữ bí kíp dân gian / quy luật tương sinh tương khắc chạy ngẫu nhiên.

```text
2D mobile game Loading Screen and Scene Transition UI, 16:9 landscape format. 

Atmospheric mythical Vietnamese underworld entrance gate at twilight: a colossal ancient dark stone and iron pagoda gate with carved mythical dragon-turtle reliefs, faint eerie cyan fog rolling across the ground, and glowing amber lantern lights. 

Center-bottom: a long ornate horizontal progress bar with ancient Dong Son bronze geometric borders and glowing golden-jade energy fill (showing 75% loading), a rotating Yin-Yang Bagua talisman wheel spinner on the left of the bar, and golden percentage text. 

Below the progress bar: a floating ancient parchment ribbon displaying a mystical gameplay tip: "⚔️ QUY LUẬT TƯƠNG KHẮC: Đánh trúng hệ khắc chế gây thêm +30% Sát thương!" with golden calligraphy typography.

[STYLE ANCHOR - DO NOT CHANGE]:
Art style strictly identical to 2D stylized vector game art, Kingdom Rush chibi aesthetic, thick solid 4px dark charcoal outlines (#1A1615), 2-tone flat cell-shading, matte finish, ancient Vietnamese Dong Son bronze and carved dark mahogany wood UI frames, exact color palette of antique gold (#E8C468), cinnabar red (#B8442C), emerald jade (#4C7A3D) and dark mahogany (#5C3A21). Strictly 2D flat vector, NO 3D render, NO realism, NO soft airbrush gradients, high contrast on dark underworld background.
```

---

## ✂️ 3. Bộ Prompt Tạo Asset Rời (Sprite Sheet 9-Slice & Buttons trên nền đen `#000000`)

Dùng để sinh các thành phần UI độc lập phục vụ bóc tách sprite và cài đặt 9-Slice trong Unity:

### 📦 Prompt: Bảng Texture Khung Gỗ 9-Slice & Nút Bấm (UI Kit 2x2 Grid)
```text
2D mobile game UI asset kit sheet, arranged in a neat 2x2 grid on a pure solid black background (#000000):

Top-left: an ornate hexagonal battle button made of dark wood with glowing amber-gold bronze edges and two crossed swords engraved in the center.
Top-right: a set of 4 horizontal dark mahogany wood navigation buttons with carved Dong Son geometric feather borders and glowing yellow icons.
Bottom-left: a decorative ancient Vietnamese wooden totem frame for hero avatars with a golden scroll ribbon on top.
Bottom-right: an ancient stone pedestal viewed from 2.5D angle with detailed bronze drum sun relief patterns.

[STYLE ANCHOR - DO NOT CHANGE]:
Art style strictly identical to 2D stylized vector game art, Kingdom Rush chibi aesthetic, thick solid 4px dark charcoal outlines (#1A1615), 2-tone flat cell-shading, matte finish, ancient Vietnamese Dong Son bronze and carved dark mahogany wood UI frames, exact color palette of antique gold (#E8C468), cinnabar red (#B8442C), emerald jade (#4C7A3D) and dark mahogany (#5C3A21). Strictly 2D flat vector, NO 3D render, NO realism, NO soft airbrush gradients, high contrast. Isolated clean game assets, no text.
```

---

## 🛠️ 4. Quy Trình Sử Dụng Ảnh Tham Chiếu (Image-to-Image Workflow)

Khi làm việc với Gemini, Midjourney hoặc Photoshop Generative AI:
1. **Bước 1:** Tải bức ảnh UI Màn hình Chính đã ưng ý lên làm **Image Reference**.
2. **Bước 2:** Dán một trong các Prompt ở Mục 2 tương ứng với màn hình cần tạo.
3. **Bước 3:** Bổ sung câu lệnh điều hướng:
   > *"Duy trì 100% phong cách vẽ vector 2D, viền đen dày 4px, chất liệu gỗ Đông Sơn và bảng màu như ảnh tham chiếu, chỉ thay đổi bố cục sang [Tên màn hình mới] theo mô tả."*
