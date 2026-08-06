using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.VFX.Indicators
{
    /// <summary>
    /// Manager quản lý tập trung và Object Pool của tất cả Indicator trong Game.
    /// Kỹ năng của Boss/Enemy chỉ cần gọi:
    /// SkillIndicatorManager.Instance.ShowIndicator(request, onCompleteCallback)
    /// </summary>
    public class SkillIndicatorManager : MonoBehaviour
    {
        public static SkillIndicatorManager Instance { get; private set; }

        [Header("Prefabs Indicator")]
        [SerializeField] private SkillIndicator _boxIndicatorPrefab;
        [SerializeField] private SkillIndicator _circleIndicatorPrefab;

        private readonly Dictionary<IndicatorShape, Queue<SkillIndicator>> _pools = new Dictionary<IndicatorShape, Queue<SkillIndicator>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializePool(IndicatorShape.Box, _boxIndicatorPrefab, 5);
            InitializePool(IndicatorShape.Circle, _circleIndicatorPrefab, 5);
        }

        private void InitializePool(IndicatorShape shape, SkillIndicator prefab, int initialCount)
        {
            if (prefab == null) return;

            var queue = new Queue<SkillIndicator>();
            for (int i = 0; i < initialCount; i++)
            {
                SkillIndicator instance = Instantiate(prefab, transform);
                instance.gameObject.SetActive(false);
                queue.Enqueue(instance);
            }
            _pools[shape] = queue;
        }

        /// <summary>
        /// Yêu cầu hiển thị vệt chỉ dấu nguy hiểm.
        /// </summary>
        /// <param name="request">Thông số hình dạng, thời gian delay, kích thước, vị trí</param>
        /// <param name="onComplete">Callback thực hiện sau khi vệt chỉ dấu kết thúc delay</param>
        public void ShowIndicator(IndicatorRequest request, Action onComplete = null)
        {
            if (!_pools.TryGetValue(request.Shape, out var queue) || queue == null)
            {
                Debug.LogWarning($"[{nameof(SkillIndicatorManager)}] Chưa khởi tạo Pool cho dạng {request.Shape}!");
                onComplete?.Invoke();
                return;
            }

            SkillIndicator indicator = null;
            if (queue.Count > 0)
            {
                indicator = queue.Dequeue();
            }
            else
            {
                // Tự mở rộng Pool nếu hết instance sẵn có
                SkillIndicator prefab = request.Shape == IndicatorShape.Box ? _boxIndicatorPrefab : _circleIndicatorPrefab;
                if (prefab != null)
                {
                    indicator = Instantiate(prefab, transform);
                }
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
