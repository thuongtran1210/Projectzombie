using UnityEngine;

namespace ProjectZombie.Features.Combat.Aiming.Renderers
{
    /// <summary>
    /// Renderer vẽ chỉ dấu lướt né đòn: Đường lướt + Vòng tròn điểm đáp (DashLine).
    /// </summary>
    public class DashLineIndicatorRenderer : IAimIndicatorRenderer
    {
        public SkillAimType SupportedType => SkillAimType.DashLine;

        private Transform _lineIndicator;
        private SpriteRenderer _lineRenderer;
        private Transform _landingIndicator;
        private SpriteRenderer _landingRenderer;

        public void Initialize(Transform root, IndicatorResourceContext context)
        {
            GameObject lineObj = new GameObject("Dash_Line_Indicator");
            lineObj.transform.SetParent(root, false);
            _lineIndicator = lineObj.transform;
            _lineRenderer = lineObj.AddComponent<SpriteRenderer>();
            _lineRenderer.sprite = context.BoxSprite != null ? context.BoxSprite : context.FillSprite;
            _lineRenderer.sortingLayerName = "Skill";
            _lineRenderer.sortingOrder = 5;
            _lineRenderer.enabled = false;

            GameObject landingObj = new GameObject("Dash_Landing_Indicator");
            landingObj.transform.SetParent(root, false);
            _landingIndicator = landingObj.transform;
            _landingRenderer = landingObj.AddComponent<SpriteRenderer>();
            _landingRenderer.sprite = context.CircleSprite;
            _landingRenderer.sortingLayerName = "Skill";
            _landingRenderer.sortingOrder = 6;
            _landingRenderer.enabled = false;
        }

        public void Render(Vector3 origin, Vector2 direction, float angle, float pullPercent, SkillAimConfig config, Color color)
        {
            if (_lineRenderer == null || _landingRenderer == null) return;
            _lineRenderer.enabled = true;
            _landingRenderer.enabled = true;

            float length = Mathf.Max(2.0f, config.range);
            float width = 0.5f;

            float spriteBoundsX = (_lineRenderer.sprite != null && _lineRenderer.sprite.bounds.size.x > 0.01f)
                ? _lineRenderer.sprite.bounds.size.x : 1.0f;
            float spriteBoundsY = (_lineRenderer.sprite != null && _lineRenderer.sprite.bounds.size.y > 0.01f)
                ? _lineRenderer.sprite.bounds.size.y : 1.0f;

            _lineIndicator.position = origin + (Vector3)(direction * (length * 0.5f));
            _lineIndicator.rotation = Quaternion.Euler(0f, 0f, angle);
            _lineIndicator.localScale = new Vector3(length / spriteBoundsX, width / spriteBoundsY, 1f);
            _lineRenderer.color = color;

            float landingRadius = 0.8f;
            float circleBounds = (_landingRenderer.sprite != null && _landingRenderer.sprite.bounds.size.x > 0.01f)
                ? _landingRenderer.sprite.bounds.size.x : 1.0f;
            float circleScale = (landingRadius * 2.0f) / circleBounds;

            _landingIndicator.position = origin + (Vector3)(direction * length);
            _landingIndicator.rotation = Quaternion.identity;
            _landingIndicator.localScale = Vector3.one * circleScale;
            _landingRenderer.color = color;
        }

        public void Hide()
        {
            if (_lineRenderer != null) _lineRenderer.enabled = false;
            if (_landingRenderer != null) _landingRenderer.enabled = false;
        }
    }
}
