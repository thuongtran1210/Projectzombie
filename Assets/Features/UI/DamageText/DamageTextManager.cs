using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ProjectZombie.Features.UI.DamageText
{
    /// <summary>Điều phối chữ combat theo scene; là nơi duy nhất sở hữu pool và quản lý vòng đời item đang hiển thị.</summary>
    public class DamageTextManager : MonoBehaviour, ICombatTextService
    {
        // Giữ tương thích với scene và nơi gọi cũ. Hệ thống mới nhận ICombatTextService qua bước khởi tạo.
        public static DamageTextManager Instance { get; private set; }
        [SerializeField] private DamageTextItem _textPrefab;
        [SerializeField] private DamageTextStyleConfig _styleConfig;
        [SerializeField] private int _defaultPoolCapacity = 50;
        [SerializeField] private int _maxPoolSize = 200;
        [SerializeField, Min(1)] private int _maxActiveCount = 100;
        [SerializeField] private Camera _camera;
        [SerializeField] private bool _listenToLegacyDamage = true;
        [SerializeField] private bool _faceCamera;
        [SerializeField] private string _sortingLayer = "UI_World";
        [SerializeField] private int _sortingOrder = 1000;
        private ObjectPool<DamageTextItem> _pool;
        private CombatTextLeases<DamageTextItem> _leases;
        private LegacyDamageTextAdapter _legacy;
        private Action<DamageTextItem, long> _completion;
        private Func<bool> _canAnimate;
        private bool _accepting = true;
        public int ActiveCount => _leases == null ? 0 : _leases.Count;
        private static readonly string[] Numbers = BuildNumbers();
        private static string[] BuildNumbers()
        {
            var result = new string[10000];
            for (int i = 0; i < result.Length; i++) result[i] = i.ToString();
            return result;
        }
        private void Awake()
        {
            if (Instance == null) Instance = this;
            _completion = Complete;
            _legacy = new LegacyDamageTextAdapter(this);
        }
        private void Start()
        {
            if (_textPrefab != null && _styleConfig != null) EnsurePool();
        }
        private void OnEnable() { if (_listenToLegacyDamage) _legacy?.Bind(); }
        private void OnDisable() { _legacy?.Dispose(); Clear(); }
        public void Configure(Camera camera, bool faceCamera, Func<bool> canAnimate = null)
        {
            _camera = camera; _faceCamera = faceCamera; _canAnimate = canAnimate;
        }
        public void SetAccepting(bool accepting) { _accepting = accepting; }
        public void SetLegacyInput(bool enabled)
        {
            _listenToLegacyDamage = enabled;
            _legacy?.Dispose();
            if (enabled && isActiveAndEnabled) _legacy?.Bind();
        }
        private void EnsurePool()
        {
            if (_pool != null) return;
            int retained = Mathf.Max(1, _maxPoolSize);
            int prewarm = Mathf.Clamp(_defaultPoolCapacity, 0, Mathf.Min(retained, Mathf.Max(1, _maxActiveCount)));
            _pool = new ObjectPool<DamageTextItem>(
                () => { var item = Instantiate(_textPrefab, transform); item.StopPresentation(); return item; },
                null, item => item.StopPresentation(), item => Destroy(item.gameObject),
                true, Mathf.Max(1, prewarm), retained);
            _leases = new CombatTextLeases<DamageTextItem>(Mathf.Max(1, _maxActiveCount), _pool.Get, _pool.Release);
            var warm = new List<DamageTextItem>(prewarm);
            for (int i = 0; i < prewarm; i++) warm.Add(_pool.Get());
            foreach (var item in warm) _pool.Release(item);
        }
        public bool Show(in CombatTextRequest request)
        {
            if (!_accepting || !isActiveAndEnabled || !request.IsValid || _textPrefab == null || _styleConfig == null ||
                (_canAnimate != null && !_canAnimate())) return false;
            var camera = _camera;
            if (camera == null && _listenToLegacyDamage) camera = Camera.main;
            if (camera == null) return false;
            var point = camera.WorldToViewportPoint(request.Position);
            if (point.z <= 0f || point.x < 0f || point.x > 1f || point.y < 0f || point.y > 1f) return false;
            EnsurePool();
            bool player = request.Target == CombatTextTarget.Player;
            Color color = request.ColorOverride ?? _styleConfig.ResolveColor(request);
            float size = request.FontSizeOverride > 0f ? request.FontSizeOverride :
                _styleConfig.GetFontSize(player, request.IsCritical, request.IsCounter);
            if (!_leases.TryAcquire(request.Priority, out var item, out long token)) return false;
            try
            {
                item.Setup(Format(request), color, size, request.Position, _styleConfig, token, _completion,
                    camera, _faceCamera, _sortingLayer, _sortingOrder, _canAnimate);
            }
            catch { _leases.Complete(item, token); throw; }
            return true;
        }
        public static string Format(in CombatTextRequest request)
        {
            if (request.Kind == CombatTextKind.Custom) return request.Text;
            if (request.Kind == CombatTextKind.Block) return "BLOCK";
            if (request.Kind == CombatTextKind.Immune) return "IMMUNE";
            float rounded = Mathf.Round(request.Amount);
            string text = rounded >= 0f && rounded < Numbers.Length ? Numbers[(int)rounded] : rounded.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
            if (request.Kind == CombatTextKind.Heal || request.Kind == CombatTextKind.Resource) text = "+" + text;
            if (request.IsCounter) text = "* " + text;
            if (request.IsCritical) text = "<b>" + text + "!</b>";
            return text;
        }
        public void ShowCustomText(string text, Vector3 position, Color color, float customFontSize = 0f)
        {
            Show(new CombatTextRequest(0f, position, kind: CombatTextKind.Custom, text: text,
                color: color, fontSize: customFontSize > 0f ? customFontSize : _styleConfig != null ? _styleConfig.NormalFontSize * 1.15f : 0f));
        }
        private void Complete(DamageTextItem item, long token) { _leases?.Complete(item, token); }
        public void Clear() { _leases?.Clear(); }
        private void OnDestroy()
        {
            _legacy?.Dispose();
            _leases?.Dispose();
            _pool?.Dispose();
            if (Instance == this) Instance = null;
        }
    }
}
