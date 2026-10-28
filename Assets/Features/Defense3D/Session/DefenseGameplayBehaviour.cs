using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Lifecycle binding for authored actors; an unbound actor cannot advance gameplay.</summary>
    public abstract class DefenseGameplayBehaviour : MonoBehaviour
    {
        protected IDefenseGameplayContext Gameplay { get; private set; }
        protected bool CanAdvanceGameplay => Gameplay != null && Gameplay.AdvancesGameplay;
        protected float GameplayTime => Gameplay != null ? Gameplay.ElapsedSeconds : 0f;

        /// <summary>Called by scene composition or the spawn factory, never by global lookup.</summary>
        public void BindGameplay(IDefenseGameplayContext gameplay)
        {
            if (gameplay == null) throw new ArgumentNullException(nameof(gameplay));
            UnbindGameplay();
            Gameplay = gameplay;
            Gameplay.GameplayGateChanged += OnGameplayGateChanged;
            Gameplay.RunCleanup += OnGameplayCleanup;
            Gameplay.RunReset += OnGameplayReset;
            OnGameplayBound();
            OnGameplayGateChanged(Gameplay.AdvancesGameplay);
        }

        protected virtual void OnGameplayBound() { }
        protected virtual void OnGameplayCleanup() { }
        protected virtual void OnGameplayGateChanged(bool active) { }
        protected virtual void OnGameplayReset() { }

        protected virtual void OnDestroy()
        {
            UnbindGameplay();
        }

        /// <summary>Gỡ đăng ký session khi trả pool; không phát lại sự kiện cleanup.</summary>
        public void UnbindGameplay()
        {
            if (Gameplay == null) return;
            Gameplay.GameplayGateChanged -= OnGameplayGateChanged;
            Gameplay.RunCleanup -= OnGameplayCleanup;
            Gameplay.RunReset -= OnGameplayReset;
            Gameplay = null;
        }
    }
}
