using UnityEngine;
using TMPro;

namespace ProjectZombie.Features.UI.StatsAndSkills
{
    public class TooltipUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private RectTransform backgroundRect;
        
        // Cấu hình offset để tooltip không bị che bởi con trỏ chuột
        [SerializeField] private Vector2 offset = new Vector2(15f, -15f);

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            Hide();
        }

        public void Show(string title, string description)
        {
            if (titleText != null) titleText.text = title;
            if (descriptionText != null) descriptionText.text = description;
            
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
            _rectTransform.position = mousePos + offset;

            // Xử lý để tooltip không tràn ra khỏi màn hình (nếu cần thiết) có thể thêm logic ở đây
        }
    }
}
