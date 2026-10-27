using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>One authored Hero/Support acquisition policy reused by Ground, grounded amphibians, Landing and Air.</summary>
    [CreateAssetMenu(menuName = "TREBRAVE/Defense3D/Aggro Profile", fileName = "Aggro_SongQue")]
    public sealed class DefenseAggroProfileSO : ScriptableObject
    {
        [SerializeField] private float _acquireRange = 4.2f;
        [SerializeField] private float _retainRange = 7f;
        [SerializeField] private float _lockSeconds = 0.75f;
        [SerializeField] private float _thinkSeconds = 0.25f;
        [SerializeField] private float _provokedSeconds = 3f;
        public DefenseAggroRules Capture() => new DefenseAggroRules(_acquireRange, _retainRange, _lockSeconds, _thinkSeconds, _provokedSeconds);
    }
}
