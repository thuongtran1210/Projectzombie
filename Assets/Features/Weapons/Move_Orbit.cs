using UnityEngine;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Component hành vi di chuyển: Xoay tròn quanh một tâm điểm liên tục.
    /// </summary>
    [RequireComponent(typeof(ProjectileCore))]
    public class Move_Orbit : MonoBehaviour
    {
        private Transform _centerPoint;
        private float _radius;
        private float _orbitSpeed;
        private float _currentAngle;

        /// <summary>
        /// Khởi tạo quỹ đạo xoay.
        /// </summary>
        public void Initialize(Transform centerPoint, float radius, float orbitSpeed, float startAngle)
        {
            _centerPoint = centerPoint;
            _radius = radius;
            _orbitSpeed = orbitSpeed;
            _currentAngle = startAngle;
        }

        public void UpdateOrbitSpeed(float newSpeed)
        {
            _orbitSpeed = newSpeed;
        }

        public void UpdateRadius(float newRadius)
        {
            _radius = newRadius;
        }

        private void Update()
        {
            if (_centerPoint == null) 
            {
                Debug.LogWarning("[Move_Orbit] CenterPoint bị NULL! Quả cầu không thể di chuyển.");
                return;
            }

            _currentAngle += _orbitSpeed * Time.deltaTime;
            
            // Giữ góc trong khoảng 0-360 để tránh tràn số
            if (_currentAngle > 360f) _currentAngle -= 360f;
            else if (_currentAngle < 0f) _currentAngle += 360f;

            float rad = _currentAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * _radius;
            transform.position = _centerPoint.position + offset;
        }
    }
}
