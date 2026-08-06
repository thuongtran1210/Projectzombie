using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.VFX.Indicators
{
    /// <summary>
    /// ScriptableObject chứa cấu hình visual style cho vệt chỉ báo kỹ năng (Telegraph Indicator).
    /// Cho phép tùy biến màu sắc, viền, thuộc tính Ngũ Hành mà không cần hardcode trong C#.
    /// </summary>
    [CreateAssetMenu(fileName = "IndicatorStyle_", menuName = "ProjectZombie/VFX/Indicator Style")]
    public class IndicatorStyleSO : ScriptableObject
    {
        [Header("Shape & Identification")]
        [SerializeField] private IndicatorShape _shape = IndicatorShape.Box;
        public IndicatorShape Shape => _shape;

        [Header("Colors & Visuals")]
        [SerializeField] private Color _borderColor = new Color(1f, 0.2f, 0.2f, 0.9f);
        public Color BorderColor => _borderColor;

        [SerializeField] private Color _fillColor = new Color(1f, 0.1f, 0.1f, 0.35f);
        public Color FillColor => _fillColor;

        [Header("Elemental Tinting")]
        [SerializeField] private ElementType _elementType = ElementType.None;
        public ElementType ElementType => _elementType;

        [Header("Animation Settings")]
        [SerializeField] private float _minAlpha = 0.2f;
        public float MinAlpha => _minAlpha;

        [SerializeField] private float _maxAlpha = 0.85f;
        public float MaxAlpha => _maxAlpha;

        [SerializeField] private bool _enablePulse = true;
        public bool EnablePulse => _enablePulse;
    }
}
