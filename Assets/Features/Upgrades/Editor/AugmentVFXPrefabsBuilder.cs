#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Upgrades.Runtimes.Augments;

namespace ProjectZombie.Features.Upgrades.Editor
{
    /// <summary>
    /// Generator tự động tạo 12 Prefabs Particle Systems VFX chuẩn URP cho Lõi Đột Biến
    /// và gán vào các trường [SerializeField] tương ứng của 12 Augment Runtimes.
    /// </summary>
    public static class AugmentVFXPrefabsBuilder
    {
        private const string VFX_PREFAB_DIR = "Assets/_Prefabs/VFX/Augments";
        private const string AUGMENT_PREFAB_DIR = "Assets/_Data/Upgrades/Prefabs/Augments";

        [MenuItem("ProjectZombie/Upgrades/✨ Sinh Toàn Bộ 12 VFX Particles Cho Lõi Đột Biến", false, 31)]
        public static void GenerateAllAugmentVFX()
        {
            if (!Directory.Exists(VFX_PREFAB_DIR))
            {
                Directory.CreateDirectory(VFX_PREFAB_DIR);
                AssetDatabase.Refresh();
            }

            // 1. Tạo 12 VFX Particle Prefabs
            var fireVfx = CreateParticleVFX("VFX_Augment_FireTrail", new Color(1f, 0.35f, 0.1f), 1.5f, 25);
            var lightningVfx = CreateParticleVFX("VFX_Augment_ChainLightning", new Color(1f, 0.9f, 0.2f), 0.3f, 40);
            var critVfx = CreateParticleVFX("VFX_Augment_CritExplode", new Color(1f, 0.4f, 0.1f), 0.5f, 50);
            var executeVfx = CreateParticleVFX("VFX_Augment_ExecuteSlash", new Color(0.9f, 0.1f, 0.1f), 0.4f, 30);
            var shieldVfx = CreateParticleVFX("VFX_Augment_ShieldBashWave", new Color(1f, 0.85f, 0.3f), 0.6f, 35);
            var vampAuraVfx = CreateParticleVFX("VFX_Augment_VampiricAura", new Color(0.85f, 0.1f, 0.2f), 3.0f, 20);

            var quakeVfx = CreateParticleVFX("VFX_Augment_PhuDongQuake", new Color(0.85f, 0.6f, 0.25f), 0.5f, 45);
            var arrowVfx = CreateParticleVFX("VFX_Augment_KimQuyArrowVolley", new Color(1f, 0.95f, 0.4f), 0.4f, 30);
            var pillarVfx = CreateParticleVFX("VFX_Augment_TanVienPillarBurst", new Color(0.9f, 0.75f, 0.3f), 1.2f, 50);
            var freezeVfx = CreateParticleVFX("VFX_Augment_FreezeNova", new Color(0.3f, 0.85f, 1f), 1.0f, 50);
            var yangAuraVfx = CreateParticleVFX("VFX_Augment_YangAura", new Color(1f, 0.5f, 0.1f), 8.0f, 25);
            var yinAuraVfx = CreateParticleVFX("VFX_Augment_YinAura", new Color(0.2f, 0.9f, 1f), 8.0f, 25);
            var resetVfx = CreateParticleVFX("VFX_Augment_DashResetBurst", new Color(0.2f, 1f, 0.7f), 0.6f, 35);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 2. Gán VFX Prefabs vào 12 Runtime Prefabs
            AssignVfxField<FireTrailAugmentRuntime>("PREFAB_AUG_GOLD_FIRE_TRAIL", "_fireVfxPrefab", fireVfx);
            AssignVfxField<ChainLightningAugmentRuntime>("PREFAB_AUG_GOLD_CHAIN_LIGHTNING", "_lightningVfxPrefab", lightningVfx);
            AssignVfxField<CritExplodeAugmentRuntime>("PREFAB_AUG_GOLD_CRIT_EXPLODE", "_critVfxPrefab", critVfx);
            AssignVfxField<ExecuteAugmentRuntime>("PREFAB_AUG_GOLD_EXECUTE", "_executeVfxPrefab", executeVfx);
            AssignVfxField<ShieldBashAugmentRuntime>("PREFAB_AUG_GOLD_SHIELD_BASH", "_shieldVfxPrefab", shieldVfx);
            AssignVfxField<VampiricFrenzyAugmentRuntime>("PREFAB_AUG_GOLD_VAMPIRIC_FRENZY", "_vampiricAuraVfxPrefab", vampAuraVfx);

            AssignVfxField<PhuDongGigantismAugmentRuntime>("PREFAB_AUG_PRIS_PHUDONG_GIGANTISM", "_quakeVfxPrefab", quakeVfx);
            AssignVfxField<KimQuyMulticastAugmentRuntime>("PREFAB_AUG_PRIS_KIMQUY_MULTICAST", "_arrowVfxPrefab", arrowVfx);
            AssignVfxField<TanVienImmortalAugmentRuntime>("PREFAB_AUG_PRIS_TANVIEN_IMMORTAL", "_pillarBurstVfxPrefab", pillarVfx);
            AssignVfxField<ThuyBaFreezeBurstAugmentRuntime>("PREFAB_AUG_PRIS_THUYBA_FREEZE_BURST", "_freezeBurstVfxPrefab", freezeVfx);
            AssignVfxField<LongTienYinYangAugmentRuntime>("PREFAB_AUG_PRIS_LONGTIEN_YIN_YANG", "_yangAuraVfxPrefab", yangAuraVfx, "_yinAuraVfxPrefab", yinAuraVfx);
            AssignVfxField<OmnipotentResetAugmentRuntime>("PREFAB_AUG_PRIS_OMNIPOTENT_RESET", "_dashResetVfxPrefab", resetVfx);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 3. Đồng bộ sang Resources
            Studio.UpgradeStudioAuditEngine.SyncDataToResources();

            EditorUtility.DisplayDialog("VFX Sinh Thành Công!", "Đã khởi tạo hoàn tất 12 bộ hạt VFX Particles và liên kết vào hệ thống Object Pooling!", "OK");
            Debug.Log("<color=#00FF88>[AugmentVFXPrefabsBuilder] Đã sinh và gán 12 bộ VFX Particles vào Augment Runtimes thành công!</color>");
        }

        private static GameObject CreateParticleVFX(string prefabName, Color color, float duration, float emissionRate)
        {
            string path = $"{VFX_PREFAB_DIR}/{prefabName}.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject(prefabName);
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = duration;
            main.startLifetime = Mathf.Min(duration, 0.8f);
            main.startSpeed = 3f;
            main.startSize = 0.5f;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.None;
            main.playOnAwake = true;

            var emission = ps.emission;
            emission.rateOverTime = emissionRate;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1.0f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(color, 0.0f), new GradientColorKey(Color.white, 0.5f), new GradientColorKey(color, 1.0f) },
                new[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.8f, 0.6f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLifetime.color = grad;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.sortingLayerName = "Skill";
                Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");
                if (particleShader != null)
                {
                    Material mat = new Material(particleShader);
                    Sprite knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                    Texture defaultTex = knobSprite != null ? knobSprite.texture : Texture2D.whiteTexture;
                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", defaultTex);
                    if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", defaultTex);
                    renderer.material = mat;
                }
            }

            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(go, path);
            GameObject.DestroyImmediate(go);
            return prefabAsset;
        }

        private static void AssignVfxField<T>(string prefabName, string fieldName, GameObject vfxPrefab, string fieldName2 = null, GameObject vfxPrefab2 = null) where T : MonoBehaviour
        {
            string prefabPath = $"{AUGMENT_PREFAB_DIR}/{prefabName}.prefab";
            if (!File.Exists(prefabPath)) return;

            var contents = PrefabUtility.LoadPrefabContents(prefabPath);
            var comp = contents.GetComponent<T>();
            if (comp != null)
            {
                var serializedObj = new SerializedObject(comp);
                var prop = serializedObj.FindProperty(fieldName);
                if (prop != null)
                {
                    prop.objectReferenceValue = vfxPrefab;
                }

                if (!string.IsNullOrEmpty(fieldName2) && vfxPrefab2 != null)
                {
                    var prop2 = serializedObj.FindProperty(fieldName2);
                    if (prop2 != null)
                    {
                        prop2.objectReferenceValue = vfxPrefab2;
                    }
                }

                serializedObj.ApplyModifiedPropertiesWithoutUndo();
            }
            PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }
}
#endif
