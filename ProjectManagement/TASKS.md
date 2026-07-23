# Danh Sách Nhiệm Vụ & Task Tracker — ProjectZombie

Tài liệu quản lý danh sách công việc (Kanban Board Task Tracker) phân chia theo các hạng mục của dự án **ProjectZombie (Android Release)**.

---

## 📋 Kanban Board Summary

| 🔴 To Do | 🟡 In Progress | 🟢 Done |
|---|---|---|
| Boss Abomination Skill Logic | 7 Weapons (Vampiric Bats, Shotgun...) | Android Target Platform Refactor |
| Boss Skeleton King Skill Logic | Spawn Timeline Wave Data Scriptable | Local Save System (`SaveSystem.cs`) |
| Meta Upgrade Tree Shop UI | Evolution Upgrade Data Scriptables | Spawn System & Pillar Animation Fix |
| Mobile Virtual Joystick Integration | | Object Pooling 0 GC Optimization |
| Android AAB Build & Signing | | GDD 3.0 & Architecture Docs Update |

---

## 🏃 Sprint Tasks Breakdown

### 🎯 Hạng Mục 1: Gameplay & Weapon Content (Ưu tiên cao)
- [x] **[TASK-101]** Khôi phục và nâng cấp `SpawnManager` & `SpawnPillar` với animation nảy trụ DOTween.
- [ ] **[TASK-102]** Tạo 3 ScriptableObject Vũ Khí Tầm Xa mới: Vampiric Bats (`W004`), Shotgun (`W005`), Grenade Launcher (`W006`).
- [ ] **[TASK-103]** Tạo 3 ScriptableObject Vũ Khí Cận Chiến / AoE mới: Crossbow (`W007`), Flamethrower (`W008`), Lightning Orb (`W009`).
- [ ] **[TASK-104]** Tạo 3 ScriptableObject Vũ Khí Hỗ Trợ mới: Poison Drone (`W010`), Holy Water (`W011`), Boomerang (`W012`).
- [ ] **[TASK-105]** Tạo bộ 12 thẻ ScriptableObject `EvolutionUpgrade` tương ứng tại cấp 6.

### 🧟 Hạng Mục 2: Enemy AI & Boss Battles (Ưu tiên cao)
- [x] **[TASK-201]** Triển khai FSM Enemy State Machine + Strategy Pattern cho 5 quái thường (Walker, Runner, Tank, Spitter, Exploder).
- [ ] **[TASK-202]** Cấu hình `SpawnTimelineData` map từng wave theo timeline 00:00 -> 20:00.
- [ ] **[TASK-203]** Tạo `AbominationBossController.cs` (Phase 1 Bull Dash/Ground Slam, Phase 2 Swarm/Toxic Cloud).
- [ ] **[TASK-204]** Tạo `SkeletonKingBossController.cs` (Phase 1 Sword Wave/Bone Cage, Phase 2 Death Zone).

### 💳 Hạng Mục 3: Meta Economy & UI Systems (Ưu tiên trung bình)
- [ ] **[TASK-301]** Xây dựng giao diện `MetaUpgradeShopView.cs` và `MetaUpgradeShopPresenter.cs` (Chuẩn MVP UI) cho Cây nâng cấp vĩnh viễn.
- [ ] **[TASK-302]** Gắn kết nối `MetaCurrencyManager` với nút Mua / Upgrade trên UI Shop.
- [ ] **[TASK-303]** Tích hợp Dynamic Virtual Joystick từ Unity New Input System vào `PlayerController.cs`.
- [ ] **[TASK-304]** Xây dựng màn hình `RunSummaryView.cs` hiển thị kết quả sau run đấu và cộng Coin.

### ⚙️ Hạng Mục 4: Performance & Build Release (Ưu tiên cuối)
- [ ] **[TASK-401]** Chuyển đổi toàn bộ Sprite Sheets sang Texture Compression **ASTC 6x6** cho Android.
- [ ] **[TASK-402]** Thử nghiệm Stress Test 200 Enemies + 100 Projectiles kiểm tra FPS (Target 60 FPS).
- [ ] **[TASK-403]** Cấu hình Build Profile Android IL2CPP ARM64, Target API 33+, xuất file `.aab`.
