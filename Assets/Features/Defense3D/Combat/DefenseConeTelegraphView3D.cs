using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Authored overlay quad. The shader and damage snapshot use the same radius/angle.</summary>
    public sealed class DefenseConeTelegraphView3D : MonoBehaviour
    {
        [SerializeField] private Transform _indicator;
        [SerializeField] private Renderer _fill;
        [SerializeField] private LayerMask _surfaceMask = 4112;
        [SerializeField] private float _surfaceLift = 0.035f;
        private MaterialPropertyBlock _block;
        private void Awake() { _block = new MaterialPropertyBlock(); Hide(); }
        public void ValidateBindings()
        {
            if (_indicator == null || _fill == null || _indicator.parent != transform || _fill.transform != _indicator)
                throw new InvalidOperationException("Cone view requires a direct authored child quad and its Renderer.");
            Vector3 scale = transform.lossyScale;
            if (scale.x <= 0f || scale.z <= 0f || !Mathf.Approximately(scale.x, scale.z))
                throw new InvalidOperationException("Cone overlay requires positive uniform XZ parent scale.");
        }
        public void Show(DefenseConeSnapshot cone)
        {
            ValidateBindings();
            if (_block == null) _block = new MaterialPropertyBlock();
            Vector3 origin = new Vector3(cone.Origin.X, cone.Origin.Y, cone.Origin.Z);
            Vector3 direction = new Vector3(cone.Forward.X, 0f, cone.Forward.Z);
            float y = origin.y;
            if (Physics.Raycast(origin + Vector3.up, Vector3.down, out var hit, 20f, _surfaceMask, QueryTriggerInteraction.Ignore)) y = hit.point.y;
            Vector3 scale = transform.lossyScale;
            if (scale.x <= 0f || scale.z <= 0f || !Mathf.Approximately(scale.x, scale.z))
                throw new InvalidOperationException("Cone overlay requires a positive uniform XZ parent scale.");
            _indicator.position = new Vector3(origin.x, y + _surfaceLift, origin.z);
            _indicator.rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
            _indicator.localScale = new Vector3(2f * cone.Radius / scale.x, 2f * cone.Radius / scale.z, 1f);
            _block.SetFloat("_SectorCos", Mathf.Cos(cone.Angle * 0.5f * Mathf.Deg2Rad));
            _fill.SetPropertyBlock(_block);
            _indicator.gameObject.SetActive(true);
        }
        public void Hide() { if (_indicator != null) _indicator.gameObject.SetActive(false); }
        private void OnDisable() => Hide();
    }
}
