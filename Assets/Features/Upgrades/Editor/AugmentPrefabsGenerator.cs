#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Upgrades.Runtimes.Augments;

namespace ProjectZombie.Features.Upgrades.Editor
{
    public static class AugmentPrefabsGenerator
    {
        private const string PREFAB_DIR = "Assets/_Data/Upgrades/Prefabs/Augments";
        private const string AUGMENTS_DATA_DIR = "Assets/_Data/Upgrades/MutationAugments";

        [MenuItem("ProjectZombie/Upgrades/⚡ Sinh 12 Prefabs Cơ Chế Cho Lõi Vàng & Kim Cương", false, 30)]
        public static void GenerateAllAugmentPrefabs()
        {
            if (!Directory.Exists(PREFAB_DIR))
            {
                Directory.CreateDirectory(PREFAB_DIR);
                AssetDatabase.Refresh();
            }

            // 1. Tạo 6 Prefabs Lõi Vàng
            CreateOrUpdatePrefab<FireTrailAugmentRuntime>("PREFAB_AUG_GOLD_FIRE_TRAIL", "AUG_GOLD_FIRE_TRAIL");
            CreateOrUpdatePrefab<ChainLightningAugmentRuntime>("PREFAB_AUG_GOLD_CHAIN_LIGHTNING", "AUG_GOLD_CHAIN_LIGHTNING");
            CreateOrUpdatePrefab<CritExplodeAugmentRuntime>("PREFAB_AUG_GOLD_CRIT_EXPLODE", "AUG_GOLD_CRIT_EXPLODE");
            CreateOrUpdatePrefab<ExecuteAugmentRuntime>("PREFAB_AUG_GOLD_EXECUTE", "AUG_GOLD_EXECUTE");
            CreateOrUpdatePrefab<ShieldBashAugmentRuntime>("PREFAB_AUG_GOLD_SHIELD_BASH", "AUG_GOLD_SHIELD_BASH");
            CreateOrUpdatePrefab<VampiricFrenzyAugmentRuntime>("PREFAB_AUG_GOLD_VAMPIRIC_FRENZY", "AUG_GOLD_VAMPIRIC_FRENZY");

            // 2. Tạo 6 Prefabs Lõi Kim Cương
            CreateOrUpdatePrefab<PhuDongGigantismAugmentRuntime>("PREFAB_AUG_PRIS_PHUDONG_GIGANTISM", "AUG_PRIS_PHUDONG_GIGANTISM");
            CreateOrUpdatePrefab<KimQuyMulticastAugmentRuntime>("PREFAB_AUG_PRIS_KIMQUY_MULTICAST", "AUG_PRIS_KIMQUY_MULTICAST");
            CreateOrUpdatePrefab<TanVienImmortalAugmentRuntime>("PREFAB_AUG_PRIS_TANVIEN_IMMORTAL", "AUG_PRIS_TANVIEN_IMMORTAL");
            CreateOrUpdatePrefab<ThuyBaFreezeBurstAugmentRuntime>("PREFAB_AUG_PRIS_THUYBA_FREEZE_BURST", "AUG_PRIS_THUYBA_FREEZE_BURST");
            CreateOrUpdatePrefab<LongTienYinYangAugmentRuntime>("PREFAB_AUG_PRIS_LONGTIEN_YIN_YANG", "AUG_PRIS_LONGTIEN_YIN_YANG");
            CreateOrUpdatePrefab<OmnipotentResetAugmentRuntime>("PREFAB_AUG_PRIS_OMNIPOTENT_RESET", "AUG_PRIS_OMNIPOTENT_RESET");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 3. Đồng bộ sang Resources
            Studio.UpgradeStudioAuditEngine.SyncDataToResources();

            EditorUtility.DisplayDialog(
                "Thành Công!",
                "Đã khởi tạo hoàn tất 12 Prefabs Cơ Chế cho Lõi Vàng & Kim Cương và liên kết trực tiếp vào Database SO!",
                "Tuyệt vời"
            );

            Debug.Log("<color=#00FF88>[AugmentPrefabsGenerator] Khởi tạo và liên kết 12 Prefabs Lõi Đột Biến thành công!</color>");
        }

        private static void CreateOrUpdatePrefab<T>(string prefabName, string targetAugmentId) where T : MonoBehaviour
        {
            string prefabPath = $"{PREFAB_DIR}/{prefabName}.prefab";
            GameObject prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefabObj == null)
            {
                var go = new GameObject(prefabName);
                go.AddComponent<T>();
                prefabObj = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                GameObject.DestroyImmediate(go);
            }
            else
            {
                // Đảm bảo component đã tồn tại
                if (prefabObj.GetComponent<T>() == null)
                {
                    var contents = PrefabUtility.LoadPrefabContents(prefabPath);
                    contents.AddComponent<T>();
                    PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }

            // Gán prefabObj vào MutationAugmentUpgradeData SO
            string assetPath = $"{AUGMENTS_DATA_DIR}/{targetAugmentId}.asset";
            var augmentAsset = AssetDatabase.LoadAssetAtPath<MutationAugmentUpgradeData>(assetPath);
            if (augmentAsset != null)
            {
                augmentAsset.mechanicRuntimePrefab = prefabObj;
                EditorUtility.SetDirty(augmentAsset);
            }
            else
            {
                Debug.LogWarning($"[AugmentPrefabsGenerator] Không tìm thấy asset SO tại {assetPath}");
            }
        }
    }
}
#endif
