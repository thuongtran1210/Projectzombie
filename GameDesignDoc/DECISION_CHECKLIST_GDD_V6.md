# PROJECTZOMBIE — GDD v6: OWNER-APPROVED DECISION RECORD

> **Trạng thái:** 12/12 quyết định cấp sản phẩm **APPROVED bởi chủ dự án ngày 2026-10-23**. Đây là **phê duyệt quy tắc thiết kế**, KHÔNG xác nhận tính năng đã có trong runtime Unity, hoàn tất 3D migration, QA hay cho phép merge PR.
> **Nguồn:** lựa chọn trực tiếp của chủ dự án trong cuộc hội thoại, Draft PR #1. Mục mở bên dưới không bị tự suy diễn thành phê duyệt.

## Các quyết định đã chốt

| ID | Chọn | Quyết định ràng buộc | Trạng thái |
| --- | :---: | --- | --- |
| **D01** | **A** | Defense là gameplay chính. Survival cũ giữ dưới Legacy Mode, không xóa scene/asset/save trước khi migration. | **APPROVED** |
| **D02** | **C** | Unity full 3D stylized; dùng art direction semi-chibi TREBRAVE, pipeline 3D, Physics3D, NavMesh/waypoint phù hợp; không port Three.js nguyên xi. | **APPROVED** |
| **D03** | **A** | 4 Hero có thể chọn ở sảnh, đúng 1 Hero do người chơi điều khiển trong mỗi run; danh tính/mapping bộ kit chờ P01. | **APPROVED** |
| **D04** | **A** | 5 pháp bảo FUN Vongxuyen cho Map 01 (Dép/Nồi/Điếu/Chiếu/Chổi), 1 slot; relic cổ phong cũ chuyển Legacy/backlog không xóa code. | **APPROVED** |
| **D05** | **A** | Relic là ACTIVE skill, dùng nút riêng, ghost aim, cast/cancel/cooldown; không auto trigger trái luật. | **APPROVED** |
| **D06** | **A** | Map 01 gameplay clock 600 giây, 5 phase; Ma Giáp Thống Lĩnh 108s, Dạ Xoa 510s là reference timeline; xử lý chạm mốc 600s chờ P02. | **APPROVED** |
| **D07** | **C** | CHỈ THUA khi Nhà Chính HP=0; THẮNG khi giết Boss cuối (Dạ Xoa) và Nhà Chính chưa bị phá. Hero HP=0 KHÔNG trigger thua trận; xử lý Hero incapacitation/respawn chờ P03. Boss Ma Giáp Thống Lĩnh là miniboss, không thắng sớm. | **APPROVED** |
| **D08** | **B** | Wood là vật phẩm rơi tại vị trí enemy chết; phải được Hero nhặt (MVP) hoặc Phu thu gom (giai đoạn sau) mới tăng ví, không cộng trực tiếp qua kill event. | **APPROVED** |
| **D09** | **A** | Map 01 dùng roster 7 types Vongxuyen; enemy Projectzombie cũ giữ trong Legacy/Future Maps; không xóa asset/code hay mapping save. | **APPROVED** |
| **D10** | **C** | MVP chỉ Hero + Base + Tower (và pickup gỗ); chưa triển khai Phu, Thầy Lang, Dân Binh, Squad/Commander/Rally; để Phase 2 / BACKLOG. | **APPROVED** |
| **D11** | **A** | Tên thương hiệu người chơi nhìn thấy là TREBRAVE; giữ repository, package, namespace kỹ thuật Projectzombie; Map 01 = Map Sông Quê. | **APPROVED** |
| **D12** | **A** | Giữ hệ thống code Ngũ Hành, Âm Dương, Gacha, Meta Save, Photon Fusion, LiveOps/Monetization; không bắt buộc đưa vào Defense MVP; không xóa legacy module. | **APPROVED** |

## Open implementation decisions — giữ PENDING, không ghi đè chủ trương đã duyệt

- [ ] **P01 — Danh tính & bộ kit 4 Hero:** chốt tên/lore/visual/bộ combo + signature/skill và mapping giữa Projectzombie 4-Hero kits với Vongxuyen (đặc biệt Võ Tăng/Ẩn Sĩ/Đạo Sĩ). Số Hero đã chốt là 4; không quyết định thay tên khi chưa duyệt.
- [ ] **P02 — Timeout đúng 600s khi Dạ Xoa còn sống:** 600s là thời lượng thiết kế/timeline đã chốt, nhưng D07 yêu cầu thắng bằng boss kill, thua duy nhất bằng Base destroyed; cần xác nhận liệu tiếp tục đánh overtime (đề nghị), auto boss enrage hay kết thúc theo một loại kết quả trung gian. **Không tự xử thua do timeout.**
- [ ] **P03 — Khi Hero HP=0:** chọn downed + hồi sinh sau N giây, respawn tại Base có cost, hoặc Hero phải được NPC/đồng đội cứu; nếu Hero không thể hành động nhưng Base còn sống, cần tránh softlock. **Không tự GameOver khi Hero chết.**
- [ ] **P04 — Cơ chế Boss Dạ Xoa:** khi đến 510s xuất hiện rồi chiến đấu tại arena hay tự tấn công Base? Nếu Hero hạ Boss trước 600s có kết thúc tức thì? Mặc định proposal: Boss cuối chết → Win ngay khi Base vẫn sống, nhưng cần xác nhận trước runtime.
- [ ] **P05 — Khả năng thu gom gỗ MVP:** wood drop duration/stack, hút vật phẩm, giới hạn vật phẩm/khung hình, reward attribution khi chết ngoài camera; khi chưa có Phu, Hero phải tiếp cận để nhặt. StartingWood 100 và +10/kill là reference chưa khóa balance.
- [ ] **P06 — Art/render/migration 3D:** URP 3D vs renderer khác, camera angle, 3D physics/nav, input, model/rig/addressable, phạm vi phải rebuild; chuyển 2D scene cũ vào Legacy; migration performance budget cần đo sau prototype.
- [ ] **P07 — 5 FUN relic chi tiết:** mapping ID/pre-existing Unity mechanics, active cast UX và các hiệu ứng đặc biệt khi chuyển từ auto sang active (không làm mất FUN implementation).
- [ ] **P08 — Khả năng chơi đủ 10 phút với D07:** vì Boss ở 510s và thắng ngay khi hạ boss, một trận có thể kết thúc trước 600s; cần quyết định có gate chiến thắng tới 600s hay điều chỉnh boss end. Không thay đổi luật chỉ thua khi Base bị phá.

## Bất biến phải được bảo toàn

1. **Không thay điều kiện thắng/thua:** chỉ BaseHP=0 gây Lose; Boss cuối bị diệt gây Win. Hero death ≠ Lose. Không biến timer 600s thành tự động Win/Lose.
2. **Wood pickup only:** kill tạo loot; kill không grant wood trực tiếp; grant sau khi nhặt và xác thực loot instance. Phu chỉ được nhặt khi giai đoạn sau được triển khai.
3. **MVP không có FriendlyArmy/Squad:** các tài liệu mô tả quân/squad chỉ là POST-MVP DESIGN, không được coi là task block release MVP.
4. **Full 3D target, hiện trạng Unity 2D:** giữ các module cũ có thể tái sử dụng, lập migration plan/tách 3D adapters, không ghi như thể đã có 3D.
5. **Defense là mode chính**, Survival là Legacy Mode và được bảo toàn ở code/asset; old enemies không được spawn vào Map 01 mới.

## Quy trình ghi nhận thay đổi

- Các quyết định D01–D12 đã có hiệu lực làm chuẩn thiết kế trong nhánh Draft PR #1. Mọi thay đổi Dxx cần được chủ dự án xác nhận qua decision record mới.
- P01–P08 đang mở không làm đảo ngược D01–D12. Nếu Pxx ảnh hưởng tới vòng lặp chơi được, phải giải quyết trước khi tạo implementation spec cuối và code.
- Sau khi đồng bộ tài liệu và link, có thể review Draft PR; merge theo quy trình repo và QA tài liệu; runtime cần PR riêng.

## Checklist hoàn tất cập nhật tài liệu

- [x] Ghi chính xác 12 quyết định owner-approved.
- [x] Phân biệt source code VX, nguồn Unity PZ hiện tại và bản thiết kế đã duyệt.
- [ ] Soát lại toàn bộ file đang mang nhãn active, xác nhận không còn lời khẳng định trái D07/D08/D10/D02.
- [ ] Chốt P01–P08 hoặc lưu làm backlog có chủ sở hữu trước khi chốt kỹ thuật implementation.
