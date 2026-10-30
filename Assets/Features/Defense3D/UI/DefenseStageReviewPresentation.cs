using System;
using UnityEngine;
using ProjectZombie.Features.Maps;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>Formatted mission information for the read-only pre-battle screen.</summary>
    public sealed class DefenseStageReviewPresentation
    {
        public string Code { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Duration { get; private set; }
        public string Rewards { get; private set; }
        public Sprite Artwork { get; private set; }

        /// <summary>Uses authored stage values; never substitutes rewards or duration from a mockup.</summary>
        public static DefenseStageReviewPresentation FromStage(StageDefinitionSO stage)
        {
            if (stage == null) return new DefenseStageReviewPresentation
            {
                Code = "THÔNG TIN ẢI", Name = "Chưa chọn ải", Description = "Trở lại danh sách để chọn một ải.",
                Duration = "Chưa có thời gian", Rewards = "Phần thưởng hiển thị sau khi chọn ải."
            };
            double seconds = Math.Ceiling(stage.DefenseDuration);
            bool valid = !double.IsNaN(seconds) && !double.IsInfinity(seconds) && seconds > 0 && seconds < int.MaxValue;
            int total = valid ? (int)seconds : 0;
            return new DefenseStageReviewPresentation
            {
                Code = "ẢI ĐÃ CHỌN",
                Name = (stage.stageName ?? string.Empty).Replace(" (3D)", string.Empty),
                Description = (stage.description ?? string.Empty).Replace("3D ", string.Empty),
                Duration = valid ? $"{total / 60:00}:{total % 60:00}" : "Chưa có thời gian",
                Rewards = $"{Mathf.Max(0, stage.baseRewardCoins):N0} Cổ Tiền   •   {Mathf.Max(0, stage.baseRewardExp):N0} Kinh nghiệm",
                Artwork = stage.stageThumbnail
            };
        }
    }
}
