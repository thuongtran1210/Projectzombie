using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Collectibles;
using ProjectZombie.Features.Player.Mechanics;

namespace ProjectZombie.Features.Player.Mechanics
{
    /// <summary>
    /// Component theo dõi và quản lý Cơ chế Độc Quyền cho nhân vật Thanh Đồng (Cô Đồng Tứ Phủ):
    /// "Thân Xác Thỉnh Thánh" (Possession Gauge) & "Phán Truyền - Ban Lộc".
    /// Hiện thực ICharacterGaugeProvider để tự động hiển thị thanh Linh Căn trên CharacterGaugeWidgetPresenter độc lập.
    /// </summary>
    public class ThanhDongPossessionTracker : MonoBehaviour, ICharacterGaugeProvider
    {
        [Header("Possession Gauge Settings")]
        [Tooltip("Giá trị Linh Căn tối đa để thỉnh Thánh giáng ngự")]
        [SerializeField] private float _maxLinhCan = 100f;

        [Tooltip("Lượng Linh Căn nạp mỗi khi nhặt được ExpGem/Lộc")]
        [SerializeField] private float _linhCanPerGem = 3.5f;

        [Tooltip("Lượng Linh Căn tự nạp mỗi giây khi chiến đấu")]
        [SerializeField] private float _passiveChargeRate = 1.0f;

        [Header("Thánh Giáng Ngự (Possession Mode)")]
        [Tooltip("Thời lượng Thánh giáng ngự (giây)")]
        [SerializeField] private float _possessionDuration = 8.0f;

        [Tooltip("Bán kính chấn động Phán Truyền gây choáng quái khi Thánh giáng")]
        [SerializeField] private float _oracleStunRadius = 12.0f;

        [Tooltip("Thời gian làm choáng quái khi Phán Truyền (giây)")]
        [SerializeField] private float _oracleStunDuration = 2.0f;

        [Tooltip("Tăng tốc độ di chuyển trong lúc Thánh giáng")]
        [SerializeField] private float _speedMultiplier = 1.25f;

        [Header("Visual & Effects")]
        [Tooltip("Prefab hào quang Thánh Giáng Ngự (nếu có)")]
        [SerializeField] private GameObject _possessionAuraPrefab;

        [Tooltip("Prefab sóng xung kích Phán Truyền khi Thánh bộc phát")]
        [SerializeField] private GameObject _oracleShockwavePrefab;

        private float _currentLinhCan = 0f;
        private bool _isPossessed = false;
        private float _possessionTimer = 0f;
        private GameObject _activeAuraObj;

        private PlayerExperience _playerExperience;
        private PlayerController _playerController;

        // ====================================================================
        // ICharacterGaugeProvider Implementation
        // ====================================================================

        public string GaugeTitle => _isPossessed 
            ? "<color=#FFD700>👑 THÁNH GIÁNG NGỰ</color>" 
            : $"<color=#E06666>Linh Căn: {Mathf.FloorToInt(_currentLinhCan)}%</color>";

        public float CurrentValue => _isPossessed 
            ? (_possessionTimer / _possessionDuration) * 100f 
            : _currentLinhCan;

        public float MinValue => 0f;
        public float MaxValue => 100f;

        public Color GaugeColor => _isPossessed 
            ? new Color(1f, 0.84f, 0f, 1f) // Vàng Hoàng Kim
            : new Color(0.88f, 0.4f, 0.4f, 1f); // Đỏ Chu Sa Tứ Phủ

        public event System.Action<float, string> OnGaugeValueChanged;

        public bool IsPossessed => _isPossessed;

        private void Awake()
        {
            _playerExperience = GetComponent<PlayerExperience>();
            _playerController = GetComponent<PlayerController>();
        }

        private float _lastExp = 0f;

        private void OnEnable()
        {
            if (_playerExperience != null)
            {
                _lastExp = _playerExperience.CurrentExp;
                _playerExperience.OnExpChanged += HandleExpChanged;
            }
        }

        private void OnDisable()
        {
            if (_playerExperience != null)
            {
                _playerExperience.OnExpChanged -= HandleExpChanged;
            }

            if (_activeAuraObj != null)
            {
                Destroy(_activeAuraObj);
            }
        }

        private void HandleExpChanged(float currentExp, float maxExp)
        {
            if (!_isPossessed)
            {
                // Khi tăng exp (nhặt ExpGem)
                if (currentExp > _lastExp)
                {
                    float gained = currentExp - _lastExp;
                    AddLinhCan(_linhCanPerGem * (gained / 10f));
                }
                else if (currentExp < _lastExp)
                {
                    // Trường hợp Level Up reset CurrentExp
                    AddLinhCan(_linhCanPerGem);
                }
            }
            _lastExp = currentExp;
        }

        private void Update()
        {
            if (_isPossessed)
            {
                _possessionTimer -= Time.deltaTime;
                OnGaugeValueChanged?.Invoke(CurrentValue, GaugeTitle);

                // Hút liên tục hạt Exp/Lộc về phía người chơi suốt thời gian giáng ngự (Ban Lộc Toàn Cõi)
                if (ExpGemPoolManager.Instance != null)
                {
                    ExpGemPoolManager.Instance.CollectAllActiveGems(transform);
                }

                if (_possessionTimer <= 0f)
                {
                    EndPossession();
                }
            }
            else
            {
                // Nạp thụ động chậm rãi khi chiến đấu
                if (_currentLinhCan < _maxLinhCan)
                {
                    AddLinhCan(_passiveChargeRate * Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// Nạp thêm điểm Linh Căn.
        /// </summary>
        public void AddLinhCan(float amount)
        {
            if (_isPossessed) return;

            _currentLinhCan = Mathf.Clamp(_currentLinhCan + amount, 0f, _maxLinhCan);
            OnGaugeValueChanged?.Invoke(_currentLinhCan, GaugeTitle);

            if (_currentLinhCan >= _maxLinhCan)
            {
                TriggerPossession();
            }
        }

        /// <summary>
        /// Kích hoạt trạng thái "Thánh Giáng Ngự" (có thể gọi tự động khi đầy thanh hoặc qua Kỹ năng chủ động).
        /// </summary>
        public void TriggerPossession()
        {
            _isPossessed = true;
            _possessionTimer = _possessionDuration;
            _currentLinhCan = 0f;

            // 1. PHÁN TRUYỀN: Gây choáng toàn bộ kẻ địch trong diện rộng
            ExecuteOracleStun();

            // 2. BAN LỘC: Thu hút toàn bộ ExpGem trên toàn màn hình
            if (ExpGemPoolManager.Instance != null)
            {
                ExpGemPoolManager.Instance.CollectAllActiveGems(transform);
            }

            // 3. Spawn Hào Quang Thánh Nhập
            SpawnPossessionAura();

            OnGaugeValueChanged?.Invoke(100f, GaugeTitle);
        }

        private void EndPossession()
        {
            _isPossessed = false;
            _possessionTimer = 0f;
            _currentLinhCan = 0f;

            if (_activeAuraObj != null)
            {
                Destroy(_activeAuraObj);
            }

            OnGaugeValueChanged?.Invoke(0f, GaugeTitle);
        }

        private void ExecuteOracleStun()
        {
            // Spawn hiệu ứng sóng xung kích Phán Truyền bộc phát
            if (_oracleShockwavePrefab != null)
            {
                Instantiate(_oracleShockwavePrefab, transform.position, Quaternion.identity);
            }

            // Quét và làm choáng quái vật xung quanh
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _oracleStunRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                var col = hits[i];
                if (col.CompareTag("Enemy") || col.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    // Đẩy lùi nhẹ quái
                    if (col.attachedRigidbody != null)
                    {
                        Vector2 pushDir = (col.transform.position - transform.position).normalized;
                        col.attachedRigidbody.AddForce(pushDir * 5f, ForceMode2D.Impulse);
                    }
                }
            }
        }

        private void SpawnPossessionAura()
        {
            if (_activeAuraObj != null) Destroy(_activeAuraObj);

            if (_possessionAuraPrefab != null)
            {
                _activeAuraObj = Instantiate(_possessionAuraPrefab, transform.position, Quaternion.identity, transform);
            }
            else
            {
                // Fallback Dynamic Aura
                _activeAuraObj = new GameObject("ThanhDong_PossessionAura");
                _activeAuraObj.transform.SetParent(transform);
                _activeAuraObj.transform.localPosition = Vector3.zero;

                var line = _activeAuraObj.AddComponent<LineRenderer>();
                line.material = new Material(Shader.Find("Sprites/Default"));
                Color goldColor = new Color(1f, 0.84f, 0.1f, 0.85f);
                line.startColor = goldColor;
                line.endColor = goldColor;
                line.startWidth = 0.12f;
                line.endWidth = 0.12f;
                line.useWorldSpace = false;

                int steps = 32;
                line.positionCount = steps + 1;
                float radius = 2.2f;
                for (int i = 0; i <= steps; i++)
                {
                    float angle = i * (Mathf.PI * 2.0f / steps);
                    line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0));
                }
            }
        }
    }
}
