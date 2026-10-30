using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>
    /// Bảng màu Dark Fantasy lấy cảm hứng từ UI RPG cao cấp.
    /// Không thay sprite hay layout hiện hữu; áp màu lên các thành phần được chỉ định.
    /// </summary>
    public static class LegendaryFantasyTheme
    {
        public static readonly Color Background = Hex("#101525");
        public static readonly Color Surface = Hex("#1B2035");
        public static readonly Color Gold = Hex("#D4AB69");
        public static readonly Color MutedGold = Hex("#93734A");
        public static readonly Color Ivory = Hex("#F3E6CF");
        public static readonly Color Jade = Hex("#4DEEEA");
        public static readonly Color Danger = Hex("#E5756C");

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out Color color);
            return color;
        }

        public static void Label(TextMeshProUGUI label, bool heading = false)
        {
            if (label == null) return;
            label.color = heading ? Gold : Ivory;
        }

        public static void Accent(TextMeshProUGUI label)
        {
            if (label != null) label.color = MutedGold;
        }

        public static void Primary(Button button, TextMeshProUGUI label = null)
        {
            if (button != null)
            {
                var graphic = button.targetGraphic;
                if (graphic != null) graphic.color = Gold;
                ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = Hex("#FFF0CF");
                colors.pressedColor = Hex("#B88A4F");
                colors.selectedColor = Color.white;
                colors.disabledColor = new Color(.45f, .45f, .45f, .65f);
                button.colors = colors;
            }
            if (label != null) label.color = Background;
        }

        public static void Secondary(Button button)
        {
            if (button == null) return;
            if (button.targetGraphic != null) button.targetGraphic.color = Surface;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Hex("#BAC6E0");
            colors.pressedColor = Hex("#8192B9");
            colors.selectedColor = Color.white;
            button.colors = colors;
        }

        public static void Health(Slider slider)
        {
            if (slider != null && slider.fillRect != null)
            {
                var graphic = slider.fillRect.GetComponent<Graphic>();
                if (graphic != null) graphic.color = Danger;
            }
        }

        public static void Experience(Slider slider)
        {
            if (slider != null && slider.fillRect != null)
            {
                var graphic = slider.fillRect.GetComponent<Graphic>();
                if (graphic != null) graphic.color = Jade;
            }
        }
    }
}
