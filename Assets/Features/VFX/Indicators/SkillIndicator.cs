using System;
using System.Collections;
using UnityEngine;

namespace ProjectZombie.Features.VFX.Indicators
{
    /// <summary>
    /// Component quản lý hiển thị và Animation lấp đầy/đậm dần của 1 vệt chỉ dấu kỹ năng.
    /// Tự động hoàn trả về Pool qua Callback sau khi hết thời gian Duration.
    /// </summary>
    public class SkillIndicator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private IndicatorShape _shape;

        public IndicatorShape Shape => _shape;
        private Coroutine _animateRoutine;

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        public void Construct(SpriteRenderer spriteRenderer, IndicatorShape shape)
        {
            _spriteRenderer = spriteRenderer;
            _shape = shape;
        }

        public void PlayTelegraph(IndicatorRequest request, Action onComplete = null)
        {
            gameObject.SetActive(true);
            transform.position = request.Position;
            
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = request.Color;
                _spriteRenderer.sortingOrder = 10; // Đảm bảo nổi trên Tilemap/Mặt đất
            }

            // Xoay hướng nếu là dạng Box/Cone
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
            Color baseColor = _spriteRenderer != null ? _spriteRenderer.color : Color.red;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / Mathf.Max(0.01f, duration);

                if (_spriteRenderer != null)
                {
                    // Tăng dần Alpha từ 0.2 lên 0.85 để cảnh báo nguy hiểm dâng cao
                    baseColor.a = Mathf.Lerp(0.2f, 0.85f, progress);
                    _spriteRenderer.color = baseColor;
                }

                yield return null;
            }

            gameObject.SetActive(false);
            onComplete?.Invoke();
        }
    }
}

