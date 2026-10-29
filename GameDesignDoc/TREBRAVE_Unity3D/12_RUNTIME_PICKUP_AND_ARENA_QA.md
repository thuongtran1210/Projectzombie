# Defense3D — Runtime pickup and Yaksha arena QA

**SUPERSEDED:** The original runner reported 17 passes, but repeated PlayMode
entry could skip later coroutine bodies. The runtime claims below are historical
and are not accepted as complete execution evidence. See
[13_ENEMY_FOUNDATION.md](13_ENEMY_FOUNDATION.md) for the replacement run, which
requires completion markers for every runtime scenario in one PlayMode session.

Date: 2026-10-25 (Asia/Saigon)

## Result

Unity 2022.3.62f3, Test Framework 1.1.33: **17 passed, 0 failed**.
Evidence: `Logs/TraversalQA/defense-runtime-integration.xml` (local test artifact).

The suite is launched as EditMode tests. The two coroutine integration tests
explicitly enter and exit Play Mode using `EnterPlayMode` / `ExitPlayMode`.
Their waits assert that Unity is playing and use the real gameplay clock and
frame updates; they do not directly invoke the runtime `Update` methods.
The open map's run, wallet, enemy and pickup objects are disabled inside the
Play Mode fixture to isolate the test. Exiting Play Mode restores the scene.

## Verified

| Coverage | Tests | Result |
| --- | ---: | --- |
| Runtime Enemy death and Wood pickup integration | 1 | PASS |
| Runtime Yaksha warning ring and arena damage integration | 1 | PASS |
| Editable Enemy/Yaksha source prefab connections | 2 | PASS |
| Wood prefab creation without collector and initial pickup delay | 2 | PASS |
| 3D targeting, ground, dash and teleport regressions | 6 | PASS |
| G2 registry, health and reward contracts | 3 | PASS |
| G6 boss state and match result contracts | 2 | PASS |

Runtime Wood assertions:

- Two lethal hits in the same frame emit one defeat and create one WoodDrop.
- Enemy death does not immediately credit the wallet.
- The defeated Enemy is destroyed; its drop remains while the Hero is distant.
- Moving the Hero to the drop credits 10 Wood exactly once and destroys the drop.
- A drop spawned on the Hero waits before collection; pausing freezes this delay.
- A drop still spawns without a Hero/wallet and can be collected by a later Hero.

Runtime Yaksha assertions:

- The warning ring is initially hidden, then becomes visible before damage.
- A Hero inside the blast takes 30 damage after the warning.
- A Hero outside the blast radius takes no further damage.
- Ending arena combat hides the ring and prevents further blast damage.

## Limits and remaining checks

This is a focused integration result, not sign-off for the entire F07 or G2–G6 gates.
It does not validate the full run controller's arena-to-Base transition, NavMesh
paths across all three bridges, authored models/animation, 90-second expiry,
terminal-state pickup behavior, EXP pickups, a full 600-second match, or Android
performance. Those checks remain outstanding; the deterministic G6 state tests
are not evidence that the scene's NavMesh transition has passed.

Next scene acceptance: Yaksha arena → Base assault with HP preserved, followed
by boss defeat/Base defeat/600-second outcome checks using a separate test profile.
