using UnityEngine;

namespace ProjectZombie.Features.Combat.Aiming.Renderers
{
    /// <summary>
    /// Renderer vẽ chỉ dấu bức tường ngăn cách xoay vuông góc với hướng ngắm (VectorWall: Điếu Cày, Nước Thánh).
    /// </summary>
    public class VectorWallIndicatorRenderer : IAimIndicatorRenderer
    {
        public SkillAimType SupportedType => SkillAimType.VectorWall;

        private Transform _wallIndicator;
        private SpriteRenderer _wallRenderer;
        private Transform _pinIndicator;
        private SpriteRenderer _pinRenderer;

        public void Initialize(Transform root, IndicatorResourceContext context)
        {
            GameObject wallObj = new GameObject("VectorWall_Bar_Indicator");
            wallObj.transform.SetParent(root, false);
            _wallIndicator = wallObj.transform;
            _wallRenderer = wallObj.AddComponent<SpriteRenderer>();
            _wallRenderer.sprite = context.BoxSprite != null ? context.BoxSprite : context.FillSprite;
            _wallRenderer.sortingLayerName = "Skill";
            _wallRenderer.sortingOrder = 5;
            _wallRenderer.enabled = false;

            GameObject pinObj = new GameObject("VectorWall_Pin_Indicator");
            pinObj.transform.SetParent(root, false);
            _pinIndicator = pinObj.transform;
            _pinRenderer = pinObj.AddComponent<SpriteRenderer>();
            _pinRenderer.sprite = context.CircleSprite;
            _pinRenderer.sortingLayerName = "Skill";
            _pinRenderer.sortingOrder = 6;
            _pinRenderer.enabled = false;
        }

        public void Render(Vector3 origin, Vector2 direction, float angle, float pullPercent, SkillAimConfig config, Color color)
        {
            if (_wallRenderer == null || _pinRenderer == null) return;
            _wallRenderer.enabled = true;
            _pinRenderer.enabled = true;

            float distance = config.range;
            Vector3 centerWallPos = origin + (Vector3)(direction * distance);
            float wallWidth = config.WallLength;
            float wallThickness = config.WallThickness;

            float spriteBoundsX = (_wallRenderer.sprite != null && _wallRenderer.sprite.bounds.size.x > 0.01f)
                ? _wallRenderer.sprite.bounds.size.x : 1.0f;
            float spriteBoundsY = (_wallRenderer.sprite != null && _wallRenderer.sprite.bounds.size.y > 0.01f)
                ? _wallRenderer.sprite.bounds.size.y : 1.0f;

            // Bức tường xoay vuông góc với hướng ngắm (angle + 90 độ)
            _wallIndicator.position = centerWallPos;
            _wallIndicator.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
            _wallIndicator.localScale = new Vector3(wallWidth / spriteBoundsX, wallThickness / spriteBoundsY, 1f);
            _wallRenderer.color = color;

            // Tâm định vị giữa bức tường
            float pinBounds = (_pinRenderer.sprite != null && _pinRenderer.sprite.bounds.size.x > 0.01f)
                ? _pinRenderer.sprite.bounds.size.x : 1.0f;
            _pinIndicator.position = centerWallPos;
            _pinIndicator.rotation = Quaternion.identity;
            _pinIndicator.localScale = Vector3.one * (0.8f / pinBounds);
            _pinRenderer.color = new Color(color.r * 1.3f, color.g * 1.3f, color.b * 1.3f, 0.9f);
        }

        public void Hide()
        {
            if (_wallRenderer != null) _wallRenderer.enabled = false;
            if (_pinRenderer != null) _pinRenderer.enabled = false;
        }
    }
}
