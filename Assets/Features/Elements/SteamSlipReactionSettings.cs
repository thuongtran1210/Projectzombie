using UnityEngine;

namespace ProjectZombie.Features.Elements
{
    [CreateAssetMenu(menuName = "ProjectZombie/Elements/Steam Slip Settings", fileName = "SteamSlipReactionSettings")]
    public sealed class SteamSlipReactionSettings : ScriptableObject
    {
        [Header("Patch")]
        [Min(0.1f)] public float patchRadius = 0.8f;
        [Min(0.1f)] public float patchLifetime = 2f;
        [Range(1, 64)] public int patchCapacity = 16;
        [Range(1, 128)] public int colliderBufferSize = 32;
        [Range(1, 8)] public int secondaryTargetCap = 2;
        [Range(0, 8)] public int collisionImpactCap = 2;
        public GameObject patchVisualPrefab;

        [Header("Enemy Responses")]
        [Min(0f)] public float lightKnockbackForce = 2.5f;
        [Min(0.01f)] public float lightSlideDuration = 0.8f;
        [Min(0.01f)] public float standardStumbleDuration = 0.35f;
        [Min(0.01f)] public float heavyStaggerDuration = 0.2f;
        [Min(0.01f)] public float flyingWobbleDuration = 0.25f;
        [Min(0.01f)] public float collisionStaggerDuration = 0.15f;

        private static SteamSlipReactionSettings _runtime;
        public static SteamSlipReactionSettings Runtime
        {
            get
            {
                if (_runtime == null)
                    _runtime = Resources.Load<SteamSlipReactionSettings>("SteamSlipReactionSettings");
                return _runtime;
            }
        }

        private void OnValidate()
        {
            patchCapacity = Mathf.Max(1, patchCapacity);
            colliderBufferSize = Mathf.Max(1, colliderBufferSize);
            secondaryTargetCap = Mathf.Clamp(secondaryTargetCap, 1, 8);
            collisionImpactCap = Mathf.Clamp(collisionImpactCap, 0, 8);
        }
    }
}
