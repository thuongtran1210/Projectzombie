using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI.DamageText
{
    [CreateAssetMenu(fileName = "DamageTextStyleConfig", menuName = "ProjectZombie/UI/Damage Text Style Config")]
    public class DamageTextStyleConfig : ScriptableObject
    {
        [System.Serializable]
        public struct ElementStyle
        {
            public ElementType Element;
            public Color TextColor;
        }

        [Header("Normal Damage")]
        public Color NormalColor = Color.white;
        public float NormalFontSize = 3.5f;

        [Header("Critical Damage")]
        public Color CritColor = new Color(1f, 0.35f, 0f, 1f); // Cam đậm
        public float CritFontSize = 5.5f;

        [Header("Player Received Damage")]
        public Color PlayerDamageColor = new Color(1f, 0.1f, 0.2f, 1f); // Hồng đỏ
        public float PlayerDamageFontSize = 4.5f;

        [Header("Elemental Colors")]
        public ElementStyle[] ElementalStyles = new ElementStyle[]
        {
            new ElementStyle { Element = ElementType.Kim, TextColor = new Color(1f, 0.85f, 0.3f, 1f) },  // Vàng
            new ElementStyle { Element = ElementType.Moc, TextColor = new Color(0.3f, 0.9f, 0.4f, 1f) },  // Lục
            new ElementStyle { Element = ElementType.Thuy, TextColor = new Color(0.2f, 0.7f, 1f, 1f) },  // Lam
            new ElementStyle { Element = ElementType.Hoa, TextColor = new Color(1f, 0.25f, 0.2f, 1f) },  // Đỏ
            new ElementStyle { Element = ElementType.Tho, TextColor = new Color(0.85f, 0.55f, 0.2f, 1f) } // Cam nâu
        };

        [Header("Motion & Animation Settings")]
        public float Lifetime = 0.6f;
        public float FloatSpeed = 2.0f;
        public float RandomArcSpread = 0.6f;
        public AnimationCurve ScaleCurve = AnimationCurve.EaseInOut(0f, 1.4f, 0.1f, 1f);
        public AnimationCurve AlphaCurve = AnimationCurve.EaseInOut(0.3f, 1f, 1f, 0f);

        public Color GetColor(bool isPlayer, bool isCrit, ElementType element)
        {
            if (isPlayer) return PlayerDamageColor;
            if (isCrit) return CritColor;

            if (element != ElementType.None && ElementalStyles != null)
            {
                foreach (var style in ElementalStyles)
                {
                    if (style.Element == element) return style.TextColor;
                }
            }

            return NormalColor;
        }

        public float GetFontSize(bool isPlayer, bool isCrit)
        {
            if (isPlayer) return PlayerDamageFontSize;
            if (isCrit) return CritFontSize;
            return NormalFontSize;
        }
    }
}
