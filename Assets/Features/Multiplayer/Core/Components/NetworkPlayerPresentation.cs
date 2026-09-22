using UnityEngine;

namespace ProjectZombie.Features.Multiplayer.Core.Components
{
    /// <summary>
    /// Component quản lý render, visual và animation presentation cho Remote Proxy.
    /// Tuân thủ Single Responsibility Principle (Mục 3.1 AGENTS.md).
    /// </summary>
    public class NetworkPlayerPresentation : MonoBehaviour
    {
        private NetworkCharacterAnimatorSync _animatorSync;

        private void Awake()
        {
            _animatorSync = GetComponent<NetworkCharacterAnimatorSync>();
            if (_animatorSync == null)
            {
                _animatorSync = gameObject.AddComponent<NetworkCharacterAnimatorSync>();
            }
        }

        public void ResetToIdle()
        {
            if (_animatorSync != null)
            {
                _animatorSync.ResetToIdle();
            }
        }

        public void ForceDeadAnimation()
        {
            if (_animatorSync != null)
            {
                _animatorSync.ForceDeadAnimation();
            }
        }

        public void UpdateRemoteVisuals()
        {
            if (_animatorSync != null)
            {
                _animatorSync.UpdateRemoteAnimation();
            }
        }
    }
}
