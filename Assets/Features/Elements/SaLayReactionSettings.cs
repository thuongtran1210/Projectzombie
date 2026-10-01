using UnityEngine;

namespace ProjectZombie.Features.Elements
{
    [CreateAssetMenu(menuName = "ProjectZombie/Elements/Sa Lay Settings", fileName = "SaLayReactionSettings")]
    public sealed class SaLayReactionSettings : ScriptableObject
    {
        [Header("Mud Patch Parameters")]
        [Tooltip("Bán kính vũng bùn Sa Lầy (chuẩn GDD: 1.4m)")]
        [Min(0.5f)] public float mudRadius = 1.4f;

        [Tooltip("Thời gian tồn tại của vũng bùn (chuẩn GDD: 2.0s)")]
        [Min(0.5f)] public float mudDuration = 2.0f;

        [Tooltip("Tỉ lệ làm chậm khi ở trong vũng bùn (chuẩn GDD: 30% - 35%)")]
        [Range(0.1f, 0.9f)] public float slowAmount = 0.35f;

        [Tooltip("Số lượng vũng bùn tối đa hoạt động cùng lúc")]
        [Range(1, 32)] public int patchCapacity = 8;

        [Tooltip("Kích thước buffer NonAlloc quét quái trong vùng")]
        [Range(8, 64)] public int colliderBufferSize = 24;

        public GameObject mudVisualPrefab;

        private static SaLayReactionSettings _runtime;

        public static SaLayReactionSettings Runtime
        {
            get
            {
                if (_runtime == null)
                    _runtime = Resources.Load<SaLayReactionSettings>("SaLayReactionSettings");
                if (_runtime == null)
                    _runtime = CreateInstance<SaLayReactionSettings>();
                return _runtime;
            }
        }

        private void OnValidate()
        {
            mudRadius = Mathf.Max(0.5f, mudRadius);
            mudDuration = Mathf.Max(0.5f, mudDuration);
            slowAmount = Mathf.Clamp(slowAmount, 0.1f, 0.9f);
            patchCapacity = Mathf.Clamp(patchCapacity, 1, 32);
            colliderBufferSize = Mathf.Clamp(colliderBufferSize, 8, 64);
        }
    }
}
