using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Opt-in one-shot Enemy -> Bridge -> Base gameplay check.
    /// Works in scenes where the Vongxuyen GLB is already placed.
    /// Not production wave spawner or pathfinding AI.
    /// </summary>
    public sealed class DefenseEnemyBridgeSmokeTest3D : MonoBehaviour
    {
        [SerializeField] private DefenseSceneInfo3D sceneInfo;
        [SerializeField] private DefenseBasePrototype3D defenseBase;
        [SerializeField, Min(0f)] private float firstSpawnDelay = 1f;
        [SerializeField] private bool spawnThreeLanes = true;
        private bool spawned;

        private void Start()
        {
            if (sceneInfo == null) sceneInfo = FindObjectOfType<DefenseSceneInfo3D>();
            if (defenseBase == null) defenseBase = FindObjectOfType<DefenseBasePrototype3D>();
            if (sceneInfo == null || sceneInfo.EastEnemySpawn == null || defenseBase == null)
            {
                Debug.LogError("[TREBRAVE Defense3D] Bridge smoke test missing SceneInfo/EnemySpawn/Base.");
                enabled = false;
                return;
            }
            Invoke(nameof(SpawnOnce), firstSpawnDelay);
        }

        private void SpawnOnce()
        {
            if (spawned || defenseBase == null || defenseBase.IsDestroyed) return;
            spawned = true;
            int count = spawnThreeLanes ? 3 : 1;
            for (int i = 0; i < count; i++)
            {
                int lane = spawnThreeLanes ? i : 1;
                float z = lane == 0 ? -10.5f : lane == 2 ? 10.5f : -0.5f;
                var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                enemy.name = "Enemy_Test_Lane_" + (lane + 1);
                enemy.transform.position = new Vector3(sceneInfo.EastEnemySpawn.position.x, 1.1f, z);
                var collider = enemy.GetComponent<Collider>();
                if (collider != null) Destroy(collider); // Prototype route is visual, not physics simulation.
                var renderer = enemy.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                    if (shader != null)
                    {
                        var mat = new Material(shader);
                        mat.color = new Color(0.85f, 0.22f, 0.26f);
                        renderer.material = mat;
                    }
                }
                enemy.AddComponent<DefenseEnemyBridgeWalker3D>().Configure(lane, defenseBase);
            }
            Debug.Log("[TREBRAVE Defense3D] Spawned " + count + " bridge test enemies.");
        }
    }
}
