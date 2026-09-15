#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Features.Upgrades.Editor.Studio.Panels
{
    /// <summary>
    /// Panel chuyên trách trực quan hóa Live WYSIWYG Card Preview theo phong cách Đông Sơn Anime URP.
    /// </summary>
    public class StudioPreviewPanel
    {
        private Vector2 _scrollPos;

        public void Draw(UpgradeData selectedUpgrade, float containerWidth, GUILayoutOption widthOption)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox, widthOption);

            GUILayout.Label("<b>👁️ LIVE CARD PREVIEW (WYSIWYG)</b>", EditorStyles.boldLabel);
            GUILayout.Space(6);

            _scrollPos = GUILayout.BeginScrollView(_scrollPos);

            if (selectedUpgrade != null)
            {
                UpgradeStudioCardPreviewRenderer.DrawCardPreview(selectedUpgrade, containerWidth);
            }
            else
            {
                EditorGUILayout.HelpBox("Chọn một thẻ để xem trước trực quan.", MessageType.Info);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }
}
#endif
