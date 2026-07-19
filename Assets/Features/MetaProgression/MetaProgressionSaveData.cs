using System;
using UnityEngine;

namespace ProjectZombie.Features.MetaProgression
{
    /// <summary>
    /// Lưu trữ dữ liệu Meta-Progression vĩnh viễn (không mất giữa các run).
    /// Serialize được để tích hợp vào PlayerSaveData của GameManager.
    /// </summary>
    [Serializable]
    public class MetaProgressionSaveData
    {
        [Tooltip("Tổng số Coin Sinh Tồn tích lũy.")]
        public int totalCurrency = 0;

        [Tooltip("Cấp độ của từng nút trong Permanent Upgrade Tree. Index khớp với PermanentUpgradeTreeData.")]
        public int[] upgradeNodeLevels = new int[0];

        [Tooltip("Danh sách ID nhân vật đã mở khóa (luôn bao gồm nhân vật mặc định).")]
        public string[] unlockedCharacters = new string[] { "default" };

        [Tooltip("Tổng số run đã chơi (dùng cho thành tựu và analytics).")]
        public int totalRunsPlayed = 0;

        [Tooltip("Thời gian tốt nhất (giây) — dùng cho Leaderboard cá nhân.")]
        public float bestRunTime = 0f;

        [Tooltip("Số kill cao nhất trong một run.")]
        public int bestKillCount = 0;

        /// <summary>
        /// Cập nhật kỷ lục sau mỗi run.
        /// </summary>
        public void UpdateBestStats(float runTime, int killCount)
        {
            totalRunsPlayed++;
            if (runTime > bestRunTime) bestRunTime = runTime;
            if (killCount > bestKillCount) bestKillCount = killCount;
        }
    }
}
