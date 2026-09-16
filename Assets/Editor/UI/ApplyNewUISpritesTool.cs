#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Tool an toàn: Cập nhật các Sprite mới vừa được cắt từ UI_Footer_NewSheet.png vào Prefab & Scene
    /// Tuyệt đối KHÔNG thay đổi vị trí, kích thước hay cấu trúc Hierarchy hiện tại của Designer.
    /// </summary>
    [InitializeOnLoad]
    public static class ApplyNewUISpritesTool
    {
        private static bool _hasExecuted = false;

        static ApplyNewUISpritesTool()
        {
            EditorApplication.delayCall += () =>
            {
                if (!_hasExecuted && !Application.isPlaying)
                {
                    _hasExecuted = true;
                    ApplyNewUISprites();
                }
            };
        }

        [MenuItem("ProjectZombie/2. 📱 Mobile UI/1. Sảnh Chính (Meta Menu)/✨ Áp Dụng Bộ Sprite Giao Diện Mới (1-Click Safe)", priority = 99)]
        public static void ApplyNewUISprites()
        {
            if (Application.isPlaying) return;

            // Load Sprites mới
            Sprite spStartRun = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_StartRun_Amber.png");
            Sprite spMultiplayer = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Multiplayer_Teal.png");
            Sprite spHeader = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Header_TopBar_WoodLeaf.png");
            Sprite spHero = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Badge_Nav_Hero.png");
            Sprite spInventory = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Badge_Nav_Inventory.png");
            Sprite spChest = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Badge_Nav_Chest.png");
            Sprite spSanctuary = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Badge_Nav_Sanctuary.png");
            Sprite spRelic = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Badge_Nav_Relic.png");

            string prefabPath = "Assets/_Prefabs/UI/MainHubUI.prefab";
            if (System.IO.File.Exists(prefabPath))
            {
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                if (prefabRoot != null)
                {
                    try
                    {
                        ApplyToHierarchy(prefabRoot.transform, spStartRun, spMultiplayer, spHeader, spHero, spInventory, spChest, spSanctuary, spRelic);
                        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);

                        string resFolder = "Assets/Resources/UI";
                        if (System.IO.Directory.Exists(resFolder))
                        {
                            PrefabUtility.SaveAsPrefabAsset(prefabRoot, $"{resFolder}/MainHubUI.prefab");
                        }
                        Debug.Log("<color=#00FF88>[ApplyNewUISpritesTool]</color> Đã áp dụng bộ Sprite giao diện mới vào Prefab MainHubUI thành công!");
                    }
                    finally
                    {
                        PrefabUtility.UnloadPrefabContents(prefabRoot);
                    }
                }
            }

            // Đồng bộ trực tiếp nếu Scene đang mở có MainHubView
            var sceneView = Object.FindAnyObjectByType<ProjectZombie.Features.UI.MainHubView>();
            if (sceneView != null)
            {
                Undo.RegisterFullObjectHierarchyUndo(sceneView.gameObject, "Apply New UI Sprites");
                ApplyToHierarchy(sceneView.gameObject.transform, spStartRun, spMultiplayer, spHeader, spHero, spInventory, spChest, spSanctuary, spRelic);
                EditorUtility.SetDirty(sceneView.gameObject);
                if (!Application.isPlaying)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(sceneView.gameObject.scene);
                }
                Debug.Log("<color=#00FF88>[ApplyNewUISpritesTool]</color> Đã áp dụng bộ Sprite giao diện mới vào Scene hiện tại!");
            }

            AssetDatabase.Refresh();
        }

        private static void ApplyToHierarchy(Transform root, Sprite spStartRun, Sprite spMultiplayer, Sprite spHeader,
            Sprite spHero, Sprite spInventory, Sprite spChest, Sprite spSanctuary, Sprite spRelic)
        {
            // 1. Cập nhật Nút Xuất Trận
            Transform btnStart = FindChildRecursive(root, "Btn_StartRun");
            if (btnStart != null && spStartRun != null)
            {
                Image img = btnStart.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = spStartRun;
                    img.type = Image.Type.Sliced;
                }
            }

            // 2. Cập nhật Nút Multiplayer
            Transform btnMp = FindChildRecursive(root, "Btn_Multiplayer");
            if (btnMp != null && spMultiplayer != null)
            {
                Image img = btnMp.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = spMultiplayer;
                    img.type = Image.Type.Sliced;
                }
            }

            // 3. Cập nhật Khung Gỗ Đỉnh
            Transform headerBar = FindChildRecursive(root, "Header_Wood_Bar");
            if (headerBar == null) headerBar = FindChildRecursive(root, "Header_TopBar");
            if (headerBar != null && spHeader != null)
            {
                Image img = headerBar.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = spHeader;
                    img.type = Image.Type.Sliced;
                }
            }

            // 4. Cập nhật 5 Nút Icon Badges Đáy (Anh Hùng, Túi Đồ, Rương, Miếu Cổ, Thần Khí)
            Transform navParent = FindChildRecursive(root, "Group_NavButtons");
            if (navParent != null)
            {
                SetButtonSprite(FindChildRecursive(navParent, "Btn_HeroSelect"), spHero);
                SetButtonSprite(FindChildRecursive(navParent, "Btn_Armory"), spInventory);
                SetButtonSprite(FindChildRecursive(navParent, "Btn_GachaShop"), spChest);
                SetButtonSprite(FindChildRecursive(navParent, "Btn_SanctuaryTree"), spSanctuary);
                SetButtonSprite(FindChildRecursive(navParent, "Btn_Codex"), spRelic);
            }
        }

        private static void SetButtonSprite(Transform btnTrans, Sprite sprite)
        {
            if (btnTrans == null || sprite == null) return;
            Image img = btnTrans.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
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
