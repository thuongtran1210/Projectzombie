#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Tool quét và tự động tắt Raycast Target trên tất cả các UI tĩnh (Image, RawImage, Text, TextMeshProUGUI)
    /// đang mở trong Scene và trong các UI Prefabs, giữ lại tương tác cho Button, Toggle, Slider, ScrollRect, InputField, Joystick.
    /// </summary>
    public static class UIRaycastOptimizer
    {
        [MenuItem("Tools/ProjectZombie/UI/Disable Static UI Raycasts (Active Scene)", priority = 2)]
        [MenuItem("ProjectZombie/UI/Disable Static UI Raycasts (Active Scene)", priority = 2)]
        [MenuItem("Tools/ProjectZombie/UI/Disable Static UI Raycasts (All Prefabs)", priority = 3)]
        [MenuItem("ProjectZombie/UI/Disable Static UI Raycasts (All Prefabs)", priority = 3)]
        public static void OptimizeRaycastInActiveScene()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.isLoaded)
            {
                Debug.LogWarning("[UI Raycast Optimizer] Không có Scene nào đang mở!");
                return;
            }

            var rootObjects = activeScene.GetRootGameObjects();
            int totalDisabled = 0;
            int totalChecked = 0;

            Undo.SetCurrentGroupName("Optimize UI Raycast Targets");
            int undoGroup = Undo.GetCurrentGroup();

            foreach (var root in rootObjects)
            {
                var graphics = root.GetComponentsInChildren<Graphic>(true);
                foreach (var g in graphics)
                {
                    totalChecked++;
                    if (!g.raycastTarget) continue;

                    // Kiểm tra xem GameObject hoặc Cha của nó có phải là thành phần tương tác hay không
                    if (IsInteractiveUIElement(g))
                    {
                        continue;
                    }

                    // Nếu là UI tĩnh (Background, Frame, Icon, Static Text, Decorator...) -> Tắt raycastTarget
                    Undo.RecordObject(g, "Disable Raycast Target");
                    g.raycastTarget = false;
                    EditorUtility.SetDirty(g);
                    totalDisabled++;
                }
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(activeScene);

            Debug.Log($"<color=#00FF00><b>[UI Raycast Optimizer]</b></color> Đã quét <b>{totalChecked}</b> UI elements. Đã tắt Raycast Target trên <b>{totalDisabled}</b> UI tĩnh trong Scene: <i>{activeScene.name}</i>!");
            EditorUtility.DisplayDialog(
                "Tối Ưu Raycast Target Hoàn Tất",
                $"Đã quét: {totalChecked} Graphics\n" +
                $"Đã tắt Raycast Target trên: {totalDisabled} UI tĩnh\n" +
                $"Giữ lại đầy đủ tương tác cho Buttons, Sliders, Toggles, Joysticks!",
                "Tuyệt vời");
        }

        [MenuItem("Tools/ProjectZombie/UI/⚡ Tắt Raycast Target Toàn Bộ UI Prefabs", priority = 11)]
        public static void OptimizeRaycastInAllUIPrefabs()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Prefabs/UI", "Assets/Art/UI" });
            int totalDisabled = 0;
            int prefabCount = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab == null) continue;

                    bool isDirty = false;
                    var graphics = prefab.GetComponentsInChildren<Graphic>(true);
                    foreach (var g in graphics)
                    {
                        if (!g.raycastTarget) continue;

                        if (IsInteractiveUIElement(g))
                        {
                            continue;
                        }

                        g.raycastTarget = false;
                        isDirty = true;
                        totalDisabled++;
                    }

                    if (isDirty)
                    {
                        EditorUtility.SetDirty(prefab);
                        prefabCount++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#00FF00><b>[UI Prefabs Optimizer]</b></color> Đã tối ưu {totalDisabled} elements trên {prefabCount} UI Prefabs!");
            EditorUtility.DisplayDialog(
                "Tối Ưu UI Prefabs Hoàn Tất",
                $"Đã cập nhật: {prefabCount} Prefabs\nĐã tắt Raycast Target trên: {totalDisabled} UI tĩnh!",
                "OK");
        }

        /// <summary>
        /// Xác định xem một UI Graphic có thuộc đối tượng cần nhận tương tác (Click/Drag/Pointer) hay không
        /// </summary>
        private static bool IsInteractiveUIElement(Graphic graphic)
        {
            if (graphic == null) return false;
            GameObject go = graphic.gameObject;

            // 1. Kiểm tra chính GameObject
            if (HasInteractiveComponent(go)) return true;

            // 2. Kiểm tra các component cha (Ví dụ: Text nằm trong Button, Icon nằm trong Button)
            var parentSelectable = go.GetComponentInParent<Selectable>(true);
            if (parentSelectable != null)
            {
                // Nếu cha là Button, Toggle, Slider, Dropdown, InputField -> Chỉ Image chính của Button mới cần nhận raycast
                // Text bên trong Button không nhất thiết phải raycast (hoặc nếu là targetGraphic thì giữ)
                if (parentSelectable.targetGraphic == graphic) return true;
                
                // Nếu là Button bình thường, bấm trúng icon/text vẫn click được thì giữ
                if (parentSelectable is Button || parentSelectable is Toggle)
                {
                    return true;
                }
            }

            var parentScroll = go.GetComponentInParent<ScrollRect>(true);
            if (parentScroll != null && (parentScroll.gameObject == go || go.name.ToLower().Contains("handle") || go.name.ToLower().Contains("viewport")))
            {
                return true;
            }

            // 3. Kiểm tra Joystick hoặc EventTrigger tùy biến
            if (go.GetComponentInParent<IPointerDownHandler>() != null ||
                go.GetComponentInParent<IDragHandler>() != null ||
                go.GetComponentInParent<IPointerClickHandler>() != null)
            {
                return true;
            }

            return false;
        }

        private static bool HasInteractiveComponent(GameObject go)
        {
            return go.GetComponent<Selectable>() != null ||
                   go.GetComponent<EventTrigger>() != null ||
                   go.GetComponent<IPointerDownHandler>() != null ||
                   go.GetComponent<IPointerClickHandler>() != null ||
                   go.GetComponent<IDragHandler>() != null;
        }
    }
}
#endif
