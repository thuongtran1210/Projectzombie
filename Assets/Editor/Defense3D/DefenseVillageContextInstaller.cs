using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;

namespace ProjectZombie.EditorTools.Defense3D
{
    /// <summary>Patches proximity controls in place without rebuilding friendly slots, prices or tower statistics.</summary>
    public static class DefenseVillageContextInstaller
    {
        private const string REQUEST = "Temp/ApplyVillageContext.request";
        private const string SCENE = "Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity";

        [InitializeOnLoadMethod]
        private static void Queue() { if (File.Exists(REQUEST)) EditorApplication.update += Process; }
        private static void Process()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= Process;
            if (!File.Exists(REQUEST)) return;
            File.Delete(REQUEST);
            try { Apply(); }
            catch (Exception exception) { File.WriteAllText("Logs/village-context-install.txt", "FAILED: " + exception); Debug.LogException(exception); }
        }

        [MenuItem("TREBRAVE/Defense 3D/Apply Source Proximity Interactions")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before updating interaction zones.");
            var active = SceneManager.GetActiveScene();
            if (active.path != SCENE && active.isDirty) throw new InvalidOperationException("Save the current scene before applying context controls.");
            var scene = active.path == SCENE ? active : EditorSceneManager.OpenScene(SCENE, OpenSceneMode.Single);
            var shop = scene.GetRootGameObjects().SelectMany(item => item.GetComponentsInChildren<DefenseVillageShop3D>(true)).Single();
            var builder = scene.GetRootGameObjects().SelectMany(item => item.GetComponentsInChildren<DefenseTowerBuildPrototype3D>(true)).Single();
            var view = shop.GetComponentInChildren<DefenseVillageShopView>(true);
            Configure(shop, builder, view);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            File.WriteAllText("Logs/village-context-install.txt", "APPLIED: hut radius 1.6m, pedestal radius <1.7m, 11 ground zone rings, projected context button, recruitment-only panel. No Play Mode or GUI QA.\n");
        }

        internal static void Configure(DefenseVillageShop3D shop, DefenseTowerBuildPrototype3D builder, DefenseVillageShopView view)
        {
            var anchor = shop.transform.Find("RecruitmentZone");
            if (anchor == null)
            {
                anchor = new GameObject("RecruitmentZone").transform;
                anchor.SetParent(shop.transform, false); anchor.position = new Vector3(-5.4f, 0f, -2.2f);
            }
            var shopData = new SerializedObject(shop);
            shopData.FindProperty("_recruitmentAnchor").objectReferenceValue = anchor;
            shopData.FindProperty("_recruitmentRadius").floatValue = 1.6f;
            shopData.ApplyModifiedPropertiesWithoutUndo();
            var builderData = new SerializedObject(builder); builderData.FindProperty("buildRadius").floatValue = 1.7f;
            builderData.ApplyModifiedPropertiesWithoutUndo();
            var presenter = shop.GetComponent<DefenseVillageShopPresenter>();
            var camera = shop.gameObject.scene.GetRootGameObjects().SelectMany(item => item.GetComponentsInChildren<Camera>(true)).FirstOrDefault(item => item.CompareTag("MainCamera"));
            var presenterData = new SerializedObject(presenter); presenterData.FindProperty("_camera").objectReferenceValue = camera;
            presenterData.ApplyModifiedPropertiesWithoutUndo();
            var viewData = new SerializedObject(view);
            var panel = (GameObject)viewData.FindProperty("_panel").objectReferenceValue;
            ((RectTransform)panel.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 426f);
            var status = (Component)viewData.FindProperty("_status").objectReferenceValue;
            ((RectTransform)status.transform).anchoredPosition = new Vector2(0f, -372f);
            foreach (string field in new[] { "_build", "_upgrade" })
                ((Component)viewData.FindProperty(field).objectReferenceValue).gameObject.SetActive(false);
            var toggle = (Component)viewData.FindProperty("_toggle").objectReferenceValue;
            var rect = (RectTransform)toggle.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = new Vector2(190f, 64f);
            toggle.gameObject.SetActive(false); panel.SetActive(false);

            const string materialPath = "Assets/Art/Minions/Vongxuyen/Materials/InteractionZone.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                var template = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Defense3D/DefenseAim_FootRing.mat");
                if (template == null) throw new InvalidOperationException("Missing ground ring material.");
                material = new Material(template); AssetDatabase.CreateAsset(material, materialPath);
            }
            material.SetFloat("_Shape", 1f); material.SetFloat("_EdgeWidth", .025f);
            material.SetColor("_BaseColor", new Color(.3f, .9f, .65f, .32f)); EditorUtility.SetDirty(material);
            var zones = shop.transform.Find("InteractionZones");
            if (zones == null) { zones = new GameObject("InteractionZones").transform; zones.SetParent(shop.transform, false); }
            Ring(zones, "Quán Trà 1.6m", anchor.position, 1.6f, material);
            var map = (DefenseSceneInfo3D)shopData.FindProperty("_map").objectReferenceValue;
            for (int i = 0; i < map.TowerSocketCount; i++) Ring(zones, "TowerZone_" + i, map.GetTowerSocket(i).position, 1.7f, material);
        }

        private static void Ring(Transform parent, string name, Vector3 center, float radius, Material material)
        {
            var item = parent.Find(name);
            if (item == null)
            {
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad); quad.name = name;
                UnityEngine.Object.DestroyImmediate(quad.GetComponent<Collider>());
                item = quad.transform; item.SetParent(parent, false);
            }
            if (Physics.Raycast(center + Vector3.up * 8f, Vector3.down, out var hit, 16f, 4096, QueryTriggerInteraction.Ignore)) center.y = hit.point.y;
            item.SetPositionAndRotation(center + Vector3.up * .055f, Quaternion.Euler(90f, 0f, 0f));
            item.localScale = new Vector3(radius * 2f, radius * 2f, 1f);
            var renderer = item.GetComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows = false;
        }
    }
}
