using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Phi Cuong claw windup/shape; damage and cooldown stay in the enemy stats snapshot.</summary>
    [CreateAssetMenu(menuName = "TREBRAVE/Defense3D/Cone Attack Profile", fileName = "PhiCuong_Claw")]
    public sealed class DefenseConeAttackProfileSO : ScriptableObject
    {
        [SerializeField] private float _windup = 0.65f;
        [SerializeField] private float _radius = 2.5f;
        [SerializeField] private float _angle = 120f;
        [SerializeField] private LayerMask _lineOfSightMask = 4105;
        public float Windup => _windup;
        public float Radius => _radius;
        public float Angle => _angle;
        public int LineOfSightMask => _lineOfSightMask.value;
        public void Validate()
        {
            if (!DefenseDamageRules.IsFinite(_windup) || _windup <= 0f || !DefenseDamageRules.IsFinite(_radius) || _radius <= 0f ||
                !DefenseDamageRules.IsFinite(_angle) || _angle <= 0f || _angle > 360f || _lineOfSightMask.value == 0)
                throw new InvalidOperationException("Invalid cone attack profile.");
        }
    }
}
