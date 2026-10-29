using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Bounded source cadence: no catch-up burst, no new shots after destruction or launch cutoff.</summary>
    public sealed class DefenseCannonSchedule
    {
        private readonly float _first, _interval, _stop;
        private float _next;
        public DefenseCannonSchedule(float first, float interval, float stop)
        {
            if (!DefenseDamageRules.IsFinite(first) || first < 0f || !DefenseDamageRules.IsFinite(interval) || interval <= 0f ||
                !DefenseDamageRules.IsFinite(stop) || stop <= 0f) throw new ArgumentException("Invalid cannon cadence.");
            _first = first; _interval = interval; _stop = stop; Reset();
        }
        public bool TryFire(float time, bool advances, bool sourceAlive, bool projectileInFlight)
        {
            if (!advances || !sourceAlive || projectileInFlight || !DefenseDamageRules.IsFinite(time) || time < _next || time >= _stop) return false;
            _next = time + _interval;
            return true;
        }
        public void Reset() => _next = _first;
    }

    public enum DefenseLandingPhase { Ballistic, Parachuting, Landed, Cancelled }

    /// <summary>Scoped, once-only landing delivery driven entirely by the shared gameplay clock.</summary>
    public sealed class DefenseLandingFlight
    {
        private readonly object _session;
        private readonly int _generation;
        private readonly float _started, _ballistic, _parachute, _arc, _openHeight;
        private readonly DefensePoint3 _origin, _landing;
        private bool _delivered;
        public DefenseLandingPhase Phase { get; private set; }
        public DefensePoint3 Position { get; private set; }

        public DefenseLandingFlight(IDefenseGameplayContext gameplay, DefensePoint3 origin, DefensePoint3 landing,
            float ballistic, float parachute, float arc, float openHeight)
        {
            if (gameplay == null || !gameplay.AdvancesGameplay || !origin.IsFinite || !landing.IsFinite ||
                !DefenseDamageRules.IsFinite(ballistic) || ballistic <= 0f || !DefenseDamageRules.IsFinite(parachute) || parachute <= 0f ||
                !DefenseDamageRules.IsFinite(arc) || arc < 0f || !DefenseDamageRules.IsFinite(openHeight) || openHeight <= landing.Y)
                throw new ArgumentException("Landing flight requires a live scope, finite points and positive durations/heights.");
            _session = gameplay.SessionIdentity; _generation = gameplay.RunGeneration; _started = gameplay.ElapsedSeconds;
            _origin = origin; _landing = landing; _ballistic = ballistic; _parachute = parachute; _arc = arc; _openHeight = openHeight;
            Position = origin;
        }

        public void Advance(IDefenseGameplayContext gameplay)
        {
            if (Phase == DefenseLandingPhase.Cancelled) return;
            if (gameplay == null || !ReferenceEquals(_session, gameplay.SessionIdentity) || _generation != gameplay.RunGeneration)
            {
                Cancel();
                return;
            }
            if (!gameplay.AdvancesGameplay) return;
            float age = Math.Max(0f, gameplay.ElapsedSeconds - _started);
            if (age < _ballistic)
            {
                float t = age / _ballistic;
                Position = new DefensePoint3(_origin.X + (_landing.X - _origin.X) * t,
                    _origin.Y + (_openHeight - _origin.Y) * t + 4f * _arc * t * (1f - t),
                    _origin.Z + (_landing.Z - _origin.Z) * t);
                Phase = DefenseLandingPhase.Ballistic;
            }
            else if (age < _ballistic + _parachute)
            {
                float t = (age - _ballistic) / _parachute;
                Position = new DefensePoint3(_landing.X, _openHeight + (_landing.Y - _openHeight) * t, _landing.Z);
                Phase = DefenseLandingPhase.Parachuting;
            }
            else
            {
                Position = _landing;
                Phase = DefenseLandingPhase.Landed;
            }
        }

        public bool TryTakeLanding(IDefenseGameplayContext gameplay, out DefensePoint3 position)
        {
            Advance(gameplay);
            position = Position;
            if (gameplay == null || !gameplay.AdvancesGameplay || Phase != DefenseLandingPhase.Landed || _delivered) return false;
            _delivered = true;
            return true;
        }
        public void Cancel() { Phase = DefenseLandingPhase.Cancelled; _delivered = true; }
    }
}
