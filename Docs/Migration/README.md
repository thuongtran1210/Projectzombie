# Tiến độ và bàn giao migration Defense3D

Cập nhật: 28/10/2026. Root dự án: `C:/Users/thuon/Unity/Projectzombie`.

Tài liệu bắt đầu ở root: [MIGRATION_STATUS.md](../../MIGRATION_STATUS.md). Kế hoạch tổng: [DEFENSE3D_GAMEPLAY_MIGRATION_PLAN.md](../DEFENSE3D_GAMEPLAY_MIGRATION_PLAN.md).

## Phạm vi phiên kiểm kê đầu tiên

Giai đoạn 1 được thực hiện là **G0 kiểm kê**, theo thứ tự đã trao đổi; G1 trong tên kỹ thuật của kế hoạch là bước lõi loadout tiếp sau. Lượt này chỉ thêm công cụ/tài liệu/báo cáo, không đổi gameplay hoặc tài nguyên Unity.

| Hạng mục | Trạng thái | Bằng chứng / còn thiếu |
|---|---|---|
| Điểm đọc root và hướng dẫn nối tiếp | Đã tạo | MIGRATION_STATUS.md và tài liệu này |
| Phân loại hệ thống và thứ tự gỡ | Bản đầu đã tạo | SYSTEM_MANIFEST.md; cần audit consumer sâu trước xóa |
| Inventory file/GUID/reference text | Đã tạo công cụ và báo cáo | Generated/assets.csv, guid_edges.csv; không phải graph đầy đủ |
| Điểm nạp động và symbol legacy | Đã liệt kê tĩnh | dynamic_load_sites.csv, legacy_symbol_sites.csv; có comment/declaration và false positive |
| Addressables entry/group/address/label/folder | Đã liệt kê trực tiếp | addressables.csv; chưa mở rộng folder hoặc đo bundle |
| Shader/material và nội dung dùng chung | Có inventory/reference ban đầu | Chưa xác nhận stripping, asset binary, dependency native |
| Root runtime SampleScene/bootstrap/sảnh | Cần rà tiếp | SampleScene là root hỗn hợp, chưa đủ điều kiện xóa |
| Consumer Photon vào gameplay 2D | Có vị trí symbol và manifest | Chưa rà toàn bộ prefab registration/lifecycle SDK |
| Kết luận xóa từng file | Chưa thực hiện | Mọi dòng assets.csv có deletion_approved=False |
| Lõi loadout và Unit Test nghiệp vụ (G1) | Chưa bắt đầu | Không coi script inventory là implementation loadout |
| Chạy Unity/build/Test Runner | Không thực hiện | Theo yêu cầu người dùng |

## Báo cáo và cách tái tạo

Từ root dự án chạy `python Tools/Audits/defense_migration_inventory.py`. Công cụ chỉ đọc Assets/ProjectSettings và git status, ghi vào `Docs/Migration/Generated`; không gọi Unity, không di chuyển/xóa asset. Python dùng thư viện chuẩn.

| File | Ý nghĩa |
|---|---|
| summary.json | Số liệu và giới hạn của lần quét |
| assets.csv | Path/GUID/loại/kích thước nguồn/phân loại tạm/incoming/reference root |
| guid_edges.csv | Cạnh reference GUID đọc được từ text và meta |
| roots.csv | Root bảo thủ dùng để duyệt reference; không phải danh sách production cuối |
| addressables.csv | Entry trực tiếp; folder=True cần kiểm kê hiệu lực thư mục |
| dynamic_load_sites.csv | Dòng code có load asset, load scene, shader lookup hoặc runtime initializer |
| legacy_symbol_sites.csv | Vị trí nhắc symbol 2D; chưa phân giải namespace hoặc call graph |
| working_tree_at_scan.txt | Git status lúc quét, gồm thay đổi có trước và file kiểm kê mới; không đủ để khôi phục file |
| scene_components.csv | Owner/component/script/fileID trực tiếp trong scene; không expand prefab instance |

Kết quả đọc root và hotspot cụ thể: [SCENE_ROOT_AUDIT.md](SCENE_ROOT_AUDIT.md). Đã chạy 3 Unit Test Python cho parser inventory (hierarchy/CRLF, script chưa resolve, dữ liệu binary); cả 3 pass. Đây không phải Unity/C# gameplay tests. Báo cáo lần quét có 2.888 file, 278 entry Addressables và 696 component scene trực tiếp. 1.101 cạnh GUID chưa resolve không được coi tự động là reference hỏng.

Kích thước CSV là kích thước source, không phải RAM/APK/bundle. GUID không resolve có thể là builtin hoặc package, không tự coi missing. Root có SampleScene và toàn vùng authoring Defense3D để bảo toàn nội dung; reachable không đồng nghĩa cần giữ mãi, unreachable không đồng nghĩa xóa được. Không lưu chỉnh sửa thủ công vào Generated vì lần chạy sau ghi đè; ghi quyết định vào SYSTEM_MANIFEST.md hoặc log bên dưới.

## Bước tiếp theo cụ thể

1. Từ SampleScene xác định GameObject/component của bootstrap, sảnh, save, audio, Photon và gameplay cũ; ghi root owner nào giữ, tách hoặc ngừng khởi tạo.
2. Dùng legacy_symbol_sites lọc source ngoài thư mục legacy, rà dependency thật của meta/UI/audio/match flow/Photon. Tạo danh sách consumer cần sửa trước provider.
3. Đối chiếu folder entry và runtime key Addressables/Resources với catalog; xác minh group local/remote và settings. Không đổi đường dẫn hoặc key ở bước audit.
4. Chọn nhóm asset 2D đầu tiên để lập đề xuất batch: mỗi asset có lý do, root giữ lại và consumer động đã rà. Shader phải rà thêm settings, include/subgraph và generator.
5. Khi đủ dependency chính, cập nhật G0 thành hoàn tất về phân tích tĩnh với các kiểm chứng Unity còn chờ; bắt đầu G1 core loadout + Unit Test. Chưa tiến hành G6 xóa.

## Nhật ký

- 28/10/2026 — Bắt đầu G0: tạo entry point root, công cụ inventory, báo cáo và manifest hệ thống. Chỉ rà tĩnh; chưa chạy Unity hoặc thay gameplay. Bản kế hoạch trước đó chuyển từ “đề xuất” sang “đang kiểm kê”. Các thay đổi combat/VFX/pooling có trước được giữ nguyên.
- 28/10/2026 — Bổ sung bản đồ owner scene và root audit. Xác nhận Save/Meta/Audio đang cùng scene với bootstrap/spawn 2D; thấy hai MetaCurrencyManager serialize trực tiếp nên cần rà vòng đời trước khi di chuyển. 3 test parser Python pass. G0 vẫn đang rà dependency; không có asset được chấp thuận xóa.
- 29/10/2026 — Rà soát toàn bộ đồ thị tham chiếu GUID và cách ly an toàn tài nguyên 2D không còn sử dụng vào `Assets/_Legacy2D_Quarantine/`: 12 thư mục sprite 2D rác (`Art/AnSi`, `Mada`, `QuyNhapTrang`, `Magiap`, v.v.), 64 asset `Resources/Tiles/`, toàn bộ thư mục `_Data/Projectiles/`, `_Data/Text Style/` và `_Prefabs/Skills/`. Giải quyết thư mục trùng lặp `_Data/Character` vào `_Data/CharacterS/`. Số lượng asset hoạt động giảm từ 2.888 xuống 2.155 mà không làm gãy tham chiếu active.

Mỗi phiên tiếp theo thêm ngày, mục đã làm, file bằng chứng, phép kiểm tra đã chạy/chưa chạy và bước còn lại. Không ghi “pass” cho Unit Test chưa thực thi hoặc cho shader chưa build.
