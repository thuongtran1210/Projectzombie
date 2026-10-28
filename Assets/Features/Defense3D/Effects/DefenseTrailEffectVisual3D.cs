using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Module trail nhận màu và độ rộng từ profile; không có timer hoặc Update riêng.</summary>
    public sealed class DefenseTrailEffectVisual3D : DefenseEffectVisual3D
    {
        [SerializeField] private TrailRenderer _trail;
        private Color _originalStart, _originalEnd;
        private float _originalWidth;
        private bool _cached;
        public override bool IsConfigured => _trail != null && !_trail.autodestruct;
        public override void Prepare(Color color, float diameter)
        {
            if (!_cached)
            {
                _originalStart = _trail.startColor; _originalEnd = _trail.endColor;
                _originalWidth = _trail.widthMultiplier; _cached = true;
            }
            _trail.Clear(); _trail.startColor = color;
            var end = color; end.a = 0f; _trail.endColor = end;
            _trail.widthMultiplier = diameter * .65f; _trail.enabled = true;
        }
        public override void Render(float progress)
        {
            // TrailRenderer tự tạo điểm theo chuyển động; service cập nhật vị trí root khi hiệu ứng active.
        }
        public override void ResetVisual()
        {
            if (_trail == null) return;
            _trail.emitting = false; _trail.Clear(); _trail.enabled = false;
            if (!_cached) return;
            _trail.startColor = _originalStart; _trail.endColor = _originalEnd; _trail.widthMultiplier = _originalWidth;
        }
    }
}
