# Defense3D enemy overhead health

The legacy OverheadStatusPresenter reads HealthSystem and is not connected to DefenseEnemy3D. The session factory now binds DefenseEnemyHealthPresenter3D once per actor lease. It receives HealthChanged and Defeated, updates a passive single-quad view, and unbinds it on release before returning to the existing pool.

All seven production enemy definition prefabs have an authored view, shared URP material and presenter. Magiap has a Head anchor; enemy prefabs without an anchor use their root collider bounds top. No runtime hierarchy search or GameObject/material creation is needed per frame. The camera is cached at factory construction, so the scene must have its enabled MainCamera before session initialization. A missing presentation camera hides the bar rather than changing gameplay.

Bars are visible at full health, billboard toward the camera, hide offscreen/behind camera or during pause/death, and reset when reused. Normal bars are red; boss/miniboss bars are wider and orange. This only adds overhead enemy bars, not an additional boss HUD or cannon health bar. Depth testing respects scene geometry.

Rebuild: Tools > ProjectZombie > Build Enemy Health Bars. Validate: Tools > ProjectZombie > Validate Enemy Health Bars. Validation writes scratch/EnemyHealthValidation.txt/.xml and renders scratch/EnemyHealthPreview.png. Tests cover seven prefab bindings plus health, pause, death and a new actor lease with both Head and collider fallback.

Current verification: seven prefab YAML/reference checks passed; current C# changes and test sources compile against installed Unity assemblies. Unity runtime tests and image render were not run because computer-use automatic approval review repeatedly failed with model capacity. No test pass count or gameplay screenshot is claimed.
