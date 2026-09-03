using UnityEngine;

namespace ProjectZombie.Features.Combat.Aiming
{
    /// <summary>
    /// Chiến lược vẽ chỉ dấu ngắm chiêu (Indicator Rendering Strategy).
    /// Triển khai Strategy Pattern & Open-Closed Principle (OCP).
    /// </summary>
    public interface IAimIndicatorRenderer
    {
        SkillAimType SupportedType { get; }
        void Initialize(Transform root, IndicatorResourceContext context);
        void Render(Vector3 origin, Vector2 direction, float angle, float pullPercent, SkillAimConfig config, Color color);
        void Hide();
    }

    /// <summary>
    /// Context tài nguyên dùng chung cho các Indicator Renderers (Zero-GC sharing).
    /// </summary>
    public class IndicatorResourceContext
    {
        public Sprite CircleSprite;
        public Sprite BoxSprite;
        public Sprite FillSprite;
        public Sprite ArrowSprite;
        public Material SectorMaterial;
    }
}
