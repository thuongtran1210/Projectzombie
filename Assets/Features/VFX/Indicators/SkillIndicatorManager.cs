using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.VFX.Indicators
{
    /// <summary>
    /// Manager quản lý tập trung và Object Pool của tất cả Indicator vệt chỉ dấu trong Game.
    /// Tự động khởi tạo Singleton và Fallback Prefabs nếu trong Scene chưa kéo thả thủ công.
    /// Kỹ năng của Boss/Enemy chỉ cần gọi:
    /// SkillIndicatorManager.Instance.ShowIndicator(request, onCompleteCallback)
    /// </summary>
    public class SkillIndicatorManager : MonoBehaviour
    {
        private static SkillIndicatorManager _instance;

        public static SkillIndicatorManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SkillIndicatorManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SkillIndicatorManager");
                        _instance = go.AddComponent<SkillIndicatorManager>();
                        if (go.transform.parent != null) go.transform.SetParent(null);
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Prefabs Indicator")]
        [SerializeField] private SkillIndicator _boxIndicatorPrefab;
        [SerializeField] private SkillIndicator _circleIndicatorPrefab;

        private readonly Dictionary<IndicatorShape, Queue<SkillIndicator>> _pools = new Dictionary<IndicatorShape, Queue<SkillIndicator>>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                if (transform.parent != null) transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            EnsurePoolsInitialized();
        }

        private void EnsurePoolsInitialized()
        {
            if (!_pools.ContainsKey(IndicatorShape.Box))
            {
                InitializePool(IndicatorShape.Box, _boxIndicatorPrefab, 5);
            }
            if (!_pools.ContainsKey(IndicatorShape.Circle))
            {
                InitializePool(IndicatorShape.Circle, _circleIndicatorPrefab, 5);
            }
        }

        private void InitializePool(IndicatorShape shape, SkillIndicator prefab, int initialCount)
        {
            var queue = new Queue<SkillIndicator>();
            for (int i = 0; i < initialCount; i++)
            {
                SkillIndicator instance = CreateIndicatorInstance(shape, prefab);
                instance.gameObject.SetActive(false);
                queue.Enqueue(instance);
            }
            _pools[shape] = queue;
        }

        private SkillIndicator CreateIndicatorInstance(IndicatorShape shape, SkillIndicator prefab)
        {
            if (prefab != null)
            {
                return Instantiate(prefab, transform);
            }

            // Tạo Fallback Indicator tự động nếu chưa có Prefab
            GameObject go = new GameObject($"Indicator_{shape}_Fallback");
            go.transform.SetParent(transform);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.1f, 0.1f, 0.4f);
            sr.sortingOrder = 10; // Đảm bảo hiển thị nổi trên mặt đất

            // Tạo Texture/Sprite hình khối tự động
            Texture2D tex = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
            tex.SetPixels(colors);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));

            var indicator = go.AddComponent<SkillIndicator>();
            
            // Set private fields via reflection fallback
            var field = typeof(SkillIndicator).GetField("_spriteRenderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(indicator, sr);

            var shapeField = typeof(SkillIndicator).GetField("_shape", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (shapeField != null) shapeField.SetValue(indicator, shape);

            return indicator;
        }

        /// <summary>
        /// Yêu cầu hiển thị vệt chỉ dấu nguy hiểm.
        /// </summary>
        /// <param name="request">Thông số hình dạng, thời gian delay, kích thước, vị trí</param>
        /// <param name="onComplete">Callback thực hiện sau khi vệt chỉ dấu kết thúc delay</param>
        public void ShowIndicator(IndicatorRequest request, Action onComplete = null)
        {
            EnsurePoolsInitialized();

            if (!_pools.TryGetValue(request.Shape, out var queue) || queue == null)
            {
                queue = new Queue<SkillIndicator>();
                _pools[request.Shape] = queue;
            }

            SkillIndicator indicator = null;
            if (queue.Count > 0)
            {
                indicator = queue.Dequeue();
            }
            else
            {
                SkillIndicator prefab = request.Shape == IndicatorShape.Box ? _boxIndicatorPrefab : _circleIndicatorPrefab;
                indicator = CreateIndicatorInstance(request.Shape, prefab);
            }

            if (indicator != null)
            {
                indicator.PlayTelegraph(request, () =>
                {
                    queue.Enqueue(indicator);
                    onComplete?.Invoke();
                });
            }
            else
            {
                onComplete?.Invoke();
            }
        }
    }
}
