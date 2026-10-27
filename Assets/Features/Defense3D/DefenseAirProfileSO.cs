using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>World-space flight limits and avoidance settings, authored separately from enemy balance stats.</summary>
    [CreateAssetMenu(menuName = "TREBRAVE/Defense3D/Air Profile", fileName = "Air_SongQue")]
    public sealed class DefenseAirProfileSO : ScriptableObject
    {
        [SerializeField] private float _minimumHeight = 1.4f;
        [SerializeField] private float _maximumHeight = 4f;
        [SerializeField] private float _cruiseHeight = 2.2f;
        [SerializeField, Min(0.01f)] private float _clearanceRadius = 0.4f;
        [SerializeField] private LayerMask _obstacleMask = 4105;
        [SerializeField, Min(1f)] private float _blockedTimeout = 10f;
        public float MinimumHeight => _minimumHeight;
        public float MaximumHeight => _maximumHeight;
        public float CruiseHeight => _cruiseHeight;
        public float ClearanceRadius => _clearanceRadius;
        public int ObstacleMask => _obstacleMask.value;
        public float BlockedTimeout => _blockedTimeout;
        public void Validate()
        {
            if (!DefenseSpatialRules.WithinHeight(_cruiseHeight, _minimumHeight, _maximumHeight) ||
                !DefenseDamageRules.IsFinite(_clearanceRadius) || _clearanceRadius <= 0f ||
                !DefenseDamageRules.IsFinite(_blockedTimeout) || _blockedTimeout <= 0f || _obstacleMask.value == 0)
                throw new InvalidOperationException("Air profile requires finite altitude, positive clearance/timeout and an obstacle mask.");
        }
    }
}
