# 📦 QUY CHUẨN NỘI DUNG BẢN BUILD APK ĐẦU TIÊN (FIRST PLAYABLE V1.0) & ADDRESSABLES SPECIFICATION

Tài liệu này định nghĩa chính thức danh mục tài nguyên đi kèm trong bản cài đặt APK đầu tiên (Day-1 First Playable) và cấu hình Addressables tương ứng để phân tách rạch ròi giữa nội dung cục bộ (Local Packed) và nội dung tải sau (Remote DLC).

---

## 1. Triết Lý Trải Nghiệm Ngày Đầu (Day-1 Experience)
1. **Zero Download Friction:** Người chơi mở game là chơi được ngay lập tức, không có màn hình chờ tải tài nguyên bổ sung ở lần khởi động đầu tiên.
2. **Trọn Vẹn Ải 1 (Stage 1 Complete Loop):** Màn chơi Màn 1 (Rừng Trúc), nhân vật mặc định, dàn vũ khí Tier 1, dàn quái vật Màn 1 và Boss Màn 1 (Ngưu Đầu Mã Diện) phải có sẵn 100% bên trong APK.
3. **Dung Lượng APK Tối Ưu:** Giới hạn file APK gốc trong khoảng **80MB - 120MB**.

---

## 2. Ma Trận Phân Bổ Addressable Groups (Local vs Remote)

### 🟢 Nhóm Cục Bộ Trong APK (Local_Packed)
*Cấu hình Schema:*
- **Build Path:** `[UnityEngine.AddressableAssets.Addressables.BuildPath]/[BuildTarget]`
- **Load Path:** `{UnityEngine.AddressableAssets.Addressables.RuntimePath}/[BuildTarget]`

| Tên Group Addressable | Nội Dung Bao Gồm |
| :--- | :--- |
| **`Group_Core_Preload`** | - Database cốt lõi: `CharacterDatabase`, `WorldStageDatabase`, `CharacterStarProgressionConfig`<br>- Stage 1 Timeline: `Stage_01_BambooForest`<br>- Bản đồ Stage 1: `Map_BambooForest.prefab` |
| **`Group_Enemies_Stage1`** | - Toàn bộ quái vật thường xuất hiện ở Màn 1 |
| **`Group_Enemies_Bosses`** | - Boss Màn 1: `Boss_NguuDauMaDien.prefab` |
| **`Group_Weapons_Tier1`** | - Toàn bộ WeaponData và Prefab Tier 1 (Kiếm gỗ, Nỏ, Dép, Nồi, Tẩu...) |
| **`Group_VFX_Skills`** | - Hiệu ứng kỹ năng, chém, nổ, vệt đạn cơ bản của Màn 1 |
| **`Group_Audio_BGM`** | - Nhạc nền MainHub và Màn 1 (Bamboo Forest BGM), toàn bộ SFX cốt lõi |

---

### 🌐 Nhóm Tải Bổ Sung (Remote_DLC)
*Cấu hình Schema:*
- **Build Path:** `ServerData/[BuildTarget]`
- **Load Path:** `https://<firebase-storage-cdn>/[BuildTarget]`

| Tên Group Addressable | Nội Dung Bao Gồm |
| :--- | :--- |
| **`Group_DLC_Stages_Remote`** | - `Stage_02_AncientCitadel`, `Stage_03_CinnabarSwamp`<br>- `Map_AncientCitadel.prefab`, `Map_CinnabarSwamp.prefab` |
| **`Group_DLC_Enemies_Remote`** | - Quái vật đặc thù Màn 2, Màn 3<br>- Boss Màn 2, Boss Diêm Vương (`Boss_DiemVuong.prefab`) |
| **`Group_DLC_Upgrades_Remote`** | - Toàn bộ thẻ nâng cấp cao cấp, thẻ hợp thể Fusion Relics |
| **`Group_DLC_Audio_Remote`** | - Nhạc nền chất lượng cao cho Màn 2, Màn 3 và các sự kiện đặc biệt |
| **`Group_DLC_MetaConfigs_Remote`** | - Cấu hình Banner Gacha giới hạn, Event đặc biệt |

---

## 3. Quy Tắc "Single Source of Truth" (Xóa Bỏ Tranh Chấp Resources)

1. **Tuyệt đối không nhân bản:** Toàn bộ các asset đã nằm trong các Addressable Groups ở trên (cả Local và Remote) **KHÔNG ĐƯỢC PHÉP** copy vào thư mục `Assets/Resources/`.
2. **Phạm vi của `Assets/Resources/`:** Chỉ giữ lại các file cấu hình bắt buộc của Engine/Plugin:
   - `DOTweenSettings.asset`
   - Cấu hình khởi động tối thiểu (nếu có).
3. **Cơ Chế Nạp Tài Nguyên Runtime:**
   - Nạp trực tiếp qua `Addressables.LoadAssetAsync<T>()` / `Addressables.InstantiateAsync()`.
   - Loại bỏ hoàn toàn sự phụ thuộc vào `Resources.Load<T>()` và `#if UNITY_EDITOR AssetDatabase`.

---

## 4. Quy Chuẩn Nạp Tài Nguyên Trong Multiplayer Co-op (Host & Client)

1. **Đồng Bộ Stage ID Qua Mạng:**
   - Host chọn hoặc mặc định Ải (`Stage_01_BambooForest`). Khi Host bấm bắt đầu trận, gói tin `MSG_MATCH_START` truyền `stageId` sang Client P2.
2. **Nạp Độc Lập Qua Addressables Trên Từng Máy:**
   - Cả Host và Client P2 đều sử dụng `WorldStageDatabase` (Local Addressables) để tra cứu `stage.mapPrefabAddress` theo `stageId`.
   - Mỗi máy tự thực hiện `Addressables.InstantiateAsync(stage.mapPrefabAddress)` để tạo bản đồ Tilemap cục bộ trên máy của mình.
3. **Đảm Bảo Ải 1 Có Sẵn Trong APK:**
   - Do `Map_BambooForest` nằm trong `Group_Core_Preload`, máy khách P2 (dù là thiết bị Android mới cài đặt và chưa tải DLC) đều nạp bản đồ thành công 100% ngay khi Host vừa bắt đầu trận.
