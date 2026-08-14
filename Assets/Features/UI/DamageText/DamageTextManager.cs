using System;
using UnityEngine;
using UnityEngine.Pool;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI.DamageText
{
    /// <summary>
    /// Manager quản lý Object Pool & Off-screen Culling cho hệ thống Floating Damage Text.
    /// Tối ưu hiệu năng 0-GC nhờ String Lookup Table.
    /// </summary>
    public class DamageTextManager : MonoBehaviour
    {
        public static DamageTextManager Instance { get; private set; }

        [Header("Configurations")]
        [SerializeField] private DamageTextItem _textPrefab;
        [SerializeField] private DamageTextStyleConfig _styleConfig;
        [SerializeField] private int _defaultPoolCapacity = 50;
        [SerializeField] private int _maxPoolSize = 200;

        private ObjectPool<DamageTextItem> _pool;
        private Camera _mainCamera;

        // GC Zero Optimization: Lookup Table Cache cho các chuỗi số từ 0 - 9999
        private static readonly string[] NUMBER_CACHE = new string[10000];

        static DamageTextManager()
        {
            for (int i = 0; i < NUMBER_CACHE.Length; i++)
            {
                NUMBER_CACHE[i] = i.ToString();
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _mainCamera = Camera.main;
            InitPool();
        }

        private void OnEnable()
        {
            HealthSystem.OnDamageReported += HandleDamageReported;
        }

        private void OnDisable()
        {
            HealthSystem.OnDamageReported -= HandleDamageReported;
        }

        private void InitPool()
        {
            _pool = new ObjectPool<DamageTextItem>(
                createFunc: () =>
                {
                    DamageTextItem item = Instantiate(_textPrefab, transform);
                    item.gameObject.SetActive(false);
                    return item;
                },
                actionOnGet: (item) => { },
                actionOnRelease: (item) => { item.gameObject.SetActive(false); },
                actionOnDestroy: (item) => { Destroy(item.gameObject); },
                collectionCheck: false,
                defaultCapacity: _defaultPoolCapacity,
                maxSize: _maxPoolSize
            );
        }

        private void HandleDamageReported(DamageReport report)
        {
            if (_styleConfig == null || _textPrefab == null) return;

            // 1. Off-Screen Culling: Kiểm tra xem vị trí trúng đòn có nằm trong Camera hay không
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera != null)
            {
                Vector3 viewportPos = _mainCamera.WorldToViewportPoint(report.Position);
                bool isVisible = viewportPos.x >= 0f && viewportPos.x <= 1f &&
                                 viewportPos.y >= 0f && viewportPos.y <= 1f &&
                                 viewportPos.z > 0f;
                if (!isVisible) return; // Bỏ qua nếu nằm ngoài màn hình
            }

            // 2. Format chuỗi sát thương không sinh GC (0-GC Lookup Table)
            string formattedText = GetFastFormattedNumber(report.Amount);
            if (report.IsCounter)
            {
                formattedText = $"* {formattedText}";
            }
            if (report.IsCritical)
            {
                formattedText = $"<b>{formattedText}!</b>";
            }

            // 3. Lấy Color & Font Size từ Config
            Color textColor = _styleConfig.GetColor(report.IsPlayerTarget, report.IsCritical, report.Element, report.IsCounter);
            float fontSize = _styleConfig.GetFontSize(report.IsPlayerTarget, report.IsCritical, report.IsCounter);

            // 4. Lấy Item từ Pool và khởi tạo
            DamageTextItem item = _pool.Get();
            item.Setup(formattedText, textColor, fontSize, report.Position, _styleConfig, (releasedItem) =>
            {
                _pool.Release(releasedItem);
            });
        }

        /// <summary>
        /// Chuyển đổi float sát thương thành string nhanh chóng dùng Lookup Cache (Triệt tiêu GC Spikes).
        /// </summary>
        private string GetFastFormattedNumber(float amount)
        {
            int rounded = Mathf.RoundToInt(amount);
            if (rounded >= 0 && rounded < NUMBER_CACHE.Length)
            {
                return NUMBER_CACHE[rounded];
            }
            return rounded.ToString();
        }
    }
}
