using System.Collections;
using TMPro;
using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Boss
{
    /// <summary>
    /// Component gắn trên Boss cho phép luân phiên xoay vòng thuộc tính Ngũ Hành theo thời gian thực (ví dụ Ngưu Đầu Mã Diện & Diêm Vương).
    /// Tích hợp World-Space Overhead Badge hiển thị trực tiếp trên đầu Boss kèm ký hiệu 5 hệ.
    /// </summary>
    public class BossElementController : MonoBehaviour
    {
        [Header("Element Cycle Settings")]
        [SerializeField] private ElementType[] _elementCycle = new ElementType[] 
        { 
            ElementType.Kim, 
            ElementType.Moc, 
            ElementType.Thuy, 
            ElementType.Hoa, 
            ElementType.Tho 
        };

        [SerializeField] private float _switchIntervalSeconds = 10.0f;

        private int _currentIndex = 0;
        private float _timer = 0f;

        [Header("UI & Visual Settings")]
        [SerializeField] private SpriteRenderer _bossSpriteRenderer;
        [SerializeField] private TextMeshPro _overheadBadgeText;
        [SerializeField] private Vector3 _overheadOffset = new Vector3(0f, 2.0f, 0f);

        public ElementType CurrentElement => _elementCycle != null && _elementCycle.Length > 0 
            ? _elementCycle[_currentIndex] 
            : ElementType.None;

        public event System.Action<ElementType> OnBossElementChanged;

        private Coroutine _punchCoroutine;

        private void Start()
        {
            if (_bossSpriteRenderer == null)
            {
                _bossSpriteRenderer = GetComponent<SpriteRenderer>();
            }

            EnsureOverheadBadge();

            if (_elementCycle != null && _elementCycle.Length > 0)
            {
                ApplyElementVisuals(CurrentElement);
                OnBossElementChanged?.Invoke(CurrentElement);
            }
        }

        private void EnsureOverheadBadge()
        {
            if (_overheadBadgeText != null) return;

            // Tìm child có sẵn
            var existingChild = transform.Find("OverheadElementBadge");
            if (existingChild != null)
            {
                _overheadBadgeText = existingChild.GetComponent<TextMeshPro>();
            }

            // Nếu chưa có, tự động sinh GameObject child và gắn TextMeshPro 3D
            if (_overheadBadgeText == null)
            {
                GameObject badgeGo = new GameObject("OverheadElementBadge");
                badgeGo.transform.SetParent(transform, false);
                badgeGo.transform.localPosition = _overheadOffset;

                _overheadBadgeText = badgeGo.AddComponent<TextMeshPro>();
                _overheadBadgeText.alignment = TextAlignmentOptions.Center;
                _overheadBadgeText.fontSize = 4.5f;
                _overheadBadgeText.sortingLayerID = SortingLayer.NameToID("UI_World");
                _overheadBadgeText.sortingOrder = 1100;
            }
        }

        private void Update()
        {
            if (_elementCycle == null || _elementCycle.Length <= 1) return;

            _timer += Time.deltaTime;
            if (_timer >= _switchIntervalSeconds)
            {
                _timer = 0f;
                _currentIndex = (_currentIndex + 1) % _elementCycle.Length;
                
                ApplyElementVisuals(CurrentElement);
                OnBossElementChanged?.Invoke(CurrentElement);
            }
        }

        private void ApplyElementVisuals(ElementType element)
        {
            Color elementColor = GetElementColor(element);
            string formattedText = GetFormattedElementBadge(element);

            if (_bossSpriteRenderer != null)
            {
                _bossSpriteRenderer.color = elementColor;
            }

            if (_overheadBadgeText != null)
            {
                _overheadBadgeText.text = formattedText;

                if (_punchCoroutine != null)
                {
                    StopCoroutine(_punchCoroutine);
                }
                _punchCoroutine = StartCoroutine(PunchBadgeRoutine());
            }
        }

        private IEnumerator PunchBadgeRoutine()
        {
            if (_overheadBadgeText == null) yield break;

            Transform badgeTransform = _overheadBadgeText.transform;
            Vector3 baseScale = Vector3.one;
            float duration = 0.25f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Punch scale từ 1.35x về 1.0x
                float scale = Mathf.Lerp(1.35f, 1.0f, t);
                badgeTransform.localScale = baseScale * scale;
                yield return null;
            }

            badgeTransform.localScale = baseScale;
            _punchCoroutine = null;
        }

        private Color GetElementColor(ElementType element)
        {
            return element switch
            {
                ElementType.Kim => new Color(0.91f, 0.77f, 0.41f),   // #E8C468
                ElementType.Moc => new Color(0.30f, 0.48f, 0.24f),   // #4C7A3D
                ElementType.Thuy => new Color(0.16f, 0.71f, 0.96f),  // #29B6F6
                ElementType.Hoa => new Color(1.00f, 0.34f, 0.13f),   // #FF5722
                ElementType.Tho => new Color(0.84f, 0.66f, 0.48f),   // #D7A87A
                _ => Color.white
            };
        }

        private string GetFormattedElementBadge(ElementType element)
        {
            return element switch
            {
                ElementType.Kim => "<color=#E8C468><b>🔷 [KIM]</b></color>",
                ElementType.Moc => "<color=#4CAF50><b>🌿 [MỘC]</b></color>",
                ElementType.Thuy => "<color=#29B6F6><b>💧 [THỦY]</b></color>",
                ElementType.Hoa => "<color=#FF5722><b>🔥 [HỎA]</b></color>",
                ElementType.Tho => "<color=#D7A87A><b>🟫 [THỔ]</b></color>",
                _ => ""
            };
        }

        public void SetElementCycle(ElementType[] newCycle, float interval)
        {
            _elementCycle = newCycle;
            _switchIntervalSeconds = interval;
            _currentIndex = 0;
            _timer = 0f;
            ApplyElementVisuals(CurrentElement);
            OnBossElementChanged?.Invoke(CurrentElement);
        }
    }
}
