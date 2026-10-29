using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.UI
{
    [ExecuteAlways]
    public sealed class CodexResponsiveLayout : MonoBehaviour
    {
        [SerializeField] private RectTransform _safeArea;
        [SerializeField] private GridLayoutGroup _grid;
        private Rect _lastSafe;
        private Vector2 _lastScreen;
        private float _lastWidth = -1;
        private void LateUpdate() => Refresh();
        public void Refresh()
        {
            var screen = new Vector2(Screen.width, Screen.height);
            var safe = Screen.safeArea;
            if (Application.isPlaying && _safeArea != null && screen.x > 0 && screen.y > 0
                && (safe != _lastSafe || screen != _lastScreen))
            {
                _safeArea.anchorMin = new Vector2(safe.xMin / screen.x, safe.yMin / screen.y);
                _safeArea.anchorMax = new Vector2(safe.xMax / screen.x, safe.yMax / screen.y);
                _safeArea.offsetMin = _safeArea.offsetMax = Vector2.zero;
                _lastSafe = safe; _lastScreen = screen;
            }
            if (_grid == null) return;
            float width = ((RectTransform)_grid.transform).rect.width;
            if (width <= 0 || Mathf.Abs(width - _lastWidth) < .5f) return;
            _lastWidth = width;
            int columns = width < 760 ? 4 : 5;
            _grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _grid.constraintCount = columns;
            float cell = (width - _grid.padding.horizontal - _grid.spacing.x * (columns - 1)) / columns;
            _grid.cellSize = new Vector2(cell, cell * 1.28f);
        }
    }
}
