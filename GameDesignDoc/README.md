# PROJECTZOMBIE — GAME DESIGN DOCUMENTATION HUB (v6 DRAFT)

> **Ngày cập nhật:** 2026-10-23. **Trạng thái:** GDD v6 đang CHỜ OWNER APPROVAL D01–D12.  
> Nhánh tài liệu này chỉ là **review draft**, không có nghĩa Unity gameplay đã thay đổi. Main hiện vẫn theo bộ tài liệu v5.1.  
> **Nguồn gameplay VX:** https://github.com/thuongtran1210/Vongxuyen

## Bắt đầu từ đây

1. **[Master GDD v6 — DRAFT](ProjectZombie_GDD.md)** — tầm nhìn thống nhất: Map Sông Quê, một Hero và một pháp bảo trong run, kinh tế gỗ, tháp, dân binh, quái Vongxuyen.
2. **[Owner Decision Checklist (D01–D12)](DECISION_CHECKLIST_GDD_V6.md)** — **phải duyệt trước khi chuyển GDD thành APPROVED**.
3. **[Refactor Ledger & Conflict Registry](DOCS_REFACTOR_LEDGER.md)** — số liệu mâu thuẫn, kế hoạch sửa 17+ tài liệu.
4. **[Map & Defense System Spec](MAP_AND_DEFENSE_SYSTEM_SPEC.md)** — map 2 bờ/3 cầu, 10 bệ tháp, Base HP, gỗ, tower, three allied unit roles và command squads.
5. **[Vongxuyen Enemy & Boss Roster](ENEMY_ROSTER_VONGXUYEN.md)** — 7 reference enemy types, dữ liệu/AI/source status.
6. **[Map 01 Scenario](LEVEL_SCENARIO_GUIDE.md)** — 600s, 34 wave events, Boss reference 108s/510s, luật kết thúc đang chờ duyệt.
7. **[1-Hero / 1-Relic Visual & Input Spec](WEAPON_VISUAL_MANIFESTATION_RULES.md)** — một relic slot, lựa chọn ACTIVE/AUTO/HYBRID, không lặp quy tắc 6 slots.

## Tài liệu bổ trợ — trạng thái hiện tại

| Tài liệu | Vai trò | Trạng thái v6 |
| --- | --- | --- |
| [FUN Combat & Enemy Mechanics](FUN_COMBAT_AND_ENEMY_MECHANICS_DOC.md) | Cơ chế FUN combat tham khảo Unity; enemy legacy chờ D09 | **LEGACY-MIXED**, KHÔNG là Enemy Roster Map 01 |
| [Tilemap Design](TILEMAP_DESIGN_SPEC.md) | Chi tiết Unity 2D/PPU; cần 3 lane/bridge khi D02=A | **D02 BLOCKED** |
| [Physics Layer Design](PHYSICS_LAYER_DESIGN_SPEC.md) | Physics2D, collision, layer | **REQUIRES DEFENSE EXTENSION** |
| [Art VFX Style](ART_VFX_STYLE_GUIDE.md) | Thiết kế 2D hiện tại | **D02 BLOCKED** |
| [Game UI Style](GAME_UI_STYLE_GUIDE.md) | UI/UX nền Unity | **REQUIRES DEFENSE HUD** |
| [UI Prompt & Art](UI_PROMPT_AND_ART_GUIDE.md) | UI/asset prompt reference | **REVIEW AFTER D02/D11** |
| [Audio System](AUDIO_SYSTEM_DESIGN_SPEC.md) | Audio infrastructure | **KEEP / EXTEND** |
| [Cloud Save](CLOUD_SAVE_SYSTEM_SPEC.md) | Save & persistence | **KEEP / ADD DEFENSE SAVE BOUNDARY** |
| [Multiplayer](MULTIPLAYER_ONLINE_SYSTEM_SPEC.md) | Photon Fusion | **D12 BLOCKED**, not automatically Defense MVP |
| [AdMob](ADMOB_MONETIZATION_SPEC.md) | Monetization | **D12 BLOCKED** |
| [Performance](PERFORMANCE_OPTIMIZATION_REPORT.md) | Profile report lịch sử | **HISTORICAL**; không coi là FPS đo cho Defense v6 |
| Uprades.doc | Artifact legacy | **AUDIT BEFORE ARCHIVE** |

## Quy định vận hành

- **Luật gameplay** chỉ nằm ở Master GDD; spec tính năng không tự đổi điều kiện thắng/thua hoặc thời lượng.
- **Thông số triển khai** ở Map/Defense, Enemy Roster và Scenario; phần nào chưa test phải ghi BASELINE/PROPOSED.
- **Tài liệu lịch sử** được giữ trong Git; không sửa chúng để giả vờ mã nguồn đã port.
- **Mọi vấn đề chưa được chủ dự án xác nhận** dùng D01–D12 và có “decision rationale” khi duyệt.
- Link luôn là đường dẫn repository-relative; không sử dụng file:/// của máy tác giả.
- Sau khi checklist được duyệt, thực hiện các hạng mục BLOCKED trong [Ledger](DOCS_REFACTOR_LEDGER.md) trước khi gắn nhãn v6 APPROVED.

## Nguồn kỹ thuật / quy trình

- [Developer Team Working Guide](../DEVELOPMENT_GUIDE.md) — quy tắc Unity/C#, Feature-Based modules, tests và PR.
- [Repository README](../README.md) — hướng dẫn cài đặt và build Unity; **pitch sản phẩm cũ chưa được cập nhật** trên nhánh draft.
- [Vongxuyen Master GDD](https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/MASTER-GAME-DESIGN-AND-COMPLETION-PLAN.md).
- [Vongxuyen Product Vision](https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/PRODUCT-VISION.md).
