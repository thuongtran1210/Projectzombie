using System;
using UnityEngine;
using ProjectZombie.Features.Upgrades;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Component quản lý vòng đời của Lõi Thần Thoại trên nhân vật Player.
    /// Điều phối việc Equip/Replace Core, Teardown sạch sẽ và bắn sự kiện ra ngoài.
    /// </summary>
    public class PlayerMythicManager : MonoBehaviour
    {
        public MythicArchetype CurrentArchetype { get; private set; } = MythicArchetype.None;
        public MythicCoreRuntime ActiveRuntime { get; private set; }

        public event Action<MythicArchetype> OnMythicArchetypeEquipped;

        private PlayerContext _context;

        public void Construct(PlayerContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Trang bị một Lõi Thần Thoại mới, tự động hủy Lõi cũ an toàn.
        /// </summary>
        public void EquipMythicCore(MythicCoreUpgradeData coreData)
        {
            if (coreData == null || coreData.runtimePrefab == null) return;

            // 1. Dọn dẹp Core cũ nếu có
            if (ActiveRuntime != null)
            {
                ActiveRuntime.Teardown();
                Destroy(ActiveRuntime.gameObject);
                ActiveRuntime = null;
            }

            // 2. Khởi tạo Core mới
            CurrentArchetype = coreData.archetype;
            ActiveRuntime = Instantiate(coreData.runtimePrefab, transform);
            ActiveRuntime.Initialize(_context);

            // 3. Bắn event thông báo
            OnMythicArchetypeEquipped?.Invoke(CurrentArchetype);
            Debug.Log($"<color=#FFD700>[PlayerMythicManager] Đã kích hoạt Đại Lõi: {coreData.mythicTitle} ({CurrentArchetype})</color>");
        }

        /// <summary>
        /// Reset trạng thái khi bắt đầu ván mới hoặc thoát trận.
        /// </summary>
        public void ResetState()
        {
            if (ActiveRuntime != null)
            {
                ActiveRuntime.Teardown();
                Destroy(ActiveRuntime.gameObject);
                ActiveRuntime = null;
            }
            CurrentArchetype = MythicArchetype.None;
        }

        private void OnDestroy()
        {
            ResetState();
        }
    }
}
