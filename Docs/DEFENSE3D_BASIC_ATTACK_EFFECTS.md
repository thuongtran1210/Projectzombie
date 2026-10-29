# Defense3D — Hiệu ứng đánh thường

## Phạm vi

Đánh thường của player và quái hiện dùng service/pool VFX chung. Không đổi damage, cooldown, windup, chọn mục tiêu hoặc knockback. Dash đã được triển khai tiếp theo trong DEFENSE3D_DASH_EFFECTS.md; signature Thư Sinh đã được chuyển trong DEFENSE3D_SCHOLAR_SIGNATURE_EFFECTS.md.

## Luồng sự kiện

```text
Gameplay xác nhận đòn ra sau windup
    → SwingResolved → adapter → profile vệt chém
Gameplay gọi TakeDamage
    → nếu trả về true: HitConfirmed → adapter → profile hit spark
```

Player phát sự kiện từ DefenseHeroAutoAttack3D. Quái đánh cone phát từ DefenseConeAttack3D; quái đánh trực tiếp phát từ DefenseEnemyAttack3D. Hai kiểu quái dùng cùng bộ style được truyền qua run và session factory. Adapter của quái được gỡ theo từng lifetime trước khi trả pool.

Vị trí impact được chụp trước TakeDamage: ưu tiên anchor Hit có sẵn, nếu chưa có anchor thì dùng điểm dự phòng. Payload không giữ reference tới nạn nhân, nên đòn kết liễu hoặc việc despawn/reuse ngay trong callback không làm spark chạy sang vị trí khác. Không gắn spark vào target.

Swing không đồng nghĩa hit. Đánh hụt vẫn có vệt chém; damage bị từ chối không có spark. Hủy windup không phát hai loại hiệu ứng. Callback VFX bị lỗi không được làm mất damage hoặc callback kế tiếp. Danh sách callback được cache khi đăng ký/gỡ, không tạo mảng observer mới trên mỗi hit.

Adapter kiểm tra session, generation và lifetime đang sở hữu source. Sự kiện cũ sau restart hoặc sau khi actor được tái sử dụng bị bỏ qua. Các sự kiện AttackStarted/AttackResolved cũ được giữ để tương thích hoạt ảnh; đã cô lập lỗi observer để không ngắt combat.

## Code và module trình diễn

DefenseHeroAttackEffect3D chỉ còn nối thao tác cast với chủ sở hữu đòn đánh. View đánh thường cũ và Update trình diễn đã được bỏ khỏi component này. Scene cũng bỏ instance HeroAttackImpact cũ để không phát hai lớp VFX.

DefenseEffectInstance3D chỉ quản lý vòng đời và reset. Mỗi hình dạng dùng module DefenseEffectVisual3D:

- DefenseRingEffectVisual3D: giữ mẫu xung lực Dép Tổ Ong.
- DefenseSlashEffectVisual3D: quét cung theo hướng đánh, cập nhật màu/progress qua MaterialPropertyBlock.
- DefenseHitSparkVisual3D: tia ngắn nở rồi mờ, không tìm target hoặc gây damage.

Module mới không cần thêm switch hình dạng vào pool hoặc service. Gán module vào mảng Visuals của prefab. Prefab Dép Tổ Ong đã được cập nhật sang module ring, giữ GUID.

DefenseEffectRequest bổ sung vị trí/hướng quay cho hiệu ứng. Service chuẩn hóa quaternion hợp lệ; API Play cũ vẫn được giữ làm đường gọi tương thích.

## Asset đã gán

Nguồn chính thức trong `Assets/Features/Defense3D/Effects/Assets`:

- VFX_BasicSlash_VongXuyen.prefab và VFX_BasicHit_VongXuyen.prefab được chia sẻ giữa hai phe.
- FX_HeroBasicSwing / FX_HeroBasicHit: chém xanh ngọc dịu, spark vàng ngà.
- FX_EnemyBasicSwing / FX_EnemyBasicHit: chém đỏ son, spark cam ngà.
- FXSet_HeroBasicAttack và FXSet_EnemyBasicAttack cấu hình profile, độ cao và cỡ spark.

Vệt chém dài 0.20–0.22 giây; spark 0.15–0.16 giây. Hit spark có hình tia sắc và viền nâu mực, dùng shader hình học không cần texture. Shader spark dùng ZTest Always để không chìm trong thân model; điều kiện tầm đánh/line-of-sight vẫn do gameplay kiểm tra. Vệt chém dùng shader CombatSlash sẵn có.

Bộ style hero được gán trên DefenseHeroBasicAttackPresenter3D; bộ style quái được gán trên DefenseRun6003D. Composition root bind presenter hero với effect service hiện có. Instance runtime nằm dưới Effects/World và được trả về Pools/Effects/theo prefab.

## Unit Test và bàn giao

- DefenseAttackFeedbackTests: lỗi observer/logger, thay đổi subscription trong callback, scope, pause/restart, lifetime cũ, dispose/rebind, mapping profile và reset module.
- DefenseBasicAttackGameplayTests: hit được chấp nhận/bị từ chối, lethal target disable và đổi vị trí, đánh hụt, hủy windup và lỗi VFX không bỏ qua mục tiêu khác; bao gồm luồng đánh trực tiếp của quái.
- DefenseBasicAttackAuthoringTests: profile/prefab/module, mẫu pháp bảo không bị mất ring, wiring scene và bỏ view cũ.

Đã biên dịch runtime/test source bằng Roslyn ngoài Unity và kiểm tra reference nội bộ scene/prefab bằng văn bản. Chưa chạy Unit Test, Unity, Play Mode, Profiler hoặc trình biên dịch shader của Unity. Warning obsolete có sẵn trong DamageProcessorTests nằm ngoài phạm vi sửa.

Dash đã có start/trail/end theo trạng thái gameplay và handle riêng (xem DEFENSE3D_DASH_EFFECTS.md). Signature Thư Sinh đã có pipeline riêng (xem DEFENSE3D_SCHOLAR_SIGNATURE_EFFECTS.md).
