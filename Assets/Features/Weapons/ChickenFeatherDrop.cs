using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Collectibles;

namespace ProjectZombie.Features.Weapons
{
    /// <summary>
    /// Vật phẩm Lông Gà Hoàng Kim rớt ra khi quái vật bị diệt.
    /// Tích đủ 5 Lông Gà sẽ tự động triệu hồi 1 Linh Thú Gà Con Hộ Vệ.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class ChickenFeatherDrop : MonoBehaviour, ICollectible
    {
        [Header("Settings")]
        [SerializeField] private float attractSpeed = 8.5f;
        [SerializeField] private float lifetime = 35f;

        private Relic_ChickenFeatherBroom _broomSource;
        private Transform _targetPlayer;
        private bool _isCollected;
        private float _spawnTime;
        private Vector3 _startPos;
        private Vector3 _targetPopPos;
        private bool _isPopping = true;
        private SpriteRenderer _sr;

        public bool IsActiveOnGround => !_isCollected;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            var col = GetComponent<CircleCollider2D>();
            if (col != null)
            {
                col.isTrigger = true;
                col.radius = 0.35f;
            }
        }

        public void Init(Relic_ChickenFeatherBroom source)
        {
            _broomSource = source;
            _isCollected = false;
            _spawnTime = Time.time;
            _startPos = transform.position;

            Vector2 randOffset = Random.insideUnitCircle.normalized * Random.Range(0.35f, 0.75f);
            _targetPopPos = _startPos + (Vector3)randOffset;
            _isPopping = true;
            transform.localScale = Vector3.zero;

            if (_sr != null)
            {
                _sr.sortingLayerName = "Collectibles";
                _sr.sortingOrder = 10;
            }
        }

        private void Update()
        {
            if (_isCollected) return;

            // Tự biến mất khi quá hạn
            if (Time.time >= _spawnTime + lifetime)
            {
                Destroy(gameObject);
                return;
            }

            // 1. Pop animation khi mới rơi ra
            if (_isPopping)
            {
                float t = (Time.time - _spawnTime) / 0.22f;
                if (t >= 1f)
                {
                    _isPopping = false;
                    transform.position = _targetPopPos;
                    transform.localScale = Vector3.one;
                }
                else
                {
                    float curve = Mathf.Sin(t * Mathf.PI);
                    transform.position = Vector3.Lerp(_startPos, _targetPopPos, Mathf.SmoothStep(0f, 1f, t)) + new Vector3(0f, curve * 0.25f, 0f);
                    transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                    return;
                }
            }

            // 2. Hiệu ứng bồng bềnh lơ lửng & xoay nhẹ
            float bobbing = Mathf.Sin((Time.time + _spawnTime) * 4f) * 0.05f;
            float tilt = Mathf.Sin((Time.time + _spawnTime) * 2.5f) * 15f;
            transform.rotation = Quaternion.Euler(0f, 0f, tilt);

            // 3. Hút về phía Hero khi được kích hoạt
            if (_targetPlayer != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, _targetPlayer.position, attractSpeed * Time.deltaTime);
                attractSpeed += 18f * Time.deltaTime;

                if (Vector2.Distance(transform.position, _targetPlayer.position) <= 0.45f)
                {
                    Collect();
                }
            }
            else
            {
                transform.position = _targetPopPos + new Vector3(0f, bobbing, 0f);
            }
        }

        public void StartMagnetEffect(Transform target)
        {
            if (_isCollected) return;
            _targetPlayer = target;
        }

        public void Collect()
        {
            if (_isCollected) return;
            _isCollected = true;

            if (_broomSource != null)
            {
                _broomSource.AddCollectedFeather();
            }

            // Âm thanh nhặt & tia sáng
            global::Core.Audio.AudioManager.Instance?.PlayCoinTick();
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isCollected) return;
            if (other.CompareTag("Player"))
            {
                Collect();
            }
        }
    }

    /// <summary>
    /// Đánh dấu quái vật bị dính đòn Chổi Lông Gà để rơi Lông Gà khi bị hạ gục.
    /// Tự động hiển thị Marker Lông Gà Vàng Kim phát sáng lơ lửng trên đầu quái.
    /// </summary>
    public class FeatherDropMarker : MonoBehaviour
    {
        private Relic_ChickenFeatherBroom _broom;
        private HealthSystem _hp;
        private bool _dropped;
        private GameObject _markerVisualObj;
        private float _headHeight = 0.85f;

        public void Setup(Relic_ChickenFeatherBroom broom, HealthSystem hp)
        {
            _broom = broom;
            _hp = hp;
            if (_hp != null)
            {
                _hp.OnDied += HandleDeath;
            }

            CreateOverheadMarker();
        }

        private void CreateOverheadMarker()
        {
            if (_markerVisualObj != null) return;

            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                _headHeight = Mathf.Max(0.75f, col.bounds.extents.y * 1.85f + 0.25f);
            }

            _markerVisualObj = new GameObject("FeatherMark_Overhead");
            _markerVisualObj.transform.SetParent(transform, false);
            _markerVisualObj.transform.localPosition = new Vector3(0f, _headHeight, 0f);

            var sr = _markerVisualObj.AddComponent<SpriteRenderer>();
            if (_broom != null && _broom.FeatherCollectibleSprite != null)
            {
                sr.sprite = _broom.FeatherCollectibleSprite;
            }
#if UNITY_EDITOR
            else
            {
                sr.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/VFX/SkillLibrary/Textures/Tex_ChickenBroom_SingleFeather_Clean.png");
            }
#endif
            sr.sortingLayerName = "Skill";
            sr.sortingOrder = 12;
            sr.color = new Color(1f, 0.95f, 0.35f, 1f); // Màu Vàng Kim phát sáng
            _markerVisualObj.transform.localScale = Vector3.one * 0.28f;
        }

        private void Update()
        {
            if (_markerVisualObj == null) return;

            // Hiệu ứng bồng bềnh & xoay nhấp nháy trên đầu quái
            float bobbing = Mathf.Sin(Time.time * 5f) * 0.08f;
            float tilt = Mathf.Sin(Time.time * 3f) * 12f;
            float pulseScale = 0.28f + 0.035f * Mathf.Sin(Time.time * 6f);

            _markerVisualObj.transform.localPosition = new Vector3(0f, _headHeight + bobbing, 0f);
            _markerVisualObj.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
            _markerVisualObj.transform.localScale = Vector3.one * pulseScale;
        }

        private void HandleDeath()
        {
            if (_dropped) return;
            _dropped = true;

            if (_markerVisualObj != null)
            {
                Destroy(_markerVisualObj);
            }

            if (_broom != null)
            {
                _broom.SpawnFeatherDrop(transform.position);
            }
        }

        private void OnDestroy()
        {
            if (_hp != null)
            {
                _hp.OnDied -= HandleDeath;
            }
            if (!_dropped && _hp != null && _hp.CurrentHealth <= 0)
            {
                HandleDeath();
            }
            if (_markerVisualObj != null)
            {
                Destroy(_markerVisualObj);
            }
        }
    }
}
