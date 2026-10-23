# TREBRAVE — SLIPPER-ONLY ACTIVE RELIC MVP — UNITY 3D UX SPEC v6.2

> **Trạng thái:** **D02=C, D03=A, D04=A, D05=A, P01/P07 owner-approved 2026-10-23** — 4 Hero chọn sảnh sử dụng ID và ScriptableObject, 1 Hero/run, 5-item FUN catalog nhưng **MVP chỉ Dép Tổ Ong ENABLED**, 1 ACTIVE relic/run, Unity full 3D target. Runtime 3D chưa port.  
> **Thay thế bản visual rules legacy** từng mô tả mang tối đa 6 pháp bảo. Lịch sử thiết kế cũ vẫn có trong Git.  
> **Cấu hình chưa khóa:** tên/kit 4 Hero (SO), cooldown/damage/range Dép (RelicDefinitionSO), camera/assets 3D. P07 MVP **không cần hiệu ứng FUN**; 4 relic còn lại LOCKED/POST-MVP. 

## 1. Combat loadout

Một Hero có các nút/kênh hành động riêng:
1. **Basic Attack:** đòn đánh cơ bản/combo tướng; KHÔNG bị tính là relic thứ hai.
2. **Dash:** di chuyển né đòn theo stats của Hero.
3. **Signature Skill:** kỹ năng riêng của Hero, KHÔNG sử dụng relic slot.
4. **Relic Slot 01:** đúng một vật phẩm trang bị; có thể tăng cấp chính relic này trong run.
5. **Build & Squad Commands:** tương tác Defense Mode, KHÔNG là equipment slot.

Giới hạn kỹ thuật hiện tại Projectzombie là WeaponManager.MAX_WEAPONS = 1; không tăng số này khi đổi GDD.

## 2. Danh mục 5 relic FUN — trạng thái availability ở MVP [P07 APPROVED]

| ID | Tên | MVP | Nội dung phải làm ở giai đoạn này |
| --- | --- | --- | --- |
| `slipper` | **Dép Tổ Ong Thần Sa** | **ENABLED / DEFAULT** | Nút cast chủ động, ghost 3D, xác thực hướng/tầm, projectile/hit basic, damage, cooldown và feedback đơn giản |
| `pot` | Nồi Cơm Thạch Sanh | **LOCKED** | Chỉ catalog/ID, không runtime effect/upgrade/cast |
| `pipe` | Điếu Cày Cửu U | **LOCKED** | Chỉ catalog/ID, không runtime effect/upgrade/cast |
| `mat` | Chiếu Trải Hoàng Tuyền | **LOCKED** | Chỉ catalog/ID, không runtime effect/upgrade/cast |
| `broom` | Chổi Lông Gà Gia Truyền | **LOCKED** | Chỉ catalog/ID, không runtime effect/upgrade/cast |

**Catalog có 5 món theo D04=A, 1 mở + 4 khóa.** Chủ dự án viết “khóa 3 món kia”; do phép đếm không khớp, tài liệu tạm khóa **toàn bộ 4 món còn lại** để đảm bảo MVP chỉ một pháp bảo cho tới khi có xác nhận danh mục khác. Không được âm thầm xóa món thứ năm.

## 3. Dép ACTIVE: minimal implementation, no advanced FUN FX [P07 APPROVED]

1. Hero vào Map 01 được equip 1 `slipper`; `MAX_WEAPONS=1` vẫn là invariant. Các relic có `isAvailableInDefenseMvp = false` không được equip qua saved loadout, level-up cards, shop, cheat/dev UI hoặc migration.
2. Bấm nút Relic → 3D GhostAim theo hướng/tầm đã config; kết quả raycast xuống Unity ground XZ. Hiện preview hình dải bay/điểm trúng.
3. Cast hợp lệ thì bắn dép/sinh projectile đơn giản, gây damage theo `RelicDefinitionSO`, phát feedback/animation tối thiểu, bắt đầu cooldown.
4. Cancel, invalid range, pause, Hero downed hoặc RunEnd không cast và **không tiêu cooldown**.
5. **Không bắt buộc** 3-hit combo, boomerang return, Quê Độ, knockback FUN nâng cao, multi-target synergy ở MVP. Hình ảnh dép nhận diện là đủ; effect chain mở rộng đặt giai đoạn sau.
6. Dùng `RelicDefinitionSO` để cấu hình unlocked, projectile speed, damage, cooldown, maxRange, preview, attack count. Không hardcode số hoặc bắt chước values prototype VX chưa được QA.
7. Những runtime FUN effects nâng cao từ Projectzombie hiện tại **giữ code/asset ở Legacy**; không bật tự động trong Defense Map 01. Khi đưa vào Defense ở Phase sau, phải kiểm thử từng effect.

**State:** READY → AIMING → VALIDATING → CAST → COOLDOWN → READY. CAST gây damage đúng một lần; ghost disposed khi đóng modal, Hero Downed hoặc run Victory.

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
- Pause hoặc LevelUp hoặc Hero Downed: khóa attack/dash/relic/build, chỉ resume khi điều kiện hợp lệ; boss death xử lý Victory và stop enemy spawning.
- Orientation/device interruptions: cancel pending cast/build, không tự trigger; raycast camera 3D must work for different aspect ratios.
- Touch target min-size, safe-area và UI collision check; đặt nút Dép ở combat cluster, contextual build panel không đè.

## 6. Code contracts

- WeaponManager: **1 slot** và one-active-item contract; giữ MAX_WEAPONS=1, không auto proc song song khi đã chuyển Map 01 sang active.
- WeaponData/RelicDefinitionSO: typeId, isAvailableInDefenseMvp, activationMode=ACTIVE, targetShape, cooldown, range, basicDamage, prefab references.
- SkillAimIndicatorController: chỉ tái sử dụng state/domain; cần **3D ghost indicator/Physics3D** preview adapter, đảm bảo shape and collision validity.
- InputCoordinator: arbiter giữa build/relic trong MVP; squad POST-MVP.
- RunHUDPresenter: view state / cooldown / owned relic.
- Upgrade cards: chỉ nâng cấp **Dép** đang có, không unlock slot mới hoặc một trong bốn relic khóa.

## 7. Acceptance & QA

- [ ] Không thể mang nhiều hơn một relic trong cùng một run (kể cả save/load/upgrade).
- [ ] 5 item IDs tồn tại trong catalog, nhưng chỉ `slipper` có Unity 3D runtime/prefab và `isAvailableInDefenseMvp=true`; 4 món kia LOCKED.
- [ ] Không có tài liệu ACTIVE v6 vẫn hướng dẫn mang 6 relic.
- [ ] **ACTIVE cho Dép duy nhất**: nút cast thật + ghost 3D hợp lệ + damage/cooldown cơ bản; không AUTO/HYBRID/FUN effects nâng cao.
- [ ] Hủy aim không mất gỗ/cooldown; Tower Build không cast nhầm relic. Rally là POST-MVP.
- [ ] Không xóa code/asset runtime FUN effects Legacy; chỉ không triển khai chúng trên Defense MVP theo P07.
- [ ] QA mobile full 3D: Dép aim/cast/disable while downed, portrait/landscape, pause, boss telegraph, occlusion, tối ưu VFX.

## 8. Source links

- [Decision Record](DECISION_CHECKLIST_GDD_V6.md)
- [Unity 3D Migration](UNITY_3D_MIGRATION_PLAN.md)
- https://github.com/thuongtran1210/Projectzombie/blob/main/Assets/Features/Weapons/WeaponManager.cs
- https://github.com/thuongtran1210/Projectzombie/blob/main/GameDesignDoc/FUN_COMBAT_AND_ENEMY_MECHANICS_DOC.md
- https://github.com/thuongtran1210/Vongxuyen/blob/main/src/combat/HeroRelicSystem.js
- https://github.com/thuongtran1210/Vongxuyen/blob/main/docs/MASTER-GAME-DESIGN-AND-COMPLETION-PLAN.md
