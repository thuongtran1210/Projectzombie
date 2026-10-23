# PROJECTZOMBIE — GDD v6: OWNER-APPROVED DECISION RECORD

> **Trạng thái:** D01–D12 và P01–P03/P05–P08 **OWNER-APPROVED** ngày 2026-10-23; **P04 + K01–K06** tiếp tục chờ đặc tả. Đây là **phê duyệt quy tắc thiết kế**, KHÔNG xác nhận tính năng đã có trong runtime Unity, hoàn tất 3D migration, QA hay cho phép merge PR.
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

## Quyết định bổ sung P01–P08 — Owner update 2026-10-23

| ID | Trạng thái | Quy tắc thiết kế / giới hạn |
| --- | --- | --- |
| **P01** | **APPROVED** | Có 4 Hero, dùng **mã Hero/Skill** ổn định ở runtime; tên hiển thị, tên skill, mô tả và bộ chỉ số về sau lấy từ **HeroDefinitionSO / SkillDefinitionSO**. Không cần chốt tên bốn tướng/skill trong GDD MVP. |
| **P02** | **APPROVED** | Map 01 là onboarding, **không xử thua/hòa tại 600s**: nếu Dạ Xoa còn sống thì **tiếp tục giao chiến (overtime) cho tới khi Boss chết -> WIN hoặc Nhà Chính bị phá -> LOSE**. Không tạo một điều kiện kết trận mới. |
| **P03** | **APPROVED** | Hero HP=0 -> downed/respawn countdown; **Hero level càng cao, thời gian hồi sinh càng dài**; respawn ở **Hero initial spawn point của run**. **Hero chết không kết thúc run**. Thời gian định lượng/curve cấu hình ở **HeroRespawnConfigSO**, không hardcode. |
| **P04** | **PENDING** | Dạ Xoa chiến đấu tại arena hay chủ động raid Nhà Chính; cơ chế đánh Base/target priority, xử lý damage/simultaneous win/loss trong cùng tick chưa được chủ dự án quyết định. Không tự chọn. |
| **P05** | **APPROVED baseline** | Dùng **90 giây** làm thời gian mặc định của **WorldWoodDrop** ở Map 01, qua **WoodDropConfigSO** để đổi sau. **Chính xác:** Vongxuyen có **gemLifetimeSeconds=90 cho EXP gem** ở src/config/experience.js; **không tồn tại TTL gỗ rơi từ kill trong nguồn VX**. Đây là baseline **áp dụng tương tự**, KHÔNG phải sao chép thông số gỗ đã có. Pickup phải được Hero thu gom. Pickup radius/magnet cần config và QA riêng, chưa chốt số khác. |
| **P06** | **APPROVED direction / IMPLEMENTATION IN PROGRESS BY OWNER** | Chủ dự án xác nhận Projectzombie **hiện đang chuyển từ 2D sang full stylized 3D**. GDD/đặc tả hướng về **Unity 3D**, tài liệu không khẳng định migration đã hoàn thành; cụ thể camera, rig, NavMesh, URP, 3D colliders theo tech spec/QA. |
| **P07** | **APPROVED MVP scope** | MVP **chỉ mở một pháp bảo: Dép Tổ Ong Thần Sa** (ID ref slipper). **Không yêu cầu hiệu ứng FUN đặc biệt** trong giai đoạn này; cast chủ động, 3D ghost aim, đòn đánh/damage/cooldown cơ bản. **Bốn relic còn lại** trong danh mục 5 (Nồi, Điếu, Chiếu, Chổi) **LOCKED/POST-MVP**, vẫn giữ definition/legacy code; không xóa. Chủ dự án nói “khóa 3 món kia”, nhưng vì roster D04 có 5 item, MVP chỉ có 1 thì bắt buộc **4 locked**; cần xác nhận nếu muốn loại một món khỏi roster sau. |
| **P08** | **APPROVED** | **Dạ Xoa chết -> WIN NGAY (nếu Base còn HP)**, bất kể trước/sau 600s; **dừng wave scheduler, hủy pending spawn queue, chặn enemy/boss spawn mới** và xử lý kết trận exactly once. Không bắt người chơi đợi đủ 10 phút; không cần spawn thêm quái. |

### Các chi tiết kỹ thuật vẫn cấu hình / chưa khóa giá trị

- **K01:** Giá trị chính xác theo cấp của hồi sinh (baseSeconds, levelMultiplier/AnimationCurve, giới hạn min/max), hành vi invulnerability khi respawn. P03 đã chốt chiều tăng và vị trí, **chưa chốt con số**.
- **K02:** WoodDrop pickup radius, magnet, pooling/merge/cap, vị trí rơi an toàn; lifetime baseline 90s đã chọn theo EXP config VX, không phải TTL gỗ gốc.
- **K03:** Dạ Xoa raid Base hay chờ trong đấu trường, target policy và thứ tự ưu tiên khi BaseDestroyed/BossKilled trong cùng simulation tick (**P04**).
- **K04:** Thiết kế cụ thể 4 Hero kits/model, mapping skill IDs và 3D production config khi asset sẵn sàng. P01 không bắt buộc chốt tên trước.
- **K05:** Dép MVP (damage/cooldown/range/indicator số cuối) trong RelicDefinitionSO; FUN effects chuyển POST-MVP.
- **K06:** Full 3D engine settings (camera/renderer/lighting/nav/perf constraints) theo quá trình migration; tài liệu không coi là DONE-CODE.

**Tách bạch:** P01–P03, P05–P08 đã được quyết định về **nguyên tắc**; P04 và K01–K06 chưa có đủ chi tiết. Không sửa D01–D12.

## Bất biến phải được bảo toàn

1. **Không thay điều kiện thắng/thua:** chỉ BaseHP=0 gây Lose; Boss cuối bị diệt gây Win. Hero death ≠ Lose. Không biến timer 600s thành tự động Win/Lose.
2. **Wood pickup only:** kill tạo loot; kill không grant wood trực tiếp; grant sau khi nhặt và xác thực loot instance. Phu chỉ được nhặt khi giai đoạn sau được triển khai.
3. **MVP không có FriendlyArmy/Squad:** các tài liệu mô tả quân/squad chỉ là POST-MVP DESIGN, không được coi là task block release MVP.
4. **Full 3D target, migration đang diễn ra từ Unity 2D:** giữ các module cũ có thể tái sử dụng, lập migration plan/tách 3D adapters, không ghi như thể đã có 3D.
5. **Defense là mode chính**, Survival là Legacy Mode và được bảo toàn ở code/asset; old enemies không được spawn vào Map 01 mới.

## Quy trình ghi nhận thay đổi

- Các quyết định D01–D12 đã có hiệu lực làm chuẩn thiết kế trong nhánh Draft PR #1. Mọi thay đổi Dxx cần được chủ dự án xác nhận qua decision record mới.
- **P04 vẫn PENDING**; P01–P03 và P05–P08 đã chốt nguyên tắc. K01–K06 là chi tiết config/implementation chưa khóa con số. Không đổi D01–D12.
- Sau khi đồng bộ tài liệu và link, có thể review Draft PR; merge theo quy trình repo và QA tài liệu; runtime cần PR riêng.

## Checklist hoàn tất cập nhật tài liệu

- [x] Ghi chính xác 12 quyết định owner-approved.
- [x] Phân biệt source code VX, nguồn Unity PZ hiện tại và bản thiết kế đã duyệt.
- [ ] Soát lại toàn bộ file đang mang nhãn active, xác nhận không còn lời khẳng định trái D07/D08/D10/D02.
- [x] Ghi nhận P01–P03/P05–P08 và giữ P04 pending.
- [ ] Xác nhận P04 và các tham số kỹ thuật K01–K06 trước khi nghiệm thu runtime tương ứng.
