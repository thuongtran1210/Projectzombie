using UnityEngine;
using ProjectZombie.Features.Combat.Events;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Runtimes
{
    public enum LongTienForm
    {
        DragonOcean, // Dương - Sát Phạt Bộc Phá (+100% Dame)
        FairyMother   // Âm - Trị Liệu Hộ Thân (Hào Quang Hồi Máu)
    }

    /// <summary>
    /// Runtime thực thi logic chiến đấu của Đại Lõi [LONG TIÊN HUYẾT MẠCH] (Hệ Âm Dương - Rồng/Tiên, Miễn Tử).
    /// Tuân thủ 0 GC Allocations trong toàn bộ gameplay loop.
    /// </summary>
    public class LongTienCoreRuntime : MythicCoreRuntime
    {
        public override MythicArchetype Archetype => MythicArchetype.LongTienHuyetMach;

        [Header("Settings - Âm Dương Lưỡng Hợp")]
        [SerializeField] private float _fairyHealRate = 0.02f; // Hồi 2% Max HP mỗi giây ở dạng Tiên
        [SerializeField] private float _dragonDamageMultiplier = 2.0f; // x2 Sát thương ở dạng Rồng
        [SerializeField] private float _formSwitchCooldown = 3.0f;

        private LongTienForm _currentForm = LongTienForm.DragonOcean;
        private int _freeReviveCount = 1; // 1 Lần Miễn Tử mỗi trận đấu
        private float _healTickTimer = 0f;
        private float _lastSwitchTime = -99f;

        public LongTienForm CurrentForm => _currentForm;
        public int RemainingRevives => _freeReviveCount;
        public float DragonDamageMultiplier => _dragonDamageMultiplier;
        public float FormSwitchCooldown => _formSwitchCooldown;

        protected override void OnInitialize()
        {
            _currentForm = LongTienForm.DragonOcean;
            _freeReviveCount = 1;
            _healTickTimer = 0f;
            _lastSwitchTime = -99f;

            Debug.Log("<color=#FF00E5>[Long Tiên]</color> Thái Cực Long Tiên Kích Hoạt! Sở hữu 1 Mạng Miễn Tử!");
        }

        protected override void OnTeardown()
        {
            _freeReviveCount = 0;
        }

        private void Update()
        {
            if (!IsActive || Context?.Health == null) return;

            // Hồi máu liên tục khi ở dạng Tiên Mẫu
            if (_currentForm == LongTienForm.FairyMother)
            {
                _healTickTimer += Time.deltaTime;
                if (_healTickTimer >= 1.0f)
                {
                    _healTickTimer = 0f;
                    float healAmount = Context.Health.MaxHealth * _fairyHealRate;
                    Context.Health.Heal(healAmount);
                }
            }
        }

        protected override void SubscribeCombatEvents()
        {
            if (Context?.Health != null)
            {
                Context.Health.OnTryDie += HandleTryDieCheatDeath;
            }

            if (Context?.CombatEvents != null)
            {
                Context.CombatEvents.OnDashPerformed += HandleDashSwitchForm;
            }
        }

        protected override void UnsubscribeCombatEvents()
        {
            if (Context?.Health != null)
            {
                Context.Health.OnTryDie -= HandleTryDieCheatDeath;
            }

            if (Context?.CombatEvents != null)
            {
                Context.CombatEvents.OnDashPerformed -= HandleDashSwitchForm;
            }
        }

        private void HandleDashSwitchForm(in DashEvent e)
        {
            if (Time.time < _lastSwitchTime + _formSwitchCooldown) return;
            _lastSwitchTime = Time.time;

            // Mỗi lần Lướt (Dash) đổi dạng giữa Rồng và Tiên
            _currentForm = _currentForm == LongTienForm.DragonOcean ? LongTienForm.FairyMother : LongTienForm.DragonOcean;
            
            // Phóng ra làn sóng Thái Cực
            Debug.Log($"<color=#FF00E5>[Long Tiên]</color> Chuyển đổi sang hình thái: {_currentForm}!");
        }

        private bool HandleTryDieCheatDeath()
        {
            // Cơ chế Miễn Tử (Cheat Death): Chặn cái chết khi Máu về 0
            if (_freeReviveCount > 0)
            {
                _freeReviveCount--;

                // Hồi phục 50% Máu tối đa và bất tử 3s
                float restoreHp = Context.Health.MaxHealth * 0.5f;
                Context.Health.SetMaxHealth(Context.Health.MaxHealth, fillCurrentHealth: false);
                Context.Health.Heal(restoreHp);
                Context.Health.TriggerInvulnerability(3.0f);

                // Phát sự kiện Hồi Sinh toàn cục
                Context.CombatEvents?.PublishPlayerRevived(new ReviveEvent(Context.GameObject, 0.5f, 3.0f, "LongTienBaoNoan"));

                Debug.Log("<color=#FF00E5>[Long Tiên] Bách Noãn Miễn Tử: Chặn cái chết thành công! Hồi 50% HP & Bất tử 3s!</color>");
                return true; // Chặn chết thành công!
            }

            return false; // Cho phép chết bình thường nếu hết lượt miễn tử
        }

        protected override void OnRevived(in ReviveEvent e)
        {
            Debug.Log("<color=#FF00E5>[Long Tiên] Thái Cực Tái Sinh: Sóng âm dương bao bọc cơ thể!</color>");
        }
    }
}
