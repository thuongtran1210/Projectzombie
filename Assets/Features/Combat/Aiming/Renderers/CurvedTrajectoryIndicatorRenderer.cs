using UnityEngine;

namespace ProjectZombie.Features.Combat.Aiming.Renderers
{
    /// <summary>
    /// Renderer vẽ quỹ đạo ném cong Boomerang/Parabol (CurvedTrajectory: Dép Tổ Ong, Phi Tiêu).
    /// </summary>
    public class CurvedTrajectoryIndicatorRenderer : IAimIndicatorRenderer
    {
        public SkillAimType SupportedType => SkillAimType.CurvedTrajectory;

        private Transform _apexIndicator;
        private SpriteRenderer _apexRenderer;
        private LineRenderer _curveLineRenderer;

        private const int CURVE_SEGMENTS = 20;
        private readonly Vector3[] _curvePointsBuffer = new Vector3[CURVE_SEGMENTS + 1];

        public void Initialize(Transform root, IndicatorResourceContext context)
        {
            GameObject curveObj = new GameObject("Curved_Trajectory_Line");
            curveObj.transform.SetParent(root, false);
            _curveLineRenderer = curveObj.AddComponent<LineRenderer>();
            _curveLineRenderer.positionCount = CURVE_SEGMENTS + 1;
            _curveLineRenderer.startWidth = 0.28f;
            _curveLineRenderer.endWidth = 0.12f;
            _curveLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _curveLineRenderer.sortingLayerName = "Skill";
            _curveLineRenderer.sortingOrder = 7;
            _curveLineRenderer.enabled = false;

            GameObject apexObj = new GameObject("Curved_Apex_Indicator");
            apexObj.transform.SetParent(root, false);
            _apexIndicator = apexObj.transform;
            _apexRenderer = apexObj.AddComponent<SpriteRenderer>();
            _apexRenderer.sprite = context.CircleSprite;
            _apexRenderer.sortingLayerName = "Skill";
            _apexRenderer.sortingOrder = 6;
            _apexRenderer.enabled = false;
        }

        public void Render(Vector3 origin, Vector2 direction, float angle, float pullPercent, SkillAimConfig config, Color color)
        {
            if (_apexRenderer == null || _curveLineRenderer == null) return;
            _apexRenderer.enabled = true;
            _curveLineRenderer.enabled = true;

            _curveLineRenderer.startColor = color;
            _curveLineRenderer.endColor = new Color(color.r, color.g, color.b, 0.2f);

            float totalLength = Mathf.Max(2.5f, config.range);
            float curveOffsetDistance = config.sectorAngle > 0.1f ? (config.sectorAngle * 0.05f) : 1.5f;

            Vector2 perpendicular = new Vector2(-direction.y, direction.x);

            Vector3 startPoint = origin;
            Vector3 apexPoint = origin + (Vector3)(direction * totalLength);
            Vector3 controlPoint = origin + (Vector3)(direction * (totalLength * 0.5f)) + (Vector3)(perpendicular * curveOffsetDistance);

            for (int i = 0; i <= CURVE_SEGMENTS; i++)
            {
                float t = (float)i / CURVE_SEGMENTS;
                _curvePointsBuffer[i] = (1f - t) * (1f - t) * startPoint + 2f * (1f - t) * t * controlPoint + t * t * apexPoint;
            }

            _curveLineRenderer.SetPositions(_curvePointsBuffer);

            float circleBounds = (_apexRenderer.sprite != null && _apexRenderer.sprite.bounds.size.x > 0.01f)
                ? _apexRenderer.sprite.bounds.size.x : 1.0f;
            _apexIndicator.position = apexPoint;
            _apexIndicator.rotation = Quaternion.identity;
            _apexIndicator.localScale = Vector3.one * (1.3f / circleBounds);
            _apexRenderer.color = color;
        }

        public void Hide()
        {
            if (_apexRenderer != null) _apexRenderer.enabled = false;
            if (_curveLineRenderer != null) _curveLineRenderer.enabled = false;
        }
    }
}
