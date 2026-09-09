using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Core.Save;

namespace ProjectZombie.Features.DebugUI
{
    /// <summary>
    /// HUD Debug hiển thị toàn bộ dữ liệu lưu trữ phần cứng và trạng thái Loadout của Player ngoài màn hình game.
    /// Cho phép bật/tắt nhanh bằng phím F1 / F3 hoặc chạm 3 ngón tay trên màn hình Android.
    /// </summary>
    public class RuntimeSaveDataOverlay : MonoBehaviour
    {
        public static RuntimeSaveDataOverlay Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool _showOverlay = true;
        [SerializeField] private int _fontSize = 13;

        private GUIStyle _boxStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _valueStyle;
        private GUIStyle _btnStyle;

        private bool _isExpanded = true;
        private Vector2 _scrollPos;

        private MetaProgressionSaveData _cachedMetaData;
        private float _lastDataRefreshTime = 0f;
        private const float REFRESH_INTERVAL = 1.0f; // Cập nhật từ ổ đĩa mỗi 1 giây

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance == null)
            {
                var go = new GameObject("[Debug] RuntimeSaveDataOverlay");
                go.AddComponent<RuntimeSaveDataOverlay>();
                DontDestroyOnLoad(go);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            RefreshDataFromDisk();
        }

        private void RefreshDataFromDisk()
        {
            _cachedMetaData = SaveSystem.Load();
            _lastDataRefreshTime = Time.unscaledTime;
        }

        private void Update()
        {
            if (Time.unscaledTime - _lastDataRefreshTime >= REFRESH_INTERVAL)
            {
                _cachedMetaData = SaveSystem.Load();
                _lastDataRefreshTime = Time.unscaledTime;
            }
#if ENABLE_INPUT_SYSTEM
            // Bật/tắt bằng phím tắt F1 / F3 trên New Input System
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.f3Key.wasPressedThisFrame || 
                    UnityEngine.InputSystem.Keyboard.current.f1Key.wasPressedThisFrame)
                {
                    _showOverlay = !_showOverlay;
                }
            }

            // Bật/tắt bằng chạm 3 ngón tay trên Touchscreen Android (New Input System)
            if (UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.enabled)
            {
                var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
                if (touches.Count >= 3 && touches[0].began)
                {
                    _showOverlay = !_showOverlay;
                }
            }
            else if (UnityEngine.InputSystem.Touchscreen.current != null)
            {
                var touches = UnityEngine.InputSystem.Touchscreen.current.touches;
                int activeCount = 0;
                for (int i = 0; i < touches.Count; i++)
                {
                    if (touches[i].press.isPressed) activeCount++;
                }
                if (activeCount >= 3)
                {
                    _showOverlay = !_showOverlay;
                }
            }
#else
            // Fallback Legacy Input
            if (Input.GetKeyDown(KeyCode.F3) || Input.GetKeyDown(KeyCode.F1))
            {
                _showOverlay = !_showOverlay;
            }

            if (Input.touchCount >= 3 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                _showOverlay = !_showOverlay;
            }
#endif
        }

        private void InitStyles()
        {
            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box);
                _boxStyle.normal.background = MakeTex(2, 2, new Color(0.06f, 0.05f, 0.08f, 0.92f));
                _boxStyle.padding = new RectOffset(10, 10, 10, 10);
            }

            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = _fontSize + 3,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
                _headerStyle.normal.textColor = new Color(0.98f, 0.88f, 0.45f);
            }

            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = _fontSize,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
                _labelStyle.normal.textColor = new Color(0.85f, 0.80f, 0.70f);
            }

            if (_valueStyle == null)
            {
                _valueStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = _fontSize,
                    alignment = TextAnchor.MiddleLeft
                };
                _valueStyle.normal.textColor = new Color(0.40f, 0.95f, 0.65f);
            }

            if (_btnStyle == null)
            {
                _btnStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = _fontSize - 1,
                    fontStyle = FontStyle.Bold
                };
            }
        }

        private void OnGUI()
        {
            if (!_showOverlay) return;

            InitStyles();

            float screenWidth = Screen.width;
            float panelWidth = Mathf.Min(380, screenWidth * 0.4f);
            float panelHeight = _isExpanded ? Mathf.Min(560, Screen.height * 0.85f) : 48;

            Rect panelRect = new Rect(16, 16, panelWidth, panelHeight);

            GUILayout.BeginArea(panelRect, _boxStyle);
            {
                // Top Header Bar
                GUILayout.BeginHorizontal();
                GUILayout.Label("💾 DỮ LIỆU LƯU TRỮ (SAVE DATA)", _headerStyle);
                if (GUILayout.Button(_isExpanded ? "▲ Thu Gọn" : "▼ Mở Rộng", _btnStyle, GUILayout.Width(80), GUILayout.Height(24)))
                {
                    _isExpanded = !_isExpanded;
                }
                GUILayout.EndHorizontal();

                if (_isExpanded)
                {
                    GUILayout.Space(6);
                    _scrollPos = GUILayout.BeginScrollView(_scrollPos);
                    {
                        // 1. DỮ LIỆU LOADOUT XUẤT TRẬN (PlayerPrefs)
                        DrawSectionHeader("1. LOADOUT XUẤT TRẬN (PlayerPrefs)");

                        CharacterEntry hero = RunLoadoutState.SelectedCharacter;
                        string heroName = hero != null ? $"{hero.characterName} (Hệ {hero.element})" : "<color=red>Chưa Chọn</color>";
                        DrawDataRow("Tướng Xuất Trận:", heroName);

                        WeaponData pri = RunLoadoutState.SelectedPrimaryWeapon;
                        string priName = pri != null ? pri.weaponName : (hero != null && hero.defaultPrimaryWeapon != null ? $"{hero.defaultPrimaryWeapon.weaponName} (Mặc định)" : "Đòn Đánh Bản Thể");
                        DrawDataRow("Vũ Khí Chính:", priName);

                        List<WeaponData> relics = RunLoadoutState.SelectedRelics;
                        if (relics != null && relics.Count > 0)
                        {
                            for (int i = 0; i < relics.Count; i++)
                            {
                                if (relics[i] != null)
                                {
                                    string role = relics[i].weaponRole == WeaponRole.PrimaryWeapon ? "Vũ Khí" : "Pháp Bảo";
                                    DrawDataRow($"Pháp Bảo #{i + 1}:", $"{relics[i].weaponName} [{role}]");
                                }
                            }
                        }
                        else
                        {
                            DrawDataRow("Pháp Bảo:", "<color=#FFA500>Chưa mang theo</color>");
                        }

                        GUILayout.Space(8);

                        // 2. DỮ LIỆU TIẾN TRÌNH VĨNH VIỄN (player_save.json)
                        DrawSectionHeader("2. TIẾN TRÌNH VĨNH VIỄN (JSON Storage)");

                        var metaData = _cachedMetaData;
                        if (metaData != null)
                        {
                            DrawDataRow("Cổ Tiền Tích Lũy:", $"{metaData.totalCurrency:N0} Đồng");
                            DrawDataRow("Tổng Số Run Đã Chơi:", $"{metaData.totalRunsPlayed} Lần");
                            DrawDataRow("Thời Gian Run Tốt Nhất:", $"{metaData.bestRunTime:F1} Giây");
                            DrawDataRow("Số Kill Cao Nhất:", $"{metaData.bestKillCount} Yêu Quái");

                            int unlockedHeroCount = metaData.unlockedCharacters != null ? metaData.unlockedCharacters.Length : 1;
                            DrawDataRow("Tướng Đã Mở Khóa:", $"{unlockedHeroCount} Nhân Vật");

                            int activeNodes = 0;
                            if (metaData.upgradeNodeLevels != null)
                            {
                                foreach (var lvl in metaData.upgradeNodeLevels) if (lvl > 0) activeNodes++;
                            }
                            DrawDataRow("Điểm Miếu Cổ Đã Tăng:", $"{activeNodes} Nút Thiên Phú");

                            int relicCount = metaData.relicProgressList != null ? metaData.relicProgressList.Count : 0;
                            DrawDataRow("Pháp Bảo Đã Có Thẻ/Sao:", $"{relicCount} Pháp Bảo");
                            if (metaData.relicProgressList != null && metaData.relicProgressList.Count > 0)
                            {
                                for (int r = 0; r < metaData.relicProgressList.Count; r++)
                                {
                                    var entry = metaData.relicProgressList[r];
                                    string starStr = entry.starLevel > 0 ? $"{entry.starLevel} Sao" : "0 Sao (Khóa)";
                                    DrawDataRow($"  • {entry.relicId}:", $"{starStr} | {entry.shardCount} Thẻ");
                                }
                            }
                        }
                        else
                        {
                            DrawDataRow("Trạng Thái File:", "<color=red>Chưa Tìm Thấy Save File</color>");
                        }

                        GUILayout.Space(8);

                        // 3. ĐƯỜNG DẪN BỘ NHỚ THIẾT BỊ
                        DrawSectionHeader("3. VỊ TRÍ LƯU PHẦN CỨNG");
                        GUILayout.Label($"<b>persistentDataPath:</b>\n<color=#A0C0E0>{Application.persistentDataPath}</color>", _labelStyle);

                        GUILayout.Space(10);

                        // 4. CÁC NÚT THAO TÁC NHANH
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("🔄 Nạp Lại", _btnStyle, GUILayout.Height(28)))
                        {
                            RunLoadoutState.LoadFromSaveOrDefaults();
                            RefreshDataFromDisk();
                        }
                        if (GUILayout.Button("🗑️ Xóa Save File", _btnStyle, GUILayout.Height(28)))
                        {
                            SaveSystem.DeleteSave();
                            RunLoadoutState.ResetLoadout();
                            RefreshDataFromDisk();
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndScrollView();
                }
            }
            GUILayout.EndArea();
        }

        private void DrawSectionHeader(string title)
        {
            GUILayout.Label($"<b><color=#FFD700>── {title} ──</color></b>", _labelStyle);
        }

        private void DrawDataRow(string label, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, _labelStyle, GUILayout.Width(140));
            GUILayout.Label(value, _valueStyle);
            GUILayout.EndHorizontal();
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
