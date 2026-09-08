using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Thư viện tiện ích dùng chung cho các Unity Editor Tools.
    /// Giúp code ngắn gọn, chuẩn hóa Anchor/Pivot/Styling và tránh can thiệp phá vỡ Scene.
    /// </summary>
    public static class UIEditorHelper
    {
        public enum AnchorPreset
        {
            BottomLeft,
            BottomCenter,
            BottomRight,
            TopLeft,
            TopCenter,
            TopRight,
            MiddleCenter,
            StretchAll,
            BottomStretch,
            TopStretch
        }

        public static GameObject CreateChildUI(Transform parent, string name)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            if (parent != null)
            {
                obj.transform.SetParent(parent, false);
            }
            Undo.RegisterCreatedObjectUndo(obj, $"Create {name}");
            return obj;
        }

        public static RectTransform ApplyAnchorPreset(RectTransform rt, AnchorPreset preset)
        {
            switch (preset)
            {
                case AnchorPreset.BottomLeft:
                    rt.anchorMin = new Vector2(0f, 0f);
                    rt.anchorMax = new Vector2(0f, 0f);
                    rt.pivot = new Vector2(0f, 0f);
                    break;
                case AnchorPreset.BottomCenter:
                    rt.anchorMin = new Vector2(0.5f, 0f);
                    rt.anchorMax = new Vector2(0.5f, 0f);
                    rt.pivot = new Vector2(0.5f, 0f);
                    break;
                case AnchorPreset.BottomRight:
                    rt.anchorMin = new Vector2(1f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(1f, 0f);
                    break;
                case AnchorPreset.TopLeft:
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(0f, 1f);
                    rt.pivot = new Vector2(0f, 1f);
                    break;
                case AnchorPreset.TopCenter:
                    rt.anchorMin = new Vector2(0.5f, 1f);
                    rt.anchorMax = new Vector2(0.5f, 1f);
                    rt.pivot = new Vector2(0.5f, 1f);
                    break;
                case AnchorPreset.TopRight:
                    rt.anchorMin = new Vector2(1f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(1f, 1f);
                    break;
                case AnchorPreset.MiddleCenter:
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    break;
                case AnchorPreset.StretchAll:
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    break;
                case AnchorPreset.BottomStretch:
                    rt.anchorMin = new Vector2(0f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(0.5f, 0f);
                    break;
                case AnchorPreset.TopStretch:
                    rt.anchorMin = new Vector2(0f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(0.5f, 1f);
                    break;
            }
            return rt;
        }

        public static Image AddImage(GameObject go, string spritePath = null, Color? color = null, bool raycastTarget = false)
        {
            Image img = go.GetComponent<Image>();
            if (img == null) img = Undo.AddComponent<Image>(go);

            if (!string.IsNullOrEmpty(spritePath))
            {
                img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            }
            img.color = color ?? Color.white;
            img.raycastTarget = raycastTarget;
            return img;
        }

        public static TextMeshProUGUI AddTMPText(GameObject go, string text, float fontSize = 24f, Color? color = null, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = Undo.AddComponent<TextMeshProUGUI>(go);

            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color ?? Color.white;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            return tmp;
        }

        public static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (comp == null)
            {
                comp = Undo.AddComponent<T>(go);
            }
            return comp;
        }

        public static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
