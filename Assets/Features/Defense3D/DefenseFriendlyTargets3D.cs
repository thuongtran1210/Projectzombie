using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Adapter for an injected friendly target; colliders are cached outside combat loops.</summary>
    public sealed class DefenseFriendlyTarget3D : IDefenseAggroCandidate
    {
        private readonly Behaviour _actor;
        public IDefenseDamageable3D Damageable { get; }
        public Collider Collider { get; }
        public Transform Transform => _actor != null ? _actor.transform : null;
        public object Identity => Damageable;
        public DefenseFriendlyKind Kind { get; }
        public bool IsAvailable => _actor != null && _actor.isActiveAndEnabled && !Damageable.IsDead;
        public DefensePoint3 Position
        {
            get { var p = Transform != null ? Transform.position : Vector3.zero; return new DefensePoint3(p.x, p.y, p.z); }
        }
        public DefenseFriendlyTarget3D(Behaviour actor, IDefenseDamageable3D damageable, DefenseFriendlyKind kind)
        {
            _actor = actor; Damageable = damageable; Kind = kind;
            Collider = actor != null ? actor.GetComponent<Collider>() : null;
        }
        public bool MatchesSource(object source) => source is Component component && component != null && Transform != null && component.transform.IsChildOf(Transform);
    }

    /// <summary>Explicit scene roster. Empty Supports is valid and does not create additional gameplay units.</summary>
    public sealed class DefenseFriendlyTargets3D : MonoBehaviour
    {
        [SerializeField] private DefenseHeroHealth3D _hero;
        [SerializeField] private DefenseSupportTarget3D[] _supports;
        public IDefenseAggroCandidate[] Candidates { get; private set; }
        public void ValidateBindings()
        {
            if (_hero == null || _hero.gameObject.scene != gameObject.scene)
                throw new InvalidOperationException("Assign the same-scene Hero health target.");
            for (int i = 0; i < (_supports != null ? _supports.Length : 0); i++)
            {
                if (_supports[i] == null || _supports[i].gameObject.scene != gameObject.scene)
                    throw new InvalidOperationException("Support targets must be explicit same-scene actors.");
                for (int j = 0; j < i; j++) if (_supports[i] == _supports[j]) throw new InvalidOperationException("Duplicate Support target.");
            }
        }
        public void Bind(IDefenseGameplayContext gameplay)
        {
            ValidateBindings();
            int count = _supports != null ? _supports.Length : 0;
            Candidates = new IDefenseAggroCandidate[count + 1];
            Candidates[0] = new DefenseFriendlyTarget3D(_hero, _hero, DefenseFriendlyKind.Hero);
            for (int i = 0; i < count; i++)
            {
                _supports[i].BindGameplay(gameplay);
                Candidates[i + 1] = new DefenseFriendlyTarget3D(_supports[i], _supports[i], DefenseFriendlyKind.Support);
            }
        }
    }
}
