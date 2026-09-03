using UnityEngine;

namespace ProjectZombie.Features.Combat.Aiming.Renderers
{
    /// <summary>
    /// Renderer vẽ chỉ dấu hình quạt nón (ConeSector: Trống Đồng, Đao Cửu Vĩ, Vệt Chém).
    /// </summary>
    public class ConeSectorIndicatorRenderer : IAimIndicatorRenderer
    {
        public SkillAimType SupportedType => SkillAimType.ConeSector;

        private Transform _coneIndicator;
        private SpriteRenderer _coneRenderer;
        private MaterialPropertyBlock _conePropertyBlock;
        private Material _sectorMaterial;

        private static readonly int PropArcAngle = Shader.PropertyToID("_ArcAngle");
        private static readonly int PropTintColor = Shader.PropertyToID("_TintColor");
        private static readonly int PropBorderColor = Shader.PropertyToID("_BorderColor");

        public void Initialize(Transform root, IndicatorResourceContext context)
        {
            _conePropertyBlock = new MaterialPropertyBlock();
            _sectorMaterial = context.SectorMaterial;

            GameObject coneObj = new GameObject("Cone_Indicator");
            coneObj.transform.SetParent(root, false);
            _coneIndicator = coneObj.transform;

            _coneRenderer = coneObj.AddComponent<SpriteRenderer>();
            _coneRenderer.sprite = context.CircleSprite != null ? context.CircleSprite : context.FillSprite;
            if (_sectorMaterial != null)
            {
                _coneRenderer.material = _sectorMaterial;
            }
            _coneRenderer.sortingLayerName = "Skill";
            _coneRenderer.sortingOrder = 5;
            _coneRenderer.enabled = false;
        }

        public void Render(Vector3 origin, Vector2 direction, float angle, float pullPercent, SkillAimConfig config, Color color)
        {
            if (_coneRenderer == null) return;
            _coneRenderer.enabled = true;

            float reach = Mathf.Max(1.8f, config.range);
            float spriteBounds = (_coneRenderer.sprite != null && _coneRenderer.sprite.bounds.size.x > 0.01f)
                ? _coneRenderer.sprite.bounds.size.x
                : 1.0f;

            float scale = (reach * 2.0f) / spriteBounds;

            if (_sectorMaterial != null && _coneRenderer.sharedMaterial == _sectorMaterial)
            {
                float arcAngle = config.sectorAngle > 0f ? config.sectorAngle : 90f;
                _conePropertyBlock.SetFloat(PropArcAngle, arcAngle);
                _conePropertyBlock.SetColor(PropTintColor, color);
                _conePropertyBlock.SetColor(PropBorderColor, new Color(color.r * 1.2f, color.g * 1.2f, color.b * 1.2f, 0.95f));
                _coneRenderer.SetPropertyBlock(_conePropertyBlock);

                _coneIndicator.position = origin;
                _coneIndicator.rotation = Quaternion.Euler(0f, 0f, angle);
                _coneIndicator.localScale = Vector3.one * scale;
            }
            else
            {
                float width = Mathf.Max(1.4f, config.radius);
                _coneIndicator.position = origin + (Vector3)(direction * (reach * 0.45f));
                _coneIndicator.rotation = Quaternion.Euler(0f, 0f, angle);
                _coneIndicator.localScale = new Vector3(reach / spriteBounds, width / spriteBounds, 1f);
                _coneRenderer.color = color;
            }
        }

        public void Hide()
        {
            if (_coneRenderer != null) _coneRenderer.enabled = false;
        }
    }
}
