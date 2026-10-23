using System.Collections;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Opt-in mini Defense vertical slice: three short waves, then survival.
    /// Intentionally not the approved 600-second / 34-event production schedule.
    /// </summary>
    public sealed class DefenseWaveSlice3D : MonoBehaviour
    {
        [SerializeField] private DefenseSceneInfo3D sceneInfo;
        [SerializeField] private DefenseBasePrototype3D defenseBase;
        [SerializeField, Min(1)] private int waveCount = 3;
        [SerializeField, Min(1)] private int enemiesPerWave = 3;
        [SerializeField, Min(1f)] private float waveInterval = 16f;
        [SerializeField, Min(0.1f)] private float enemySpacing = 1f;
        private int wave;
        private bool defeated;

        private void Start()
        {
            if (sceneInfo == null) sceneInfo = FindObjectOfType<DefenseSceneInfo3D>();
            if (defenseBase == null) defenseBase = FindObjectOfType<DefenseBasePrototype3D>();
            if (sceneInfo == null || sceneInfo.EastEnemySpawn == null || defenseBase == null)
            {
                Debug.LogError("[TREBRAVE Defense3D] DefenseWaveSlice3D needs active SceneInfo, EnemySpawn and Base.");
                enabled = false;
                return;
            }
            defenseBase.BaseDestroyed += OnBaseDestroyed;
            StartCoroutine(RunWaves());
        }

        private void OnDestroy()
        {
            if (defenseBase != null) defenseBase.BaseDestroyed -= OnBaseDestroyed;
        }

        private void OnBaseDestroyed()
        {
            defeated = true;
            StopAllCoroutines();
        }

        private IEnumerator RunWaves()
        {
            for (wave = 1; wave <= waveCount && !defeated; wave++)
            {
                Debug.Log("[TREBRAVE Defense3D] Wave " + wave + "/" + waveCount + " begins.");
                for (int i = 0; i < enemiesPerWave && !defeated; i++)
                {
                    Spawn(i % 3, wave, i);
                    yield return new WaitForSeconds(enemySpacing);
                }
                if (wave < waveCount && !defeated)
                    yield return new WaitForSeconds(Mathf.Max(0f, waveInterval - enemiesPerWave * enemySpacing));
            }
            if (!defeated)
                Debug.Log("[TREBRAVE Defense3D] Test waves complete. Survive until BaseHP=0; no boss/win logic yet.");
        }

        private void Spawn(int lane, int currentWave, int ordinal)
        {
            float z = lane == 0 ? -10.5f : lane == 2 ? 10.5f : -0.5f;
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "Enemy_Wave_" + currentWave + "_Lane_" + (lane + 1) + "_" + ordinal;
            enemy.transform.position = new Vector3(sceneInfo.EastEnemySpawn.position.x, 1.1f, z);
            var collider = enemy.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            var renderer = enemy.GetComponent<Renderer>();
            if (renderer != null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (shader != null)
                {
                    var material = new Material(shader);
                    material.color = new Color(0.78f, 0.12f, 0.25f);
                    renderer.material = material;
                }
            }
            enemy.AddComponent<DefenseEnemyBridgeWalker3D>().Configure(lane, defenseBase);
        }

        private void OnGUI()
        {
            if (defenseBase == null) return;
            GUI.Box(new Rect(16, 16, 325, 92), "TREBRAVE Defense 3D - Test Slice");
            GUI.Label(new Rect(30, 44, 290, 22),
                "Base HP: " + Mathf.CeilToInt(defenseBase.CurrentHP) + "/" + Mathf.CeilToInt(defenseBase.MaxHP));
            GUI.Label(new Rect(30, 68, 290, 22),
                defeated ? "THUA - Nha Chinh bi pha huy"
                : "Wave: " + Mathf.Min(wave, waveCount) + "/" + waveCount);
        }
    }
}
