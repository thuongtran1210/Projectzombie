# PROJECTZOMBIE — MAP 01 LEVEL SCENARIO GUIDE v6 (DRAFT)

> **Trạng thái:** PROPOSED / CHƯA PHÊ DUYỆT D06–D07.  
> **Thay thế trên nhánh draft:** kịch bản “Bến Đò Vong Xuyên”, 20 phút và Diêm Vương trong GDD v5.1.  
> **Bản gốc v5.1:** vẫn có thể đọc qua Git history; không thay đổi runtime/asset Stage_01 hiện tại.

## 1. Mục tiêu trận đấu

**Tên Map:** Map Sông Quê. **Mode đề xuất:** Defense / Hero Tower Defense.  
**Bản đồ:** Bờ Tây / Sông + 3 cầu / Bờ Đông, ba tuyến North–Mid–South.  
**Thời lượng reference:** 600 giây gameplay; tăng áp lực theo 5 phase, wave đến mỗi 18 giây.  
**Run thành công:** theo quy tắc được chốt tại D07. Đề xuất: Base còn HP và Hero còn sống ở mốc 600s, Boss chính đã chết. Đây chưa là sự thật trong Projectzombie.

**Thất bại đề xuất:** BaseHP = 0 hoặc HeroHP = 0. Tất cả nguồn thất bại phải qua RunResultResolver, chỉ xử lý một lần.  
**Các mục tiêu phụ:** bảo vệ tháp, giữ choke, tiêu diệt elite, duy trì economy, quản lý EXP/card, bảo toàn quân, đẩy lùi boss.

## 2. Nhịp 5 giai đoạn

| Phase | Đồng hồ reference | Áp lực/chức năng | Nghiệm thu |
| --- | --- | --- | --- |
| 1 — Dựng phòng tuyến | 00:00–02:00 | Giới thiệu gỗ, bệ xây, lane; Ma Giáp/Dơi/Thủy quái sớm; Ma Giáp Boss ở 01:48 | Có ít nhất 1 tháp có thể xây bằng gỗ ban đầu |
| 2 — Ba tuyến mở rộng | 02:00–04:00 | Tăng lane coverage; cơ hội tuyển quân | Không spam 1 lane duy nhất |
| 3 — Phòng tuyến bị thử | 04:00–06:00 | Quái đặc tính nhanh/giáp tăng, pressure towers | Có hành vi địch đánh Base/Ally/Tower |
| 4 — Phản công chiến thuật | 06:00–08:00 | Đẩy cao density, chọn squad+rally và nhịp boss | Các vùng path vẫn không nghẽn |
| 5 — Áp lực cuối | 08:00–10:00 | Dạ Xoa xuất hiện tại 08:30 (reference), vòng chống đỡ cuối | Boss cảnh báo rõ, run kết thúc nhất quán |

**Chú ý:** các câu mô tả áp lực là thiết kế màn chơi, không khẳng định script hiện đã cấu hình đúng ở Unity.

## 3. Công thức timeline source

Vongxuyen STAGE_01 tạo 34 event có eventIndex từ 0 đến 33:
- eventId: benque-01 ... benque-34
- atSeconds = eventIndex × 18; last event at = 594
- phase = min(5, floor(atSeconds / 120) + 1)
- normal enemy cycle = [magiap/eastBank, huyetdoi/northCanopy, thuyquai/canalWater, magiap/eastBank, magiap/southBank]
- eventIndex = 6, timestamp = 108: magiap_boss, count = 1
- normal count = min(8, 3 + phase + (eventIndex % 2))
- cap active=32, pending=40, spawnBudgetPerFrame=4, pendingLifetimeSeconds=25, minSpawnDistance=3.1.

Dạ Xoa được mô tả trong YakshaBoss như boss lúc 510s. **Nó không phải phần tử thứ 35 trong STAGE_01.events chỉ vì game có hai nguồn spawn.** Stage adapter phải định nghĩa BossSpawnEvent từ boss runtime riêng hoặc đưa vào timeline thống nhất có test.

## 4. Biến thể target / spawn

**Spawn Zone:** eastBank, northCanopy, canalWater, southBank; cần ánh xạ vào Unity 2D với transform, collider, spawn safety.  
**Lane:** North, Mid, South; quái bám waypoint graph, không phá luật water.  
**Threat:** địch có thể đổi target từ Hero sang Ally/Tower/Base, nhưng phải theo role policy, không tự aggro bất thường qua ba lane.
**Death:** enemy chết bởi tất cả nguồn damage hợp lệ đều thả/trao thưởng như nhau, đúng một lần.

## 5. Boss encounters

### 5.1 Ma Giáp Thống Lĩnh — 01:48
- Spawn event thứ 7 trong WaveScheduler source.
- Boss/elite giáp lớn, HP cơ sở 700; chiến đấu quanh trục lane phù hợp.
- Cảnh báo spawn, HP bar, kill/event cleanup, giới hạn spawn đủ cap.
- Thưởng wood/EXP riêng theo Balance SO (chưa chốt mức).

### 5.2 Tướng Dạ Xoa — 08:30
- Sinh tại arena bờ Đông; runtime VX có state riêng.
- Kỹ năng chém quét, bổ mạnh, đao khí có telegraph/targeting.
- Chưa chốt D07: boss có bắt buộc phải chết, có được gây damage Nhà Chính, kill sớm có end ngay.
- Nếu Boss spawned sau khi Base đã bị phá/GameOver thì không kích hoạt.

## 6. Điều kiện kết trận: state table

| Tình huống | Hành vi bản đề xuất | Phụ thuộc |
| --- | --- | --- |
| Hero HP về 0 trước 600s | Lose | D07 |
| Base HP về 0 trước 600s | Lose | D07 |
| 600s, Hero+Base còn sống, Boss đã chết | Win | D07 |
| 600s, Hero+Base còn sống, Boss chưa chết | Chờ quyết định: timeout grace hay lose | D07 |
| Boss chết trước 600s | Tiếp tục defense tới 600s | D07 |
| Pause / LevelUp Selection | Tạm dừng game clock, spawn, tower, ally và boss | D06 |
| Menu restart/run start | Reset timeline, live counts, spawn queue, pooled state, economy, towers | P0 |
| Cùng lúc Hero chết + Base chết | Chỉ một GameOver result, có reason priority | P0 |

## 7. Dữ liệu Unity & integration

**Không tái sử dụng Level1_Timeline.asset 900s như timeline Map 01** trừ khi chủ dự án chốt D06 và tạo bản được migrate có version. Stage_01_BambooForest.asset hiện 600s nhưng Level1_Timeline maxLevelDuration 900, nên cần validator khi link asset. Phương án:
- StageDefinitionSO giữ metadata map/stage.
- DefenseScenarioSO chứa mode=Defense, mapId=MapSongQue, durationSeconds, victory policy.
- WaveTimelineSO chứa event list và boss injections.
- LaneSpawnLocator chọn điểm spawn hợp lệ trong zone.
- WaveScheduler tick theo shared GameStateManager/gameClock.
- DefenseRunController đưa result về MatchFlowOrchestrator và RunStatsTracker.
- Scenario schema versioned để không phá save/map cũ.

## 8. QA checklist

- [ ] Start timer 600s duy nhất, không nhảy số khi pause, load.
- [ ] Ma Giáp Boss đúng mốc 108s, không lặp khi resume.
- [ ] 34 event được lập lịch đúng, cap/pending đủ và hết hạn hợp lý.
- [ ] 3 lane có cảnh báo dự đoán và spawn hợp lệ.
- [ ] Dạ Xoa 510s theo quyết định, không spawn trong GameOver.
- [ ] BaseHP và HeroHP thay đổi qua damage resolver, result chỉ một lần.
- [ ] Boss chết sớm, sống sau timeout và song tử trận có policy.
- [ ] Rerun reset tất cả stage subscriptions và pool.
- [ ] Không làm hỏng Survival legacy stage 900s.
- [ ] Tests PlayMode Android cho portrait/landscape và modal.

## 9. Liên kết
- [Master GDD v6](ProjectZombie_GDD.md)
- [Map / Defense](MAP_AND_DEFENSE_SYSTEM_SPEC.md)
- [Roster](ENEMY_ROSTER_VONGXUYEN.md)
- [Decision Checklist](DECISION_CHECKLIST_GDD_V6.md)
