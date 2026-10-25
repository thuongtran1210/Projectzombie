using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Prototype of the approved 600-second Defense schedule. 34 timed spawn
    /// events at 0..594 seconds (every 18s), miniboss at 108s, Yaksha at 510s.
    /// Win requires Yaksha defeat with Base alive; BaseHP zero is the only loss.
    /// A surviving Yaksha after 600s causes overtime, not an automatic loss.
    /// </summary>
    public sealed class DefenseRun6003D : MonoBehaviour
    {
        private const int EventCount = 34;
        private const float EventInterval = 18f;
        private const float MinibossTime = 108f;
        private const float YakshaTime = 10f;
        private const float RunDuration = 600f;

        [SerializeField] private DefenseSceneInfo3D sceneInfo;
        [SerializeField] private DefenseBasePrototype3D defenseBase;
        [SerializeField, Min(1f)] private float yakshaArenaDuration = 45f;
        [SerializeField, Min(1f)] private float yakshaHealth = 1200f;
        [SerializeField, Min(1)] private int enemiesPerEvent = 3;
        [Header("Optional Yaksha visual (child of gameplay capsule)")]
        [SerializeField] private GameObject yakshaVisualPrefab;
        [SerializeField] private Vector3 yakshaVisualLocalOffset = Vector3.zero;
        [SerializeField] private Vector3 yakshaVisualLocalEuler = Vector3.zero;
        [SerializeField] private Vector3 yakshaVisualLocalScale = Vector3.one;

        private DefenseG1RuntimeAdapter g1;
        private bool yakshaDefeated;
        private float elapsed => g1 != null ? g1.Run.Clock.ElapsedSeconds : 0f;
        private int nextEvent;
        private bool minibossSpawned;
        private bool bossSpawned;
        private bool assaultStarted;
        private bool overtimeLogged;
        private bool ended;
        private bool won;
        private DefenseEnemyBridgeWalker3D yaksha;
        private DefenseYakshaArenaCombat3D arenaCombat;
        private DefenseHeroHealth3D heroHealth;
        private readonly System.Collections.Generic.List<DefenseEnemyBridgeWalker3D> spawnedEnemies = new System.Collections.Generic.List<DefenseEnemyBridgeWalker3D>();
        private readonly DefenseSpawnPlanner spawnPlanner = new DefenseSpawnPlanner();
        private System.Collections.Generic.List<DefenseStageEvent> stageEvents;

        private void Start()
        {
            if (sceneInfo == null) sceneInfo = FindObjectOfType<DefenseSceneInfo3D>();
            if (defenseBase == null) defenseBase = FindObjectOfType<DefenseBasePrototype3D>();
            if (sceneInfo == null || sceneInfo.EastEnemySpawn == null || defenseBase == null)
            {
                Debug.LogError("[TREBRAVE Defense3D] 600s run needs active SceneInfo, EastEnemySpawn, Base.");
                enabled = false;
                return;
            }
            stageEvents = DefenseStage01Schedule.GenerateEvents();
            spawnPlanner.JobSpawned += OnJobSpawned;
            heroHealth = sceneInfo.HeroSpawn != null ? sceneInfo.HeroSpawn.GetComponent<DefenseHeroHealth3D>() : null;
            g1 = new DefenseG1RuntimeAdapter();
            g1.RegisterOwner(CleanupCurrentRunEntities, ResetRunState);
            g1.Start();
            defenseBase.BaseDestroyed += OnBaseDestroyed;
            Debug.Log("[TREBRAVE Defense3D] 600s run started: 34 timed events (D-05 Miniboss@108s, D-11/A Phi Cuong elite lead), Yaksha@510s.");
        }

        private void CleanupCurrentRunEntities()
        {
            for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
            {
                if (spawnedEnemies[i] != null)
                {
                    Destroy(spawnedEnemies[i].gameObject);
                }
            }
            spawnedEnemies.Clear();
            spawnPlanner.Reset();
            if (arenaCombat != null)
            {
                arenaCombat.EndArena();
                arenaCombat = null;
            }
            yaksha = null;
        }

        private void ResetRunState()
        {
            yakshaDefeated = false;
            nextEvent = 0;
            minibossSpawned = false;
            bossSpawned = false;
            assaultStarted = false;
            overtimeLogged = false;
            ended = false;
            won = false;

            if (defenseBase != null) defenseBase.ResetBase();
            if (heroHealth != null) heroHealth.ResetHero();
        }

        public void RestartRun()
        {
            if (g1 == null) return;
            g1.Restart();
            g1.Start();
            Debug.Log("[TREBRAVE Defense3D] Run restarted cleanly via G1 adapter.");
        }

        private void OnDestroy()
        {
            if (defenseBase != null) defenseBase.BaseDestroyed -= OnBaseDestroyed;
            if (yaksha != null) yaksha.Defeated -= OnYakshaDefeated;
            if (spawnPlanner != null) spawnPlanner.JobSpawned -= OnJobSpawned;
            if (g1 != null) g1.Dispose();
        }

        private void Update()
        {
            if (ended || g1 == null) return;
            // One authoritative clock and terminal resolver; Base loss precedes boss victory.
            float acceptedDt = g1.Tick(Time.deltaTime, defenseBase.CurrentHP, yakshaDefeated);
            if (heroHealth != null)
                heroHealth.AdvanceGameplayTime(acceptedDt, g1.Run.State.IsTerminal);

            ApplyG1Result();
            if (ended) return;

            // Preserve all due events and enqueue their jobs into the bounded SpawnPlanner
            if (stageEvents == null) stageEvents = DefenseStage01Schedule.GenerateEvents();
            while (nextEvent < stageEvents.Count && elapsed >= stageEvents[nextEvent].TriggerTime && !ended)
            {
                EnqueueWaveEvent(stageEvents[nextEvent]);
                nextEvent++;
            }

            // Process spawn queue with budget (max 4 per frame, max 32 active)
            spawnPlanner.ProcessSpawns(elapsed, CanSafelySpawn);

            if (!bossSpawned && elapsed >= YakshaTime)
                SpawnYaksha();
            if (bossSpawned && !assaultStarted && yaksha != null &&
                elapsed >= YakshaTime + yakshaArenaDuration)
            {
                assaultStarted = true;
                if (arenaCombat != null) arenaCombat.EndArena();
                yaksha.BeginBossAssault(defenseBase);
            }
            // Re-evaluate after frame events; never make pre-600 boss defeat a victory.
            float terminalCheckDt = g1.Tick(0f, defenseBase.CurrentHP, yakshaDefeated);
            if (heroHealth != null)
                heroHealth.AdvanceGameplayTime(terminalCheckDt, g1.Run.State.IsTerminal);

            ApplyG1Result();
            if (ended) return;
            if (g1.Run.State.Phase == DefenseGamePhase.Overtime && !overtimeLogged)
            {
                overtimeLogged = true;
                Debug.Log("[TREBRAVE Defense3D] OVERTIME: 600s elapsed, wait for Yaksha defeat or Base destruction. No additional timed spawns.");
            }
        }

        private void EnqueueWaveEvent(DefenseStageEvent evt)
        {
            for (int slot = 0; slot < evt.JobCount; slot++)
            {
                string actualType = evt.DeclaredType;
                // D-11/A: Magiap events have slot 0 assigned as Phi Cuong elite
                if (evt.DeclaredType == DefenseEnemyRegistry.ID_MAGIAP && slot == 0)
                {
                    actualType = DefenseEnemyRegistry.ID_PHICUONG;
                }
                var job = new DefenseSpawnJob(evt.EventId, slot, evt.DeclaredType, actualType, evt.Zone, evt.TriggerTime);
                spawnPlanner.EnqueueJob(job, elapsed);
            }
            Debug.Log($"[TREBRAVE Defense3D] Enqueued event {evt.EventId} ({evt.DeclaredType} x{evt.JobCount}) at {evt.TriggerTime}s.");
        }

        private bool CanSafelySpawn(DefenseSpawnJob job)
        {
            // Safety rule: avoid spawning directly on top of Hero if closer than 3.1m
            if (heroHealth != null && sceneInfo != null && sceneInfo.EastEnemySpawn != null)
            {
                float dist = Vector3.Distance(heroHealth.transform.position, sceneInfo.EastEnemySpawn.position);
                if (dist < 3.1f) return false;
            }
            return true;
        }

        private void OnJobSpawned(DefenseSpawnJob job)
        {
            int lane = job.SlotIndex % 3;
            float hpOverride = 0f;
            if (job.ActualSpawnType == DefenseEnemyRegistry.ID_MAGIAP_BOSS)
            {
                hpOverride = 700f;
            }
            else if (DefenseEnemyRegistry.TryGetStats(job.ActualSpawnType, out var stats))
            {
                hpOverride = stats.MaxHp;
            }
            string enemyName = $"{job.ActualSpawnType}_{job.EventId}_Slot{job.SlotIndex + 1}";
            SpawnEnemy(enemyName, lane, false, hpOverride);
        }


        private DefenseEnemyBridgeWalker3D SpawnEnemy(string enemyName, int lane, bool arenaBoss, float health)
        {
            float z = lane == 0 ? -10.5f : lane == 2 ? 10.5f : -0.5f;
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = enemyName;
            go.transform.position = new Vector3(sceneInfo.EastEnemySpawn.position.x, 1.1f, arenaBoss ? 0f : z);
            var collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider); // Existing test enemies are visual-only.
            var renderer = go.GetComponent<Renderer>();
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (renderer != null && shader != null)
            {
                var material = new Material(shader);
                material.color = arenaBoss ? new Color(0.55f, 0.10f, 0.8f) : new Color(0.78f, 0.14f, 0.23f);
                renderer.material = material;
            }
            if (arenaBoss)
            {
                if (yakshaVisualPrefab == null)
                    go.transform.localScale = Vector3.one * 2.3f;
                else
                {
                    if (renderer != null) renderer.enabled = false;
                    var visual = Instantiate(yakshaVisualPrefab, go.transform);
                    visual.name = "Yaksha_3D_Visual";
                    visual.transform.localPosition = yakshaVisualLocalOffset;
                    visual.transform.localRotation = Quaternion.Euler(yakshaVisualLocalEuler);
                    visual.transform.localScale = Vector3.Scale(visual.transform.localScale, yakshaVisualLocalScale);
                    // Gameplay capsule remains the NavMesh/HP authority.
                    foreach (var visualCollider in visual.GetComponentsInChildren<Collider>())
                        visualCollider.enabled = false;
                }
            }
            var enemy = go.AddComponent<DefenseEnemyBridgeWalker3D>();
            if (arenaBoss) enemy.PrepareArenaBoss(health);
            else
            {
                if (health > 0f) enemy.SetMaxHealth(health);
                enemy.Configure(lane, defenseBase);
            }
            spawnedEnemies.Add(enemy);
            return enemy;
        }


        private void SpawnYaksha()
        {
            bossSpawned = true;
            yaksha = SpawnEnemy("yaksha_boss_ARENA_COMBAT", 1, true, yakshaHealth);
            yaksha.Defeated += OnYakshaDefeated;
            arenaCombat = yaksha.gameObject.AddComponent<DefenseYakshaArenaCombat3D>();
            arenaCombat.Begin(heroHealth);
            Debug.Log("[TREBRAVE Defense3D] Yaksha spawned at world position " + yaksha.transform.position + " (East arena). Select yaksha_boss_ARENA_COMBAT in Hierarchy while playing, then press F in Scene View. Yaksha Visual Prefab assigned: " + (yakshaVisualPrefab != null) + ".");
            Debug.Log("[TREBRAVE Defense3D] Yaksha entered ARENA_COMBAT at 510s. Assault after " + yakshaArenaDuration + "s.");
        }

        private void OnYakshaDefeated(DefenseEnemyBridgeWalker3D enemy)
        {
            if (ended || g1 == null) return;
            yakshaDefeated = true;
            g1.Tick(0f, defenseBase.CurrentHP, true);
            ApplyG1Result();
        }

        private void OnBaseDestroyed()
        {
            if (ended || g1 == null) return;
            g1.Tick(0f, 0f, yakshaDefeated);
            ApplyG1Result();
        }

        private void ApplyG1Result()
        {
            if (ended || g1 == null || !g1.Run.State.IsTerminal) return;
            ended = true;
            won = g1.Run.State.Phase == DefenseGamePhase.Win;
            if (won) defenseBase.LockDamageAfterWin();
            if (arenaCombat != null) arenaCombat.EndArena();
            Debug.Log(won
                ? "[TREBRAVE Defense3D] WIN: 600s elapsed and Yaksha defeated while Base alive."
                : "[TREBRAVE Defense3D] LOSS: BaseHP=0 (priority over WIN).");
        }

        private void OnGUI()
        {
            if (defenseBase == null) return;
            GUI.Box(new Rect(16, 16, 355, 158), "TREBRAVE Defense 3D - 600s Run");
            GUI.Label(new Rect(28, 43, 330, 22), "Time: " + Mathf.FloorToInt(elapsed) + "s / 600s | Events: " + nextEvent + "/34");
            GUI.Label(new Rect(28, 66, 330, 22), "Base HP: " + Mathf.CeilToInt(defenseBase.CurrentHP) + "/" + Mathf.CeilToInt(defenseBase.MaxHP));
            GUI.Label(new Rect(28, 89, 330, 22), ended ? (won ? "WIN - Yaksha defeated" : "LOSS - Base destroyed") :
                elapsed >= 600f ? "OVERTIME - Yaksha alive" :
                bossSpawned ? (assaultStarted ? "Yaksha: ASSAULT_BASE" : "Yaksha: ARENA_COMBAT") : "Defend the West Bank");
            if (bossSpawned && yaksha != null && !yaksha.IsDead)
                GUI.Label(new Rect(28, 112, 330, 22), "Yaksha HP: " + Mathf.CeilToInt(yaksha.CurrentHP));
            if (heroHealth != null)
            {
                string heroText = "Hero HP: " + Mathf.CeilToInt(heroHealth.CurrentHP);
                if (heroHealth.IsDown)
                    heroText += " (DOWN - Respawn in " + Mathf.CeilToInt(heroHealth.RemainingRespawnTime) + "s)";
                GUI.Label(new Rect(28, 134, 330, 22), heroText);
            }
            if (ended)
            {
                float panelX = (Screen.width - 420f) * 0.5f;
                float panelY = (Screen.height - 180f) * 0.5f;
                GUI.Box(new Rect(panelX, panelY, 420f, 180f), won ? "TREBRAVE - VICTORY" : "TREBRAVE - DEFEAT");
                GUI.Label(new Rect(panelX + 35f, panelY + 60f, 360f, 35f), won ? "Da Xa defeated. The village survives!" : "Nha Chinh was destroyed.");
                if (GUI.Button(new Rect(panelX + 135f, panelY + 110f, 150f, 40f), "Restart Match"))
                {
                    RestartRun();
                }
            }
        }
    }
}

