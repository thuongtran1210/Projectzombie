#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.UI;

namespace ProjectZombie.EditorTools
{
    public static class CardThemeSetupUtility
    {
        private const string THEME_ASSET_PATH = "Assets/Art/UI/CardThemeDatabase_Default.asset";

        [MenuItem("ProjectZombie/2. 📱 Mobile UI/4. Tối Ưu UI/7. Setup Default Card Theme", priority = 168)]
        public static void CreateAndSetupDefaultCardTheme()
        {
            // 1. Tạo hoặc load CardThemeDatabase Asset
            CardThemeDatabase theme = AssetDatabase.LoadAssetAtPath<CardThemeDatabase>(THEME_ASSET_PATH);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<CardThemeDatabase>();
                AssetDatabase.CreateAsset(theme, THEME_ASSET_PATH);
                Debug.Log($"<color=#00FF88>[CardThemeSetup]</color> Created new CardThemeDatabase at {THEME_ASSET_PATH}");
            }

            // 2. Tìm và gán các Frame Sprites
            var wood = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Card_Wood_9Slice.png");
            var jade = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Card_Jade_9Slice.png");
            var gold = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Card_Evolution_Gold_9Slice.png");
            var synergy = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Frame_Card_Synergy_9Slice.png");

            SerializedObject serializedTheme = new SerializedObject(theme);
            if (wood != null) serializedTheme.FindProperty("_frameCommonWood").objectReferenceValue = wood;
            if (jade != null) serializedTheme.FindProperty("_frameRareJade").objectReferenceValue = jade;
            if (gold != null) serializedTheme.FindProperty("_frameEvolutionGold").objectReferenceValue = gold;
            if (synergy != null) serializedTheme.FindProperty("_frameSynergyAmber").objectReferenceValue = synergy;

            serializedTheme.ApplyModifiedProperties();
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            Debug.Log("<color=#00FF88>[CardThemeSetup]</color> Đã thiết lập xong CardThemeDatabase mặc định.");
        }
    }
}
#endif
