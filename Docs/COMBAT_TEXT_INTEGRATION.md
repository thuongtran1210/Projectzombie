# Combat text integration

## Ownership

DamageTextManager implements ICombatTextService and retains its existing script GUID and serialized prefab/style fields. It owns a private Unity ObjectPool and CombatTextLeases. Only the owner releases items; coroutine completion includes an increasing lease token. Clear invalidates active leases without resetting tokens. No Update loop is added. Animation coroutines run only for active text.

Max Active Count bounds visible items. A higher-priority request replaces the oldest item at the lowest eligible priority: player > critical/counter > normal. Equal-priority requests are dropped at capacity. Max Pool Size controls retained inactive objects, not active objects. Prewarm runs in Start when prefab/style are configured; lazy initialization remains available for a later configuration.

## Existing 2D scenes

Existing prefab/style references and ShowCustomText callers remain compatible. LegacyDamageTextAdapter owns the HealthSystem static subscription, enabled by Listen To Legacy Damage. A serialized camera is preferred; Camera.main is used only for legacy compatibility. No camera means the request is skipped. Keep one legacy listener per gameplay scene.

Instance and EnemyDebtCollector's existing calls remain as a compatibility boundary. New systems should receive ICombatTextService rather than looking up Instance. Removing the last singleton caller requires wiring the 2D spawn/composition owner separately; it is not silently replaced with another global lookup.

## Defense3D scene setup (performed by the project owner)

1. Add/use a DamageTextManager in the same scene. Assign the existing DamageTextItem prefab and DamageTextStyleConfig. Keep its transform scale at one.
2. On DefenseSceneCompositionRoot, assign Combat Text and Combat Text Camera. The root disables legacy input, configures billboard rendering, and passes the service to the run and enemy factory.
3. Damageable scene participants bind automatically. For other authored health endpoints (supports, structures), add Combat Text Actors entries with the actual IDefenseDamageable3D component, target classification and world-space offset. Do not register a wrapper and its underlying health authority separately.
4. Enemies, including the boss health authority, bind through the session factory and detach before release. Scene actor adapters remain valid across restart; events explicitly carrying an old generation or a foreign session are rejected.
5. Gameplay pause freezes text through the session gate. Run cleanup clears active text; scene disposal stops accepting requests and clears it before session teardown.

The Defense3D scene is now wired to CombatText_VongXuyen under DefenseRuntime/CombatText. The presenter references the main camera, dedicated prefab, shared outlined material and style under Assets/Resources/UI/CombatText. Existing scene changes and the legacy 2D style are preserved. The steps above remain useful for additional scenes. Runtime-created friendly units require their creation owner to construct/dispose DefenseCombatTextAdapter, just like the enemy factory.

## Data and extension

CombatTextRequest supports Damage, Heal, Block, Immune, Resource and Custom. It contains presentation data only. Both adapters preserve resolved attack damage, including overkill, rather than calculating actual HP lost. Defense3D currently has no elemental/counter metadata in its damage context, so the adapter does not invent those values.

New producers call Show(request) after gameplay accepts their result. New kinds can add style/format resolution without changing health or pool code. Existing DamageTextStyleConfig retains its GUID and adds heal/resource/block colors.

## Unit tests

Assets/Tests/Editor/CombatTextTests.cs covers active capacity, priority eviction, duplicate/stale completion, clear/dispose, reentrant cleanup, request validation/mapping, formatting/style precedence, adapter session/generation/pause filtering and unsubscription. Pool allocation is represented by fake objects; no scene or prefab is needed for lease tests.

Unity, Play Mode, Profiler and the Unity test runner were not launched. Tests are supplied for the project owner to run. No claim of zero allocations is made: formatting of decorated numbers and coroutine creation can allocate.


## Style Vong Xuyên đã gán

- Chữ thường: ngà ấm; crit: vàng đất sáng; player nhận sát thương: đỏ son sáng.
- Viền nâu mực, font sans đậm, không glow hoặc bóng đổ để giữ hình chữ rõ.
- Cỡ chữ thường/crit/player: 5.2 / 6.8 / 6.2; thời gian 0.62 giây, bay 0.9 đơn vị/giây, lệch ngang tối đa 0.22 đơn vị.
- Nảy nhẹ từ 1.12 về 1, giữ rõ nửa đầu rồi mờ dần; tối đa 48 text, prewarm 32, giữ lại 64 item nhàn rỗi.
- Asset và wiring có thêm Unit Test trong VongXuyenCombatTextAssetTests.cs. Chưa chạy Unity hoặc test runner; kích thước theo góc camera cần người dùng kiểm tra khi chạy game.
