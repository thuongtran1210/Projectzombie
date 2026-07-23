using UnityEngine;
using UnityEditor;

namespace ProjectZombie.Editor.VFX
{
    /// <summary>
    /// Unity Editor Tool tự động tạo cấu trúc Hierarchy chuẩn cho:
    /// 1. Weapon Attack VFX (Vệt chém, Vùng xoay vũ khí cận chiến)
    /// 2. Bullet Projectile VFX (Đầu đạn, Trail đuôi đạn, Hit Impact va chạm)
    /// Dành cho game Survival Roguelite (Top-down 2D URP).
    /// </summary>
    public class VFXHierarchyGenerator : EditorWindow
    {
        public enum VFXCategory
        {
            WeaponAttack,   // Hiệu ứng đòn đánh vũ khí (Dual Slash, Orbit Saw)
            BulletProjectile // Hiệu ứng đạn bay & va chạm (Pistol, Fireball, Energy Bullet)
        }

        private VFXCategory _category = VFXCategory.WeaponAttack;
        private string _targetName = "Weapon_DualSlash";

        // Weapon Attack Layer Toggles
        private bool _incSlashArc = true;
        private bool _incSlashGlow = true;
        private bool _incSwingMuzzle = true;
        private bool _incSparks = true;
        private bool _incHitImpact = true;
        private bool _incSmoke = true;

        // Projectile Bullet Layer Toggles
        private bool _incBulletCore = true;
        private bool _incBulletTrail = true;
        private bool _incMuzzleFlash = true;

        // Sorting Layers
        private string _frontSortingLayer = "VFX_Front";
        private string _backSortingLayer = "VFX_Back";

        [MenuItem("Tools/VFX Generator/Create Weapon & Projectile VFX Hierarchy", false, 10)]
        [MenuItem("GameObject/2D Object/VFX/Weapon & Projectile VFX Generator", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<VFXHierarchyGenerator>("Weapon & Projectile VFX Generator");
            window.minSize = new Vector2(400, 520);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🎯 Weapon & Projectile VFX Generator (Survival Roguelite 2D)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Tạo bộ khung VFX chuẩn cho Vũ Khí (Weapon Slashes/Orbit) và Đạn (Bullet Projectile/Impact).", MessageType.Info);
            EditorGUILayout.Space(5);

            _category = (VFXCategory)EditorGUILayout.EnumPopup("Loại Hiệu Ứng (Category):", _category);
            _targetName = EditorGUILayout.TextField("Tên Vũ Khí / Đạn (Name):", _targetName);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("📌 Chọn Các Layer Thành Phần (VFX Layer Breakdown):", EditorStyles.boldLabel);

            if (_category == VFXCategory.WeaponAttack)
            {
                _incSwingMuzzle = EditorGUILayout.ToggleLeft(" 1. Muzzle_Swing (Chớp vung đòn - Order: 5)", _incSwingMuzzle);
                _incSlashArc = EditorGUILayout.ToggleLeft(" 2. Slash_Arc (Vệt chém/Vệt vung chính - Order: 10)", _incSlashArc);
                _incSlashGlow = EditorGUILayout.ToggleLeft(" 3. Slash_Glow (Hào quang Additive - Order: 8)", _incSlashGlow);
                _incSparks = EditorGUILayout.ToggleLeft(" 4. Sparks_Burst (Tia lửa văng - Order: 12)", _incSparks);
                _incHitImpact = EditorGUILayout.ToggleLeft(" 5. Hit_Impact (Chớp va chạm quái - Order: 15)", _incHitImpact);
                _incSmoke = EditorGUILayout.ToggleLeft(" 6. Smoke_Residual (Khói/Bụi tàn dư - Order: 2)", _incSmoke);
            }
            else
            {
                _incMuzzleFlash = EditorGUILayout.ToggleLeft(" 1. Muzzle_Flash (Chớp bắn nòng súng - Order: 5)", _incMuzzleFlash);
                _incBulletCore = EditorGUILayout.ToggleLeft(" 2. Bullet_Core (Đầu đạn/Lõi đạn - Order: 10)", _incBulletCore);
                _incBulletTrail = EditorGUILayout.ToggleLeft(" 3. Bullet_Trail (Đuôi đạn/Vệt bay - Order: 8)", _incBulletTrail);
                _incSparks = EditorGUILayout.ToggleLeft(" 4. Sparks_Burst (Tia lửa đạn - Order: 12)", _incSparks);
                _incHitImpact = EditorGUILayout.ToggleLeft(" 5. Hit_Impact (Nổ va chạm quái - Order: 15)", _incHitImpact);
                _incSmoke = EditorGUILayout.ToggleLeft(" 6. Smoke_Residual (Vệt khói đạn - Order: 2)", _incSmoke);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("⚙️ Sorting Layer:", EditorStyles.boldLabel);
            _frontSortingLayer = EditorGUILayout.TextField("Front Sorting Layer:", _frontSortingLayer);
            _backSortingLayer = EditorGUILayout.TextField("Back Sorting Layer:", _backSortingLayer);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("⚡ Nút Preset Nhanh Cho Vũ Khí & Đạn:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🗡️ Dual Slash", GUILayout.Height(25))) PresetDualSlash();
            if (GUILayout.Button("🪚 Orbit Saw", GUILayout.Height(25))) PresetOrbitSaw();
            if (GUILayout.Button("🔫 Bullet Projectile", GUILayout.Height(25))) PresetBulletProjectile();
            if (GUILayout.Button("🔥 Fireball Bullet", GUILayout.Height(25))) PresetFireballProjectile();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(15);
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
            if (GUILayout.Button("🚀 TẠO HIERARCHY VFX VŨ KHÍ / ĐẠN", GUILayout.Height(40)))
            {
                GenerateVFXHierarchy();
            }
            GUI.backgroundColor = Color.white;
        }

        private void PresetDualSlash()
        {
            _category = VFXCategory.WeaponAttack;
            _targetName = "Weapon_DualSlash";
            _incSwingMuzzle = true;
            _incSlashArc = true;
            _incSlashGlow = true;
            _incSparks = true;
            _incHitImpact = true;
            _incSmoke = true;
        }

        private void PresetOrbitSaw()
        {
            _category = VFXCategory.WeaponAttack;
            _targetName = "Weapon_OrbitSaw";
            _incSwingMuzzle = false;
            _incSlashArc = true;
            _incSlashGlow = true;
            _incSparks = true;
            _incHitImpact = true;
            _incSmoke = false;
        }

        private void PresetBulletProjectile()
        {
            _category = VFXCategory.BulletProjectile;
            _targetName = "Projectile_PistolBullet";
            _incMuzzleFlash = true;
            _incBulletCore = true;
            _incBulletTrail = true;
            _incSparks = true;
            _incHitImpact = true;
            _incSmoke = false;
        }

        private void PresetFireballProjectile()
        {
            _category = VFXCategory.BulletProjectile;
            _targetName = "Projectile_Fireball";
            _incMuzzleFlash = true;
            _incBulletCore = true;
            _incBulletTrail = true;
            _incSparks = true;
            _incHitImpact = true;
            _incSmoke = true;
        }

        private void GenerateVFXHierarchy()
        {
            if (string.IsNullOrEmpty(_targetName))
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng nhập tên vũ khí hoặc đạn!", "OK");
                return;
            }

            GameObject rootGo = new GameObject(_targetName);
            Undo.RegisterCreatedObjectUndo(rootGo, $"Create VFX Hierarchy {_targetName}");

            Transform parentTransform = Selection.activeTransform;
            if (parentTransform != null)
            {
                rootGo.transform.SetParent(parentTransform, false);
            }

            VFXMaterialGenerator.GenerateAllVFXMaterials();

            if (_category == VFXCategory.WeaponAttack)
            {
                if (_incSwingMuzzle) CreateParticleChild(rootGo, "Muzzle_Swing", _frontSortingLayer, 5, 0.1f, 0.05f, 0f, 1);
                if (_incSlashArc) CreateParticleChild(rootGo, "Slash_Arc", _frontSortingLayer, 10, 0.2f, 0.12f, 0f, 1);
                if (_incSlashGlow) CreateParticleChild(rootGo, "Slash_Glow", _frontSortingLayer, 8, 0.25f, 0.15f, 0f, 1);
                if (_incSparks) CreateParticleChild(rootGo, "Sparks_Burst", _frontSortingLayer, 12, 0.3f, 0.25f, 15f, 15);
                if (_incHitImpact) CreateParticleChild(rootGo, "Hit_Impact", _frontSortingLayer, 15, 0.25f, 0.15f, 0f, 1);
                if (_incSmoke) CreateParticleChild(rootGo, "Smoke_Residual", _backSortingLayer, 2, 0.45f, 0.4f, 5f, 5);
            }
            else
            {
                if (_incMuzzleFlash) CreateParticleChild(rootGo, "Muzzle_Flash", _frontSortingLayer, 5, 0.1f, 0.05f, 0f, 1);
                if (_incBulletCore) CreateParticleChild(rootGo, "Bullet_Core", _frontSortingLayer, 10, 1.0f, 1.0f, 0f, 1);
                if (_incBulletTrail) CreateParticleChild(rootGo, "Bullet_Trail", _frontSortingLayer, 8, 1.0f, 0.3f, 0f, 10);
                if (_incSparks) CreateParticleChild(rootGo, "Sparks_Burst", _frontSortingLayer, 12, 0.3f, 0.2f, 10f, 10);
                if (_incHitImpact) CreateParticleChild(rootGo, "Hit_Impact", _frontSortingLayer, 15, 0.25f, 0.15f, 0f, 1);
                if (_incSmoke) CreateParticleChild(rootGo, "Smoke_Residual", _backSortingLayer, 2, 0.4f, 0.3f, 3f, 5);
            }

            Selection.activeGameObject = rootGo;
            Debug.Log($"<color=#00FF00>[VFX Generator]</color> Đã tạo thành công Hierarchy VFX cho <b>{_targetName}</b>!");
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
