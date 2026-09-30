using UnityEngine;

namespace ProjectZombie.Features.Elements
{
    [CreateAssetMenu(menuName = "ProjectZombie/Elements/Reaction Rules", fileName = "ElementReactionSettings")]
    public sealed class ElementReactionSettings : ScriptableObject
    {
        [Header("Target Marks")]
        [Min(0.1f)] public float markDuration = 4f;

        [Header("Reaction Limits")]
        [Min(0f)] public float reactionCooldown = 1f;

        private static ElementReactionSettings _runtime;

        public static ElementReactionSettings Runtime
        {
            get
            {
                if (_runtime == null)
                    _runtime = Resources.Load<ElementReactionSettings>("ElementReactionSettings");
                return _runtime;
            }
        }

        private void OnValidate()
        {
            markDuration = Mathf.Max(0.1f, markDuration);
            reactionCooldown = Mathf.Max(0f, reactionCooldown);
        }
    }
}
