# PROJECTZOMBIE — ONE HERO / ONE RELIC VISUAL & INPUT SPEC v6 (DRAFT)

> **Trạng thái:** PROPOSED. **Quy tắc bất biến:** **mỗi run một Hero do người chơi điều khiển và một relic equipped**.  
> **Thay thế bản visual rules legacy** từng mô tả mang tối đa 6 pháp bảo. Lịch sử thiết kế cũ vẫn có trong Git.  
> **Cần phê duyệt:** D03 số Hero ở sảnh, D04 danh mục relic, D05 ACTIVE/AUTO/HYBRID, D02 phong cách art 2D/3D.

## 1. Combat loadout

Một Hero có các nút/kênh hành động riêng:
1. **Basic Attack:** đòn đánh cơ bản/combo tướng; KHÔNG bị tính là relic thứ hai.
2. **Dash:** di chuyển né đòn theo stats của Hero.
3. **Signature Skill:** kỹ năng riêng của Hero, KHÔNG sử dụng relic slot.
4. **Relic Slot 01:** đúng một vật phẩm trang bị; có thể tăng cấp chính relic này trong run.
5. **Build & Squad Commands:** tương tác Defense Mode, KHÔNG là equipment slot.

Giới hạn kỹ thuật hiện tại Projectzombie là WeaponManager.MAX_WEAPONS = 1; không tăng số này khi đổi GDD.

## 2. Danh mục relic mục tiêu Map 01 [D04]

| ID từ Vongxuyen | Tên | Silhouette/indicator | Gameplay role |
| --- | --- | --- | --- |
| slipper | Dép Tổ Ong Thần Sa | đường bay / boomerang arc | Hit & return / CC |
| pot | Nồi Cơm Thạch Sanh | vòng tròn hút + hướng phóng | Gather & launch |
| pipe | Điếu Cày Cửu U | quạt khói / cone | Area & damage-over-time |
| mat | Chiếu Trải Hoàng Tuyền | hình chữ nhật trên mặt đất | Zone trap / sleep |
| broom | Chổi Lông Gà Gia Truyền | điểm rơi/sweep radius | High knockback |

**Phân biệt source:** Vongxuyen JS hiện có active cast/ghost aim và damage cơ bản; cơ chế FUN sâu hơn ở tài liệu VX có phần chưa thực hiện. Projectzombie Unity lại có nhiều hiệu ứng FUN runtime phức tạp hơn và có xu hướng auto/passive. Chỉ chốt UX thi triển tại D05, không áp lại mechanic prototype thấp cấp khiến Unity bị mất tính năng.

## 3. AUTO vs ACTIVE vs HYBRID [D05]

**ACTIVE (khuyến nghị):**
- Combat HUD có 1 nút pháp bảo bên cạnh Attack / Dash / Signature.
- Tap/hold mở aim preview hợp lệ, người chơi drag/thả để cast.
- Cooldown chỉ bắt đầu khi cast thành công; hủy/chọn ngoài tầm không tiêu cooldown.
- Thao tác pháp bảo phải cancel tower build/rally mode an toàn.

**AUTO (nếu chọn):**
- Relic tự kích hoạt theo cooldown/proc; HUD chỉ cần icon + status, không có phantom button cast không tác dụng.
- Tooltip giải thích target policy và proc.
- Không buộc mọi relic giả lập “quăng nút” nếu đã code auto.

**HYBRID (nếu chọn):**
- Một RelicDefinition quy định activationMode, inputMode, indicator, targeting và effect chain.
- UI khác nhau tùy loại, vẫn một equipped slot.
- Không cho Hero kích hoạt cả auto và manual cùng tạo hai instance relic active trái hợp đồng.

## 4. Quy tắc hiển thị vũ khí và VFX

- Props vũ khí cố định trên nhân vật (Identity Props) phải phân biệt với relic được trang bị.
- Một relic có thể spawn nhiều projectile/particle/zone nhưng vẫn **tính là một item**.
- Đồ họa không làm khuất máu Base, đội quân, tháp, telegraph Boss và lối qua cầu.
- VFX mức độ nguy hiểm cao có layer ưu tiên hơn hiệu ứng FUN; tất cả hiệu ứng có thời lượng và limit rõ.
- Aim shapes phải tương ứng damage hitbox thực, tránh vòng tròn giả không đúng tác dụng.
- Data-based visual: color, range, windup, launch socket, ghost shape, cooldown indicator, priority.
- Phong cách 2D hoặc 3D sẽ theo D02; không tái sử dụng quy tắc pixel hay rig 3D của engine kia khi chưa có conversion spec.

## 5. Input mode priority

**State exclusivity**: NORMAL | RELIC_AIM | BUILD | UPGRADE_TOWER | SQUAD_RALLY | PAUSE | LEVEL_UP | GAME_OVER.  
- Enter RELIC_AIM: hủy Build ghost, ẩn tower selection/rally preview.
- Enter BUILD: hủy relic ghost, không tiêu cooldown.
- Pause hoặc LevelUp: khóa attack/dash/relic/build/rally; resume khôi phục input mode hợp lệ duy nhất.
- Orientation/device interruptions: cancel pending cast/build, không tự trigger.
- Touch target min-size, safe-area và UI collision check; đặt nút relic ở combat cluster, contextual build/squad panel không đè.

## 6. Code contracts

- WeaponManager: one slot và quản lý relic object.
- WeaponData/RelicDefinitionSO: typeId, activationMode, targetShape, cooldown, range, effect/prefab references.
- SkillAimIndicatorController: dùng lại preview API cho ACTIVE, đảm bảo shape and collision validity.
- InputCoordinator: arbiter giữa build/relic/squad.
- RunHUDPresenter: view state / cooldown / owned relic.
- Upgrade cards: chỉ nâng cấp relic hiện có, không unlock slot mới.

## 7. Acceptance & QA

- [ ] Không thể mang nhiều hơn một relic trong cùng một run (kể cả save/load/upgrade).
- [ ] 5 typeId được map 1-1 sang Unity prefab/definition hoặc có trạng thái chưa implement.
- [ ] Không có tài liệu ACTIVE v6 vẫn hướng dẫn mang 6 relic.
- [ ] AUTO không hiển thị nút cast giả; ACTIVE ghost aim hợp lệ; HYBRID không double trigger.
- [ ] Hủy aim không mất gỗ/cooldown; build/rally không cast nhầm relic.
- [ ] Runtime effects được giữ nếu port/thiết kế là tương thích; không downgrade vô ý.
- [ ] QA mobile: portrait/landscape, pause, boss telegraph, tối ưu VFX.

## 8. Source links
- https://github.com/thuongtran1210/Projectzombie/blob/main/Assets/Features/Weapons/WeaponManager.cs
- https://github.com/thuongtran1210/Projectzombie/blob/main/GameDesignDoc/FUN_COMBAT_AND_ENEMY_MECHANICS_DOC.md
- https://github.com/thuongtran1210/Vongxuyen/blob/main/src/combat/HeroRelicSystem.js
- https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/MASTER-GAME-DESIGN-AND-COMPLETION-PLAN.md
