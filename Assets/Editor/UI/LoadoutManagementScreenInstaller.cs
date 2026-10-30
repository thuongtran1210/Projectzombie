#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.Defense3D.UI;

namespace ProjectZombie.Editor.UI
{
    /// <summary>Creates a separately registered team editor while preserving the pre-match review.</summary>
    public static class LoadoutManagementScreenInstaller
    {
        [MenuItem("ProjectZombie/UI Tools/Install Standalone Loadout Screen")]
        public static string Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install loadout screen in Edit Mode.");
            foreach (string folder in new[] { "Assets/_Prefabs/UI/", "Assets/Resources/UI/" })
            {
                string target = folder + "DefenseLoadoutManagementUI.prefab";
                string source = AssetDatabase.LoadAssetAtPath<GameObject>(target) != null ? target : folder + "DefensePreparationUI.prefab";
                var root = PrefabUtility.LoadPrefabContents(source);
                try
                {
                    root.name = "DefenseLoadoutManagementUI";
                    var existing = root.GetComponent<DefensePreparationView>();
                    var management = existing as DefenseLoadoutManagementView;
                    if (management == null)
                    {
                        var from = new SerializedObject(existing);
                        management = Undo.AddComponent<DefenseLoadoutManagementView>(root);
                        var to = new SerializedObject(management);
                        var iterator = from.GetIterator();
                        bool enter = true;
                        while (iterator.NextVisible(enter))
                        {
                            enter = false;
                            if (iterator.propertyPath != "m_Script") to.CopyFromSerializedProperty(iterator);
                        }
                        to.ApplyModifiedProperties();
                        Undo.DestroyObjectImmediate(existing);
                    }
                    Configure(root, management, true);
                    PrefabUtility.SaveAsPrefabAsset(root, target);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }

                var reviewRoot = PrefabUtility.LoadPrefabContents(folder + "DefensePreparationUI.prefab");
                try
                {
                    Configure(reviewRoot, reviewRoot.GetComponent<DefensePreparationView>(), false);
                    PrefabUtility.SaveAsPrefabAsset(reviewRoot, folder + "DefensePreparationUI.prefab");
                }
                finally { PrefabUtility.UnloadPrefabContents(reviewRoot); }
            }

            var registry = AssetDatabase.LoadAssetAtPath<UIRegistrySO>("Assets/Resources/UI/UIRegistry.asset");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/UI/DefenseLoadoutManagementUI.prefab");
            if (registry == null || prefab == null) throw new InvalidOperationException("Missing registry or standalone loadout prefab.");
            var serialized = new SerializedObject(registry);
            var entries = serialized.FindProperty("_screens");
            int index = -1;
            for (int i = 0; i < entries.arraySize; i++)
                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("screenType").intValue == (int)MetaScreenType.LoadoutManagement) index = i;
            if (index < 0) { index = entries.arraySize; entries.arraySize++; }
            var entry = entries.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("screenType").intValue = (int)MetaScreenType.LoadoutManagement;
            entry.FindPropertyRelative("screenPrefab").objectReferenceValue = prefab.GetComponent<DefenseLoadoutManagementView>();
            entry.FindPropertyRelative("screenPrefabRef.m_AssetGUID").stringValue = string.Empty;
            entry.FindPropertyRelative("screenHierarchyName").stringValue = "Screen_LoadoutManagement";
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssetIfDirty(registry);
            typeof(UIRegistrySO).GetMethod("BuildLookup", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(registry, null);
            return "Standalone LoadoutManagement registered separately from DefensePreparation review. Both prefab variants updated; open scene not saved.";
        }

        private static void Configure(GameObject root, DefensePreparationView view, bool management)
        {
            var so = new SerializedObject(view);
            SetText(so, "_titleText", management ? "ĐỘI HÌNH • SÂN ĐÌNH" : "XÁC NHẬN XUẤT TRẬN");
            SetText(so, "_subtitleText", management ? "Chọn Anh hùng, Thần khí, Lính và Trụ mang vào trận." : "Kiểm tra lực lượng trước khi bắt đầu ải.");
            SetText(so, "_startBattleButtonText", management ? "XONG" : "XUẤT TRẬN");
            SetText(so, "_validationMessageText", management ? "Chạm từng vị trí để thay trang bị." : "Đội hình sẵn sàng xuất trận.");
            var edit = (Button)so.FindProperty("_editLoadoutButton").objectReferenceValue;
            edit.gameObject.SetActive(!management);
            foreach (string field in new[] { "_heroSlotButton", "_relicSlotButton" })
                ((Button)so.FindProperty(field).objectReferenceValue).interactable = management;
            foreach (string field in new[] { "_minionSlotButtons", "_towerSlotButtons" })
            {
                var buttons = so.FindProperty(field);
                for (int i = 0; i < buttons.arraySize; i++) ((Button)buttons.GetArrayElementAtIndex(i).objectReferenceValue).interactable = management;
            }
            var hint = root.transform.Find("Defense_Viewport/Frame_Defense_Content/Courtyard_StageHint")?.GetComponent<TextMeshProUGUI>();
            if (hint != null) hint.text = management ? "CHẠM ANH HÙNG ĐỂ THAY" : "KIỂM TRA ĐỘI HÌNH TRƯỚC KHI XUẤT TRẬN";
            if (management)
            {
                var start = (Button)so.FindProperty("_startBattleButton").objectReferenceValue;
                start.GetComponent<Image>().color = new Color(.60f, .90f, .75f);
            }
            var frame = root.transform.Find("Defense_Viewport/Frame_Defense_Content");
            var stageButton = (Button)so.FindProperty("_heroSlotButton").objectReferenceValue;
            stageButton.gameObject.SetActive(management);
            ((Button)so.FindProperty("_relicSlotButton").objectReferenceValue).gameObject.SetActive(management);
            var summaryPanel = frame.Find("Review_StageSummary") as RectTransform;
            if (!management && summaryPanel == null)
            {
                var node = new GameObject("Review_StageSummary", typeof(RectTransform), typeof(Image));
                Undo.RegisterCreatedObjectUndo(node, "Review stage summary");
                node.transform.SetParent(frame, false);
                summaryPanel = (RectTransform)node.transform;
                var image = node.GetComponent<Image>();
                image.sprite = frame.Find("Card_Hero").GetComponent<Image>().sprite;
                image.type = Image.Type.Sliced; image.color = new Color(.73f, .82f, .79f, .97f); image.raycastTarget = false;
            }
            if (summaryPanel != null)
            {
                summaryPanel.gameObject.SetActive(!management);
                summaryPanel.anchorMin = new Vector2(500f / 1920f, 260f / 1080f);
                summaryPanel.anchorMax = new Vector2(1220f / 1920f, 850f / 1080f);
                summaryPanel.offsetMin = summaryPanel.offsetMax = Vector2.zero;
                var summary = summaryPanel.GetComponentInChildren<TextMeshProUGUI>(true);
                if (summary == null)
                {
                    var node = new GameObject("Txt_StageReview", typeof(RectTransform));
                    Undo.RegisterCreatedObjectUndo(node, "Stage review text");
                    node.transform.SetParent(summaryPanel, false);
                    summary = Undo.AddComponent<TextMeshProUGUI>(node);
                }
                summary.font = ((TextMeshProUGUI)so.FindProperty("_titleText").objectReferenceValue).font;
                summary.fontSize = 28; summary.alignment = TextAlignmentOptions.Center;
                summary.color = new Color(.93f, .89f, .80f); summary.raycastTarget = false;
                summary.rectTransform.anchorMin = Vector2.zero; summary.rectTransform.anchorMax = Vector2.one;
                summary.rectTransform.offsetMin = new Vector2(28, 24); summary.rectTransform.offsetMax = new Vector2(-28, -24);
                summary.text = "ẢI ĐÃ CHỌN\n<color=#F1D08A>Xác nhận trận địa</color>\n\nKIỂM TRA LỰC LƯỢNG\nLính • Trụ • Anh hùng • Thần khí\n\nChọn Chỉnh đội hình nếu cần thay đổi.\nXác nhận khi đã sẵn sàng.";
                so.FindProperty("_reviewStageSummary").objectReferenceValue = summary;
                so.ApplyModifiedProperties();
            }
            EditorUtility.SetDirty(view);
        }

        private static void SetText(SerializedObject so, string field, string value)
        {
            var text = (TextMeshProUGUI)so.FindProperty(field).objectReferenceValue;
            if (text != null) { Undo.RecordObject(text, "Loadout screen role"); text.text = value; }
        }
    }
}
#endif
