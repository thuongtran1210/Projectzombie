using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Defense3D.Loadout;

namespace ProjectZombie.Features.MetaProgression
{
    /// <summary>
    /// Lưu trữ dữ liệu Meta-Progression vĩnh viễn (không mất giữa các run).
    /// Serialize được để tích hợp vào PlayerSaveData của GameManager.
    /// Tự động chuẩn hóa dữ liệu save cũ (legacy 2D IDs) sang chuẩn Defense 3D thông qua SanitizeLegacyData().
    /// </summary>
    [Serializable]
    public class MetaProgressionSaveData
    {
        public const int CURRENT_SAVE_VERSION = 2;

        [Header("Cloud & Metadata")]
        [Tooltip("Phiên bản cấu trúc dữ liệu lưu (dùng để migrate dữ liệu khi update game).")]
        public int saveVersion = CURRENT_SAVE_VERSION;
        public List<string> rewardedRunIds = new List<string>();
        public List<string> unlockedTowerIds = new List<string>();

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
        public string[] unlockedCharacters = new string[] { "C001_ThuSinh" };

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
        public List<string> completedStages = new List<string>();

        [Tooltip("Danh sách thành tích kỷ lục của từng Ải (Thời gian tốt nhất, số sao đạt được).")]
        public List<StageRecordEntry> stageRecords = new List<StageRecordEntry>();

        [Tooltip("Danh sách tiến trình thẻ mảnh & cấp sao của vũ khí / pháp bảo.")]
        public List<RelicProgressEntry> relicProgressList = new List<RelicProgressEntry>();

        [Header("Gacha Chest Progression")]
        [Tooltip("Số lượt mở rương liên tiếp chưa nhận được Thần Binh (Legendary Pity Counter).")]
        public int gachaPityLegendary = 0;

        [Tooltip("Số lượt mở rương liên tiếp chưa nhận được Cực Phẩm (Epic Pity Counter).")]
        public int gachaPityEpic = 0;

        [Tooltip("Tổng số lượt quay Gacha đã thực hiện.")]
        public int totalGachaRolls = 0;

        [Tooltip("Danh sách tiến trình thẻ mảnh & cấp sao của tướng / nhân vật.")]
        public List<CharacterProgressEntry> characterProgressList = new List<CharacterProgressEntry>();

        /// <summary>
        /// Chuẩn hóa và dọn dẹp dữ liệu local save cũ từ hệ thống 2D sang hệ thống Defense 3D hiện hành.
        /// Không bao giờ ném ngoại lệ khi load file save cũ hoặc thiếu trường.
        /// </summary>
        public bool SanitizeLegacyData()
        {
            bool modified = false;

            if (rewardedRunIds == null) { rewardedRunIds = new List<string>(); modified = true; }
            if (unlockedTowerIds == null) { unlockedTowerIds = new List<string>(); modified = true; }
            if (upgradeNodeLevels == null) { upgradeNodeLevels = new int[0]; modified = true; }
            if (completedStages == null) { completedStages = new List<string>(); modified = true; }
            if (stageRecords == null) { stageRecords = new List<StageRecordEntry>(); modified = true; }
            if (relicProgressList == null) { relicProgressList = new List<RelicProgressEntry>(); modified = true; }
            if (characterProgressList == null) { characterProgressList = new List<CharacterProgressEntry>(); modified = true; }

            // 1. Chuẩn hóa selectedHeroId
            if (string.IsNullOrWhiteSpace(selectedHeroId) || !DefenseLoadoutIdMapper.TryMapHeroId(selectedHeroId, out string mappedSelected))
            {
                selectedHeroId = "C001_ThuSinh";
                modified = true;
            }
            else
            {
                string canonicalMetaHero = DefenseLoadoutIdMapper.GetMetaCharacterId(mappedSelected) ?? "C001_ThuSinh";
                if (!string.Equals(selectedHeroId, canonicalMetaHero, StringComparison.Ordinal))
                {
                    selectedHeroId = canonicalMetaHero;
                    modified = true;
                }
            }

            // 2. Chuẩn hóa unlockedCharacters
            var cleanUnlockedChars = new List<string>();
            var seenUnlockedChars = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (unlockedCharacters != null)
            {
                for (int i = 0; i < unlockedCharacters.Length; i++)
                {
                    string raw = unlockedCharacters[i];
                    if (!string.IsNullOrWhiteSpace(raw) && DefenseLoadoutIdMapper.TryMapHeroId(raw, out string mappedHero))
                    {
                        string metaId = DefenseLoadoutIdMapper.GetMetaCharacterId(mappedHero);
                        if (!string.IsNullOrEmpty(metaId) && seenUnlockedChars.Add(metaId))
                        {
                            cleanUnlockedChars.Add(metaId);
                        }
                    }
                }
            }
            if (seenUnlockedChars.Add("C001_ThuSinh"))
            {
                cleanUnlockedChars.Add("C001_ThuSinh");
            }
            if (unlockedCharacters == null || unlockedCharacters.Length != cleanUnlockedChars.Count)
            {
                unlockedCharacters = cleanUnlockedChars.ToArray();
                modified = true;
            }

            // 3. Chuẩn hóa characterProgressList (gộp bí danh cũ và loại bỏ ID rác 2D)
            var mergedCharMap = new Dictionary<string, CharacterProgressEntry>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < characterProgressList.Count; i++)
            {
                var entry = characterProgressList[i];
                if (string.IsNullOrWhiteSpace(entry.characterId))
                {
                    modified = true;
                    continue;
                }

                if (!DefenseLoadoutIdMapper.TryMapHeroId(entry.characterId, out string mappedHero))
                {
                    modified = true;
                    continue;
                }

                string canonicalId = DefenseLoadoutIdMapper.GetMetaCharacterId(mappedHero);
                if (string.IsNullOrEmpty(canonicalId))
                {
                    modified = true;
                    continue;
                }

                if (!string.Equals(entry.characterId, canonicalId, StringComparison.Ordinal))
                {
                    modified = true;
                }

                if (mergedCharMap.TryGetValue(canonicalId, out var existing))
                {
                    existing.shardCount = Mathf.Max(0, existing.shardCount + Mathf.Max(0, entry.shardCount));
                    existing.starLevel = Mathf.Clamp(Mathf.Max(existing.starLevel, entry.starLevel), 0, 5);
                    mergedCharMap[canonicalId] = existing;
                    modified = true;
                }
                else
                {
                    mergedCharMap[canonicalId] = new CharacterProgressEntry
                    {
                        characterId = canonicalId,
                        shardCount = Mathf.Max(0, entry.shardCount),
                        starLevel = Mathf.Clamp(entry.starLevel, 0, 5)
                    };
                }
            }

            if (modified || characterProgressList.Count != mergedCharMap.Count)
            {
                characterProgressList = new List<CharacterProgressEntry>(mergedCharMap.Values);
            }

            // 4. Chuẩn hóa relicProgressList (loại bỏ W001..W012, W_PIPE, W_POT, R007, R008 của 2D cũ; gộp bí danh Dép Tổ Ong)
            var catalog = Defense3DCatalog.CreateDefault();
            var mergedRelicMap = new Dictionary<string, RelicProgressEntry>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < relicProgressList.Count; i++)
            {
                var entry = relicProgressList[i];
                if (string.IsNullOrWhiteSpace(entry.relicId))
                {
                    modified = true;
                    continue;
                }

                string cleanId = entry.relicId.Trim();
                string canonicalRelicId = null;

                if (DefenseLoadoutIdMapper.TryMapRelicId(cleanId, out string mappedRelic))
                {
                    canonicalRelicId = mappedRelic;
                }
                else if (catalog.TryGetEntry(cleanId, out var catEntry) && catEntry.Type != LoadoutItemType.Hero)
                {
                    canonicalRelicId = catEntry.Id;
                }
                else if (string.Equals(cleanId, "wp_kiem_truc", StringComparison.OrdinalIgnoreCase))
                {
                    canonicalRelicId = "wp_kiem_truc";
                }

                if (string.IsNullOrEmpty(canonicalRelicId))
                {
                    // Bỏ qua vũ khí/pháp bảo 2D cũ (W001..W012, W_PIPE, R007, R008...)
                    modified = true;
                    continue;
                }

                if (!string.Equals(entry.relicId, canonicalRelicId, StringComparison.Ordinal))
                {
                    modified = true;
                }

                if (mergedRelicMap.TryGetValue(canonicalRelicId, out var existing))
                {
                    existing.shardCount = Mathf.Max(0, existing.shardCount + Mathf.Max(0, entry.shardCount));
                    existing.starLevel = Mathf.Clamp(Mathf.Max(existing.starLevel, entry.starLevel), 0, 5);
                    mergedRelicMap[canonicalRelicId] = existing;
                    modified = true;
                }
                else
                {
                    mergedRelicMap[canonicalRelicId] = new RelicProgressEntry
                    {
                        relicId = canonicalRelicId,
                        shardCount = Mathf.Max(0, entry.shardCount),
                        starLevel = Mathf.Clamp(entry.starLevel, 0, 5)
                    };
                }
            }

            if (modified || relicProgressList.Count != mergedRelicMap.Count)
            {
                relicProgressList = new List<RelicProgressEntry>(mergedRelicMap.Values);
            }

            if (saveVersion < CURRENT_SAVE_VERSION)
            {
                saveVersion = CURRENT_SAVE_VERSION;
                modified = true;
            }

            return modified;
        }

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
            var entry = relicProgressList.Find(x => string.Equals(x.relicId, relicId, StringComparison.OrdinalIgnoreCase));
            return entry.relicId != null ? entry.shardCount : 0;
        }

        public int GetRelicStarLevel(string relicId)
        {
            if (string.IsNullOrEmpty(relicId) || relicProgressList == null) return 0;
            var entry = relicProgressList.Find(x => string.Equals(x.relicId, relicId, StringComparison.OrdinalIgnoreCase));
            return entry.relicId != null ? entry.starLevel : 0;
        }

        public void SetRelicProgress(string relicId, int shardCount, int starLevel)
        {
            if (string.IsNullOrEmpty(relicId)) return;
            if (relicProgressList == null) relicProgressList = new List<RelicProgressEntry>();

            int idx = relicProgressList.FindIndex(x => string.Equals(x.relicId, relicId, StringComparison.OrdinalIgnoreCase));
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

        public int GetCharacterShards(string characterId)
        {
            if (string.IsNullOrEmpty(characterId) || characterProgressList == null) return 0;
            var entry = characterProgressList.Find(x => string.Equals(x.characterId, characterId, StringComparison.OrdinalIgnoreCase));
            return entry.characterId != null ? entry.shardCount : 0;
        }

        public int GetCharacterStarLevel(string characterId)
        {
            if (string.IsNullOrEmpty(characterId) || characterProgressList == null) return 0;
            var entry = characterProgressList.Find(x => string.Equals(x.characterId, characterId, StringComparison.OrdinalIgnoreCase));
            return entry.characterId != null ? entry.starLevel : 0;
        }

        public void SetCharacterProgress(string characterId, int shardCount, int starLevel)
        {
            if (string.IsNullOrEmpty(characterId)) return;
            if (characterProgressList == null) characterProgressList = new List<CharacterProgressEntry>();

            int idx = characterProgressList.FindIndex(x => string.Equals(x.characterId, characterId, StringComparison.OrdinalIgnoreCase));
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
            if (completedStages == null) completedStages = new List<string>();
            if (!completedStages.Contains(stageId))
            {
                completedStages.Add(stageId);
            }

            if (stageRecords == null) stageRecords = new List<StageRecordEntry>();
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
