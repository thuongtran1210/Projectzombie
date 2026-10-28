using UnityEngine;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Passive, noninteractive billboard health bar over an authored friendly slot.</summary>
    public sealed class DefenseMinionHealthView3D : MonoBehaviour
    {
        [SerializeField] private Transform _fill;
        [SerializeField] private Camera _camera;
        [SerializeField] private float _width = .65f;
        [SerializeField] private float _height = .055f;
        public void SetRatio(float ratio)
        {
            ratio = Mathf.Clamp01(ratio);
            _fill.localScale = new Vector3(_width * ratio, _height, 1f);
            _fill.localPosition = new Vector3(-_width * .5f + _width * ratio * .5f, 0f, -.005f);
        }

        private void LateUpdate()
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera != null) transform.rotation = _camera.transform.rotation;
        }
    }
}
