# PROJECTZOMBIE — VONGXUYEN ENEMY & BOSS ROSTER (DRAFT)

> **Trạng thái:** REFERENCE-VX → PROPOSED-PZ. Chưa có kết luận các archetype đã được port/QA trên Unity.  
> **Nguồn runtime:** Vongxuyen src/entities/EnemyFactory.js, src/combat/WaveManager.js, src/combat/YakshaBoss.js, src/config/stage01.js.  
> **Câu hỏi mở:** D06 thời lượng/Boss, D07 thắng/thua, D09 phạm vi quái cũ.

## 1. Mục tiêu hệ thống

Map 01 của Projectzombie sử dụng **enemy identity, vai trò, khả năng tấn công và cách triển khai theo lane** của Vongxuyen. Không giữ roster Ma Đòi Nợ/Ngưu Đầu–Mã Diện/Diêm Vương làm “kẻ địch chuẩn Map 01” trong cùng một GDD. Những enemy cũ vẫn còn trong repo Unity và phải qua quyết định keep/deprecate/reassign, không tự xóa.

**Tiêu chí của mọi enemy:**
- Có ID ổn định, displayName, vai trò, HP, tốc độ, damage, range, attack cooldown, target policy, navigation mode, reward và addressable asset.
- Phân biệt prototype values với balance target; enemy boss không phải enemy thường phóng to.
- Attack chỉ resolve khi hợp lệ, có telegraph cho đòn nguy hiểm; enemy chết chỉ award một lần.
- Support map lane, faction, pooling, hit reaction, death cleanup, game-state pause, hitbox 2D (nếu D02 chọn Unity 2D).
- Không mặc định mọi enemy bay có thể xuyên mọi vật cản hoặc tấn công Base từ dưới nước.

## 2. Roster được kiểm chứng từ factory/runnable code Vongxuyen

| ID kỹ thuật | Tên hiển thị | Vai trò gameplay | HP cơ sở VX | Speed cơ sở VX | Damage cơ sở VX | Nguồn xuất hiện |
| --- | --- | --- | ---: | ---: | ---: | --- |
| magiap | Ma Giáp Cương Thi | Ground bruiser / armor | 130 | 1.45 | 16 | STAGE_01 theo vùng |
| thuyquai | Thủy Quái Rạch Đêm | Amphibious / water ambush | 95 | 2.05 | 12 | STAGE_01 vùng nước |
| huyetdoi | Huyết Dơi Đêm | Fast flying skirmisher | 50 | 3.10 | 8 | STAGE_01 vùng Bắc |
| magiap_boss | Ma Giáp Thống Lĩnh | Early elite/boss | 700 | 1.18 | 24 | Event thứ 7 tại t = 108s |
| quaidu | Tiểu Quỷ Nón Lá Dù | Air-drop raider | 90 | 1.75 | 14 | Factory + Ghost Cannon |
| phicuong | Phi Cương Tinh Anh | Flying elite | 260 | 1.65 | 18 | Factory/special spawn |
| yaksha_boss | Tướng Dạ Xoa — Trấn Bến Vong Môn | Major arena boss | 3,200 | 1.35 | 38 | Boss runtime tại khoảng t = 510s |

Các giá trị ở cột HP/speed/damage là trực tiếp từ factory/Boss, trước waveMultiplier và buff; **không** phải balance final trong Unity. Boss Dạ Xoa có attack đặc biệt gây damage khác damage thường.

## 3. Đặc tả từng enemy

### E01 — Ma Giáp Cương Thi (magiap)
- Nhận diện: giáp cổ, lá bùa vàng trước trán, thân hình cương thi.
- Vai trò: quái áp lực chậm/vừa, có khả năng tiếp cận tuyến tháp và sát thương ổn định.
- Navigation: Ground lane graph; qua các cầu; bị chặn bởi defensive front.
- AI: SeekLane → TargetEvaluate → Approach → Attack → Recover → Reposition. Nếu không target được, quay về lane path; không kẹt cạnh Base.
- Counters: đánh tầm xa/tower, giữ lane bằng Dân Binh.
- Test: xuyên sông = fail; attack cooldown pause; reset pool xóa target cũ.

### E02 — Thủy Quái Rạch Đêm (thuyquai)
- Nhận diện: màu nước xanh lục, vây lưng phát sáng, móng vuốt.
- Vai trò: địch đánh từ vùng nước/cạnh cầu tạo rủi ro cho vị trí tháp và hero.
- Navigation: WaterZonePath, chuyển qua điểm xuất bờ hợp lệ; không tùy tiện “dùng graph ground” để cắt nước.
- AI: patrol/approach water access point → ambush target → retreat/chase policy; mức độ amphibious phải QA thực tế trên Unity.
- Counters: bảo vệ lối ra bờ và phản ứng lane warning.
- Test: không spawn trong collider solid, không đứng bất động ngoài attack range, không bị kẹt tại mép bờ.

### E03 — Huyết Dơi Đêm (huyetdoi)
- Nhận diện: dơi tím đen, mắt đỏ, silhouette cánh rõ.
- Vai trò: fast skirmisher, đe dọa Hero/sau tuyến nếu không có target coverage.
- Navigation: FlyingPath/waypoints có luật giới hạn vùng; không phụ thuộc bridge ground nếu sản phẩm cho phép bay qua sông.
- AI: approach → swoop/attack → cooldown reposition; tần suất đổi target có giới hạn.
- Test: cảm giác hitbox/aim trên Mobile; không spawn trực tiếp trên player; không bị giữ trong lane wall.

### E04 — Ma Giáp Thống Lĩnh (magiap_boss)
- Nhận diện: cương thi mặc giáp lớn, lá bùa đỏ, cờ lệnh và đại đao.
- Vai trò: boss áp lực sớm (STAGE_01 event 7 tại **108s**, không phải boss cuối 600s).
- BASELINE: 700 HP, 1.18 speed, 24 damage, attackRange 1.45 (chỉ số trước multiplier).
- AI: approach strategic target, có slam/claw/telegraph tùy implementation đang có trong WaveManager, có thể triệu hồi lính; các đòn phải mô tả damage time rõ.
- Test: đúng mốc stage event, không spawn lặp khi pause, không bị xoá khi spawn queue full, boss phase độc lập khỏi kill reward.

### E05 — Tiểu Quỷ Nón Lá Dù (quaidu)
- Nhận diện: nón lá bùa, hình dáng nhỏ, dao găm.
- Vai trò: raider đột kích; được tạo trong factory và dùng ở tương tác Ghost Cannon.
- BASELINE: 90 HP, 1.75 speed, 14 damage.
- AI: SpawnDrop → LandingTelegraph → Approach/Attack; cần thống nhất rơi xuống có gây damage trực tiếp và spawn limit.
- Test: landing không lên nước/tháp, thông báo vị trí rõ, pooling của raider không nhân quái vô hạn.

### E06 — Phi Cương Tinh Anh (phicuong)
- Nhận diện: cương thi bay, giáp tinh anh vàng.
- Vai trò: high HP elite hỗ trợ wave/đột phá.
- BASELINE: 260 HP, 1.65 speed, 18 damage, attackRange 1.35.
- Navigation: flying hoặc hover policy; không khóa cứng route từ Ma Giáp ground.
- Test: layer targeting và projectile hit hợp lệ; UI elite, VFX/health bar và xử lý knockback.

### B01 — Tướng Dạ Xoa (yaksha_boss)
- Nhận diện: Tướng Dạ Xoa cầm đại đao tại khu vực Vong Môn bờ Đông.
- BASELINE: 3,200 HP, 1.35 speed, 38 damage; xuất hiện khoảng **08:30 = 510s** theo lớp YakshaBoss.
- AI có state **idle, chase, cleave_windup, slam_windup, stunned, die**; có ba nhóm skill cooldown tham chiếu cleave 3.5s, slam 8.5s, crescentWave 12.0s.
- Đòn ví dụ: đại đao quét, khai sơn phá thạch, đao khí. Telegraph phải mở trước hit frame và cho người chơi cửa né/interrupt đúng.
- Boss phải có phase gating theo thời gian/run và chiến thuật raid/hạn chế chase theo bờ.
- **Chưa chốt:** hạ Dạ Xoa có bắt buộc để thắng ở 600s? Dạ Xoa có trực tiếp đập Nhà Chính hay chỉ đấu arena? Câu trả lời tại D07.
- Test: Boss spawn duy nhất, UI HP/vfx đúng, phase chuyển đúng, boss death có cleanup/cancel projectile và game result không trùng.

## 4. Wave 01 tham chiếu

Stage Vongxuyen định nghĩa:
- duration = 600s
- 34 events, at = eventIndex × 18s (index 0–33)
- phase = min(5, floor(at/120)+1)
- normal roster cycling: magiap/eastBank; huyetdoi/northCanopy; thuyquai/canalWater; magiap/eastBank; magiap/southBank
- event index 6: magiap_boss, count = 1
- các event normal có count = min(8, 3+phase+(index mod 2))
- maxActiveEnemies = 32, maxPendingSpawns = 40, spawnBudgetPerFrame = 4, pendingLifetime = 25s, minSpawnDistance = 3.1.

**Không kết luận:** mọi 34 event đều có thể spawn đủ khi cap đầy; các event tùy lịch không bao gồm quaidu/phicuong/yaksha. Chúng dùng cơ chế spawn riêng. Port sang Unity phải quyết định scheduler eventId mapping, queue expiry/retry và spawn preservation với boss.

## 5. Tầng AI & mục tiêu công trình Unity

Projectzombie hiện có Enemy FSM và ITargetSelector trả **PlayerContext**. Thay vì sửa tất cả enemy để trả Transform hay IDamageable mới, thiết kế:
1. **DefenseTargetCatalog:** đăng ký Hero, Base, Tower, Allies, với Faction, Position, HP, TargetKind.
2. **DefenseTargetPolicy:** nhận role, lane, current path, threat table, trả mục tiêu có thể tiếp cận và attack hợp lệ.
3. **EnemyTargetAdapter:** hỗ trợ current PlayerContext flow cho Survival trong khi Defense dùng policy mới.
4. **NavigationAdapter:** chuyển đến target qua WaypointGraph và terrain permission.
5. **AttackTargetAdapter:** nối damage events sang IDamageable và damage feedback; tránh assume PlayerHealthSystem cho mọi target.
6. **Fallback:** khi target bị phá/hết HP, cập nhật mục tiêu và route, không đứng Idle vô thời hạn nếu Base vẫn còn.

**Acceptance:** enemy có thể chuyển từ đánh Hero sang Tower/Base khi điều kiện đúng; không gây ngoại lệ casting target; dân binh chặn được tuyến nếu còn sống; dead target không được nhận đòn; đảm bảo backward compatibility Survival.

## 6. Quy tắc chuyển đổi asset

Từ web 3D sang Unity 2D (nếu D02 = 2D):
- Giữ role, silhouette, tên, dấu hiệu nhận biết, âm thanh và gameplay; không sao chép Three.js material/mesh trực tiếp.
- Prefab mỗi enemy: SpriteRenderer/Animator, Collider2D, Health/Target, AI, DamageReceiver, Audio/VFX, Projectile/Hitbox, pooling reset.
- Boss telegraph được dựng bằng URP shader/mesh/UI world-space; hình dạng hit phải trùng VFX.
- Asset path và ID không bị đổi giữa GDD và ScriptableObject; lưu source art/provenance khi tạo asset mới.
- Unit tests về domain; PlayMode tests về spawn, layer, collision, state transitions và navigation.

## 7. Old Projectzombie enemy reconciliation

| Tài liệu trước đây | Map 01 mới | Hướng xử lý trước khi duyệt D09 |
| --- | --- | --- |
| Ngưu Đầu / Mã Diện | Không thuộc Wave 01 Vongxuyen | Giữ code/asset, đưa legacy/next map |
| Diêm Vương 20:00 | Không là Boss Dạ Xoa 510s | Xóa khỏi luật Map 01 v6, chờ quyết định |
| Ma Đòi Nợ (FUN Doc) | Không thuộc baseline | Không spawn vào Map 01 mặc định |
| Quái Projectzombie khác | Không rõ mapping | Audit registry SO/prefab, không delete tự động |

**Không xóa asset, save key hay script cũ chỉ vì đổi GDD.** Nếu sau này loại bỏ, cần dependency graph, migration, PlayMode regression và PR riêng.

## 8. Data schema đề xuất

**EnemyDefinitionSO**
- enemyId (string, immutable)
- displayName, category (Normal/Elite/Boss), role
- health, attackDamage, attackRange, attackCooldown, moveSpeed, resistances
- navMode (Ground/Flying/Water), permittedZones, allowedLanes
- targetPriorityWeights (Hero/Tower/Ally/Base)
- attackSet, telegraphSet, hitboxShape, rewardWood, rewardExp
- prefabReference, VFX, audio, animationProfile, poolKey.

**DefenseWaveEvent**
- eventId (unique per scenario), atSeconds, typeId, zoneId, laneId (optional), count, isBoss, priority, spawnGroupId.

**BossPhaseDefinition**
- phaseId, min/max healthPercent, minTime, attackSet, spawnPolicy, phaseTelegraph.

## 9. QA checklist

- [ ] 7 reference types have definition, prefab or explicit unavailable status.
- [ ] E01/E02/E03 navigate correct intended terrain without crossing invalid water/bridges.
- [ ] E04 event at 108s fires exactly once unless overridden by versioned scenario.
- [ ] E05 drop indicators appear before impact and no infinite child spawn.
- [ ] E06 displays elite health bar; flying targeting works.
- [ ] B01 at 510s (if selected) and every telegraph leads damage only after windup.
- [ ] Boss attack cannot damage destroyed/unreachable target; run end cancels boss projectiles.
- [ ] Wood/EXP/kill only once per enemy death; no pooled stale IDs.
- [ ] Spawner honors cap and backpressure, no permanent queue on reset.
- [ ] Survival Mode enemy roster unchanged unless D01 deliberately replaces that mode.
- [ ] D09 approved: legacy enemy plan documented.

## 10. Links

- https://github.com/thuongtran1210/Vongxuyen/blob/main/src/entities/EnemyFactory.js
- https://github.com/thuongtran1210/Vongxuyen/blob/main/src/combat/WaveManager.js
- https://github.com/thuongtran1210/Vongxuyen/blob/main/src/combat/YakshaBoss.js
- https://github.com/thuongtran1210/Vongxuyen/blob/main/src/config/stage01.js
- https://github.com/thuongtran1210/Projectzombie/blob/main/Assets/Features/Enemies/ITargetSelector.cs
- https://github.com/thuongtran1210/Projectzombie/blob/main/Assets/Features/Enemies/FSM/Enemy.cs
