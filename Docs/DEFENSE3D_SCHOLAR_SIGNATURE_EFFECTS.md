# Defense3D — Signature Thư Sinh

## Hành vi gameplay

Skill vẫn là AoE quanh bản thân sau windup. Không thêm projectile và không đổi asset Skill_ScholarSignature:

- Radius/range: 6.
- Damage gốc: 52.5; giữ công thức scale theo EffectiveDamage/BaseDamage của đánh thường.
- Cooldown: 30 giây; windup: 0.38 giây.
- Sau khi resolve hợp lệ, giảm 20% **phần cooldown còn lại** của pháp bảo đúng session, một lần. Vẫn giảm khi AoE không có mục tiêu hoặc tất cả target từ chối damage, như trước.
- Hủy trước resolve không gây damage và không giảm hồi pháp bảo. Cooldown signature đã commit không được hoàn lại.

DefenseScholarSignatureEffect3D chỉ giữ gameplay và sự kiện. Không còn _impactView, timer ẩn view, ShowAtFeet hoặc Physics.Raycast để đặt hình ảnh.

## Luồng sự kiện

```text
Cast hợp lệ → Started → vòng niệm sustained bám chân
Windup đủ → Released → dừng vòng niệm, phát xung lực
TakeDamage trả về true → HitConfirmed → spark tại điểm đã chụp trước damage
Resolve xong → Ended(completed) → burst/spark tự hết hạn
Cancel/Downed/dash/pause/reset → Ended(cancelled) hoặc Clear → thu hồi handle riêng
```

Mỗi cast có ID tăng dần, session identity và run generation. IsCasting vẫn phản ánh windup cho hoạt ảnh Thư Sinh hiện có. Vị trí AoE được lấy từ actor lúc tung chiêu, giữ khả năng di chuyển trong lúc niệm. Xung lực nở ra chỉ là hình ảnh; damage vẫn được xử lý tại thời điểm release, không chạy theo bán kính animation.

Khi target chết, despawn hoặc đổi vị trí ngay trong TakeDamage, spark vẫn dùng vị trí đã chụp trước callback. Khi restart hoặc gián đoạn trong một callback damage, các target còn lại của lượt cũ không bị đánh và lượt mới không bị giảm hồi nhầm. Nếu đường damage ném exception, cast được đóng để không kẹt trạng thái; lỗi vẫn được chuyển lên caller.

## Cấu trúc code

- DefenseSignatureFeedbackEvents: các sự kiện có payload bất biến; dùng DefensePresentationSignal để cache observer và cô lập lỗi VFX/logger.
- DefenseSignatureEffectSetSO: profile cast/release/hit, lift, kích thước spark và giới hạn spark.
- DefenseSignatureFeedbackAdapter: quản lý phase và handle của đúng cast; không gọi StopOwner để dừng VFX kỹ năng khác.
- DefenseScholarSignaturePresenter3D: bind scene, nghe Downed và dash Started; dọn cả afterglow sau khi cast đã hoàn tất.
- DefenseSealEffectVisual3D: module vẽ vòng niệm theo bán kính; không có logic combat hoặc Update riêng.

Callback cancel/restart xảy ra ngay trong Play được kiểm tra bằng revision trước khi nhận handle. Handle trả về muộn được thu hồi, không gắn vào cast mới. Phase và ID ngăn Started/Released/Ended cũ phát trùng hoặc dừng cast mới.

## Asset đã tạo và gán

Nguồn ở Assets/Features/Defense3D/Effects/Assets:

- VFX_ScholarCast_VongXuyen.prefab + MAT_ScholarSeal_VongXuyen.mat: ấn trận xanh ngọc, viền mực và họa tiết hướng tâm thưa, dùng shader hình học DefenseScholarSeal.
- VFX_ScholarRelease_VongXuyen.prefab: xung lực hai vòng, tái sử dụng module ring.
- FX_ScholarCast: sustained, kết thúc theo sự kiện gameplay.
- FX_ScholarRelease: 0.28 giây.
- FX_ScholarHit: 0.18 giây, dùng chung prefab spark của hệ đánh thường.
- FXSet_ScholarSignature: tối đa 16 spark mỗi cast. Giới hạn chỉ áp dụng trình diễn; gameplay vẫn xử lý toàn bộ snapshot target hợp lệ.

Scene MapSongQue3D_Prototype đã gán presenter, source, health, motor, style, surface và ground anchor dùng chung với dash. Instance cast nằm dưới Effects/Attached; burst/spark dưới Effects/World. Pool và vị trí lưu trữ theo prefab dùng service hiện có.

Instance ScholarSignatureImpact cũ đã được bỏ khỏi scene; aim preview của UI và hoạt ảnh nhân vật được giữ lại. Không tiếp tục chỉnh cùng một hiệu ứng ở cả view cũ và prefab mới.

## Unit Test và xác minh

- DefenseScholarSignatureGameplayTests: windup, damage được chấp nhận/từ chối, scale damage, refund đúng một lần, AoE rỗng, hủy, dash/Downed/pause/restart, caster di chuyển, target đổi vị trí, lỗi observer, restart trong damage, foreign ability và unwind khi damage ném exception.
- DefenseSignatureFeedbackTests: phase/profile, cap spark, ownership riêng, ID/run cũ, callback cancel/new cast trong Play và Dispose.
- DefenseSignatureInterruptionTests: dash và Downed dọn cả burst/spark sau khi gameplay đã resolve.
- DefenseSignatureAuthoringTests: profile/module/prefab, wiring scene, bỏ view cũ và giữ nguyên dữ liệu gameplay.

Đã biên dịch runtime và test source bằng Roslyn ngoài Unity, kiểm tra ID/reference nội bộ scene và prefab bằng văn bản. Chưa chạy Unit Test, Unity, Play Mode, Profiler hoặc trình biên dịch shader Unity. Warning obsolete có sẵn trong DamageProcessorTests nằm ngoài phạm vi.

Người dùng kiểm tra trong Unity: vị trí ấn trận trên địa hình, độ rõ ở radius 6, nhịp skill với animation, spark khi đông quái, dash ngắt niệm, Downed và restart. Script VerifyScholarCombatWiring.py cũ kiểm tra diff với baseline lịch sử trước các đợt VFX; dùng các authoring test hiện tại cho cấu trúc mới.
