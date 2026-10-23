# D-07 — CHECKLIST REVIEW: Base, Hero, Enemy, Tower & Economy Balance

**Status:** `D-07-01/A APPROVED (2026-10-23) / D-07-02..08 PENDING / D-07 tổng thể CHƯA DUYỆT` · **chỉ lập kế hoạch, KHÔNG CODE / KHÔNG ĐỔI CHỈ SỐ TRONG UNITY**
**Phụ thuộc đã duyệt:** D-03/A (WIN sau >=600s + Boss chết + Base sống), D-04/A (Hero Down, hồi sinh sau 5 gameplay s), D-05 (@108 chỉ 1 miniboss), D-06/A (8/8 Enemy Ground/Water/Air/Landing/GhostCannon), D-11/A (1 Phi Cương thay 1 suất mỗi event Ma Giáp).
**Mục tiêu review:** chọn **nguyên tắc cân bằng** rồi mới khóa số; không nhập thông số Web vào Unity mặc định hoặc ghi đè scene / Inspector.

## 1. Audit chỉ số NGUỒN vs UNITY prototype (từ mã đã kiểm tra)

### 1.1. Nhà Chính, tài nguyên và tháp

| Hạng mục | Vongxuyen / Three.js | Projectzombie / Unity Defense3D | Phân tích chênh lệch |
|---|---|---|---|
| Nhà Chính tối đa | **500 HP** — `DefenseRules.js` | **1000 HP** — `DefenseBasePrototype3D.cs` | Unity x2 Web |
| Gỗ khởi điểm | **100** — `DefenseRules.js` | **0** — `DefenseWoodWallet3D.cs` | Web cho xây ngay, Unity yêu cầu nhặt trước |
| Xây tháp cơ bản | **50 Gỗ** | **25 Gỗ** | Unity = 1/2 Web |
| Bán kính tầm đánh tháp | **7m** Web; mỗi level thêm **1m** | **8m** Unity | Unity chưa có level tower |
| HP tháp lúc xây | **250 HP** Web | prototype Unity không có HP/combat-health riêng được xác nhận | khó nghiệm thu repair/destroy |
| Nâng cấp tháp | Web level 1→2 tốn **40**, level 2→3 tốn **80**; max Lv3; +100 maxHP mỗi cấp | Unity chưa triển khai nâng cấp | nếu đưa sang Unity phải data-driven |
| Sửa tháp / sửa Base | `DefenseRules.repairTower(amount=25)` và `repairBase(amount=15)` tăng HP; bản thân hàm này **không trừ Gỗ** | chưa triển khai repair | chính sách giá/tần suất phải duyệt, tránh hồi phục vô hạn |
| Thưởng Gỗ khi giết quái | `DefenseRules.earnKill(amount=10)` tăng ví trực tiếp (helper), integration runtime có thể khác | quái rơi **WoodDrop**, Hero nhặt mới được **+10 Gỗ** (prototype) | cần giữ một authority reward, ngăn nhận hai lần |
| Vị trí tháp | **10 strategic slots**, snap / spacing | **10 TowerSocket** marker | tọa độ scene local cần xác minh trước khi áp data |

**Lưu ý:** Web `DefenseRules.js` là module luật có default, chưa chứng minh mọi tham số đều giữ nguyên trong từng run cụ thể. Unity serialized fields có thể đã bị Inspector override trong scene local, **giá trị mã không bảo đảm là runtime value tại máy người dùng**.

### 1.2. Quái và Boss (HP chưa nhân phase)

| Loại Enemy | HP Web | Speed Web | Damage Web | Unity prototype |
|---|---:|---:|---:|---|
| `magiap` | 130 | 1.45 | 16 | EnemyWalker default 100 HP, 2.5 speed, 25 Base damage; chưa data per type |
| `thuyquai` | 95 | 2.05 | 12 | chung Walker |
| `huyetdoi` | 50 | 3.10 | 8 | chung Walker |
| `quaidu` | 90 | 1.75 | 14 | chung Walker |
| `phicuong` | 260 | 1.65 | 18 | chung Walker |
| `magiap_boss` | 700 | 1.18 | 24 | Unity spawn 350 HP thử nghiệm |
| `yaksha_boss` | **3200** (Web `YakshaBoss.js`) | cần đọc config Boss riêng trước khóa speed/attack | Boss có skill riêng | Unity `DefenseRun6003D`: **1200 HP** |

- `EnemyFactory.js` Web nhân **HP** với `waveMultiplier`; `WaveManager.updateTimedSpawns()` dùng `1+(phase-1)*0.18` cho spawn stage thường. **Boss Dạ Xoa dùng module riêng**, không lấy HP từ ordinary EnemyFactory.
- Một Enemy Unity mới có thể nhận damage của Hero 35 mỗi 0.65s hoặc Tower 22 mỗi 0.9s. Các con số này là **prototype**, chưa thể kết luận 600s quá dễ/quá khó nếu chưa test số lượng/phân bố/AI.
- **D-05/D-11 được duyệt:** stage 34 events, 214 suất theo config, 20 Phi Cương thay 20 suất Ma Giáp, **không thêm tổng số**.
- **D-06/A** yêu cầu quái bay thật + anti-air + 3 Linh Pháo, nên dự đoán DPS nếu chỉ tính distance X–Z sẽ sai; phải test va chạm/range 3D.

### 1.3. Hero và EXP

| Hạng mục | Vongxuyen | Unity prototype | Cần chốt |
|---|---|---|---|
| HP Hero | từ Player/character config, **chưa đưa giá trị vào bảng vì chưa audit đủ char data** | `DefenseHeroHealth3D` default **200 HP** | HP theo Hero class; baseline |
| Auto attack Hero | Web có hệ thống combat đa nguồn | 35 dmg/0.65s, range 5m | DPS và anti-air source |
| EXP & 3-card | `config/experience.js`: baseExp 36, growthFactor 1.32, maxLevel50; gemLifetime90s, active100, pickup0.68m, magnet2.35m, speed4.8 | chưa có module Defense3D tương đương được xác nhận | level curve, gem/reward queue, balancing |
| EXP mỗi enemy (Web) | Ma Giáp 14; Thủy Quái20; Huyết Dơi12; Phi Cương45; Tiểu Quỷ16; Miniboss95; GhostCannon80 | chưa có EXP reward pipeline cho Defense3D | data ID → XP amount, one-shot grant |
| Hồi sinh | Web master có phương án khác; **D-04/A đã chốt** | prototype 5s | **5 gameplay seconds đã duyệt**; HP sau hồi sinh là tham số D-07 |

## 2. D-07 — 1 mục đã duyệt, 7 mục còn chờ

### D-07-01 — Triết lý cân bằng và nguồn giá trị ban đầu
- [x] **A — APPROVED 2026-10-23:** snapshot dữ liệu Vongxuyen + Unity hiện tại; dùng Web làm **starting benchmark/role identity**, chọn **Unity balance profile v0** riêng, sau đó tune từ 600s PlayMode; không auto-copy con số giữa engine.
- [ ] **B:** chép đúng mọi số Web vào Unity trước khi test.
- [ ] **C:** giữ nguyên tất cả số prototype Unity.
- [ ] Khác: __________

**Biên bản D-07-01/A đã duyệt:** Chủ dự án chỉ phê duyệt **nguồn/tham chiếu cân bằng**. Vongxuyen là benchmark, Unity dùng profile riêng có version, việc điều chỉnh số dựa trên PlayMode. **Không phê duyệt các giá trị Web/Unity mặc định và không phê duyệt D-07-02..08.**

### D-07-02 — BaseHP + Gỗ đầu trận
- [ ] **A — ĐỀ XUẤT:** bảo đảm gameplay tutorial/cửa sổ xây tháp đầu trận, chọn **starting wood đủ xây ít nhất 1 tháp** ở baseline thử nghiệm; Base HP là config có version; **chưa khóa** 500/1000 cho production cho tới khi thử full-run.
- [ ] **B:** lấy cặp Web mặc định `500 HP / 100 Wood`.
- [ ] **C:** giữ cặp Unity `1000 HP / 0 Wood`.
- [ ] Khác / đề xuất số cụ thể: __________

**Lý do:** D-03/A bắt buộc bảo vệ đủ 600s. Với 0 Gỗ đầu trận, cần kill+nhặt WoodDrop rồi mới xây; với Enemy bay thực theo D-06/A, nguy cơ game khó bất hợp lý nếu không có tháp anti-air khi bắt đầu.

### D-07-03 — Tower cấp độ / giá / tầm / độ bền
- [ ] **A — ĐỀ XUẤT:** giữ cấu trúc Web **10 slot, tối đa Lv3**, chi phí `buildCost, upgradeCost[1→2], upgradeCost[2→3]`, HP/damage/range/fireInterval theo profile; số Web 50/40/80/250HP/range7m là baseline tham khảo, không khóa cứng.
- [ ] **B:** nhập đúng Web 50 Gỗ xây; +40/+80 nâng, 250 HP, 7m.
- [ ] **C:** giữ Unity 25 Gỗ, range8m, tháp không HP/nâng cấp trong Core.
- [ ] Khác: __________

### D-07-04 — Repair, chống hồi máu vô hạn, tài nguyên
- [ ] **A — ĐỀ XUẤT:** repair Tower/Base có **chi phí Gỗ + cooldown/giới hạn/lượng hồi** có config; không dùng hàm Web `repairTower`/`repairBase` miễn phí không giới hạn. Có transaction atomic đảm bảo chỉ trừ phí khi repair thành công.
- [ ] **B:** giữ hành vi helper Web tăng 25 Tower HP hoặc 15 Base HP, không trừ Gỗ trong helper.
- [ ] **C:** không cho sửa Base/Tower trong Core, tránh bơm kinh tế.
- [ ] Khác: __________

### D-07-05 — WoodDrop & EXP là hai luồng thưởng
- [ ] **A — ĐỀ XUẤT:** **Gỗ và EXP là tài nguyên khác nhau, theo pickup vật lý**, death/reward đúng một lần; Web `earnKill(10)` là reference nhưng không phát Gỗ trực tiếp nếu đã có WoodDrop; event type data quyết định reward/expiry/cap. Boss/cannon reward không tự cộng hai lần.
- [ ] **B:** giết quái là vào ví Gỗ/EXP ngay, không cần nhặt.
- [ ] **C:** Gỗ nhặt, EXP trực tiếp, hoặc cơ chế lai khác (ghi rõ): __________

### D-07-06 — Stat Enemy, phase scaling & anti-air
- [ ] **A — ĐỀ XUẤT:** dùng ID gốc + bảng HP/speed/damage Web làm baseline **riêng theo Enemy**; hệ số theo phase có cap; AI bay/đổ bộ giữ role D-06/A; balance bắn quái bay qua test Tower/Hero, không dùng chung 100 HP/25 damage cho mọi Enemy.
- [ ] **B:** copy raw Web stats + `1+0.18*(phase-1)` HP scaling ngay.
- [ ] **C:** giữ Enemy prototype Unity cùng chỉ số, khác hình ảnh.
- [ ] Khác: __________

### D-07-07 — Boss HP, difficulty và điều kiện thắng D-03/A
- [ ] **A — ĐỀ XUẤT:** Yaksha **tách Boss balance profile**, lưu reference 3200 Web/1200 Unity, kết hợp skill và uptime Hero/Tower rồi quyết định HP sau full-run; Miniboss có profile riêng, không mặc định 350 hay 700. **Không thay đổi D-03/A**.
- [ ] **B:** Boss 3200 HP / Miniboss700 HP theo Web.
- [ ] **C:** Boss1200 HP / Miniboss350 HP như Unity hiện tại.
- [ ] Khác: __________

### D-07-08 — Benchmark, Fast Test & khóa số chính thức
- [ ] **A — ĐỀ XUẤT:** config **Versioned `BalanceProfile`** + `FastTestProfile` tách biệt; kiểm thử 3 kịch bản đầu/mid/cuối run 600s, Enemy 5 loại + cannon + Yaksha, đếm lần xây/nâng tháp, Gỗ/EXP earned-vs-spent, Hero Down, Base HP và Overtime; chỉ chuyển v0→v1 sau **PlayMode evidence và owner duyệt**.
- [ ] **B:** chỉnh số Inspector tùy tay sau mỗi run, không lưu dữ liệu phiên bản.
- [ ] **C:** không cân bằng cho đến khi model hoàn chỉnh.
- [ ] Khác: __________

## 3. QA Balance Checklist (đề xuất, chưa chạy)

- [ ] **D07-QA-01:** xuất báo cáo diff cấu hình Web vs Unity scene Inspector thật (không coi C# default là runtime).
- [ ] **D07-QA-02:** ở cấu hình khởi đầu có phương án xây Tower trước đợt nguy hiểm; không phát Gỗ hai lần.
- [ ] **D07-QA-03:** xây/nâng 1/2/3 cấp, Gỗ trừ chính xác một lần, không vượt max Lv3, 10 slot không trùng.
- [ ] **D07-QA-04:** repair HP clamp đúng, không heal lúc full/HP0 khi luật cấm, không tiêu Gỗ nếu repair không xảy ra, pause đúng.
- [ ] **D07-QA-05:** 5 Enemy IDs + `magiap_boss` + Yaksha có profile hợp lệ/unique, phase scaling và damage theo loại.
- [ ] **D07-QA-06:** D-11/A vẫn có 1 elite/event, 34 events/214 jobs không thay vì balance.
- [ ] **D07-QA-07:** Hero và Tower anti-air có mục tiêu hợp lệ; chiến đấu 3D không làm mất accuracy khi quái ở trên cao.
- [ ] **D07-QA-08:** Enemy chết bởi Hero/Tower/Relic cùng tick chỉ grant WoodDrop/EXP đúng một lần.
- [ ] **D07-QA-09:** EXP theo type, hút/nhặt gem, qua nhiều LevelUp, chọn 3 card không phá balance/reset.
- [ ] **D07-QA-10:** Boss chết sớm vẫn chạy đến 600s; boss sống @600 vào OVERTIME; BaseHP0 ⇒ LOSS, bất kể damage tuning.
- [ ] **D07-QA-11:** kiểm tra bình quân Wood income/spend và tuổi thọ Tower/Base trong 3 mốc trận, không softlock do Wood thiếu.
- [ ] **D07-QA-12:** sau Restart ×3, Wood/EXP/HP/Boss/Upgrade/RunStats về giá trị profile; FastTest không bị lẫn production.

**Trạng thái toàn bộ D07-QA: NOT_RUN.** Không cộng các ca này vào QA chính thức cho tới khi D-07 được owner duyệt và cập nhật QA Gate.

## 4. Những câu hỏi kỹ thuật để khóa sau review

- Web `DefenseRules` có helper `repairTower`/`repairBase` không trừ Gỗ; việc caller có giới hạn không **chưa được audit hết**. Vì vậy, không khẳng định game Web có sửa miễn phí vô hạn; chỉ không nên sao chép helper một cách thiếu ràng buộc.
- Hero HP tương ứng 4 lớp nhân vật và Boss skill DPS toàn cảnh chưa được đưa vào bảng này. Cần đọc config đúng class và chạy Full Run sau khi Gate G0 hoàn tất.
- Đang có **D-06/A**: không bỏ 3 Linh Pháo, Air thật và anti-air chỉ để dễ balance.
- Các chỉ tiêu thắng chuẩn (tỷ lệ thắng, time-to-kill Boss, mục tiêu Gỗ mỗi phút, FPS thiết bị) hiện **chưa được người dùng chốt**, không tự ghi con số trong tài liệu.
- Scene local/glTFast/NavMesh/.meta phải sao lưu [runbook 07](07_G0_LOCAL_SAFETY_RUNBOOK.md) trước khi triển khai; D-07 là tài liệu không thực thi.

## 5. Phiếu duyệt D-07

```text
D-07-01 Source/reference baseline: **A — APPROVED 2026-10-23**
D-07-02 BaseHP & starting Wood: A / B / C / số khác
D-07-03 Tower economy/upgrade: A / B / C
D-07-04 Repair: A / B / C
D-07-05 WoodDrop & EXP: A / B / C
D-07-06 Enemy scaling: A / B / C
D-07-07 Boss HP/balance: A / B / C
D-07-08 Balance profile/QA: A / B / C
Các số cụ thể muốn khóa ngay:
Ngoại lệ / chỉnh sửa:
D-07: **APPROVE MỘT PHẦN** — chỉ D-07-01/A; D-07-02..08: PENDING
```

**Đề xuất cho phần còn chờ:** phương án A của D-07-02..08 (chưa duyệt). D-07-01/A đã duyệt về phương pháp, **chưa khóa mọi số production**. Bản Profile số cụ thể `balance-v0` cần duyệt ở một lần review sau với evidence hoặc số mặc định có nguồn và đã gắn nhãn "test only".

## 6. Source links (blob đã kiểm tra 2026-10-23)

- [Vongxuyen DefenseRules.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/defense/DefenseRules.js) — `a42ca8f6a4ea40b15d01729a637bd35f8ad880bb`
- [Vongxuyen EnemyFactory.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/entities/EnemyFactory.js) — `c4ac4a7111b672360740a1c4a14fa04aa5`
- [Vongxuyen stage01.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/config/stage01.js)
- [Vongxuyen experience.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/config/experience.js)
- [Vongxuyen YakshaBoss.js](https://github.com/thuongtran1210/Vongxuyen/blob/main/src/combat/YakshaBoss.js)
- [Projectzombie DefenseBasePrototype3D.cs](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/Assets/Features/Defense3D/DefenseBasePrototype3D.cs)
- [Projectzombie DefenseWoodWallet3D.cs](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/Assets/Features/Defense3D/DefenseWoodWallet3D.cs)
- [Projectzombie DefenseTowerBuildPrototype3D.cs](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/Assets/Features/Defense3D/DefenseTowerBuildPrototype3D.cs)
- [Projectzombie DefenseTowerPrototype3D.cs](https://github.com/thuongtran1210/Projectzombie/blob/feat/trebrave-defense3d-first-scene/Assets/Features/Defense3D/DefenseTowerPrototype3D.cs)

