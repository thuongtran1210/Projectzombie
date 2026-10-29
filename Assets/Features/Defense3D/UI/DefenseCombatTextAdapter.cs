using System;
using UnityEngine;
using ProjectZombie.Features.UI.DamageText;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Mỗi vòng đời actor có một đăng ký sự kiện; chỉ chuyển sát thương đã được chấp nhận sang phần hiển thị.</summary>
    public sealed class DefenseCombatTextAdapter : IDisposable
    {
        private readonly IDefenseDamageable3D _source;
        private readonly IDefenseGameplayContext _gameplay;
        private readonly ICombatTextService _service;
        private readonly Func<Vector3> _position;
        private readonly CombatTextTarget _target;

        private bool _disposed;
        public DefenseCombatTextAdapter(IDefenseDamageable3D source, IDefenseGameplayContext gameplay,
            ICombatTextService service, Func<Vector3> position, CombatTextTarget target)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _position = position ?? throw new ArgumentNullException(nameof(position));
            _target = target;
            _source.Damaged += OnDamage;
        }
        private void OnDamage(DefenseDamageContext damage)
        {
            if (_disposed || !_gameplay.AdvancesGameplay ||
                !DefenseDamageRules.IsValid(damage) ||
                (damage.RunGeneration != -1 && damage.RunGeneration != _gameplay.RunGeneration) ||
                (damage.SessionIdentity != null && !ReferenceEquals(damage.SessionIdentity, _gameplay.SessionIdentity))) return;
            _service.Show(new CombatTextRequest(damage.Amount, _position(), _target, damage.IsCritical));
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _source.Damaged -= OnDamage;
        }
    }
}
