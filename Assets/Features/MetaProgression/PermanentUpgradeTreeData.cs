using UnityEngine;
using System;

namespace ProjectZombie.Features.MetaProgression
{
    public enum SanctuaryBranch
    {
        TanVienSonThanh,   // ⚔️ Thiên về Tấn Công (Công, Bạo Kích, Tốc Đánh)
        PhuDongThienVuong, // 🛡️ Thiên về Phòng Thủ (Máu, Dash, Tốc Chạy)
        LieuHanhChuDongTu  // 📜 Bổ Trợ & Tài Phú (Hút Tiền, Tăng EXP, Reroll)
    }

    /// <summary>
    /// Mô tả một nút trong cây nâng cấp vĩnh viễn.
    /// </summary>
    [Serializable]
    public class PermanentUpgradeNode
    {
        [Tooltip("Nhánh thần linh bảo hộ của nút nâng cấp này.")]
        public SanctuaryBranch branch = SanctuaryBranch.TanVienSonThanh;

        [Tooltip("ID duy nhất của nút nâng cấp này (dùng để lưu cấp độ vào SaveData).")]
        public string nodeId;

        [Tooltip("Tên hiển thị trên UI Shop.")]
        public string displayName;

        [TextArea(2, 4)]
        [Tooltip("Mô tả hiệu ứng.")]
        public string description;

        [Tooltip("Icon đại diện cho nút nâng cấp này.")]
        public Sprite icon;

        [Tooltip("Số cấp tối đa có thể nâng.")]
        public int maxLevel = 5;

        [Tooltip("Chi phí Coin Sinh Tồn cho mỗi cấp. Phần tử 0 = chi phí cấp 1, v.v.")]
        public int[] costPerLevel = { 10, 20, 40, 80, 150 };

        [Tooltip("Chỉ số Player được cộng mỗi cấp.")]
        public ProjectZombie.Features.Upgrades.PlayerStatModifier statBonusPerLevel;

        /// <summary>Lấy chi phí cho cấp tiếp theo. -1 nếu đã đạt max.</summary>
        public int GetCostForLevel(int currentLevel)
        {
            if (currentLevel >= maxLevel) return -1;
            if (currentLevel < costPerLevel.Length)
                return costPerLevel[currentLevel];
            // Fallback: tăng mũ nếu thiếu entry
            return costPerLevel[costPerLevel.Length - 1] * (currentLevel - costPerLevel.Length + 2);
        }
    }

    /// <summary>
    /// ScriptableObject chứa toàn bộ cây nâng cấp vĩnh viễn (Permanent Upgrade Tree).
    /// Designer tạo một SO duy nhất này và điền các node vào.
    /// PermanentUpgradeShopUI đọc SO này để render giao diện Shop.
    /// </summary>
    [CreateAssetMenu(fileName = "PermanentUpgradeTree", menuName = "ProjectZombie/Meta/Permanent Upgrade Tree")]
    public class PermanentUpgradeTreeData : ScriptableObject
    {
        [Header("Upgrade Nodes")]
        public PermanentUpgradeNode[] nodes;

        /// <summary>Tìm node theo ID.</summary>
        public PermanentUpgradeNode GetNodeById(string nodeId)
        {
            if (nodes == null) return null;
            foreach (var node in nodes)
            {
                if (node.nodeId == nodeId) return node;
            }
            return null;
        }

        /// <summary>Lấy index của node trong array.</summary>
        public int GetNodeIndex(string nodeId)
        {
            if (nodes == null) return -1;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].nodeId == nodeId) return i;
            }
            return -1;
        }
    }
}
