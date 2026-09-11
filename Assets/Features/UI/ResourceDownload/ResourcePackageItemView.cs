using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectZombie.Features.UI.ResourceDownload
{
    [Serializable]
    public class ResourcePackageItemData
    {
        public string packageId;
        public string title;
        public string description;
        public string addressableKey;
        public float estimatedSizeMb;
        public bool isDownloaded;
        public long actualDownloadSizeBytes;
    }

    /// <summary>
    /// Item View hiển thị một gói tài nguyên trong danh sách.
    /// </summary>
    public class ResourcePackageItemView : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descText;
        [SerializeField] private TextMeshProUGUI _statusText;

        [Header("Buttons")]
        [SerializeField] private Button _downloadButton;
        [SerializeField] private TextMeshProUGUI _downloadButtonText;
        [SerializeField] private Button _deleteButton;

        [Header("Progress")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _progressText;

        public event Action<ResourcePackageItemData> OnDownloadClicked;
        public event Action<ResourcePackageItemData> OnDeleteClicked;

        private ResourcePackageItemData _data;

        public void Bind(ResourcePackageItemData data)
        {
            _data = data;
            if (_titleText != null) _titleText.text = data.title;
            if (_descText != null) _descText.text = data.description;

            if (_downloadButton != null)
            {
                _downloadButton.onClick.RemoveAllListeners();
                _downloadButton.onClick.AddListener(() => OnDownloadClicked?.Invoke(_data));
            }

            if (_deleteButton != null)
            {
                _deleteButton.onClick.RemoveAllListeners();
                _deleteButton.onClick.AddListener(() => OnDeleteClicked?.Invoke(_data));
            }

            if (_progressBar != null)
            {
                _progressBar.interactable = false;
                _progressBar.gameObject.SetActive(false);
            }

            UpdateState(data.isDownloaded, data.actualDownloadSizeBytes);
        }

        public void UpdateState(bool isDownloaded, long downloadSizeBytes)
        {
            if (_data != null)
            {
                _data.isDownloaded = isDownloaded;
                _data.actualDownloadSizeBytes = downloadSizeBytes;
            }

            if (_progressBar != null) _progressBar.gameObject.SetActive(false);

            if (isDownloaded)
            {
                // ĐÃ TẢI: Hiện nhãn Đã tải + Nút XÓA, Ẩn nút Tải
                if (_statusText != null) _statusText.text = "<color=#00FF88>[ĐÃ TẢI]</color>";
                if (_downloadButton != null) _downloadButton.gameObject.SetActive(false);
                if (_deleteButton != null) _deleteButton.gameObject.SetActive(true);
            }
            else
            {
                // CHƯA TẢI: Tính toán chuỗi dung lượng thông minh (KB hoặc MB)
                string sizeStr = "";
                if (downloadSizeBytes > 0)
                {
                    float sizeMb = downloadSizeBytes / 1048576f;
                    if (sizeMb >= 0.1f)
                    {
                        sizeStr = $" ({sizeMb:0.0} MB)";
                    }
                    else
                    {
                        float sizeKb = downloadSizeBytes / 1024f;
                        sizeStr = $" ({sizeKb:0.0} KB)";
                    }
                }
                else if (_data != null && _data.estimatedSizeMb > 0f)
                {
                    sizeStr = $" ({_data.estimatedSizeMb:0.0} MB)";
                }

                if (_statusText != null) _statusText.text = $"<color=#FFAA00>[CHƯA TẢI]</color>";
                if (_downloadButton != null)
                {
                    _downloadButton.gameObject.SetActive(true);
                    if (_downloadButtonText != null)
                    {
                        _downloadButtonText.text = $"TẢI VỀ{sizeStr}";
                    }
                }
                if (_deleteButton != null) _deleteButton.gameObject.SetActive(false); // Chưa tải thì KHÔNG CÓ nút Xóa
            }
        }

        public void UpdateProgress(float percent, string message)
        {
            if (_progressBar != null)
            {
                _progressBar.interactable = false;
                _progressBar.gameObject.SetActive(true);
                _progressBar.value = percent;
            }
            if (_downloadButton != null) _downloadButton.gameObject.SetActive(false);
            if (_deleteButton != null) _deleteButton.gameObject.SetActive(false);
            if (_progressText != null) _progressText.text = message;
        }
    }
}
