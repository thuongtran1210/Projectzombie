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
        private const float YakshaTime = 510f;
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

        private float elapsed;
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
            defenseBase.BaseDestroyed += OnBaseDestroyed;
            heroHealth = sceneInfo.HeroSpawn != null ? sceneInfo.HeroSpawn.GetComponent<DefenseHeroHealth3D>() : null;
            Debug.Log("[TREBRAVE Defense3D] 600s run started: 34 timed events, Miniboss@108, Yaksha@510.");
        }

        private void OnDestroy()
        {
            if (defenseBase != null) defenseBase.BaseDestroyed -= OnBaseDestroyed;
            if (yaksha != null) yaksha.Defeated -= OnYakshaDefeated;
        }

        private void Update()
        {
            if (ended) return;
            elapsed += Time.deltaTime;

            // Preserve all due events even during occasional long frames.
            while (nextEvent < EventCount && elapsed >= nextEvent * EventInterval && !ended)
            {
                SpawnWaveEvent(nextEvent);
                nextEvent++;
            }
            if (!minibossSpawned && elapsed >= MinibossTime)
            {
                minibossSpawned = true;
                SpawnEnemy("magiap_boss_MaGiap_ThongLinh", 1, false, 350f);
                Debug.Log("[TREBRAVE Defense3D] Miniboss at 108 seconds.");
            }
            if (!bossSpawned && elapsed >= YakshaTime)
                SpawnYaksha();
            if (bossSpawned && !assaultStarted && yaksha != null &&
                elapsed >= YakshaTime + yakshaArenaDuration)
            {
                assaultStarted = true;
                if (arenaCombat != null) arenaCombat.EndArena();
                yaksha.BeginBossAssault(defenseBase);
            }
            if (elapsed >= RunDuration && !overtimeLogged)
            {
                overtimeLogged = true;
                Debug.Log("[TREBRAVE Defense3D] OVERTIME: 600s elapsed, wait for Yaksha defeat or Base destruction. No additional timed spawns.");
            }
        }

        private void SpawnWaveEvent(int index)
        {
            // Seven-VX-enemy roster is a design target; these are placeholder capsules.
            string[] names = { "magiap", "thuyquai", "huyetdoi", "quaidu", "phicuong" };
            string kind = names[index % names.Length];
            for (int i = 0; i < enemiesPerEvent; i++)
                SpawnEnemy(kind + "_Event_" + (index + 1) + "_" + (i + 1), (index + i) % 3, false, 0f);
            Debug.Log("[TREBRAVE Defense3D] Timed event " + (index + 1) + "/34 at " + (index * 18) + "s.");
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
            if (ended || defenseBase == null || defenseBase.IsDestroyed) return;
            ended = true;
            won = true;
            defenseBase.LockDamageAfterWin();
            if (arenaCombat != null) arenaCombat.EndArena();
            Debug.Log("[TREBRAVE Defense3D] WIN: Yaksha defeated while Base alive. All future spawns stopped.");
        }

        private void OnBaseDestroyed()
        {
            if (ended) return;
            ended = true;
            won = false;
            if (arenaCombat != null) arenaCombat.EndArena();
            Debug.Log("[TREBRAVE Defense3D] Run ended: LOSS (BaseHP=0). All future spawns stopped.");
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
                GUI.Label(new Rect(28, 134, 330, 22), "Hero HP: " + Mathf.CeilToInt(heroHealth.CurrentHP) + (heroHealth.IsDown ? " (DOWN)" : ""));
            if (ended)
            {
                float panelX = (Screen.width - 420f) * 0.5f;
                float panelY = (Screen.height - 160f) * 0.5f;
                GUI.Box(new Rect(panelX, panelY, 420f, 160f), won ? "TREBRAVE - VICTORY" : "TREBRAVE - DEFEAT");
                GUI.Label(new Rect(panelX + 35f, panelY + 70f, 360f, 35f), won ? "Da Xa defeated. The village survives!" : "Nha Chinh was destroyed.");
            }
        }
    }
}
