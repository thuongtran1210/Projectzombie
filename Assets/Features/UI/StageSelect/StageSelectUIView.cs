using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.Maps;
using ProjectZombie.Core.Services.Addressables;

namespace ProjectZombie.Features.UI.StageSelect
{
    /// <summary>
    /// Giao diện Chọn Ải / Màn chơi (Stage Selection View) - Chuẩn MVP Pattern.
    /// Hiển thị danh sách ải, ảnh minh họa, thông tin phần thưởng và trạng thái DLC.
    /// </summary>
    public class StageSelectUIView : BaseMetaScreenView
    {
        public override MetaScreenType ScreenType => MetaScreenType.StageSelect;

        [Header("Stage Info UI")]
        [SerializeField] private TextMeshProUGUI _stageTitleText;
        [SerializeField] private TextMeshProUGUI _stageDescText;
        [SerializeField] private TextMeshProUGUI _recommendedLevelText;
        [SerializeField] private Image _stageThumbnailImage;

        [Header("Rewards UI")]
        [SerializeField] private TextMeshProUGUI _coinsRewardText;
        [SerializeField] private TextMeshProUGUI _expRewardText;

        [Header("DLC & Action Buttons")]
        [SerializeField] private Button _startBattleButton;
        [SerializeField] private Button _downloadDlcButton;
        [SerializeField] private TextMeshProUGUI _dlcSizeText;
        [SerializeField] private Slider _downloadProgressBar;
        [SerializeField] private TextMeshProUGUI _downloadStatusText;

        [Header("Navigation")]
        [SerializeField] private Button _prevStageButton;
        [SerializeField] private Button _nextStageButton;
        [SerializeField] private Button _backButton;

        public event Action OnStartBattleClicked;
        public event Action OnDownloadDlcClicked;
        public event Action OnPrevStageClicked;
        public event Action OnNextStageClicked;
        public event Action OnBackClicked;

        protected override void Awake()
        {
            base.Awake();
            if (_startBattleButton != null) _startBattleButton.onClick.AddListener(() => OnStartBattleClicked?.Invoke());
            if (_downloadDlcButton != null) _downloadDlcButton.onClick.AddListener(() => OnDownloadDlcClicked?.Invoke());
            if (_prevStageButton != null) _prevStageButton.onClick.AddListener(() => OnPrevStageClicked?.Invoke());
            if (_nextStageButton != null) _nextStageButton.onClick.AddListener(() => OnNextStageClicked?.Invoke());
            if (_backButton != null) _backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed(); // Gọi MetaUIManager.PopScreen() chuẩn từ BaseMetaScreenView
        }

        public void RenderStageInfo(StageDefinitionSO stage, bool isDlcDownloaded, bool isPreviousAvailable, bool isNextAvailable, float actualDownloadSizeMb = 0f)
        {
            if (stage == null) return;

            if (_stageTitleText != null) _stageTitleText.text = stage.stageName;
            if (_stageDescText != null) _stageDescText.text = stage.description;
            if (_recommendedLevelText != null) _recommendedLevelText.text = $"<color=#FFD700>Cấp Đề Cử:</color> Lv.{stage.recommendedLevel}";
            
            if (_stageThumbnailImage != null && stage.stageThumbnail != null)
            {
                _stageThumbnailImage.sprite = stage.stageThumbnail;
            }

            if (_coinsRewardText != null) _coinsRewardText.text = $"<color=#FFD700>Cổ Tiền:</color> {stage.baseRewardCoins:N0}";
            if (_expRewardText != null) _expRewardText.text = $"<color=#00E5FF>Kinh Nghiệm:</color> {stage.baseRewardExp:N0}";

            // Hiển thị trạng thái Đã Vượt Ải từ Save Data
            bool isCompleted = ProjectZombie.Core.Save.GameManager.Instance != null &&
                               ProjectZombie.Core.Save.GameManager.Instance.SaveData != null &&
                               ProjectZombie.Core.Save.GameManager.Instance.SaveData.IsStageCompleted(stage.stageId);

            if (isCompleted && _stageTitleText != null)
            {
                _stageTitleText.text = $"{stage.stageName} <color=#FFD700>[ĐÃ VƯỢT ẢI]</color>";
            }
            if (_startBattleButton != null) _startBattleButton.gameObject.SetActive(isDlcDownloaded);
            if (_downloadDlcButton != null)
            {
                _downloadDlcButton.gameObject.SetActive(!isDlcDownloaded);
                if (_dlcSizeText != null)
                {
                    float displayMb = actualDownloadSizeMb > 0.01f ? actualDownloadSizeMb : stage.estimatedDlcSizeMb;
                    _dlcSizeText.text = displayMb > 0.01f ? $"Tải Màn Chơi ({displayMb:0.0} MB)" : "Tải Màn Chơi";
                }
            }

            if (_downloadProgressBar != null)
            {
                _downloadProgressBar.interactable = false; // Khóa không cho người dùng kéo trượt bằng tay
                _downloadProgressBar.gameObject.SetActive(false);
            }

            if (_prevStageButton != null) _prevStageButton.interactable = isPreviousAvailable;
            if (_nextStageButton != null) _nextStageButton.interactable = isNextAvailable;
        }

        public void UpdateDownloadProgress(float percent, string statusText)
        {
            if (_downloadProgressBar != null)
            {
                _downloadProgressBar.interactable = false;
                _downloadProgressBar.gameObject.SetActive(true);
                _downloadProgressBar.value = percent;
            }
            if (_startBattleButton != null) _startBattleButton.gameObject.SetActive(false);
            if (_downloadDlcButton != null) _downloadDlcButton.gameObject.SetActive(false);
            if (_downloadStatusText != null) _downloadStatusText.text = statusText;
        }
    }
}
