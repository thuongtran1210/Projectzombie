using System;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Combat.Coop;

namespace ProjectZombie.Features.Multiplayer.Core.Components
{
    /// <summary>
    /// Component chuyên trách quản lý sinh lực, máu mạng và trạng thái gục ngã / hồi sinh.
    /// Tuân thủ Single Responsibility Principle (Mục 3.1 AGENTS.md).
    /// </summary>
    public class NetworkPlayerVitals : MonoBehaviour
    {
        private HealthSystem _healthSystem;
        private CoopDownedMechanic _downedMechanic;

        public bool IsDead => _healthSystem != null && !_healthSystem.IsAlive;
        public bool IsDowned => _downedMechanic != null && _downedMechanic.IsDowned;

        public HealthSystem HealthSystem => _healthSystem;

        private void Awake()
        {
            _healthSystem = GetComponent<HealthSystem>();
            if (!TryGetComponent<CoopDownedMechanic>(out _downedMechanic))
            {
                _downedMechanic = gameObject.AddComponent<CoopDownedMechanic>();
            }
        }

        public void ResetVitals()
        {
            if (_downedMechanic != null)
            {
                _downedMechanic.ResetDownedState();
            }
            if (_healthSystem != null)
            {
                _healthSystem.ResetHealth();
            }
        }

        public void SyncRemoteHealth(float networkHealth, float networkMaxHealth)
        {
            if (_healthSystem == null) return;

            if (networkMaxHealth > 0 && Mathf.Abs(_healthSystem.MaxHealth - networkMaxHealth) > 0.5f)
            {
                _healthSystem.SetMaxHealth(networkMaxHealth, fillCurrentHealth: false);
            }

            if (Mathf.Abs(_healthSystem.CurrentHealth - networkHealth) > 0.5f)
            {
                _healthSystem.SetCurrentHealth(networkHealth);
            }
        }

        public void UpdateDownedState(bool networkIsDowned)
        {
            if (_downedMechanic == null) return;

            if (networkIsDowned)
            {
                if (!_downedMechanic.IsDowned)
                {
                    _downedMechanic.EnterDownedState();
                }
            }
            else
            {
                if (_downedMechanic.IsDowned)
                {
                    _downedMechanic.CompleteRevive();
                }
            }
        }
    }
}
