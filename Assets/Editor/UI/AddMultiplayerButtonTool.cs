#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Tool an toàn (Non-destructive): Chỉ chèn thêm GameObject Btn_Multiplayer ngay trên Btn_StartRun
    /// Tuyệt đối KHÔNG thay đổi, ghi đè hoặc tái tạo bất kỳ thành phần/tọa độ nào khác của Designer.
    /// </summary>
    [InitializeOnLoad]
    public static class AddMultiplayerButtonTool
    {
        private static bool _hasExecuted = false;

        static AddMultiplayerButtonTool()
        {
            EditorApplication.delayCall += () =>
            {
                if (!_hasExecuted)
                {
                    _hasExecuted = true;
                    AddMultiplayerButtonOnly();
                }
            };
        }

        [MenuItem("ProjectZombie/2. 📱 Mobile UI/1. Sảnh Chính (Meta Menu)/⚡ Thêm Nút Multiplayer (Bảo Tồn Toàn Bộ Tùy Chỉnh)", priority = 100)]
        public static void AddMultiplayerButtonOnly()
        {
            string prefabPath = "Assets/_Prefabs/UI/MainHubUI.prefab";
            if (!System.IO.File.Exists(prefabPath))
            {
                Debug.LogError($"[AddMultiplayerButtonTool] Không tìm thấy Prefab tại {prefabPath}");
                return;
            }

            // 1. Mở Prefab Contents để chỉnh sửa trực tiếp nội dung mà không rebuild
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError("[AddMultiplayerButtonTool] Không thể mở Prefab Contents!");
                return;
            }

            try
            {
                ProcessGameObject(prefabRoot);

                // 2. Lưu lại Prefab chính
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);

                // 3. Đồng bộ sang Assets/Resources/UI nếu có
                string resFolder = "Assets/Resources/UI";
                if (System.IO.Directory.Exists(resFolder))
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, $"{resFolder}/MainHubUI.prefab");
                }

                Debug.Log("<color=#00FF88>[AddMultiplayerButtonTool]</color> Đã thêm thành công nút Multiplayer vào Prefab MainHubUI mà KHÔNG làm thay đổi bất kỳ tùy chỉnh nào khác!");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            // 4. Nếu trong Scene đang mở có instance của MainHubUI, cập nhật luôn trong Scene
            var sceneView = Object.FindAnyObjectByType<MainHubView>();
            if (sceneView != null)
            {
                Undo.RegisterFullObjectHierarchyUndo(sceneView.gameObject, "Add Multiplayer Button In Scene");
                ProcessGameObject(sceneView.gameObject);
                EditorUtility.SetDirty(sceneView.gameObject);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(sceneView.gameObject.scene);
                Debug.Log("<color=#00FF88>[AddMultiplayerButtonTool]</color> Đã đồng bộ trực tiếp nút Multiplayer vào Scene hiện tại!");
            }

            AssetDatabase.Refresh();
        }

        private static void ProcessGameObject(GameObject rootObj)
        {
            var view = rootObj.GetComponent<MainHubView>();
            Transform startRunTrans = FindChildRecursive(rootObj.transform, "Btn_StartRun");
            if (startRunTrans == null) startRunTrans = FindChildRecursive(rootObj.transform, "StartRunButton");
            if (startRunTrans == null) startRunTrans = FindChildRecursive(rootObj.transform, "Button_StartRun");

            if (startRunTrans == null)
            {
                Debug.LogWarning("[AddMultiplayerButtonTool] Không tìm thấy Btn_StartRun trong Hierarchy!");
                return;
            }

            RectTransform startRunRT = startRunTrans.GetComponent<RectTransform>();
            Transform parentTrans = startRunTrans.parent;

            // Kiểm tra xem Btn_Multiplayer đã tồn tại chưa
            Transform existingMp = FindChildRecursive(parentTrans, "Btn_Multiplayer");
            GameObject mpObj;
            if (existingMp != null)
            {
                mpObj = existingMp.gameObject;
            }
            else
            {
                mpObj = new GameObject("Btn_Multiplayer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                mpObj.transform.SetParent(parentTrans, false);
            }

            RectTransform mpRT = mpObj.GetComponent<RectTransform>();
            mpRT.anchorMin = startRunRT.anchorMin;
            mpRT.anchorMax = startRunRT.anchorMax;
            mpRT.pivot = startRunRT.pivot;

            // Đặt vị trí ngay trên Btn_StartRun dựa theo chiều cao thực tế của Btn_StartRun
            float startY = startRunRT.anchoredPosition.y;
            float startHeight = startRunRT.sizeDelta.y;
            float mpHeight = 46f;
            float mpWidth = Mathf.Min(startRunRT.sizeDelta.x, 230f);

            mpRT.anchoredPosition = new Vector2(startRunRT.anchoredPosition.x, startY + startHeight + 8f);
            mpRT.sizeDelta = new Vector2(mpWidth, mpHeight);
            mpRT.localScale = Vector3.one;
            mpRT.localRotation = Quaternion.identity;

            // Cấu hình Image 9-slice
            var mpImg = mpObj.GetComponent<Image>();
            Sprite btnMpWood = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Nav_Wood_Stitched.png");
            if (btnMpWood == null) btnMpWood = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_GoMun_Dark.png");
            if (btnMpWood != null)
            {
                mpImg.sprite = btnMpWood;
                mpImg.type = Image.Type.Sliced;
                mpImg.color = Color.white;
            }

            var mpBtn = mpObj.GetComponent<Button>();

            // Cấu hình Text bên trong
            Transform txtTrans = mpObj.transform.Find("Text");
            GameObject txtObj;
            if (txtTrans != null)
            {
                txtObj = txtTrans.gameObject;
            }
            else
            {
                txtObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer));
                txtObj.transform.SetParent(mpObj.transform, false);
            }

            RectTransform txtRT = txtObj.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = new Vector2(12, 4);
            txtRT.offsetMax = new Vector2(-12, -4);

            var tmp = txtObj.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = txtObj.AddComponent<TextMeshProUGUI>();

            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (vietFont == null) vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (vietFont != null) tmp.font = vietFont;

            tmp.text = "<color=#FFD700>ĐỒNG ĐỘI (CO-OP)</color>";
            tmp.fontSize = 15f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            // Wire vào MainHubView
            if (view != null)
            {
                SerializedObject soView = new SerializedObject(view);
                var mpProp = soView.FindProperty("_multiplayerButton");
                if (mpProp != null)
                {
                    mpProp.objectReferenceValue = mpBtn;
                    soView.ApplyModifiedProperties();
                }
            }
        }

        private static Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent == null) return null;
            if (parent.name.Equals(childName, System.StringComparison.OrdinalIgnoreCase)) return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindChildRecursive(parent.GetChild(i), childName);
                if (found != null) return found;
            }
            return null;
        }
    }
}
#endif
