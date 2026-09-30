# Kiểm tra Ngũ Hành sau sửa

## Luật đã áp dụng

- Đòn thường lấy `PlayerStats.CurrentElement` của chủ sở hữu. Bốn prefab tướng có hệ gốc; `SetElementOverride` đổi hệ trong runtime, truyền `None` để trở về hệ gốc.
- Một lần đánh thường chỉ ghi nhận một hit Tương Sinh thành công, kể cả chém nhiều mục tiêu, bắn nhiều viên hoặc đạn xuyên. Đạn giữ hệ tại thời điểm được bắn.
- `HealthSystem` xử lý Tương Khắc cho dữ liệu sát thương có hệ; dữ liệu đã tính bonus được đánh dấu để không nhân lần hai. Vụ nổ giữ hệ, chí mạng, nguồn và chủ sở hữu từ đạn.
- Tương Sinh giảm 20% hồi chiêu **còn lại của kỹ năng pháp bảo**, phát cập nhật HUD ngay. Không thay đổi nhịp đánh hay cửa sổ tái kích hoạt.
- Thư Sinh tự chọn hệ sinh ra hệ pháp bảo có hồi kỹ năng còn lại dài nhất: Kim ← Thổ, Mộc ← Thủy, Thủy ← Kim, Hỏa ← Mộc, Thổ ← Hỏa. Lựa chọn thủ công đi qua cùng một lần thi triển, chỉ tạo một hit ảo.
- Bộ nhớ hit, cửa sổ nối 3 giây và hồi chờ proc 3 giây riêng theo người chơi. Giữ luật cũ: hit ảo bỏ qua hồi chờ proc thường và chỉ được dùng một lần.

## Cần xác nhận trực tiếp trong trận

1. Tắt chí mạng và các hiệu ứng cộng sát thương. Đòn Kim 100 vào Mộc mất 130 HP; vào mục tiêu không bị Kim khắc mất 100 HP. Thử cả đòn chém và đạn.
2. Đổi hệ qua `SetElementOverride(Hoa)`: đòn đánh mới, số sát thương và màu chỉ hướng dùng Hỏa; trả `None` để khôi phục hệ gốc.
3. Một đòn chém trúng nhiều quái hoặc nhiều viên cùng lượt bắn chỉ thêm một hit. Đòn hụt/bị bất tử chặn không thêm hit.
4. Đòn Kim của tướng → pháp bảo Thủy trong 3 giây kích hoạt Tương Sinh. Hồi kỹ năng còn 5 giây xuống 4 giây; nút HUD cập nhật ngay.
5. Đạn Hỏa nổ vào cả Kim và Mộc: tính từng mục tiêu độc lập. Với sát thương trước Tương Khắc là 200 và có cờ chí mạng, Kim nhận 260, Mộc nhận 200; cờ chí mạng vẫn còn.
6. Thư Sinh tự chọn Kim khi pháp bảo là Thủy. Chọn thủ công Hỏa phải giữ Hỏa; không bị một hit tự chọn khác ghi đè. Không thêm hit khi kỹ năng chưa sẵn sàng.
7. Hai người chơi A/B: Kim của A không nối với Thủy của B. A đang hồi chờ proc không ngăn B nối chuỗi riêng. Thử đạn đang bay, vùng sát thương và đổi pháp bảo.
8. Bắn lại sau khi đạn được trả về pool: không giữ chủ sở hữu hoặc trạng thái đã ghi nhận của lần bắn trước.

## Phạm vi kiểm chứng

Ngày 30/09/2026: build `Assembly-CSharp-Editor.csproj` thành công, 0 lỗi (8 cảnh báo hiện có). Bộ 23 kiểm thử EditMode chạy trong Unity 2022.3.62f3 ở dự án tạm: 23 đạt, 0 lỗi, 0 bỏ qua. Mã runtime dùng cùng nguồn của dự án, biên dịch với tên assembly riêng để Unity nạp được các component trong môi trường tạm. Kết quả XML: `Temp/ElementRegression-results-final.xml`.

Các ca tự động nằm trong `Assets/Tests/Editor/DamageProcessorTests.cs`. Chúng kiểm tra sát thương, bộ nhớ Tương Sinh, hồi chiêu và sự kiện cập nhật HUD; không thay thế kiểm tra hình ảnh HUD, prefab đạn thực tế hay phiên Photon hai máy.

Co-op tiếp tục dùng cơ chế dự đoán cục bộ có sẵn: client ghi nhận hit khi đưa sát thương vào lô gửi host. Bộ nhớ đã tách theo chủ sở hữu; chưa bổ sung giao thức xác nhận/hoàn tác proc theo từng hit từ host.
