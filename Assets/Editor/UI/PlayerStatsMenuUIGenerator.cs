using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;
using ProjectZombie.Features.UI.StatsAndSkills;

namespace ProjectZombie.Editor.UI
{
    public static class PlayerStatsMenuUIGenerator
    {
        [MenuItem("ProjectZombie/UI/Generate In-Game Character Stats Menu")]
        public static GameObject GeneratePlayerStatsMenuPrefab()
        {
            string prefabFolder = "Assets/_Prefabs/UI";
            if (!Directory.Exists(prefabFolder)) Directory.CreateDirectory(prefabFolder);

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset");
            if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/GameFont_Vietnamese_SD.asset");
            if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/BeVietnamPro-Regular SDF.asset");
            if (font == null) font = TMP_Settings.defaultFontAsset;

            // 1. Load Visual Assets
            Sprite modalFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Modal_TangBaoCac_9Slice.png");
            Sprite headerBar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Header_Wood_Bar_VongXuyen.png");
            Sprite btnCloseX = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Nav_Close_X_Wood.png");
            Sprite cardTotem = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Card_Upgrade_Wood_Totem_9Slice.png");
            Sprite cardSubBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Frames/Card_Stat_Sub_Bg.png");
            if (cardSubBg == null) cardSubBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Badge_Upgrade_Pill_Wood_9Slice.png");
            Sprite badgePill = cardSubBg;
            Sprite btnAmber = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Battle_Hex_Amber_Glow.png");
            Sprite btnGoMun = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_GoMun_Dark.png");
            Sprite btnSonMai = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_SonMai_ChuSa.png");
            Sprite iconBoxFrame = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Box_Skill_Icon_Wood_9Slice.png");
            Sprite currencyPill = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Pill_Currency_Wood.png");

            // 2. Root GameObject
            GameObject root = new GameObject("Panel_PlayerStatsMenu", typeof(RectTransform), typeof(CanvasGroup), typeof(PlayerStatsMenuUIView), typeof(PlayerInfoUIPresenter));
            RectTransform rootRT = root.GetComponent<RectTransform>();
            SetStretchAnchor(rootRT);

            var cg = root.GetComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;

            var view = root.GetComponent<PlayerStatsMenuUIView>();
            var presenter = root.GetComponent<PlayerInfoUIPresenter>();

            // Dim Background Button
            GameObject dimObj = CreateUIElement("Dim_Background", root.transform);
            SetStretchAnchor(dimObj.GetComponent<RectTransform>());
            var dimImg = dimObj.AddComponent<Image>();
            dimImg.color = new Color(0, 0, 0, 0.75f);
            var dimBtn = dimObj.AddComponent<Button>();

            // Modal Container
            GameObject modalObj = CreateUIElement("Modal_Container", root.transform);
            RectTransform modalRT = modalObj.GetComponent<RectTransform>();
            modalRT.anchorMin = new Vector2(0.5f, 0.5f);
            modalRT.anchorMax = new Vector2(0.5f, 0.5f);
            modalRT.pivot = new Vector2(0.5f, 0.5f);
            modalRT.anchoredPosition = Vector2.zero;
            modalRT.sizeDelta = new Vector2(960, 560);
            var modalImg = modalObj.AddComponent<Image>();
            modalImg.color = Color.white;
            if (modalFrame != null)
            {
                modalImg.sprite = modalFrame;
                modalImg.type = Image.Type.Sliced;
            }

            // Header Bar
            GameObject headerObj = CreateUIElement("Header_Bar", modalObj.transform);
            RectTransform headerRT = headerObj.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0, 1);
            headerRT.anchorMax = new Vector2(1, 1);
            headerRT.pivot = new Vector2(0.5f, 1);
            headerRT.anchoredPosition = new Vector2(0, -12);
            headerRT.sizeDelta = new Vector2(-40, 56);
            var headerImg = headerObj.AddComponent<Image>();
            headerImg.color = Color.white;
            if (headerBar != null)
            {
                headerImg.sprite = headerBar;
                headerImg.type = Image.Type.Sliced;
            }

            // Title Text
            GameObject titleObj = CreateUIElement("Txt_Title", headerObj.transform);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0);
            titleRT.anchorMax = new Vector2(0.5f, 1);
            titleRT.pivot = new Vector2(0, 0.5f);
            titleRT.anchoredPosition = new Vector2(30, 0);
            titleRT.sizeDelta = new Vector2(0, 0);
            var titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
            if (font != null) titleTxt.font = font;
            titleTxt.text = "THÔNG SỐ & KHÍ VẬN";
            titleTxt.fontSize = 20;
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.alignment = TextAlignmentOptions.MidlineLeft;
            titleTxt.color = new Color(1f, 0.95f, 0.80f, 1f);

            // Currency Pill
            GameObject currObj = CreateUIElement("Pill_Currency", headerObj.transform);
            RectTransform currRT = currObj.GetComponent<RectTransform>();
            currRT.anchorMin = new Vector2(1, 0.5f);
            currRT.anchorMax = new Vector2(1, 0.5f);
            currRT.pivot = new Vector2(1, 0.5f);
            currRT.anchoredPosition = new Vector2(-70, 0);
            currRT.sizeDelta = new Vector2(180, 36);
            var currImg = currObj.AddComponent<Image>();
            currImg.color = Color.white;
            if (currencyPill != null)
            {
                currImg.sprite = currencyPill;
                currImg.type = Image.Type.Sliced;
            }

            GameObject currTextObj = CreateUIElement("Txt_Currency", currObj.transform);
            SetStretchAnchor(currTextObj.GetComponent<RectTransform>());
            var currTxt = currTextObj.AddComponent<TextMeshProUGUI>();
            if (font != null) currTxt.font = font;
            currTxt.text = "Cổ Tiền: <color=#FFD700>0</color>";
            currTxt.fontSize = 14;
            currTxt.fontStyle = FontStyles.Bold;
            currTxt.alignment = TextAlignmentOptions.Center;
            currTxt.color = new Color(1f, 0.95f, 0.80f, 1f);

            // Close Button
            GameObject closeObj = CreateUIElement("Btn_Close", headerObj.transform);
            RectTransform closeRT = closeObj.GetComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(1, 0.5f);
            closeRT.anchorMax = new Vector2(1, 0.5f);
            closeRT.pivot = new Vector2(1, 0.5f);
            closeRT.anchoredPosition = new Vector2(-12, 0);
            closeRT.sizeDelta = new Vector2(44, 44);
            var closeImg = closeObj.AddComponent<Image>();
            closeImg.color = Color.white;
            if (btnCloseX != null) closeImg.sprite = btnCloseX;
            var closeBtn = closeObj.AddComponent<Button>();

            // Content Area (3 Columns)
            GameObject contentObj = CreateUIElement("Content_Area", modalObj.transform);
            RectTransform contentRT = contentObj.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 0);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 0.5f);
            contentRT.anchoredPosition = new Vector2(0, -32);
            contentRT.sizeDelta = new Vector2(-40, -84);

            // ==========================================
            // COLUMN 1: Hero & Active Weapon (Left)
            // ==========================================
            GameObject col1Obj = CreateUIElement("Column1_HeroAndWeapon", contentObj.transform);
            RectTransform col1RT = col1Obj.GetComponent<RectTransform>();
            col1RT.anchorMin = new Vector2(0, 0);
            col1RT.anchorMax = new Vector2(0.30f, 1);
            col1RT.pivot = new Vector2(0, 0.5f);
            col1RT.anchoredPosition = Vector2.zero;
            col1RT.sizeDelta = new Vector2(-10, 0);

            // Hero Card
            GameObject heroCardObj = CreateUIElement("Hero_Card", col1Obj.transform);
            RectTransform heroCardRT = heroCardObj.GetComponent<RectTransform>();
            heroCardRT.anchorMin = new Vector2(0, 0.45f);
            heroCardRT.anchorMax = new Vector2(1, 1);
            heroCardRT.pivot = new Vector2(0.5f, 1);
            heroCardRT.anchoredPosition = Vector2.zero;
            heroCardRT.sizeDelta = Vector2.zero;
            var heroCardImg = heroCardObj.AddComponent<Image>();
            heroCardImg.color = Color.white;
            if (cardTotem != null)
            {
                heroCardImg.sprite = cardTotem;
                heroCardImg.type = Image.Type.Sliced;
            }

            // Hero Avatar Frame
            GameObject avatarBox = CreateUIElement("Box_Avatar", heroCardObj.transform);
            RectTransform abRT = avatarBox.GetComponent<RectTransform>();
            abRT.anchorMin = new Vector2(0.5f, 0.65f);
            abRT.anchorMax = new Vector2(0.5f, 0.65f);
            abRT.pivot = new Vector2(0.5f, 0.5f);
            abRT.sizeDelta = new Vector2(96, 96);
            var abImg = avatarBox.AddComponent<Image>();
            abImg.color = Color.white;
            if (iconBoxFrame != null)
            {
                abImg.sprite = iconBoxFrame;
                abImg.type = Image.Type.Sliced;
            }

            GameObject avatarObj = CreateUIElement("Img_Avatar", avatarBox.transform);
            RectTransform avatarRT = avatarObj.GetComponent<RectTransform>();
            avatarRT.anchorMin = new Vector2(0.5f, 0.5f);
            avatarRT.anchorMax = new Vector2(0.5f, 0.5f);
            avatarRT.pivot = new Vector2(0.5f, 0.5f);
            avatarRT.sizeDelta = new Vector2(80, 80);
            var avatarImg = avatarObj.AddComponent<Image>();
            avatarImg.preserveAspect = true;
            avatarImg.color = Color.white;

            // Hero Name
            GameObject heroNameObj = CreateUIElement("Txt_HeroName", heroCardObj.transform);
            RectTransform hnRT = heroNameObj.GetComponent<RectTransform>();
            hnRT.anchorMin = new Vector2(0, 0.18f);
            hnRT.anchorMax = new Vector2(1, 0.35f);
            hnRT.pivot = new Vector2(0.5f, 0.5f);
            hnRT.sizeDelta = Vector2.zero;
            var heroNameTxt = heroNameObj.AddComponent<TextMeshProUGUI>();
            if (font != null) heroNameTxt.font = font;
            heroNameTxt.text = "THƯ SINH";
            heroNameTxt.fontSize = 16;
            heroNameTxt.fontStyle = FontStyles.Bold;
            heroNameTxt.alignment = TextAlignmentOptions.Center;
            heroNameTxt.color = new Color(1f, 0.95f, 0.75f, 1f);

            // Hero Element Badge
            GameObject heroElemObj = CreateUIElement("Txt_HeroElement", heroCardObj.transform);
            RectTransform heRT = heroElemObj.GetComponent<RectTransform>();
            heRT.anchorMin = new Vector2(0, 0.04f);
            heRT.anchorMax = new Vector2(1, 0.18f);
            heRT.pivot = new Vector2(0.5f, 0.5f);
            heRT.sizeDelta = Vector2.zero;
            var heroElemTxt = heroElemObj.AddComponent<TextMeshProUGUI>();
            if (font != null) heroElemTxt.font = font;
            heroElemTxt.text = "<color=#4DEEEA>[Mộc]</color>";
            heroElemTxt.fontSize = 13;
            heroElemTxt.fontStyle = FontStyles.Bold;
            heroElemTxt.alignment = TextAlignmentOptions.Center;

            // Weapon Card
            GameObject weaponCardObj = CreateUIElement("Weapon_Card", col1Obj.transform);
            RectTransform weaponCardRT = weaponCardObj.GetComponent<RectTransform>();
            weaponCardRT.anchorMin = new Vector2(0, 0);
            weaponCardRT.anchorMax = new Vector2(1, 0.42f);
            weaponCardRT.pivot = new Vector2(0.5f, 0);
            weaponCardRT.anchoredPosition = Vector2.zero;
            weaponCardRT.sizeDelta = Vector2.zero;
            var weaponCardImg = weaponCardObj.AddComponent<Image>();
            weaponCardImg.color = Color.white;
            if (cardTotem != null)
            {
                weaponCardImg.sprite = cardTotem;
                weaponCardImg.type = Image.Type.Sliced;
            }

            // Weapon Icon Box
            GameObject wIconBox = CreateUIElement("Img_WeaponIconBox", weaponCardObj.transform);
            RectTransform wibRT = wIconBox.GetComponent<RectTransform>();
            wibRT.anchorMin = new Vector2(0, 0.5f);
            wibRT.anchorMax = new Vector2(0, 0.5f);
            wibRT.pivot = new Vector2(0, 0.5f);
            wibRT.anchoredPosition = new Vector2(14, 0);
            wibRT.sizeDelta = new Vector2(64, 64);
            var wibImg = wIconBox.AddComponent<Image>();
            wibImg.color = Color.white;
            if (iconBoxFrame != null)
            {
                wibImg.sprite = iconBoxFrame;
                wibImg.type = Image.Type.Sliced;
            }

            GameObject wIconObj = CreateUIElement("Img_WeaponIcon", wIconBox.transform);
            RectTransform wiRT = wIconObj.GetComponent<RectTransform>();
            wiRT.anchorMin = new Vector2(0.5f, 0.5f);
            wiRT.anchorMax = new Vector2(0.5f, 0.5f);
            wiRT.pivot = new Vector2(0.5f, 0.5f);
            wiRT.sizeDelta = new Vector2(48, 48);
            var wIconImg = wIconObj.AddComponent<Image>();
            wIconImg.preserveAspect = true;

            // Weapon Info Texts
            GameObject wNameObj = CreateUIElement("Txt_WeaponName", weaponCardObj.transform);
            RectTransform wnRT = wNameObj.GetComponent<RectTransform>();
            wnRT.anchorMin = new Vector2(0, 0.60f);
            wnRT.anchorMax = new Vector2(1, 0.95f);
            wnRT.pivot = new Vector2(0, 0.5f);
            wnRT.anchoredPosition = new Vector2(86, 0);
            wnRT.sizeDelta = new Vector2(-96, 0);
            var wNameTxt = wNameObj.AddComponent<TextMeshProUGUI>();
            if (font != null) wNameTxt.font = font;
            wNameTxt.text = "Bút Phán Quan";
            wNameTxt.fontSize = 14;
            wNameTxt.fontStyle = FontStyles.Bold;
            wNameTxt.color = new Color(1f, 0.85f, 0.35f, 1f);

            GameObject wLvlObj = CreateUIElement("Txt_WeaponLevel", weaponCardObj.transform);
            RectTransform wlRT = wLvlObj.GetComponent<RectTransform>();
            wlRT.anchorMin = new Vector2(0, 0.35f);
            wlRT.anchorMax = new Vector2(1, 0.62f);
            wlRT.pivot = new Vector2(0, 0.5f);
            wlRT.anchoredPosition = new Vector2(86, 0);
            wlRT.sizeDelta = new Vector2(-96, 0);
            var wLvlTxt = wLvlObj.AddComponent<TextMeshProUGUI>();
            if (font != null) wLvlTxt.font = font;
            wLvlTxt.text = "Cấp 1/5";
            wLvlTxt.fontSize = 12;
            wLvlTxt.fontStyle = FontStyles.Bold;
            wLvlTxt.color = new Color(0.3f, 0.95f, 0.9f, 1f);

            GameObject wDpsObj = CreateUIElement("Txt_WeaponDps", weaponCardObj.transform);
            RectTransform wdRT = wDpsObj.GetComponent<RectTransform>();
            wdRT.anchorMin = new Vector2(0, 0.05f);
            wdRT.anchorMax = new Vector2(1, 0.35f);
            wdRT.pivot = new Vector2(0, 0.5f);
            wdRT.anchoredPosition = new Vector2(86, 0);
            wdRT.sizeDelta = new Vector2(-96, 0);
            var wDpsTxt = wDpsObj.AddComponent<TextMeshProUGUI>();
            if (font != null) wDpsTxt.font = font;
            wDpsTxt.text = "Sát thương: 45 DPS";
            wDpsTxt.fontSize = 12;
            wDpsTxt.fontStyle = FontStyles.Bold;
            wDpsTxt.color = new Color(1f, 0.45f, 0.25f, 1f);

            // ==========================================
            // COLUMN 2: 8 Core RPG Stats (Middle)
            // ==========================================
            GameObject col2Obj = CreateUIElement("Column2_RPGStats", contentObj.transform);
            RectTransform col2RT = col2Obj.GetComponent<RectTransform>();
            col2RT.anchorMin = new Vector2(0.31f, 0);
            col2RT.anchorMax = new Vector2(0.68f, 1);
            col2RT.pivot = new Vector2(0.5f, 0.5f);
            col2RT.anchoredPosition = Vector2.zero;
            col2RT.sizeDelta = new Vector2(-10, 0);
            var col2Img = col2Obj.AddComponent<Image>();
            col2Img.color = Color.white;
            if (cardTotem != null)
            {
                col2Img.sprite = cardTotem;
                col2Img.type = Image.Type.Sliced;
            }

            GameObject statsHeader = CreateUIElement("Txt_StatsHeader", col2Obj.transform);
            RectTransform shRT = statsHeader.GetComponent<RectTransform>();
            shRT.anchorMin = new Vector2(0, 1);
            shRT.anchorMax = new Vector2(1, 1);
            shRT.pivot = new Vector2(0.5f, 1);
            shRT.anchoredPosition = new Vector2(0, -10);
            shRT.sizeDelta = new Vector2(-20, 26);
            var shTxt = statsHeader.AddComponent<TextMeshProUGUI>();
            if (font != null) shTxt.font = font;
            shTxt.text = "THUỘC TÍNH BẢN THÂN";
            shTxt.fontSize = 14;
            shTxt.fontStyle = FontStyles.Bold;
            shTxt.alignment = TextAlignmentOptions.Center;
            shTxt.color = new Color(1f, 0.85f, 0.35f, 1f);

            // Grid Container for 8 Stats
            GameObject statsGridObj = CreateUIElement("Stats_Grid", col2Obj.transform);
            RectTransform sgRT = statsGridObj.GetComponent<RectTransform>();
            sgRT.anchorMin = new Vector2(0, 0);
            sgRT.anchorMax = new Vector2(1, 1);
            sgRT.pivot = new Vector2(0.5f, 0.5f);
            sgRT.anchoredPosition = new Vector2(0, -18);
            sgRT.sizeDelta = new Vector2(-24, -46);

            var gridLayout = statsGridObj.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(150, 78);
            gridLayout.spacing = new Vector2(10, 8);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 2;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;

            // Generate 8 Stat Entries with Dark Wood & High Contrast
            StatUIEntry hpEntry = CreateStatEntry("Stat_Health", "Sinh Lực", "100 / 100", statsGridObj.transform, badgePill, font);
            StatUIEntry dmgEntry = CreateStatEntry("Stat_Damage", "Công Kích", "20.0 (x1.0)", statsGridObj.transform, badgePill, font);
            StatUIEntry critEntry = CreateStatEntry("Stat_Crit", "Bạo Kích", "10.0%", statsGridObj.transform, badgePill, font);
            StatUIEntry atkSpdEntry = CreateStatEntry("Stat_AttackSpeed", "Tốc Đánh", "1.00 đòn/s", statsGridObj.transform, badgePill, font);
            StatUIEntry spdEntry = CreateStatEntry("Stat_MoveSpeed", "Thân Pháp", "5.0 m/s", statsGridObj.transform, badgePill, font);
            StatUIEntry dashEntry = CreateStatEntry("Stat_DashCooldown", "Phi Vân", "1.5s", statsGridObj.transform, badgePill, font);
            StatUIEntry pickupEntry = CreateStatEntry("Stat_PickupRange", "Thu Hút", "3.0m", statsGridObj.transform, badgePill, font);
            StatUIEntry expEntry = CreateStatEntry("Stat_ExpMultiplier", "Ngộ Tính", "+0%", statsGridObj.transform, badgePill, font);

            // ==========================================
            // COLUMN 3: Passives & Controls (Right)
            // ==========================================
            GameObject col3Obj = CreateUIElement("Column3_PassivesAndControls", contentObj.transform);
            RectTransform col3RT = col3Obj.GetComponent<RectTransform>();
            col3RT.anchorMin = new Vector2(0.69f, 0);
            col3RT.anchorMax = new Vector2(1, 1);
            col3RT.pivot = new Vector2(1, 0.5f);
            col3RT.anchoredPosition = Vector2.zero;
            col3RT.sizeDelta = new Vector2(-10, 0);

            // Run Stats Card (Top)
            GameObject runStatsCard = CreateUIElement("RunStats_Card", col3Obj.transform);
            RectTransform rscRT = runStatsCard.GetComponent<RectTransform>();
            rscRT.anchorMin = new Vector2(0, 0.70f);
            rscRT.anchorMax = new Vector2(1, 1);
            rscRT.pivot = new Vector2(0.5f, 1);
            rscRT.anchoredPosition = Vector2.zero;
            rscRT.sizeDelta = Vector2.zero;
            var rscImg = runStatsCard.AddComponent<Image>();
            rscImg.color = Color.white;
            if (cardTotem != null)
            {
                rscImg.sprite = cardTotem;
                rscImg.type = Image.Type.Sliced;
            }

            GameObject timerObj = CreateUIElement("Txt_Timer", runStatsCard.transform);
            RectTransform tRT = timerObj.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0, 0.5f);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.pivot = new Vector2(0.5f, 0.5f);
            tRT.anchoredPosition = new Vector2(16, 0);
            tRT.sizeDelta = new Vector2(-32, 0);
            var timerTxt = timerObj.AddComponent<TextMeshProUGUI>();
            if (font != null) timerTxt.font = font;
            timerTxt.text = "Thời Gian: <color=#00FF88>00:00</color>";
            timerTxt.fontSize = 13;
            timerTxt.fontStyle = FontStyles.Bold;
            timerTxt.alignment = TextAlignmentOptions.MidlineLeft;
            timerTxt.color = new Color(0.95f, 0.85f, 0.55f, 1f);

            GameObject killObj = CreateUIElement("Txt_Kills", runStatsCard.transform);
            RectTransform kRT = killObj.GetComponent<RectTransform>();
            kRT.anchorMin = new Vector2(0, 0);
            kRT.anchorMax = new Vector2(1, 0.5f);
            kRT.pivot = new Vector2(0.5f, 0.5f);
            kRT.anchoredPosition = new Vector2(16, 0);
            kRT.sizeDelta = new Vector2(-32, 0);
            var killTxt = killObj.AddComponent<TextMeshProUGUI>();
            if (font != null) killTxt.font = font;
            killTxt.text = "Diệt Quái: <color=#FF5722>0</color>";
            killTxt.fontSize = 13;
            killTxt.fontStyle = FontStyles.Bold;
            killTxt.alignment = TextAlignmentOptions.MidlineLeft;
            killTxt.color = new Color(0.95f, 0.85f, 0.55f, 1f);

            // Passives Container (Middle)
            GameObject passivesBox = CreateUIElement("Passives_Box", col3Obj.transform);
            RectTransform pbRT = passivesBox.GetComponent<RectTransform>();
            pbRT.anchorMin = new Vector2(0, 0.38f);
            pbRT.anchorMax = new Vector2(1, 0.68f);
            pbRT.pivot = new Vector2(0.5f, 0.5f);
            pbRT.anchoredPosition = Vector2.zero;
            pbRT.sizeDelta = Vector2.zero;
            var pbImg = passivesBox.AddComponent<Image>();
            pbImg.color = Color.white;
            if (cardTotem != null)
            {
                pbImg.sprite = cardTotem;
                pbImg.type = Image.Type.Sliced;
            }

            GameObject passivesTitle = CreateUIElement("Txt_PassivesTitle", passivesBox.transform);
            RectTransform ptRT = passivesTitle.GetComponent<RectTransform>();
            ptRT.anchorMin = new Vector2(0, 1);
            ptRT.anchorMax = new Vector2(1, 1);
            ptRT.pivot = new Vector2(0.5f, 1);
            ptRT.anchoredPosition = new Vector2(0, -6);
            ptRT.sizeDelta = new Vector2(-20, 20);
            var ptTxt = passivesTitle.AddComponent<TextMeshProUGUI>();
            if (font != null) ptTxt.font = font;
            ptTxt.text = "BÙA CHÚ ĐÃ NHẶT";
            ptTxt.fontSize = 11;
            ptTxt.fontStyle = FontStyles.Bold;
            ptTxt.alignment = TextAlignmentOptions.Center;
            ptTxt.color = new Color(1f, 0.85f, 0.35f, 1f);

            GameObject passivesContainer = CreateUIElement("Passives_Container", passivesBox.transform);
            RectTransform pcRT = passivesContainer.GetComponent<RectTransform>();
            pcRT.anchorMin = new Vector2(0, 0);
            pcRT.anchorMax = new Vector2(1, 1);
            pcRT.pivot = new Vector2(0.5f, 0.5f);
            pcRT.anchoredPosition = new Vector2(0, -10);
            pcRT.sizeDelta = new Vector2(-20, -30);

            var passivesLayout = passivesContainer.AddComponent<GridLayoutGroup>();
            passivesLayout.cellSize = new Vector2(40, 40);
            passivesLayout.spacing = new Vector2(6, 6);
            passivesLayout.childAlignment = TextAnchor.UpperCenter;

            // Pause Buttons (Bottom)
            GameObject resumeBtnObj = CreateButton("Btn_Resume", "TIẾP TỤC", col3Obj.transform, new Vector2(0, 0.25f), new Vector2(1, 0.36f), btnAmber, font, new Color(1f, 0.95f, 0.80f, 1f));
            Button resumeBtn = resumeBtnObj.GetComponent<Button>();

            GameObject settingsBtnObj = CreateButton("Btn_Settings", "CÀI ĐẶT", col3Obj.transform, new Vector2(0, 0.13f), new Vector2(1, 0.23f), btnGoMun, font, new Color(0.92f, 0.88f, 0.80f, 1f));
            Button settingsBtn = settingsBtnObj.GetComponent<Button>();

            GameObject quitBtnObj = CreateButton("Btn_Quit", "BỎ CUỘC (VỀ SẢNH)", col3Obj.transform, new Vector2(0, 0.01f), new Vector2(1, 0.11f), btnSonMai, font, new Color(1f, 0.90f, 0.40f, 1f));
            Button quitBtn = quitBtnObj.GetComponent<Button>();

            // 3. Serialize Fields into PlayerStatsMenuUIView & PlayerInfoUIPresenter
            SerializedObject soView = new SerializedObject(view);

            soView.FindProperty("_dimBackgroundButton").objectReferenceValue = dimBtn;
            soView.FindProperty("_closeButton").objectReferenceValue = closeBtn;
            soView.FindProperty("_titleText").objectReferenceValue = titleTxt;
            soView.FindProperty("_currencyText").objectReferenceValue = currTxt;

            soView.FindProperty("_heroAvatarImage").objectReferenceValue = avatarImg;
            soView.FindProperty("_heroNameText").objectReferenceValue = heroNameTxt;
            soView.FindProperty("_heroElementBadgeText").objectReferenceValue = heroElemTxt;

            soView.FindProperty("_weaponIconImage").objectReferenceValue = wIconImg;
            soView.FindProperty("_weaponNameText").objectReferenceValue = wNameTxt;
            soView.FindProperty("_weaponLevelText").objectReferenceValue = wLvlTxt;
            soView.FindProperty("_weaponDpsText").objectReferenceValue = wDpsTxt;

            soView.FindProperty("_healthStatEntry").objectReferenceValue = hpEntry;
            soView.FindProperty("_damageStatEntry").objectReferenceValue = dmgEntry;
            soView.FindProperty("_critStatEntry").objectReferenceValue = critEntry;
            soView.FindProperty("_attackSpeedStatEntry").objectReferenceValue = atkSpdEntry;
            soView.FindProperty("_moveSpeedStatEntry").objectReferenceValue = spdEntry;
            soView.FindProperty("_dashCooldownStatEntry").objectReferenceValue = dashEntry;
            soView.FindProperty("_pickupRangeStatEntry").objectReferenceValue = pickupEntry;
            soView.FindProperty("_expMultiplierStatEntry").objectReferenceValue = expEntry;

            soView.FindProperty("_timerText").objectReferenceValue = timerTxt;
            soView.FindProperty("_killCountText").objectReferenceValue = killTxt;
            soView.FindProperty("_passivesContainer").objectReferenceValue = passivesContainer.transform;

            soView.FindProperty("_resumeButton").objectReferenceValue = resumeBtn;
            soView.FindProperty("_settingsButton").objectReferenceValue = settingsBtn;
            soView.FindProperty("_quitButton").objectReferenceValue = quitBtn;

            soView.ApplyModifiedProperties();

            SerializedObject soPresenter = new SerializedObject(presenter);
            soPresenter.FindProperty("_statsMenuView").objectReferenceValue = view;
            soPresenter.ApplyModifiedProperties();

            // Save Prefab
            string prefabPath = $"{prefabFolder}/PlayerStatsMenuUI.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            GameObject.DestroyImmediate(root);

            Debug.Log($"<color=#00FF88>[PlayerStatsMenuUIGenerator]</color> Đã sinh thành công Prefab tại {prefabPath}");
            return prefab;
        }

        private static StatUIEntry CreateStatEntry(string name, string statName, string defaultValue, Transform parent, Sprite bgSprite, TMP_FontAsset font)
        {
            GameObject entryObj = CreateUIElement(name, parent);
            var img = entryObj.AddComponent<Image>();
            if (bgSprite != null)
            {
                img.sprite = bgSprite;
                img.type = Image.Type.Sliced;
                img.color = new Color(0.15f, 0.10f, 0.08f, 0.95f);
            }
            else
            {
                img.color = new Color(0.18f, 0.12f, 0.08f, 0.95f);
            }

            GameObject titleObj = CreateUIElement("Txt_StatName", entryObj.transform);
            RectTransform tRT = titleObj.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0, 0.52f);
            tRT.anchorMax = new Vector2(1, 1);
            tRT.pivot = new Vector2(0.5f, 0.5f);
            tRT.anchoredPosition = new Vector2(0, -3);
            tRT.sizeDelta = new Vector2(-16, 0);
            var titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
            if (font != null) titleTxt.font = font;
            titleTxt.text = statName;
            titleTxt.fontSize = 12;
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.alignment = TextAlignmentOptions.Center;
            titleTxt.color = new Color(0.96f, 0.86f, 0.58f, 1f);

            GameObject valObj = CreateUIElement("Txt_StatValue", entryObj.transform);
            RectTransform vRT = valObj.GetComponent<RectTransform>();
            vRT.anchorMin = new Vector2(0, 0);
            vRT.anchorMax = new Vector2(1, 0.52f);
            vRT.pivot = new Vector2(0.5f, 0.5f);
            vRT.anchoredPosition = new Vector2(0, 3);
            vRT.sizeDelta = new Vector2(-16, 0);
            var valTxt = valObj.AddComponent<TextMeshProUGUI>();
            if (font != null) valTxt.font = font;
            valTxt.text = defaultValue;
            valTxt.fontSize = 15;
            valTxt.fontStyle = FontStyles.Bold;
            valTxt.alignment = TextAlignmentOptions.Center;
            valTxt.color = new Color(0.0f, 1.0f, 0.65f, 1f);

            StatUIEntry entry = entryObj.AddComponent<StatUIEntry>();
            SerializedObject so = new SerializedObject(entry);
            so.FindProperty("_statNameText").objectReferenceValue = titleTxt;
            so.FindProperty("_statValueText").objectReferenceValue = valTxt;
            so.ApplyModifiedProperties();

            return entry;
        }

        private static GameObject CreateButton(string name, string text, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Sprite sprite, TMP_FontAsset font, Color textColor)
        {
            GameObject btnObj = CreateUIElement(name, parent);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            var img = btnObj.AddComponent<Image>();
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.2f, 0.15f, 0.10f, 0.95f);
            }
            btnObj.AddComponent<Button>();

            GameObject textObj = CreateUIElement("Text", btnObj.transform);
            SetStretchAnchor(textObj.GetComponent<RectTransform>());
            var txt = textObj.AddComponent<TextMeshProUGUI>();
            if (font != null) txt.font = font;
            txt.text = text;
            txt.fontSize = 12;
            txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = textColor;

            return btnObj;
        }

        [MenuItem("Tools/ProjectZombie/UI/⚡ Rebuild In-Game Character Stats Menu", priority = 21)]
        public static void RebuildPlayerStatsMenuUI()
        {
            GameObject prefab = GeneratePlayerStatsMenuPrefab();
            if (prefab == null) return;

            Canvas mainCanvas = Object.FindAnyObjectByType<Canvas>();
            if (mainCanvas == null) return;

            Transform gameRoot = mainCanvas.transform.Find("Canvas_Gameplay");
            if (gameRoot == null) gameRoot = mainCanvas.transform;

            Transform existingPanel = gameRoot.Find("Panel_PlayerStatsMenu");
            if (existingPanel == null) existingPanel = mainCanvas.transform.Find("Panel_PlayerStatsMenu");

            if (existingPanel != null)
            {
                Object.DestroyImmediate(existingPanel.gameObject);
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, gameRoot);
            instance.name = "Panel_PlayerStatsMenu";
            instance.SetActive(false);

            var presenter = instance.GetComponent<PlayerInfoUIPresenter>();

            // Wire into GameplayBootstrapper
            var bootstrapper = Object.FindAnyObjectByType<Features.Player.GameplayBootstrapper>();
            if (bootstrapper != null)
            {
                var soBoot = new SerializedObject(bootstrapper);
                var pProp = soBoot.FindProperty("playerInfoUIPresenter");
                if (pProp != null) pProp.objectReferenceValue = presenter;
                soBoot.ApplyModifiedProperties();
            }

            // Đảm bảo Modal_Settings có mặt trong Scene
            Transform existingSettings = mainCanvas.transform.Find("Modal_Settings");
            if (existingSettings == null && gameRoot != null) existingSettings = gameRoot.Find("Modal_Settings");
            if (existingSettings == null)
            {
                var settingsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/UI/SettingsModalUI.prefab");
                if (settingsPrefab != null)
                {
                    var setObj = (GameObject)PrefabUtility.InstantiatePrefab(settingsPrefab, mainCanvas.transform);
                    setObj.name = "Modal_Settings";
                    setObj.SetActive(false);
                }
            }

            EditorUtility.SetDirty(mainCanvas);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mainCanvas.gameObject.scene);

            Debug.Log("<color=#00FF88>[PlayerStatsMenuUIGenerator]</color> ĐÃ DỰNG THÀNH CÔNG VÀ KẾT NỐI BẢNG THÔNG SỐ NHÂN VẬT 3 CỘT VÀO SCENE!");
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static void SetStretchAnchor(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }
    }
}
