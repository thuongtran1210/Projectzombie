using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Telegraph -> detonation -> recovery cycle. The visible red warning ring
    /// uses a LineRenderer (visible in Game View without Gizmos).
    /// </summary>
    public sealed class DefenseYakshaArenaCombat3D : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float blastRadius = 5f;
        [SerializeField, Min(0.2f)] private float blastInterval = 3f;
        [SerializeField, Min(0.1f)] private float warningDuration = 1.1f;
        [SerializeField, Min(0f)] private float blastDamage = 30f;
        private DefenseHeroHealth3D hero;
        private float nextBlastAt;
        private float detonateAt;
        private bool activeArena;
        private bool warning;
        private LineRenderer ring;
        private Material ringMaterial;
        private const int Segments = 64;

        public void Begin(DefenseHeroHealth3D heroHealth)
        {
            hero = heroHealth;
            activeArena = true;
            warning = false;
            nextBlastAt = Time.time + blastInterval;
            CreateTelegraph();
        }

        public void EndArena()
        {
            activeArena = false;
            warning = false;
            if (ring != null) ring.enabled = false;
        }

        private void CreateTelegraph()
        {
            var marker = new GameObject("Yaksha_Blast_Warning_Ring");
            marker.transform.SetParent(transform, false);
            ring = marker.AddComponent<LineRenderer>();
            ring.useWorldSpace = true;
            ring.loop = true;
            ring.positionCount = Segments;
            ring.widthMultiplier = 0.17f;
            ring.startColor = Color.red;
            ring.endColor = Color.red;
            ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ring.receiveShadows = false;
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            if (shader != null)
            {
                ringMaterial = new Material(shader);
                ringMaterial.color = new Color(1f, 0.12f, 0.08f);
                ring.material = ringMaterial;
            }
            // Compensate for the enlarged Boss transform.
            float radius = blastRadius;
            for (int i = 0; i < Segments; i++)
            {
                float angle = i * Mathf.PI * 2f / Segments;
                ring.SetPosition(i, transform.position + new Vector3(Mathf.Cos(angle) * radius, 0.35f, Mathf.Sin(angle) * radius));
            }
            ring.enabled = false;
        }

        private void Update()
        {
            if (!activeArena) return;
            if (!warning)
            {
                if (Time.time < nextBlastAt) return;
                warning = true;
                detonateAt = Time.time + warningDuration;
                if (ring != null)
                {
                    UpdateRingPosition();
                    ring.enabled = true;
                }
                Debug.Log("[TREBRAVE Defense3D] Yaksha warning: leave the red circle!");
                return;
            }
            if (Time.time < detonateAt) return;
            warning = false;
            if (ring != null) ring.enabled = false;
            nextBlastAt = Time.time + Mathf.Max(0.1f, blastInterval - warningDuration);
            if (hero == null || hero.IsDown) return;
            Vector3 delta = hero.transform.position - transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude > blastRadius * blastRadius) return;
            hero.ReceiveDamage(blastDamage);
            Debug.Log("[TREBRAVE Defense3D] Yaksha arena blast hit Hero for " + blastDamage);
        }

        private void UpdateRingPosition()
        {
            if (ring == null) return;
            for (int i = 0; i < Segments; i++)
            {
                float angle = i * Mathf.PI * 2f / Segments;
                ring.SetPosition(i, transform.position +
                    new Vector3(Mathf.Cos(angle) * blastRadius, 0.35f, Mathf.Sin(angle) * blastRadius));
            }
        }

        private void LateUpdate()
        {
            if (activeArena && warning) UpdateRingPosition();
        }

        private void OnDestroy()
        {
            if (ringMaterial != null) Destroy(ringMaterial);
        }
    }
}
