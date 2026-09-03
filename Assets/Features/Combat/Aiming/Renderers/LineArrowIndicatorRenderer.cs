using UnityEngine;

namespace ProjectZombie.Features.Combat.Aiming.Renderers
{
    /// <summary>
    /// Renderer vẽ chỉ dấu đường thẳng / mũi tên định hướng (LineArrow: Nỏ Thần, Cung, Kiếm Khí).
    /// </summary>
    public class LineArrowIndicatorRenderer : IAimIndicatorRenderer
    {
        public SkillAimType SupportedType => SkillAimType.LineArrow;

        private Transform _lineIndicator;
        private SpriteRenderer _lineRenderer;

        public void Initialize(Transform root, IndicatorResourceContext context)
        {
            GameObject lineObj = new GameObject("Line_Indicator");
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
            if (_lineRenderer == null) return;
            _lineRenderer.enabled = true;

            float length = Mathf.Max(2.0f, config.range);
            float width = Mathf.Max(0.6f, config.radius);

            float spriteBoundsX = (_lineRenderer.sprite != null && _lineRenderer.sprite.bounds.size.x > 0.01f)
                ? _lineRenderer.sprite.bounds.size.x
                : 1.0f;
            float spriteBoundsY = (_lineRenderer.sprite != null && _lineRenderer.sprite.bounds.size.y > 0.01f)
                ? _lineRenderer.sprite.bounds.size.y
                : 1.0f;

            float scaleX = length / spriteBoundsX;
            float scaleY = width / spriteBoundsY;

            _lineIndicator.position = origin + (Vector3)(direction * (length * 0.5f));
            _lineIndicator.rotation = Quaternion.Euler(0f, 0f, angle);
            _lineIndicator.localScale = new Vector3(scaleX, scaleY, 1f);
            _lineRenderer.color = color;
        }

        public void Hide()
        {
            if (_lineRenderer != null) _lineRenderer.enabled = false;
        }
    }
}
