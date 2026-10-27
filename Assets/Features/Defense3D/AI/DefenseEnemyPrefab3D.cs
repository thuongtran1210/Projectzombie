using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Shared prefab source for the Defense runtime and smoke-test spawners.</summary>
    public static class DefenseEnemyPrefab3D
    {
        public const string EnemyResource = "Defense3D/DefenseEnemy3D";
        public const string BossResource = "Defense3D/DefenseYaksha3D";
        public const string MagiapResource = "Defense3D/DefenseEnemy_Magiap";
        public const string ThuyQuaiResource = "Defense3D/DefenseEnemy_ThuyQuai";
        public const string HuyetDoiResource = "Defense3D/DefenseEnemy_HuyetDoi";
        public const string QuaiDuResource = "Defense3D/DefenseEnemy_QuaiDu";
        public const string PhiCuongResource = "Defense3D/DefenseEnemy_PhiCuong";
        public const string MagiapBossResource = "Defense3D/DefenseEnemy_MagiapBoss";

        public static string GetResourceForArchetype(string enemyId)
        {
            switch (enemyId)
            {
                case DefenseEnemyRegistry.ID_MAGIAP: return MagiapResource;
                case DefenseEnemyRegistry.ID_THUYQUAI: return ThuyQuaiResource;
                case DefenseEnemyRegistry.ID_HUYETDOI: return HuyetDoiResource;
                case DefenseEnemyRegistry.ID_QUAIDU: return QuaiDuResource;
                case DefenseEnemyRegistry.ID_PHICUONG: return PhiCuongResource;
                case DefenseEnemyRegistry.ID_MAGIAP_BOSS: return MagiapBossResource;
                case DefenseEnemyRegistry.ID_YAKSHA_BOSS: return BossResource;
                default: return EnemyResource;
            }
        }

        public static GameObject Instantiate(bool boss = false)
        {
            string resource = boss ? BossResource : EnemyResource;
            var prefab = Resources.Load<GameObject>(resource);
            if (prefab == null)
                throw new System.InvalidOperationException("Missing Defense enemy prefab: Resources/" + resource);
            return Instantiate(prefab);
        }

        public static GameObject Instantiate(GameObject prefab, Transform parent = null)
        {
            if (prefab == null) throw new System.ArgumentNullException(nameof(prefab));
#if UNITY_EDITOR
            // Object.Instantiate loses the source-prefab connection in Play Mode.
            // Keep it in the Editor so spawned enemies expose Select/Open Prefab.
            return (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab,parent);
#else
            return Object.Instantiate(prefab,parent);
#endif
        }
    }
}
