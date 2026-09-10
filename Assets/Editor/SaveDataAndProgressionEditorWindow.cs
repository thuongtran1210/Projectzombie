#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProjectZombie.Core.Save;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.UI;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Debug & Cheat Tool Quản Lý Dữ Liệu Lưu (player_save.json & Runtime Managers).
    /// Cho phép xem, tăng giảm (+ / -) Cổ Tiền, Thẻ Mảnh Pháp Bảo, Cấp Sao và Quyền Sở Hữu Nhân Vật.
    /// Menu: Tools > ProjectZombie > Save Data & Progression Editor (+/-)
    /// </summary>
    public class SaveDataAndProgressionEditorWindow : EditorWindow
    {
        // 1. Menu chính trên thanh công cụ Unity (Dễ thấy nhất - Top Menu)
        [MenuItem("⚡ CHEAT & SAVE DATA ⚡/🎮 Save Data & Cheat Manager (+/-) %F1", priority = 0)]
        public static void ShowWindowTop()
        {
            OpenWindow();
        }

        // 2. Menu phụ trong Tools
        [MenuItem("Tools/ProjectZombie/⚡ Save Data & Progression Editor (+/-)", priority = 0)]
        public static void ShowWindow()
        {
            OpenWindow();
        }

        private static void OpenWindow()
        {
            var window = GetWindow<SaveDataAndProgressionEditorWindow>("⚡ CHEAT & SAVE DATA ⚡");
            window.minSize = new Vector2(700, 750);
            window.Show();
        }

        private Vector2 _scrollPos;
        private MetaProgressionSaveData _cachedSaveData;
        private List<WeaponData> _allWeapons = new List<WeaponData>();
        private List<CharacterDataSO> _allHeroes = new List<CharacterDataSO>();

        private int _customMoneyInput = 1000;
        private int _customShardInput = 5;
        private string _searchWeaponText = "";

        private void OnEnable()
        {
            LoadData();
        }

        private void OnFocus()
        {
            LoadData();
        }

        private void LoadData()
        {
            // 1. Nạp Save Data
            if (Application.isPlaying && GameManager.Instance != null && GameManager.Instance.SaveData != null)
            {
                _cachedSaveData = GameManager.Instance.SaveData;
            }
            else
            {
                _cachedSaveData = SaveSystem.Load();
            }

            // 2. Nạp Weapon Catalog
            _allWeapons.Clear();
            var seenWIds = new HashSet<string>();
            string[] wGuids = AssetDatabase.FindAssets("t:WeaponData");
            foreach (var g in wGuids)
            {
                var w = AssetDatabase.LoadAssetAtPath<WeaponData>(AssetDatabase.GUIDToAssetPath(g));
                if (w != null && !string.IsNullOrEmpty(w.weaponId) && seenWIds.Add(w.weaponId))
                {
                    _allWeapons.Add(w);
                }
            }

            // 3. Nạp Hero Catalog
            _allHeroes.Clear();
            var seenHIds = new HashSet<string>();
            string[] hGuids = AssetDatabase.FindAssets("t:CharacterDataSO");
            foreach (var g in hGuids)
            {
                var h = AssetDatabase.LoadAssetAtPath<CharacterDataSO>(AssetDatabase.GUIDToAssetPath(g));
                if (h != null && !string.IsNullOrEmpty(h.characterId) && seenHIds.Add(h.characterId))
                {
                    _allHeroes.Add(h);
                }
            }
        }

        private void SaveData()
        {
            if (_cachedSaveData == null) return;

            if (Application.isPlaying)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SaveGame();
                }
                else
                {
                    SaveSystem.Save(_cachedSaveData);
                }
            }
            else
            {
                SaveSystem.Save(_cachedSaveData);
            }

            // Cập nhật UI nếu đang mở
            if (Application.isPlaying)
            {
                var codexPresenter = FindObjectOfType<CardCodexPresenter>(true);
                if (codexPresenter != null) codexPresenter.RefreshUI();

                var loadoutPresenter = FindObjectOfType<WeaponLoadoutPresenter>(true);
                if (loadoutPresenter != null && RunLoadoutState.SelectedCharacter != null)
                {
                    loadoutPresenter.SetupForHero(RunLoadoutState.SelectedCharacter);
                }

                var charSelectPresenter = FindObjectOfType<CharacterSelectionPresenter>(true);
                if (charSelectPresenter != null)
                {
                    charSelectPresenter.SendMessage("RenderCurrentCharacter", SendMessageOptions.DontRequireReceiver);
                }
            }

            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("🎮 BẢNG QUẢN LÝ DỮ LIỆU LƯU Ổ CỨNG (SAVE DATA DEBUGGER & CHEAT)", EditorStyles.boldLabel);
            
            string savePath = Path.Combine(Application.persistentDataPath, "player_save.json");
            EditorGUILayout.HelpBox($"File lưu: {savePath}\nChế độ hiện tại: {(Application.isPlaying ? "🟢 RUNTIME PLAYMODE (Tự động sync Live)" : "⚪ OFFLINE JSON DISK EDIT (Đọc/Ghi trực tiếp file)")}", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Tải Lại Dữ Liệu (Reload)", GUILayout.Height(26)))
            {
                LoadData();
            }
            if (GUILayout.Button("💾 Lưu Xuống Ổ Cứng (Save Disk)", GUILayout.Height(26)))
            {
                SaveData();
            }
            if (GUILayout.Button("📂 Mở Thư Mục Chứa Save", GUILayout.Height(26)))
            {
                EditorUtility.RevealInFinder(savePath);
            }
            if (GUILayout.Button("💥 Xóa Sạch Dữ Liệu (Reset Default)", GUILayout.Height(26)))
            {
                if (EditorUtility.DisplayDialog("Xóa Dữ Liệu Save", "Bạn có chắc chắn muốn xóa toàn bộ file save không? Dữ liệu sẽ quay về mặc định.", "Xóa", "Hủy"))
                {
                    SaveSystem.DeleteSave();
                    LoadData();
                    if (Application.isPlaying && GameManager.Instance != null)
                    {
                        GameManager.Instance.LoadGame();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            if (_cachedSaveData == null)
            {
                EditorGUILayout.HelpBox("Không thể tải Save Data!", MessageType.Error);
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            EditorGUILayout.Space(10);

            // =========================================================================
            // 1. CỔ TIỀN (CURRENCY)
            // =========================================================================
            DrawCurrencySection();

            EditorGUILayout.Space(15);

            // =========================================================================
            // 2. MỞ KHÓA ANH HÙNG (CHARACTERS)
            // =========================================================================
            DrawHeroesSection();

            EditorGUILayout.Space(15);

            // =========================================================================
            // 3. TIẾN TRÌNH PHÁP BẢO: THẺ MẢNH & CẤP SAO
            // =========================================================================
            DrawRelicsSection();

            EditorGUILayout.Space(10);
            EditorGUILayout.EndScrollView();
        }

        private void DrawCurrencySection()
        {
            EditorGUILayout.BeginVertical("box");
            int currentMoney = Application.isPlaying && MetaCurrencyManager.Instance != null 
                ? MetaCurrencyManager.Instance.TotalCurrency 
                : _cachedSaveData.totalCurrency;

            EditorGUILayout.LabelField($"💰 CỔ TIỀN HIỆN CÓ: <color=#FFD700><b>{currentMoney:N0}</b></color>", new GUIStyle(EditorStyles.boldLabel) { richText = true });

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("-10,000", GUILayout.Width(75))) ModifyCurrency(-10000);
            if (GUILayout.Button("-1,000", GUILayout.Width(70))) ModifyCurrency(-1000);
            if (GUILayout.Button("-100", GUILayout.Width(55))) ModifyCurrency(-100);
            if (GUILayout.Button("+100", GUILayout.Width(55))) ModifyCurrency(100);
            if (GUILayout.Button("+1,000", GUILayout.Width(70))) ModifyCurrency(1000);
            if (GUILayout.Button("+10,000", GUILayout.Width(75))) ModifyCurrency(10000);
            if (GUILayout.Button("+100,000", GUILayout.Width(85))) ModifyCurrency(100000);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _customMoneyInput = EditorGUILayout.IntField("Tùy chỉnh số tiền:", _customMoneyInput);
            if (GUILayout.Button("Đặt Chính Xác Số Này", GUILayout.Width(160)))
            {
                SetExactCurrency(_customMoneyInput);
            }
            if (GUILayout.Button("Đặt Về 0", GUILayout.Width(80)))
            {
                SetExactCurrency(0);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void ModifyCurrency(int delta)
        {
            if (Application.isPlaying && MetaCurrencyManager.Instance != null)
            {
                if (delta > 0) MetaCurrencyManager.Instance.AddCurrency(delta);
                else MetaCurrencyManager.Instance.SpendCurrency(Mathf.Abs(delta));
                _cachedSaveData.totalCurrency = MetaCurrencyManager.Instance.TotalCurrency;
            }
            else
            {
                _cachedSaveData.totalCurrency = Mathf.Max(0, _cachedSaveData.totalCurrency + delta);
            }
            SaveData();
        }

        private void SetExactCurrency(int value)
        {
            if (Application.isPlaying && MetaCurrencyManager.Instance != null)
            {
                MetaCurrencyManager.Instance.SetCurrency(value);
                _cachedSaveData.totalCurrency = MetaCurrencyManager.Instance.TotalCurrency;
            }
            else
            {
                _cachedSaveData.totalCurrency = Mathf.Max(0, value);
            }
            SaveData();
        }

        private void DrawHeroesSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("🧙 QUYỀN SỞ HỮU & THẺ MẢNH ANH HÙNG (HERO PROGRESSION +/-)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Tướng đạt từ 1★ trở lên được tính là ĐÃ CHIÊU MỘ. 0★ là Chưa Sở Hữu (có thể gộp đủ mảnh để thức tỉnh).", MessageType.None);

            if (_allHeroes.Count == 0)
            {
                EditorGUILayout.HelpBox("Không tìm thấy CharacterDataSO trong project.", MessageType.Warning);
            }

            foreach (var hero in _allHeroes)
            {
                if (hero == null) continue;
                string heroId = hero.characterId;
                int currentShards = _cachedSaveData.GetCharacterShards(heroId);
                int currentStar = _cachedSaveData.GetCharacterStarLevel(heroId);
                bool isUnlocked = currentStar >= 1 || heroId == "default" || heroId == "C001_ThuSinh";

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                
                string starLabel = currentStar == 0 ? "<color=#888888>[Chưa Sở Hữu - 0★]</color>" : $"<color=#FFD700><b>[{currentStar}★]</b></color>";
                string rarityTag = $"<color={hero.rarity.GetHexColor()}>[{hero.rarity.GetDisplayName()}]</color>";
                EditorGUILayout.LabelField($"[{heroId}] {rarityTag} <b>{hero.characterName}</b> ({hero.element}) {starLabel}", new GUIStyle(EditorStyles.label) { richText = true }, GUILayout.Width(340));

                EditorGUILayout.LabelField($"Mảnh: <b>{currentShards}</b>", new GUIStyle(EditorStyles.label) { richText = true }, GUILayout.Width(70));

                // Nút +/- Thẻ Mảnh Tướng
                if (GUILayout.Button($"-10", GUILayout.Width(45)))
                {
                    ModifyHeroShards(heroId, -10);
                }
                if (GUILayout.Button($"+10", GUILayout.Width(45)))
                {
                    ModifyHeroShards(heroId, 10);
                }

                // Nút +/- Cấp Sao
                if (GUILayout.Button("★ -", GUILayout.Width(40)))
                {
                    SetHeroStar(heroId, Mathf.Max(0, currentStar - 1));
                }
                if (GUILayout.Button("★ +", GUILayout.Width(40)))
                {
                    SetHeroStar(heroId, Mathf.Min(5, currentStar + 1));
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("⭐ Mở Khóa Tất Cả Tướng (1★)"))
            {
                foreach (var h in _allHeroes)
                {
                    if (h != null) SetHeroStar(h.characterId, 1);
                }
            }
            if (GUILayout.Button("⭐⭐⭐⭐⭐ Max Toàn Bộ Tướng (5★)"))
            {
                foreach (var h in _allHeroes)
                {
                    if (h != null) SetHeroStar(h.characterId, 5);
                }
            }
            if (GUILayout.Button("+20 Mảnh Cho Tất Cả Tướng"))
            {
                foreach (var h in _allHeroes)
                {
                    if (h != null) ModifyHeroShards(h.characterId, 20);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void ModifyHeroShards(string heroId, int delta)
        {
            if (Application.isPlaying && CharacterProgressionManager.Instance != null)
            {
                if (delta > 0) CharacterProgressionManager.Instance.AddCharacterShards(heroId, delta);
                else
                {
                    int cur = _cachedSaveData.GetCharacterShards(heroId);
                    int star = _cachedSaveData.GetCharacterStarLevel(heroId);
                    _cachedSaveData.SetCharacterProgress(heroId, Mathf.Max(0, cur + delta), star);
                }
            }
            else
            {
                int cur = _cachedSaveData.GetCharacterShards(heroId);
                int star = _cachedSaveData.GetCharacterStarLevel(heroId);
                _cachedSaveData.SetCharacterProgress(heroId, Mathf.Max(0, cur + delta), star);
            }
            SaveData();
        }

        private void SetHeroStar(string heroId, int star)
        {
            star = Mathf.Clamp(star, 0, 5);
            int curShards = _cachedSaveData.GetCharacterShards(heroId);
            _cachedSaveData.SetCharacterProgress(heroId, curShards, star);

            var list = new List<string>(_cachedSaveData.unlockedCharacters ?? new string[0]);
            if (star >= 1 && !list.Contains(heroId)) list.Add(heroId);
            else if (star == 0 && list.Contains(heroId) && heroId != "default" && heroId != "C001_ThuSinh") list.Remove(heroId);
            _cachedSaveData.unlockedCharacters = list.ToArray();

            SaveData();
        }

        private void DrawRelicsSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("🏮 QUẢN LÝ PHÁP BẢO: THẺ MẢNH & CẤP SAO (+ / -)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Pháp bảo đạt từ 1★ trở lên sẽ được tính là ĐÃ MỞ KHÓA và cho phép trang bị xuất trận. 0★ là Chưa Sở Hữu.", MessageType.None);

            EditorGUILayout.BeginHorizontal();
            _searchWeaponText = EditorGUILayout.TextField("Tìm Pháp Bảo:", _searchWeaponText);
            _customShardInput = EditorGUILayout.IntField("Số thẻ muốn (+/-):", _customShardInput, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);

            // Nút bấm tiện ích toàn bộ
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("⭐ Mở Khóa Tất Cả 1★"))
            {
                foreach (var w in _allWeapons)
                {
                    if (w != null) SetRelicStar(w.weaponId, 1);
                }
            }
            if (GUILayout.Button("⭐⭐⭐⭐⭐ Max Toàn Bộ 5★"))
            {
                foreach (var w in _allWeapons)
                {
                    if (w != null) SetRelicStar(w.weaponId, 5);
                }
            }
            if (GUILayout.Button("🔒 Khóa Hết Về 0★ (Trừ Kiếm Trúc)"))
            {
                foreach (var w in _allWeapons)
                {
                    if (w != null)
                    {
                        int star = w.weaponId == "wp_kiem_truc" ? 1 : 0;
                        if (Application.isPlaying && RelicInventoryManager.Instance != null)
                        {
                            RelicInventoryManager.Instance.SetRelicShards(w.weaponId, 0);
                            RelicInventoryManager.Instance.SetRelicStarLevel(w.weaponId, star);
                        }
                        else
                        {
                            SetRelicProgressDirect(w.weaponId, 0, star);
                        }
                    }
                }
                SaveData();
            }
            if (GUILayout.Button($"+{_customShardInput} Thẻ Cho Tất Cả"))
            {
                foreach (var w in _allWeapons)
                {
                    if (w != null) ModifyRelicShards(w.weaponId, _customShardInput);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            // Danh sách từng Pháp Bảo
            foreach (var weapon in _allWeapons)
            {
                if (weapon == null) continue;
                if (!string.IsNullOrEmpty(_searchWeaponText) && 
                    !weapon.weaponName.ToLower().Contains(_searchWeaponText.ToLower()) && 
                    !weapon.weaponId.ToLower().Contains(_searchWeaponText.ToLower()))
                {
                    continue;
                }

                string wid = weapon.weaponId;
                int currentShards = _cachedSaveData.GetRelicShards(wid);
                int currentStar = _cachedSaveData.GetRelicStarLevel(wid);
                bool isUnlocked = currentStar >= 1;

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();

                string starLabel = currentStar == 0 ? "<color=#888888>[Chưa Sở Hữu - 0★]</color>" : $"<color=#FFD700><b>[{currentStar}★]</b></color>";
                EditorGUILayout.LabelField($"[{wid}] <b>{weapon.weaponName}</b> (Hệ {weapon.elementType}) {starLabel}", new GUIStyle(EditorStyles.label) { richText = true }, GUILayout.Width(340));

                EditorGUILayout.LabelField($"Thẻ: <b>{currentShards}</b>", new GUIStyle(EditorStyles.label) { richText = true }, GUILayout.Width(70));

                // Nút +/- Thẻ Mảnh
                if (GUILayout.Button($"-{_customShardInput}", GUILayout.Width(45)))
                {
                    ModifyRelicShards(wid, -_customShardInput);
                }
                if (GUILayout.Button($"+{_customShardInput}", GUILayout.Width(45)))
                {
                    ModifyRelicShards(wid, _customShardInput);
                }
                if (GUILayout.Button("+10", GUILayout.Width(40)))
                {
                    ModifyRelicShards(wid, 10);
                }

                // Chỉnh Cấp Sao
                EditorGUILayout.LabelField("Sao:", GUILayout.Width(35));
                if (GUILayout.Button("0★", GUILayout.Width(32))) SetRelicStar(wid, 0);
                if (GUILayout.Button("1★", GUILayout.Width(32))) SetRelicStar(wid, 1);
                if (GUILayout.Button("2★", GUILayout.Width(32))) SetRelicStar(wid, 2);
                if (GUILayout.Button("3★", GUILayout.Width(32))) SetRelicStar(wid, 3);
                if (GUILayout.Button("4★", GUILayout.Width(32))) SetRelicStar(wid, 4);
                if (GUILayout.Button("5★", GUILayout.Width(32))) SetRelicStar(wid, 5);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        private void ModifyRelicShards(string wid, int delta)
        {
            if (Application.isPlaying && RelicInventoryManager.Instance != null)
            {
                RelicInventoryManager.Instance.AddRelicShards(wid, delta);
                _cachedSaveData.SetRelicProgress(wid, RelicInventoryManager.Instance.GetRelicShardCount(wid), RelicInventoryManager.Instance.GetRelicStarLevel(wid));
            }
            else
            {
                int cur = _cachedSaveData.GetRelicShards(wid);
                int star = _cachedSaveData.GetRelicStarLevel(wid);
                SetRelicProgressDirect(wid, cur + delta, star);
            }
            SaveData();
        }

        private void SetRelicStar(string wid, int targetStar)
        {
            if (Application.isPlaying && RelicInventoryManager.Instance != null)
            {
                RelicInventoryManager.Instance.SetRelicStarLevel(wid, targetStar);
                _cachedSaveData.SetRelicProgress(wid, RelicInventoryManager.Instance.GetRelicShardCount(wid), targetStar);
            }
            else
            {
                int cur = _cachedSaveData.GetRelicShards(wid);
                SetRelicProgressDirect(wid, cur, targetStar);
            }
            SaveData();
        }

        private void SetRelicProgressDirect(string wid, int shards, int star)
        {
            _cachedSaveData.SetRelicProgress(wid, shards, star);
        }
    }
}
#endif
