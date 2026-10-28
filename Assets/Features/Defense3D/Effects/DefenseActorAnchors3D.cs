using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Các điểm gắn được gán sẵn trong prefab; không tìm Transform theo tên khi combat.</summary>
    [DisallowMultipleComponent]
    public sealed class DefenseActorAnchors3D : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;
        [SerializeField] private Transform _hit;
        [SerializeField] private Transform _head;
        [SerializeField] private Transform _cast;
        [SerializeField] private Transform _ground;
        public Transform VisualRoot => _visualRoot;
        public Transform Hit => _hit;
        public Transform Head => _head;
        public Transform Cast => _cast;
        public Transform Ground => _ground;
        public bool IsConfigured => IsChild(_visualRoot) && IsChild(_hit) && IsChild(_head) && IsChild(_cast) && IsChild(_ground);
        private bool IsChild(Transform point) => point != null && point != transform && point.IsChildOf(transform);
    }
}
