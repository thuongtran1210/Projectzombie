using System;
using UnityEngine;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.UI.HUD;
using ProjectZombie.Features.UI.StatsAndSkills;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Chuyên trách phân phối và inject dependencies từ PlayerContext vào tất cả các Presenter UI.
    /// Giúp GameplayBootstrapper tuân thủ nguyên tắc SRP (Single Responsibility Principle).
    /// </summary>
    [Serializable]
    public class GameplayUIBinder
    {
        [Header("UI Presenters")]
        [SerializeField] private RunHUDPresenter _runHUDPresenter;
        [SerializeField] private PlayerInfoUIPresenter _playerInfoUIPresenter;
        [SerializeField] private UpgradeUIPresenter _upgradeUIPresenter;
        [SerializeField] private GameOverScreenPresenter _gameOverScreenPresenter;
        [SerializeField] private CharacterGaugeWidgetPresenter _characterGaugeWidgetPresenter;
        [SerializeField] private SignatureSkillPresenter _signatureSkillPresenter;
        [SerializeField] private AttackButtonPresenter _attackButtonPresenter;

        public GameplayUIBinder(
            RunHUDPresenter runHUD,
            PlayerInfoUIPresenter playerInfo,
            UpgradeUIPresenter upgradeUI,
            GameOverScreenPresenter gameOver,
            CharacterGaugeWidgetPresenter gaugeWidget)
        {
            _runHUDPresenter = runHUD;
            _playerInfoUIPresenter = playerInfo;
            _upgradeUIPresenter = upgradeUI;
            _gameOverScreenPresenter = gameOver;
            _characterGaugeWidgetPresenter = gaugeWidget;
        }

        public void BindAll(PlayerContext context)
        {
            if (context == null || context.GameObject == null)
            {
                Debug.LogWarning("[GameplayUIBinder] Không thể Bind UI vì PlayerContext null.");
                return;
            }

            BindRunHUD(context);
            BindPlayerInfo(context);
            BindUpgradeUI(context);
            BindGameOverScreen(context);
            BindCharacterGauge(context);
            BindSignatureSkill(context);
            BindAttackButton(context);
        }

        private void BindRunHUD(PlayerContext context)
        {
            if (_runHUDPresenter != null)
            {
                _runHUDPresenter.Construct(
                    context.Health,
                    context.Stats,
                    context.Experience,
                    context.WeaponManager,
                    context.Passives);
                Debug.Log("[GameplayUIBinder] Đã inject dependencies vào RunHUDPresenter.");
            }
        }

        private void BindPlayerInfo(PlayerContext context)
        {
            if (_playerInfoUIPresenter != null)
            {
                _playerInfoUIPresenter.Construct(
                    context.Stats,
                    context.Health,
                    context.Experience,
                    context.WeaponManager);
                Debug.Log("[GameplayUIBinder] Đã inject dependencies vào PlayerInfoUIPresenter.");
            }
        }

        private void BindUpgradeUI(PlayerContext context)
        {
            if (_upgradeUIPresenter != null)
            {
                _upgradeUIPresenter.Construct(context.Experience, context.WeaponManager);
                Debug.Log("[GameplayUIBinder] Đã inject dependencies vào UpgradeUIPresenter.");
            }
        }

        private void BindGameOverScreen(PlayerContext context)
        {
            if (_gameOverScreenPresenter != null)
            {
                _gameOverScreenPresenter.Construct(context.Health);
                Debug.Log("[GameplayUIBinder] Đã inject dependencies vào GameOverScreenPresenter.");
            }
        }

        private void BindCharacterGauge(PlayerContext context)
        {
            if (_characterGaugeWidgetPresenter == null)
            {
                _characterGaugeWidgetPresenter = UnityEngine.Object.FindObjectOfType<CharacterGaugeWidgetPresenter>(true);
            }

            if (_characterGaugeWidgetPresenter != null)
            {
                _characterGaugeWidgetPresenter.Bind(context.GaugeProvider);
                Debug.Log($"[GameplayUIBinder] CharacterGaugeWidgetPresenter đã Bind provider: {(context.GaugeProvider != null ? context.GaugeProvider.GetType().Name : "None")}");
            }
        }

        private void BindSignatureSkill(PlayerContext context)
        {
            if (_signatureSkillPresenter == null)
            {
                _signatureSkillPresenter = UnityEngine.Object.FindObjectOfType<SignatureSkillPresenter>(true);
            }

            if (_signatureSkillPresenter != null)
            {
                _signatureSkillPresenter.Bind(context.SignatureSkillManager);
                Debug.Log("[GameplayUIBinder] SignatureSkillPresenter đã Bind SignatureSkillManager.");
            }
        }

        private void BindAttackButton(PlayerContext context)
        {
            if (_attackButtonPresenter == null)
            {
                _attackButtonPresenter = UnityEngine.Object.FindObjectOfType<AttackButtonPresenter>(true);
            }

            if (_attackButtonPresenter != null)
            {
                _attackButtonPresenter.Bind(context.WeaponManager);
                Debug.Log("[GameplayUIBinder] AttackButtonPresenter đã Bind WeaponManager.");
            }
        }
    }
}
