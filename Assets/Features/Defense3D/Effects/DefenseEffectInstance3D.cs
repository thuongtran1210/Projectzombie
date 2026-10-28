using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>View thụ động; service cập nhật khi active và giữ toàn bộ quyền trả pool.</summary>
    public sealed class DefenseEffectInstance3D : MonoBehaviour
    {
        [SerializeField] private DefenseEffectVisual3D[] _visuals;
        [SerializeField] private ParticleSystem[] _particles;
        [SerializeField] private TrailRenderer[] _trails;
        private Transform[] _nodes;
        private Vector3[] _positions, _scales;
        private Quaternion[] _rotations;
        public bool IsConfigured
        {
            get
            {
                if (_visuals != null) foreach (var visual in _visuals) if (visual != null && visual.IsConfigured) return true;
                if (_particles != null) foreach (var particle in _particles) if (particle != null) return true;
                return false;
            }
        }
        private void Cache()
        {
            if (_nodes != null) return;
            // Cache một lần để không bỏ sót particle/trail con khi reset prefab được mở rộng.
            _particles = GetComponentsInChildren<ParticleSystem>(true);
            _trails = GetComponentsInChildren<TrailRenderer>(true);
            _nodes = GetComponentsInChildren<Transform>(true);
            _positions = new Vector3[_nodes.Length]; _scales = new Vector3[_nodes.Length]; _rotations = new Quaternion[_nodes.Length];
            for (int i = 0; i < _nodes.Length; i++)
            { _positions[i] = _nodes[i].localPosition; _scales[i] = _nodes[i].localScale; _rotations[i] = _nodes[i].localRotation; }
        }
        public void Prepare(Color color, float diameter)
        {
            Cache(); ResetPresentation();
            if (_visuals != null) foreach (var visual in _visuals) if (visual != null && visual.IsConfigured) visual.Prepare(color, diameter);
        }
        public void Begin()
        {
            gameObject.SetActive(true);
            if (_particles != null) foreach (var particle in _particles) if (particle != null) particle.Play(true);
            if (_trails != null) foreach (var trail in _trails) if (trail != null) { trail.Clear(); trail.emitting = true; }
        }
        public void SetPaused(bool paused)
        {
            if (_particles == null) return;
            foreach (var particle in _particles)
            {
                if (particle == null) continue;
                if (paused) particle.Pause(true);
                else if (particle.isPaused) particle.Play(true);
            }
        }
        public void Render(float progress)
        {
            if (_visuals != null) foreach (var visual in _visuals)
                if (visual != null && visual.IsConfigured) visual.Render(progress);
        }
        public void ResetPresentation()
        {
            Cache();
            if (_particles != null) foreach (var particle in _particles) if (particle != null) particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_trails != null) foreach (var trail in _trails) if (trail != null) { trail.emitting = false; trail.Clear(); }
            if (_visuals != null) foreach (var visual in _visuals) if (visual != null) visual.ResetVisual();
            for (int i = 1; i < _nodes.Length; i++)
                if (_nodes[i] != null) { _nodes[i].localPosition = _positions[i]; _nodes[i].localRotation = _rotations[i]; _nodes[i].localScale = _scales[i]; }
            transform.localScale = Vector3.one;
            gameObject.SetActive(false);
        }
    }
}
