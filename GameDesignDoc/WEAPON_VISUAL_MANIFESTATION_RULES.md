# TREBRAVE — ONE HERO / ONE ACTIVE FUN RELIC — UNITY 3D UX SPEC v6.1

> **Trạng thái:** **D02=C, D03=A, D04=A, D05=A owner-approved 2026-10-23** — 4 Hero chọn sảnh, 1 Hero/run, 5 FUN relic choices, 1 ACTIVE relic/run, Unity full 3D target. Runtime 3D chưa port.  
> **Thay thế bản visual rules legacy** từng mô tả mang tối đa 6 pháp bảo. Lịch sử thiết kế cũ vẫn có trong Git.  
> **PENDING chỉ còn:** P01 mapping/naming 4 Hero kits; P06 camera/assets/3D pipeline; P07 giữ các hiệu ứng FUN phức tạp khi chuyển auto sang ACTIVE; không cho phép AUTO/HYBRID trong Map 01.

## 1. Combat loadout

Một Hero có các nút/kênh hành động riêng:
1. **Basic Attack:** đòn đánh cơ bản/combo tướng; KHÔNG bị tính là relic thứ hai.
2. **Dash:** di chuyển né đòn theo stats của Hero.
3. **Signature Skill:** kỹ năng riêng của Hero, KHÔNG sử dụng relic slot.
4. **Relic Slot 01:** đúng một vật phẩm trang bị; có thể tăng cấp chính relic này trong run.
5. **Build & Squad Commands:** tương tác Defense Mode, KHÔNG là equipment slot.

Giới hạn kỹ thuật hiện tại Projectzombie là WeaponManager.MAX_WEAPONS = 1; không tăng số này khi đổi GDD.

## 2. Danh mục ĐÃ CHỐT của 5 relic Map 01 [D04=A]

| ID từ Vongxuyen | Tên | Silhouette/indicator | Gameplay role |
| --- | --- | --- | --- |
| slipper | Dép Tổ Ong Thần Sa | đường bay / boomerang arc | Hit & return / CC |
| pot | Nồi Cơm Thạch Sanh | vòng tròn hút + hướng phóng | Gather & launch |
| pipe | Điếu Cày Cửu U | quạt khói / cone | Area & damage-over-time |
| mat | Chiếu Trải Hoàng Tuyền | hình chữ nhật trên mặt đất | Zone trap / sleep |
| broom | Chổi Lông Gà Gia Truyền | điểm rơi/sweep radius | High knockback |

**Phân biệt source:** Vongxuyen JS hiện có active cast/ghost aim và damage cơ bản; cơ chế FUN sâu hơn ở tài liệu VX có phần chưa thực hiện. Projectzombie Unity lại có nhiều hiệu ứng FUN runtime phức tạp hơn và có xu hướng auto/passive. Chỉ chốt UX thi triển tại D05, không áp lại mechanic prototype thấp cấp khiến Unity bị mất tính năng.

## 3. ACTIVE RELIC ONLY (D05=A — APPROVED)

**Không sử dụng AUTO/PASSIVE/HYBRID cho slot trang bị Map 01.** Mọi relic trong năm pháp bảo FUN cần có cách kích hoạt chủ động, nút Relic riêng, 3D aim ghost chuẩn hit shape, cast confirmation, cooldown, cancel.

- **State**: READY → AIMING → CAST_VALIDATION → EXECUTE → COOLDOWN → READY. CANCEL chỉ từ AIMING/VALIDATION và không trừ cooldown.
- **3D targeting**: Physics.Raycast xuống ground/XZ, target lane and collisions; indicator chiếu xuống đúng diện tích thật, không dùng Unity Physics2D shape làm nguồn hit.
- **One equip slot**: equip 1 vật phẩm trước run; Attack/Dash/Signature là Hero abilities, không chiếm relic slot. Upgrade không phát sinh slot nữa.
- **Effect retention**: Projectzombie đã có các hệ FUN/weapon effects phức tạp hơn prototype Vongxuyen; đánh giá từng relic P07 và chuyển sang cast controller mà không tắt mechanic hợp lệ.
- **Failure**: invalid range/terrain, paused, Hero downed hoặc Base GameOver phải hủy preview, không chi cooldown/wood. Hero death không tự đặt RunEnded(LOSE).
- **3D feedback**: chỉ dấu line/arc/circle/cone/rect/swept đúng collision volume, occlusion và priority bảo toàn Boss telegraph.

## 4. Quy tắc hiển thị vũ khí và VFX

- Props vũ khí cố định trên nhân vật (Identity Props) phải phân biệt với relic được trang bị.
- Một relic có thể spawn nhiều projectile/particle/zone nhưng vẫn **tính là một item**.
- Đồ họa không làm khuất máu Base, đội quân, tháp, telegraph Boss và lối qua cầu.
- VFX mức độ nguy hiểm cao có layer ưu tiên hơn hiệu ứng FUN; tất cả hiệu ứng có thời lượng và limit rõ.
- Aim shapes phải tương ứng damage hitbox thực, tránh vòng tròn giả không đúng tác dụng.
- Data-based visual: color, range, windup, launch socket, ghost shape, cooldown indicator, priority.
- **D02=C:** full stylized Unity 3D. Character models/rigs/sockets và đồ họa pháp bảo chuyển sang 3D; không lấy pixel-sprite or 2D Tilemap assets làm chuẩn Map 01. Camera/URP/render constraints còn P06.

## 5. Input mode priority

**MVP State exclusivity**: NORMAL | RELIC_AIM | BUILD | UPGRADE_TOWER | PAUSE | LEVEL_UP | GAME_OVER. **SQUAD_RALLY là POST-MVP (D10=C).**  
- Enter RELIC_AIM: hủy 3D Build ghost, ẩn tower selection; rally mode không triển khai trong MVP.
- Enter BUILD: hủy relic ghost, không tiêu cooldown; world-space selection bằng 3D raycast.
- Pause hoặc LevelUp: khóa attack/dash/relic/build/rally; resume khôi phục input mode hợp lệ duy nhất.
- Orientation/device interruptions: cancel pending cast/build, không tự trigger; raycast camera 3D must work for different aspect ratios.
- Touch target min-size, safe-area và UI collision check; đặt nút relic ở combat cluster, contextual build/squad panel không đè.

## 6. Code contracts

- WeaponManager: **1 slot** và one-active-item contract; giữ MAX_WEAPONS=1, không auto proc song song khi đã chuyển Map 01 sang active.
- WeaponData/RelicDefinitionSO: typeId, activationMode, targetShape, cooldown, range, effect/prefab references.
- SkillAimIndicatorController: chỉ tái sử dụng state/domain; cần **3D ghost indicator/Physics3D** preview adapter, đảm bảo shape and collision validity.
- InputCoordinator: arbiter giữa build/relic trong MVP; squad POST-MVP.
- RunHUDPresenter: view state / cooldown / owned relic.
- Upgrade cards: chỉ nâng cấp relic hiện có, không unlock slot mới.

## 7. Acceptance & QA

- [ ] Không thể mang nhiều hơn một relic trong cùng một run (kể cả save/load/upgrade).
- [ ] 5 typeId được map 1-1 sang Unity prefab/definition hoặc có trạng thái chưa implement.
- [ ] Không có tài liệu ACTIVE v6 vẫn hướng dẫn mang 6 relic.
- [ ] **ACTIVE cho cả 5 relic**: nút cast thật + ghost 3D hợp lệ; không AUTO/HYBRID/double trigger.
- [ ] Hủy aim không mất gỗ/cooldown; Tower Build không cast nhầm relic. Rally là POST-MVP.
- [ ] Runtime effects được giữ nếu port/thiết kế là tương thích; không downgrade vô ý.
- [ ] QA mobile full 3D: portrait/landscape, pause, boss telegraph, occlusion, tối ưu VFX.

## 8. Source links

- [Decision Record](DECISION_CHECKLIST_GDD_V6.md)
- [Unity 3D Migration](UNITY_3D_MIGRATION_PLAN.md)
- https://github.com/thuongtran1210/Projectzombie/blob/main/Assets/Features/Weapons/WeaponManager.cs
- https://github.com/thuongtran1210/Projectzombie/blob/main/GameDesignDoc/FUN_COMBAT_AND_ENEMY_MECHANICS_DOC.md
- https://github.com/thuongtran1210/Vongxuyen/blob/main/src/combat/HeroRelicSystem.js
- https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/MASTER-GAME-DESIGN-AND-COMPLETION-PLAN.md
