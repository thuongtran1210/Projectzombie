using System;
using TMPro;
using UnityEngine;

namespace ProjectZombie.Features.UI.DamageText
{
    /// <summary>
    /// Passive View hiển thị chữ số sát thương chiến đấu (Zero GC, không dùng Coroutine).
    /// Toàn bộ việc cập nhật vị trí, tỉ lệ và độ mờ được điều phối tập trung bởi DamageTextManager.
    /// </summary>
    [RequireComponent(typeof(TextMeshPro))]
    public class DamageTextItem : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _textMesh;

        private void Awake()
        {
            if (_textMesh == null) _textMesh = GetComponent<TextMeshPro>();
        }

        /// <summary>
        /// Dừng hiển thị và đưa item về trạng thái sẵn sàng trong Pool.
        /// </summary>
        public void StopPresentation()
        {
            gameObject.SetActive(false);
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Khởi tạo nội dung và trạng thái ban đầu của nhãn số sát thương khi mượn từ Pool.
        /// </summary>
        public void InitializePresentation(
            string text,
            Color color,
            float fontSize,
            Vector3 position,
            Quaternion rotation,
            string sortingLayer,
            int sortingOrder)
        {
            if (_textMesh == null) _textMesh = GetComponent<TextMeshPro>();
            if (_textMesh == null) throw new InvalidOperationException("Combat text requires TextMeshPro.");

            _textMesh.SetText(text);
            _textMesh.color = color;
            _textMesh.fontSize = fontSize;
            _textMesh.alignment = TextAlignmentOptions.Center;
            _textMesh.sortingLayerID = SortingLayer.NameToID(sortingLayer);
            _textMesh.sortingOrder = sortingOrder;

            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = Vector3.one;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Cập nhật vị trí, hướng quay, tỉ lệ và màu sắc (alpha) mỗi frame từ vòng lặp trung tâm (Zero GC).
        /// </summary>
        public void UpdatePresentation(Vector3 position, Quaternion rotation, float scale, Color color)
        {
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = Vector3.one * scale;
            if (_textMesh != null)
            {
                _textMesh.color = color;
            }
        }
    }
}
