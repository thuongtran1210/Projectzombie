using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

namespace ProjectZombie.EditorTools
{
    public static class ApplyVietnameseUIToScene
    {
        [MenuItem("Tools/Vong Xuyen/Apply Vietnamese Folklore UI to Scene")]
        public static void ApplyUI()
        {
            var panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Frames/Panel_DongSon_GameOver.png");
            var btnSonMaiSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_SonMai_ChuSa.png");
            var btnGoMunSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Buttons/Btn_GoMun_Dark.png");

            var rootObj = GameObject.Find("GameOverUI_Root");
            if (rootObj == null)
            {
                Debug.LogWarning("[ApplyVietnameseUI] Không tìm thấy GameOverUI_Root trong Scene!");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(rootObj, "Apply Vietnamese UI Styling");

            // 1. Background_Panel 9-Slice
            var bgPanel = rootObj.transform.Find("Background_Panel");
            if (bgPanel != null)
            {
                var img = bgPanel.GetComponent<Image>();
                if (img != null && panelSprite != null)
                {
                    img.sprite = panelSprite;
                    img.type = Image.Type.Sliced;
                    img.color = Color.white;
                }
            }

            // 2. PlayAgain_Button (Tái Chiến)
            var playBtn = rootObj.transform.Find("PlayAgain_Button");
            if (playBtn != null)
            {
                var img = playBtn.GetComponent<Image>();
                if (img != null && btnSonMaiSprite != null)
                {
                    img.sprite = btnSonMaiSprite;
                    img.type = Image.Type.Sliced;
                    img.color = Color.white;
                }
                var tmp = playBtn.GetComponentInChildren<TMP_Text>(true);
                if (tmp != null)
                {
                    tmp.text = "TÁI CHIẾN";
                    tmp.color = new Color(1f, 0.94f, 0.75f, 1f); // Warm gold
                    tmp.fontStyle = FontStyles.Bold;
                }
            }

            // 3. MainMenu_Button (Hồi Quy)
            var menuBtn = rootObj.transform.Find("MainMenu_Button");
            if (menuBtn != null)
            {
                var img = menuBtn.GetComponent<Image>();
                if (img != null && btnGoMunSprite != null)
                {
                    img.sprite = btnGoMunSprite;
                    img.type = Image.Type.Sliced;
                    img.color = Color.white;
                }
                var tmp = menuBtn.GetComponentInChildren<TMP_Text>(true);
                if (tmp != null)
                {
                    tmp.text = "HỒI QUY";
                    tmp.color = new Color(0.88f, 0.86f, 0.92f, 1f); // Antique silver
                    tmp.fontStyle = FontStyles.Bold;
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("<color=#00FFAA>[ApplyVietnameseUI]</color> Đã áp dụng thành công UI Cổ Phong Đông Sơn thuần Việt vào Scene!");
        }
    }
}
