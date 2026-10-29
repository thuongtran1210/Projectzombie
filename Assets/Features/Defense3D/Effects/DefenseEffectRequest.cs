using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Đầu vào trình diễn có hướng quay; không làm thay đổi hình học gây sát thương.</summary>
    public readonly struct DefenseEffectRequest
    {
        public readonly DefenseEffectProfileSO Profile;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly float Radius;
        public readonly Component Owner;
        public readonly Transform Anchor;
        public DefenseEffectRequest(DefenseEffectProfileSO profile, Vector3 position, float radius = 1f,
            Component owner = null, Transform anchor = null, Quaternion? rotation = null)
        { Profile = profile; Position = position; Radius = radius; Owner = owner; Anchor = anchor; Rotation = rotation ?? Quaternion.identity; }
    }
}
