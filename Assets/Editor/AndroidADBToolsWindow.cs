#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Bảng điều khiển ADB Debug & Device Control Tool cho Unity Editor.
    /// Giúp theo dõi Logcat realtime, đo hiệu năng/FPS, chụp màn hình, hack save data từ xa trên thiết bị thật.
    /// Menu: ⚡ CHEAT & SAVE DATA ⚡ > 📱 ADB Device & Runtime Debugger (+/-)
    /// </summary>
    public class AndroidADBToolsWindow : EditorWindow
    {
        [MenuItem("⚡ CHEAT & SAVE DATA ⚡/📱 ADB Device & Runtime Debugger (+/-) %F2", priority = 1)]
        public static void ShowWindowTop()
        {
            OpenWindow();
        }

        [MenuItem("Tools/ProjectZombie/📱 Android ADB Test & Remote Control Tool", priority = 2)]
        public static void ShowWindow()
        {
            OpenWindow();
        }

        private static void OpenWindow()
        {
            var window = GetWindow<AndroidADBToolsWindow>("📱 ADB DEVICE DEBUGGER");
            window.minSize = new Vector2(720, 780);
            window.RefreshDeviceInfo();
            window.Show();
        }

        private const string PACKAGE_NAME = "com.thuongtran.southernmyth";
        private const string REMOTE_SAVE_PATH = "/sdcard/Android/data/com.thuongtran.southernmyth/files/player_save.json";

        private Vector2 _scrollPos;
        private Vector2 _logScrollPos;
        private string _deviceStatusText = "Chưa kết nối";
        private bool _isDeviceConnected = false;
        private string _connectedDeviceId = "";
        private string _batteryLevel = "---";
        private string _screenResolution = "---";
        
        // Log & Command output
        private string _consoleOutput = "";
        private string _logFilterKeyword = "Unity";
        private Texture2D _capturedScreenshot;
        private string _remoteSaveJsonContent = "";
        private int _addCurrencyAmount = 50000;

        private void OnEnable()
        {
            RefreshDeviceInfo();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            DrawHeader();
            EditorGUILayout.Space(6);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawDeviceStatusBar();
            EditorGUILayout.Space(10);

            if (!_isDeviceConnected)
            {
                EditorGUILayout.HelpBox("Không tìm thấy thiết bị Android nào qua ADB.\n1. Hãy cắm cáp USB vào máy tính.\n2. Bật 'USB Debugging' (Gỡ lỗi USB) trong Cài đặt nhà phát triển.\n3. Nhấn '🔄 Quét Lại Thiết Bị' bên trên.", MessageType.Warning);
                EditorGUILayout.EndScrollView();
                return;
            }

            DrawAppControlsSection();
            EditorGUILayout.Space(12);

            DrawSaveDataRemoteCheatSection();
            EditorGUILayout.Space(12);

            DrawPerformanceAndDiagnosticSection();
            EditorGUILayout.Space(12);

            DrawScreenCaptureAndMediaSection();
            EditorGUILayout.Space(12);

            DrawLiveLogcatSection();
            EditorGUILayout.Space(10);

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.2f, 0.85f, 1f) }
            };
            GUILayout.Label("📱 ANDROID ADB REMOTE CONTROL & LIVE DEBUGGER", headerStyle);
            EditorGUILayout.LabelField($"Target Package: <b><color=#FFD700>{PACKAGE_NAME}</color></b>", new GUIStyle(EditorStyles.centeredGreyMiniLabel) { richText = true });
            EditorGUILayout.EndVertical();
        }

        private void DrawDeviceStatusBar()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            GUIStyle statusStyle = new GUIStyle(EditorStyles.boldLabel) { richText = true };
            string statusIcon = _isDeviceConnected ? "🟢" : "🔴";
            EditorGUILayout.LabelField($"{statusIcon} Trạng Thái: <b>{_deviceStatusText}</b>", statusStyle, GUILayout.Width(350));

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("🔄 Quét Lại Thiết Bị (Refresh)", GUILayout.Height(26), GUILayout.Width(180)))
            {
                RefreshDeviceInfo();
            }
            EditorGUILayout.EndHorizontal();

            if (_isDeviceConnected)
            {
                EditorGUILayout.LabelField($"Thiết Bị: <b>{_connectedDeviceId}</b> | Pin: <b>{_batteryLevel}</b> | Màn hình: <b>{_screenResolution}</b>", new GUIStyle(EditorStyles.miniLabel) { richText = true });
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAppControlsSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("🎮 ĐIỀU KHIỂN & VÒNG ĐỜI ỨNG DỤNG (APP LIFECYCLE)", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.3f, 0.85f, 0.4f);
            if (GUILayout.Button("▶️ Mở Game", GUILayout.Height(30)))
            {
                RunAdbCommand($"shell monkey -p {PACKAGE_NAME} -c android.intent.category.LAUNCHER 1");
            }

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("⏹️ Đóng Game (Force Stop)", GUILayout.Height(30)))
            {
                RunAdbCommand($"shell am force-stop {PACKAGE_NAME}");
            }

            GUI.backgroundColor = new Color(1f, 0.75f, 0.2f);
            if (GUILayout.Button("🔄 Khởi Động Lại Game", GUILayout.Height(30)))
            {
                RunAdbCommand($"shell am force-stop {PACKAGE_NAME}");
                System.Threading.Thread.Sleep(300);
                RunAdbCommand($"shell monkey -p {PACKAGE_NAME} -c android.intent.category.LAUNCHER 1");
            }

            GUI.backgroundColor = Color.white;
            if (GUILayout.Button("🔙 Phím Back Cứng", GUILayout.Height(30)))
            {
                RunAdbCommand("shell input keyevent 4");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🌐 Tắt Mạng (Test Offline)"))
            {
                RunAdbCommand("shell svc wifi disable");
                RunAdbCommand("shell svc data disable");
            }
            if (GUILayout.Button("📶 Bật Mạng (Online)"))
            {
                RunAdbCommand("shell svc wifi enable");
                RunAdbCommand("shell svc data enable");
            }
            if (GUILayout.Button("💥 Xóa Toàn Bộ Data App (Clear Storage)"))
            {
                if (EditorUtility.DisplayDialog("Xóa sạch data app trên máy", "Hành động này sẽ xóa file save và cache game trên điện thoại như mới tải về từ Store. Bạn có chắc chắn không?", "Xóa", "Hủy"))
                {
                    RunAdbCommand($"shell pm clear {PACKAGE_NAME}");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawSaveDataRemoteCheatSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("💾 CHEAT SAVE DATA TRỰC TIẾP TRÊN THIẾT BỊ THẬT", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Đọc và ghi đè trực tiếp player_save.json vào điện thoại qua cáp ADB mà không cần build lại APK!", MessageType.None);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📥 Đọc Save Data Từ Điện Thoại", GUILayout.Height(28)))
            {
                _remoteSaveJsonContent = RunAdbCommandWithOutput($"shell cat {REMOTE_SAVE_PATH}");
            }

            _addCurrencyAmount = EditorGUILayout.IntField("Số tiền muốn nạp:", _addCurrencyAmount, GUILayout.Width(220));

            GUI.backgroundColor = new Color(0.2f, 0.9f, 0.4f);
            if (GUILayout.Button($"💰 Hack +{_addCurrencyAmount:N0} Cổ Tiền", GUILayout.Height(28)))
            {
                InjectRemoteCurrency(_addCurrencyAmount);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("⭐ Mở Khóa Full Tướng Trên Điện Thoại"))
            {
                InjectRemoteUnlockAllHeroes();
            }
            if (GUILayout.Button("🏮 Mở Khóa Full Pháp Bảo (5★)"))
            {
                InjectRemoteMaxAllWeapons();
            }
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_remoteSaveJsonContent))
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Nội dung player_save.json trên máy:", EditorStyles.miniBoldLabel);
                _remoteSaveJsonContent = EditorGUILayout.TextArea(_remoteSaveJsonContent, GUILayout.Height(100));
                
                if (GUILayout.Button("📤 Ghi Đè Nội Dung Này Lên Điện Thoại", GUILayout.Height(24)))
                {
                    PushSaveJsonToDevice(_remoteSaveJsonContent);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPerformanceAndDiagnosticSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("📊 ĐO HIỆU NĂNG & THÔNG SỐ KHUNG HÌNH (FPS / RAM / CPU)", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("⚡ Đo RAM & CPU Đang Dùng", GUILayout.Height(26)))
            {
                string mem = RunAdbCommandWithOutput($"shell dumpsys meminfo {PACKAGE_NAME}");
                _consoleOutput = $"[RAM MEMORY USAGE]\n{mem}";
            }
            if (GUILayout.Button("🎯 Kiểm Tra FPS & Frame Timing", GUILayout.Height(26)))
            {
                string gfx = RunAdbCommandWithOutput($"shell dumpsys gfxinfo {PACKAGE_NAME}");
                _consoleOutput = $"[GRAPHICS & FPS TIMING]\n{gfx}";
            }
            if (GUILayout.Button("📦 Xem Danh Sách File /files", GUILayout.Height(26)))
            {
                string files = RunAdbCommandWithOutput($"shell ls -la /sdcard/Android/data/{PACKAGE_NAME}/files");
                _consoleOutput = $"[STORAGE FILES]\n{files}";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawScreenCaptureAndMediaSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("📸 CHỤP MÀN HÌNH GAME TRỰC TIẾP TỪ THIẾT BỊ", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("📷 Chụp Màn Hình Ngay (Screenshot)", GUILayout.Height(28)))
            {
                CaptureDeviceScreenshot();
            }
            if (_capturedScreenshot != null && GUILayout.Button("📂 Mở Thư Mục Ảnh Đã Chụp", GUILayout.Height(28)))
            {
                string shotPath = Path.Combine(Application.dataPath, "../screenshot_adb.png");
                if (File.Exists(shotPath)) EditorUtility.RevealInFinder(shotPath);
            }
            EditorGUILayout.EndHorizontal();

            if (_capturedScreenshot != null)
            {
                EditorGUILayout.Space(6);
                float aspect = (float)_capturedScreenshot.width / _capturedScreenshot.height;
                float displayWidth = Mathf.Min(position.width - 40, 480);
                float displayHeight = displayWidth / aspect;
                
                Rect r = GUILayoutUtility.GetRect(displayWidth, displayHeight);
                GUI.DrawTexture(r, _capturedScreenshot, ScaleMode.ScaleToFit);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawLiveLogcatSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("📋 LOGCAT TERMINAL & KẾT QUẢ ADB", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _logFilterKeyword = EditorGUILayout.TextField("Lọc theo từ khóa:", _logFilterKeyword);
            if (GUILayout.Button("📜 Lấy 100 Dòng Log Mới Nhất", GUILayout.Width(200)))
            {
                string cmd = string.IsNullOrEmpty(_logFilterKeyword) 
                    ? "logcat -d -t 100" 
                    : $"logcat -d -t 100 -s {_logFilterKeyword}";
                _consoleOutput = RunAdbCommandWithOutput(cmd);
            }
            if (GUILayout.Button("🧹 Xóa Logcat Bộ Nhớ Đệm", GUILayout.Width(170)))
            {
                RunAdbCommand("logcat -c");
                _consoleOutput = "[ADB] Đã dọn sạch buffer Logcat.";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            _logScrollPos = EditorGUILayout.BeginScrollView(_logScrollPos, GUILayout.Height(180));
            EditorGUILayout.TextArea(_consoleOutput, EditorStyles.textArea);
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void RefreshDeviceInfo()
        {
            string outDevices = RunAdbCommandWithOutput("devices");
            string[] lines = outDevices.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            _isDeviceConnected = false;
            _connectedDeviceId = "";

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.EndsWith("device"))
                {
                    _connectedDeviceId = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    _isDeviceConnected = true;
                    break;
                }
            }

            if (_isDeviceConnected)
            {
                _deviceStatusText = $"Đã kết nối: {_connectedDeviceId}";
                string batt = RunAdbCommandWithOutput("shell dumpsys battery | grep level");
                _batteryLevel = batt.Replace("level:", "").Trim() + "%";
                string wm = RunAdbCommandWithOutput("shell wm size");
                _screenResolution = wm.Replace("Physical size:", "").Trim();
            }
            else
            {
                _deviceStatusText = "Không có thiết bị kết nối";
                _batteryLevel = "---";
                _screenResolution = "---";
            }
            Repaint();
        }

        private void CaptureDeviceScreenshot()
        {
            string tempRemote = "/sdcard/screen_tmp.png";
            string localPath = Path.Combine(Application.dataPath, "../screenshot_adb.png");

            RunAdbCommand($"shell screencap -p {tempRemote}");
            RunAdbCommand($"pull {tempRemote} \"{localPath}\"");
            RunAdbCommand($"shell rm {tempRemote}");

            if (File.Exists(localPath))
            {
                byte[] bytes = File.ReadAllBytes(localPath);
                if (_capturedScreenshot == null) _capturedScreenshot = new Texture2D(2, 2);
                _capturedScreenshot.LoadImage(bytes);
                ShowNotification(new GUIContent("✓ Đã chụp ảnh màn hình!"));
            }
        }

        private void InjectRemoteCurrency(int addAmount)
        {
            string json = RunAdbCommandWithOutput($"shell cat {REMOTE_SAVE_PATH}");
            if (string.IsNullOrEmpty(json) || !json.Contains("totalCurrency"))
            {
                ShowNotification(new GUIContent("Không tìm thấy file save trên máy!"));
                return;
            }

            try
            {
                var saveData = JsonUtility.FromJson<MetaProgressionSaveData>(json);
                if (saveData != null)
                {
                    saveData.totalCurrency += addAmount;
                    string updatedJson = JsonUtility.ToJson(saveData, true);
                    PushSaveJsonToDevice(updatedJson);
                    ShowNotification(new GUIContent($"✓ Đã cộng {addAmount:N0} Cổ Tiền!"));
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[AndroidADBTools] Lỗi inject tiền: {ex.Message}");
            }
        }

        private void InjectRemoteUnlockAllHeroes()
        {
            string json = RunAdbCommandWithOutput($"shell cat {REMOTE_SAVE_PATH}");
            if (string.IsNullOrEmpty(json)) return;

            try
            {
                var saveData = JsonUtility.FromJson<MetaProgressionSaveData>(json);
                if (saveData != null)
                {
                    string[] heroIds = new string[] { "default", "C001_ThuSinh", "C002_DaoSi", "C003_ThanhDong", "C004_AnSi" };
                    var uList = new List<string>(saveData.unlockedCharacters ?? new string[0]);
                    foreach (var id in heroIds)
                    {
                        if (!uList.Contains(id)) uList.Add(id);
                        saveData.SetCharacterProgress(id, 50, 5);
                    }
                    saveData.unlockedCharacters = uList.ToArray();
                    PushSaveJsonToDevice(JsonUtility.ToJson(saveData, true));
                    ShowNotification(new GUIContent("✓ Đã mở khóa Max 5★ toàn bộ Tướng trên máy!"));
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[AndroidADBTools] Lỗi inject Tướng: {ex.Message}");
            }
        }

        private void InjectRemoteMaxAllWeapons()
        {
            string json = RunAdbCommandWithOutput($"shell cat {REMOTE_SAVE_PATH}");
            if (string.IsNullOrEmpty(json)) return;

            try
            {
                var saveData = JsonUtility.FromJson<MetaProgressionSaveData>(json);
                if (saveData != null)
                {
                    string[] relicIds = new string[] {
                        "wp_kiem_truc", "wp_but_phan_quan", "wp_bua_tru_ta", "wp_chuoi_ho_phach",
                        "wp_trong_dong", "wp_luu_dan_than_sa", "wp_cung_bach_xa", "wp_dao_gam_dong",
                        "wp_truong_phap_su", "wp_linh_phu_ho_than", "wp_nuoc_thanh_cam_lo", "wp_phi_tieu_ngu_hanh"
                    };
                    foreach (var rid in relicIds)
                    {
                        saveData.SetRelicProgress(rid, 100, 5);
                    }
                    PushSaveJsonToDevice(JsonUtility.ToJson(saveData, true));
                    ShowNotification(new GUIContent("✓ Đã Max 5★ toàn bộ Pháp Bảo trên máy!"));
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[AndroidADBTools] Lỗi inject Pháp Bảo: {ex.Message}");
            }
        }

        private void PushSaveJsonToDevice(string jsonContent)
        {
            string tempLocal = Path.Combine(Application.dataPath, "../temp_save_push.json");
            File.WriteAllText(tempLocal, jsonContent, Encoding.UTF8);

            RunAdbCommand($"push \"{tempLocal}\" {REMOTE_SAVE_PATH}");
            if (File.Exists(tempLocal)) File.Delete(tempLocal);

            _remoteSaveJsonContent = jsonContent;
        }

        private string RunAdbCommandWithOutput(string arguments)
        {
            try
            {
                var processInfo = new ProcessStartInfo("adb", arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8
                };

                using (var process = Process.Start(processInfo))
                {
                    if (process == null) return "Không thể khởi động tiến trình ADB.";
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit(5000);

                    return !string.IsNullOrEmpty(output) ? output.Trim() : error.Trim();
                }
            }
            catch (Exception ex)
            {
                return $"[Lỗi thực thi ADB] {ex.Message}";
            }
        }

        private void RunAdbCommand(string arguments)
        {
            try
            {
                var processInfo = new ProcessStartInfo("adb", arguments)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using (var process = Process.Start(processInfo))
                {
                    process?.WaitForExit(3000);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[AndroidADBTools] Lỗi gọi lệnh: {ex.Message}");
            }
        }
    }
}
#endif
