using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Tunable prototype cadence and flight settings; separate from the 34 timed Stage events.</summary>
    [CreateAssetMenu(menuName = "TREBRAVE/Defense3D/Ghost Cannon Profile", fileName = "GhostCannon_SongQue")]
    public sealed class DefenseGhostCannonProfileSO : ScriptableObject
    {
        [SerializeField] private float _health = 300f;
        [SerializeField] private float _firstShotTime = 30f;
        [SerializeField] private float _interval = 12f;
        [SerializeField] private float _ballisticSeconds = 2f;
        [SerializeField] private float _parachuteSeconds = 3f;
        [SerializeField] private float _arcHeight = 5f;
        [SerializeField] private float _openHeight = 4f;
        [SerializeField] private float _stopLaunchingAt = 600f;
        public float Health => _health;
        public float FirstShotTime => _firstShotTime;
        public float Interval => _interval;
        public float BallisticSeconds => _ballisticSeconds;
        public float ParachuteSeconds => _parachuteSeconds;
        public float ArcHeight => _arcHeight;
        public float OpenHeight => _openHeight;
        public float StopLaunchingAt => _stopLaunchingAt;
        public void Validate()
        {
            if (!Positive(_health) || !Positive(_interval) || !Positive(_ballisticSeconds) || !Positive(_parachuteSeconds) ||
                !Positive(_openHeight) || !Positive(_stopLaunchingAt) || !DefenseDamageRules.IsFinite(_firstShotTime) || _firstShotTime < 0f ||
                !DefenseDamageRules.IsFinite(_arcHeight) || _arcHeight < 0f)
                throw new InvalidOperationException("Invalid Ghost Cannon profile.");
        }
        private static bool Positive(float value) => DefenseDamageRules.IsFinite(value) && value > 0f;
    }
}
