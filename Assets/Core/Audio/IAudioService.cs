using UnityEngine;

namespace ProjectZombie.Core.Audio
{
    /// <summary>Audio capabilities used by gameplay systems.</summary>
    public interface IAudioService
    {
        float BGMVolume { get; }
        float SFXVolume { get; }
        void PlaySound(global::Core.Audio.AudioConfigSO config, Vector3 position = default, float customPitch = 0f);
        void PlaySound(AudioClip clip, Vector3 position = default, float volume = 1f, float pitch = 1f);
        void PlayBGM(global::Core.Audio.AudioConfigSO config, bool fade = true);
        void PlayPhaseStinger(global::Core.Audio.AudioConfigSO stingerConfig);
        void PlayUIClick(float pitch = 1f);
        void PlayUIConfirm(float pitch = 1f);
        void PlayUIError(float pitch = 1f);
        void PlayWeaponEquip(float pitch = 1f);
        void PlayHubBGM();
        void PlayCoinTick(float pitch = 1f);
        void PlaySlash(bool isCritical = false, Vector3 position = default);
        void PlayPlayerDash(Vector3 position = default);
        void PlayPlayerHurt(Vector3 position = default);
        void PlayProjectileShoot(Vector3 position = default);
        void PlayProjectileExplode(Vector3 position = default);
        void PlayMagicOrbit(Vector3 position = default);
        void PlayUltimateSkillCast(Vector3 position = default);
        void PlayElementalReaction(Vector3 position = default);
        void PlayStatusFreeze(Vector3 position = default);
        void PlayStatusBurn(Vector3 position = default);
        void PlayBossRoar(Vector3 position = default);
        void PlayBossSmash(Vector3 position = default);
        void SetBGMVolume(float linearVolume, bool saveToPrefs = true);
        void SetSFXVolume(float linearVolume, bool saveToPrefs = true);
    }

    /// <summary>Temporary composition bridge while Unity consumers migrate to injected references.</summary>
    public static class AudioService
    {
        public static IAudioService Current
        {
            get
            {
                if (global::ProjectZombie.Core.Architecture.ServiceContext.TryGet<IAudioService>(out var service))
                    return service;

                // Preserve the legacy lazy-create behavior while keeping gameplay independent
                // from the concrete AudioManager type.
                var manager = global::Core.Audio.AudioManager.Instance;
                global::ProjectZombie.Core.Architecture.ServiceContext.Register<IAudioService>(manager);
                return manager;
            }
        }
    }
}
