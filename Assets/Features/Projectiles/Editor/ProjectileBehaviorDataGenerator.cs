#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Projectiles.Data;
using System.Collections.Generic;

namespace ProjectZombie.Features.Projectiles.Editor
{
    /// <summary>
    /// Editor Tool tự động tạo và gắn các Behavior Data ScriptableObjects (Straight, Pierce, Homing, Orbit, Explosion, Bounce, PeriodicHit)
    /// vào từng ProjectileData SO của 12 Pháp Bảo MVP theo đúng mô tả GDD v4.0.
    /// Menu: ProjectZombie > Projectiles > Attach Behaviors to 12 MVP ProjectileData SOs
    /// </summary>
    public static class ProjectileBehaviorDataGenerator
    {
        [MenuItem("ProjectZombie/Projectiles/Attach Behaviors to 12 MVP ProjectileData SOs")]
        public static void GenerateAndAttachBehaviors()
        {
            string behaviorFolder = "Assets/_Data/Projectiles/Behavior";
            string dataFolder = "Assets/_Data/Projectiles/Data";

            if (!AssetDatabase.IsValidFolder("Assets/_Data")) AssetDatabase.CreateFolder("Assets", "_Data");
            if (!AssetDatabase.IsValidFolder("Assets/_Data/Projectiles")) AssetDatabase.CreateFolder("Assets/_Data", "Projectiles");
            if (!AssetDatabase.IsValidFolder(behaviorFolder)) AssetDatabase.CreateFolder("Assets/_Data/Projectiles", "Behavior");

            // 1. W001: Nỏ Thần (Kim) -> Straight + Pierce (2 Targets)
            SetupProjectileBehaviors("Proj_W001", dataFolder, behaviorFolder, new List<ProjectileBehaviorData>
            {
                CreateOrGetStraightBehavior(behaviorFolder, "Proj_W001_Straight", 100),
                CreateOrGetPierceBehavior(behaviorFolder, "Proj_W001_Pierce", 2, 10)
            });

            // 2. W002: Bút Phán Quan (Kim) -> PeriodicHit (Melee Slash)
            SetupProjectileBehaviors("Proj_W002", dataFolder, behaviorFolder, new List<ProjectileBehaviorData>
            {
                CreateOrGetPeriodicHitBehavior(behaviorFolder, "Proj_W002_SlashHit", 0.1f, 10)
            });

            // 3. W003: Bùa Trấn Yêu (Mộc) -> Orbit + Pierce
            SetupProjectileBehaviors("Proj_W003", dataFolder, behaviorFolder, new List<ProjectileBehaviorData>
            {
                CreateOrGetOrbitBehavior(behaviorFolder, "Proj_W003_Orbit", 2.5f, 180f, 10),
                CreateOrGetPierceBehavior(behaviorFolder, "Proj_W003_Pierce", 5, 20)
            });

            // 4. W004: Cửu Vĩ Hồ Trảo (Hỏa) -> Homing + Straight
            SetupProjectileBehaviors("Proj_W004", dataFolder, behaviorFolder, new List<ProjectileBehaviorData>
            {
                CreateOrGetHomingBehavior(behaviorFolder, "Proj_W004_Homing", 10f, 360f, 10),
                CreateOrGetStraightBehavior(behaviorFolder, "Proj_W004_Straight", 100)
            });

            // 5. W005: Trống Đồng Đông Sơn (Thổ) -> Straight Wave
            SetupProjectileBehaviors("Proj_W005", dataFolder, behaviorFolder, new List<ProjectileBehaviorData>
            {
                CreateOrGetStraightBehavior(behaviorFolder, "Proj_W005_Straight", 100)
            });

            // 6. W006: Lựu Đạn Thần Sa (Hỏa) -> Straight + Explosion (3.5m)
            SetupProjectileBehaviors("Proj_W006", dataFolder, behaviorFolder, new List<ProjectileBehaviorData>
            {
                CreateOrGetStraightBehavior(behaviorFolder, "Proj_W006_Straight", 100),
                CreateOrGetExplosionBehavior(behaviorFolder, "Proj_W006_Explosion", 3.5f, 45f, 50)
            });

            // 7. W007: Cung Thạch Sanh (Kim) -> Straight + Pierce (8 Targets)
            SetupProjectileBehaviors("Proj_W007", dataFolder, behaviorFolder, new List<ProjectileBehaviorData>
            {
                CreateOrGetStraightBehavior(behaviorFolder, "Proj_W007_Straight", 100),
                CreateOrGetPierceBehavior(behaviorFolder, "Proj_W007_Pierce", 8, 10)
            });

            // 8. W008: Đao Cửu Vĩ (Hỏa) -> Straight + PeriodicHit
            SetupProjectileBehaviors("Proj_W008", dataFolder, behaviorFolder, new List<ProjectileBehaviorData>
            {
                CreateOrGetStraightBehavior(behaviorFolder, "Proj_W008_Straight", 100),
                CreateOrGetPeriodicHitBehavior(behaviorFolder, "Proj_W008_StreamHit", 0.25f, 10)
            });

            // 9. W009: Trượng Long Vương (Thủy) -> Straight + Bounce (Chain 6)
            SetupProjectileBehaviors("Proj_W009", dataFolder, behaviorFolder, new List<ProjectileBehaviorData>
            {
                CreateOrGetStraightBehavior(behaviorFolder, "Proj_W009_Straight", 100),
                CreateOrGetBounceBehavior(behaviorFolder, "Proj_W009_Bounce", 6, 8f, 20)
            });

            // 10. W010: Linh Phù Ma Da (Thủy) -> PeriodicHit Poison Area
            SetupProjectileBehaviors("Proj_W010", dataFolder, behaviorFolder, new List<ProjectileBehaviorData>
            {
                CreateOrGetPeriodicHitBehavior(behaviorFolder, "Proj_W010_PoisonHit", 0.5f, 10)
            });

            // 11. W011: Nước Thánh Chùa Hương (Thổ) -> PeriodicHit Holy Well
            SetupProjectileBehaviors("Proj_W011", dataFolder, behaviorFolder, new List<ProjectileBehaviorData>
            {
                CreateOrGetPeriodicHitBehavior(behaviorFolder, "Proj_W011_WellHit", 1.0f, 10)
            });

            // 12. W012: Phi Tiêu Bát Quái (Mộc) -> Dual Curved Crescent Boomerang + Pierce
            SetupProjectileBehaviors("Proj_W012", dataFolder, behaviorFolder, new List<ProjectileBehaviorData>
            {
                CreateOrGetCurvedBoomerangBehavior(behaviorFolder, "Proj_W012_CurvedBoomerang", 240f, 0.5f, 100),
                CreateOrGetPierceBehavior(behaviorFolder, "Proj_W012_Pierce", 4, 10)
            });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ProjectileBehaviorDataGenerator] Đã tạo & gán thành công toàn bộ Behavior SOs cho 12 ProjectileData SOs!");
        }

        private static void SetupProjectileBehaviors(string projId, string dataFolder, string behaviorFolder, List<ProjectileBehaviorData> behaviors)
        {
            string[] guids = AssetDatabase.FindAssets($"{projId}_ t:ProjectileData", new string[] { dataFolder });
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var projData = AssetDatabase.LoadAssetAtPath<ProjectileData>(path);
                if (projData != null)
                {
                    SerializedObject so = new SerializedObject(projData);
                    SerializedProperty behaviorsProp = so.FindProperty("Behaviors");
                    behaviorsProp.ClearArray();

                    for (int i = 0; i < behaviors.Count; i++)
                    {
                        behaviorsProp.InsertArrayElementAtIndex(i);
                        behaviorsProp.GetArrayElementAtIndex(i).objectReferenceValue = behaviors[i];
                    }

                    so.ApplyModifiedProperties();
                    Debug.Log($"[ProjectileBehaviorDataGenerator] Đã gán {behaviors.Count} Behaviors vào '{projData.name}'");
                }
            }
        }

        private static StraightBehaviorData CreateOrGetStraightBehavior(string folder, string name, int order)
        {
            string path = $"{folder}/{name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<StraightBehaviorData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<StraightBehaviorData>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.ExecutionOrder = order;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static PierceBehaviorData CreateOrGetPierceBehavior(string folder, string name, int count, int order)
        {
            string path = $"{folder}/{name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<PierceBehaviorData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<PierceBehaviorData>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.PierceCount = count;
            asset.ExecutionOrder = order;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static HomingBehaviorData CreateOrGetHomingBehavior(string folder, string name, float radius, float turnSpeed, int order)
        {
            string path = $"{folder}/{name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<HomingBehaviorData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<HomingBehaviorData>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.HomingRadius = radius;
            asset.HomingStrength = turnSpeed;
            asset.ExecutionOrder = order;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static OrbitBehaviorData CreateOrGetOrbitBehavior(string folder, string name, float radius, float speed, int order)
        {
            string path = $"{folder}/{name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<OrbitBehaviorData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<OrbitBehaviorData>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.radius = radius;
            asset.orbitSpeed = speed;
            asset.ExecutionOrder = order;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static ExplosionBehaviorData CreateOrGetExplosionBehavior(string folder, string name, float radius, float damage, int order)
        {
            string path = $"{folder}/{name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<ExplosionBehaviorData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ExplosionBehaviorData>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.ExplosionRadius = radius;
            asset.ExplosionDamageMultiplier = 1f;
            asset.ExecutionOrder = order;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static BounceBehaviorData CreateOrGetBounceBehavior(string folder, string name, int count, float radius, int order)
        {
            string path = $"{folder}/{name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<BounceBehaviorData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<BounceBehaviorData>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.BounceCount = count;
            asset.ExecutionOrder = order;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static PeriodicHitBehaviorData CreateOrGetPeriodicHitBehavior(string folder, string name, float interval, int order)
        {
            string path = $"{folder}/{name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<PeriodicHitBehaviorData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<PeriodicHitBehaviorData>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.hitCooldown = interval;
            asset.ExecutionOrder = order;
            EditorUtility.SetDirty(asset);
            return asset;
        }
        private static CurvedBoomerangBehaviorData CreateOrGetCurvedBoomerangBehavior(string folder, string name, float turnRate, float forwardDuration, int order)
        {
            string path = $"{folder}/{name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<CurvedBoomerangBehaviorData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<CurvedBoomerangBehaviorData>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.curveTurnRate = turnRate;
            asset.forwardDuration = forwardDuration;
            asset.spinSpeed = 1080f;
            asset.returnTurnRate = 420f;
            asset.returnSpeedMultiplier = 1.3f;
            asset.ExecutionOrder = order;
            EditorUtility.SetDirty(asset);
            return asset;
        }
    }
}
#endif
