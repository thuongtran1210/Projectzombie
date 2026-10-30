# VONG XUYÊN — Legendary Fantasy UI rollout

## Mục tiêu
Chuyển visual UI Defense3D sang cảm hứng Epic Fantasy RPG UI Kit (dark navy, warm antique gold, magical jade accents), nhưng giữ bản sắc Việt Nam qua họa tiết Đông Sơn, đồng cổ và pháp bảo dân gian. Không sao chép asset thương mại từ trang tham khảo.

## Đã triển khai — Phase 1 (code theme)
- `LegendaryFantasyTheme.cs`: token màu và helper styling cho TextMeshPro, Button, Slider.
- `DefensePreparationView`: áp màu tiêu đề, nội dung, nút Xuất Trận, nút phụ, loading.
- `DefenseMatchHUDView`: thông tin HUD, thanh HP/EXP, nút xây.
- `DefenseMatchResultModalView`: chữ kết quả, nút chơi lại/về menu.
- Chỉ thay đổi visual runtime; không sửa Presenter, save data, logic trận, scene hoặc prefab.

## Chú ý khi tích hợp
- Những Image / Button với sprite có màu sẵn có thể đổi màu bất ngờ bởi tint. Chụp trước/sau và điều chỉnh theo prefab cụ thể.
- Không dùng `LegendaryFantasyTheme.Primary` cho Button targetGraphic là một icon không phải background.
- Nếu một panel đang dùng texture gỗ cũ, phase 1 **chưa thay texture**. Thay hoàn chỉnh cần bộ sprite 9-slice mới và editor scene/prefab review.
- Áp màu HUD trong Awake; dữ liệu chiến đấu vẫn render bởi Presenter.
- Phải mở project bằng Unity 2022.3, kiểm tra compile/Play Mode và Android trước khi merge; GitHub không xác nhận runtime.
- Khi kiểm tra các vị trí dùng nhiều Rich Text, xác nhận không có tag màu cũ ghi đè palette.

## Phase 2 — Visual asset production
1. Tạo bộ 9-slice panel nền navy, viền đồng Đông Sơn; các trạng thái button.
2. Tạo icon pháp bảo / minion / tower theo catalog Defense3D, không thay ID gameplay.
3. Duyệt độ đọc trên 16:9, 19.5:9, 4:3 và Android màn hình nhỏ.
4. Áp texture lần lượt vào prefab qua Unity Editor, kiểm tra reference và SpriteAtlas.
5. Chỉ sau visual QA mới sửa style cho toàn bộ Meta UI / Codex / Gacha.

## Reference
https://ultra-knight.itch.io/epic-fantasy-rpg-ui-kit/devlog/1097205/elevate-your-fantasy-rpg-game-with-our-legendary-ui-kit-epic-fantasy-rpg-ui-kit

Art direction: **Dark navy panels + fine aged-gold borders + controlled jade magic glow + Vietnamese Dong Son ornament**.
