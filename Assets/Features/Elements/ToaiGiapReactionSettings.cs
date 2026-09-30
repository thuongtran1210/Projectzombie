using UnityEngine;

namespace ProjectZombie.Features.Elements
{
    [CreateAssetMenu(menuName = "ProjectZombie/Elements/Toai Giap Settings", fileName = "ToaiGiapReactionSettings")]
    public sealed class ToaiGiapReactionSettings : ScriptableObject
    {
        [Header("Primary Target")]
        [Min(0f)] public float staggerDuration = 0.2f;

        [Header("Stone Fragments")]
        [Min(0.1f)] public float fragmentRadius = 1f;
        [Min(0f)] public float fragmentDamageMultiplier = 0.25f;
        [Range(0, 8)] public int secondaryTargetCap = 2;
        [Range(1, 64)] public int colliderBufferSize = 24;
        public GameObject fragmentVisualPrefab;

        private static ToaiGiapReactionSettings _runtime;

        public static ToaiGiapReactionSettings Runtime
        {
            get
            {
                if (_runtime == null)
                    _runtime = Resources.Load<ToaiGiapReactionSettings>("ToaiGiapReactionSettings");
                return _runtime;
            }
        }

        private void OnValidate()
        {
            staggerDuration = Mathf.Max(0f, staggerDuration);
            fragmentRadius = Mathf.Max(0.1f, fragmentRadius);
            fragmentDamageMultiplier = Mathf.Max(0f, fragmentDamageMultiplier);
            secondaryTargetCap = Mathf.Clamp(secondaryTargetCap, 0, 8);
            colliderBufferSize = Mathf.Clamp(colliderBufferSize, 1, 64);
        }
    }
}
