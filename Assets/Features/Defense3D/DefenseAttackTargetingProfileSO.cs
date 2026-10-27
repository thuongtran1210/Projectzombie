using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Authored attack capabilities. Air eligibility is explicit per weapon/tower profile.</summary>
    [CreateAssetMenu(menuName = "TREBRAVE/Defense3D/Attack Targeting", fileName = "AttackTargeting")]
    public sealed class DefenseAttackTargetingProfileSO : ScriptableObject
    {
        [SerializeField] private DefenseTargetMask _targets = DefenseTargetMask.All;
        [SerializeField] private LayerMask _lineOfSightMask = 4105;
        [SerializeField] private Vector3 _originOffset = new Vector3(0f, 0.5f, 0f);
        public DefenseTargetMask Targets => _targets;
        public int LineOfSightMask => _lineOfSightMask.value;
        public Vector3 OriginOffset => _originOffset;
    }
}
