using UnityEngine;
using UnityEditor;

namespace ProjectZombie.Editor.VFX
{
    /// <summary>
    /// Unity Editor Tool cho phép tự động tạo cấu trúc Hierarchy chuẩn cho 2D VFX Skill (Anime Top-down URP).
    /// Tuân thủ các quy chuẩn trong unity-hierarchy-generator SKILL.md.
    /// </summary>
    public class VFXHierarchyGenerator : EditorWindow
    {
        private string _skillName = "FireSlash";

        // Tùy chọn các layer cần sinh
        private bool _includeAnticipation = true;
        private bool _includeSlashArc = true;
        private bool _includeSlashGlow = true;
        private bool _includeSparks = true;
        private bool _includeImpact = true;
        private bool _includeSmoke = true;
        private bool _includeDistortion = true;

        // Cấu hình Sorting Layer & Order in Layer
        private string _frontSortingLayer = "VFX_Front";
        private string _backSortingLayer = "VFX_Back";

        [MenuItem("Tools/VFX Skill Generator/Create VFX Hierarchy", false, 10)]
        [MenuItem("GameObject/2D Object/VFX/VFX Skill Hierarchy Generator", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<VFXHierarchyGenerator>("VFX Hierarchy Generator");
            window.minSize = new Vector2(380, 480);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🎨 VFX Skill Hierarchy Generator (2D Anime URP)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Tự động tạo bộ khung GameObject & Particle System tối ưu 60 FPS cho đòn đánh/kỹ năng.", MessageType.Info);
            EditorGUILayout.Space(5);

            _skillName = EditorGUILayout.TextField("Tên Kỹ Năng / Prefab:", _skillName);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("📌 Chọn Các Layer Thành Phần (VFX Layer Breakdown):", EditorStyles.boldLabel);
            _includeAnticipation = EditorGUILayout.ToggleLeft(" 1. Flash_Anticipation (Nạp đòn - Order: 5)", _includeAnticipation);
            _includeSlashArc = EditorGUILayout.ToggleLeft(" 2. Slash_Arc (Vệt chém chính - Order: 10)", _includeSlashArc);
            _includeSlashGlow = EditorGUILayout.ToggleLeft(" 3. Slash_Glow (Hào quang Additive - Order: 8)", _includeSlashGlow);
            _includeSparks = EditorGUILayout.ToggleLeft(" 4. Sparks_Burst (Tia lửa văng - Order: 12)", _includeSparks);
            _includeImpact = EditorGUILayout.ToggleLeft(" 5. Hit_Impact (Tác động va chạm - Order: 15)", _includeImpact);
            _includeSmoke = EditorGUILayout.ToggleLeft(" 6. Smoke_Residual (Khói tàn dư - Order: 2)", _includeSmoke);
            _includeDistortion = EditorGUILayout.ToggleLeft(" 7. Distortion_Heat (Sóng nhiệt méo URP - Order: 20)", _includeDistortion);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("⚙️ Cấu Hình Sorting Layer:", EditorStyles.boldLabel);
            _frontSortingLayer = EditorGUILayout.TextField("Front Sorting Layer:", _frontSortingLayer);
            _backSortingLayer = EditorGUILayout.TextField("Back Sorting Layer:", _backSortingLayer);

            EditorGUILayout.Space(15);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Chọn Tất Cả", GUILayout.Height(25))) SetAllLayers(true);
            if (GUILayout.Button("Bỏ Chọn Hết", GUILayout.Height(25))) SetAllLayers(false);
            if (GUILayout.Button("Preset FireSlash", GUILayout.Height(25))) ApplyFireSlashPreset();
            if (GUILayout.Button("Preset IceBlade", GUILayout.Height(25))) ApplyIceBladePreset();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(15);
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
            if (GUILayout.Button("🚀 TẠO HIERARCHY VFX", GUILayout.Height(40)))
            {
                GenerateVFXHierarchy();
            }
            GUI.backgroundColor = Color.white;
        }

        private void SetAllLayers(bool state)
        {
            _includeAnticipation = state;
            _includeSlashArc = state;
            _includeSlashGlow = state;
            _includeSparks = state;
            _includeImpact = state;
            _includeSmoke = state;
            _includeDistortion = state;
        }

        private void ApplyFireSlashPreset()
        {
            _skillName = "FireSlash";
            _includeAnticipation = true;
            _includeSlashArc = true;
            _includeSlashGlow = true;
            _includeSparks = true;
            _includeImpact = true;
            _includeSmoke = true;
            _includeDistortion = false;
        }

        private void ApplyIceBladePreset()
        {
            _skillName = "IceBlade";
            _includeAnticipation = true;
            _includeSlashArc = true;
            _includeSlashGlow = true;
            _includeSparks = true;
            _includeImpact = true;
            _includeSmoke = true;
            _includeDistortion = true;
        }

        private void GenerateVFXHierarchy()
        {
            if (string.IsNullOrEmpty(_skillName))
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng nhập tên kỹ năng!", "OK");
                return;
            }

            // Tạo Root GameObject
            GameObject rootGo = new GameObject(_skillName);
            Undo.RegisterCreatedObjectUndo(rootGo, $"Create VFX Hierarchy {_skillName}");

            Transform parentTransform = Selection.activeTransform;
            if (parentTransform != null)
            {
                rootGo.transform.SetParent(parentTransform, false);
            }

            // 1. Anticipation
            if (_includeAnticipation)
            {
                CreateParticleChild(rootGo, "Flash_Anticipation", _frontSortingLayer, 5, 0.1f, 0.05f, 0f, 1);
            }

            // 2. Slash Arc (Chính)
            if (_includeSlashArc)
            {
                CreateParticleChild(rootGo, "Slash_Arc", _frontSortingLayer, 10, 0.2f, 0.12f, 0f, 1);
            }

            // 3. Slash Glow
            if (_includeSlashGlow)
            {
                CreateParticleChild(rootGo, "Slash_Glow", _frontSortingLayer, 8, 0.25f, 0.15f, 0f, 1);
            }

            // 4. Sparks Burst
            if (_includeSparks)
            {
                CreateParticleChild(rootGo, "Sparks_Burst", _frontSortingLayer, 12, 0.3f, 0.25f, 15f, 15);
            }

            // 5. Hit Impact
            if (_includeImpact)
            {
                CreateParticleChild(rootGo, "Hit_Impact", _frontSortingLayer, 15, 0.25f, 0.15f, 0f, 1);
            }

            // 6. Smoke Residual
            if (_includeSmoke)
            {
                CreateParticleChild(rootGo, "Smoke_Residual", _backSortingLayer, 2, 0.45f, 0.4f, 5f, 5);
            }

            // 7. Distortion Heat
            if (_includeDistortion)
            {
                CreateParticleChild(rootGo, "Distortion_Heat", _frontSortingLayer, 20, 0.15f, 0.1f, 0f, 1);
            }

            Selection.activeGameObject = rootGo;
            Debug.Log($"<color=#00FF00>[VFX Generator]</color> Đã tạo thành công Hierarchy cho <b>{_skillName}</b> với các layer tối ưu 60 FPS!");
        }

        private GameObject CreateParticleChild(GameObject root, string childName, string sortingLayer, int sortingOrder, float duration, float lifetime, float speed, short burstCount)
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(root.transform, false);

            ParticleSystem ps = child.AddComponent<ParticleSystem>();
            ParticleSystemRenderer psRenderer = child.GetComponent<ParticleSystemRenderer>();

            // Main module
            var main = ps.main;
            main.duration = duration;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.loop = false;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            // Emission module
            var emission = ps.emission;
            emission.rateOverTime = 0;
            if (burstCount > 0)
            {
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, burstCount) });
            }

            // Shape module
            var shape = ps.shape;
            if (speed == 0f && burstCount == 1)
            {
                shape.enabled = false;
            }

            // Sorting Layer
            if (!string.IsNullOrEmpty(sortingLayer))
            {
                psRenderer.sortingLayerName = sortingLayer;
            }
            psRenderer.sortingOrder = sortingOrder;

            // Đảm bảo Materials tồn tại và gán tự động chuẩn xác cho từng Layer
            VFXMaterialGenerator.GenerateAllVFXMaterials();
            string layerTag = "Arc";
            if (childName.Contains("Anticipation") || childName.Contains("Flash")) layerTag = "Flash";
            else if (childName.Contains("Arc")) layerTag = "Arc";
            else if (childName.Contains("Glow")) layerTag = "Glow";
            else if (childName.Contains("Sparks")) layerTag = "Sparks";
            else if (childName.Contains("Impact")) layerTag = "Impact";
            else if (childName.Contains("Smoke")) layerTag = "Smoke";

            string matPath = $"Assets/VFX/SkillLibrary/Materials/MAT_{_skillName}_{layerTag}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            if (mat == null)
            {
                mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/VFX/SkillLibrary/Materials/MAT_Additive_Default.mat");
            }

            if (mat != null)
            {
                psRenderer.sharedMaterial = mat;
            }

            return child;
        }
    }
}
