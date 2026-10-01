using UnityEngine;

namespace ProjectZombie.Features.Elements
{
    [CreateAssetMenu(menuName = "ProjectZombie/Elements/Sinh Truong Settings", fileName = "SinhTruongReactionSettings")]
    public sealed class SinhTruongReactionSettings : ScriptableObject
    {
        [Header("Root Duration")]
        [Tooltip("Thời gian trói chân (giữ nguyên khả năng đánh). Chuẩn GDD: 1.0s - 1.25s")]
        [Min(0.1f)] public float rootDuration = 1.25f;

        [Tooltip("Thời gian khựng tối thiểu khi gặp Boss (giảm còn 0.25s)")]
        [Min(0f)] public float bossRootDuration = 0.25f;

        public GameObject rootVisualPrefab;

        private static SinhTruongReactionSettings _runtime;

        public static SinhTruongReactionSettings Runtime
        {
            get
            {
                if (_runtime == null)
                    _runtime = Resources.Load<SinhTruongReactionSettings>("SinhTruongReactionSettings");
                if (_runtime == null)
                    _runtime = CreateInstance<SinhTruongReactionSettings>();
                return _runtime;
            }
        }

        private void OnValidate()
        {
            rootDuration = Mathf.Max(0.1f, rootDuration);
            bossRootDuration = Mathf.Max(0f, bossRootDuration);
        }
    }
}
