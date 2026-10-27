using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Authored thumb pad anchored to the original press, using the same screen-pixel pull as aim.</summary>
    public sealed class DefenseSkillStickView : MonoBehaviour
    {
        [SerializeField] private RectTransform _pad;
        [SerializeField] private RectTransform _thumb;
        [SerializeField] private Graphic _padGraphic;
        [SerializeField] private Graphic _thumbGraphic;
        [SerializeField] private Graphic _cancelGraphic;
        [SerializeField, Min(1f)] private float _padDiameterPixels = 128f;
        [SerializeField, Min(1f)] private float _thumbDiameterPixels = 24f;
        [SerializeField] private Color _normal = new Color(0.475f, 0.812f, 1f, 0.65f);
        [SerializeField] private Color _cancel = new Color(0.843f, 0.471f, 0.435f, 0.85f);
        private Vector2 _press;
        private Camera _eventCamera;
        private RectTransform _parent;
        public bool IsConfigured => _pad != null && _thumb != null && _padGraphic != null && _thumbGraphic != null;

        public void Begin(Vector2 position, Camera eventCamera)
        {
            if (!IsConfigured) return;
            _parent = _pad.parent as RectTransform;
            if (_parent == null) return;
            _press = position;
            _eventCamera = eventCamera;
            float scale = Mathf.Max(0.001f, Mathf.Abs(_pad.lossyScale.x));
            _pad.sizeDelta = Vector2.one * (_padDiameterPixels / scale);
            _thumb.sizeDelta = Vector2.one * (_thumbDiameterPixels / scale);
            Place(position, position, false);
            _pad.gameObject.SetActive(true);
        }

        public void Move(Vector2 position, float maximumScreenDistance, bool cancelled)
        {
            if (!IsConfigured || _parent == null || maximumScreenDistance <= 0f) return;
            Vector2 delta = Vector2.ClampMagnitude(position - _press, maximumScreenDistance);
            Place(_press, _press + delta, cancelled);
        }

        private void Place(Vector2 center, Vector2 thumb, bool cancelled)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_parent, center, _eventCamera, out var local)) return;
            _pad.localPosition = new Vector3(local.x, local.y, 0f);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_pad, thumb, _eventCamera, out var offset))
                _thumb.anchoredPosition = offset;
            _padGraphic.color = _thumbGraphic.color = cancelled ? _cancel : _normal;
            if (_cancelGraphic != null) _cancelGraphic.color = cancelled ? _cancel : _normal;
        }

        public void Hide()
        {
            if (_pad != null) _pad.gameObject.SetActive(false);
            if (_cancelGraphic != null) _cancelGraphic.color = _normal;
        }
    }
}
