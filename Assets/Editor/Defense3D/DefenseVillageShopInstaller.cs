using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;

namespace ProjectZombie.EditorTools.Defense3D
{
    /// <summary>Authors shop UI, source models and a fixed friendly pool into the Defense scene.</summary>
    public static class DefenseVillageShopInstaller
    {
        private const string ART = "Assets/Art/Minions/Vongxuyen";
        private const string SCENE = "Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity";
        private const string REQUEST = "Temp/InstallVillageShop.request";
        private const string ROOT = "[DEFENSE VILLAGE SHOP]";
        private const int SLOTS_PER_ROLE = 12;
        private static readonly string[] IDS = { "danbinh", "phutapdich", "thaylang" };
        private static readonly string[] NAMES = { "Dân Binh Giáo Tre", "Phu Tạp Dịch", "Thầy Lang Nam Dược" };
        private static readonly int[] COSTS = { 40, 30, 35 };
        private static readonly float[] HEALTH = { 260f, 115f, 135f };
        private static readonly float[] SPEEDS = { 1.85f, 2.95f, 2.25f };
        private static readonly float[] DAMAGE = { 9f, 6f, 4f };
        private static readonly float[] RANGES = { 1.45f, 1.1f, 4.2f };
        private static TMP_FontAsset _font;

        [InitializeOnLoadMethod]
        private static void Queue()
        {
            if (File.Exists(REQUEST)) EditorApplication.update += ProcessRequest;
        }
        private static void ProcessRequest()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= ProcessRequest;
            if (!File.Exists(REQUEST)) return;
            File.Delete(REQUEST);
            try { Install(); }
            catch (Exception exception)
            {
                File.WriteAllText("Logs/village-shop-install.txt", "FAILED: " + exception);
                Debug.LogException(exception);
            }
        }

        [MenuItem("TREBRAVE/Defense 3D/Install Vongxuyen Village Shop")]
        public static void Install()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before installing the shop.");
            var active = SceneManager.GetActiveScene();
            if (active.path != SCENE && active.isDirty)
                throw new InvalidOperationException("Save the current scene before installing the Defense shop.");
            Directory.CreateDirectory(ART + "/Materials");
            Directory.CreateDirectory(ART + "/Prefabs");
            Directory.CreateDirectory(ART + "/Definitions");
            AssetDatabase.Refresh();
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Art/Characters/ThuSinh/ScholarToon.shader");
            if (shader == null) throw new InvalidOperationException("Missing shared toon shader.");
            var definitions = new DefenseMinionDefinitionSO[3];
            var prefabs = new GameObject[3];
            for (int role = 0; role < 3; role++)
            {
                definitions[role] = BuildDefinition(role);
                string path = ART + "/" + IDS[role] + ".glb";
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model == null) throw new InvalidOperationException("Missing minion GLB: " + path);
                string visual = DefenseEnemyVisualImport.BuildVisual(model, shader, IDS[role], ART);
                prefabs[role] = BuildUnit(role, definitions[role], visual);
            }
            var scene = active.path == SCENE ? active : EditorSceneManager.OpenScene(SCENE, OpenSceneMode.Single);
            Func<Type, Component> find = type => scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren(type, true)).Single();
            var composition = (DefenseSceneCompositionRoot)find(typeof(DefenseSceneCompositionRoot));
            var map = (DefenseSceneInfo3D)find(typeof(DefenseSceneInfo3D));
            var home = (DefenseBasePrototype3D)find(typeof(DefenseBasePrototype3D));
            var hero = (DefenseHeroHealth3D)find(typeof(DefenseHeroHealth3D));
            var wallet = hero.GetComponent<DefenseWoodWallet3D>();
            var builder = (DefenseTowerBuildPrototype3D)find(typeof(DefenseTowerBuildPrototype3D));
            var friends = (DefenseFriendlyTargets3D)find(typeof(DefenseFriendlyTargets3D));
            var roots = (DefenseRuntimeRoots3D)find(typeof(DefenseRuntimeRoots3D));
            var compositionData = new SerializedObject(composition);
            var oldRoot = scene.GetRootGameObjects().SingleOrDefault(item => item.name == ROOT);
            var participants = ReadArray<DefenseGameplayBehaviour>(compositionData, "_sceneParticipants")
                .Where(item => item != null && (oldRoot == null || !item.transform.IsChildOf(oldRoot.transform))).ToList();
            var friendsData = new SerializedObject(friends);
            var supports = ReadArray<DefenseSupportTarget3D>(friendsData, "_supports")
                .Where(item => item != null && (oldRoot == null || !item.transform.IsChildOf(oldRoot.transform))).ToList();
            if (oldRoot != null) UnityEngine.Object.DestroyImmediate(oldRoot);
            var root = new GameObject(ROOT);
            var shop = root.AddComponent<DefenseVillageShop3D>();
            var units = new GameObject("FriendlyUnits"); units.transform.SetParent(root.transform, false);
            var slots = new List<DefenseMinion3D>();
            var sceneCamera = scene.GetRootGameObjects().SelectMany(item => item.GetComponentsInChildren<Camera>(true)).FirstOrDefault(item => item.CompareTag("MainCamera"));
            for (int role = 0; role < 3; role++)
            {
                for (int i = 0; i < SLOTS_PER_ROLE; i++)
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[role], units.transform);
                    instance.name = IDS[role] + "_Slot" + (i + 1);
                    instance.SetActive(false);
                    slots.Add(instance.GetComponent<DefenseMinion3D>());
                    supports.Add(instance.GetComponent<DefenseSupportTarget3D>());
                    var bar = new SerializedObject(instance.GetComponentInChildren<DefenseMinionHealthView3D>(true));
                    Set(bar, "_camera", sceneCamera); bar.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            var builderData = new SerializedObject(builder);
            foreach (var tower in ReadArray<DefenseTowerPrototype3D>(builderData, "socketTowers"))
            {
                var health = tower.GetComponent<DefenseSupportTarget3D>();
                if (health == null) health = tower.gameObject.AddComponent<DefenseSupportTarget3D>();
                var healthData = new SerializedObject(health); healthData.FindProperty("_maxHp").floatValue = 250f;
                healthData.FindProperty("_isStructure").boolValue = true; healthData.ApplyModifiedPropertiesWithoutUndo();
                if (tower.GetComponent<Collider>() == null)
                {
                    var box = tower.gameObject.AddComponent<BoxCollider>();
                    box.isTrigger = true;
                    var mesh = tower.GetComponent<MeshFilter>();
                    if (mesh != null && mesh.sharedMesh != null) { box.center = mesh.sharedMesh.bounds.center; box.size = mesh.sharedMesh.bounds.size; }
                }
                if (!supports.Contains(health)) supports.Add(health);
                var towerData = new SerializedObject(tower);
                towerData.FindProperty("attackRange").floatValue = 7f;
                towerData.FindProperty("damage").floatValue = 18f;
                towerData.FindProperty("fireInterval").floatValue = 1.2f;
                towerData.ApplyModifiedPropertiesWithoutUndo();
            }
            builderData.FindProperty("towerWoodCost").intValue = 50;
            builderData.ApplyModifiedPropertiesWithoutUndo();
            var walletData = new SerializedObject(wallet);
            walletData.FindProperty("startingWood").intValue = 100;
            walletData.ApplyModifiedPropertiesWithoutUndo();
            WriteArray(friendsData, "_supports", supports); friendsData.ApplyModifiedPropertiesWithoutUndo();
            var shopData = new SerializedObject(shop);
            WriteArray(shopData, "_catalog", definitions); WriteArray(shopData, "_slots", slots);
            Set(shopData, "_hero", hero); Set(shopData, "_base", home); Set(shopData, "_wallet", wallet);
            Set(shopData, "_map", map); Set(shopData, "_friends", friends); Set(shopData, "_roots", roots); Set(shopData, "_towerBuilder", builder);
            shopData.ApplyModifiedPropertiesWithoutUndo();
            var view = BuildUI(root.transform);
            var presenter = root.AddComponent<DefenseVillageShopPresenter>();
            var presenterData = new SerializedObject(presenter);
            Set(presenterData, "_shop", shop); Set(presenterData, "_view", view); presenterData.ApplyModifiedPropertiesWithoutUndo();
            participants.Add(shop); participants.Add(presenter);
            WriteArray(compositionData, "_sceneParticipants", participants); compositionData.ApplyModifiedPropertiesWithoutUndo();
            DefenseVillageContextInstaller.Configure(shop, builder, view);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            File.WriteAllText("Logs/village-shop-install.txt", "INSTALLED: 3 source minion models, 36 authored reusable slots, friendly damage roster, recruitment/orders UI, shared wood economy, 10 upgradeable towers. No Play Mode or GUI QA performed.\n");
        }

        private static DefenseMinionDefinitionSO BuildDefinition(int role)
        {
            string path = ART + "/Definitions/" + IDS[role] + ".asset";
            var definition = AssetDatabase.LoadAssetAtPath<DefenseMinionDefinitionSO>(path);
            if (definition == null) { definition = ScriptableObject.CreateInstance<DefenseMinionDefinitionSO>(); AssetDatabase.CreateAsset(definition, path); }
            var data = new SerializedObject(definition);
            data.FindProperty("_id").stringValue = IDS[role]; data.FindProperty("_displayName").stringValue = NAMES[role];
            data.FindProperty("_role").enumValueIndex = role; data.FindProperty("_woodCost").intValue = COSTS[role];
            data.FindProperty("_maxHp").floatValue = HEALTH[role]; data.FindProperty("_speed").floatValue = SPEEDS[role];
            data.FindProperty("_damage").floatValue = DAMAGE[role]; data.FindProperty("_attackRange").floatValue = RANGES[role];
            data.ApplyModifiedPropertiesWithoutUndo();
            return definition;
        }

        private static GameObject BuildUnit(int role, DefenseMinionDefinitionSO definition, string visualPath)
        {
            var root = new GameObject(IDS[role] + "_Minion");
            root.SetActive(false);
            try
            {
                root.layer = LayerMask.NameToLayer("Player");
                var agent = root.AddComponent<NavMeshAgent>(); agent.enabled = false;
                agent.radius = .23f; agent.height = 1.35f; agent.baseOffset = 0f; agent.speed = definition.Speed; agent.angularSpeed = 540f; agent.acceleration = 14f;
                var hitbox = root.AddComponent<CapsuleCollider>(); hitbox.isTrigger = true; hitbox.radius = .23f; hitbox.height = 1.35f; hitbox.center = Vector3.up * .675f;
                var health = root.AddComponent<DefenseSupportTarget3D>();
                var healthData = new SerializedObject(health); healthData.FindProperty("_maxHp").floatValue = definition.MaxHp; healthData.ApplyModifiedPropertiesWithoutUndo();
                var visual = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(visualPath), root.transform);
                visual.name = "MinionVisual";
                var renderers = visual.GetComponentsInChildren<Renderer>();
                var bounds = renderers[0].bounds; foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                float scale = 1.35f / Mathf.Max(.01f, bounds.size.y);
                visual.transform.localScale = Vector3.one * scale;
                visual.transform.localPosition = Vector3.down * (bounds.min.y * scale);
                var actor = root.AddComponent<DefenseMinion3D>();
                var data = new SerializedObject(actor);
                Set(data, "_definition", definition); Set(data, "_health", health); Set(data, "_agent", agent);
                var transforms = visual.GetComponentsInChildren<Transform>(true);
                Set(data, "_visualBody", transforms.Single(item => item.name == "MinionBody"));
                WriteArray(data, "_legs", transforms.Where(item => item.name.StartsWith("MinionLeg_", StringComparison.Ordinal)));
                Set(data, "_healthView", BuildHealthBar(root.transform));
                data.ApplyModifiedPropertiesWithoutUndo();
                foreach (Transform item in root.GetComponentsInChildren<Transform>(true)) item.gameObject.layer = root.layer;
                string path = ART + "/Prefabs/" + IDS[role] + "Minion.prefab";
                PrefabUtility.SaveAsPrefabAsset(root, path);
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private sealed class ButtonParts { public Button Button; public TMP_Text Label; }
        private static DefenseMinionHealthView3D BuildHealthBar(Transform parent)
        {
            var root = new GameObject("MinionHealthBar"); root.transform.SetParent(parent, false); root.transform.localPosition = Vector3.up * 1.5f;
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Art/Characters/ThuSinh/ScholarToon.shader");
            Transform fill = null;
            for (int i = 0; i < 2; i++)
            {
                string path = ART + "/Materials/HealthBar_" + i + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
                material.SetColor("_BaseColor", i == 0 ? new Color(.04f, .07f, .06f) : new Color(.18f, .86f, .38f));
                material.SetFloat("_Cull", 0f); EditorUtility.SetDirty(material);
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad); quad.name = i == 0 ? "Track" : "Fill";
                UnityEngine.Object.DestroyImmediate(quad.GetComponent<Collider>()); quad.transform.SetParent(root.transform, false);
                quad.transform.localScale = new Vector3(i == 0 ? .7f : .65f, i == 0 ? .09f : .055f, 1f);
                quad.GetComponent<MeshRenderer>().sharedMaterial = material;
                if (i == 1) fill = quad.transform;
            }
            var view = root.AddComponent<DefenseMinionHealthView3D>();
            var data = new SerializedObject(view); Set(data, "_fill", fill); data.ApplyModifiedPropertiesWithoutUndo();
            view.SetRatio(1f);
            return view;
        }

        private static DefenseVillageShopView BuildUI(Transform parent)
        {
            _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath("8f586378b4e144a9851e7b34d9b748ee"));
            if (_font == null) _font = TMP_Settings.defaultFontAsset;
            var canvasRoot = new GameObject("VillageShopCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasRoot.transform.SetParent(parent, false);
            var canvas = canvasRoot.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 90;
            var scaler = canvasRoot.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = .5f;
            var safe = Rect("SafeArea", canvasRoot.transform, Vector2.zero, Vector2.zero);
            safe.anchorMin = Vector2.zero; safe.anchorMax = Vector2.one; safe.offsetMin = safe.offsetMax = Vector2.zero;
            var panel = Rect("ShopPanel", safe, new Vector2(-20, -154), new Vector2(430, 540));
            panel.anchorMin = panel.anchorMax = Vector2.one; panel.pivot = Vector2.one;
            panel.gameObject.AddComponent<Image>().color = new Color(.035f, .07f, .08f, .97f);
            panel.GetComponent<Image>().raycastTarget = false;
            var toggle = Button("ToggleShop", safe, -90, "Chiêu mộ / Shop", 190, 52);
            var toggleRect = (RectTransform)toggle.Button.transform; toggleRect.anchorMin = toggleRect.anchorMax = Vector2.one; toggleRect.pivot = Vector2.one; toggleRect.anchoredPosition = new Vector2(-20, -90);
            Text("Title", panel, -14, "Chiêu mộ & Xây dựng", 24, 32);
            var wood = Text("Wood", panel, -50, "Gỗ: 100", 21, 28);
            var recruit = new List<ButtonParts>(); var orders = new List<ButtonParts>();
            for (int role = 0; role < 3; role++)
            {
                recruit.Add(Button("Recruit_" + IDS[role], panel, -90 - role * 90, NAMES[role], 406, 46));
                orders.Add(Button("Order_" + IDS[role], panel, -138 - role * 90, "Thủ nhà", 406, 40));
            }
            var build = Button("BuildTower", panel, -374, "Xây tháp", 406, 46);
            var upgrade = Button("UpgradeTower", panel, -424, "Nâng cấp tháp", 406, 46);
            var status = Text("Status", panel, -482, "Chọn lính hoặc đến gần bệ tháp.", 17, 46);
            var view = canvasRoot.AddComponent<DefenseVillageShopView>();
            var data = new SerializedObject(view);
            Set(data, "_panel", panel.gameObject); Set(data, "_safeArea", safe); Set(data, "_toggle", toggle.Button);
            Set(data, "_wood", wood); Set(data, "_status", status); Set(data, "_build", build.Button); Set(data, "_upgrade", upgrade.Button);
            Set(data, "_buildLabel", build.Label); Set(data, "_upgradeLabel", upgrade.Label);
            WriteArray(data, "_recruit", recruit.Select(item => item.Button)); WriteArray(data, "_orders", orders.Select(item => item.Button));
            WriteArray(data, "_recruitLabels", recruit.Select(item => item.Label)); WriteArray(data, "_orderLabels", orders.Select(item => item.Label));
            data.ApplyModifiedPropertiesWithoutUndo();
            DefenseVillageShopUIStyler.Apply(view);
            return view;
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var root = new GameObject(name, typeof(RectTransform)); root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1f); rect.pivot = new Vector2(.5f, 1f);
            rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        private static TMP_Text Text(string name, Transform parent, float y, string label, int fontSize, float height)
        {
            var rect = Rect(name, parent, new Vector2(0, y), new Vector2(406, height));
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>(); text.font = _font; text.text = label; text.fontSize = fontSize;
            text.color = new Color(.95f, .94f, .84f); text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            text.enableAutoSizing = true; text.fontSizeMin = 14; text.fontSizeMax = fontSize; return text;
        }
        private static ButtonParts Button(string name, Transform parent, float y, string label, int width, int height)
        {
            var rect = Rect(name, parent, new Vector2(0, y), new Vector2(width, height));
            var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(.13f, .26f, .24f); image.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var text = Text(name + "Label", rect, 0, label, 19, height);
            var textRect = (RectTransform)text.transform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = new Vector2(6, 0); textRect.offsetMax = new Vector2(-6, 0);
            return new ButtonParts { Button = button, Label = text };
        }
        private static void Set(SerializedObject data, string field, UnityEngine.Object value) => data.FindProperty(field).objectReferenceValue = value;
        private static IEnumerable<T> ReadArray<T>(SerializedObject data, string field) where T : UnityEngine.Object
        {
            var array = data.FindProperty(field);
            for (int i = 0; i < array.arraySize; i++) yield return array.GetArrayElementAtIndex(i).objectReferenceValue as T;
        }
        private static void WriteArray<T>(SerializedObject data, string field, IEnumerable<T> values) where T : UnityEngine.Object
        {
            var list = values.ToArray(); var array = data.FindProperty(field); array.arraySize = list.Length;
            for (int i = 0; i < list.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
        }
    }
}
