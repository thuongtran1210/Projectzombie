using UnityEngine;
using ProjectZombie.Features.Player.Mechanics;

namespace ProjectZombie.Features.UI.HUD
{
    /// <summary>
    /// Presenter điều phối hiển thị thanh cơ chế đặc thù nhân vật (Character Mechanic Gauge).
    /// Hoạt động độc lập với RunHUDPresenter, tự động bind bất kỳ component nào hiện thực ICharacterGaugeProvider.
    /// Tuân thủ MVP và Open/Closed Principle.
    /// </summary>
    public class CharacterGaugeWidgetPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private CharacterGaugeWidgetView _view;

        private ICharacterGaugeProvider _currentProvider;

        private void Awake()
        {
            if (_view == null)
            {
                _view = GetComponent<CharacterGaugeWidgetView>();
            }

            // Mặc định ẩn View cho đến khi có Provider được bind
            if (_view != null)
            {
                _view.SetVisible(false);
            }
        }

        private void Start()
        {
            // Tự động fallback tìm ICharacterGaugeProvider trên Player nếu chưa được inject qua Bootstrapper
            if (_currentProvider == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    var provider = player.GetComponent<ICharacterGaugeProvider>();
                    if (provider != null)
                    {
                        Bind(provider);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            Unbind();
        }

        /// <summary>
        /// Kết nối với Provider cơ chế của nhân vật hiện tại và hiển thị UI.
        /// </summary>
        public void Bind(ICharacterGaugeProvider provider)
        {
            Unbind();

            if (provider == null)
            {
                if (_view != null)
                {
                    _view.SetVisible(false);
                }
                return;
            }

            _currentProvider = provider;
            _currentProvider.OnGaugeValueChanged += HandleGaugeValueChanged;

            if (_view != null)
            {
                _view.Setup(_currentProvider.MinValue, _currentProvider.MaxValue);
                _view.UpdateGauge(_currentProvider.CurrentValue, _currentProvider.GaugeTitle, _currentProvider.GaugeColor);
                _view.SetVisible(true);
            }
        }

        /// <summary>
        /// Hủy kết nối Provider hiện tại và ẩn UI.
        /// </summary>
        public void Unbind()
        {
            if (_currentProvider != null)
            {
                _currentProvider.OnGaugeValueChanged -= HandleGaugeValueChanged;
                _currentProvider = null;
            }

            if (_view != null)
            {
                _view.SetVisible(false);
            }
        }

        private void HandleGaugeValueChanged(float value, string title)
        {
            if (_view != null)
            {
                Color color = _currentProvider != null ? _currentProvider.GaugeColor : Color.white;
                _view.UpdateGauge(value, title, color);
            }
        }
    }
}
