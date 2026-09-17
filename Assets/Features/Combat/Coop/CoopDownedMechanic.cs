using System;
using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Player.Core;
using ProjectZombie.Features.Shared;
using ProjectZombie.Core.Architecture;

namespace ProjectZombie.Features.Combat.Coop
{
    /// <summary>
    /// Cơ chế Gục Ngã (Downed) & Cứu Trợ Hồi Sinh (Revive) trong chế độ Multiplayer Co-op.
    /// Khi nhân vật hết máu, không chết ngay mà chuyển sang trạng thái chờ đồng đội cứu.
    /// Trận đấu chỉ kết thúc khi TOÀN BỘ người chơi trong phòng đều gục ngã (Team Wipe).
    /// </summary>
    public class CoopDownedMechanic : MonoBehaviour
    {
        [Header("Co-op Revive Settings")]
        [Tooltip("Bán kính đồng đội cần đứng gần để hồi sinh (mét)")]
        [SerializeField] private float _reviveRadius = 2.5f;

        [Tooltip("Thời gian đứng cứu để hồi sinh hoàn toàn (giây)")]
        [SerializeField] private float _reviveDurationRequired = 3.0f;

        [Tooltip("Tỷ lệ phần trăm máu hồi phục sau khi được cứu sống")]
        [Range(0.1f, 1.0f)]
        [SerializeField] private float _reviveHealthPercentage = 0.35f;

        [Header("Visual Indicators")]
        [SerializeField] private GameObject _downedVfx;
        [SerializeField] private GameObject _revivingProgressIndicator;

        private HealthSystem _healthSystem;
        private PlayerController _playerController;
        private PlayerAnimator _playerAnimator;
        private bool _isDowned = false;
        private float _currentReviveProgress = 0f;

        public bool IsDowned => _isDowned;
        public float ReviveProgress01 => Mathf.Clamp01(_currentReviveProgress / _reviveDurationRequired);

        public event Action OnPlayerDowned;
        public event Action OnPlayerRevived;
        public static event Action OnTeamWipe;

        private void Awake()
        {
            _healthSystem = GetComponent<HealthSystem>();
            _playerController = GetComponent<PlayerController>();
            _playerAnimator = GetComponentInChildren<PlayerAnimator>();
        }

        private void OnEnable()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnTryDie += HandleTryDie;
            }
        }

        private void OnDisable()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnTryDie -= HandleTryDie;
            }
        }

        private void Update()
        {
            if (!_isDowned) return;

            CheckTeammateReviving();
        }

        private bool HandleTryDie()
        {
            // Trong chế độ Co-op, chặn cái chết mặc định và chuyển sang Downed
            EnterDownedState();
            return true; // Chặn cái chết để HealthSystem không disable GameObject
        }

        public void EnterDownedState()
        {
            if (_isDowned) return;

            _isDowned = true;
            _currentReviveProgress = 0f;

            // Vô hiệu hóa di chuyển và tấn công
            if (_playerController != null && _playerController.InputProvider != null)
            {
                _playerController.InputProvider.IsInputBlocked = true;
            }

            if (_downedVfx != null) _downedVfx.SetActive(true);
            if (_revivingProgressIndicator != null) _revivingProgressIndicator.SetActive(false);

            OnPlayerDowned?.Invoke();

            // Kiểm tra xem tất cả người chơi khác có đều gục ngã hay không
            CheckTeamWipeCondition();
        }

        private void CheckTeammateReviving()
        {
            if (!ServiceContext.TryGet<IPlayerRegistry>(out var registry) || registry == null)
            {
                return;
            }

            bool hasTeammateReviving = false;
            Vector2 myPos = transform.position;

            for (int i = 0; i < registry.ActivePlayers.Count; i++)
            {
                var teammate = registry.ActivePlayers[i];
                if (teammate == null || teammate.GameObject == gameObject || !teammate.IsAlive) continue;

                // Kiểm tra nếu đồng đội cũng đang bị Downed thì không thể cứu
                if (teammate.GameObject.TryGetComponent<CoopDownedMechanic>(out var teammateDowned) && teammateDowned.IsDowned)
                {
                    continue;
                }

                float distSqr = ((Vector2)teammate.Transform.position - myPos).sqrMagnitude;
                if (distSqr <= _reviveRadius * _reviveRadius)
                {
                    hasTeammateReviving = true;
                    break;
                }
            }

            if (hasTeammateReviving)
            {
                _currentReviveProgress += Time.deltaTime;
                if (_revivingProgressIndicator != null) _revivingProgressIndicator.SetActive(true);

                if (_currentReviveProgress >= _reviveDurationRequired)
                {
                    CompleteRevive();
                }
            }
            else
            {
                // Giảm dần tiến độ cứu nếu đồng đội chạy ra ngoài vòng
                if (_currentReviveProgress > 0f)
                {
                    _currentReviveProgress = Mathf.Max(0f, _currentReviveProgress - Time.deltaTime * 1.5f);
                }
                if (_revivingProgressIndicator != null && _currentReviveProgress <= 0f)
                {
                    _revivingProgressIndicator.SetActive(false);
                }
            }
        }

        public void CompleteRevive()
        {
            _isDowned = false;
            _currentReviveProgress = 0f;

            if (_healthSystem != null)
            {
                float reviveHp = _healthSystem.MaxHealth * _reviveHealthPercentage;
                _healthSystem.Heal(reviveHp, allowRevive: true);
                _healthSystem.TriggerInvulnerability(2.5f); // 2.5s bất tử sau khi đứng dậy
            }

            if (_playerController != null && _playerController.InputProvider != null)
            {
                _playerController.InputProvider.IsInputBlocked = false;
            }

            if (_downedVfx != null) _downedVfx.SetActive(false);
            if (_revivingProgressIndicator != null) _revivingProgressIndicator.SetActive(false);

            OnPlayerRevived?.Invoke();
        }

        private void CheckTeamWipeCondition()
        {
            if (!ServiceContext.TryGet<IPlayerRegistry>(out var registry) || registry == null)
            {
                OnTeamWipe?.Invoke();
                return;
            }

            bool anyPlayerStanding = false;
            for (int i = 0; i < registry.ActivePlayers.Count; i++)
            {
                var p = registry.ActivePlayers[i];
                if (p == null || !p.IsAlive) continue;

                if (p.GameObject.TryGetComponent<CoopDownedMechanic>(out var downed) && downed.IsDowned)
                {
                    continue;
                }

                anyPlayerStanding = true;
                break;
            }

            if (!anyPlayerStanding)
            {
                Debug.Log("<color=#FF4444>[CoopDownedMechanic] Toàn đội đã gục ngã (Team Wipe)! Trận đấu kết thúc.</color>");
                OnTeamWipe?.Invoke();
            }
        }
    }
}
