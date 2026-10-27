using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public sealed class DefenseDashSkillEffect3D : DefenseSkillEffect3D
    {
        [SerializeField] private DefenseHeroMotor3D _motor;
        [SerializeField] private DefenseDashTraversal3D _traversal;
        [SerializeField] private DefenseHeroHealth3D _health;
        public override DefenseSkillKind Kind => DefenseSkillKind.Dash;
        public override bool IsConfigured => _motor != null && _health != null && _traversal != null && _traversal.IsConfigured;

        public override bool TryPrepare(DefenseResolvedAim requested, out DefenseResolvedAim prepared)
        {
            prepared = default;
            if (!CanAdvanceGameplay || Definition == null || !IsConfigured || _health.IsDown || _motor.IsDashing || !_motor.isActiveAndEnabled ||
                !_traversal.TryResolve(_motor.transform.position, Vector(requested.Target), out var destination)) return false;
            var origin = Point(_motor.transform.position);
            if (!DefenseAimResolver.TryResolve(origin, Point(destination), Definition.Range, out var geometry)) return false;
            prepared = new DefenseResolvedAim(origin, Point(destination), geometry.Direction, geometry.Distance);
            return true;
        }

        public override void Execute(DefenseResolvedAim prepared)
        {
            _motor.GetComponent<DefenseHeroAutoAttack3D>()?.CancelPendingAttack();
            _motor.GetComponent<DefenseScholarSignatureEffect3D>()?.CancelCasting();
            _motor.BeginDash(Vector(prepared.Target), Definition.Duration, _traversal);
        }

        protected override void OnGameplayCleanup() => _motor?.CancelDash();
        protected override void OnGameplayReset() => _motor?.CancelDash();
        private static Vector3 Vector(DefensePoint3 point) => new Vector3(point.X, point.Y, point.Z);
        private static DefensePoint3 Point(Vector3 point) => new DefensePoint3(point.x, point.y, point.z);
    }
}
