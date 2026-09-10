using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI.HUD;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Tool ĐỘC LẬP CHUYÊN BIỆT: Chỉ dựng và cấu hình Đại Banner Đột Phá Chuyển Wave (Center_WaveTransitionBanner).
    /// HOÀN TOÀN KHÔNG CHẠM VÀO MiniBadge_Bamboo hay bất kỳ thành phần Top HUD nào của bạn.
    /// </summary>
    public static class EpicWaveTransitionBannerSetupTool
    {
        [MenuItem("Tools/ProjectZombie/UI/Trong Trận (In-Game Gameplay)/2.1. Thiết Lập Đại Banner Chuyển Wave (Giữa Màn Hình)", priority = 23)]
        [MenuItem("ProjectZombie/UI/2.1. Thiết Lập Đại Banner Chuyển Wave (Giữa Màn Hình)", priority = 23)]
        public static void SetupEpicWaveTransitionBannerOnly()
        {
            // 1. Tìm Canvas_Gameplay trực tiếp từ Scene
            Transform gameplayRoot = null;
            var allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var c in allCanvases)
            {
                if (c.name.Contains("Gameplay") || c.name.Contains("Master"))
                {
                    var found = c.transform.Find("Canvas_Gameplay");
                    gameplayRoot = found != null ? found : c.transform;
                    break;
                }
            }

            if (gameplayRoot == null)
            {
                Canvas mainCanvas = Object.FindFirstObjectByType<Canvas>();
                if (mainCanvas != null) gameplayRoot = mainCanvas.transform;
            }

            if (gameplayRoot == null)
            {
                Debug.LogError("[EpicWaveTransitionBannerSetupTool] Không tìm thấy Canvas trong Scene!");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(gameplayRoot.gameObject, "Setup Epic Center Wave Transition Banner");

            // 2. Load Resources (Font & Sprite Cổ Phong của người dùng)
            TMP_FontAsset vietFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Art/Font/GameFont_Vietnamese_SD.asset");
            if (vietFont == null) vietFont = Resources.Load<TMP_FontAsset>("Fonts/GameFont_Vietnamese_SD");

            Sprite customBannerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Frames/Frame_Wave_Transition_Banner_9Slice.png");
            if (customBannerSprite == null) customBannerSprite = Resources.Load<Sprite>("UI/Frames/Frame_Wave_Transition_Banner_9Slice");
            if (customBannerSprite == null) customBannerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_HUD_Timer_Kill_Wood.png");
            if (customBannerSprite == null) customBannerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Badge_Upgrade_Pill_Wood_9Slice.png");

            // 3. TẠO HOẶC TÁI SỬ DỤNG DUY NHẤT NODE: Center_WaveTransitionBanner TRỰC TIẾP DƯỚI Canvas_Gameplay
            // (TUYỆT ĐỐI KHÔNG TÌM HAY CAN THIỆP VÀO TopCenter_WaveBannerWidget / MiniBadge_Bamboo)
            Transform bannerTrans = GetOrCreateChild(gameplayRoot, "Center_WaveTransitionBanner");
            bannerTrans.SetAsLastSibling(); // Luôn hiển thị trên cùng các panel khác

            RectTransform bnRT = EnsureComponent<RectTransform>(bannerTrans.gameObject);
            bnRT.anchorMin = Vector2.zero;
            bnRT.anchorMax = Vector2.one;
            bnRT.pivot = new Vector2(0.5f, 0.5f);
            bnRT.offsetMin = Vector2.zero;
            bnRT.offsetMax = Vector2.zero;

            CanvasGroup bnCG = EnsureComponent<CanvasGroup>(bannerTrans.gameObject);
            bnCG.alpha = 0f;
            bnCG.blocksRaycasts = false;
            bnCG.interactable = false;
            bannerTrans.gameObject.SetActive(false);

            // 4.1. Lớp Phủ Nền Đen Khói (Backdrop)
            Transform dimTrans = GetOrCreateChild(bannerTrans, "Img_DimBackdrop");
            RectTransform dimRT = EnsureComponent<RectTransform>(dimTrans.gameObject);
            dimRT.anchorMin = Vector2.zero;
            dimRT.anchorMax = Vector2.one;
            dimRT.offsetMin = Vector2.zero;
            dimRT.offsetMax = Vector2.zero;
            Image dimImg = EnsureComponent<Image>(dimTrans.gameObject);
            dimImg.color = new Color(0.04f, 0.03f, 0.06f, 0.45f);
            dimImg.raycastTarget = false;

            // 4.2. Khung Nội Dung Đại Sớ (Panel 960x220 Giữa Màn Hình)
            Transform containerTrans = GetOrCreateChild(bannerTrans, "Container_EpicPanel");
            RectTransform cRT = EnsureComponent<RectTransform>(containerTrans.gameObject);
            cRT.anchorMin = new Vector2(0.5f, 0.5f);
            cRT.anchorMax = new Vector2(0.5f, 0.5f);
            cRT.pivot = new Vector2(0.5f, 0.5f);
            cRT.anchoredPosition = new Vector2(0, 0);
            cRT.sizeDelta = new Vector2(960, 220);

            // Nền Khung Banner Gỗ Mun Viền Đồng Của Người Dùng (Frame_Wave_Transition_Banner_9Slice)
            Transform bannerBgTrans = GetOrCreateChild(containerTrans, "Img_BannerBg");
            RectTransform bbgRT = EnsureComponent<RectTransform>(bannerBgTrans.gameObject);
            bbgRT.anchorMin = Vector2.zero;
            bbgRT.anchorMax = Vector2.one;
            bbgRT.sizeDelta = Vector2.zero;
            Image bbgImg = EnsureComponent<Image>(bannerBgTrans.gameObject);
            bbgImg.color = Color.white; // Giữ nguyên màu gốc tinh xảo của ảnh người dùng cung cấp
            bbgImg.type = Image.Type.Sliced;
            bbgImg.raycastTarget = false;
            if (customBannerSprite != null) bbgImg.sprite = customBannerSprite;

            // Icon Huy Hiệu
            Transform badgeTrans = GetOrCreateChild(containerTrans, "Icon_EventBadge");
            RectTransform bIconRT = EnsureComponent<RectTransform>(badgeTrans.gameObject);
            bIconRT.anchorMin = new Vector2(0.5f, 0.5f);
            bIconRT.anchorMax = new Vector2(0.5f, 0.5f);
            bIconRT.pivot = new Vector2(0.5f, 0.5f);
            bIconRT.anchoredPosition = new Vector2(0, 44);
            bIconRT.sizeDelta = new Vector2(52, 52);
            Image bIconImg = EnsureComponent<Image>(badgeTrans.gameObject);
            bIconImg.preserveAspect = true;
            bIconImg.raycastTarget = false;
            bIconImg.color = new Color(1f, 0.72f, 0.15f);

            // Tag Hồi Text
            Transform bTagTrans = GetOrCreateChild(containerTrans, "Txt_BannerTag");
            RectTransform btagRT = EnsureComponent<RectTransform>(bTagTrans.gameObject);
            btagRT.anchorMin = new Vector2(0, 0.5f);
            btagRT.anchorMax = new Vector2(1, 0.5f);
            btagRT.pivot = new Vector2(0.5f, 0.5f);
            btagRT.anchoredPosition = new Vector2(0, 78);
            btagRT.sizeDelta = new Vector2(0, 26);
            btagRT.offsetMin = new Vector2(100, btagRT.offsetMin.y);
            btagRT.offsetMax = new Vector2(-100, btagRT.offsetMax.y);
            TextMeshProUGUI bTagTMP = EnsureComponent<TextMeshProUGUI>(bTagTrans.gameObject);
            if (vietFont != null) bTagTMP.font = vietFont;
            bTagTMP.fontSize = 16;
            bTagTMP.fontStyle = FontStyles.Bold;
            bTagTMP.alignment = TextAlignmentOptions.Center;
            bTagTMP.text = "HỒI THỨ 03 / 10";
            bTagTMP.color = new Color(1f, 0.88f, 0.5f);

            // Tiêu Đề Lớn
            Transform bTitleTrans = GetOrCreateChild(containerTrans, "Txt_BannerTitle");
            RectTransform btRT = EnsureComponent<RectTransform>(bTitleTrans.gameObject);
            btRT.anchorMin = new Vector2(0, 0.5f);
            btRT.anchorMax = new Vector2(1, 0.5f);
            btRT.pivot = new Vector2(0.5f, 0.5f);
            btRT.anchoredPosition = new Vector2(0, -6);
            btRT.sizeDelta = new Vector2(0, 48);
            btRT.offsetMin = new Vector2(110, btRT.offsetMin.y);
            btRT.offsetMax = new Vector2(-110, btRT.offsetMax.y);
            TextMeshProUGUI bTitleTMP = EnsureComponent<TextMeshProUGUI>(bTitleTrans.gameObject);
            if (vietFont != null) bTitleTMP.font = vietFont;
            bTitleTMP.enableAutoSizing = true;
            bTitleTMP.fontSizeMin = 18;
            bTitleTMP.fontSizeMax = 32;
            bTitleTMP.enableWordWrapping = true;
            bTitleTMP.fontStyle = FontStyles.Bold;
            bTitleTMP.alignment = TextAlignmentOptions.Center;
            bTitleTMP.text = "BẦY QUỶ XƯƠNG BAO VÂY!";
            bTitleTMP.color = new Color(1f, 0.63f, 0f);

            // Phụ Đề
            Transform bSubTrans = GetOrCreateChild(containerTrans, "Txt_BannerSub");
            RectTransform bsRT = EnsureComponent<RectTransform>(bSubTrans.gameObject);
            bsRT.anchorMin = new Vector2(0, 0.5f);
            bsRT.anchorMax = new Vector2(1, 0.5f);
            bsRT.pivot = new Vector2(0.5f, 0.5f);
            bsRT.anchoredPosition = new Vector2(0, -56);
            bsRT.sizeDelta = new Vector2(0, 28);
            bsRT.offsetMin = new Vector2(100, bsRT.offsetMin.y);
            bsRT.offsetMax = new Vector2(-100, bsRT.offsetMax.y);
            TextMeshProUGUI bSubTMP = EnsureComponent<TextMeshProUGUI>(bSubTrans.gameObject);
            if (vietFont != null) bSubTMP.font = vietFont;
            bSubTMP.enableAutoSizing = true;
            bSubTMP.fontSizeMin = 12;
            bSubTMP.fontSizeMax = 16;
            bSubTMP.enableWordWrapping = true;
            bSubTMP.fontStyle = FontStyles.Bold;
            bSubTMP.alignment = TextAlignmentOptions.Center;
            bSubTMP.text = "BẦY QUÁI BỘC PHÁT (BURST WAVE)";
            bSubTMP.color = new Color(0.92f, 0.88f, 0.82f);

            CleanupDuplicateChildren(containerTrans);
            CleanupDuplicateChildren(bannerTrans);

            // 5. Kết nối Serialized Properties với WaveBannerWidgetView (nếu có)
            var view = Object.FindFirstObjectByType<WaveBannerWidgetView>();
            if (view != null)
            {
                SerializedObject soView = new SerializedObject(view);
                soView.FindProperty("_bannerCanvasGroup").objectReferenceValue = bnCG;
                soView.FindProperty("_bannerContainer").objectReferenceValue = cRT;
                soView.FindProperty("_bannerDimBackdrop").objectReferenceValue = dimImg;
                soView.FindProperty("_bannerFrameImage").objectReferenceValue = bbgImg;
                soView.FindProperty("_bannerIconBadge").objectReferenceValue = bIconImg;
                soView.FindProperty("_bannerTagText").objectReferenceValue = bTagTMP;
                soView.FindProperty("_bannerTitleText").objectReferenceValue = bTitleTMP;
                soView.FindProperty("_bannerSubText").objectReferenceValue = bSubTMP;
                soView.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(bannerTrans.gameObject);
            Debug.Log("<color=#4DEEEA><b>[EpicWaveTransitionBannerSetupTool]</b> Đã dựng ĐỘC LẬP Đại Banner Chuyển Wave giữa màn hình thành công (Không ảnh hưởng đến MiniBadge_Bamboo)!</color>");
        }

        private static Transform GetOrCreateChild(Transform parent, string childName)
        {
            Transform found = parent.Find(childName);
            if (found != null) return found;

            GameObject go = new GameObject(childName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            var comp = go.GetComponent<T>();
            if (comp == null) comp = go.AddComponent<T>();
            return comp;
        }

        private static void CleanupDuplicateChildren(Transform parent)
        {
            if (parent == null) return;
            var seenNames = new System.Collections.Generic.HashSet<string>();
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (seenNames.Contains(child.name))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
                else
                {
                    seenNames.Add(child.name);
                }
            }
        }
    }
}
