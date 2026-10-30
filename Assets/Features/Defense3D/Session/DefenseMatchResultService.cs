using System;
using UnityEngine;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Core.Save;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Dữ liệu đầu vào của kết quả trận đấu Defense 3D.
    /// </summary>
    public struct DefenseMatchResultData
    {
        public bool IsWon;
        public float ElapsedSeconds;
        public int WoodCollected;
        public int EnemiesDefeated;
        public string StageId;
        public string RunId;
        public int VictoryReward;
        public float BaseRemainingHpRatio;
        public int FirstClearBonusCurrency;
        public string FirstClearRelicId;
        public int FirstClearRelicShards;

        public DefenseMatchResultData(
            bool isWon,
            float elapsedSeconds,
            int woodCollected,
            int enemiesDefeated = 0,
            string stageId = "STAGE_01",
            string runId = null,
            int victoryReward = 500,
            float baseRemainingHpRatio = 1f,
            int firstClearBonusCurrency = 0,
            string firstClearRelicId = null,
            int firstClearRelicShards = 0)
        {
            IsWon = isWon;
            ElapsedSeconds = Mathf.Max(0f, elapsedSeconds);
            WoodCollected = Mathf.Max(0, woodCollected);
            EnemiesDefeated = Mathf.Max(0, enemiesDefeated);
            StageId = string.IsNullOrWhiteSpace(stageId) ? "STAGE_01" : stageId.Trim();
            RunId = runId;
            VictoryReward = Mathf.Max(0, victoryReward);
            BaseRemainingHpRatio = isWon ? Mathf.Clamp(baseRemainingHpRatio, 0.01f, 1f) : Mathf.Clamp01(baseRemainingHpRatio);
            FirstClearBonusCurrency = Mathf.Max(0, firstClearBonusCurrency);
            FirstClearRelicId = firstClearRelicId;
            FirstClearRelicShards = Mathf.Max(0, firstClearRelicShards);
        }
    }

    /// <summary>
    /// Dữ liệu đã xử lý và tính toán phần thưởng sau trận.
    /// </summary>
    public sealed class DefenseProcessedResult
    {
        public bool IsWon { get; }
        public float ElapsedSeconds { get; }
        public int WoodCollected { get; }
        public int EnemiesDefeated { get; }
        public int EarnedCurrency { get; }
        public int EarnedStars { get; }
        public bool IsFirstClear { get; }
        public int FirstClearBonusCurrency { get; }
        public int FirstClearBonusShards { get; }
        public string FormattedTime { get; }
        public string Title { get; }
        public string Message { get; }
        public string Summary { get; }

        public DefenseProcessedResult(
            bool isWon,
            float elapsedSeconds,
            int woodCollected,
            int enemiesDefeated,
            int earnedCurrency,
            string title,
            string message,
            string summary,
            int earnedStars = 0,
            bool isFirstClear = false,
            int firstClearBonusCurrency = 0,
            int firstClearBonusShards = 0)
        {
            IsWon = isWon;
            ElapsedSeconds = elapsedSeconds;
            WoodCollected = woodCollected;
            EnemiesDefeated = enemiesDefeated;
            EarnedCurrency = earnedCurrency;
            EarnedStars = earnedStars;
            IsFirstClear = isFirstClear;
            FirstClearBonusCurrency = firstClearBonusCurrency;
            FirstClearBonusShards = firstClearBonusShards;
            int minutes = Mathf.FloorToInt(elapsedSeconds / 60f);
            int seconds = Mathf.FloorToInt(elapsedSeconds % 60f);
            FormattedTime = $"{minutes:00}:{seconds:00}";
            Title = title;
            Message = message;
            Summary = summary;
        }
    }

    /// <summary>
    /// Dịch vụ nghiệp vụ tính toán kết quả trận đấu, trao thưởng và cập nhật tiến trình vĩnh viễn (Save/Meta).
    /// Tuân thủ quy chuẩn G5:
    /// 1. UI kết quả KHÔNG sở hữu việc tính thưởng; mọi tính toán thuộc về Service độc lập.
    /// 2. Cơ chế chống cấp thưởng hai lần (Idempotency Guard).
    /// 3. Chính sách thưởng rõ ràng: Thưởng theo lượng gỗ thu thập + Thưởng thắng trận cố định + Thưởng vượt ải lần đầu.
    /// 4. Đánh giá Sao vượt ải (1★ - 3★) theo tỷ lệ máu Căn Cứ còn lại và cập nhật vào MetaProgressionSaveData.
    /// </summary>
    public sealed class DefenseMatchResultService
    {
        public const int REWARD_VICTORY_CURRENCY = 500;
        public const float REWARD_WOOD_TO_CURRENCY_RATIO = 2.0f;

        public static Action<string> LogWarningHook { get; set; }
        public static Action<string> LogErrorHook { get; set; }

        private static void LogWarning(string msg)
        {
            if (LogWarningHook != null) LogWarningHook(msg);
            else Debug.LogWarning(msg);
        }

        private static void LogError(string msg)
        {
            if (LogErrorHook != null) LogErrorHook(msg);
            else Debug.LogError(msg);
        }

        private bool _hasProcessedResult;
        private DefenseProcessedResult _lastProcessedResult;

        /// <summary>
        /// Cho biết kết quả của trận đấu hiện tại đã được tính thưởng hay chưa.
        /// </summary>
        public bool HasProcessedResult => _hasProcessedResult;

        /// <summary>
        /// Kết quả xử lý gần nhất trong phiên.
        /// </summary>
        public DefenseProcessedResult LastProcessedResult => _lastProcessedResult;

        /// <summary>
        /// Tính số Sao đạt được (1..3) dựa trên tỷ lệ máu Căn Cứ còn lại khi chiến thắng:
        /// - >= 80% máu Căn Cứ: 3★ (Thủ Thành Hoàn Hảo)
        /// - >= 40% máu Căn Cứ: 2★ (Thủ Thành Vững Chắc)
        /// - > 0% máu Căn Cứ: 1★ (Thủ Thành Thành Công)
        /// </summary>
        public static int EvaluateStageStars(bool isWon, float baseRemainingHpRatio)
        {
            if (!isWon || baseRemainingHpRatio <= 0f) return 0;
            if (baseRemainingHpRatio >= 0.8f) return 3;
            if (baseRemainingHpRatio >= 0.4f) return 2;
            return 1;
        }

        /// <summary>
        /// Xử lý kết quả trận đấu, tính thưởng Cổ Tiền và cập nhật tiến trình vào SaveData.
        /// Chống cấp thưởng 2 lần: Nếu đã xử lý, trả về kết quả đã tính trước đó.
        /// </summary>
        public DefenseProcessedResult ProcessMatchResult(
            DefenseMatchResultData resultData,
            MetaProgressionSaveData customSaveData = null,
            Action<MetaProgressionSaveData> customSaveAction = null)
        {
            // Idempotency Guard: Nếu đã xử lý rồi, trả về kết quả trước đó để không cấp thưởng lần 2
            if (_hasProcessedResult && _lastProcessedResult != null)
            {
                LogWarning("[DefenseMatchResultService] Kết quả trận này đã được xử lý trước đó. Bỏ qua để chống trùng lặp.");
                return _lastProcessedResult;
            }

            MetaProgressionSaveData saveData = null;
            try
            {
                saveData = customSaveData ?? SaveSystem.Load();
            }
            catch (Exception ex)
            {
                LogError($"[DefenseMatchResultService] Không thể tải dữ liệu lưu trước khi tính thưởng: {ex.Message}");
            }

            bool isDuplicateRunId = false;
            if (saveData != null && !string.IsNullOrEmpty(resultData.RunId))
            {
                if (saveData.rewardedRunIds == null) saveData.rewardedRunIds = new System.Collections.Generic.List<string>();
                isDuplicateRunId = saveData.rewardedRunIds.Contains(resultData.RunId);
            }

            bool isFirstClear = resultData.IsWon && saveData != null && !saveData.IsStageCompleted(resultData.StageId);
            int earnedStars = EvaluateStageStars(resultData.IsWon, resultData.BaseRemainingHpRatio);

            // 1. Tính toán phần thưởng
            int woodReward = Mathf.RoundToInt(resultData.WoodCollected * REWARD_WOOD_TO_CURRENCY_RATIO);
            int victoryBonus = resultData.IsWon ? resultData.VictoryReward : 0;
            int firstClearCurrency = isFirstClear ? resultData.FirstClearBonusCurrency : 0;
            int firstClearShards = isFirstClear && !string.IsNullOrEmpty(resultData.FirstClearRelicId) ? resultData.FirstClearRelicShards : 0;
            int totalEarnedCurrency = woodReward + victoryBonus + firstClearCurrency;

            // 2. Định dạng thông điệp hiển thị
            string starBadge = earnedStars > 0
                ? $" [{new string('★', earnedStars)}{new string('☆', 3 - earnedStars)}]"
                : string.Empty;

            string title = resultData.IsWon
                ? $"<color=#FFD700>CHIẾN THẮNG QUANG VINH{starBadge}</color>"
                : "<color=#FF4444>CĂN CỨ THẤT THỦ</color>";

            string message = resultData.IsWon
                ? "Đã hoàn thành nhiệm vụ phòng thủ và đánh bại thủ lĩnh!"
                : "Căn cứ đã thất thủ. Hãy chuẩn bị đội hình và thử lại.";

            int minutes = Mathf.FloorToInt(resultData.ElapsedSeconds / 60f);
            int seconds = Mathf.FloorToInt(resultData.ElapsedSeconds % 60f);
            string summary = $"Thời gian: {minutes:00}:{seconds:00} | Gỗ: {resultData.WoodCollected} | Thưởng Cổ Tiền: +{totalEarnedCurrency:N0}";
            if (isFirstClear && (firstClearCurrency > 0 || firstClearShards > 0))
            {
                string shardNote = firstClearShards > 0 ? $" +{firstClearShards} Mảnh Tín Vật" : string.Empty;
                summary += $" <color=#00FF88>[Thưởng Lần Đầu{shardNote}]</color>";
            }

            _lastProcessedResult = new DefenseProcessedResult(
                resultData.IsWon,
                resultData.ElapsedSeconds,
                resultData.WoodCollected,
                resultData.EnemiesDefeated,
                totalEarnedCurrency,
                title,
                message,
                summary,
                earnedStars,
                isFirstClear,
                firstClearCurrency,
                firstClearShards);

            _hasProcessedResult = true;

            // 3. Cập nhật vào MetaProgressionSaveData
            try
            {
                if (saveData != null && !isDuplicateRunId)
                {
                    if (!string.IsNullOrEmpty(resultData.RunId))
                    {
                        saveData.rewardedRunIds.Add(resultData.RunId);
                        if (saveData.rewardedRunIds.Count > 128) saveData.rewardedRunIds.RemoveAt(0);
                    }

                    // Cộng tiền thưởng
                    saveData.totalCurrency += totalEarnedCurrency;

                    // Cộng mảnh Tín Vật thưởng vượt ải lần đầu
                    if (isFirstClear && firstClearShards > 0 && !string.IsNullOrEmpty(resultData.FirstClearRelicId))
                    {
                        int curShards = saveData.GetRelicShards(resultData.FirstClearRelicId);
                        int curStar = Mathf.Max(1, saveData.GetRelicStarLevel(resultData.FirstClearRelicId));
                        saveData.SetRelicProgress(resultData.FirstClearRelicId, curShards + firstClearShards, curStar);
                    }

                    // Cập nhật kỷ lục cá nhân
                    saveData.UpdateBestStats(resultData.ElapsedSeconds, resultData.EnemiesDefeated);

                    // Đánh dấu hoàn thành Ải và số Sao đạt được nếu chiến thắng
                    if (resultData.IsWon)
                    {
                        saveData.MarkStageCompleted(resultData.StageId, resultData.ElapsedSeconds, earnedStars);
                    }

                    // Lưu dữ liệu
                    if (customSaveAction != null)
                    {
                        customSaveAction(saveData);
                    }
                    else
                    {
                        SaveSystem.Save(saveData);

                        // Nạp lại và đồng bộ tất cả module (bao gồm MetaCurrencyManager) nếu GameManager đang chạy
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.LoadGame();
                        }
                        else if (MetaCurrencyManager.Instance != null)
                        {
                            MetaCurrencyManager.Instance.Initialize(saveData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"[DefenseMatchResultService] Lỗi khi lưu kết quả trận đấu: {ex.Message}");
            }

            return _lastProcessedResult;
        }

        /// <summary>
        /// Khởi tạo lại trạng thái dịch vụ khi bắt đầu lại trận đấu (Restart).
        /// </summary>
        public void Reset()
        {
            _hasProcessedResult = false;
            _lastProcessedResult = null;
        }
    }
}
