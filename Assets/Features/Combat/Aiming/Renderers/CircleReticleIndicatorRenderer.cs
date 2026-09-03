using UnityEngine;

namespace ProjectZombie.Features.Combat.Aiming.Renderers
{
    /// <summary>
    /// Renderer vẽ chỉ dấu vòng tròn điểm rơi AOE + đường nối quỹ đạo (CircleReticle: Lựu Đạn, Nước Thánh, Nồi Cơm).
    /// </summary>
    public class CircleReticleIndicatorRenderer : IAimIndicatorRenderer
    {
        public SkillAimType SupportedType => SkillAimType.CircleReticle;

        private Transform _circleIndicator;
        private SpriteRenderer _circleRenderer;
        private Transform _lineIndicator;
        private SpriteRenderer _lineRenderer;

        public void Initialize(Transform root, IndicatorResourceContext context)
        {
            GameObject circleObj = new GameObject("Circle_Reticle_Indicator");
            circleObj.transform.SetParent(root, false);
            _circleIndicator = circleObj.transform;
            _circleRenderer = circleObj.AddComponent<SpriteRenderer>();
            _circleRenderer.sprite = context.CircleSprite;
            _circleRenderer.sortingLayerName = "Skill";
            _circleRenderer.sortingOrder = 6;
            _circleRenderer.enabled = false;

            GameObject lineObj = new GameObject("Reticle_Trajectory_Line");
            lineObj.transform.SetParent(root, false);
            _lineIndicator = lineObj.transform;
            _lineRenderer = lineObj.AddComponent<SpriteRenderer>();
            _lineRenderer.sprite = context.BoxSprite != null ? context.BoxSprite : context.FillSprite;
            _lineRenderer.sortingLayerName = "Skill";
            _lineRenderer.sortingOrder = 5;
            _lineRenderer.enabled = false;
        }

        public void Render(Vector3 origin, Vector2 direction, float angle, float pullPercent, SkillAimConfig config, Color color)
        {
            if (_circleRenderer == null) return;
            _circleRenderer.enabled = true;

            float distance = Mathf.Clamp01(pullPercent) * config.range;
            Vector3 targetPos = origin + (Vector3)(direction * distance);
            float radius = Mathf.Max(1.0f, config.radius);

            float spriteBounds = (_circleRenderer.sprite != null && _circleRenderer.sprite.bounds.size.x > 0.01f)
                ? _circleRenderer.sprite.bounds.size.x
                : 1.0f;
            float scale = (radius * 2.0f) / spriteBounds;

            _circleIndicator.position = targetPos;
            _circleIndicator.rotation = Quaternion.identity;
            _circleIndicator.localScale = Vector3.one * scale;
            _circleRenderer.color = color;

            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = true;
                float lineLen = Mathf.Max(0.5f, distance);
                float lineSpriteX = (_lineRenderer.sprite != null && _lineRenderer.sprite.bounds.size.x > 0.01f) ? _lineRenderer.sprite.bounds.size.x : 1.0f;
                float lineSpriteY = (_lineRenderer.sprite != null && _lineRenderer.sprite.bounds.size.y > 0.01f) ? _lineRenderer.sprite.bounds.size.y : 1.0f;
                _lineIndicator.position = origin + (Vector3)(direction * (lineLen * 0.5f));
                _lineIndicator.rotation = Quaternion.Euler(0f, 0f, angle);
                _lineIndicator.localScale = new Vector3(lineLen / lineSpriteX, 0.25f / lineSpriteY, 1f);
                _lineRenderer.color = new Color(color.r, color.g, color.b, 0.4f);
            }
        }

        public void Hide()
        {
            if (_circleRenderer != null) _circleRenderer.enabled = false;
            if (_lineRenderer != null) _lineRenderer.enabled = false;
        }
    }
}
