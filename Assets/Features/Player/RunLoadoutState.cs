using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Lưu trữ và tự động nạp trạng thái Loadout xuất trận (Nhân vật + Vũ Khí Chính + Pháp Bảo)
    /// Hỗ trợ nạp mặc định khi mở game và lưu vĩnh viễn (Persistence) qua PlayerPrefs theo GDD v5.0.
    /// </summary>
    public static class RunLoadoutState
    {
        private const string KEY_HERO_ID = "VONGXUYEN_SAVED_HERO_ID";
        private const string KEY_PRIMARY_ID = "VONGXUYEN_SAVED_PRIMARY_ID";
        private const string KEY_RELICS_CSV = "VONGXUYEN_SAVED_RELICS_CSV";

        private static bool _isInitialized = false;

        public static CharacterEntry SelectedCharacter
        {
            get
            {
                EnsureInitialized();
                return _selectedCharacter;
            }
            set => _selectedCharacter = value;
        }

        public static WeaponData SelectedPrimaryWeapon
        {
            get
            {
                EnsureInitialized();
                return _selectedPrimaryWeapon;
            }
            set => _selectedPrimaryWeapon = value;
        }

        public static WeaponData SelectedRelic
        {
            get
            {
                EnsureInitialized();
                return _selectedRelics != null && _selectedRelics.Count > 0 ? _selectedRelics[0] : null;
            }
            set
            {
                _selectedRelics.Clear();
                if (value != null) _selectedRelics.Add(value);
            }
        }

        public static List<WeaponData> SelectedRelics
        {
            get
            {
                EnsureInitialized();
                return _selectedRelics;
            }
            set => _selectedRelics = value ?? new List<WeaponData>();
        }

        private static CharacterEntry _selectedCharacter;
        private static WeaponData _selectedPrimaryWeapon;
        private static List<WeaponData> _selectedRelics = new List<WeaponData>();

        public static bool HasCustomLoadout => _selectedCharacter != null || _selectedPrimaryWeapon != null || (_selectedRelics != null && _selectedRelics.Count > 0);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void EnsureInitialized()
        {
            if (_isInitialized && _selectedCharacter != null && _selectedPrimaryWeapon != null) return;
            LoadFromSaveOrDefaults();
        }

        /// <summary>
        /// Nạp dữ liệu đã lưu từ bộ nhớ, nếu chưa có sẽ nạp bộ mặc định của tướng đầu tiên.
        /// </summary>
        public static void LoadFromSaveOrDefaults()
        {
            _isInitialized = true;

            // 1. Nạp Database Nhân Vật từ CharacterDatabaseSO (Single Source of Truth)
            var characterDatabase = Resources.Load<CharacterDatabaseSO>("CharacterDatabase");
            #if UNITY_EDITOR
            if (characterDatabase == null)
            {
                characterDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDatabaseSO>("Assets/_Data/CharacterDatabase.asset");
            }
            #endif

            // 2. Nạp Database Vũ Khí
            var allWeapons = LoadAllWeaponsDatabase();

            // 3. Đọc từ PlayerPrefs
            string savedHeroId = PlayerPrefs.GetString(KEY_HERO_ID, string.Empty);
            string savedPrimaryId = PlayerPrefs.GetString(KEY_PRIMARY_ID, string.Empty);
            string savedRelicsCsv = PlayerPrefs.GetString(KEY_RELICS_CSV, string.Empty);

            // Tìm nhân vật từ CharacterDatabaseSO
            if (characterDatabase != null && characterDatabase.Characters != null && characterDatabase.Characters.Count > 0)
            {
                CharacterDataSO matchedSO = null;
                if (!string.IsNullOrEmpty(savedHeroId))
                {
                    matchedSO = characterDatabase.GetCharacterById(savedHeroId);
                }

                if (matchedSO == null)
                {
                    matchedSO = characterDatabase.GetCharacterByIndex(0);
                }

                if (matchedSO != null)
                {
                    _selectedCharacter = matchedSO.ToEntry();
                }
            }

            // Tìm Vũ Khí Chính
            if (!string.IsNullOrEmpty(savedPrimaryId))
            {
                _selectedPrimaryWeapon = allWeapons.Find(w => w.weaponId == savedPrimaryId || w.name == savedPrimaryId || w.weaponName == savedPrimaryId);
            }

            if (_selectedPrimaryWeapon == null && _selectedCharacter != null && _selectedCharacter.defaultPrimaryWeapon != null)
            {
                _selectedPrimaryWeapon = _selectedCharacter.defaultPrimaryWeapon;
            }
            else if (_selectedPrimaryWeapon == null && allWeapons.Count > 0)
            {
                _selectedPrimaryWeapon = allWeapons.Find(w => w.weaponRole == WeaponRole.PrimaryWeapon);
            }

            // Tìm Pháp Bảo Hộ Thân (Tối đa 3)
            _selectedRelics.Clear();
            if (!string.IsNullOrEmpty(savedRelicsCsv))
            {
                string[] relicIds = savedRelicsCsv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var rId in relicIds)
                {
                    var found = allWeapons.Find(w => w.weaponId == rId || w.name == rId || w.weaponName == rId);
                    if (found != null && !_selectedRelics.Contains(found) && _selectedRelics.Count < 3)
                    {
                        _selectedRelics.Add(found);
                    }
                }
            }

            if (_selectedRelics.Count == 0 && _selectedCharacter != null && _selectedCharacter.defaultRelics != null)
            {
                foreach (var r in _selectedCharacter.defaultRelics)
                {
                    if (r != null && !_selectedRelics.Contains(r) && _selectedRelics.Count < 3)
                    {
                        _selectedRelics.Add(r);
                    }
                }
            }

            if (_selectedRelics.Count == 0)
            {
                foreach (var w in allWeapons)
                {
                    if (w.weaponRole != WeaponRole.PrimaryWeapon && !_selectedRelics.Contains(w) && _selectedRelics.Count < 3)
                    {
                        _selectedRelics.Add(w);
                    }
                }
            }

            Debug.Log($"<color=#00FF88>[RunLoadoutState]</color> Nạp Loadout thành công: Hero={(_selectedCharacter != null ? _selectedCharacter.characterName : "Null")}, Primary={(_selectedPrimaryWeapon != null ? _selectedPrimaryWeapon.weaponName : "Null")}, Relics={_selectedRelics.Count}");
        }

        /// <summary>
        /// Thiết lập và lưu toàn bộ cấu hình Loadout xuống bộ nhớ thiết bị.
        /// </summary>
        public static void SetLoadout(CharacterEntry character, WeaponData primaryWeapon, List<WeaponData> relics)
        {
            _isInitialized = true;
            _selectedCharacter = character;
            _selectedPrimaryWeapon = primaryWeapon;
            
            _selectedRelics.Clear();
            if (relics != null)
            {
                for (int i = 0; i < Mathf.Min(3, relics.Count); i++)
                {
                    if (relics[i] != null && !_selectedRelics.Contains(relics[i]))
                    {
                        _selectedRelics.Add(relics[i]);
                    }
                }
            }

            SaveToDisk();
            Debug.Log($"<color=#00FF88>[RunLoadoutState]</color> Đã lưu Loadout: Hero={(_selectedCharacter != null ? _selectedCharacter.characterName : "Null")}, Primary={(_selectedPrimaryWeapon != null ? _selectedPrimaryWeapon.weaponName : "None")}, Relics Count={_selectedRelics.Count}");
        }

        /// <summary>
        /// Cập nhật nhân vật đã chọn và tự động gán vũ khí mặc định nếu chưa có.
        /// </summary>
        public static void SetCharacter(CharacterEntry character)
        {
            _isInitialized = true;
            _selectedCharacter = character;

            if (character != null)
            {
                if (_selectedPrimaryWeapon == null && character.defaultPrimaryWeapon != null)
                {
                    _selectedPrimaryWeapon = character.defaultPrimaryWeapon;
                }
                if ((_selectedRelics == null || _selectedRelics.Count == 0) && character.defaultRelics != null)
                {
                    _selectedRelics.Clear();
                    foreach (var r in character.defaultRelics)
                    {
                        if (r != null && !_selectedRelics.Contains(r) && _selectedRelics.Count < 3)
                        {
                            _selectedRelics.Add(r);
                        }
                    }
                }
            }

            SaveToDisk();
            Debug.Log($"<color=#00FF88>[RunLoadoutState]</color> Đã lưu chọn tướng: {(character != null ? character.characterName : "Null")}");
        }

        /// <summary>
        /// Ghi dữ liệu Loadout hiện tại vào PlayerPrefs vĩnh viễn.
        /// </summary>
        public static void SaveToDisk()
        {
            if (_selectedCharacter != null)
            {
                string cId = !string.IsNullOrEmpty(_selectedCharacter.characterId) ? _selectedCharacter.characterId : _selectedCharacter.characterName;
                PlayerPrefs.SetString(KEY_HERO_ID, cId);
            }

            if (_selectedPrimaryWeapon != null)
            {
                string pId = !string.IsNullOrEmpty(_selectedPrimaryWeapon.weaponId) ? _selectedPrimaryWeapon.weaponId : _selectedPrimaryWeapon.name;
                PlayerPrefs.SetString(KEY_PRIMARY_ID, pId);
            }

            if (_selectedRelics != null && _selectedRelics.Count > 0)
            {
                var rIds = new List<string>();
                foreach (var r in _selectedRelics)
                {
                    if (r != null)
                    {
                        rIds.Add(!string.IsNullOrEmpty(r.weaponId) ? r.weaponId : r.name);
                    }
                }
                PlayerPrefs.SetString(KEY_RELICS_CSV, string.Join(",", rIds));
            }
            else
            {
                PlayerPrefs.SetString(KEY_RELICS_CSV, string.Empty);
            }

            PlayerPrefs.Save();
        }

        private static List<WeaponData> LoadAllWeaponsDatabase()
        {
            var list = new List<WeaponData>();
            #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:WeaponData", new[] { "Assets/_Data/Weapons" });
            foreach (var guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var wd = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponData>(path);
                if (wd != null && !list.Contains(wd)) list.Add(wd);
            }
            #else
            var loaded = Resources.LoadAll<WeaponData>("ScriptableObjects/Weapons");
            list.AddRange(loaded);
            #endif
            return list;
        }

        /// <summary>
        /// Xóa bỏ cấu hình tùy chỉnh để quay về mặc định.
        /// </summary>
        public static void ResetLoadout()
        {
            _selectedCharacter = null;
            _selectedPrimaryWeapon = null;
            _selectedRelics.Clear();
            PlayerPrefs.DeleteKey(KEY_HERO_ID);
            PlayerPrefs.DeleteKey(KEY_PRIMARY_ID);
            PlayerPrefs.DeleteKey(KEY_RELICS_CSV);
            PlayerPrefs.Save();
            LoadFromSaveOrDefaults();
        }
    }
}
