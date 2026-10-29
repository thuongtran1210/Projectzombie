using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI.DamageText
{
    /// <summary>Kết nối sự kiện sát thương tĩnh của hệ 2D cũ với dịch vụ hiển thị. Chủ sở hữu scene quản lý đăng ký và gỡ sự kiện.</summary>
    public sealed class LegacyDamageTextAdapter : System.IDisposable
    {
        private readonly ICombatTextService _service;
        private bool _bound;
        public LegacyDamageTextAdapter(ICombatTextService service)
        { _service = service ?? throw new System.ArgumentNullException(nameof(service)); }
        public void Bind()
        {
            if (_bound) return;
            HealthSystem.OnDamageReported += OnDamage;
            _bound = true;
        }
        private void OnDamage(DamageReport report) { _service.Show(CombatTextRequest.FromDamage(report)); }
        public void Dispose()
        {
            if (!_bound) return;
            HealthSystem.OnDamageReported -= OnDamage;
            _bound = false;
        }
    }
}
