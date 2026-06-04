using UnityEngine;

namespace ProjectZombie.Features.Enemies.Passives
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class FactionPassive_Titans : MonoBehaviour
    {
        private void Start()
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Kháng hiệu ứng đẩy lùi bằng cách tăng khối lượng lên cực lớn
                rb.mass = 9999f;
            }
        }
    }
}
