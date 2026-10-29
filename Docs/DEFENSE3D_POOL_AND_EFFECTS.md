# Defense3D — Pool, Hierarchy và hiệu ứng pháp bảo

## Phạm vi đã triển khai

- Pool quái và wood dùng chung lõi `OwnedPool<T>` C# thuần; mỗi lượt mượn có owner, item và token.
- Luồng production nhận dependency trực tiếp qua composition root, run, factory, drop và village shop.
- Session sở hữu thứ tự cleanup/dispose; runtime/factory kết thúc trước khi pool bị hủy.
- Scene MapSongQue3D_Prototype có root và reference mới đã gán sẵn.
- Ma Giáp là prefab mẫu có VisualRoot và Hit/Head/Cast/Ground. Các prefab khác giữ cấu trúc cũ; không sửa hàng loạt khi chưa duyệt mẫu.
- Dép Tổ Ong phát sự kiện impact; adapter riêng phát prefab pooled, không quyết định damage/cooldown.

## Hierarchy đã gán

```text
DefenseRuntime
├── Actors
│   ├── Enemies
│   └── Allies
├── Staging                  inactive
├── Pickups
├── Pools                    inactive
│   ├── Enemies / theo prefab
│   ├── Pickups / theo prefab
│   └── Effects / theo prefab
├── Effects
│   ├── World
│   └── Attached
└── CombatText
    └── CombatText_VongXuyen
```

Root được gán bằng serialized reference, không tìm bằng tên khi combat. Root có transform identity. Các group theo prefab được pool tạo dưới nhóm đã gán khi phát sinh prefab đó lần đầu. Những actor/slot đồng minh đã được thiết kế sẵn trong scene vẫn giữ vị trí authored; `Allies` dành cho luồng spawn đồng minh mở rộng sau.

Object được tạo dưới storage inactive, bind/reset khi inactive rồi mới chuyển sang root active và bật. Khi trả pool, đóng lượt mượn trước callback, dừng hành vi, gỡ lifecycle session và deactivate. Việc reparent về storage được hoãn đến frame an toàn: run gọi FlushReturns cho quái/pickup; service VFX dùng coroutine ngắn cho các lượt trả đang chờ. Không reparent trực tiếp trong OnDisable của ancestor.

## Ownership và API

`OwnedPool<T>` chịu trách nhiệm identity lượt mượn, active/idle, reset và dispose. Pool cụ thể quyết định cách tạo/hủy/reset Unity object. Factory giữ `PoolLease<GameObject>` cùng `DefenseActorLifetime`:

- PoolLease xác nhận lượt sử dụng object, chặn release trùng/cũ/sai pool.
- DefenseActorLifetime xác nhận đời sống gameplay, session/run và quyền nhận reward.
- Callback pickup giữ token của đúng lượt; callback cũ không thể trả item đã được mượn lại.
- Pool theo dõi object active khi dispose; không chỉ clear item nhàn rỗi.
- Reset lỗi sẽ hủy item lỗi; cleanup tiếp tục thử xử lý các item còn lại.
- Max idle giới hạn bộ nhớ giữ lại, không giới hạn số quái/pickup đang hoạt động để tránh làm mất gameplay.

API static ActiveSessionPool/RegisterSessionPool chỉ giữ cho caller tương thích cũ. Luồng run hiện tại không đăng ký hoặc tra cứu chúng. Đăng ký pool khác khi đã có owner sẽ báo lỗi, không tự dispose tài nguyên của session khác. API Get/Release không token chỉ dành cho caller đồng bộ cũ; callback bất đồng bộ phải giữ PoolLease.

## Quy tắc prefab

Ma Giáp giữ GUID và các reference visual con cũ. VisualRoot bọc visual đã có; anchor nằm riêng để chuyển động visual không đẩy điểm gắn gameplay. Renderer placeholder bị tắt trên root vẫn được giữ tương thích; renderer hoạt động thuộc VisualRoot. Collider/NavMesh/gameplay không chuyển vào VisualRoot.

Prefab VFX mẫu nằm duy nhất trong `Assets/Features/Defense3D/Effects/Assets`:

- `VFX_DepToOng_Impact.prefab`: hai vòng xung lực, không collider hoặc component gameplay.
- `MAT_DepToOng_Impact.mat`: dùng shader hình học đang có, màu vàng đất Vong Xuyên, không tạo material instance mỗi lần phát.
- `FX_DepToOng_Impact.asset`: thời gian 0.48 giây, priority 2, scale theo bán kính gameplay.

View reset particle, trail, pose child và property block. Hình dạng được tách thành các module DefenseEffectVisual3D; prefab Dép đã chuyển sang DefenseRingEffectVisual3D và giữ GUID. Particle/trail được cache một lần khi sử dụng. Khi thêm particle vào prefab, đặt stopAction không Destroy và tắt playOnAwake; service bắt đầu/kết thúc chúng.

## Luồng pháp bảo

```text
DefenseSkillCastRuntime xác nhận và commit cooldown
    → DefenseRelicSkillEffect3D.Execute
        → ImpactOccurred
            → DefenseRelicEffectPresenter3D
                → IDefenseEffectService.Play(profile)
        → Damage và status như trước
```

Observer VFX lỗi được ghi log nhưng không chặn damage hoặc observer khác. Thiếu cấu hình bắt buộc được kiểm tra lúc bind. Hiệu ứng quá tải có thể bỏ hoặc thay hiệu ứng priority thấp hơn; gameplay không bị bỏ. Indicator báo vùng nguy hiểm vẫn thuộc hệ indicator hiện có, không dùng budget VFX trang trí.

Damage của Dép vẫn tức thì; hiệu ứng vòng lan không phải projectile gây damage. Muốn thêm thời gian bay có ảnh hưởng đến thời điểm trúng phải thay đổi runtime gameplay riêng.

## Thêm hiệu ứng

1. Tạo prefab chỉ trình diễn, root scale 1, gán DefenseEffectInstance3D và renderer/particle cần thiết.
2. Tạo DefenseEffectProfileSO: prefab, duration, màu, scale, priority; bật Sustained nếu phải dừng bằng handle.
3. Adapter nghe sự kiện gameplay đã xác nhận và gọi IDefenseEffectService. Không gọi singleton từ skill.
4. Hiệu ứng tại điểm dùng position; hiệu ứng bám dùng anchor và owner. Instance vẫn thuộc Effects/Attached, không parent trực tiếp vào actor.
5. Giữ handle để Stop khi trạng thái kết thúc; gọi StopOwner khi owner despawn. Factory quái đã làm việc này trước khi trả pool, kể cả reuse cùng frame.
6. Bind/gỡ adapter theo session. RunCleanup clear hiệu ứng; handle cũ không thể dừng lượt phát mới.

Đồng hồ hiệu ứng dùng thời gian gameplay. Pause dừng tiến độ và particle. Coroutine trình diễn chỉ chạy khi có hiệu ứng active; coroutine đưa về storage chỉ chạy khi có lượt trả chờ xử lý.

## Kiểm tra và giới hạn

Unit Test mới: OwnedPoolTests, DefensePoolLifecycleTests, DefenseEffectAuthoringTests, DefenseRelicPresentationTests. Bao phủ token cũ, sai pool, release trùng, dispose active, cleanup tái nhập/lỗi, callback pickup cũ, cấu trúc prefab, profile/scene wiring và lỗi observer VFX không chặn damage/cooldown.

Đã kiểm tra biên dịch bằng Roslyn C# ngoài Unity cho runtime và test source. Chưa chạy Unit Test, Unity, Play Mode hoặc Profiler. Warning obsolete có sẵn trong DamageProcessorTests chưa được sửa ngoài phạm vi.

Cần người dùng chạy test và xem trực quan: chiều cao đặt vòng trên địa hình, kích thước vòng, sorting và lifecycle khi đổi scene/restart. Đánh thường player/quái đã chuyển sang service mới (xem DEFENSE3D_BASIC_ATTACK_EFFECTS.md). Dash đã chuyển với start/trail/end (xem DEFENSE3D_DASH_EFFECTS.md). Signature Thư Sinh đã chuyển (xem DEFENSE3D_SCHOLAR_SIGNATURE_EFFECTS.md). Prefab quái khác và hệ VFX 2D chưa chuyển hàng loạt; các adapter có thể được mở rộng theo mẫu Dép Tổ Ong. Không thay GlobalVFXPoolManager của 2D trong đợt này.
