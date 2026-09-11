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
        public string[] unlockedCharacters = new string[] { "default", "C001_ThuSinh" };

        [Tooltip("ID của Anh Hùng đang được chọn sử dụng.")]
        public string selectedHeroId = "C001_ThuSinh";

        [Tooltip("Tổng số run đã chơi (dùng cho thành tựu và analytics).")]
        public int totalRunsPlayed = 0;

        [Tooltip("Thời gian tốt nhất (giây) — dùng cho Leaderboard cá nhân.")]
        public float bestRunTime = 0f;

        [Tooltip("Số kill cao nhất trong một run.")]
        public int bestKillCount = 0;

        [Header("Stage Progression")]
        [Tooltip("Danh sách ID các Ải đã hoàn thành (Ví dụ: 'STAGE_01', 'STAGE_02').")]
        public System.Collections.Generic.List<string> completedStages = new System.Collections.Generic.List<string>();

        [Tooltip("Danh sách thành tích kỷ lục của từng Ải (Thời gian tốt nhất, số sao đạt được).")]
        public System.Collections.Generic.List<StageRecordEntry> stageRecords = new System.Collections.Generic.List<StageRecordEntry>();

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

        [Tooltip("Danh sách tiến trình thẻ mảnh & cấp sao của tướng / nhân vật.")]
        public System.Collections.Generic.List<CharacterProgressEntry> characterProgressList = new System.Collections.Generic.List<CharacterProgressEntry>();

        public int GetCharacterShards(string characterId)
        {
            if (string.IsNullOrEmpty(characterId) || characterProgressList == null) return 0;
            var entry = characterProgressList.Find(x => x.characterId == characterId);
            return entry.characterId != null ? entry.shardCount : 0;
        }

        public int GetCharacterStarLevel(string characterId)
        {
            if (string.IsNullOrEmpty(characterId) || characterProgressList == null) return 0;
            var entry = characterProgressList.Find(x => x.characterId == characterId);
            return entry.characterId != null ? entry.starLevel : 0;
        }

        public void SetCharacterProgress(string characterId, int shardCount, int starLevel)
        {
            if (string.IsNullOrEmpty(characterId)) return;
            if (characterProgressList == null) characterProgressList = new System.Collections.Generic.List<CharacterProgressEntry>();

            int idx = characterProgressList.FindIndex(x => x.characterId == characterId);
            if (idx >= 0)
            {
                var entry = characterProgressList[idx];
                entry.shardCount = Mathf.Max(0, shardCount);
                entry.starLevel = Mathf.Clamp(starLevel, 0, 5);
                characterProgressList[idx] = entry;
            }
            else
            {
                characterProgressList.Add(new CharacterProgressEntry
                {
                    characterId = characterId,
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

        public bool IsStageCompleted(string stageId)
        {
            if (string.IsNullOrEmpty(stageId) || completedStages == null) return false;
            return completedStages.Contains(stageId);
        }

        public void MarkStageCompleted(string stageId, float clearTime, int stars = 3)
        {
            if (string.IsNullOrEmpty(stageId)) return;
            if (completedStages == null) completedStages = new System.Collections.Generic.List<string>();
            if (!completedStages.Contains(stageId))
            {
                completedStages.Add(stageId);
            }

            if (stageRecords == null) stageRecords = new System.Collections.Generic.List<StageRecordEntry>();
            int idx = stageRecords.FindIndex(x => x.stageId == stageId);
            if (idx >= 0)
            {
                var entry = stageRecords[idx];
                entry.isCompleted = true;
                if (clearTime < entry.bestClearTime || entry.bestClearTime <= 0) entry.bestClearTime = clearTime;
                if (stars > entry.starRating) entry.starRating = Mathf.Clamp(stars, 1, 3);
                stageRecords[idx] = entry;
            }
            else
            {
                stageRecords.Add(new StageRecordEntry
                {
                    stageId = stageId,
                    isCompleted = true,
                    bestClearTime = clearTime,
                    starRating = Mathf.Clamp(stars, 1, 3)
                });
            }
        }

        public StageRecordEntry GetStageRecord(string stageId)
        {
            if (string.IsNullOrEmpty(stageId) || stageRecords == null) return default;
            var entry = stageRecords.Find(x => x.stageId == stageId);
            return entry;
        }
    }

    /// <summary>
    /// Bản ghi lưu trữ thành tích vượt Ải của người chơi.
    /// </summary>
    [Serializable]
    public struct StageRecordEntry
    {
        public string stageId;
        public bool isCompleted;
        public float bestClearTime;
        public int starRating; // 1..3 sao
    }

    /// <summary>
    /// Bản ghi lưu trữ số thẻ mảnh (Shards) và cấp sao (0..5★) của một tướng / nhân vật.
    /// </summary>
    [Serializable]
    public struct CharacterProgressEntry
    {
        public string characterId;
        public int shardCount;
        public int starLevel;
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
