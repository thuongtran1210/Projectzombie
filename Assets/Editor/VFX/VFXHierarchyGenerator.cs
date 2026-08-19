using UnityEngine;
using UnityEditor;
using ProjectZombie.VFX;

namespace ProjectZombie.Editor.VFX
{
    /// <summary>
    /// Unity Editor Tool tự động tạo cấu trúc Hierarchy chuẩn cho:
    /// 1. Signature Skill VFX (Kỹ năng diện rộng trọn gói của Nhân vật)
    /// 2. Weapon Attack VFX (Hiệu ứng đòn đánh vũ khí cận chiến)
    /// 3. Bullet Projectile VFX (Đầu đạn & Vệt đạn bay - ĐỘC LẬP)
    /// 4. Hit Impact VFX (Hiệu ứng nổ va chạm quái - ĐỘC LẬP)
    /// Dành cho game Survival Roguelite (Top-down 2D URP).
    /// </summary>
    public class VFXHierarchyGenerator : EditorWindow
    {
        public enum VFXCategory
        {
            SignatureSkill,  // Kỹ năng chủ động nhân vật (Bát Quái Trận Đồ, Phá Giới Chấn Thế, ...)
            WeaponAttack,    // Hiệu ứng đòn đánh vũ khí cận chiến (Bút Phán Quan, Bùa Trấn Yêu, ...)
            BulletProjectile,// Đạn bay tầm xa (Nỏ Thần Core, Fireball Core - KHÔNG chứa Hit Impact)
            HitImpact        // Nổ va chạm trên thân quái (ĐỘC LẬP - Dùng riêng cho Object Pool va chạm)
        }

        private VFXCategory _category = VFXCategory.WeaponAttack;
        private string _targetName = "Weapon_DualSlash";

        // Signature Skill Toggles
        private bool _incSkillGroundDecal = true;
        private bool _incSkillMainEffect = true;
        private bool _incSkillAuraSwirl = true;

        // Weapon Attack Layer Toggles
        private bool _incSwingMuzzle = true;
        private bool _incSlashArc = true;
        private bool _incSlashGlow = true;

        // Projectile Bullet Layer Toggles
        private bool _incBulletCore = true;
        private bool _incBulletTrail = true;
        private bool _incMuzzleFlash = false;

        // Hit Impact Layer Toggles
        private bool _incImpactBurst = true;
        private bool _incImpactSparks = true;
        private bool _incImpactSmoke = true;

        // Shared
        private bool _incSparks = true;
        private bool _attachPoolResetter = true;

        // Sorting Layers
        private string _frontSortingLayer = "Skill";
        private string _backSortingLayer = "Tilemap_Decals";

        [MenuItem("Tools/VFX Generator/Create Modular VFX Hierarchy", false, 10)]
        [MenuItem("GameObject/2D Object/VFX/Modular VFX Generator", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<VFXHierarchyGenerator>("Modular VFX Generator");
            window.minSize = new Vector2(420, 560);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🎯 Modular VFX Hierarchy Generator (Survival Roguelite 2D)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Tạo bộ khung VFX phân bóc module riêng biệt cho Kỹ năng, Vũ khí, Đạn bay và Nổ va chạm.", MessageType.Info);
            EditorGUILayout.Space(5);

            _category = (VFXCategory)EditorGUILayout.EnumPopup("Loại Hiệu Ứng (Category):", _category);
            _targetName = EditorGUILayout.TextField("Tên Hiệu Ứng (Name):", _targetName);
            _attachPoolResetter = EditorGUILayout.ToggleLeft(" 🛠️ Gắn VFXPoolResetter (Reset particle khi về Pool)", _attachPoolResetter);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("📌 Chọn Các Layer Thành Phần (VFX Layer Breakdown):", EditorStyles.boldLabel);

            switch (_category)
            {
                case VFXCategory.SignatureSkill:
                    _incSkillGroundDecal = EditorGUILayout.ToggleLeft(" 1. Ground_Decal (Họa tiết mặt đất/Bát Quái - Order: 1)", _incSkillGroundDecal);
                    _incSkillMainEffect = EditorGUILayout.ToggleLeft(" 2. Skill_Main (VFX Kỹ năng chính - Order: 10)", _incSkillMainEffect);
                    _incSkillAuraSwirl = EditorGUILayout.ToggleLeft(" 3. Aura_Swirl (Hào quang Âm Dương/Hắc Khí - Order: 5)", _incSkillAuraSwirl);
                    _incSparks = EditorGUILayout.ToggleLeft(" 4. Sparks_Burst (Tia sáng bùng nổ - Order: 12)", _incSparks);
                    break;

                case VFXCategory.WeaponAttack:
                    _incSwingMuzzle = EditorGUILayout.ToggleLeft(" 1. Muzzle_Swing (Chớp vung đòn - Order: 5)", _incSwingMuzzle);
                    _incSlashArc = EditorGUILayout.ToggleLeft(" 2. Slash_Arc (Vệt chém/Vệt vung chính - Order: 10)", _incSlashArc);
                    _incSlashGlow = EditorGUILayout.ToggleLeft(" 3. Slash_Glow (Hào quang Additive - Order: 8)", _incSlashGlow);
                    _incSparks = EditorGUILayout.ToggleLeft(" 4. Sparks_Burst (Tia lửa văng - Order: 12)", _incSparks);
                    break;

                case VFXCategory.BulletProjectile:
                    _incBulletCore = EditorGUILayout.ToggleLeft(" 1. Bullet_Core (Đầu đạn/Lõi đạn - Order: 10)", _incBulletCore);
                    _incBulletTrail = EditorGUILayout.ToggleLeft(" 2. Bullet_Trail (Đuôi đạn/Vệt bay - Order: 8)", _incBulletTrail);
                    _incMuzzleFlash = EditorGUILayout.ToggleLeft(" 3. Muzzle_Flash (Chớp nòng súng ngắn - Order: 5)", _incMuzzleFlash);
                    break;

                case VFXCategory.HitImpact:
                    _incImpactBurst = EditorGUILayout.ToggleLeft(" 1. Impact_Burst (Chớp nổ va chạm - Order: 15)", _incImpactBurst);
                    _incImpactSparks = EditorGUILayout.ToggleLeft(" 2. Impact_Sparks (Tia lửa nổ văng - Order: 12)", _incImpactSparks);
                    _incImpactSmoke = EditorGUILayout.ToggleLeft(" 3. Impact_Smoke (Khói/Bụi tàn dư - Order: 2)", _incImpactSmoke);
                    break;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("⚙️ Sorting Layer:", EditorStyles.boldLabel);
            _frontSortingLayer = EditorGUILayout.TextField("Front Sorting Layer:", _frontSortingLayer);
            _backSortingLayer = EditorGUILayout.TextField("Back Sorting Layer:", _backSortingLayer);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("⚡ Nút Preset Nhanh Theo Module:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("☯️ Signature Skill", GUILayout.Height(25))) PresetSignatureSkill();
            if (GUILayout.Button("🗡️ Weapon Slash", GUILayout.Height(25))) PresetWeaponSlash();
            if (GUILayout.Button("🔫 Bullet Core", GUILayout.Height(25))) PresetBulletCore();
            if (GUILayout.Button("💥 Hit Impact", GUILayout.Height(25))) PresetHitImpact();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(15);
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
            if (GUILayout.Button("🚀 TẠO HIERARCHY VFX MODULAR", GUILayout.Height(40)))
            {
                GenerateVFXHierarchy();
            }
            GUI.backgroundColor = Color.white;
        }

        private void PresetSignatureSkill()
        {
            _category = VFXCategory.SignatureSkill;
            _targetName = "Skill_BatQuaiTranDo";
            _incSkillGroundDecal = true;
            _incSkillMainEffect = true;
            _incSkillAuraSwirl = true;
            _incSparks = true;
        }

        private void PresetWeaponSlash()
        {
            _category = VFXCategory.WeaponAttack;
            _targetName = "Weapon_ButPhanQuan";
            _incSwingMuzzle = true;
            _incSlashArc = true;
            _incSlashGlow = true;
            _incSparks = true;
        }

        private void PresetBulletCore()
        {
            _category = VFXCategory.BulletProjectile;
            _targetName = "Projectile_NoThan_Core";
            _incBulletCore = true;
            _incBulletTrail = true;
            _incMuzzleFlash = false;
        }

        private void PresetHitImpact()
        {
            _category = VFXCategory.HitImpact;
            _targetName = "Impact_NoThan_Burst";
            _incImpactBurst = true;
            _incImpactSparks = true;
            _incImpactSmoke = true;
        }

        private void GenerateVFXHierarchy()
        {
            if (string.IsNullOrEmpty(_targetName))
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng nhập tên hiệu ứng!", "OK");
                return;
            }

            GameObject rootGo = new GameObject(_targetName);
            Undo.RegisterCreatedObjectUndo(rootGo, $"Create VFX Hierarchy {_targetName}");

            Transform parentTransform = Selection.activeTransform;
            if (parentTransform != null)
            {
                rootGo.transform.SetParent(parentTransform, false);
            }

            if (_attachPoolResetter)
            {
                rootGo.AddComponent<VFXPoolResetter>();
            }

            GameObject visualRoot = new GameObject("Visual_Root");
            visualRoot.transform.SetParent(rootGo.transform, false);

            VFXMaterialGenerator.GenerateAllVFXMaterials();

            switch (_category)
            {
                case VFXCategory.SignatureSkill:
                    if (_incSkillGroundDecal) CreateParticleChild(visualRoot, "PS_GroundDecal", _backSortingLayer, 1, 4.0f, 4.0f, 0f, 1);
                    if (_incSkillAuraSwirl) CreateParticleChild(visualRoot, "PS_AuraSwirl", _frontSortingLayer, 5, 4.0f, 2.0f, 1f, 10);
                    if (_incSkillMainEffect) CreateParticleChild(visualRoot, "PS_SkillMain", _frontSortingLayer, 10, 4.0f, 3.5f, 0f, 1);
                    if (_incSparks) CreateParticleChild(visualRoot, "PS_SparksBurst", _frontSortingLayer, 12, 0.5f, 0.3f, 10f, 20);
                    break;

                case VFXCategory.WeaponAttack:
                    if (_incSwingMuzzle) CreateParticleChild(visualRoot, "PS_MuzzleSwing", _frontSortingLayer, 5, 0.1f, 0.05f, 0f, 1);
                    if (_incSlashArc) CreateParticleChild(visualRoot, "PS_SlashArc", _frontSortingLayer, 10, 0.2f, 0.12f, 0f, 1);
                    if (_incSlashGlow) CreateParticleChild(visualRoot, "PS_SlashGlow", _frontSortingLayer, 8, 0.25f, 0.15f, 0f, 1);
                    if (_incSparks) CreateParticleChild(visualRoot, "PS_SparksBurst", _frontSortingLayer, 12, 0.3f, 0.25f, 15f, 15);
                    break;

                case VFXCategory.BulletProjectile:
                    if (_incMuzzleFlash) CreateParticleChild(visualRoot, "PS_MuzzleFlash", _frontSortingLayer, 5, 0.1f, 0.05f, 0f, 1);
                    if (_incBulletCore) CreateParticleChild(visualRoot, "PS_BulletCore", _frontSortingLayer, 10, 1.0f, 1.0f, 0f, 1);
                    if (_incBulletTrail) CreateParticleChild(visualRoot, "PS_BulletTrail", _frontSortingLayer, 8, 1.0f, 0.3f, 0f, 10);
                    break;

                case VFXCategory.HitImpact:
                    if (_incImpactBurst) CreateParticleChild(visualRoot, "PS_ImpactBurst", _frontSortingLayer, 15, 0.25f, 0.15f, 0f, 1);
                    if (_incImpactSparks) CreateParticleChild(visualRoot, "PS_ImpactSparks", _frontSortingLayer, 12, 0.3f, 0.2f, 10f, 15);
                    if (_incImpactSmoke) CreateParticleChild(visualRoot, "PS_ImpactSmoke", _backSortingLayer, 2, 0.45f, 0.4f, 3f, 5);
                    break;
            }

            // Refetch cached components inside VFXPoolResetter
            var resetter = rootGo.GetComponent<VFXPoolResetter>();
            if (resetter != null)
            {
                resetter.CacheComponents();
            }

            Selection.activeGameObject = rootGo;
            Debug.Log($"<color=#00FF00>[VFX Generator]</color> Đã tạo thành công Hierarchy Modular VFX cho <b>{_targetName}</b>!");
        }

        private GameObject CreateParticleChild(GameObject root, string childName, string sortingLayer, int sortingOrder, float duration, float lifetime, float speed, short burstCount)
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(root.transform, false);

            ParticleSystem ps = child.AddComponent<ParticleSystem>();
            ParticleSystemRenderer psRenderer = child.GetComponent<ParticleSystemRenderer>();

            var main = ps.main;
            main.duration = duration;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.loop = false;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            if (burstCount > 0)
            {
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, burstCount) });
            }

            if (!string.IsNullOrEmpty(sortingLayer))
            {
                psRenderer.sortingLayerName = sortingLayer;
            }
            psRenderer.sortingOrder = sortingOrder;

            Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/VFX/SkillLibrary/Materials/MAT_Additive_Default.mat");
            if (mat != null)
            {
                psRenderer.sharedMaterial = mat;
            }

            return child;
        }
    }
}
