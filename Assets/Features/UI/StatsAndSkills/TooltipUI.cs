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
            Vector2 pos = Input.mousePosition;
            Vector3 targetPos = (Vector3)pos + (Vector3)_offset;

            // Clamp vị trí trong kích thước màn hình
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            if (_rectTransform != null)
            {
                float width = _rectTransform.rect.width * _rectTransform.lossyScale.x;
                float height = _rectTransform.rect.height * _rectTransform.lossyScale.y;

                if (targetPos.x + width > screenWidth)
                {
                    targetPos.x = pos.x - width - Mathf.Abs(_offset.x);
                }
                if (targetPos.y - height < 0)
                {
                    targetPos.y = pos.y + height + Mathf.Abs(_offset.y);
                }

                _rectTransform.position = targetPos;
            }
            else
            {
                transform.position = targetPos;
            }
        }
    }
}
