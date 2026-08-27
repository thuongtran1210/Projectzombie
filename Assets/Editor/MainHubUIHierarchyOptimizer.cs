using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Editor tool tự động cấu trúc lại và áp dụng Sprite Chibi Casual Arcade 9-Slice cho Sảnh Chính (Main Hub UI).
    /// </summary>
    public static class MainHubUIHierarchyOptimizer
    {
        [MenuItem("Tools/ProjectZombie/UI/Optimize MainHub UI Hierarchy (Chibi Arcade)")]
        public static void OptimizeMainHubUI()
        {
            GameObject rootObj = GameObject.Find("MainHub_Root");
            if (rootObj == null)
            {
                rootObj = GameObject.Find("MainHubView");
            }
            if (rootObj == null)
            {
                // Thử tìm trong Canvas
                MainHubView view = Object.FindAnyObjectByType<MainHubView>();
                if (view != null) rootObj = view.gameObject;
            }

            if (rootObj == null)
            {
                Debug.LogError("[MainHubOptimizer] Không tìm thấy GameObject MainHubView / MainHub_Root trong Scene!");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(rootObj, "Optimize MainHub UI Hierarchy");

            // 1. Tải các asset sprite Vọng Xuyên mới
            Sprite btnBattleAmber = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Battle_Hex_Amber_Glow.png");
            Sprite btnNavWood = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Nav_Wood_Stitched.png");
            Sprite pillWood = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Pill_Currency_Wood.png");
            Sprite trayWood = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Tray_Loadout_Wood_Frame.png");
            Sprite pedestalHex = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Pedestal_Hexagon_2_5D_WoodStone.png");
            Sprite bgForest = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/BG_VongXuyen_Forest_Hub.png");
            Sprite cardDeck = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Card_Relic_Fan_Deck.png");
            Sprite headerWood = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Header_Wood_Bar_VongXuyen.png");

            // 1.1 Cập nhật ảnh nền Rừng Vọng Xuyên
            Transform bgOverlay = FindChildRecursive(rootObj.transform, "Scenery_Overlay");
            if (bgOverlay == null) bgOverlay = FindChildRecursive(rootObj.transform, "Background");
            if (bgOverlay != null && bgForest != null)
            {
                Image bgImg = bgOverlay.GetComponent<Image>();
                if (bgImg != null)
                {
                    bgImg.sprite = bgForest;
                    bgImg.color = Color.white;
                }
            }

            // 1.2 Cập nhật Khung Gỗ Đỉnh
            Transform headerBar = FindChildRecursive(rootObj.transform, "Header_TopBar");
            if (headerBar != null && headerWood != null)
            {
                RectTransform hRect = headerBar.GetComponent<RectTransform>();
                if (hRect != null)
                {
                    hRect.anchoredPosition = new Vector2(0, 0);
                    hRect.sizeDelta = new Vector2(0, 78);
                }
                Image hImg = headerBar.GetComponent<Image>();
                if (hImg != null)
                {
                    hImg.sprite = headerWood;
                    hImg.type = Image.Type.Sliced;
                    hImg.color = Color.white;
                }
            }

            // 2. Tìm hoặc tối ưu Nút Xuất Trận Lục Giác Ngọc Hổ Phách
            Transform btnStartTrans = FindChildRecursive(rootObj.transform, "Btn_StartRun");
            if (btnStartTrans == null) btnStartTrans = FindChildRecursive(rootObj.transform, "StartRunButton");
            if (btnStartTrans == null) btnStartTrans = FindChildRecursive(rootObj.transform, "Button_StartRun");

            if (btnStartTrans != null)
            {
                RectTransform rect = btnStartTrans.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(1f, 0f);
                    rect.anchorMax = new Vector2(1f, 0f);
                    rect.pivot = new Vector2(1f, 0f);
                    rect.anchoredPosition = new Vector2(-15, 6);
                    rect.sizeDelta = new Vector2(165, 86);
                }

                Image img = btnStartTrans.GetComponent<Image>();
                if (img != null && btnBattleAmber != null)
                {
                    img.sprite = btnBattleAmber;
                    img.type = Image.Type.Sliced;
                    img.color = Color.white;
                }

                TextMeshProUGUI txt = btnStartTrans.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.text = "XUẤT TRẬN";
                    txt.fontStyle = FontStyles.Bold;
                    txt.fontSize = 22;
                    txt.color = Color.white;
                    txt.alignment = TextAlignmentOptions.Center;
                }
            }

            // 3. Tối ưu Navigation Buttons (Thẻ Gỗ Khâu Chỉ: Anh Hùng, Tàng Bảo Các, Miếu Cổ)
            Transform navParent = FindChildRecursive(rootObj.transform, "Group_NavButtons");
            if (navParent == null) navParent = FindChildRecursive(rootObj.transform, "Navigation_Buttons");
            if (navParent == null) navParent = FindChildRecursive(rootObj.transform, "BottomNavigation");

            if (navParent != null)
            {
                RectTransform navRect = navParent.GetComponent<RectTransform>();
                if (navRect != null)
                {
                    navRect.anchorMin = new Vector2(0.5f, 0f);
                    navRect.anchorMax = new Vector2(0.5f, 0f);
                    navRect.pivot = new Vector2(0.5f, 0f);
                    navRect.anchoredPosition = new Vector2(0, 8);
                }

                ApplyNavButtonStyle(FindChildRecursive(navParent, "Btn_HeroSelect"), btnNavWood, "ANH HÙNG");
                ApplyNavButtonStyle(FindChildRecursive(navParent, "Btn_Armory"), btnNavWood, "TÀNG BẢO CÁC");
                ApplyNavButtonStyle(FindChildRecursive(navParent, "Btn_SanctuaryTree"), btnNavWood, "MIẾU CỔ");
            }

            // 4. Tối ưu Loadout Summary Card (Góc dưới trái)
            Transform loadoutCard = FindChildRecursive(rootObj.transform, "Tray_Loadout_Wood_Frame");
            if (loadoutCard == null) loadoutCard = FindChildRecursive(rootObj.transform, "Card_LoadoutSummary");
            if (loadoutCard == null) loadoutCard = FindChildRecursive(rootObj.transform, "Loadout_Card");

            if (loadoutCard != null)
            {
                RectTransform cardRect = loadoutCard.GetComponent<RectTransform>();
                if (cardRect != null)
                {
                    cardRect.sizeDelta = new Vector2(215, 96);
                }

                Image cardBg = loadoutCard.GetComponent<Image>();
                if (cardBg != null && trayWood != null)
                {
                    cardBg.sprite = trayWood;
                    cardBg.type = Image.Type.Sliced;
                    cardBg.color = Color.white;
                }
            }

            // 5. Cập nhật Bục Đá Lục Giác 2.5D Cổ Dưới Chân Nhân Vật
            Transform heroStage = FindChildRecursive(rootObj.transform, "Stage_HeroCenter");
            if (heroStage == null) heroStage = FindChildRecursive(rootObj.transform, "HeroStage");
            
            if (heroStage != null && pedestalHex != null)
            {
                Transform pedestalTrans = heroStage.Find("Pedestal_Hexagon_2_5D");
                if (pedestalTrans == null) pedestalTrans = heroStage.Find("Pedestal_Magic_Array");
                if (pedestalTrans == null)
                {
                    GameObject pObj = new GameObject("Pedestal_Hexagon_2_5D", typeof(RectTransform), typeof(Image));
                    pObj.transform.SetParent(heroStage, false);
                    pObj.transform.SetAsFirstSibling();
                    pedestalTrans = pObj.transform;
                }

                RectTransform pRect = pedestalTrans.GetComponent<RectTransform>();
                if (pRect != null)
                {
                    pRect.sizeDelta = new Vector2(256, 138);
                    pRect.anchoredPosition = new Vector2(0, -40);
                }

                Image pImg = pedestalTrans.GetComponent<Image>();
                if (pImg != null)
                {
                    pImg.sprite = pedestalHex;
                    pImg.color = Color.white;
                }
            }

            EditorUtility.SetDirty(rootObj);
            Debug.Log("<color=#00FF00>[MainHubOptimizer] Đã áp dụng thành công chuẩn giao diện Sảnh Rừng Vọng Xuyên khớp 100% ảnh mẫu!</color>");
        }

        private static void ApplyNavButtonStyle(Transform btnTrans, Sprite sprite, string label)
        {
            if (btnTrans == null) return;
            RectTransform rect = btnTrans.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(106, 44);
            }

            Image img = btnTrans.GetComponent<Image>();
            if (img != null && sprite != null)
            {
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }

            TextMeshProUGUI txt = btnTrans.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                if (!string.IsNullOrEmpty(label)) txt.text = label;
                txt.fontStyle = FontStyles.Bold;
                txt.fontSize = 18;
                txt.color = Color.white;
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
