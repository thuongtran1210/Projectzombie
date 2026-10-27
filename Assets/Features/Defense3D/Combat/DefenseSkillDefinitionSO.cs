using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    [CreateAssetMenu(menuName = "Defense3D/Skill Definition")]
    public sealed class DefenseSkillDefinitionSO : ScriptableObject
    {
        [SerializeField] private DefenseSkillKind _kind;
        [SerializeField, Min(0.01f)] private float _range;
        [SerializeField, Min(0f)] private float _radius;
        [SerializeField, Min(0f)] private float _damage;
        [SerializeField, Min(0.01f)] private float _cooldown;
        [SerializeField, Min(0.01f)] private float _duration;
        [SerializeField] private bool _autoTargetOnTap;
        [SerializeField] private DefenseTargetMask _targetMask = DefenseTargetMask.All;

        public DefenseSkillDefinition Capture() => new DefenseSkillDefinition(_kind, _range, _radius, _damage,
            _cooldown, _duration, _autoTargetOnTap, _targetMask);
    }
}
