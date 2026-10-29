using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Plays the imported Scholar poses from the existing Defense gameplay state.</summary>
    /// <summary>
    /// Triển khai cụ thể của Visual Controller cho nhân vật Thư Sinh (Scholar).
    /// Kế thừa DefenseHeroVisualController3D để đảm bảo 100% tương thích ngược với các Scene/Prefab hiện có.
    /// </summary>
    public sealed class DefenseScholarVisual3D : DefenseHeroVisualController3D
    {
        [SerializeField] private DefenseScholarSignatureEffect3D _signature;

        protected override void Awake()
        {
            base.Awake();
            if (_signatureEffect == null && _signature != null)
            {
                _signatureEffect = _signature;
            }
        }
    }
}
