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

        public DefenseMatchResultData(bool isWon, float elapsedSeconds, int woodCollected, int enemiesDefeated = 0, string stageId = "STAGE_01")
        {
            IsWon = isWon;
            ElapsedSeconds = Mathf.Max(0f, elapsedSeconds);
            WoodCollected = Mathf.Max(0, woodCollected);
            EnemiesDefeated = Mathf.Max(0, enemiesDefeated);
            StageId = string.IsNullOrWhiteSpace(stageId) ? "STAGE_01" : stageId.Trim();
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
            string summary)
        {
            IsWon = isWon;
            ElapsedSeconds = elapsedSeconds;
            WoodCollected = woodCollected;
            EnemiesDefeated = enemiesDefeated;
            EarnedCurrency = earnedCurrency;
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
    /// 3. Chính sách thưởng rõ ràng: Thưởng theo lượng gỗ thu thập + Thưởng thắng trận cố định.
    /// 4. Cập nhật thành tích kỷ lục và hoàn thành Ải vào MetaProgressionSaveData.
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

            // 1. Tính toán phần thưởng
            int woodReward = Mathf.RoundToInt(resultData.WoodCollected * REWARD_WOOD_TO_CURRENCY_RATIO);
            int victoryBonus = resultData.IsWon ? REWARD_VICTORY_CURRENCY : 0;
            int totalEarnedCurrency = woodReward + victoryBonus;

            // 2. Định dạng thông điệp hiển thị
            string title = resultData.IsWon
                ? "<color=#FFD700>CHIẾN THẮNG QUANG VINH</color>"
                : "<color=#FF4444>CĂN CỨ THẤT THỦ</color>";

            string message = resultData.IsWon
                ? "Đã tiêu diệt Đại Boss Dạ Xoa và bảo vệ vững chắc Sông Quê!"
                : "Căn cứ Nhà Chính Bờ Tây đã bị quỷ sứ tàn phá hoàn toàn.";

            int minutes = Mathf.FloorToInt(resultData.ElapsedSeconds / 60f);
            int seconds = Mathf.FloorToInt(resultData.ElapsedSeconds % 60f);
            string summary = $"Thời gian: {minutes:00}:{seconds:00} | Gỗ: {resultData.WoodCollected} | Thưởng Cổ Tiền: +{totalEarnedCurrency:N0}";

            _lastProcessedResult = new DefenseProcessedResult(
                resultData.IsWon,
                resultData.ElapsedSeconds,
                resultData.WoodCollected,
                resultData.EnemiesDefeated,
                totalEarnedCurrency,
                title,
                message,
                summary);

            _hasProcessedResult = true;

            // 3. Cập nhật vào MetaProgressionSaveData
            try
            {
                var saveData = customSaveData ?? SaveSystem.Load();
                if (saveData != null)
                {
                    // Cộng tiền thưởng
                    saveData.totalCurrency += totalEarnedCurrency;

                    // Cập nhật kỷ lục cá nhân
                    saveData.UpdateBestStats(resultData.ElapsedSeconds, resultData.EnemiesDefeated);

                    // Đánh dấu hoàn thành Ải nếu chiến thắng
                    if (resultData.IsWon)
                    {
                        saveData.MarkStageCompleted(resultData.StageId, resultData.ElapsedSeconds, 3);
                    }

                    // Lưu dữ liệu
                    if (customSaveAction != null)
                    {
                        customSaveAction(saveData);
                    }
                    else
                    {
                        SaveSystem.Save(saveData);
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
