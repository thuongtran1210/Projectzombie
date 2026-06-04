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
        [SerializeField] private float baseExpRequired = 100f;
        [SerializeField] private float expGrowthFactor = 1.2f;

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

        private void Awake()
        {
            _playerStats = GetComponent<PlayerStats>();
            MaxExp = CalculateMaxExp(CurrentLevel);
        }

        public void AddExp(float amount)
        {
            // Apply multiplier from stats
            float multiplier = _playerStats != null ? _playerStats.ExpMultiplier : 1f;
            float finalExp = amount * multiplier;

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

        private float CalculateMaxExp(int level)
        {
            return baseExpRequired * Mathf.Pow(expGrowthFactor, level - 1);
        }
    }
}
