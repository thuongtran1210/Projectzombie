using System;
using System.Collections;
using UnityEngine;

namespace ProjectZombie.Features.VFX.Indicators
{
    /// <summary>
    /// Component quản lý hiển thị và Animation lấp đầy/đậm dần của 1 vệt chỉ dấu kỹ năng của Boss.
    /// Tự động hiển thị tiến trình (Progress Fill Expansion) và hoàn trả về Pool qua Callback khi xong.
    /// </summary>
    public class SkillIndicator : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SpriteRenderer _borderRenderer;
        [SerializeField] private SpriteRenderer _fillRenderer;
        [SerializeField] private IndicatorShape _shape;

        public IndicatorShape Shape => _shape;
        private Coroutine _animateRoutine;

        private void Awake()
        {
            FetchComponents();
        }

        private void FetchComponents()
        {
            if (_borderRenderer == null)
            {
                _borderRenderer = GetComponent<SpriteRenderer>();
            }

            if (_fillRenderer == null)
            {
                var fillChild = transform.Find("FillVisual");
                if (fillChild != null)
                {
                    _fillRenderer = fillChild.GetComponent<SpriteRenderer>();
                }
            }
        }

        public void Construct(SpriteRenderer borderRenderer, IndicatorShape shape)
        {
            _borderRenderer = borderRenderer;
            _shape = shape;
        }

        public void Construct(SpriteRenderer borderRenderer, SpriteRenderer fillRenderer, IndicatorShape shape)
        {
            _borderRenderer = borderRenderer;
            _fillRenderer = fillRenderer;
            _shape = shape;
        }

        public void PlayTelegraph(IndicatorRequest request, Action onComplete = null)
        {
            FetchComponents();
            gameObject.SetActive(true);
            transform.position = request.Position;
            
            if (_borderRenderer != null)
            {
                _borderRenderer.sortingLayerName = "Shadows";
                _borderRenderer.sortingOrder = 7; // Nằm trên sàn gạch, dưới chân nhân vật
            }

            if (_fillRenderer != null)
            {
                _fillRenderer.sortingLayerName = "Shadows";
                _fillRenderer.sortingOrder = 8;
                _fillRenderer.color = new Color(1f, 0.2f, 0.2f, 0.65f);
            }

            // Xoay hướng nếu là dạng Box (húc thẳng / chém càn quét)
            if (request.Direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(request.Direction.y, request.Direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
            }
            else
            {
                transform.rotation = Quaternion.identity;
            }

            // Điều chỉnh Kích thước (Scale) chuẩn World Meters
            if (_shape == IndicatorShape.Box)
            {
                transform.localScale = new Vector3(request.Size.x, request.Size.y, 1f);
                // Dịch chuyển tâm về giữa vệt chỉ báo
                transform.position += request.Direction * (request.Size.y / 2f);
            }
            else if (_shape == IndicatorShape.Circle)
            {
                float diameter = request.Size.x * 2f;
                transform.localScale = new Vector3(diameter, diameter, 1f);
            }

            if (_animateRoutine != null) StopCoroutine(_animateRoutine);
            _animateRoutine = StartCoroutine(AnimateProgress(request.Duration, onComplete));
        }

        private IEnumerator AnimateProgress(float duration, Action onComplete)
        {
            float elapsed = 0f;
            duration = Mathf.Max(0.01f, duration);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);

                // 1. Fill Visual nở to dần từ 0 lên 1 theo tiến trình sạc chiêu
                if (_fillRenderer != null)
                {
                    _fillRenderer.transform.localScale = new Vector3(progress, progress, 1f);
                }

                // 2. Viền ngoài nhấp nháy đỏ rực cảnh báo
                if (_borderRenderer != null)
                {
                    float pulse = Mathf.PingPong(elapsed * 6f, 0.35f);
                    _borderRenderer.color = new Color(1f, 0.3f, 0.3f, 0.75f + pulse);
                }

                yield return null;
            }

            gameObject.SetActive(false);
            onComplete?.Invoke();
        }
    }
}
