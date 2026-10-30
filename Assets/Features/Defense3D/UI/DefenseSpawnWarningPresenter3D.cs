using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Read-only runtime warnings, including scheduled waves and live cannon landing flights.</summary>
    public sealed class DefenseSpawnWarningPresenter3D : DefenseGameplayBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RectTransform _layer;
        [SerializeField] private DefenseSpawnWarningGraphic[] _views;
        [SerializeField, Min(.1f)] private float _leadSeconds = 5f;
        [SerializeField, Min(32f)] private float _screenMargin = 38f;
        [SerializeField, Min(40f)] private float _spacing = 46f;
        private IDefenseSpawnWarningSource _warnings;
        [SerializeField, Min(0f)] private float _arrivalSeconds = .8f;
        private DefensePoint3[] _occupied;

        public void Configure(IDefenseSpawnWarningSource source)
        {
            ValidateBindings();
            if (Gameplay == null || source == null)
                throw new InvalidOperationException("Bind warning session and runtime source before configuring.");
            _warnings = source;
            _occupied = new DefensePoint3[_views.Length];
            HideAll();
        }

        private void ValidateBindings()
        {
            if (_camera == null || _canvas == null || _layer == null || _canvas.renderMode != RenderMode.ScreenSpaceOverlay ||
                _views == null || _views.Length == 0 || !DefenseDamageRules.IsFinite(_leadSeconds) || _leadSeconds <= 0f ||
                !DefenseDamageRules.IsFinite(_arrivalSeconds) || _arrivalSeconds < 0f ||
                !DefenseDamageRules.IsFinite(_screenMargin) || _screenMargin < 32f || !DefenseDamageRules.IsFinite(_spacing) || _spacing < 40f)
                throw new InvalidOperationException("Assign camera, overlay canvas, layer and authored warning slots.");
            var unique = new HashSet<DefenseSpawnWarningGraphic>();
            foreach (var view in _views)
                if (view == null || !unique.Add(view) || !view.transform.IsChildOf(_layer) || view.raycastTarget)
                    throw new InvalidOperationException("Warning slots must be unique non-raycast children of the authored layer.");
        }

        private void LateUpdate()
        {
            if (!CanAdvanceGameplay || _warnings == null || _occupied == null || _camera == null || !_camera.isActiveAndEnabled)
            {
                HideAll();
                return;
            }
            Rect pixels = _camera.pixelRect;
            Rect safe = Screen.safeArea;
            float left = Mathf.Max(pixels.xMin, safe.xMin), bottom = Mathf.Max(pixels.yMin, safe.yMin);
            float width = Mathf.Min(pixels.xMax, safe.xMax) - left, height = Mathf.Min(pixels.yMax, safe.yMax) - bottom;
            if (width <= _screenMargin * 2f || height <= _screenMargin * 2f)
            {
                HideAll();
                return;
            }
            int shown = 0;
            // Bosses, elite and live landings get available slots before ordinary wave units.
            for (int priority = 3; priority >= 1; priority--)
            for (int i = 0; i < _warnings.Count && shown < _views.Length; i++)
            {
                var warning = _warnings.GetNotice(i);
                var symbol = SymbolFor(warning.Job.ActualSpawnType, warning.Job.Kind);
                int rank = symbol == DefenseSpawnWarningSymbol.Boss || symbol == DefenseSpawnWarningSymbol.Commander ? 3 :
                    symbol == DefenseSpawnWarningSymbol.Elite || warning.Job.Kind == DefenseSpawnKind.Landing ? 2 : 1;
                if (rank != priority || !warning.TryDisplay(GameplayTime, _leadSeconds, _arrivalSeconds, out float fraction, out bool pending)) continue;
                Vector3 point = new Vector3(warning.Position.X, warning.Position.Y + .6f, warning.Position.Z);
                Vector3 viewport = _camera.WorldToViewportPoint(point);
                // Rebase viewport coordinates onto the intersection of camera pixels and device safe area.
                viewport.x = (pixels.xMin + viewport.x * pixels.width - left) / width;
                viewport.y = (pixels.yMin + viewport.y * pixels.height - bottom) / height;
                if (!DefenseIndicatorProjection.TryProject(new DefensePoint3(viewport.x, viewport.y, viewport.z), left, bottom,
                    width, height, _screenMargin, out var projected) ||
                    !DefenseIndicatorProjection.TryPlace(projected, left, bottom, width, height, _screenMargin, _spacing,
                        _occupied, shown, out var placed) ||
                    !RectTransformUtility.ScreenPointToLocalPointInRectangle(_layer, new Vector2(placed.X, placed.Y), null, out var local)) continue;
                var view = _views[shown];
                _occupied[shown++] = placed;
                view.rectTransform.localPosition = new Vector3(local.x, local.y, 0f);
                view.rectTransform.localScale = Vector3.one / Mathf.Max(.001f, _canvas.scaleFactor);
                view.Render(fraction, projected.Angle, projected.IsOffscreen, symbol, .5f + .5f * Mathf.Sin(GameplayTime * 6.981317f),
                    pending, warning.State == DefenseSpawnNoticeState.Spawned);
                if (!view.gameObject.activeSelf) view.gameObject.SetActive(true);
            }
            for (int i = shown; i < _views.Length; i++)
                if (_views[i] != null && _views[i].gameObject.activeSelf) _views[i].gameObject.SetActive(false);
        }

        public static DefenseSpawnWarningSymbol SymbolFor(string type, DefenseSpawnKind kind)
        {
            if (kind == DefenseSpawnKind.Landing) return DefenseSpawnWarningSymbol.Parachute;
            if (kind == DefenseSpawnKind.ArenaBoss) return DefenseSpawnWarningSymbol.Boss;
            switch (type)
            {
                case "magiap": return DefenseSpawnWarningSymbol.Armored;
                case "magiap_boss": return DefenseSpawnWarningSymbol.Commander;
                case "yaksha_boss": return DefenseSpawnWarningSymbol.Boss;
                case "thuyquai": return DefenseSpawnWarningSymbol.Fish;
                case "huyetdoi": return DefenseSpawnWarningSymbol.Bat;
                case "phicuong": return DefenseSpawnWarningSymbol.Elite;
                case "quaidu": return DefenseSpawnWarningSymbol.Parachute;
                case "ghost_cannon": return DefenseSpawnWarningSymbol.Cannon;
                default: return DefenseSpawnWarningSymbol.Demon;
            }
        }

        private void HideAll()
        {
            if (_views == null) return;
            foreach (var view in _views)
                if (view != null && view.gameObject.activeSelf) view.gameObject.SetActive(false);
        }

        protected override void OnGameplayGateChanged(bool active)
        {
            if (!active) HideAll();
        }
        protected override void OnGameplayCleanup() => HideAll();
        protected override void OnGameplayReset() => HideAll();
        private void OnDisable() => HideAll();
    }
}
