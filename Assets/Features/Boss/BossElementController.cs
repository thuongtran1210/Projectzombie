using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Boss
{
    /// <summary>
    /// Component gắn trên Boss cho phép luân phiên xoay vòng thuộc tính Ngũ Hành theo thời gian thực (ví dụ Ngưu Đầu Mã Diện & Diêm Vương).
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

        public ElementType CurrentElement => _elementCycle != null && _elementCycle.Length > 0 
            ? _elementCycle[_currentIndex] 
            : ElementType.None;

        public event System.Action<ElementType> OnBossElementChanged;

        private void Start()
        {
            if (_bossSpriteRenderer == null)
            {
                _bossSpriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_elementCycle != null && _elementCycle.Length > 0)
            {
                ApplyElementVisuals(CurrentElement);
                OnBossElementChanged?.Invoke(CurrentElement);
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
            if (_bossSpriteRenderer == null) return;

            Color elementColor = element switch
            {
                ElementType.Kim => new Color(1f, 0.84f, 0f),      // Vàng (Gold)
                ElementType.Moc => new Color(0.3f, 0.85f, 0.4f),  // Xanh lá (Green)
                ElementType.Thuy => new Color(0.13f, 0.59f, 0.95f),// Xanh dương (Blue)
                ElementType.Hoa => new Color(1f, 0.34f, 0.13f),    // Đỏ cam (Red)
                ElementType.Tho => new Color(0.47f, 0.33f, 0.28f), // Nâu (Brown)
                _ => Color.white
            };

            _bossSpriteRenderer.color = elementColor;
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
