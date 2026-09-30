using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Shared.VFX;
using UnityEngine;

namespace ProjectZombie.Features.Elements
{
    /// <summary>Composes patch tracking with pooled visual presentation.</summary>
    public sealed class SteamSlipPatchManager : MonoBehaviour
    {
        private static SteamSlipPatchManager _instance;
        private SteamSlipReactionSettings _settings;
        private SteamSlipPatchTracker _tracker;

        public static void CreatePatch(Enemy primary)
        {
            if (primary == null) return;
            Instance.ShowPatch(primary);
        }

        private static SteamSlipPatchManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindObjectOfType<SteamSlipPatchManager>();
                if (_instance == null) _instance = new GameObject("[SteamSlipPatchManager]").AddComponent<SteamSlipPatchManager>();
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            _settings = SteamSlipReactionSettings.Runtime;
            if (_settings == null)
            {
                Debug.LogError("Missing Resources/SteamSlipReactionSettings asset; Bốc Hơi patches are disabled.", this);
                enabled = false;
                return;
            }
            _tracker = new SteamSlipPatchTracker(_settings);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void ShowPatch(Enemy primary)
        {
            if (_tracker == null) return;
            if (!_tracker.TryCreatePatch(primary, Time.time, out Vector2 center)) return;
            global::ProjectZombie.Core.Audio.AudioService.Current?.PlayElementalReaction(center);
            if (_settings.patchVisualPrefab == null || GlobalVFXPoolManager.Instance == null) return;
            GlobalVFXPoolManager.Instance.PlayEffect(_settings.patchVisualPrefab, center, Quaternion.identity, _settings.patchLifetime);
        }

        private void Update() => _tracker.Tick(Time.time, TargetingUtility.EnemyLayerMask);
    }
}
