using System;
using System.IO;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D.Loadout
{
    /// <summary>
    /// Repository quản lý lưu trữ và nạp cấu hình đội hình xuất trận Defense 3D.
    /// Tuân thủ nghiêm ngặt các nguyên tắc G3:
    /// 1. Một nguồn lưu lựa chọn chính duy nhất (defense_loadout.json); PlayerPrefs cũ chỉ là đầu vào migration một lần.
    /// 2. Ghi tệp an toàn (Atomic Write qua file tạm và backup .bak) để chống hỏng file trên Android.
    /// 3. Không ghi đè save của phiên bản tương lai (Version > CURRENT_VERSION).
    /// 4. Fallback an toàn về default loadout khi gặp dữ liệu hỏng hoặc thiếu trường.
    /// </summary>
    public sealed class DefenseLoadoutRepository : ILoadoutRepository
    {
        public const int CURRENT_VERSION = 1;
        public const string DEFAULT_FILE_NAME = "defense_loadout.json";

        // Khóa PlayerPrefs của hệ thống 2D cũ dùng cho migration một lần
        public const string LEGACY_PREF_HERO_ID = "RunLoadout_SelectedHero";
        public const string LEGACY_PREF_RELICS_CSV = "RunLoadout_EquippedRelics";

        private readonly string _saveFilePath;
        private readonly string _backupFilePath;
        private readonly string _tempFilePath;

        public DefenseLoadoutRepository(string customDirectory = null, string fileName = null)
        {
            string dir = string.IsNullOrEmpty(customDirectory) ? Application.persistentDataPath : customDirectory;
            string file = string.IsNullOrEmpty(fileName) ? DEFAULT_FILE_NAME : fileName;

            _saveFilePath = Path.Combine(dir, file);
            _backupFilePath = _saveFilePath + ".bak";
            _tempFilePath = _saveFilePath + ".tmp";
        }

        public string SaveFilePath => _saveFilePath;

        /// <summary>
        /// Nạp cấu hình Loadout hiện tại.
        /// Tự động thực hiện migration một lần từ PlayerPrefs/Meta cũ nếu chưa có tệp JSON.
        /// </summary>
        public LoadoutSaveDto Load()
        {
            try
            {
                // 1. Nếu tệp save đã tồn tại, đọc từ tệp JSON
                if (File.Exists(_saveFilePath))
                {
                    string json = File.ReadAllText(_saveFilePath);
                    var dto = ParseAndValidateJson(json);
                    if (dto != null) return dto;

                    // Nếu file chính bị hỏng, thử khôi phục từ backup
                    if (File.Exists(_backupFilePath))
                    {
                        Debug.LogWarning("[DefenseLoadoutRepository] File chính hỏng, đang khôi phục từ backup...");
                        string backupJson = File.ReadAllText(_backupFilePath);
                        var backupDto = ParseAndValidateJson(backupJson);
                        if (backupDto != null) return backupDto;
                    }
                }

                // 2. Chưa có tệp save: thực hiện migration từ dữ liệu cũ
                var migratedDto = TryMigrateFromLegacy();
                if (migratedDto != null)
                {
                    Save(migratedDto);
                    return migratedDto;
                }

                // 3. Fallback: Tạo DTO mặc định
                var defaultDto = CreateDefaultDto();
                Save(defaultDto);
                return defaultDto;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DefenseLoadoutRepository] Lỗi khi nạp Loadout: {ex.Message}. Trả về cấu hình mặc định.");
                return CreateDefaultDto();
            }
        }

        /// <summary>
        /// Lưu cấu hình Loadout xuống đĩa.
        /// Sử dụng kỹ thuật Atomic Write: Ghi vào .tmp -> Sao lưu .bak -> Đổi tên đè lên file chính.
        /// </summary>
        public void Save(LoadoutSaveDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            try
            {
                // Kiểm tra phiên bản tương lai: Không ghi đè nếu phiên bản trên đĩa cao hơn phiên bản app hiểu
                if (File.Exists(_saveFilePath))
                {
                    try
                    {
                        string existingJson = File.ReadAllText(_saveFilePath);
                        var existingDto = JsonUtility.FromJson<LoadoutSaveDto>(existingJson);
                        if (existingDto != null && existingDto.Version > CURRENT_VERSION)
                        {
                            Debug.LogWarning($"[DefenseLoadoutRepository] Phát hiện dữ liệu từ phiên bản tương lai (v{existingDto.Version} > v{CURRENT_VERSION}). Từ chối ghi đè để bảo vệ save của người dùng.");
                            return;
                        }
                    }
                    catch
                    {
                        // Nếu file hiện tại hỏng, cho phép ghi đè
                    }
                }

                dto.Version = CURRENT_VERSION;
                string json = JsonUtility.ToJson(dto, true);

                string dir = Path.GetDirectoryName(_saveFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                // Ghi ra file tạm
                File.WriteAllText(_tempFilePath, json);

                // Tạo backup từ file cũ nếu có
                if (File.Exists(_saveFilePath))
                {
                    try
                    {
                        File.Copy(_saveFilePath, _backupFilePath, true);
                    }
                    catch (Exception bakEx)
                    {
                        Debug.LogWarning($"[DefenseLoadoutRepository] Không thể tạo backup: {bakEx.Message}");
                    }
                }

                // Đổi file tạm thành file chính
                File.Copy(_tempFilePath, _saveFilePath, true);
                File.Delete(_tempFilePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DefenseLoadoutRepository] Lỗi khi lưu Loadout: {ex.Message}");
            }
        }

        private LoadoutSaveDto ParseAndValidateJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                var dto = JsonUtility.FromJson<LoadoutSaveDto>(json);
                if (dto == null) return null;

                // Nếu file trên đĩa thuộc phiên bản tương lai, giữ nguyên không sửa đổi
                if (dto.Version > CURRENT_VERSION)
                {
                    return dto;
                }

                string mappedHero = DefenseLoadoutIdMapper.MapHeroId(dto.HeroId);
                string mappedRelic = DefenseLoadoutIdMapper.MapRelicId(dto.RelicId);
                var mappedMinions = DefenseLoadoutIdMapper.MapMinionIds(dto.MinionIds);
                var mappedTowers = DefenseLoadoutIdMapper.MapTowerIds(dto.TowerIds);

                bool needsRewrite = !string.Equals(dto.HeroId, mappedHero, StringComparison.Ordinal)
                    || !string.Equals(dto.RelicId, mappedRelic, StringComparison.Ordinal)
                    || dto.MinionIds == null || dto.MinionIds.Count != mappedMinions.Count
                    || dto.TowerIds == null || dto.TowerIds.Count != mappedTowers.Count;

                dto.HeroId = mappedHero;
                dto.RelicId = mappedRelic;
                dto.MinionIds = mappedMinions;
                dto.TowerIds = mappedTowers;

                if (needsRewrite)
                {
                    Save(dto);
                }

                return dto;
            }
            catch
            {
                return null;
            }
        }

        private LoadoutSaveDto TryMigrateFromLegacy()
        {
            string legacyHero = PlayerPrefs.GetString("VONGXUYEN_SAVED_HERO_ID", null);
            string legacyRelic = null;
            string currentRelics = PlayerPrefs.GetString("VONGXUYEN_SAVED_RELICS_CSV", string.Empty);
            if (!string.IsNullOrEmpty(currentRelics)) legacyRelic = currentRelics.Split(',')[0].Trim();

            // Đọc PlayerPrefs cũ nếu có
            if (PlayerPrefs.HasKey(LEGACY_PREF_HERO_ID))
            {
                legacyHero = PlayerPrefs.GetString(LEGACY_PREF_HERO_ID, null);
            }

            if (PlayerPrefs.HasKey(LEGACY_PREF_RELICS_CSV))
            {
                string csv = PlayerPrefs.GetString(LEGACY_PREF_RELICS_CSV, null);
                if (!string.IsNullOrEmpty(csv))
                {
                    var parts = csv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                    {
                        legacyRelic = parts[0].Trim();
                    }
                }
            }

            if (string.IsNullOrEmpty(legacyHero) && string.IsNullOrEmpty(legacyRelic))
            {
                return null;
            }

            string mappedHero = DefenseLoadoutIdMapper.MapHeroId(legacyHero);
            string mappedRelic = DefenseLoadoutIdMapper.MapRelicId(legacyRelic);
            var minions = DefenseLoadoutIdMapper.MapMinionIds(null);
            var towers = DefenseLoadoutIdMapper.MapTowerIds(null);

            Debug.Log($"[DefenseLoadoutRepository] Đã hoàn thành migration từ PlayerPrefs cũ: Hero '{legacyHero}' -> '{mappedHero}', Relic '{legacyRelic}' -> '{mappedRelic}'.");

            return new LoadoutSaveDto(mappedHero, mappedRelic, minions, towers, CURRENT_VERSION);
        }

        public static LoadoutSaveDto CreateDefaultDto()
        {
            var defSnapshot = DefenseLoadoutSnapshot.CreateDefault();
            return new LoadoutSaveDto(
                defSnapshot.HeroId,
                defSnapshot.RelicId,
                defSnapshot.MinionIds,
                defSnapshot.TowerIds,
                CURRENT_VERSION);
        }
    }
}
