using UnityEngine;

namespace ProjectZombie.Features.Combat.Aiming.Renderers
{
    /// <summary>
    /// Renderer vẽ vòng tròn co giãn theo nhịp QTE (RhythmPulse: Trống Đồng Đông Sơn).
    /// </summary>
    public class RhythmPulseIndicatorRenderer : IAimIndicatorRenderer
    {
        public SkillAimType SupportedType => SkillAimType.RhythmPulse;

        private Transform _pulseIndicator;
        private SpriteRenderer _pulseRenderer;

        public void Initialize(Transform root, IndicatorResourceContext context)
        {
            GameObject pulseObj = new GameObject("RhythmPulse_Circle_Indicator");
            pulseObj.transform.SetParent(root, false);
            _pulseIndicator = pulseObj.transform;
            _pulseRenderer = pulseObj.AddComponent<SpriteRenderer>();
            _pulseRenderer.sprite = context.CircleSprite;
            _pulseRenderer.sortingLayerName = "Skill";
            _pulseRenderer.sortingOrder = 6;
            _pulseRenderer.enabled = false;
        }

        public void Render(Vector3 origin, Vector2 direction, float angle, float pullPercent, SkillAimConfig config, Color color)
        {
            if (_pulseRenderer == null) return;
            _pulseRenderer.enabled = true;

            float baseRadius = Mathf.Max(2.0f, config.radius);
            float pulseOffset = Mathf.PingPong(Time.unscaledTime * 3.5f, 0.8f);
            float currentRadius = baseRadius + pulseOffset;

            float spriteBounds = (_pulseRenderer.sprite != null && _pulseRenderer.sprite.bounds.size.x > 0.01f)
                ? _pulseRenderer.sprite.bounds.size.x : 1.0f;

            _pulseIndicator.position = origin;
            _pulseIndicator.rotation = Quaternion.identity;
            _pulseIndicator.localScale = Vector3.one * ((currentRadius * 2.0f) / spriteBounds);
            _pulseRenderer.color = color;
        }

        public void Hide()
        {
            if (_pulseRenderer != null) _pulseRenderer.enabled = false;
        }
    }
}
