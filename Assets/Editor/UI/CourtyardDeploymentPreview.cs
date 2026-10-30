#if UNITY_EDITOR
using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;
using ProjectZombie.Features.Defense3D.Loadout;

namespace ProjectZombie.Editor.UI
{
    /// <summary>Renders authored courtyard prefabs and a real hero visual in an isolated scene.</summary>
    public static class CourtyardDeploymentPreview
    {
        public static string RenderAll()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Render the isolated courtyard preview in Edit Mode.");
            Render(1920, 1080, true);
            Render(1440, 1080, true);
            Render(1920, 1080, false);
            Render(1440, 1080, false);
            return Path.GetFullPath("Logs/CourtyardQA/loadout-1920x1080.png") + "\n"
                + Path.GetFullPath("Logs/CourtyardQA/loadout-1440x1080.png") + "\n"
                + Path.GetFullPath("Logs/CourtyardQA/review-1920x1080.png");
        }

        private static void Render(int width, int height, bool management)
        {
            var original = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(original);
            RenderTexture target = null;
            Texture2D output = null;
            try
            {
                var cameraNode = new GameObject("CourtyardPreviewCamera", typeof(Camera));
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraNode, scene);
                var camera = cameraNode.GetComponent<Camera>();
                camera.scene = scene;
                camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(scene);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.black;
                camera.orthographic = true;
                camera.transform.position = new Vector3(0, 0, -10);
                camera.nearClipPlane = .1f; camera.farClipPlane = 100;
                target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                target.Create(); camera.targetTexture = target;
                var canvasNode = new GameObject("CourtyardPreviewCanvas", typeof(RectTransform), typeof(Canvas));
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(canvasNode, scene);
                var canvas = canvasNode.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 10;
                canvas.scaleFactor = Mathf.Sqrt(width / 1920f * height / 1080f);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(management
                    ? "Assets/_Prefabs/UI/DefenseLoadoutManagementUI.prefab" : "Assets/_Prefabs/UI/DefensePreparationUI.prefab");
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                instance.transform.SetParent(canvasNode.transform, false);
                var view = instance.GetComponent<DefensePreparationView>();
                if (!management)
                    view.SetStageReview(DefenseStageReviewPresentation.FromStage(AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.Maps.StageDefinitionSO>("Assets/_Data/Levels/Stages/Stage_04_SongQue_Defense3D.asset")));
                var so = new SerializedObject(view);
                var catalog = Resources.Load<DefenseHeroCatalogSO>("Defense3D/DefenseHeroCatalog");
                var hero = catalog != null ? catalog.GetHeroById(Defense3DCatalog.HERO_THU_SINH) : null;
                if (hero == null) throw new InvalidOperationException("Hero catalog fixture is unavailable.");
                view.SetHeroInfo("THƯ SINH <color=#E5BE68>• 1 SAO</color>", "Chủ tướng dẫn đầu đội hình\nMáu 100 • Công 35 • Tốc 5.0", hero.Avatar);
                var icons = so.FindProperty("_catalogIcons");
                Sprite GetIcon(string id)
                {
                    for (int i = 0; i < icons.arraySize; i++)
                    {
                        var entry = icons.GetArrayElementAtIndex(i);
                        if (entry.FindPropertyRelative("Id").stringValue == id)
                            return (Sprite)entry.FindPropertyRelative("Icon").objectReferenceValue;
                    }
                    return null;
                }
                view.SetRelicInfo("DÉP TỔ ONG", "Thần khí hộ thân\nLao tới • Đẩy lùi • Sát thương diện rộng", GetIcon(Defense3DCatalog.RELIC_DEP_TO_ONG));
                string[] minions = { Defense3DCatalog.MINION_DAN_BINH, Defense3DCatalog.MINION_PHU_TAP_DICH, Defense3DCatalog.MINION_THAY_LANG };
                string[] towers = { Defense3DCatalog.TOWER_CUNG_TIEN };
                var cards = Defense3DCatalog.CreateDefault();
                for (int i = 0; i < 4; i++)
                {
                    view.SetMinionSlotState(i, i < minions.Length, i < minions.Length && cards.TryGetEntry(minions[i], out var m) ? m.DisplayName : "+ Thêm");
                    view.SetTowerSlotState(i, i < towers.Length, i < towers.Length && cards.TryGetEntry(towers[i], out var t) ? t.DisplayName : "+ Thêm");
                    if (i < minions.Length) ((Image)so.FindProperty("_minionSlotCheckmarks").GetArrayElementAtIndex(i).objectReferenceValue).sprite = GetIcon(minions[i]);
                    if (i < towers.Length) ((Image)so.FindProperty("_towerSlotCheckmarks").GetArrayElementAtIndex(i).objectReferenceValue).sprite = GetIcon(towers[i]);
                }
                ((TextMeshProUGUI)so.FindProperty("_minionCountText").objectReferenceValue).text = "3/4 vị trí • Cận chiến / Hậu cần / Hỗ trợ";
                ((TextMeshProUGUI)so.FindProperty("_towerCountText").objectReferenceValue).text = "1/4 vị trí • Hỏa lực phòng thủ";
                view.UpdateValidationDisplay(true, management ? "Chạm từng vị trí để thay trang bị." : "Lực lượng đã tập hợp. Sẵn sàng bảo vệ làng.");
                view.ShowLoadingOverlay(false, string.Empty);
                var previewPrefab = hero.HeroPrefab != null ? hero.HeroPrefab : view.GetAuthoredHeroPreview(Defense3DCatalog.HERO_THU_SINH);
                if (management && previewPrefab != null)
                {
                    var stageNode = new GameObject("CourtyardModelStage");
                    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(stageNode, scene);
                    stageNode.transform.position = new Vector3(2000, 2000, 0);
                    var stage = stageNode.AddComponent<CharacterPreviewStage>();
                    stage.DisplayCharacter(previewPrefab, "Idle");
                    stage.FrameCurrentCharacter();
                    var modelCamera = stage.GetComponentInChildren<Camera>();
                    modelCamera.scene = scene;
                    modelCamera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(scene);
                    modelCamera.Render();
                    var raw = (RawImage)so.FindProperty("_courtyardHeroPreview").objectReferenceValue;
                    raw.texture = stage.PreviewTexture; raw.enabled = true;
                    ((Image)so.FindProperty("_courtyardHeroFallback").objectReferenceValue).enabled = false;
                }
                Canvas.ForceUpdateCanvases();
                foreach (var text in instance.GetComponentsInChildren<TextMeshProUGUI>(true))
                    if (text.gameObject.activeInHierarchy) text.ForceMeshUpdate();
                camera.Render();
                var previous = RenderTexture.active;
                try
                {
                    RenderTexture.active = target;
                    output = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    output.ReadPixels(new Rect(0, 0, width, height), 0, 0); output.Apply();
                }
                finally { RenderTexture.active = previous; }
                Directory.CreateDirectory("Logs/CourtyardQA");
                File.WriteAllBytes("Logs/CourtyardQA/" + (management ? "loadout-" : "review-") + width + "x" + height + ".png", output.EncodeToPNG());
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (target != null) { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
                if (output != null) UnityEngine.Object.DestroyImmediate(output);
            }
        }
    }
}
#endif
