using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>One non-stacking burn per actor lease, measured exclusively by the session clock.</summary>
    public sealed class DefenseBurnRuntime
    {
        private readonly IDefenseGameplayContext _gameplay;
        private int _generation;
        private float _dps, _until, _sample, _nextTick;
        public bool IsBurning => _generation == _gameplay.RunGeneration && _dps > 0f && _gameplay.ElapsedSeconds < _until;
        public DefenseBurnRuntime(IDefenseGameplayContext gameplay) { _gameplay = gameplay ?? throw new ArgumentNullException(nameof(gameplay)); Reset(); }
        public bool Apply(float dps, float duration)
        {
            float now = _gameplay.ElapsedSeconds;
            if (!_gameplay.AdvancesGameplay || _generation != _gameplay.RunGeneration ||
                !DefenseDamageRules.IsFinite(dps) || dps <= 0f || !DefenseDamageRules.IsFinite(duration) || duration <= 0f ||
                !DefenseDamageRules.IsFinite(now + duration) || !DefenseDamageRules.IsFinite(dps * duration)) return false;
            if (_dps == 0f) { _sample = now; _nextTick = now + .5f; }
            _dps = Math.Max(_dps, dps);
            _until = Math.Max(_until, now + duration);
            return true;
        }
        public bool TryTick(out float damage, bool flush = false)
        {
            damage = 0f;
            if (!_gameplay.AdvancesGameplay || _generation != _gameplay.RunGeneration || _dps <= 0f) return false;
            float now = _gameplay.ElapsedSeconds;
            if (!flush && now < _nextTick && now < _until) return false;
            float sample = Math.Min(now, _until);
            damage = Math.Max(0f, sample - _sample) * _dps;
            _sample = sample; _nextTick = sample + .5f;
            if (now >= _until) _dps = 0f;
            return damage > 0f;
        }
        public void Reset() { _generation = _gameplay.RunGeneration; _dps = _until = _sample = _nextTick = 0f; }
    }
}
