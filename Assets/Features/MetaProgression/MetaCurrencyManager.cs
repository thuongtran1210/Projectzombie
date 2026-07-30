using UnityEngine;
using System;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.MetaProgression
{
    /// <summary>
    /// Singleton quản lý Currency Meta ("Cổ Tiền" — tiền xu cổ Việt Nam) — đồng tiền vĩnh viễn không mất
    /// giữa các run. Đồng bộ với GameManager để lưu/tải qua Local Save System.
    /// </summary>
    public class MetaCurrencyManager : MonoBehaviour
    {
        // ====================================================================
        // SINGLETON
        // ====================================================================
        public static MetaCurrencyManager Instance { get; private set; }

        // ====================================================================
        // STATE
        // ====================================================================

        /// <summary>Tổng số Cổ Tiền hiện có.</summary>
        public int TotalCurrency { get; private set; } = 0;

        /// <summary>Kích hoạt khi số dư thay đổi. Truyền số dư mới.</summary>
        public event Action<int> OnCurrencyChanged;

        private MetaProgressionSaveData _saveData;

        // ====================================================================
        // UNITY LIFECYCLE
        // ====================================================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // ====================================================================
        // PUBLIC API
        // ====================================================================

        /// <summary>
        /// Khởi tạo từ dữ liệu save.
        /// </summary>
        public void Initialize(MetaProgressionSaveData saveData)
        {
            _saveData = saveData ?? new MetaProgressionSaveData();
            TotalCurrency = _saveData.totalCurrency;
            OnCurrencyChanged?.Invoke(TotalCurrency);
            Debug.Log($"[MetaCurrencyManager] Khởi tạo — Cổ Tiền: {TotalCurrency}");
        }

        /// <summary>
        /// Thêm currency sau khi kết thúc run. Tự động sync vào SaveData.
        /// </summary>
        public void AddCurrency(int amount)
        {
            if (amount <= 0) return;
            TotalCurrency += amount;
            SyncToSaveData();
            OnCurrencyChanged?.Invoke(TotalCurrency);
            Debug.Log($"[MetaCurrencyManager] +{amount} Coin Sinh Tồn. Tổng: {TotalCurrency}");
        }

        /// <summary>
        /// Chi tiêu currency (VD: mua Permanent Upgrade). Trả về false nếu không đủ.
        /// </summary>
        public bool SpendCurrency(int amount)
        {
            if (amount <= 0) return true;
            if (TotalCurrency < amount)
            {
                Debug.Log($"[MetaCurrencyManager] Không đủ coin. Cần: {amount}, Có: {TotalCurrency}");
                return false;
            }

            TotalCurrency -= amount;
            SyncToSaveData();
            OnCurrencyChanged?.Invoke(TotalCurrency);
            Debug.Log($"[MetaCurrencyManager] Chi {amount} Coin Sinh Tồn. Còn lại: {TotalCurrency}");
            return true;
        }

        /// <summary>
        /// Kiểm tra xem nhân vật đã được mở khóa chưa.
        /// </summary>
        public bool IsCharacterUnlocked(string characterId)
        {
            if (_saveData == null) return characterId == "default";
            return Array.IndexOf(_saveData.unlockedCharacters, characterId) >= 0;
        }

        /// <summary>
        /// Mở khóa nhân vật mới (sau khi đã SpendCurrency thành công).
        /// </summary>
        public bool UnlockCharacter(string characterId, int cost)
        {
            if (IsCharacterUnlocked(characterId))
            {
                Debug.Log($"[MetaCurrencyManager] Nhân vật '{characterId}' đã mở khóa rồi.");
                return false;
            }

            if (!SpendCurrency(cost)) return false;

            // Mở rộng mảng unlockedCharacters
            var current = _saveData.unlockedCharacters;
            var updated = new string[current.Length + 1];
            Array.Copy(current, updated, current.Length);
            updated[current.Length] = characterId;
            _saveData.unlockedCharacters = updated;

            SyncToSaveData();
            Debug.Log($"[MetaCurrencyManager] Mở khóa nhân vật: '{characterId}'");
            return true;
        }

        /// <summary>
        /// Trả về SaveData hiện tại để GameManager có thể serialize và gửi lên web.
        /// </summary>
        public MetaProgressionSaveData GetSaveData() => _saveData;

        // ====================================================================
        // PRIVATE HELPERS
        // ====================================================================

        private void SyncToSaveData()
        {
            if (_saveData != null)
                _saveData.totalCurrency = TotalCurrency;
        }
    }
}
