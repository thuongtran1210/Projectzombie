using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Scene-owned, bounded projectile simulation. All geometry is authored and reused.</summary>
    public sealed class DefenseTowerProjectileVfx3D : DefenseGameplayBehaviour, IDefenseProjectileLauncher
    {
        [SerializeField] private DefenseCombatVfxProfileSO _profile;
        [SerializeField] private LineRenderer[] _arrows = Array.Empty<LineRenderer>();
        [SerializeField] private LineRenderer[] _trails = Array.Empty<LineRenderer>();
        [SerializeField] private LineRenderer[] _impacts = Array.Empty<LineRenderer>();
        private Shot[] _shots;
        private float[] _impactEnds;
        private Vector3[] _impactOrigins;
        private struct Shot
        {
            public IDefenseEnemyTarget3D target;
            public DefenseActorLifetime lifetime;
            public DefenseEnemy3D enemy;
            public Component source;
            public Vector3 start;
            public float time, damage;
            public int generation;
            public object session;
        }
        private void Awake()
        {
            _shots = new Shot[_arrows.Length];
            _impactEnds = new float[_impacts.Length];
            _impactOrigins = new Vector3[_impacts.Length];
            Clear();
        }

        public bool TryLaunch(DefenseProjectileLaunchRequest request)
        {
            return TryLaunch(request.Source, request.Target, request.Damage);
        }

        public bool TryLaunch(Component source, IDefenseEnemyTarget3D target, float damage)
        {
            if (!CanAdvanceGameplay || !isActiveAndEnabled || _profile == null || _shots == null || target == null) return false;
            for (int i = 0; i < _shots.Length; i++)
            {
                if (_shots[i].target != null) continue;
                var enemy = target as DefenseEnemy3D;
                _shots[i] = new Shot { target = target, enemy = enemy, lifetime = enemy != null ? enemy.Lifetime : null,
                    source = source, start = source.transform.position + Vector3.up * 1.65f,
                    time = GameplayTime, damage = damage, generation = Gameplay.RunGeneration, session = Gameplay.SessionIdentity };
                _arrows[i].enabled = true;
                _trails[i].enabled = true;
                Draw(i, _shots[i].start, Vector3.forward, 0f);
                return true;
            }
            return false; // Tower retains an immediate-hit fallback when authored capacity is exhausted.
        }
        private bool IsValid(Shot shot) => shot.target != null && shot.target.TargetTransform != null &&
            shot.target.IsTargetable && shot.generation == Gameplay.RunGeneration &&
            ReferenceEquals(shot.session, Gameplay.SessionIdentity) &&
            (shot.lifetime == null || (shot.enemy != null && ReferenceEquals(shot.enemy.Lifetime, shot.lifetime) && shot.lifetime.IsCurrent(Gameplay)));
        private void Update()
        {
            if (!CanAdvanceGameplay || _profile == null) return;
            for (int i = 0; i < _shots.Length; i++)
            {
                var shot = _shots[i];
                if (shot.target == null) continue;
                if (!IsValid(shot)) { Release(i); continue; }
                float age = Mathf.Clamp01((GameplayTime - shot.time) / Mathf.Max(.05f, _profile.flightDuration));
                Vector3 end = shot.target.TargetTransform.position + Vector3.up * .65f;
                Vector3 position = Vector3.Lerp(shot.start, end, age) + Vector3.up * (4f * _profile.flightArc * age * (1f - age));
                Vector3 tangent = end - shot.start + Vector3.up * (4f * _profile.flightArc * (1f - 2f * age));
                Draw(i, position, tangent.normalized, age);
                if (age < 1f) continue;
                // Release before damage callbacks, which can synchronously end the run and clear every slot.
                Release(i);
                if (shot.target.TakeDamage(new DefenseDamageContext(shot.damage, shot.source, end,
                    runGeneration: shot.generation, sessionIdentity: shot.session)) && CanAdvanceGameplay) Impact(end);
            }
            for (int i = 0; i < _impacts.Length; i++)
            {
                if (_impactEnds[i] <= 0f) continue;
                float t = 1f - (_impactEnds[i] - GameplayTime) / Mathf.Max(.05f, _profile.impactDuration);
                if (t >= 1f) { _impacts[i].enabled = false; _impactEnds[i] = 0f; continue; }
                var line = _impacts[i];
                line.widthMultiplier = .045f * (1f - t);
                for (int p = 0; p < 24; p++)
                {
                    float angle = (p / 3) * Mathf.PI / 4f;
                    var ray = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle) * .7f, Mathf.Sin(angle));
                    line.SetPosition(p, _impactOrigins[i] + ray * (p % 3 == 1 ? .17f + t * .4f : 0f));
                }
            }
        }
        private void Draw(int i, Vector3 position, Vector3 direction, float age)
        {
            Vector3 side = Vector3.Cross(direction, Vector3.up).normalized;
            if (side.sqrMagnitude < .01f) side = Vector3.right;
            var arrow = _arrows[i];
            arrow.SetPosition(0, position - direction * .48f);
            arrow.SetPosition(1, position);
            arrow.SetPosition(2, position - direction * .14f + side * .085f);
            arrow.SetPosition(3, position);
            arrow.SetPosition(4, position - direction * .14f - side * .085f);
            var trail = _trails[i];
            trail.SetPosition(0, position - direction * (.48f + Mathf.Min(age * 3f, .75f)));
            trail.SetPosition(1, position - direction * .42f);
        }
        private void Impact(Vector3 position)
        {
            for (int i = 0; i < _impacts.Length; i++)
            {
                if (_impactEnds[i] > GameplayTime) continue;
                _impactOrigins[i] = position;
                _impactEnds[i] = GameplayTime + _profile.impactDuration;
                _impacts[i].enabled = true;
                for (int p = 0; p < 24; p++) _impacts[i].SetPosition(p, position);
                break;
            }
        }
        private void Release(int i) { _shots[i] = default; _arrows[i].enabled = false; _trails[i].enabled = false; }
        private void Clear()
        {
            if (_shots == null) return;
            for (int i = 0; i < _shots.Length; i++) Release(i);
            for (int i = 0; i < _impacts.Length; i++) { _impacts[i].enabled = false; _impactEnds[i] = 0f; }
        }
        protected override void OnGameplayCleanup() => Clear();
        protected override void OnGameplayReset() => Clear();
        private void OnDisable() => Clear();
    }
}
