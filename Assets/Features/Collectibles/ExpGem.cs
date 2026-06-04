using UnityEngine;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Collectibles
{
    /// <summary>
    /// An experience gem dropped by enemies. It flies towards the player when they are in range.
    /// </summary>
    public class ExpGem : MonoBehaviour
    {
        [SerializeField] private float expAmount = 10f;
        [SerializeField] private float flySpeed = 10f;

        private Transform _targetPlayer;
        private bool _isFlyingToPlayer = false;

        private void Update()
        {
            if (_isFlyingToPlayer && _targetPlayer != null)
            {
                // Fly towards the player
                transform.position = Vector3.MoveTowards(transform.position, _targetPlayer.position, flySpeed * Time.deltaTime);

                // Check distance for collection. Using Vector2 to ignore Z-axis differences in 2D.
                if (Vector2.Distance(transform.position, _targetPlayer.position) < 1.0f)
                {
                    Collect();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!_isFlyingToPlayer && collision.CompareTag("Player"))
            {
                // Assuming Player has PlayerStats to get PickupRange
                _targetPlayer = collision.transform;
                _isFlyingToPlayer = true;
            }
        }
        
        private void OnTriggerEnter(Collider collision)
        {
            if (!_isFlyingToPlayer && collision.CompareTag("Player"))
            {
                _targetPlayer = collision.transform;
                _isFlyingToPlayer = true;
            }
        }

        private void Collect()
        {
            if (_targetPlayer != null)
            {
                var playerExp = _targetPlayer.GetComponent<PlayerExperience>();
                if (playerExp != null)
                {
                    playerExp.AddExp(expAmount);
                }
            }
            
            // For now, just destroy the object. In a real game, use Object Pooling.
            Destroy(gameObject);
        }
        
        // This can be used to set exp amount based on enemy type
        public void SetExpAmount(float amount)
        {
            expAmount = amount;
        }
    }
}
