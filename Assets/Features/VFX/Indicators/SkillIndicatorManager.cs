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

            // Tạo Fallback Indicator tự động chuẩn 1x1m nếu chưa có Prefab kéo thả trong Inspector
            GameObject go = new GameObject($"Indicator_{shape}_Fallback");
            go.transform.SetParent(transform);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.1f, 0.1f, 0.4f);
            sr.sortingOrder = 10; // Đảm bảo hiển thị nổi trên mặt đất

            if (shape == IndicatorShape.Box)
            {
                Texture2D tex = new Texture2D(32, 32);
                Color[] colors = new Color[32 * 32];
                for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
                tex.SetPixels(colors);
                tex.Apply();
                // PPU = 32 cho Texture 32x32 -> Kích thước World Space là đúng 1x1m
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
            }
            else
            {
                Texture2D tex = new Texture2D(64, 64);
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
                        if (dist <= 31.5f)
                            tex.SetPixel(x, y, Color.white);
                        else
                            tex.SetPixel(x, y, Color.clear);
                    }
                }
                tex.Apply();
                // PPU = 64 cho Texture 64x64 -> Kích thước World Space là đúng 1x1m
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
            }

            var indicator = go.AddComponent<SkillIndicator>();
            indicator.Construct(sr, shape);
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
