# Phụ Lục GDD — Quy Chuẩn Hiển Thị Vũ Khí (Weapon Visual Manifestation Rules)
## Dự án: VONG XUYÊN

**Phiên bản:** 1.0 (Bổ sung cho GDD v4.0 — Mục 4 & ART_VFX_STYLE_GUIDE.md)  
**Mục đích:** Giải quyết bài toán hiển thị khi nhân vật mang tối đa 6 pháp bảo cùng lúc mà KHÔNG tăng khối lượng animation (loại bỏ bài toán vẽ animation riêng theo từng tổ hợp vũ khí). Quy định rõ pháp bảo hiển thị thế nào và Animator / Developer cần cài đặt gì.  

**Đối tượng sử dụng:** Game Designer, Artist / Animator, Technical Artist (TA), Developer.  

---

## 1. Nguyên Tắc Cốt Lõi

> **Nhân vật KHÔNG cầm vũ khí trong tay theo nghĩa vật lý.** Toàn bộ 12 pháp bảo là các "biểu hiện linh khí" (VFX + object độc lập) phát ra từ hoặc xoay quanh nhân vật. Animation nhân vật cố định ở 4 loại cơ bản (Idle, Walk, Hit-react, Death) + **tối đa 1 animation "Gesture" dùng chung** — không tăng theo số lượng vũ khí đang mang.

### Lý do nguyên tắc này bắt buộc:
1. **Tránh combinatorial explosion (bộc phát khối lượng art):** 1 nhân vật mang tối đa 6 pháp bảo/run $\rightarrow$ nếu vẽ animation riêng theo từng tổ hợp là hoàn toàn bất khả thi.
2. **Chuẩn mực ngành (Genre Standard):** Các tựa game survivor hàng đầu (*Vampire Survivors, Brotato, 20 Minutes Till Dawn*) đều áp dụng nguyên tắc này để tối ưu nhịp đấu.
3. **Phù hợp bối cảnh Lore Vong Xuyên:** Pháp bảo, linh khí, bùa chú trong cõi âm ty mang bản chất "tự vận hành / biến hóa theo linh lực" của người sở hữu mà không cần thao tác cầm nắm vật lý.

---

## 2. Phân Loại 12 Pháp Bảo Theo Cách Hiển Thị (Visual Manifestation Groups)

| Nhóm | Pháp Bảo | Cách Hiển Thị Visual | Animation Nhân Vật Cần | Vị Trí Spawn / Attach |
|---|---|---|---|---|
| **A — Tự triệu hồi** | Nỏ Thần (`W001`), Cửu Vĩ Hồ Trảo (`W004`), Cung Thạch Sanh (`W007`) | Không hiện trên người. 1 vòng ấn chú/ánh sáng lóe ngắn (0.1–0.15s) tại điểm phát, đạn/móng vuốt tự bay ra | **Không cần** — Giữ nguyên Idle / Walk | `FirePoint` — Empty GameObject ngang vai, hướng theo `flipX` hiện tại |
| **B — Gesture cận chiến / luồng** | Bút Phán Quan (`W002`), Đao Cửu Vĩ (`W008`) | 1 animation Gesture dùng chung (đưa tay ra trước). VFX (vệt chém mực / luồng lửa) overlay đúng lúc gesture xảy ra | **1 animation "Gesture" dùng chung** cho cả nhóm B | `WeaponSocket` — Empty GameObject trước ngực/tay, VFX phát từ đây |
| **C — Orbit quanh người** | Bùa Trấn Yêu (`W003`), Phi Tiêu Bát Quái (`W012`) | Object bay lơ lửng quanh nhân vật theo quỹ đạo cố định, độc lập hoàn toàn với animation | **Không cần** — Giữ nguyên Idle / Walk | `OrbitCenter` — Empty GameObject tại tâm nhân vật, object con xoay bằng `transform.Rotate` |
| **D — Ground AoE** | Trống Đồng Đông Sơn (`W005`), Lựu Đạn Thần Sa (`W006`), Nước Thánh Chùa Hương (`W011`) | Xuất hiện thẳng dưới đất tại vị trí kích hoạt, không liên quan tay/animation | **Không cần** — Giữ nguyên Idle / Walk | Instantiate trực tiếp tại `transform.position` của Player, Sorting Layer mặt đất |
| **E — Chain / Pet** | Trượng Long Vương (`W009`), Linh Phù Ma Da (`W010`) | Trượng: dùng chung Gesture nhóm B. Ma Da: Pet Prefab độc lập đi bên cạnh nhân vật | Trượng: **Gesture chung**. Ma Da: **Không cần** | Trượng: `WeaponSocket`. Ma Da: Spawn cạnh player, giữ khoảng cách bám theo bằng script riêng |

> 📌 **Tóm tắt khối lượng animation thực tế cho Vũ khí:** Chỉ cần vẽ **1 animation Gesture dùng chung** cho toàn bộ 12 pháp bảo (dùng cho Nhóm B + Trượng ở Nhóm E). 9/12 pháp bảo còn lại hoàn toàn không cần animation riêng!

---

## 3. Prop Cầm Tay Cố Định (Character Identity Prop)

Mỗi nhân vật sở hữu **1 prop cầm tay cố định**, gắn tại `WeaponSocket` và hiển thị xuyên suốt Idle / Walk — **không đổi theo loadout vũ khí đang mang**, đóng vai trò nhận diện nhân vật (Character Identity):

| Nhân vật | Prop Cầm Tay Cố Định | Ghi Chú Kỹ Thuật |
|---|---|---|
| **Thư Sinh** | Bút Phán Quan (dạng cầm tay tĩnh) | Vẽ 1 lần, hiển thị ở mọi trạng thái Idle/Walk bất kể mang loadout gì |
| **Đạo Sĩ** | Bùa Trấn Yêu / Phất trần (dạng cầm tay tĩnh) | Tương tự |
| **Võ Tăng** | Thiền Trượng (dạng cầm tay tĩnh) | Tương tự |

> [!IMPORTANT]
> **Phân biệt Prop Cầm Tay vs VFX Gameplay:**
> Prop cố định này KHÁC với VFX gameplay của cùng-tên-vũ-khí. Ví dụ: Thư Sinh luôn cầm hình ảnh cây bút trên tay (prop nhận diện), nhưng nếu người chơi *chưa nhặt* pháp bảo Bút Phán Quan trong run đó, VFX chém mực (Nhóm B) đơn giản là không kích hoạt — prop cầm tay vẫn hiển thị bình thường. Tách biệt 2 khái niệm này để tránh dev nhầm lẫn giữa "hiển thị thẩm mỹ" và "logic gameplay".

---

## 4. Ví Dụ Áp Dụng Thực Tế — Võ Tăng Mang 4 Pháp Bảo

**Loadout:** Thiền Trượng (Khởi điểm) + Trống Đồng Đông Sơn + Bùa Trấn Yêu + Cửu Vĩ Hồ Trảo

| Pháp bảo | Nhóm | Biểu hiện hình ảnh trong trận đấu |
|---|---|---|
| Thiền Trượng | E (Prop cố định + Gesture) | Cầm cố định trên tay; khi tấn công, kích hoạt animation Gesture dùng chung |
| Trống Đồng Đông Sơn | D | Sóng âm nổ ra từ vị trí Võ Tăng mỗi 1.5s, không hiện vật thể trên người |
| Bùa Trấn Yêu | C | 5 lá bùa bay vòng quanh người liên tục theo quỹ đạo `OrbitCenter` |
| Cửu Vĩ Hồ Trảo | A | Móng vuốt lửa tự bay ra từ điểm sáng ngắn cạnh vai (`FirePoint`) mỗi 1.2s |

**Kết quả hình ảnh tổng thể:** Võ Tăng cầm trượng cố định (nhận diện nhân vật) + bùa bay quanh người + ánh sáng lóe + sóng âm định kỳ $\rightarrow$ Đọc đúng bản chất *"một võ tăng đang vận nhiều loại linh khí Ngũ Hành cùng lúc"*, vừa đúng lore vừa không bị cảm giác vô lý ôm 4 món vũ khí trên tay.

---

## 5. Yêu Cầu Kỹ Thuật Cho Developer

1. **Hierarchy Sockets (`WeaponSocket`, `FirePoint`, `OrbitCenter`):**
   - Thiết lập 3 Empty GameObject cố định trong Prefab Nhân vật.
   - Vị trí neo tự động xoay/lật theo `flipX` của `SpriteRenderer` (SocketPoint tự lật bên trái/phải theo hướng nhân vật quay).
2. **Animation Trigger:**
   - Chỉ tạo 1 Animator Trigger tên `Gesture` trong Animator Controller.
   - Khi bất kỳ vũ khí nhóm B/E nào hồi chiêu xong, C# Controller gọi `animator.SetTrigger("Gesture")`. VFX tương ứng (vệt chém/luồng lửa/tia sét) tách biệt hoàn toàn khỏi Animator và được spawn qua `GlobalVFXPoolManager`.
3. **Va Chạm NonAlloc:**
   - Tuyệt đối không dùng `OnTriggerEnter2D`/`OnCollisionEnter2D` cho các hiệu ứng trên. Tuân thủ chuẩn `Physics2D.OverlapCircleNonAlloc` (GDD Mục 9).
4. **Render Prop Cố Định:**
   - Prop cầm tay cố định là 1 `SpriteRenderer` con riêng biệt gắn vào `WeaponSocket`, set 1 lần lúc chọn nhân vật và không update runtime theo loadout.

---

## 6. Checklist Khối Lượng Công Việc Thực Tế Cho Artist / Animator

Với mỗi nhân vật (3 nhân vật MVP), Artist / Animator chỉ cần thực hiện:
- [ ] **Idle:** 2–4 frames (từ Art Style Guide)
- [ ] **Walk:** 4–6 frames (từ Art Style Guide)
- [ ] **Gesture:** 3–5 frames — *Dùng chung cho mọi vũ khí nhóm B/E, không vẽ riêng theo từng vũ khí*
- [ ] **Hit-react:** 1–2 frames (hoặc dùng `HitFlashShader`)
- [ ] **Death:** 3–4 frames
- [ ] **1 Prop cầm tay cố định:** 1 sprite tĩnh (gắn `WeaponSocket`)

$\Rightarrow$ **Tổng số animation nhân vật KHÔNG ĐỔI dù người chơi mang 1 hay 6 pháp bảo cùng lúc.** Toàn bộ sự phong phú hình ảnh đến từ VFX vũ khí (`GlobalVFXPoolManager` & `MODULAR_VFX_SYSTEM_DOC.md`), không gây gánh nặng cho Animator.

---

## 🔗 Tài Liệu Tham Chiếu Liên Kết
- 🎮 **[ProjectZombie_GDD.md](file:///c:/Users/thuon/Unity/Projectzombie/GameDesignDoc/ProjectZombie_GDD.md)**: Game Design Document v4.0.
- 🎨 **[ART_VFX_STYLE_GUIDE.md](file:///c:/Users/thuon/Unity/Projectzombie/GameDesignDoc/ART_VFX_STYLE_GUIDE.md)**: Hướng dẫn Mỹ thuật, Bảng màu & Sprite Specs.
- 💥 **[MODULAR_VFX_SYSTEM_DOC.md](file:///c:/Users/thuon/Unity/Projectzombie/Assets/Features/VFX/MODULAR_VFX_SYSTEM_DOC.md)**: Hướng dẫn Hệ thống Modular VFX & Object Pooling.
