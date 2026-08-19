using UnityEngine;
using ProjectZombie.Features.Collectibles;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Component gắn trên Player chuyên trách quản lý vùng từ trường (Magnet Area) hút ExpGem và Vật Phẩm.
    /// Tự động cập nhật bán kính Trigger Collider theo chỉ số PlayerStats.PickupRange.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerMagnetTrigger : MonoBehaviour
    {
        [Header("Magnet Settings")]
        [Tooltip("Bán kính hút cơ bản (tính bằng đơn vị thế giới Unity)")]
        [SerializeField] private float _baseMagnetRadius = 2.5f;

        private PlayerStats _playerStats;
        private CircleCollider2D _magnetCollider;

        private void Awake()
        {
            _playerStats = GetComponent<PlayerStats>();
            SetupMagnetCollider();
        }

        private void Start()
        {
            UpdateMagnetRadius();
            if (_playerStats != null)
            {
                _playerStats.OnStatsUpdated += UpdateMagnetRadius;
            }
        }

        private void OnDestroy()
        {
            if (_playerStats != null)
            {
                _playerStats.OnStatsUpdated -= UpdateMagnetRadius;
            }
        }

        private void SetupMagnetCollider()
        {
            // Tạo một child GameObject chuyên dụng để phân tách hoàn toàn Trigger Magnet với Body Collider của Player
            Transform magnetChild = transform.Find("[MagnetArea]");
            if (magnetChild == null)
            {
                var childObj = new GameObject("[MagnetArea]");
                childObj.transform.SetParent(transform, false);
                childObj.tag = "Player";

                int pickupLayer = LayerMask.NameToLayer("Pickup");
                if (pickupLayer < 0) pickupLayer = LayerMask.NameToLayer("Ignore Raycast");
                if (pickupLayer >= 0) childObj.layer = pickupLayer;

                magnetChild = childObj.transform;
            }
            else
            {
                magnetChild.tag = "Player";
                int pickupLayer = LayerMask.NameToLayer("Pickup");
                if (pickupLayer < 0) pickupLayer = LayerMask.NameToLayer("Ignore Raycast");
                if (pickupLayer >= 0) magnetChild.gameObject.layer = pickupLayer;
            }

            _magnetCollider = magnetChild.GetComponent<CircleCollider2D>();
            if (_magnetCollider == null)
            {
                _magnetCollider = magnetChild.gameObject.AddComponent<CircleCollider2D>();
            }

            _magnetCollider.isTrigger = true;

            var proxy = magnetChild.GetComponent<MagnetTriggerProxy>();
            if (proxy == null)
            {
                proxy = magnetChild.gameObject.AddComponent<MagnetTriggerProxy>();
            }
            proxy.Init(this);
        }

        public void UpdateMagnetRadius()
        {
            if (_magnetCollider == null || _playerStats == null) return;

            // pickupRange: 100 tương đương 100% (1.0x) base radius
            float rangeFactor = _playerStats.PickupRange > 0f ? (_playerStats.PickupRange / 100f) : 1f;
            _magnetCollider.radius = _baseMagnetRadius * rangeFactor;
        }

        /// <summary>
        /// Xử lý khi có ExpGem đi vào vùng hút từ trường.
        /// </summary>
        public void OnGemDetected(ExpGem gem)
        {
            if (gem != null)
            {
                gem.StartMagnetEffect(transform);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.35f);
            float radius = _baseMagnetRadius;
            if (_playerStats != null && _playerStats.PickupRange > 0f)
            {
                radius = _baseMagnetRadius * (_playerStats.PickupRange / 100f);
            }
            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif
    }

    /// <summary>
    /// Proxy chuyển tiếp sự kiện va chạm từ Child GameObject [MagnetArea] lên PlayerMagnetTrigger.
    /// </summary>
    public class MagnetTriggerProxy : MonoBehaviour
    {
        private PlayerMagnetTrigger _parentMagnet;

        public void Init(PlayerMagnetTrigger parent)
        {
            _parentMagnet = parent;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_parentMagnet == null) return;

            if (collision.TryGetComponent<ExpGem>(out var gem))
            {
                _parentMagnet.OnGemDetected(gem);
            }
        }
    }
}
