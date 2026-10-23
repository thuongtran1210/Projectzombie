using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Yaksha arena attack prototype: visible danger radius, timed proximity blast.
    /// Stops permanently when the run enters ASSAULT_BASE or ends.
    /// </summary>
    public sealed class DefenseYakshaArenaCombat3D : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float blastRadius = 5f;
        [SerializeField, Min(0.1f)] private float blastInterval = 3f;
        [SerializeField, Min(0f)] private float blastDamage = 30f;
        private DefenseHeroHealth3D hero;
        private float nextBlastAt;
        private bool activeArena;

        public void Begin(DefenseHeroHealth3D heroHealth)
        {
            hero = heroHealth;
            activeArena = true;
            nextBlastAt = Time.time + blastInterval;
        }

        public void EndArena() { activeArena = false; }

        private void Update()
        {
            if (!activeArena || Time.time < nextBlastAt) return;
            nextBlastAt = Time.time + blastInterval;
            if (hero == null || hero.IsDown) return;
            Vector3 delta = hero.transform.position - transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude > blastRadius * blastRadius) return;
            hero.ReceiveDamage(blastDamage);
            Debug.Log("[TREBRAVE Defense3D] Yaksha arena blast hit Hero for " + blastDamage);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !activeArena) return;
            Gizmos.color = new Color(0.95f, 0.1f, 0.15f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, blastRadius);
        }
    }
}
