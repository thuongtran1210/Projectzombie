using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    [CreateAssetMenu(menuName="Defense3D/Ground Navigation Profile")]
    public sealed class DefenseGroundNavigationProfileSO : ScriptableObject
    {
        [SerializeField] private float _westEdge=4.45f;
        [SerializeField] private float _eastEdge=8.05f;
        [SerializeField] private float _eastEntry=8.8f;
        [SerializeField] private float _bridgeCenter=6.25f;
        [SerializeField] private float _westExit=3.8f;
        [SerializeField] private float _northZ=-10.5f;
        [SerializeField] private float _middleZ=-0.5f;
        [SerializeField] private float _southZ=10.5f;
        [SerializeField] private float _acquireRange=4.2f;
        [SerializeField] private float _retainRange=7f;
        [SerializeField] private float _targetLockSeconds=0.75f;
        [SerializeField] private float _repathSeconds=0.25f;
        [SerializeField] private float _provokedSeconds=3f;
        [SerializeField] private float _bridgeHalfWidth=1.4f;
        public DefenseGroundNavigationRules Capture() => new DefenseGroundNavigationRules(_westEdge,_eastEdge,_eastEntry,
            _bridgeCenter,_westExit,_northZ,_middleZ,_southZ,_acquireRange,_retainRange,_targetLockSeconds,_repathSeconds,_provokedSeconds,_bridgeHalfWidth);
    }
}
