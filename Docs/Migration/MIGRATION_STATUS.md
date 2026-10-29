# Trạng Thái Di Cư Lên Defense 3D (Defense 3D Migration Status)

Cập nhật lần cuối: 28/10/2026.  
Dự án: **Projectzombie** — Chuyển đổi mô hình gameplay cốt lõi từ 2D Top-down Rogue-lite sang **Top-down 3D Defense Rogue-lite**.

---

## 1. Bảng Tổng Hợp Trạng Thái Các Giai Đoạn

| Giai đoạn | Mô tả | Trạng thái | Ghi chú & Kết quả |
|---|---|:---:|---|
| **G0** | Khảo sát & Đóng băng ranh giới hệ thống cũ | **HOÀN THÀNH** | Lập hồ sơ 15 đối tượng 2D trong SampleScene, kiểm kê Shaders và các điểm liên kết |
| **G1** | Chuẩn hóa Định danh & Catalog Defense 3D | **HOÀN THÀNH** | Tạo `Defense3DCatalog`, `DefenseLoadoutIdMapper`, DTOs và 100% test độc lập |
| **G2** | Kho lưu trữ Loadout & Tích hợp Dữ liệu Meta | **HOÀN THÀNH** | `DefenseLoadoutRepository`, `DefenseMetaOwnershipAdapter`, lưu trữ local Json an toàn |
| **G3** | Điều phối Trận đấu & Cơ chế Chuyển Scene | **HOÀN THÀNH** | `DefenseSessionContext`, `DefenseMatchNavigation`, điều hướng an toàn sang `MapSongQue3D_Prototype` |
| **G4** | Màn hình Chuẩn bị Xuất Trận & MVP UI | **HOÀN THÀNH** | `DefensePreparationPresenter`, `DefensePreparationModalView`, wire-up mượt mà với MainHub |
| **G5** | Tổng kết Trận đấu & Lưu trữ Meta Thưởng | **HOÀN THÀNH** | `DefenseMatchResultService`, trao thưởng Tiền/Gỗ/Stage Progress vào `SaveData` |
| **G6** | Ngắt và Loại bỏ hoàn toàn Gameplay 2D Cũ | **HOÀN THÀNH** | Dọn sạch Player 2D, Weapons 2D, Spawners 2D, Enemies 2D, Upgrades 2D, Collectibles 2D; bảo toàn 100% Meta Data |

---

## 2. Chi Tiết Kết Quả Thực Thi G6 (Legacy 2D Cleanup)

### 2.1. Các Thành Phần 2D Đã Được Loại Bỏ An Toàn
1. **MatchFlow & Khởi Động Trận Đấu 2D:**
   - Đã gỡ bỏ: `MatchFlowService.cs`, `MatchFlowOrchestrator.cs`, `GameplayBootstrapper.cs`.
   - Đã tỉa gọn `MetaSceneTransitionController.cs` để SampleScene trở thành Dedicated Hub chuyên trách.
2. **Nhân Vật & Điều Khiển 2D:**
   - Đã gỡ bỏ: `PlayerController.cs`, `PlayerMotor2D.cs`, `PlayerMovement.cs`, `PlayerMythicManager.cs`, `PlayerExperience.cs`, `PlayerLevelManager.cs`, `PlayerHealthBar.cs`, `PlayerProvider.cs`.
   - Đã tạo `PlayerStatModifier.cs` độc lập trong `Features/Player/Stats/` để phục vụ cây kỹ năng vĩnh viễn (MetaProgression).
3. **Nâng Cấp & Trọng Số 2D (Upgrades):**
   - Đã gỡ bỏ toàn bộ thư mục `Assets/Features/Upgrades` (56 files gồm Runtimes, Augments, Weighting, SynergyTrait).
4. **Vũ Khí & Đạn 2D (Weapons & Projectiles):**
   - Đã gỡ bỏ: 62 scripts vũ khí (Boomerang, Flamethrower, HolyWater, Crossbow, Shotgun, MeleeBase, RangedBase, Pets, Projectiles, BatQuaiTranZone).
   - Tinh giản `WeaponBase.cs` và `WeaponData.cs` thành Data Model catalog an toàn không còn phụ thuộc runtime 2D.
5. **Kẻ Địch & Spawners & Thu Thập 2D (Enemies, Spawners, Collectibles):**
   - Đã gỡ bỏ toàn bộ `Assets/Features/Spawners/`, `Assets/Features/Enemies/`, `Assets/Features/Collectibles/`.
   - Đã dọn dẹp các phản ứng nguyên tố 2D phụ thuộc quái 2D trong `Assets/Features/Elements/`.

### 2.2. Kiểm Chứng Chất Lượng & Tính Toàn Vẹn
- **Biên dịch mã nguồn:** `dotnet msbuild Assembly-CSharp.csproj` hoàn thành với **0 errors**.
- **Bộ kiểm tra G1-G4 Core Checks:** `compile_and_run.ps1` đạt **59/59 checks PASSED**.
- **Bộ kiểm tra NUnit Editor Tests:** `compile_tests.ps1` biên dịch và chạy sạch 100%.
- **Bảo toàn Dữ liệu Meta:** `CharacterDataSO`, `CharacterDatabaseSO`, `HeroStatsConfig`, `WeaponData`, `MetaProgressionSaveData`, `SaveSystem` được giữ nguyên vẹn, đảm bảo tương thích ngược dữ liệu người chơi.
