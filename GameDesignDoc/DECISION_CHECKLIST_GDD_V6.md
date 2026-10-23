# PROJECTZOMBIE — GDD v6 DESIGN DECISION CHECKLIST (OWNER APPROVAL)

> **Trạng thái:** 12/12 PENDING; các dấu chọn ở mục "đề xuất" chỉ là khuyến nghị của tác giả, KHÔNG có giá trị phê duyệt.  
> **Cách duyệt:** Chọn mỗi quyết định đúng **một** phương án, ghi chú nếu khác và xác nhận phiên bản. Chỉ sau đó chuyển GDD v6 và phụ lục từ DRAFT sang APPROVED.  
> **Mục tiêu:** khóa một bộ luật duy nhất cho Map, Hero, Relic, Defense, Enemy, Boss và phạm vi legacy.

## D01 — Defense có phải gameplay chính của Projectzombie?

- [ ] A — **Đề xuất:** Defense là mode chính / màn 01 chính; giữ Survival cũ ở Legacy Mode (không phải marketing core).
- [ ] B — Defense chỉ là mode bổ sung; Survival vẫn mode chính.
- [ ] C — Thay Survival hoàn toàn; xóa mode cũ sau migration đã duyệt.
- [ ] Khác / ghi chú: …

**Hệ quả:** quyết định home screen, GDD core loop, khả năng dùng lại stage/scene, Save và quảng bá.

## D02 — Dựng Map Sông Quê trên công nghệ nào?

- [ ] A — **Đề xuất:** Unity 2D/Tilemap (giữ Projectzombie hiện tại), port layout/logic từ Vongxuyen; không port 3D ngay.
- [ ] B — Unity 2.5D (world 2D hoặc 3D visual), cần spec camera/collider mới.
- [ ] C — Chuyển Unity sang full stylized 3D theo art direction TREBRAVE; yêu cầu roadmap/asset rewrite riêng.

**Hệ quả:** prefab, render pipeline, animation, input, navigation, performance và art budget.

## D03 — Danh mục Hero ngoài sảnh?

- [ ] A — **Đề xuất:** Giữ tối đa bốn Hero có thể chọn/mở khóa, **mỗi run điều khiển một Hero duy nhất**.
- [ ] B — MVP chỉ một Hero (Thư Sinh), ba Hero khác trong backlog.
- [ ] C — Giữ nguyên roster/hero kits của Projectzombie v5.1, chỉ chốt 1 Hero trong run.

**Cần ghi:** tên hero chính thức và mapping Thư Sinh/Thanh Đồng/Ẩn Sĩ/Đạo Sĩ/Võ Tăng.

## D04 — Danh mục Pháp bảo cho Map 01?

- [ ] A — **Đề xuất:** 5 FUN relic của Vongxuyen (Dép, Nồi, Điếu, Chiếu, Chổi) là bộ chuẩn Map 01; bộ cổ phong đưa backlog/legacy.
- [ ] B — Cho chọn từ toàn bộ 17 pháp bảo cũ, nhưng 1 slot và phải cân bằng lại.
- [ ] C — 5 FUN trước + mở thêm cổ phong theo unlock từng giai đoạn, vẫn 1 slot.

**Bất biến:** đúng 1 pháp bảo / run, không được tách 17 item thành 17 slot.

## D05 — Cách dùng một pháp bảo?

- [ ] A — **Đề xuất:** ACTIVE skill, có nút riêng, ghost aim, cooldown; người chơi quyết định thời điểm cast.
- [ ] B — AUTO/passive theo Projectzombie hiện tại.
- [ ] C — HYBRID: mỗi relic có mode riêng nhưng chỉ 1 equipped; UI mô tả rõ.

**Hệ quả:** WeaponManager runtime, targeting, nút HUD, upgrade và balance.

## D06 — Thời gian và timeline Map 01?

- [ ] A — **Đề xuất:** 600 giây / 10 phút, 5 phase; mốc Boss Ma Giáp 108s, Dạ Xoa 510s theo Vongxuyen reference.
- [ ] B — 900 giây / 15 phút và tune lại toàn bộ wave/Boss.
- [ ] C — 1.200 giây / 20 phút giống kịch bản GDD Projectzombie cũ.

**Cần chốt:** co-op/revive pause có làm dừng clock? (Đề xuất: gameplay pause dừng tất cả).

## D07 — Điều kiện thắng/thua Map 01?

- [ ] A — **Đề xuất:** thua khi Base HP=0 hoặc Hero chết; thắng khi tới 600s **và** Boss chính đã chết.
- [ ] B — thua khi Base HP=0 hoặc Hero chết; thắng sống sót đủ giờ, Boss là bonus.
- [ ] C — chỉ thua khi Base bị phá (Hero có hồi sinh), thắng diệt Boss.
- [ ] D — điều kiện riêng khác, ghi rõ …

**Phải làm rõ thêm:** kill Boss sớm thì dừng trận ngay hay vẫn thủ đủ 600s? Boss có được đánh Base không? Hero có revive không?

## D08 — Gỗ nhận theo cách nào?

- [ ] A — **Đề xuất:** +gỗ vào tài nguyên run khi quái chết; Phu thu gom EXP/pickup vật lý hoặc làm logistics, chống double rewards.
- [ ] B — Gỗ rơi dưới đất và bắt buộc Hero/Phu nhặt về mới được sử dụng.
- [ ] C — Hybrid (một phần trực tiếp, phần thưởng lớn rơi vật phẩm).

**BASELINE cho thử nghiệm:** 100 gỗ đầu; 50/tháp; 40/80 nâng lv2/lv3; 40 dân binh, 30 phu, 35 lang; +10 gỗ/kill trước modifier. Tất cả có thể tune sau.

## D09 — Có chuyển toàn bộ enemy Map 01 sang roster Vongxuyen?

- [ ] A — **Đề xuất:** **Map 01 chỉ dùng 7 reference types Vongxuyen**; enemy/boss cũ Projectzombie sang Legacy/Future Maps.
- [ ] B — Map 01 dùng 7 loại VX và một số enemy cũ như optional elite.
- [ ] C — Giữ cả roster Projectzombie và Vongxuyen, cần full reconciliation + balance.

**7 types:** magiap, thuyquai, huyetdoi, magiap_boss, quaidu, phicuong, yaksha_boss.

## D10 — Phạm vi hệ đồng minh và điều binh?

- [ ] A — **Đề xuất:** MVP có 3 loại đơn vị + ba squad + defend/choke/follow/rally, commander succession đưa phase sau.
- [ ] B — Full Vongxuyen system cả commander, succession, parry/block/counter ở lần phát hành đầu.
- [ ] C — MVP chỉ tháp + Hero; toàn bộ lính/squad thêm sau.

**Ảnh hưởng:** HUD mobile, AI FSM, performance và độ khó dự án.

## D11 — Tên hiển thị và lore chính của sản phẩm?

- [ ] A — **Đề xuất:** giữ repo/namespace Projectzombie nhưng thương hiệu trò chơi TREBRAVE; Map 01 = Map Sông Quê; bối cảnh giữ quê hương/giành lại vùng bị chiếm.
- [ ] B — tên game Vong Xuyên, chỉ cập nhật Map 01 = Map Sông Quê và narrative như VX.
- [ ] C — thương hiệu Projectzombie, chỉ áp gameplay VX mà không đổi lore.
- [ ] D — Khác: …

**Lưu ý:** không nhầm địa danh Vong Môn với tên Map 01, không quảng cáo territory reclaim như tính năng đã có.

## D12 — Scope các tính năng Projectzombie cũ?

- [ ] A — **Đề xuất:** giữ code/infrastructure của Ngũ Hành, Âm Dương, Gacha, Meta Save, Photon Fusion; **MVP Defense tạm ẩn hoặc hạn chế** các hệ ngoài core, tránh bội thực thiết kế.
- [ ] B — tất cả chạy song song đầy đủ ngay trong Defense MVP.
- [ ] C — loại bỏ vĩnh viễn các hệ không tương thích sau khi có kế hoạch migration rõ ràng.

**Cần chốt phụ:** multiplayer co-op Defense có bắt buộc ở bản đầu không? Ads/gacha/store có thuộc tài liệu thiết kế launch không?

---

## Quyết định phụ không làm blocker cho bản GDD

- **Build positions:** mặc định 10 slot cố định của VX; nếu muốn xây tự do, lập Change Request riêng.
- **Territory reclaim:** mới là long-term vision, không tính vào MVP cho đến khi có GDD riêng.
- **Balance:** thông số HP/cost/wave đầu được đánh dấu BASELINE; qua playtest mới nâng thành APPROVED-BALANCE.
- **Art:** semi-chibi stylized 3D là VX target style; không mặc nhiên ép dự án Unity hiện tại bỏ 2D khi D02 chưa duyệt.

## Mẫu phản hồi nhanh

Trả lời một dòng: **D01=A; D02=A; D03=A; D04=A; D05=A; D06=A; D07=A; D08=A; D09=A; D10=A; D11=A; D12=A**. Có thể viết lựa chọn khác và thêm ghi chú riêng.

**Quy trình sau phê duyệt:**
1. Ghi quyết định + ngày + owner vào checklist, chuyển status ACCEPTED.
2. Cập nhật cùng lúc GDD chính, scenario, map, enemy roster, weapon rules, doc hub.
3. Tài liệu legacy được dán nhãn SUPERSEDED hoặc HISTORICAL; không xóa làm mất lịch sử.
4. Revalidate tất cả con số/xung đột; khóa version, merge PR sau review.
5. Mới tách Epic/Issue/PR triển khai runtime Unity.
