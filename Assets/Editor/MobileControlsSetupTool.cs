using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.UI;

namespace ProjectZombie.Editor.Tools
{
    /// <summary>
    /// Editor Tool giúp tự động quét, chuẩn hóa và gắn kết (Auto-Wire) các thành phần Mobile Controls Canvas:
    /// - DynamicVirtualJoystick
    /// - SignatureSkillButtonView & SignatureSkillPresenter
    /// - DashButtonView
    /// Tuân thủ quy chuẩn UI Art Guide & MVP Pattern.
    /// </summary>
    public class MobileControlsSetupTool : EditorWindow
    {
        [MenuItem("Tools/ProjectZombie/Mobile Controls Setup & Auto-Wire")]
        public static void ShowWindow()
        {
            var window = GetWindow<MobileControlsSetupTool>("Mobile Controls Setup");
            window.minSize = new Vector2(420, 480);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("🎮 Mobile Controls Setup & Auto-Wire", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Tool này sẽ tìm kiếm 'Panel_MobileControls' (hoặc Canvas hiện có) trong Scene đang mở để:\n" +
                "1. Tự động kiểm tra và gắn các Script MVP còn thiếu.\n" +
                "2. Tự động liên kết (Wire) các biến SerializedField (Image, TextMeshProUGUI, Button, CanvasGroup).\n" +
                "3. Chuẩn hóa Canvas Scaler (1920x1080 Match 0.5) và Anchors mà không làm mất các Sprite đã kéo.",
                MessageType.Info
            );

            EditorGUILayout.Space(15);

            if (GUILayout.Button("⚡ Tự Động Quét & Chuẩn Hóa Scene Hiện Tại", GUILayout.Height(40)))
            {
                SetupAndWireControlsInScene();
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("🛠️ Tạo Mới Panel_MobileControls (Nếu Chưa Có)", GUILayout.Height(30)))
            {
                CreateDefaultMobileControlsHierarchy();
            }
        }

        private static void SetupAndWireControlsInScene()
        {
            // 1. Tìm hoặc kiểm tra Canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy Canvas nào trong Scene! Vui lòng tạo Canvas trước.", "OK");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, "Setup Mobile Controls");

            // Đảm bảo Canvas Scaler chuẩn 1920x1080 Match 0.5
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                EditorUtility.SetDirty(scaler);
            }

            // 2. Tìm Panel_MobileControls hoặc root phù hợp
            GameObject mobilePanel = GameObject.Find("Panel_MobileControls");
            if (mobilePanel == null)
            {
                // Tìm kiếm theo tên gần đúng
                foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name.ToLower().Contains("mobilecontrol") || t.name.ToLower().Contains("touchcontrol"))
                    {
                        mobilePanel = t.gameObject;
                        break;
                    }
                }
            }

            if (mobilePanel == null)
            {
                bool createNew = EditorUtility.DisplayDialog(
                    "Không tìm thấy Panel_MobileControls",
                    "Chưa tìm thấy 'Panel_MobileControls' trong Scene. Bạn có muốn Tool tự tạo cấu trúc chuẩn ngay bây giờ?",
                    "Tạo Mới", "Hủy"
                );
                if (createNew)
                {
                    CreateDefaultMobileControlsHierarchy();
                }
                return;
            }

            int wiredCount = 0;

            // 3. Chuẩn hóa & Wire DynamicVirtualJoystick
            DynamicVirtualJoystick joystick = mobilePanel.GetComponentInChildren<DynamicVirtualJoystick>(true);
            if (joystick != null)
            {
                WireJoystick(joystick);
                wiredCount++;
            }
            else
            {
                // Thử tìm GameObject có tên Joystick
                Transform joyTransform = FindChildRecursive(mobilePanel.transform, "Joystick");
                if (joyTransform != null)
                {
                    joystick = joyTransform.gameObject.AddComponent<DynamicVirtualJoystick>();
                    WireJoystick(joystick);
                    wiredCount++;
                }
            }

            // 4. Chuẩn hóa & Wire SignatureSkillButtonView & Presenter
            SignatureSkillButtonView skillView = mobilePanel.GetComponentInChildren<SignatureSkillButtonView>(true);
            Transform skillTransform = skillView != null ? skillView.transform : FindChildRecursive(mobilePanel.transform, "Skill");
            if (skillTransform != null)
            {
                if (skillView == null) skillView = skillTransform.gameObject.AddComponent<SignatureSkillButtonView>();
                var presenter = skillTransform.GetComponent<SignatureSkillPresenter>();
                if (presenter == null) presenter = skillTransform.gameObject.AddComponent<SignatureSkillPresenter>();

                WireSignatureSkill(skillView, presenter);
                wiredCount++;
            }

            // 5. Chuẩn hóa & Wire DashButtonView & DashButtonPresenter
            DashButtonView dashView = mobilePanel.GetComponentInChildren<DashButtonView>(true);
            Transform dashTransform = dashView != null ? dashView.transform : FindChildRecursive(mobilePanel.transform, "Dash");
            if (dashTransform != null)
            {
                if (dashView == null) dashView = dashTransform.gameObject.AddComponent<DashButtonView>();
                var dashPresenter = dashTransform.GetComponent<DashButtonPresenter>();
                if (dashPresenter == null) dashPresenter = dashTransform.gameObject.AddComponent<DashButtonPresenter>();

                WireDashButton(dashView, dashPresenter);
                wiredCount++;
            }

            // 6. Chuẩn hóa & Wire AttackButtonView & AttackButtonPresenter
            AttackButtonView attackView = mobilePanel.GetComponentInChildren<AttackButtonView>(true);
            Transform attackTransform = attackView != null ? attackView.transform : FindChildRecursive(mobilePanel.transform, "Attack");
            if (attackTransform != null)
            {
                if (attackView == null) attackView = attackTransform.gameObject.AddComponent<AttackButtonView>();
                var attackPresenter = attackTransform.GetComponent<AttackButtonPresenter>();
                if (attackPresenter == null) attackPresenter = attackTransform.gameObject.AddComponent<AttackButtonPresenter>();

                WireAttackButton(attackView, attackPresenter);
                wiredCount++;
            }

            EditorUtility.SetDirty(mobilePanel);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mobilePanel.scene);

            EditorUtility.DisplayDialog(
                "Hoàn Tất",
                $"Đã hoàn tất quét và Auto-Wire các thành phần Mobile Controls!\nSố cụm được cấu hình: {wiredCount}",
                "OK"
            );
        }

        private static void WireJoystick(DynamicVirtualJoystick joystick)
        {
            var so = new SerializedObject(joystick);
            RectTransform container = joystick.GetComponent<RectTransform>();
            RectTransform handle = null;

            foreach (RectTransform child in container)
            {
                if (child.name.ToLower().Contains("handle") || child.name.ToLower().Contains("knob") || child.name.ToLower().Contains("point"))
                {
                    handle = child;
                    break;
                }
            }

            if (handle == null && container.childCount > 0)
            {
                handle = container.GetChild(0) as RectTransform;
            }

            so.FindProperty("containerRect").objectReferenceValue = container;
            so.FindProperty("handleRect").objectReferenceValue = handle;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(joystick);
            Debug.Log($"[MobileControlsSetupTool] Đã Auto-Wire Joystick: {joystick.name} (Handle: {(handle != null ? handle.name : "None")})");
        }

        private static void WireSignatureSkill(SignatureSkillButtonView view, SignatureSkillPresenter presenter)
        {
            var soView = new SerializedObject(view);

            // Tìm Button
            Button btn = view.GetComponent<Button>();
            if (btn == null) btn = view.GetComponentInChildren<Button>(true);
            soView.FindProperty("_skillButton").objectReferenceValue = btn;

            // Tìm Cooldown Image Fill (Image có ImageType = Filled)
            Image fillImage = null;
            Image[] images = view.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.gameObject == view.gameObject && images.Length > 1) continue;
                if (img.type == Image.Type.Filled || img.name.ToLower().Contains("cooldown") || img.name.ToLower().Contains("fill") || img.name.ToLower().Contains("radial"))
                {
                    fillImage = img;
                    break;
                }
            }
            if (fillImage != null)
            {
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Radial360;
                fillImage.fillOrigin = (int)Image.Origin360.Top;
                fillImage.fillClockwise = false;
                soView.FindProperty("_cooldownRadialFill").objectReferenceValue = fillImage;
            }

            // Tìm TextMeshProUGUI
            TextMeshProUGUI cdText = view.GetComponentInChildren<TextMeshProUGUI>(true);
            soView.FindProperty("_cooldownText").objectReferenceValue = cdText;

            // Tìm CanvasGroup
            CanvasGroup cg = view.GetComponent<CanvasGroup>();
            if (cg == null) cg = view.gameObject.AddComponent<CanvasGroup>();
            soView.FindProperty("_canvasGroup").objectReferenceValue = cg;

            soView.ApplyModifiedProperties();
            EditorUtility.SetDirty(view);

            // Wire Presenter
            var soPresenter = new SerializedObject(presenter);
            soPresenter.FindProperty("_buttonView").objectReferenceValue = view;
            
            // Tìm ThuSinh Overlay nếu có trong Canvas
            var overlay = FindObjectOfType<ThuSinhElementPickerOverlayView>();
            if (overlay != null)
            {
                soPresenter.FindProperty("_elementPickerOverlayView").objectReferenceValue = overlay;
            }

            soPresenter.ApplyModifiedProperties();
            EditorUtility.SetDirty(presenter);

            Debug.Log($"[MobileControlsSetupTool] Đã Auto-Wire SignatureSkill: {view.name}");
        }

        private static void WireDashButton(DashButtonView view, DashButtonPresenter presenter)
        {
            var soView = new SerializedObject(view);

            Button btn = view.GetComponent<Button>();
            if (btn == null) btn = view.GetComponentInChildren<Button>(true);
            soView.FindProperty("_dashButton").objectReferenceValue = btn;

            Image fillImage = null;
            Image[] images = view.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.gameObject == view.gameObject && images.Length > 1) continue;
                if (img.type == Image.Type.Filled || img.name.ToLower().Contains("cooldown") || img.name.ToLower().Contains("fill") || img.name.ToLower().Contains("radial"))
                {
                    fillImage = img;
                    break;
                }
            }
            if (fillImage != null)
            {
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Radial360;
                fillImage.fillOrigin = (int)Image.Origin360.Top;
                fillImage.fillClockwise = false;
                soView.FindProperty("_cooldownRadialFill").objectReferenceValue = fillImage;
            }

            TextMeshProUGUI cdText = view.GetComponentInChildren<TextMeshProUGUI>(true);
            soView.FindProperty("_cooldownText").objectReferenceValue = cdText;

            CanvasGroup cg = view.GetComponent<CanvasGroup>();
            if (cg == null) cg = view.gameObject.AddComponent<CanvasGroup>();
            soView.FindProperty("_canvasGroup").objectReferenceValue = cg;

            soView.ApplyModifiedProperties();
            EditorUtility.SetDirty(view);

            var soPresenter = new SerializedObject(presenter);
            soPresenter.FindProperty("_view").objectReferenceValue = view;
            soPresenter.ApplyModifiedProperties();
            EditorUtility.SetDirty(presenter);

            Debug.Log($"[MobileControlsSetupTool] Đã Auto-Wire DashButton: {view.name}");
        }

        private static void WireAttackButton(AttackButtonView view, AttackButtonPresenter presenter)
        {
            var soView = new SerializedObject(view);

            Button btn = view.GetComponent<Button>();
            if (btn == null) btn = view.GetComponentInChildren<Button>(true);
            soView.FindProperty("_attackButton").objectReferenceValue = btn;

            Image[] images = view.GetComponentsInChildren<Image>(true);
            Image fillImage = null;
            Image iconImage = null;

            foreach (var img in images)
            {
                if (img.gameObject == view.gameObject && images.Length > 1) continue;
                if (img.type == Image.Type.Filled || img.name.ToLower().Contains("cooldown") || img.name.ToLower().Contains("fill") || img.name.ToLower().Contains("radial"))
                {
                    fillImage = img;
                }
                else if (img.name.ToLower().Contains("icon") || img.name.ToLower().Contains("weapon") || img.name.ToLower().Contains("art"))
                {
                    iconImage = img;
                }
            }

            if (fillImage != null)
            {
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Radial360;
                fillImage.fillOrigin = (int)Image.Origin360.Top;
                fillImage.fillClockwise = false;
                soView.FindProperty("_cooldownRadialFill").objectReferenceValue = fillImage;
            }

            if (iconImage != null)
            {
                soView.FindProperty("_iconImage").objectReferenceValue = iconImage;
            }

            CanvasGroup cg = view.GetComponent<CanvasGroup>();
            if (cg == null) cg = view.gameObject.AddComponent<CanvasGroup>();
            soView.FindProperty("_canvasGroup").objectReferenceValue = cg;

            soView.ApplyModifiedProperties();
            EditorUtility.SetDirty(view);

            var soPresenter = new SerializedObject(presenter);
            soPresenter.FindProperty("_view").objectReferenceValue = view;
            soPresenter.ApplyModifiedProperties();
            EditorUtility.SetDirty(presenter);

            Debug.Log($"[MobileControlsSetupTool] Đã Auto-Wire AttackButton: {view.name}");
        }

        private static void CreateDefaultMobileControlsHierarchy()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas_Gameplay", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var s = canvasObj.GetComponent<CanvasScaler>();
                s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                s.referenceResolution = new Vector2(1920, 1080);
                s.matchWidthOrHeight = 0.5f;
            }

            GameObject mobilePanel = new GameObject("Panel_MobileControls", typeof(RectTransform));
            mobilePanel.transform.SetParent(canvas.transform, false);
            RectTransform panelRect = mobilePanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            // 1. Joystick
            GameObject joyObj = new GameObject("DynamicVirtualJoystick", typeof(RectTransform), typeof(Image), typeof(DynamicVirtualJoystick));
            joyObj.transform.SetParent(mobilePanel.transform, false);
            RectTransform joyRect = joyObj.GetComponent<RectTransform>();
            joyRect.anchorMin = new Vector2(0f, 0f);
            joyRect.anchorMax = new Vector2(0f, 0f);
            joyRect.pivot = new Vector2(0.5f, 0.5f);
            joyRect.anchoredPosition = new Vector2(250, 250);
            joyRect.sizeDelta = new Vector2(240, 240);
            Image joyBg = joyObj.GetComponent<Image>();
            joyBg.color = Color.white;
            Sprite joyBaseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Joystick/Joystick_Base_DongSon.png");
            if (joyBaseSprite != null) joyBg.sprite = joyBaseSprite;

            GameObject handleObj = new GameObject("JoystickHandle", typeof(RectTransform), typeof(Image));
            handleObj.transform.SetParent(joyObj.transform, false);
            RectTransform handleRect = handleObj.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(100, 100);
            Image handleImg = handleObj.GetComponent<Image>();
            handleImg.color = Color.white;
            Sprite joyKnobSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Joystick/Joystick_Knob_Taiji.png");
            if (joyKnobSprite != null) handleImg.sprite = joyKnobSprite;

            // 2. Attack Button (Nút Đánh Chính - Kích thước lớn nhất)
            GameObject attackObj = new GameObject("Btn_Attack", typeof(RectTransform), typeof(Image), typeof(Button), typeof(CanvasGroup), typeof(AttackButtonView), typeof(AttackButtonPresenter));
            attackObj.transform.SetParent(mobilePanel.transform, false);
            RectTransform attackRect = attackObj.GetComponent<RectTransform>();
            attackRect.anchorMin = new Vector2(1f, 0f);
            attackRect.anchorMax = new Vector2(1f, 0f);
            attackRect.pivot = new Vector2(0.5f, 0.5f);
            attackRect.anchoredPosition = new Vector2(-150, 150);
            attackRect.sizeDelta = new Vector2(140, 140);
            Image attackBg = attackObj.GetComponent<Image>();
            attackBg.color = new Color(0.85f, 0.25f, 0.2f, 0.95f);

            GameObject attackIconObj = new GameObject("Icon_Weapon", typeof(RectTransform), typeof(Image));
            attackIconObj.transform.SetParent(attackObj.transform, false);
            RectTransform attackIconRect = attackIconObj.GetComponent<RectTransform>();
            attackIconRect.anchorMin = new Vector2(0.15f, 0.15f);
            attackIconRect.anchorMax = new Vector2(0.85f, 0.85f);
            attackIconRect.sizeDelta = Vector2.zero;

            GameObject attackFillObj = new GameObject("CooldownFill", typeof(RectTransform), typeof(Image));
            attackFillObj.transform.SetParent(attackObj.transform, false);
            RectTransform attackFillRect = attackFillObj.GetComponent<RectTransform>();
            attackFillRect.anchorMin = Vector2.zero;
            attackFillRect.anchorMax = Vector2.one;
            attackFillRect.sizeDelta = Vector2.zero;
            Image attackFillImg = attackFillObj.GetComponent<Image>();
            attackFillImg.color = new Color(0f, 0f, 0f, 0.65f);
            attackFillImg.type = Image.Type.Filled;
            attackFillImg.fillMethod = Image.FillMethod.Radial360;

            // 3. Skill Button (Nút Tuyệt Kỹ)
            GameObject skillObj = new GameObject("Btn_SignatureSkill", typeof(RectTransform), typeof(Image), typeof(Button), typeof(CanvasGroup), typeof(SignatureSkillButtonView), typeof(SignatureSkillPresenter));
            skillObj.transform.SetParent(mobilePanel.transform, false);
            RectTransform skillRect = skillObj.GetComponent<RectTransform>();
            skillRect.anchorMin = new Vector2(1f, 0f);
            skillRect.anchorMax = new Vector2(1f, 0f);
            skillRect.pivot = new Vector2(0.5f, 0.5f);
            skillRect.anchoredPosition = new Vector2(-150, 310);
            skillRect.sizeDelta = new Vector2(100, 100);
            Image skillBg = skillObj.GetComponent<Image>();
            skillBg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            GameObject skillFillObj = new GameObject("CooldownFill", typeof(RectTransform), typeof(Image));
            skillFillObj.transform.SetParent(skillObj.transform, false);
            RectTransform skillFillRect = skillFillObj.GetComponent<RectTransform>();
            skillFillRect.anchorMin = Vector2.zero;
            skillFillRect.anchorMax = Vector2.one;
            skillFillRect.sizeDelta = Vector2.zero;
            Image skillFillImg = skillFillObj.GetComponent<Image>();
            skillFillImg.color = new Color(0f, 0f, 0f, 0.65f);
            skillFillImg.type = Image.Type.Filled;
            skillFillImg.fillMethod = Image.FillMethod.Radial360;

            GameObject skillTextObj = new GameObject("Txt_Cooldown", typeof(RectTransform), typeof(TextMeshProUGUI));
            skillTextObj.transform.SetParent(skillObj.transform, false);
            RectTransform skillTextRect = skillTextObj.GetComponent<RectTransform>();
            skillTextRect.anchorMin = Vector2.zero;
            skillTextRect.anchorMax = Vector2.one;
            skillTextRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI skillText = skillTextObj.GetComponent<TextMeshProUGUI>();
            skillText.alignment = TextAlignmentOptions.Center;
            skillText.fontSize = 28;

            // 4. Dash Button (Nút Lướt)
            GameObject dashObj = new GameObject("Btn_Dash", typeof(RectTransform), typeof(Image), typeof(Button), typeof(DashButtonView), typeof(DashButtonPresenter));
            dashObj.transform.SetParent(mobilePanel.transform, false);
            RectTransform dashRect = dashObj.GetComponent<RectTransform>();
            dashRect.anchorMin = new Vector2(1f, 0f);
            dashRect.anchorMax = new Vector2(1f, 0f);
            dashRect.pivot = new Vector2(0.5f, 0.5f);
            dashRect.anchoredPosition = new Vector2(-310, 150);
            dashRect.sizeDelta = new Vector2(100, 100);
            Image dashBg = dashObj.GetComponent<Image>();
            dashBg.color = new Color(0.25f, 0.3f, 0.4f, 0.9f);

            GameObject dashFillObj = new GameObject("CooldownFill", typeof(RectTransform), typeof(Image));
            dashFillObj.transform.SetParent(dashObj.transform, false);
            RectTransform dashFillRect = dashFillObj.GetComponent<RectTransform>();
            dashFillRect.anchorMin = Vector2.zero;
            dashFillRect.anchorMax = Vector2.one;
            dashFillRect.sizeDelta = Vector2.zero;
            Image dashFillImg = dashFillObj.GetComponent<Image>();
            dashFillImg.color = new Color(0f, 0f, 0f, 0.65f);
            dashFillImg.type = Image.Type.Filled;
            dashFillImg.fillMethod = Image.FillMethod.Radial360;

            GameObject dashTextObj = new GameObject("Txt_Cooldown", typeof(RectTransform), typeof(TextMeshProUGUI));
            dashTextObj.transform.SetParent(dashObj.transform, false);
            RectTransform dashTextRect = dashTextObj.GetComponent<RectTransform>();
            dashTextRect.anchorMin = Vector2.zero;
            dashTextRect.anchorMax = Vector2.one;
            dashTextRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI dashText = dashTextObj.GetComponent<TextMeshProUGUI>();
            dashText.alignment = TextAlignmentOptions.Center;
            dashText.fontSize = 24;

            SetupAndWireControlsInScene();
        }

        private static Transform FindChildRecursive(Transform parent, string nameContains)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name.ToLower().Contains(nameContains.ToLower()))
                {
                    return child;
                }
                Transform found = FindChildRecursive(child, nameContains);
                if (found != null) return found;
            }
            return null;
        }
    }
}
