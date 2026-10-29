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

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime);
        }
    }
}
