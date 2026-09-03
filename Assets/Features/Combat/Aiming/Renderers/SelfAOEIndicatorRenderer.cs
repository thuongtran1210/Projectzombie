using UnityEngine;

namespace ProjectZombie.Features.Combat.Aiming.Renderers
{
    /// <summary>
    /// Renderer vẽ chỉ dấu vòng tròn hào quang cố định quanh chân người chơi (SelfAOE: Aura, Khiên hộ thể).
    /// </summary>
    public class SelfAOEIndicatorRenderer : IAimIndicatorRenderer
    {
        public SkillAimType SupportedType => SkillAimType.SelfAOE;

        private Transform _circleIndicator;
        private SpriteRenderer _circleRenderer;

        public void Initialize(Transform root, IndicatorResourceContext context)
        {
            GameObject circleObj = new GameObject("SelfAOE_Circle_Indicator");
            circleObj.transform.SetParent(root, false);
            _circleIndicator = circleObj.transform;
            _circleRenderer = circleObj.AddComponent<SpriteRenderer>();
            _circleRenderer.sprite = context.CircleSprite;
            _circleRenderer.sortingLayerName = "Skill";
            _circleRenderer.sortingOrder = 6;
            _circleRenderer.enabled = false;
        }

        public void Render(Vector3 origin, Vector2 direction, float angle, float pullPercent, SkillAimConfig config, Color color)
        {
            if (_circleRenderer == null) return;
            _circleRenderer.enabled = true;

            float radius = Mathf.Max(1.2f, config.radius);
            float spriteBounds = (_circleRenderer.sprite != null && _circleRenderer.sprite.bounds.size.x > 0.01f)
                ? _circleRenderer.sprite.bounds.size.x : 1.0f;
            float scale = (radius * 2.0f) / spriteBounds;

            _circleIndicator.position = origin;
            _circleIndicator.rotation = Quaternion.identity;
            _circleIndicator.localScale = Vector3.one * scale;
            _circleRenderer.color = color;
        }

        public void Hide()
        {
            if (_circleRenderer != null) _circleRenderer.enabled = false;
        }
    }
}
