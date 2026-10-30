using UnityEngine;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Runtime camera follow for the Unity 3D migration sandbox.
    /// Must live outside Assets/Editor so generated scenes work in player builds.
    /// </summary>
    public sealed class Camera3DFollower : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 18f, -13f);
        public float smoothSpeed = 10f;
        private Vector3 _impulseOffset;
        private float _impulseRemaining, _impulseDuration, _impulseAmplitude;

        /// <summary>Plays a bounded, decaying impact without accumulating drift in the follow position.</summary>
        public void PlayImpulse(float amplitude, float duration)
        {
            if (!isActiveAndEnabled || amplitude <= 0f || duration <= 0f) return;
            _impulseAmplitude = amplitude;
            _impulseDuration = duration;
            _impulseRemaining = duration;
        }

        /// <summary>Removes the last offset immediately on interruption or session cleanup.</summary>
        public void ClearImpulse()
        {
            transform.position -= _impulseOffset;
            _impulseOffset = Vector3.zero;
            _impulseRemaining = 0f;
        }

        private void LateUpdate()
        {
            transform.position -= _impulseOffset;
            _impulseOffset = Vector3.zero;
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime);
            if (_impulseRemaining > 0f)
            {
                _impulseRemaining = Mathf.Max(0f, _impulseRemaining - Time.deltaTime);
                float progress = 1f - _impulseRemaining / _impulseDuration;
                float envelope = (1f - progress) * (1f - progress) * _impulseAmplitude;
                _impulseOffset = (transform.right * Mathf.Cos(progress * 31.41593f) +
                    transform.up * Mathf.Sin(progress * 25.13274f) * .45f) * envelope;
                transform.position += _impulseOffset;
            }
        }

        private void OnDisable() => ClearImpulse();
    }
}
