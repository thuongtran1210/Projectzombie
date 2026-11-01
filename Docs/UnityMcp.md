# Unity MCP — Projectzombie

MCP stdio server dành cho Unity 2022.3, nằm tại `Tools/UnityMcp/server.py`.
Editor bridge riêng ở `Assets/Editor/UnityMcp`, không thay đổi bridge legacy.
Assembly chỉ dành cho Editor, không đi vào Android build. Không cần OpenAI API key.

## Cài đặt

Tại thư mục gốc dự án, dùng Python 3.11 trở lên và pip:

```powershell
./Tools/UnityMcp/Setup.ps1
```

Môi trường Python nằm trong `Library/UnityMcpPython` (Git bỏ qua). Nếu xóa Library,
chạy lại setup. SDK MCP được ghim phiên bản đã kiểm chứng trong requirements.txt.
Mở đúng dự án trong Unity và cho phép import/compile. Nếu Auto Refresh tắt:

```powershell
./Tools/Invoke-UnityBridge.ps1 refresh
```

Đăng ký với Codex bằng executable Python riêng của dự án:

```powershell
codex mcp add projectzombie-unity -- "C:\Users\thuon\Unity\Projectzombie\Library\UnityMcpPython\Scripts\python.exe" "C:\Users\thuon\Unity\Projectzombie\Tools\UnityMcp\server.py" --project "C:\Users\thuon\Unity\Projectzombie"
```

Thay đường dẫn khi clone sang máy khác. `Setup.ps1` cũng tạo cấu hình JSON cho
client MCP thông thường tại `Library/UnityMcp.client.json`. Không đưa JSON này vào
config TOML của Codex. Tham khảo [hướng dẫn MCP chính thức](https://learn.chatgpt.com/docs/extend/mcp).
Tool đã đăng ký có thể cần mở chat/phiên Codex mới để xuất hiện trong danh sách tool;
đăng ký config không thay đổi tool inventory của phiên đang chạy.

## 10 tools

| Tool | Chức năng |
|---|---|
| `editor_status` | Kết nối, Play Mode, scene, dirty state và diagnostics compiler |
| `console_read` | Tối đa 500 log từ lần domain load hiện tại, kèm stack trace, lọc severity |
| `hierarchy_query` | Object/component ID, tìm theo tên, phân trang, gồm object inactive trong scene đã load |
| `component_read` | Đọc serialized fields hoặc một property path cụ thể |
| `serialized_property_set` | Sửa một property với expected_value, Undo và prefab-instance override |
| `asset_find` | Tìm trong Assets bằng cú pháp AssetDatabase, có GUID và object ID |
| `capture_game_view` | Chụp frame Game View có overlay UI, chạy bất đồng bộ |
| `playmode_set` | Bật/tắt Play Mode và theo dõi hoàn tất qua reload |
| `tests_run` | EditMode/PlayMode tests, lọc assembly/test names, báo cáo NUnit XML |
| `job_status` | Đọc kết quả theo ID; include_image=true trả nội dung ảnh MCP |

## Luồng sử dụng

1. Gọi `editor_status`, kiểm tra state và kết nối.
2. Tìm component qua `hierarchy_query`; lấy `id` trong danh sách components.
3. `component_read(object_id, property_path)` lấy kiểu và giá trị hiện tại.
4. `serialized_property_set` gửi cùng `expected_value` vừa đọc. Nếu Inspector đã
   thay đổi giá trị, lệnh bị từ chối trước khi áp dụng.
5. Gọi `job_status` bằng ID trả về nếu state còn queued/running.

Ví dụ thay vị trí Transform:

```json
{
  "object_id": "<component ID lấy từ hierarchy_query>",
  "property_path": "m_LocalPosition",
  "expected_value": [0, 0, 0],
  "value": [0, 1.5, 0]
}
```

Các kiểu hỗ trợ: integer, float, bool, string, enum theo index, object reference
(ID hoặc null), Vector2/3/4 và Color RGBA dưới dạng mảng. Đọc property con bằng
property_path, ví dụ `someArray.Array.data[0]`; không hỗ trợ resize array,
managed reference hay sửa script/prefab metadata. Generic/unsupported fields có
value=null kèm type để phân biệt với reference null.

Sửa property chỉ ở Edit Mode ổn định. Scene instance được đánh dấu dirty; prefab
instance ghi override. Asset ghi trực tiếp chỉ hỗ trợ ScriptableObject trong
Assets; không sửa trực tiếp prefab/material/importer trong bản đầu. Thay đổi
không tự save, có Undo trong Editor. Unity có thể lưu dirty asset trong lần save
sau đó; `saved=false` nghĩa là tool không chủ động ghi asset/scene xuống đĩa.

## Tác vụ và reload

File nội bộ trong `Library/UnityMcpBridge`:

- `requests/<id>.json`: mỗi yêu cầu một file, publish bằng atomic rename.
- `jobs/<id>.json`: trạng thái và kết quả riêng cho từng yêu cầu.
- `heartbeat.json`: cập nhật mỗi giây; quá 10 giây coi là mất kết nối.
- `artifacts/<id>.png` hoặc `.xml`: ảnh và báo cáo.

Bridge xử lý yêu cầu tuần tự trên main thread. Các tác vụ bất đồng bộ chặn thao
tác thay đổi tiếp theo, nhưng vẫn cho đọc trạng thái. Client chờ tối đa khoảng
2 giây rồi trả mã tác vụ. Timeout phía client không hủy và không gửi lại tác vụ.
Request chờ quá 120 giây bị expired trước khi thực thi. Play Mode chờ 90 giây,
capture 20 giây, tests 15 phút; quá hạn được đánh dấu interrupted, không tự dừng
test hoặc hoàn tác tác động. Dừng test qua Unity Test Runner khi cần.

Khi domain reload, callback test được đăng ký lại và tác vụ Play Mode tiếp tục
theo trạng thái thực tế. Editor restart làm tác vụ đang chạy chuyển interrupted.
Không tự chạy lại thao tác có thể đã thực hiện. Kết quả cũ vẫn đọc được khi Editor
offline. Các tác vụ đã hoàn tất được giữ lại đến khi xóa Library; không có tác vụ
retention tự động trong bản này.

GlobalObjectId dùng cho object đã lưu. Object chưa lưu và runtime dùng instance
ID theo session; query lại sau chuyển Play Mode hoặc khi ID không còn resolve.
Hierarchy bản đầu gồm scene đã load, chưa gồm Prefab Stage và DontDestroyOnLoad.

## Chụp Game View và chạy tests

`capture_game_view` yêu cầu Play Mode đang chạy, không pause và Game View được
render/hiển thị. Dùng ScreenCapture của Unity để bao gồm overlay UI. Bản đầu
không tự mở Game View hoặc chọn độ phân giải; không coi camera render là Game
View capture. Khi tác vụ succeeded, `job_status(id, include_image=true)` trả PNG
qua MCP. Giới hạn ảnh trả qua MCP là 20 MiB.

Vào Play Mode và chạy tests yêu cầu scene sạch, đã lưu. Bridge không tự save
hoặc bỏ scene của người dùng. Test job succeeded nghĩa là runner đã hoàn tất:
phải kiểm tra thêm `result.failed` và `result.resultState` để biết test có pass.
Nếu không có test khớp filter, kiểm tra tổng passed/failed/skipped trước khi kết
luận. Tests có thể tự thay đổi trạng thái dự án tùy nội dung test được chọn.

Compiler diagnostics được thu khi bridge đang được load và tồn tại qua reload;
không phải bản sao toàn bộ Console lịch sử. Nếu lỗi compile ngăn bridge load lần
đầu, kiểm tra Console/Editor.log và sửa lỗi trước.

## Kiểm chứng

```powershell
Library/UnityMcpPython/Scripts/python.exe -m unittest discover -s Tools/UnityMcp -p 'test_*.py' -v
Library/UnityMcpPython/Scripts/python.exe Tools/UnityMcp/check_mcp.py --live
Library/UnityMcpPython/Scripts/python.exe Tools/UnityMcp/run_editor_tests.py
Library/UnityMcpPython/Scripts/python.exe Tools/UnityMcp/check_playmode.py
```

Lệnh cuối chỉ chạy assembly `ProjectZombie.UnityMcp.Editor.Tests`, kiểm tra ID,
sửa vector/Undo, xung đột, input sai và metadata bị bảo vệ. NUnit XML của từng
test run được trả trong result.reportPath. Lệnh check_playmode kiểm chứng chuyển
Play Mode, chụp PNG và khôi phục Edit Mode; chỉ chạy khi scene đã lưu và Game View
đang hiển thị. Các kiểm tra này không xác nhận gameplay hoặc Android build.

Đã kiểm chứng trên Unity 2022.3.62f3 ngày 01/11/2026: 6 Python tests, 5 Editor tests,
MCP stdio handshake với đủ 10 schema, đọc trạng thái/Console/hierarchy/asset,
vào và thoát Play Mode qua domain reload, chụp Game View PNG 2532×1170.

MCP server không mở cổng mạng và không cung cấp thực thi C# tùy ý. Quyền thao tác
được giới hạn bởi tool schema, kiểm tra Editor và phạm vi project đã cấu hình.
