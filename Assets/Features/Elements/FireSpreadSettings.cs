using UnityEngine;

namespace ProjectZombie.Features.Elements
{
    [CreateAssetMenu(menuName = "ProjectZombie/Elements/Fire Spread Settings", fileName = "FireSpreadSettings")]
    public sealed class FireSpreadSettings : ScriptableObject
    {
        [Min(0.05f)] public float burnDuration = 3f;
        [Min(0.05f)] public float burnTickInterval = 1f;
        [Tooltip("Provisional fraction of the triggering Mộc hit used for each Burn tick.")]
        [Min(0f)] public float burnDamageMultiplier = 0.2f;
        [Min(0.1f)] public float spreadRadius = 1.25f;
        [Range(0, 8)] public int secondaryTargetCap = 2;
        [Range(1, 32)] public int colliderBufferSize = 16;

        private static FireSpreadSettings _runtime;
        public static FireSpreadSettings Runtime
        {
            get
            {
                if (_runtime == null)
                    _runtime = Resources.Load<FireSpreadSettings>("FireSpreadSettings");
                return _runtime;
            }
        }

        private void OnValidate()
        {
            burnDuration = Mathf.Max(0.05f, burnDuration);
            burnTickInterval = Mathf.Max(0.05f, burnTickInterval);
            burnDamageMultiplier = Mathf.Max(0f, burnDamageMultiplier);
            spreadRadius = Mathf.Max(0.1f, spreadRadius);
            secondaryTargetCap = Mathf.Clamp(secondaryTargetCap, 0, 8);
            colliderBufferSize = Mathf.Clamp(colliderBufferSize, 1, 32);
        }
    }
}
