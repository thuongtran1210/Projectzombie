#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.UI.Common;
using ProjectZombie.Features.UI.HUD;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Tool gắn kết Safe Area hoàn toàn KHÔNG PHÁ HỦY (Non-Destructive Safe Area Injector).
    /// Cam kết 100%:
    /// - KHÔNG xóa bất kỳ GameObject, Component hay Child nào.
    /// - KHÔNG thay đổi Sprite, Màu sắc, Font chữ, Kích thước (sizeDelta) hay vị trí tương đối của nút bấm.
    /// - Chỉ bọc một container trung gian gắn SafeAreaFitter và giữ nguyên vị trí thế giới (worldPositionStays = true).
    /// </summary>
    public static class InGameSafeAreaInjector
    {
        [MenuItem("ProjectZombie/2. 📱 Mobile UI/4. Tối Ưu UI/4. Gắn Safe Zone Trong Trận (1-Click)", priority = 163)]
        public static void InjectAllInGameSafeAreas()
        {
            int modifiedCount = 0;

            if (InjectTopHUD()) modifiedCount++;
            if (InjectMobileControls()) modifiedCount++;

            if (modifiedCount > 0)
            {
                Debug.Log($"<color=#00FF88>[InGameSafeAreaInjector]</color> Đã gắn Safe Area an toàn vào {modifiedCount} cụm UI trên Scene mà không làm thay đổi bất kỳ thuộc tính nào!");
                EditorUtility.DisplayDialog("Thành Công", $"Đã gắn Safe Area cho {modifiedCount} cụm UI (Top HUD & Mobile Controls) thành công 100% không đè asset hay vị trí!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Thông Báo", "Tất cả các cụm UI trong Scene đã có Safe Area hoặc không tìm thấy UI trong trận.", "OK");
            }
        }

        [MenuItem("ProjectZombie/2. 📱 Mobile UI/4. Tối Ưu UI/4.1 Gắn Safe Zone Cho Top HUD", priority = 164)]
        public static bool InjectTopHUD()
        {
            // Tìm root Top HUD
            GameObject hudRoot = GameObject.Find("UI_RunHUDRoot");
            if (hudRoot == null) hudRoot = GameObject.Find("RunHUD_Root");
            if (hudRoot == null)
            {
                var view = Object.FindAnyObjectByType<RunHUDView>();
                if (view != null) hudRoot = view.gameObject;
            }

            if (hudRoot == null)
            {
                Debug.LogWarning("[InGameSafeAreaInjector] Không tìm thấy UI_RunHUDRoot trong Scene!");
                return false;
            }

            // Kiểm tra xem đã có SafeArea_TopHUD chưa
            Transform existingSafe = hudRoot.transform.Find("SafeArea_TopHUD");
            if (existingSafe != null)
            {
                var fitter = existingSafe.GetComponent<SafeAreaFitter>();
                if (fitter == null) fitter = existingSafe.gameObject.AddComponent<SafeAreaFitter>();
                fitter.ConfigureEdges(left: true, right: true, top: true, bottom: false);
                Debug.Log("[InGameSafeAreaInjector] Top HUD đã có SafeArea_TopHUD, đã cập nhật cấu hình fitter!");
                return true;
            }

            Undo.RegisterFullObjectHierarchyUndo(hudRoot, "Inject Safe Area Top HUD");

            // Tạo container trung gian
            GameObject safeObj = new GameObject("SafeArea_TopHUD", typeof(RectTransform));
            safeObj.transform.SetParent(hudRoot.transform, false);
            safeObj.transform.SetAsFirstSibling();

            RectTransform safeRT = safeObj.GetComponent<RectTransform>();
            safeRT.anchorMin = Vector2.zero;
            safeRT.anchorMax = Vector2.one;
            safeRT.offsetMin = Vector2.zero;
            safeRT.offsetMax = Vector2.zero;

            var safeFitter = safeObj.AddComponent<SafeAreaFitter>();
            safeFitter.ConfigureEdges(left: true, right: true, top: true, bottom: false);

            // Chuyển tất cả con hiện có của hudRoot vào safeObj (bảo toàn 100% transform)
            var childrenToMove = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < hudRoot.transform.childCount; i++)
            {
                Transform ch = hudRoot.transform.GetChild(i);
                if (ch != safeObj.transform)
                {
                    childrenToMove.Add(ch);
                }
            }

            foreach (var ch in childrenToMove)
            {
                ch.SetParent(safeObj.transform, true); // worldPositionStays = true: Tuyệt đối không xê dịch 1 pixel
            }

            EditorUtility.SetDirty(hudRoot);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(hudRoot.scene);
            Debug.Log("<color=#00FF88>[InGameSafeAreaInjector]</color> Đã bọc Top HUD vào SafeArea_TopHUD an toàn tuyệt đối!");
            return true;
        }

        [MenuItem("ProjectZombie/2. 📱 Mobile UI/4. Tối Ưu UI/4.2 Gắn Safe Zone Cho Mobile Controls", priority = 165)]
        public static bool InjectMobileControls()
        {
            // Tìm root Mobile Controls
            GameObject mobilePanel = GameObject.Find("Panel_MobileControls");
            if (mobilePanel == null)
            {
                var canvas = Object.FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
                    {
                        if (t.name.ToLower().Contains("mobilecontrol") || t.name.ToLower().Contains("touchcontrol"))
                        {
                            mobilePanel = t.gameObject;
                            break;
                        }
                    }
                }
            }

            if (mobilePanel == null)
            {
                Debug.LogWarning("[InGameSafeAreaInjector] Không tìm thấy Panel_MobileControls trong Scene!");
                return false;
            }

            // Kiểm tra xem đã có SafeArea_ControlsContainer chưa
            Transform existingSafe = mobilePanel.transform.Find("SafeArea_ControlsContainer");
            if (existingSafe != null)
            {
                var fitter = existingSafe.GetComponent<SafeAreaFitter>();
                if (fitter == null) fitter = existingSafe.gameObject.AddComponent<SafeAreaFitter>();
                fitter.ConfigureEdges(left: true, right: true, top: false, bottom: true);
                Debug.Log("[InGameSafeAreaInjector] Mobile Controls đã có SafeArea_ControlsContainer, đã cập nhật cấu hình fitter!");
                return true;
            }

            Undo.RegisterFullObjectHierarchyUndo(mobilePanel, "Inject Safe Area Mobile Controls");

            // Tạo container trung gian
            GameObject safeObj = new GameObject("SafeArea_ControlsContainer", typeof(RectTransform));
            safeObj.transform.SetParent(mobilePanel.transform, false);
            safeObj.transform.SetAsFirstSibling();

            RectTransform safeRT = safeObj.GetComponent<RectTransform>();
            safeRT.anchorMin = Vector2.zero;
            safeRT.anchorMax = Vector2.one;
            safeRT.offsetMin = Vector2.zero;
            safeRT.offsetMax = Vector2.zero;

            var safeFitter = safeObj.AddComponent<SafeAreaFitter>();
            safeFitter.ConfigureEdges(left: true, right: true, top: false, bottom: true);

            // Chuyển tất cả con hiện có của mobilePanel vào safeObj (bảo toàn 100% transform)
            var childrenToMove = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < mobilePanel.transform.childCount; i++)
            {
                Transform ch = mobilePanel.transform.GetChild(i);
                if (ch != safeObj.transform)
                {
                    childrenToMove.Add(ch);
                }
            }

            foreach (var ch in childrenToMove)
            {
                ch.SetParent(safeObj.transform, true); // worldPositionStays = true: Tuyệt đối không xê dịch 1 pixel
            }

            EditorUtility.SetDirty(mobilePanel);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mobilePanel.scene);
            Debug.Log("<color=#00FF88>[InGameSafeAreaInjector]</color> Đã bọc Mobile Controls vào SafeArea_ControlsContainer an toàn tuyệt đối!");
            return true;
        }
    }
}
#endif
