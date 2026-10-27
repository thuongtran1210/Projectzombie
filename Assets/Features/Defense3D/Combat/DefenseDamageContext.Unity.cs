using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public partial struct DefenseDamageContext
    {
        /// <summary>Compatibility surface for Unity callers; core health only consumes the engine-free payload.</summary>
        public Vector3 HitPoint
        {
            get => new Vector3(WorldHitPoint.X, WorldHitPoint.Y, WorldHitPoint.Z);
            set => WorldHitPoint = new DefensePoint3(value.x, value.y, value.z);
        }

        public DefenseDamageContext(float amount, object source, Vector3 hitPoint, bool isCritical = false,
            int runGeneration = -1, object sessionIdentity = null)
            : this(amount, source, new DefensePoint3(hitPoint.x, hitPoint.y, hitPoint.z), isCritical, runGeneration, sessionIdentity) { }
    }
}
