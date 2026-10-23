# PROJECTZOMBIE — DOCUMENT REFACTOR LEDGER v6 (DRAFT)

> **Trạng thái:** Proposed documentation IA / impact assessment; chưa phải toàn bộ file đã được refactor.  
> **Nguồn:** GameDesignDoc trên Projectzombie main@657c8215; Vongxuyen main@5f5d9b26.  
> **Mục đích:** xác định file cần sửa, file giữ, phần mâu thuẫn, owner và nguồn sự thật sau khi D01–D12 được duyệt.

## 1. Hệ phân cấp tài liệu

**L0 — Product:** PROJECTZOMBIE GDD v6 xác định luật Map 01, 1 Hero / 1 Relic, gameplay chính và phạm vi sản phẩm.  
**L1 — Game Feature Specs:** Map/Defense, Enemy Roster, Level Scenario, Combat & Relic, Hero Kits, UX Controls.  
**L2 — Production Specs:** Tilemap/Art, UI Style, Physics Layers, Audio, Performance, Build, Data/Addressables.  
**L3 — Services / Optional:** Multiplayer, Cloud Save, Monetization, Meta Progression.  
**L4 — History:** v5.1 reference, design concepts, bản khác engine, các prototype bỏ.

**Single Source of Truth:** GDD v6 nắm luật chơi, L1 nắm thuật toán/thông số; khi tài liệu khác mâu thuẫn, ghi issue và sửa cùng PR, không âm thầm chọn số bất kỳ. Mọi file phải có version/date/status/dependency và link tương đối.

## 2. Ma trận refactor

| File | Hiện trạng trước refactor | Hành động v6 | Trạng thái trên nhánh draft |
| --- | --- | --- | --- |
| GameDesignDoc/ProjectZombie_GDD.md | GDD 5.1 survival 20m, 17 relic roster, enemy/boss cũ | **REWRITE** unified Map Sông Quê/Defense/1 Hero+Relic/VX enemies | DRAFT REWRITTEN |
| GameDesignDoc/README.md | Index v5.1, link local file:/// và chỉ dẫn nhiều file thiếu | **REWRITE** relative link hub, status từng spec | DRAFT REWRITTEN |
| GameDesignDoc/LEVEL_SCENARIO_GUIDE.md | Kịch bản 20 phút, Ngưu Đầu–Mã Diện và Diêm Vương | **REWRITE** reference 600s, 34 events, Boss 108/510, D07 | DRAFT REWRITTEN |
| GameDesignDoc/WEAPON_VISUAL_MANIFESTATION_RULES.md | Nói “tối đa 6 pháp bảo cùng lúc” và 12 relic cổ phong | **REWRITE** one slot, 5 FUN candidates, active/auto pending D05 | DRAFT REWRITTEN |
| GameDesignDoc/FUN_COMBAT_AND_ENEMY_MECHANICS_DOC.md | Vừa mô tả FUN relic cũ vừa có enemy/boss legacy | **REFACTOR NEXT** giữ FUN combat as mechanics reference; tách enemy cũ thành HISTORY nếu D09=A | LEGACY / NEEDS REVIEW |
| GameDesignDoc/TILEMAP_DESIGN_SPEC.md | PPU=64, 2D Tilemap chung, chưa mô tả 3 bridges/lane | **UPDATE AFTER D02** chuyên biệt hóa map, giữ PPU nếu vẫn 2D | D02 BLOCKED |
| GameDesignDoc/PHYSICS_LAYER_DESIGN_SPEC.md | Layers 2D theo enemy/player/projectile | **UPDATE AFTER D02** thêm Base/Tower/Ally/Water/Nav/Placement hitboxes | D02 BLOCKED |
| GameDesignDoc/ART_VFX_STYLE_GUIDE.md | Mỹ thuật 2D, chưa nhất quán TREBRAVE 3D | **UPDATE AFTER D02** art target theo engine được duyệt; giữ readability/telegraph | D02 BLOCKED |
| GameDesignDoc/GAME_UI_STYLE_GUIDE.md | Combat UI cũ, không có build/squad/base | **UPDATE** Defense HUD dock, contextual actions, mobile safe-area | NOT STARTED |
| GameDesignDoc/UI_PROMPT_AND_ART_GUIDE.md | UI prompt cũ | **UPDATE** nếu UI role/state đổi | NOT STARTED |
| GameDesignDoc/AUDIO_SYSTEM_DESIGN_SPEC.md | Master audio DNA và mixer | **KEEP & EXTEND** feedback tower/base/wood/boss; không overwrite | NOT STARTED |
| GameDesignDoc/MULTIPLAYER_ONLINE_SYSTEM_SPEC.md | Photon Fusion phạm vi multiplayer | **KEEP AS OPTIONAL** theo D12; thêm authority note Defense | D12 BLOCKED |
| GameDesignDoc/CLOUD_SAVE_SYSTEM_SPEC.md | Save/migration | **KEEP & EXTEND** tránh persist Wood, stage schema version | NOT STARTED |
| GameDesignDoc/ADMOB_MONETIZATION_SPEC.md | Ads/monetization | **KEEP AS OPTIONAL** không ảnh hưởng GDD lõi | D12 BLOCKED |
| GameDesignDoc/PERFORMANCE_OPTIMIZATION_REPORT.md | Benchmark/kiểm tra runtime cũ | **KEEP HISTORICAL**, chỉ cập nhật khi có đo performance mới | UNCHANGED |
| GameDesignDoc/Uprades.doc | Artifact cũ, định dạng .doc | **AUDIT & ARCHIVE** (không xóa trước khi biết nội dung) | UNCHANGED |
| README.md (root) | Mô tả Vong Xuyên survivor 2D, long 20m | **UPDATE AFTER APPROVAL** brand/pitch và link GDD v6 | NOT STARTED |
| DEVELOPMENT_GUIDE.md | Quy trình và kiến trúc Unity, feature-based | **KEEP**; bổ sung Defense module khi code triển khai | UNCHANGED |

**Không xoá** assets/scripts/tài liệu cũ hoặc chỉnh Save keys trong PR tài liệu. GDD cũ được lưu bởi lịch sử Git của branch base; có thể xuất ARCHIVE/v5_1 nếu team muốn bản đọc độc lập.

## 3. Conflict registry

| ID | Mâu thuẫn | Nguồn A | Nguồn B | Resolution |
| --- | --- | --- | --- | --- |
| C01 | Core gameplay | PZ v5.1 survival hack-slash | VX hero TD 3 lanes | Chờ D01 |
| C02 | Map 01 name/terrain | Bến Đò Vong Xuyên | Map Sông Quê two banks | GDD v6 proposed |
| C03 | 20m vs 10m vs 900s timeline | LEVEL_SCENARIO / Level1_Timeline | VX Stage01 600s | D06, separate asset |
| C04 | Win killing Diêm Vương vs survive/defend | PZ old scenario | VX Defense + Yaksha | D07 |
| C05 | 6 shown vs 1 code relic slot | WEAPON_VISUAL_RULES | WeaponManager.MAX_WEAPONS=1 | 1 slot enforced |
| C06 | 17 relic catalog vs five FUN | GDD v5.1 | VX active FUN set | D04–D05 |
| C07 | Enemy roster | PZ ghost/deity bosses | VX magiap/water/bat/boss | D09 |
| C08 | 2D 64PPU vs VX 3D semi-chibi | PZ art docs | VX art direction | D02 |
| C09 | 4 hero names/kits differ | PZ multiple guides | VX roster | D03, identity mapping |
| C10 | SP/MP, Gacha in MVP | PZ Services | VX prototype scope | D12 |
| C11 | Economy reward auto vs gatherer role | VX DefenseRules, MinionManager | New PZ loop | D08 |
| C12 | Commander plans vs implementation | VX squad plan | port requirement | D10 |

## 4. Quy trình áp dụng thay đổi

1. Ký checklist D01–D12, ghi rationale nếu chọn khác phương án đề xuất.
2. GDD root cập nhật quyết định và một bộ từ điển key chính thức: Map01, Hero, RelicSlot, Wood, Base, Tower, Lane, Enemy.
3. Đặc tả L1 đồng bộ từ GDD; chỗ chưa có code dùng nhãn PROPOSED.
4. Tài liệu L2 chỉ thay đổi phần có tác động thật; giữ nguyên tech constraints không xung đột.
5. Đối chiếu bằng tìm kiếm từ khóa legacy: “20:00”, “20 phút”, “6 pháp bảo”, “Diêm Vương”, “Bến Đò Vong Xuyên”, “17 pháp bảo”, “1 pháp bảo”; cập nhật hoặc đóng khung HISTORICAL.
6. Documentation PR review; không merge nếu còn hai rule active mâu thuẫn. QA tài liệu link markdown và nguồn external.
7. Sau merge mới tạo issue/PR code và asset theo P0–P7.

## 5. Naming dictionary tạm thời

- Repo/project kỹ thuật: Projectzombie (không đổi).
- Tên sản phẩm hiển thị: CHỜ D11.
- Map 01: Map Sông Quê (proposal).
- Hero: one controlled player entity per run.
- Relic: one equipped slot; not synonymous with Basic Attack, Skill, or summoned pet.
- Stage Time: t_gameplay; không dùng wall clock.
- Wave: scheduled encounter event; Lane: strategic traversal route; Zone: spawn/build/AI restriction.
- Wood: currency in-run; Coins: meta currency; EXP: level-progression.
- Base: primary defend structure; Tower: built structure; Friendly: recruited allied unit; Squad: group of friendly soldiers.
- Enemy ID: stable VX typeId mapping; Unity prefab names có thể khác nhưng SO phải có alias rõ.
- Status: EXISTING-PZ / REFERENCE-VX / PROPOSED-PZ / APPROVED / BACKLOG / HISTORICAL.

## 6. Owner approval

Tài liệu này chưa được phê duyệt. [GDD v6](ProjectZombie_GDD.md) và [Checklist](DECISION_CHECKLIST_GDD_V6.md) phải cùng phiên bản sau mỗi vòng chốt.
