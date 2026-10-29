using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Module trình diễn thay thế được; instance chỉ quản lý vòng đời, không biết hình dạng VFX.</summary>
    public abstract class DefenseEffectVisual3D : MonoBehaviour
    {
        public abstract bool IsConfigured { get; }
        public abstract void Prepare(Color color, float diameter);
        public abstract void Render(float progress);
        public abstract void ResetVisual();
    }
}
