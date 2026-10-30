#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ProjectZombie.Features.Defense3D.UI;
using ProjectZombie.Features.Maps;

namespace ProjectZombie.Editor.UI
{
    /// <summary>Authors a mission-first, read-only confirmation screen without modifying the team editor.</summary>
    public static class CleanBattleReviewStyler
    {
        private static readonly Color JADE = new Color(.035f, .085f, .082f);
        private static readonly Color SURFACE = new Color(.067f, .14f, .13f);
        private static readonly Color CREAM = new Color(.94f, .92f, .85f);
        private static readonly Color INK = new Color(.12f, .20f, .18f);
        private static readonly Color MUTED = new Color(.65f, .75f, .69f);
        private static readonly Color GOLD = new Color(.90f, .68f, .30f);
        private static TMP_FontAsset Font => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/GameFont_Vietnamese_SD.asset");

        [MenuItem("ProjectZombie/UI Tools/Apply Clean Battle Review")]
        public static string Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Style battle review in Edit Mode.");
            const string artworkPath = "Assets/Art/UI/StageReview/Stage_SongQue_Overview.png";
            var importer = AssetImporter.GetAtPath(artworkPath) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing Song Que stage illustration.");
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false; importer.maxTextureSize = 2048; importer.SaveAndReimport();
            var stage = AssetDatabase.LoadAssetAtPath<StageDefinitionSO>("Assets/_Data/Levels/Stages/Stage_04_SongQue_Defense3D.asset");
            if (stage != null && (stage.stageThumbnail == null || AssetDatabase.GetAssetPath(stage.stageThumbnail) == "Assets/Art/UI/VongXuyen/BG_VongXuyen_Forest_Hub.png"))
            {
                Undo.RecordObject(stage, "Replace Song Que placeholder artwork");
                stage.stageThumbnail = AssetDatabase.LoadAssetAtPath<Sprite>(artworkPath);
                EditorUtility.SetDirty(stage); AssetDatabase.SaveAssetIfDirty(stage);
            }
            foreach (string path in new[] { "Assets/_Prefabs/UI/DefensePreparationUI.prefab", "Assets/Resources/UI/DefensePreparationUI.prefab" })
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try { Patch(root); PrefabUtility.SaveAsPrefabAsset(root, path); }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            return "Clean battle review applied to both review prefabs. Standalone team editor and open scene not saved or changed.";
        }

        /// <summary>Validates repeat authoring, stage bindings and read-only empty-slot presentation.</summary>
        public static string Validate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Validate clean review in Edit Mode.");
            int checks = 0;
            foreach (string path in new[] { "Assets/_Prefabs/UI/DefensePreparationUI.prefab", "Assets/Resources/UI/DefensePreparationUI.prefab" })
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var view = root.GetComponent<DefensePreparationView>();
                    var so = new SerializedObject(view);
                    var launch = so.FindProperty("_startBattleButton").objectReferenceValue;
                    var edit = so.FindProperty("_editLoadoutButton").objectReferenceValue;
                    int nodes = root.GetComponentsInChildren<Transform>(true).Length;
                    Patch(root); Patch(root);
                    so.Update();
                    if (root.GetComponentsInChildren<Transform>(true).Length != nodes
                        || so.FindProperty("_startBattleButton").objectReferenceValue != launch
                        || so.FindProperty("_editLoadoutButton").objectReferenceValue != edit)
                        throw new InvalidOperationException("Clean review authoring must be idempotent and preserve actions.");
                    checks++;
                    foreach (string field in new[] { "_reviewStageCode", "_reviewStageName", "_reviewStageDescription", "_reviewStageDuration", "_reviewStageRewards", "_reviewStageArtwork" })
                    {
                        if (so.FindProperty(field).objectReferenceValue == null) throw new InvalidOperationException("Missing clean review field: " + field);
                        checks++;
                    }
                    view.SetMinionSlotState(3, false, "+ Thêm");
                    if (((Button)so.FindProperty("_minionSlotButtons").GetArrayElementAtIndex(3).objectReferenceValue).gameObject.activeSelf)
                        throw new InvalidOperationException("Confirmation must not display empty equipment slots.");
                    checks++;
                    view.SetStageReview(DefenseStageReviewPresentation.FromStage(null));
                    if (((Image)so.FindProperty("_reviewStageArtwork").objectReferenceValue).enabled)
                        throw new InvalidOperationException("Missing stage must clear stale map imagery.");
                    checks++;
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            string report = "PASS: " + checks + " clean review binding/idempotence/state checks. No scene or player save writes.";
            System.IO.Directory.CreateDirectory("Logs/CourtyardQA");
            System.IO.File.WriteAllText("Logs/CourtyardQA/clean-review-validation.txt", report);
            return report;
        }

        private static void Patch(GameObject root)
        {
            var view = root.GetComponent<DefensePreparationView>();
            if (view == null || view is DefenseLoadoutManagementView) throw new InvalidOperationException("Clean review tool only accepts the confirmation screen.");
            var so = new SerializedObject(view);
            var frame = root.transform.Find("Defense_Viewport/Frame_Defense_Content");
            if (frame == null) throw new InvalidOperationException("Missing authored review frame.");
            foreach (string name in new[] { "Courtyard_Background", "Courtyard_HeaderShade", "Courtyard_FooterShade", "Courtyard_StageHint", "Review_StageSummary", "Bg_Inner_Wood" })
            {
                var old = frame.Find(name);
                if (old != null) old.gameObject.SetActive(false);
            }
            var background = Panel(frame, "Clean_Background", JADE);
            Place(background.transform, 0, 0, 1920, 1080);
            background.transform.SetAsFirstSibling();
            var mission = Panel(frame, "Clean_Mission", CREAM);
            Place(mission.transform, 64, 142, 1008, 790);
            var team = Panel(frame, "Clean_Team", SURFACE);
            Place(team.transform, 1110, 142, 746, 790);
            var title = MoveText(so, "_titleText", frame, 64, 38, 1480, 62, 38, CREAM);
            title.text = "XÁC NHẬN XUẤT TRẬN";
            var subtitle = MoveText(so, "_subtitleText", frame, 66, 101, 1350, 26, 18, MUTED);
            subtitle.text = "Thông tin ải và lực lượng đã chọn";

            var close = Ref(so, "_closeButton");
            Move(close, frame); Place(close, 1716, 47, 140, 54);
            FlatButton(close, new Color(1, 1, 1, .035f), CREAM, "TRỞ LẠI", 20);
            Bind(so, "_reviewStageCode", Label(mission.transform, "Clean_StageCode", "ẢI ĐÃ CHỌN", 17, new Color(.40f, .45f, .36f)));
            Bind(so, "_reviewStageName", Label(mission.transform, "Clean_StageName", "Chọn ải", 38, INK));
            Bind(so, "_reviewStageDescription", Label(mission.transform, "Clean_StageDescription", "", 20, new Color(.29f, .36f, .32f)));
            Bind(so, "_reviewStageDuration", Label(mission.transform, "Clean_StageDuration", "", 26, INK));
            Bind(so, "_reviewStageRewards", Label(mission.transform, "Clean_StageRewards", "", 22, INK));
            Place(Field(so, "_reviewStageCode"), 96, 168, 944, 28);
            Place(Field(so, "_reviewStageName"), 96, 208, 944, 62);
            Place(Field(so, "_reviewStageDescription"), 96, 276, 944, 48);
            ((TextMeshProUGUI)so.FindProperty("_reviewStageDescription").objectReferenceValue).overflowMode = TextOverflowModes.Ellipsis;
            var artwork = Panel(mission.transform, "Clean_StageArtwork", Color.white);
            artwork.preserveAspect = true;
            Place(artwork.transform, 96, 336, 944, 513);
            Bind(so, "_reviewStageArtwork", artwork);
            var timeLabel = Label(mission.transform, "Clean_TimeLabel", "THỜI GIAN", 14, new Color(.42f, .46f, .37f));
            Place(timeLabel.transform, 96, 865, 170, 20);
            Place(Field(so, "_reviewStageDuration"), 96, 887, 170, 28);
            var rewardLabel = Label(mission.transform, "Clean_RewardLabel", "PHẦN THƯỞNG CƠ BẢN", 14, new Color(.42f, .46f, .37f));
            Place(rewardLabel.transform, 320, 865, 720, 20);
            Place(Field(so, "_reviewStageRewards"), 320, 887, 720, 28);
            var heading = Label(team.transform, "Clean_TeamHeading", "ĐỘI HÌNH ĐÃ CHỌN", 24, CREAM);
            Place(heading.transform, 1142, 174, 682, 34);
            Line(team.transform, "Clean_RuleA", 1142, 228, 682);

            var heroIcon = Ref(so, "_heroAvatarImage"); Move(heroIcon, team.transform);
            Place(heroIcon, 1142, 254, 80, 80);
            var heroName = MoveText(so, "_heroNameText", team.transform, 1244, 260, 580, 68, 27, CREAM);
            heroName.alignment = TextAlignmentOptions.MidlineLeft;
            Ref(so, "_heroRoleText").gameObject.SetActive(false);
            var relicIcon = Ref(so, "_relicIconImage"); Move(relicIcon, team.transform);
            Place(relicIcon, 1150, 368, 62, 62);
            MoveText(so, "_relicNameText", team.transform, 1244, 377, 580, 50, 24, CREAM).alignment = TextAlignmentOptions.MidlineLeft;
            Ref(so, "_relicDescText").gameObject.SetActive(false);
            Line(team.transform, "Clean_RuleB", 1142, 462, 682);
            var minionLabel = Label(team.transform, "Clean_MinionsLabel", "LÍNH", 20, MUTED);
            Place(minionLabel.transform, 1142, 492, 300, 28);
            var towerLabel = Label(team.transform, "Clean_TowersLabel", "TRỤ", 20, MUTED);
            Place(towerLabel.transform, 1142, 698, 300, 28);
            MoveText(so, "_minionCountText", team.transform, 1490, 492, 334, 28, 17, MUTED).alignment = TextAlignmentOptions.MidlineRight;
            MoveText(so, "_towerCountText", team.transform, 1490, 698, 334, 28, 17, MUTED).alignment = TextAlignmentOptions.MidlineRight;
            Rows(so, "_minion", team.transform, 538, 132);
            Rows(so, "_tower", team.transform, 744, 108);
            var status = MoveText(so, "_validationMessageText", team.transform, 1142, 880, 682, 32, 20, new Color(.63f, .80f, .66f));
            status.text = "Đội hình hợp lệ";
            var edit = Ref(so, "_editLoadoutButton"); Move(edit, frame); Place(edit, 1110, 962, 300, 72);
            FlatButton(edit, new Color(.12f, .23f, .21f), CREAM, "CHỈNH ĐỘI HÌNH", 22);
            var start = Ref(so, "_startBattleButton"); Move(start, frame); Place(start, 1434, 962, 422, 72);
            FlatButton(start, GOLD, INK, "XUẤT TRẬN", 27);
            foreach (string name in new[] { "Banner_Header", "Card_Hero", "Card_Relic", "Card_Minions", "Card_Towers", "Btn_HeroSlot" })
            {
                var old = frame.Find(name);
                if (old != null) old.gameObject.SetActive(false);
            }
            // The existing tower placeholder is a flat yellow bar; use the authored crossbow portrait for this review only.
            var icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/Icon_W001_NoThan.png");
            if (icon != null)
            {
                var entries = so.FindProperty("_catalogIcons");
                for (int i = 0; i < entries.arraySize; i++)
                    if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("Id").stringValue == "tower_cungtien")
                        entries.GetArrayElementAtIndex(i).FindPropertyRelative("Icon").objectReferenceValue = icon;
            }
            so.ApplyModifiedProperties();
            var stage = AssetDatabase.LoadAssetAtPath<StageDefinitionSO>("Assets/_Data/Levels/Stages/Stage_04_SongQue_Defense3D.asset");
            view.SetStageReview(DefenseStageReviewPresentation.FromStage(stage));
            EditorUtility.SetDirty(view);
        }

        private static void Rows(SerializedObject so, string prefix, Transform parent, float y, float height)
        {
            var buttons = so.FindProperty(prefix + "SlotButtons");
            var images = so.FindProperty(prefix + "SlotCheckmarks");
            var names = so.FindProperty(prefix + "SlotNameTexts");
            for (int i = 0; i < buttons.arraySize; i++)
            {
                var button = (Button)buttons.GetArrayElementAtIndex(i).objectReferenceValue;
                Move(button.transform, parent);
                Place(button.transform, 1142 + i * 174, y, 160, height);
                button.interactable = false;
                button.transition = Selectable.Transition.None;
                button.GetComponent<Image>().sprite = null;
                button.GetComponent<Image>().color = new Color(.10f, .22f, .20f, 1f);
                var image = (Image)images.GetArrayElementAtIndex(i).objectReferenceValue;
                image.preserveAspect = true; image.raycastTarget = false;
                var rect = image.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1);
                rect.pivot = new Vector2(.5f, 1); rect.anchoredPosition = new Vector2(0, -8);
                rect.sizeDelta = new Vector2(72, prefix == "_minion" ? 72 : 50);
                var name = (TextMeshProUGUI)names.GetArrayElementAtIndex(i).objectReferenceValue;
                name.rectTransform.anchorMin = Vector2.zero; name.rectTransform.anchorMax = new Vector2(1, 0);
                name.rectTransform.pivot = new Vector2(.5f, 0); name.rectTransform.anchoredPosition = new Vector2(0, 4);
                name.rectTransform.sizeDelta = new Vector2(-8, 43);
                name.fontSize = 19; name.enableAutoSizing = true; name.fontSizeMin = 17; name.fontSizeMax = 19;
                name.alignment = TextAlignmentOptions.Center; name.raycastTarget = false;
                button.gameObject.SetActive(i < (prefix == "_minion" ? 3 : 1));
            }
        }

        private static void FlatButton(Transform node, Color background, Color foreground, string value, float size)
        {
            var image = node.GetComponent<Image>(); image.sprite = null; image.color = background; image.type = Image.Type.Simple;
            var text = node.GetComponentInChildren<TextMeshProUGUI>(true) ?? Label(node, "Clean_ButtonLabel", value, size, foreground);
            text.gameObject.SetActive(true); text.text = value; text.fontSize = size; text.color = foreground;
            text.enableAutoSizing = true; text.fontSizeMin = 18; text.fontSizeMax = size; text.alignment = TextAlignmentOptions.Center;
            text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(12, 4); text.rectTransform.offsetMax = new Vector2(-12, -4);
            var button = node.GetComponent<Button>(); button.transition = Selectable.Transition.ColorTint;
        }
        private static TextMeshProUGUI MoveText(SerializedObject so, string field, Transform parent, float x, float y, float w, float h, float size, Color color)
        {
            var transform = Ref(so, field); Move(transform, parent); Place(transform, x, y, w, h);
            var text = transform.GetComponent<TextMeshProUGUI>();
            text.gameObject.SetActive(true); text.fontSize = size; text.color = color; text.enableAutoSizing = true;
            text.fontSizeMin = size - 2; text.fontSizeMax = size; text.alignment = TextAlignmentOptions.TopLeft;
            text.raycastTarget = false;
            return text;
        }
        private static Transform Ref(SerializedObject so, string field) => ((Component)so.FindProperty(field).objectReferenceValue).transform;
        private static Transform Field(SerializedObject so, string field) => Ref(so, field);
        private static void Bind(SerializedObject so, string field, UnityEngine.Object value) => so.FindProperty(field).objectReferenceValue = value;
        private static void Move(Transform node, Transform parent) { if (node.parent != parent) Undo.SetTransformParent(node, parent, "Clean review layout"); }
        private static RectTransform Node(Transform parent, string name)
        {
            var existing = parent.Find(name) as RectTransform;
            if (existing != null) return existing;
            var node = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(node, "Clean review element"); node.transform.SetParent(parent, false);
            return (RectTransform)node.transform;
        }
        private static Image Panel(Transform parent, string name, Color color)
        {
            var node = Node(parent, name); var image = node.GetComponent<Image>() ?? Undo.AddComponent<Image>(node.gameObject);
            image.color = color; image.sprite = null; image.raycastTarget = false; return image;
        }
        private static TextMeshProUGUI Label(Transform parent, string name, string value, float size, Color color)
        {
            var node = Node(parent, name); var text = node.GetComponent<TextMeshProUGUI>() ?? Undo.AddComponent<TextMeshProUGUI>(node.gameObject);
            text.font = Font; text.text = value; text.fontSize = size; text.color = color;
            text.enableAutoSizing = true; text.fontSizeMin = size - 2; text.fontSizeMax = size;
            text.alignment = TextAlignmentOptions.TopLeft; text.raycastTarget = false; return text;
        }
        private static void Line(Transform parent, string name, float x, float y, float width) => Place(Panel(parent, name, new Color(1, 1, 1, .10f)).transform, x, y, width, 1);
        private static void Place(Transform target, float x, float y, float width, float height)
        {
            // Coordinates are relative to the full authored frame, independent of the grouping parent.
            var parent = (RectTransform)target.parent;
            var frame = parent;
            while (frame != null && frame.name != "Frame_Defense_Content") frame = frame.parent as RectTransform;
            if (frame == null) throw new InvalidOperationException("Review element must be under the authored frame.");
            var rect = (RectTransform)target;
            if (parent != frame)
            {
                float px = parent.anchorMin.x * 1920f;
                float py = (1 - parent.anchorMax.y) * 1080f;
                float pw = (parent.anchorMax.x - parent.anchorMin.x) * 1920f;
                float ph = (parent.anchorMax.y - parent.anchorMin.y) * 1080f;
                x = (x - px) / pw; y = (y - py) / ph; width /= pw; height /= ph;
            }
            else { x /= 1920; y /= 1080; width /= 1920; height /= 1080; }
            Undo.RecordObject(rect, "Clean review position");
            rect.anchorMin = new Vector2(x, 1 - y - height); rect.anchorMax = new Vector2(x + width, 1 - y);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
#endif
