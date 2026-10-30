#if UNITY_EDITOR
using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ProjectZombie.Features.Defense3D.UI;

namespace ProjectZombie.Editor.UI
{
    /// <summary>Patches existing preparation prefabs into the courtyard layout without replacing their references.</summary>
    public static class CourtyardDeploymentStyler
    {
        private const string BACKGROUND = "Assets/Art/UI/Courtyard/BG_Courtyard_Deployment.png";
        private static readonly Color GOLD = new Color(.96f, .79f, .43f);
        private static Sprite Art(string name)
        {
            string path = "Assets/Art/UI/VongXuyen/" + name + ".png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path)) if (asset is Sprite found) return found;
            Debug.LogError("Missing courtyard UI sprite: " + path);
            return null;
        }

        /// <summary>Updates both authored variants and renders them separately from the open scene.</summary>
        [MenuItem("ProjectZombie/UI Tools/Apply Courtyard Deployment")]
        public static string Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Apply courtyard layout in Edit Mode.");
            var importer = AssetImporter.GetAtPath(BACKGROUND) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing courtyard background: " + BACKGROUND);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
            foreach (string path in new[] { "Assets/_Prefabs/UI/DefensePreparationUI.prefab", "Assets/Resources/UI/DefensePreparationUI.prefab" })
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    Patch(root);
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            return "Courtyard layout applied to both preparation prefabs; open scene not saved.";
        }

        /// <summary>Checks references and repeat application on isolated prefab contents without saving them.</summary>
        public static string Validate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Validate courtyard layout in Edit Mode.");
            int checks = 0;
            foreach (string path in new[] { "Assets/_Prefabs/UI/DefensePreparationUI.prefab", "Assets/Resources/UI/DefensePreparationUI.prefab" })
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var view = root.GetComponent<DefensePreparationView>();
                    var so = new SerializedObject(view);
                    var heroButton = so.FindProperty("_heroSlotButton").objectReferenceValue;
                    var relicButton = so.FindProperty("_relicSlotButton").objectReferenceValue;
                    int originalNodes = root.GetComponentsInChildren<Transform>(true).Length;
                    Patch(root);
                    Patch(root);
                    if (root.GetComponentsInChildren<Transform>(true).Length != originalNodes)
                        throw new InvalidOperationException("Repeated courtyard patch created duplicate nodes: " + path);
                    checks++;
                    so.Update();
                    if (so.FindProperty("_heroSlotButton").objectReferenceValue != heroButton
                        || so.FindProperty("_relicSlotButton").objectReferenceValue != relicButton)
                        throw new InvalidOperationException("Courtyard patch replaced original actions.");
                    checks++;
                    foreach (string field in new[] { "_courtyardHeroPreview", "_courtyardHeroFallback", "_startBattleButton", "_editLoadoutButton", "_closeButton" })
                    {
                        if (so.FindProperty(field).objectReferenceValue == null) throw new InvalidOperationException("Missing courtyard reference: " + field);
                        checks++;
                    }
                    var previewRect = ((RawImage)so.FindProperty("_courtyardHeroPreview").objectReferenceValue).rectTransform;
                    if (previewRect.rect.width <= 0f || !Mathf.Approximately(previewRect.rect.width, previewRect.rect.height))
                        throw new InvalidOperationException("Courtyard model preview must have a nonzero square viewport.");
                    checks++;
                    foreach (string prefix in new[] { "_minion", "_tower" })
                    {
                        var buttons = so.FindProperty(prefix + "SlotButtons");
                        var icons = so.FindProperty(prefix + "SlotCheckmarks");
                        var labels = so.FindProperty(prefix + "SlotNameTexts");
                        if (buttons.arraySize != 4 || icons.arraySize != 4 || labels.arraySize != 4)
                            throw new InvalidOperationException("Courtyard requires four authored slots per unit group.");
                        for (int i = 0; i < 4; i++)
                            if (buttons.GetArrayElementAtIndex(i).objectReferenceValue == null || icons.GetArrayElementAtIndex(i).objectReferenceValue == null
                                || labels.GetArrayElementAtIndex(i).objectReferenceValue == null)
                                throw new InvalidOperationException("Unbound courtyard position.");
                        checks++;
                    }
                    if (((Button)so.FindProperty("_startBattleButton").objectReferenceValue).GetComponent<Image>().sprite == null)
                        throw new InvalidOperationException("Courtyard launch button sprite missing.");
                    checks++;
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            string report = "PASS: " + checks + " courtyard reference/idempotence checks on both prefab variants. No player save writes. Full Play Mode remains unverified.";
            Directory.CreateDirectory("Logs/CourtyardQA");
            File.WriteAllText("Logs/CourtyardQA/validation.txt", report);
            return report;
        }

        private static void Patch(GameObject root)
        {
            var view = root.GetComponent<DefensePreparationView>();
            if (view == null) throw new InvalidOperationException("Preparation view is missing.");
            var so = new SerializedObject(view);
            var frame = (RectTransform)root.transform.Find("Defense_Viewport/Frame_Defense_Content");
            if (frame == null) throw new InvalidOperationException("Expected authored preparation frame.");
            Undo.RecordObject(frame, "Courtyard layout");
            var viewport = (RectTransform)frame.parent;
            Stretch(viewport);
            var aspect = frame.GetComponent<AspectRatioFitter>();
            if (aspect != null) { Undo.RecordObject(aspect, "Courtyard frame aspect"); aspect.enabled = false; }
            Stretch(frame);
            Tint(frame, Color.clear);
            var inner = frame.Find("Bg_Inner_Wood");
            if (inner != null) inner.gameObject.SetActive(false);
            var background = ImageNode(frame, "Courtyard_Background", AssetDatabase.LoadAssetAtPath<Sprite>(BACKGROUND), Color.white);
            Stretch(background.rectTransform);
            background.transform.SetAsFirstSibling();
            var header = ImageNode(frame, "Courtyard_HeaderShade", null, new Color(.035f, .075f, .068f, .93f));
            Place(header.transform, 0, 0, 1920, 128);
            header.transform.SetSiblingIndex(1);
            var footer = ImageNode(frame, "Courtyard_FooterShade", null, new Color(.035f, .065f, .057f, .94f));
            Place(footer.transform, 0, 924, 1920, 156);
            footer.transform.SetSiblingIndex(2);
            Place(frame.Find("Banner_Header"), 50, 25, 1250, 76);
            Tint(frame.Find("Banner_Header"), Color.clear);
            Text(so, "_titleText", "SÂN ĐÌNH XUẤT TRẬN", 38, GOLD);
            Place(Ref(so, "_titleText"), 0, 0, 1250, 76, 1250, 76);
            Place(Ref(so, "_subtitleText"), 60, 91, 1500, 28);
            Text(so, "_subtitleText", "Tập hợp lực lượng • Chạm vị trí để thay thẻ", 20, new Color(.79f, .84f, .73f));
            Place(Ref(so, "_closeButton"), 1790, 32, 70, 70);

            var hero = frame.Find("Card_Hero");
            var relic = frame.Find("Card_Relic");
            var minions = frame.Find("Card_Minions");
            var towers = frame.Find("Card_Towers");
            Card(hero, 60, 166, 402, 262);
            Card(relic, 60, 468, 402, 322);
            Card(minions, 1270, 162, 590, 316);
            Card(towers, 1270, 514, 590, 316);
            Pill(hero.Find("Pill_Hero"), "ANH HÙNG", 402);
            Pill(relic.Find("Pill_Relic"), "THẦN KHÍ", 402);
            Pill(minions.Find("Pill_Minions"), "QUÂN LÍNH", 590);
            Pill(towers.Find("Pill_Towers"), "TRỤ PHÒNG THỦ", 590);
            Place(hero.Find("Box_Avatar"), 22, 68, 78, 78, 402, 262);
            Place(Ref(so, "_heroAvatarImage"), 4, 4, 70, 70, 78, 78);
            Place(Ref(so, "_heroNameText"), 116, 69, 264, 68, 402, 262);
            Text(so, "_heroNameText", "THƯ SINH", 26, GOLD);
            Place(Ref(so, "_heroRoleText"), 22, 151, 358, 88, 402, 262);
            Text(so, "_heroRoleText", "Chủ tướng dẫn đầu đội hình\nChạm nhân vật để thay anh hùng", 20, new Color(.90f, .88f, .79f));
            // Keep the original hero action reference as a transparent stage-sized target.
            var heroButton = Ref(so, "_heroSlotButton");
            Undo.SetTransformParent(heroButton, frame, "Move hero stage action");
            Place(heroButton, 490, 180, 730, 656);
            Tint(heroButton, new Color(1, 1, 1, .002f));
            foreach (var label in heroButton.GetComponentsInChildren<TextMeshProUGUI>(true)) label.gameObject.SetActive(false);
            var fallback = ImageNode(heroButton, "Courtyard_HeroFallback", null, Color.white);
            Place(fallback.transform, 175, 135, 380, 410, 730, 656);
            fallback.preserveAspect = true;
            var previewNode = Node(heroButton, "Courtyard_HeroPreview");
            var raw = previewNode.GetComponent<RawImage>();
            if (raw == null) raw = Undo.AddComponent<RawImage>(previewNode.gameObject);
            raw.raycastTarget = false;
            raw.enabled = false;
            Place(raw.transform, 45, 10, 640, 640, 730, 656);
            var previewAspect = raw.GetComponent<AspectRatioFitter>();
            if (previewAspect != null) previewAspect.enabled = false;
            // A centered square scales with the root CanvasScaler and is stable in prefab/preview scenes.
            raw.rectTransform.anchorMin = raw.rectTransform.anchorMax = new Vector2(.5f, .5f);
            raw.rectTransform.anchoredPosition = Vector2.zero;
            raw.rectTransform.sizeDelta = new Vector2(640f, 640f);
            so.FindProperty("_courtyardHeroPreview").objectReferenceValue = raw;
            so.FindProperty("_courtyardHeroFallback").objectReferenceValue = fallback;
            var models = so.FindProperty("_courtyardHeroModels");
            models.arraySize = 1;
            models.GetArrayElementAtIndex(0).FindPropertyRelative("Id").stringValue = "hero_thusinh";
            var scholar = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Characters/ThuSinh/ThuSinhVisual.prefab");
            if (scholar == null) throw new InvalidOperationException("Scholar preview visual is missing.");
            models.GetArrayElementAtIndex(0).FindPropertyRelative("Prefab").objectReferenceValue = scholar;
            var hint = Label(frame, "Courtyard_StageHint", "CHẠM ANH HÙNG ĐỂ THAY", 20, new Color(.98f, .89f, .66f));
            Place(hint.transform, 550, 845, 650, 38);
            hint.alignment = TextAlignmentOptions.Center;

            Place(relic.Find("Box_RelicIcon"), 22, 72, 105, 105, 402, 322);
            Place(Ref(so, "_relicIconImage"), 8, 8, 89, 89, 105, 105);
            Place(Ref(so, "_relicNameText"), 141, 76, 237, 70, 402, 322);
            Text(so, "_relicNameText", "DÉP TỔ ONG", 25, GOLD);
            Place(Ref(so, "_relicDescText"), 22, 183, 358, 63, 402, 322);
            Text(so, "_relicDescText", "Thần khí đồng hành cùng chủ tướng", 20, new Color(.90f, .88f, .79f));
            Place(Ref(so, "_relicSlotButton"), 22, 258, 358, 44, 402, 322);
            ButtonLabel(Ref(so, "_relicSlotButton"), "ĐỔI THẦN KHÍ", 21);
            Group(so, "_minion", minions);
            Group(so, "_tower", towers);
            Place(Ref(so, "_minionCountText"), 24, 55, 540, 28, 590, 316);
            Place(Ref(so, "_towerCountText"), 24, 55, 540, 28, 590, 316);
            Place(Ref(so, "_validationMessageText"), 65, 942, 1160, 38);
            Text(so, "_validationMessageText", "Đội hình sẵn sàng xuất trận.", 22, new Color(.70f, .88f, .74f));
            Place(Ref(so, "_editLoadoutButton"), 65, 998, 430, 52);
            ButtonLabel(Ref(so, "_editLoadoutButton"), "TÙY CHỈNH ĐỘI HÌNH", 22);
            Place(Ref(so, "_startBattleButton"), 1375, 960, 480, 82);
            var start = Ref(so, "_startBattleButton").GetComponent<Image>();
            start.sprite = Art("Btn_Upgrade_Wood_Sub_9Slice");
            start.color = new Color(1f, .74f, .32f, 1f);
            start.type = Image.Type.Sliced;
            ButtonLabel(start.transform, "XUẤT TRẬN", 30);
            foreach (var text in root.GetComponentsInChildren<TextMeshProUGUI>(true)) text.raycastTarget = false;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(view);
        }

        private static void Group(SerializedObject so, string prefix, Transform parent)
        {
            var buttons = so.FindProperty(prefix + "SlotButtons");
            var icons = so.FindProperty(prefix + "SlotCheckmarks");
            var names = so.FindProperty(prefix + "SlotNameTexts");
            for (int i = 0; i < buttons.arraySize; i++)
            {
                var button = ((Button)buttons.GetArrayElementAtIndex(i).objectReferenceValue).transform;
                Place(button, 20 + i % 2 * 279, 98 + i / 2 * 101, 269, 91, 590, 316);
                Tint(button, Color.white);
                button.GetComponent<Image>().sprite = Art("Slot_Inventory_Wood_9Slice");
                var icon = (Image)icons.GetArrayElementAtIndex(i).objectReferenceValue;
                Place(icon.transform, 12, 13, 62, 62, 269, 91);
                icon.preserveAspect = true;
                var label = (TextMeshProUGUI)names.GetArrayElementAtIndex(i).objectReferenceValue;
                Place(label.transform, 87, 12, 169, 66, 269, 91);
                label.fontSize = 21;
                label.enableAutoSizing = true;
                label.fontSizeMin = 18;
                label.fontSizeMax = 21;
                label.alignment = TextAlignmentOptions.MidlineLeft;
            }
        }
        private static void Card(Transform target, float x, float y, float w, float h)
        {
            Place(target, x, y, w, h);
            var image = target.GetComponent<Image>();
            Undo.RecordObject(image, "Courtyard panel");
            image.sprite = Art("Slot_Inventory_Wood_9Slice");
            image.color = new Color(.83f, .91f, .84f, .97f);
        }
        private static void Pill(Transform target, string caption, float width)
        {
            Place(target, 18, 16, width - 36, 34, width, width == 402 ? 262 : 316);
            Tint(target, Color.clear);
            var text = target.GetComponentInChildren<TextMeshProUGUI>(true);
            text.text = caption; text.color = GOLD; text.fontSize = 23;
        }
        private static Transform Ref(SerializedObject so, string field) => ((Component)so.FindProperty(field).objectReferenceValue).transform;
        private static void Text(SerializedObject so, string field, string value, float size, Color color)
        {
            var text = Ref(so, field).GetComponent<TextMeshProUGUI>();
            Undo.RecordObject(text, "Courtyard text");
            text.text = value; text.fontSize = size; text.color = color;
            text.enableAutoSizing = true; text.fontSizeMin = size - 2; text.fontSizeMax = size;
        }
        private static void ButtonLabel(Transform button, string value, float size)
        {
            var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null) return;
            label.text = value; label.fontSize = size; label.enableAutoSizing = true; label.fontSizeMin = 18; label.fontSizeMax = size;
        }
        private static RectTransform Node(Transform parent, string name)
        {
            var existing = parent.Find(name) as RectTransform;
            if (existing != null) return existing;
            var node = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(node, "Courtyard UI element");
            node.transform.SetParent(parent, false);
            return (RectTransform)node.transform;
        }
        private static Image ImageNode(Transform parent, string name, Sprite sprite, Color color)
        {
            var node = Node(parent, name);
            var image = node.GetComponent<Image>() ?? Undo.AddComponent<Image>(node.gameObject);
            image.sprite = sprite; image.color = color; image.raycastTarget = false;
            return image;
        }
        private static TextMeshProUGUI Label(Transform parent, string name, string value, float size, Color color)
        {
            var node = Node(parent, name);
            var text = node.GetComponent<TextMeshProUGUI>() ?? Undo.AddComponent<TextMeshProUGUI>(node.gameObject);
            text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/GameFont_Vietnamese_SD.asset");
            if (text.font == null) text.font = TMP_Settings.defaultFontAsset;
            text.text = value; text.fontSize = size; text.color = color; text.raycastTarget = false;
            return text;
        }
        private static void Tint(Transform target, Color color)
        {
            var image = target.GetComponent<Image>();
            if (image != null) { Undo.RecordObject(image, "Courtyard tint"); image.color = color; }
        }
        private static void Stretch(RectTransform rect)
        {
            Undo.RecordObject(rect, "Courtyard anchors");
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        private static void Place(Transform target, float x, float y, float w, float h, float pw = 1920, float ph = 1080)
        {
            var rect = (RectTransform)target;
            Undo.RecordObject(rect, "Courtyard position");
            rect.anchorMin = new Vector2(x / pw, 1 - (y + h) / ph);
            rect.anchorMax = new Vector2((x + w) / pw, 1 - y / ph);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
#endif
