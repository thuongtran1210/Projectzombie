using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace ProjectZombie.Features.UI.DamageText
{
    /// <summary>
    /// Điều phối chữ combat theo scene; là nơi duy nhất sở hữu pool, lắng nghe sự kiện sát thương
    /// và điều phối vòng lặp cập nhật tập trung (Centralized 0-GC Update).
    /// </summary>
    public class DamageTextManager : MonoBehaviour, ICombatTextService
    {
        public static DamageTextManager Instance { get; private set; }

        [Header("1. Configuration & References")]
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

        /// <summary>
        /// Struct phẳng quản lý trạng thái động của một nhãn số sát thương đang hiển thị (0 GC).
        /// </summary>
        private struct ActiveTextEntry
        {
            public DamageTextItem Item;
            public Vector3 Origin;
            public Color InitialColor;
            public float Elapsed;
            public float Lifetime;
            public float FloatSpeed;
            public AnimationCurve ScaleCurve;
            public AnimationCurve AlphaCurve;
            public long Token;
            public bool FaceCamera;
        }

        private ObjectPool<DamageTextItem> _pool;
        private CombatTextLeases<DamageTextItem> _leases;
        private LegacyDamageTextAdapter _legacy;
        private Func<bool> _canAnimate;
        private bool _accepting = true;
        private bool _prewarmInProgress;

        private readonly List<ActiveTextEntry> _activeEntries = new List<ActiveTextEntry>(128);

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
            _legacy = new LegacyDamageTextAdapter(this);
        }

        private void Start()
        {
            if (_textPrefab != null && _styleConfig != null) EnsurePool();
        }

        private void OnEnable()
        {
            CombatDamageEvents.OnDamageDealt += HandleCombatDamageEvent;
            if (_listenToLegacyDamage) _legacy?.Bind();
        }

        private void OnDisable()
        {
            CombatDamageEvents.OnDamageDealt -= HandleCombatDamageEvent;
            _legacy?.Dispose();
            Clear();
        }

        public void Configure(Camera camera, bool faceCamera, Func<bool> canAnimate = null)
        {
            _camera = camera;
            _faceCamera = faceCamera;
            _canAnimate = canAnimate;
        }

        public void SetAccepting(bool accepting) => _accepting = accepting;

        public void SetLegacyInput(bool enabled)
        {
            _listenToLegacyDamage = enabled;
            _legacy?.Dispose();
            if (enabled && isActiveAndEnabled) _legacy?.Bind();
        }

        private void HandleCombatDamageEvent(CombatTextRequest request)
        {
            Show(request);
        }

        private void EnsurePool()
        {
            if (_pool != null) return;
            int retained = Mathf.Max(1, _maxPoolSize);
            int prewarm = Mathf.Clamp(_defaultPoolCapacity, 0, Mathf.Min(retained, Mathf.Max(1, _maxActiveCount)));

            _pool = new ObjectPool<DamageTextItem>(
                () =>
                {
                    var item = Instantiate(_textPrefab, transform);
                    item.StopPresentation();
                    return item;
                },
                null,
                item => item.StopPresentation(),
                item => Destroy(item.gameObject),
                true,
                Mathf.Max(1, prewarm),
                retained);

            _leases = new CombatTextLeases<DamageTextItem>(Mathf.Max(1, _maxActiveCount), _pool.Get, _pool.Release);

            if (prewarm > 0 && gameObject.activeInHierarchy && !_prewarmInProgress)
            {
                StartCoroutine(PrewarmNonBlockingRoutine(prewarm));
            }
        }

        /// <summary>
        /// Phân bổ việc nạp trước Object Pool qua từng frame (Non-blocking Prewarm)
        /// nhằm triệt tiêu hiện tượng sụt giảm FPS / Frame Freeze lúc tải Scene.
        /// </summary>
        private IEnumerator PrewarmNonBlockingRoutine(int totalCount)
        {
            _prewarmInProgress = true;
            const int batchPerFrame = 8;
            var tempBatch = new List<DamageTextItem>(batchPerFrame);
            int allocated = 0;

            while (allocated < totalCount && _pool != null)
            {
                int currentBatchSize = Mathf.Min(batchPerFrame, totalCount - allocated);
                for (int i = 0; i < currentBatchSize; i++)
                {
                    tempBatch.Add(_pool.Get());
                }

                for (int i = 0; i < tempBatch.Count; i++)
                {
                    _pool.Release(tempBatch[i]);
                }

                tempBatch.Clear();
                allocated += currentBatchSize;
                yield return null;
            }

            _prewarmInProgress = false;
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
                var right = _faceCamera ? camera.transform.right : Vector3.right;
                var up = _faceCamera ? camera.transform.up : Vector3.up;
                Vector3 spawnPos = request.Position
                    + right * UnityEngine.Random.Range(-_styleConfig.RandomArcSpread, _styleConfig.RandomArcSpread)
                    + up * UnityEngine.Random.Range(0f, _styleConfig.RandomArcSpread * 0.5f);
                Quaternion spawnRot = _faceCamera ? camera.transform.rotation : Quaternion.identity;

                item.InitializePresentation(
                    Format(request),
                    color,
                    size,
                    spawnPos,
                    spawnRot,
                    _sortingLayer,
                    _sortingOrder);

                _activeEntries.Add(new ActiveTextEntry
                {
                    Item = item,
                    Origin = spawnPos,
                    InitialColor = color,
                    Elapsed = 0f,
                    Lifetime = Mathf.Max(0.01f, _styleConfig.Lifetime),
                    FloatSpeed = _styleConfig.FloatSpeed,
                    ScaleCurve = _styleConfig.ScaleCurve,
                    AlphaCurve = _styleConfig.AlphaCurve,
                    Token = token,
                    FaceCamera = _faceCamera
                });
            }
            catch
            {
                _leases.Complete(item, token);
                throw;
            }

            return true;
        }

        /// <summary>
        /// Vòng lặp cập nhật tập trung toàn bộ nhãn chữ sát thương đang hoạt động (Centralized 0-GC Update Loop).
        /// </summary>
        private void Update()
        {
            if (_canAnimate != null && !_canAnimate()) return;
            if (_activeEntries.Count == 0) return;

            float dt = Time.deltaTime;
            var camera = _camera;
            if (camera == null && _listenToLegacyDamage) camera = Camera.main;

            Vector3 cameraUp = camera != null ? camera.transform.up : Vector3.up;
            Quaternion cameraRot = camera != null ? camera.transform.rotation : Quaternion.identity;

            for (int i = _activeEntries.Count - 1; i >= 0; i--)
            {
                var entry = _activeEntries[i];
                entry.Elapsed += dt;

                if (entry.Elapsed >= entry.Lifetime)
                {
                    _activeEntries.RemoveAt(i);
                    _leases.Complete(entry.Item, entry.Token);
                    continue;
                }

                _activeEntries[i] = entry;

                float t = Mathf.Clamp01(entry.Elapsed / entry.Lifetime);
                Vector3 upDir = entry.FaceCamera ? cameraUp : Vector3.up;
                Vector3 currentPos = entry.Origin + upDir * (entry.FloatSpeed * entry.Elapsed);
                Quaternion currentRot = entry.FaceCamera ? cameraRot : Quaternion.identity;
                float currentScale = entry.ScaleCurve != null ? entry.ScaleCurve.Evaluate(t) : 1f;

                Color c = entry.InitialColor;
                c.a *= entry.AlphaCurve != null ? entry.AlphaCurve.Evaluate(t) : (1f - t);

                entry.Item.UpdatePresentation(currentPos, currentRot, currentScale, c);
            }
        }

        public static string Format(in CombatTextRequest request)
        {
            if (request.Kind == CombatTextKind.Custom) return request.Text;
            if (request.Kind == CombatTextKind.Block) return "BLOCK";
            if (request.Kind == CombatTextKind.Immune) return "IMMUNE";

            float rounded = Mathf.Round(request.Amount);
            string text = rounded >= 0f && rounded < Numbers.Length
                ? Numbers[(int)rounded]
                : rounded.ToString("0", System.Globalization.CultureInfo.InvariantCulture);

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

        public void Clear()
        {
            _activeEntries.Clear();
            _leases?.Clear();
        }

        private void OnDestroy()
        {
            _legacy?.Dispose();
            _leases?.Dispose();
            _pool?.Dispose();
            if (Instance == this) Instance = null;
        }
    }
}
