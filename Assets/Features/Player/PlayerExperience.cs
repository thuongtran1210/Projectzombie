using UnityEngine;
using System;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Manages the player's experience points, levels, and level-up events.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerExperience : MonoBehaviour
    {
        [Header("Experience Settings")]
        [SerializeField] private float baseExpRequired = 5f;
        [SerializeField] private float expGrowthFactor = 1.25f;

        public int CurrentLevel { get; private set; } = 1;
        public float CurrentExp { get; private set; } = 0f;
        public float MaxExp { get; private set; } = 100f;

        private PlayerStats _playerStats;

        /// <summary>
        /// Triggered when the player levels up. Passes the new level.
        /// </summary>
        public event Action<int> OnLevelUp;
        /// <summary>
        /// Triggered when EXP changes. Useful for updating the EXP bar UI.
        /// </summary>
        public event Action<float, float> OnExpChanged;
        /// <summary>
        /// Triggered when raw EXP is gained. Dùng cho hệ thống Co-op phân phối kinh nghiệm.
        /// </summary>
        public event Action<float> OnExpGained;

        private void Awake()
        {
            _playerStats = GetComponent<PlayerStats>();
            MaxExp = CalculateMaxExp(CurrentLevel);
        }

        public void AddExp(float amount)
        {
            // Không nhận EXP và không kích hoạt LevelUp nếu nhân vật đã tử trận hoặc Game Over
            if (TryGetComponent<HealthSystem>(out var hp) && hp.CurrentHealth <= 0) return;
            if (GameStateManager.Instance != null && (GameStateManager.Instance.CurrentState == GameState.GameOver || GameStateManager.Instance.CurrentState == GameState.MainMenu)) return;

            // Apply multiplier from stats
            float multiplier = _playerStats != null ? _playerStats.ExpMultiplier : 1f;
            float finalExp = amount * multiplier;

            AddDirectExp(finalExp);
            OnExpGained?.Invoke(finalExp);
        }

        public void AddDirectExp(float finalExp)
        {
            if (TryGetComponent<HealthSystem>(out var hp) && hp.CurrentHealth <= 0) return;
            if (GameStateManager.Instance != null && (GameStateManager.Instance.CurrentState == GameState.GameOver || GameStateManager.Instance.CurrentState == GameState.MainMenu)) return;

            CurrentExp += finalExp;

            // Check for level up(s)
            while (CurrentExp >= MaxExp)
            {
                CurrentExp -= MaxExp;
                LevelUp();
            }

            OnExpChanged?.Invoke(CurrentExp, MaxExp);
        }

        private void LevelUp()
        {
            CurrentLevel++;
            MaxExp = CalculateMaxExp(CurrentLevel);
            OnLevelUp?.Invoke(CurrentLevel);
        }

        public void ResetExperience()
        {
            CurrentLevel = 1;
            CurrentExp = 0f;
            MaxExp = CalculateMaxExp(1);
            OnExpChanged?.Invoke(0f, MaxExp);
            Debug.Log("[PlayerExperience] Đã reset Level và EXP về cấp 1.");
        }

        private float CalculateMaxExp(int level)
        {
            return baseExpRequired * Mathf.Pow(expGrowthFactor, level - 1);
        }
    }
}
