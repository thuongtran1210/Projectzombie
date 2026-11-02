using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Checks the capsule footprint against solid ground, independently of enemy NavMesh.</summary>
    public sealed class DefenseHeroGroundSafety3D
    {
        private readonly CapsuleCollider capsule;
        private readonly int groundMask;
        private Vector3 safePosition;
        private bool hasSafePosition;
        private float unsupportedTime;

        public DefenseHeroGroundSafety3D(CapsuleCollider capsule, int groundMask)
        { this.capsule = capsule; this.groundMask = groundMask; }

        public bool HasSupport(Vector3 root)
        {
            if (capsule == null || groundMask == 0) return false;
            Vector3 scale = capsule.transform.lossyScale;
            float radius = capsule.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float halfHeight = Mathf.Max(capsule.height * Mathf.Abs(scale.y) * .5f, radius);
            Vector3 foot = root + capsule.transform.TransformVector(capsule.center) - Vector3.up * halfHeight;
            float margin = Mathf.Max(.02f, radius - .04f);
            return Probe(foot) && Probe(foot + Vector3.right * margin) && Probe(foot - Vector3.right * margin)
                && Probe(foot + Vector3.forward * margin) && Probe(foot - Vector3.forward * margin);
        }

        private bool Probe(Vector3 foot)
        {
            return Physics.Raycast(foot + Vector3.up * .35f, Vector3.down, out var hit, .7f,
                groundMask, QueryTriggerInteraction.Ignore) && hit.normal.y >= .65f;
        }

        public Vector3 ConstrainVelocity(Vector3 root, Vector3 velocity, float step)
        {
            Vector3 delta = new Vector3(velocity.x, 0f, velocity.z) * step;
            if (!DefenseDamageRules.IsFinite(delta.x) || !DefenseDamageRules.IsFinite(delta.z) || delta.magnitude > 12.8f)
                return new Vector3(0f, velocity.y, 0f);
            // Sweep the whole intended movement so a high speed cannot skip a narrow gap.
            int samples = Mathf.Clamp(Mathf.CeilToInt(delta.magnitude / .1f), 1, 128);
            for (int i = 1; i <= samples; i++)
                if (!HasSupport(root + delta * ((float)i / samples)))
                {
                    bool x = SupportedPath(root, new Vector3(delta.x, 0f, 0f), samples);
                    bool z = SupportedPath(root, new Vector3(0f, 0f, delta.z), samples);
                    // Only permit one sliding axis; both individually valid can still cross a corner.
                    if (x && Mathf.Abs(delta.x) >= Mathf.Abs(delta.z)) return new Vector3(velocity.x, velocity.y, 0f);
                    if (z) return new Vector3(0f, velocity.y, velocity.z);
                    return new Vector3(0f, velocity.y, 0f);
                }
            return velocity;
        }

        private bool SupportedPath(Vector3 root, Vector3 delta, int samples)
        {
            for (int i = 1; i <= samples; i++)
                if (!HasSupport(root + delta * ((float)i / samples))) return false;
            return true;
        }

        public bool TryRecover(Rigidbody body, float step)
        {
            if (HasSupport(body.position))
            {
                safePosition = body.position; hasSafePosition = true; unsupportedTime = 0f;
                return false;
            }
            unsupportedTime += step;
            if (!hasSafePosition || unsupportedTime < .25f || !HasSupport(safePosition)) return false;
            body.position = safePosition;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            unsupportedTime = 0f;
            return true;
        }

        public void Reset() { hasSafePosition = false; unsupportedTime = 0f; }
    }
}
