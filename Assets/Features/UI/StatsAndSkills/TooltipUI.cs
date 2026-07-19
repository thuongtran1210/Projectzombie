using UnityEngine;
using TMPro;

namespace ProjectZombie.Features.UI.StatsAndSkills
{
    public class TooltipUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private RectTransform _backgroundRect;
        
        // Cấu hình offset để tooltip không bị che bởi con trỏ chuột
        [SerializeField] private Vector2 _offset = new Vector2(15f, -15f);

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            Hide();
        }

        public void Show(string title, string description)
        {
            if (_titleText != null) _titleText.text = title;
            if (_descriptionText != null) _descriptionText.text = description;
            
            gameObject.SetActive(true);
            UpdatePosition();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (gameObject.activeSelf)
            {
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            Vector2 mousePos = Input.mousePosition;
            
            if (_rectTransform != null)
            {
                _rectTransform.position = (Vector3)mousePos + (Vector3)_offset;
            }
            else
            {
                transform.position = (Vector3)mousePos + (Vector3)_offset;
            }
        }
    }
}
