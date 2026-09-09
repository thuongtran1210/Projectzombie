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
        [Header("Cloud & Metadata")]
        [Tooltip("Phiên bản cấu trúc dữ liệu lưu (dùng để migrate dữ liệu khi update game).")]
        public int saveVersion = 1;

        [Tooltip("Unix Timestamp thời điểm lưu lần cuối (dùng để resolve xung đột Cloud / Local).")]
        public long lastSavedTimestamp = 0;

        [Tooltip("Định danh phần cứng thiết bị chơi.")]
        public string deviceId = "";

        [Header("Currencies & Progression")]
        [Tooltip("Tổng số Cổ Tiền (tiền xu cổ Việt Nam) tích lũy.")]
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

        [Tooltip("Danh sách tiến trình thẻ mảnh & cấp sao của vũ khí / pháp bảo.")]
        public System.Collections.Generic.List<RelicProgressEntry> relicProgressList = new System.Collections.Generic.List<RelicProgressEntry>();

        [Header("Gacha Chest Progression")]
        [Tooltip("Số lượt mở rương liên tiếp chưa nhận được Thần Binh (Legendary Pity Counter).")]
        public int gachaPityLegendary = 0;

        [Tooltip("Số lượt mở rương liên tiếp chưa nhận được Cực Phẩm (Epic Pity Counter).")]
        public int gachaPityEpic = 0;

        [Tooltip("Tổng số lượt quay Gacha đã thực hiện.")]
        public int totalGachaRolls = 0;

        /// <summary>
        /// Cập nhật kỷ lục sau mỗi run.
        /// </summary>
        public void UpdateBestStats(float runTime, int killCount)
        {
            totalRunsPlayed++;
            if (runTime > bestRunTime) bestRunTime = runTime;
            if (killCount > bestKillCount) bestKillCount = killCount;
        }

        public int GetRelicShards(string relicId)
        {
            if (string.IsNullOrEmpty(relicId) || relicProgressList == null) return 0;
            var entry = relicProgressList.Find(x => x.relicId == relicId);
            return entry.relicId != null ? entry.shardCount : 0;
        }

        public int GetRelicStarLevel(string relicId)
        {
            if (string.IsNullOrEmpty(relicId) || relicProgressList == null) return 0;
            var entry = relicProgressList.Find(x => x.relicId == relicId);
            return entry.relicId != null ? entry.starLevel : 0;
        }

        public void SetRelicProgress(string relicId, int shardCount, int starLevel)
        {
            if (string.IsNullOrEmpty(relicId)) return;
            if (relicProgressList == null) relicProgressList = new System.Collections.Generic.List<RelicProgressEntry>();

            int idx = relicProgressList.FindIndex(x => x.relicId == relicId);
            if (idx >= 0)
            {
                var entry = relicProgressList[idx];
                entry.shardCount = Mathf.Max(0, shardCount);
                entry.starLevel = Mathf.Clamp(starLevel, 0, 5);
                relicProgressList[idx] = entry;
            }
            else
            {
                relicProgressList.Add(new RelicProgressEntry
                {
                    relicId = relicId,
                    shardCount = Mathf.Max(0, shardCount),
                    starLevel = Mathf.Clamp(starLevel, 0, 5)
                });
            }
        }

        public int GetUpgradeLevel(int nodeIndex)
        {
            if (upgradeNodeLevels == null || nodeIndex < 0 || nodeIndex >= upgradeNodeLevels.Length) return 0;
            return upgradeNodeLevels[nodeIndex];
        }

        public void SetUpgradeLevel(int nodeIndex, int level)
        {
            if (nodeIndex < 0) return;
            if (upgradeNodeLevels == null || upgradeNodeLevels.Length <= nodeIndex)
            {
                int newSize = Mathf.Max(nodeIndex + 1, (upgradeNodeLevels?.Length ?? 0) * 2);
                int[] newArray = new int[newSize];
                if (upgradeNodeLevels != null)
                {
                    Array.Copy(upgradeNodeLevels, newArray, upgradeNodeLevels.Length);
                }
                upgradeNodeLevels = newArray;
            }
            upgradeNodeLevels[nodeIndex] = level;
        }
    }

    /// <summary>
    /// Bản ghi lưu trữ số thẻ mảnh (Shards) và cấp sao (0..5★) của một vũ khí / pháp bảo.
    /// </summary>
    [Serializable]
    public struct RelicProgressEntry
    {
        public string relicId;
        public int shardCount;
        public int starLevel;
    }
}
