// ============================================================================
// FILE: GameManager.cs
// VỊ TRÍ: Assets/Scripts/GameManager.cs (trong Unity Project)
// MÔ TẢ: Script quản lý chính cho luồng Save/Load game thông qua JS Bridge.
//         Nhận dữ liệu save cũ từ ReactJS khi khởi tạo, gửi dữ liệu mới
//         ra ngoài web khi trigger save. Tích hợp checksum SHA-256 cho
//         anti-cheat khi submit điểm cao lên Leaderboard.
// GHI CHÚ: Gắn script này vào một GameObject tên "GameManager" trong scene.
// ============================================================================

using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ========================================================================
    // SINGLETON
    // ========================================================================
    public static GameManager Instance { get; private set; }

    // ========================================================================
    // IMPORT CÁC HÀM TỪ JSLIB (Cầu nối JS Bridge)
    // Các hàm này chỉ hoạt động khi build WebGL, không dùng được trong Editor
    // ========================================================================
    [DllImport("__Internal")]
    private static extern void NotifyGameReady();

    [DllImport("__Internal")]
    private static extern void SaveGameToWeb(string jsonDataStr, int level, long score);

    [DllImport("__Internal")]
    private static extern void SubmitHighScoreToWeb(long score, string checksumStr);

    [DllImport("__Internal")]
    private static extern void RequestLeaderboardFromWeb(int topN);

    // ========================================================================
    // CẤU HÌNH
    // ========================================================================
    
    /// <summary>
    /// Khóa muối bí mật dùng để sinh checksum SHA-256.
    /// QUAN TRỌNG: Giá trị này phải KHỚP CHÍNH XÁC với biến môi trường 
    /// GAME_SECRET_SALT trên Supabase Edge Function.
    /// Khi compile sang WebAssembly (.wasm), giá trị này sẽ được mã hóa nhị phân,
    /// khó bị trích xuất hơn so với Javascript thuần.
    /// </summary>
    [SerializeField]
    private string secretSalt = "YOUR_SECRET_SALT_HERE_CHANGE_ME";

    /// <summary>
    /// Thời gian giữa mỗi lần auto-save (giây). Mặc định: 120 giây (2 phút).
    /// </summary>
    [SerializeField]
    private float autoSaveInterval = 120f;

    /// <summary>
    /// ID của game hiện tại (sẽ được truyền từ ReactJS qua SendMessage).
    /// </summary>
    private int gameId = -1;

    /// <summary>
    /// User ID (UUID) của người chơi (truyền từ ReactJS).
    /// </summary>
    private string userId = "";

    // ========================================================================
    // TRẠNG THÁI GAME
    // ========================================================================
    
    /// <summary>
    /// Dữ liệu trạng thái game hiện tại.
    /// </summary>
    public PlayerSaveData CurrentSaveData { get; private set; }

    /// <summary>
    /// Điểm số hiện tại trong phiên chơi.
    /// </summary>
    public long CurrentScore { get; set; } = 0;

    /// <summary>
    /// Màn chơi hiện tại.
    /// </summary>
    public int CurrentLevel { get; set; } = 1;

    /// <summary>
    /// Cờ đánh dấu game đã sẵn sàng (đã nhận được save data từ web).
    /// </summary>
    private bool isGameReady = false;

    /// <summary>
    /// Bộ đếm thời gian auto-save.
    /// </summary>
    private float autoSaveTimer = 0f;

    // ========================================================================
    // UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        // Singleton Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Khởi tạo save data mặc định
        CurrentSaveData = new PlayerSaveData();
    }

    private void Start()
    {
        #if !UNITY_EDITOR && UNITY_WEBGL
        // Thông báo cho ReactJS rằng Unity đã load xong, sẵn sàng nhận save data
        Debug.Log("[GameManager] Gửi tín hiệu GameReady tới ReactJS...");
        NotifyGameReady();
        #else
        Debug.Log("[GameManager] Đang chạy trong Editor - Bỏ qua JS Bridge. Khởi tạo dữ liệu mặc định.");
        isGameReady = true;
        #endif
    }

    private void Update()
    {
        if (!isGameReady) return;

        // Auto-save định kỳ
        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= autoSaveInterval)
        {
            autoSaveTimer = 0f;
            TriggerSaveProgress("Auto-save");
        }
    }

    private void OnApplicationQuit()
    {
        // Lưu game khi người chơi thoát (đóng tab trình duyệt)
        TriggerSaveProgress("OnQuit");
    }

    // ========================================================================
    // NHẬN DỮ LIỆU TỪ REACTJS (Gọi bởi SendMessage từ web)
    // ========================================================================

    /// <summary>
    /// Hàm được ReactJS gọi thông qua unityInstance.SendMessage("GameManager", "SetGameConfig", json)
    /// để truyền thông tin cấu hình game (game_id, user_id).
    /// </summary>
    public void SetGameConfig(string configJson)
    {
        try
        {
            GameConfig config = JsonUtility.FromJson<GameConfig>(configJson);
            gameId = config.game_id;
            userId = config.user_id;
            Debug.Log($"[GameManager] Nhận config - GameID: {gameId}, UserID: {userId}");
        }
        catch (Exception ex)
        {
            Debug.LogError("[GameManager] Lỗi parse GameConfig: " + ex.Message);
        }
    }

    /// <summary>
    /// Hàm được ReactJS gọi thông qua unityInstance.SendMessage("GameManager", "LoadSaveData", json)
    /// để truyền dữ liệu save cũ vào game khi khởi tạo.
    /// </summary>
    public void LoadSaveData(string jsonData)
    {
        Debug.Log("[GameManager] Nhận dữ liệu save từ ReactJS: " + jsonData);

        if (string.IsNullOrEmpty(jsonData) || jsonData == "{}")
        {
            Debug.Log("[GameManager] Không có save cũ. Khởi tạo game mới với dữ liệu mặc định.");
            CurrentSaveData = new PlayerSaveData();
        }
        else
        {
            try
            {
                CurrentSaveData = JsonUtility.FromJson<PlayerSaveData>(jsonData);
                CurrentLevel = CurrentSaveData.current_level;
                CurrentScore = CurrentSaveData.high_score;
                Debug.Log($"[GameManager] Khôi phục thành công - Level: {CurrentLevel}, HP: {CurrentSaveData.health}");
            }
            catch (Exception ex)
            {
                Debug.LogError("[GameManager] Lỗi parse save data, khởi tạo mặc định: " + ex.Message);
                CurrentSaveData = new PlayerSaveData();
            }
        }

        isGameReady = true;

        // Callback cho các hệ thống khác biết game đã sẵn sàng
        OnGameReady?.Invoke();
    }

    /// <summary>
    /// Hàm nhận kết quả BXH từ ReactJS (nếu có yêu cầu).
    /// </summary>
    public void OnLeaderboardReceived(string leaderboardJson)
    {
        Debug.Log("[GameManager] Nhận dữ liệu Leaderboard: " + leaderboardJson);
        OnLeaderboardLoaded?.Invoke(leaderboardJson);
    }

    // ========================================================================
    // GỬI DỮ LIỆU RA REACTJS (Qua JS Bridge)
    // ========================================================================

    /// <summary>
    /// Trigger lưu tiến trình game hiện tại lên cloud thông qua ReactJS.
    /// Gọi hàm này khi: qua màn, nhặt vật phẩm quan trọng, hoặc auto-save.
    /// </summary>
    public void TriggerSaveProgress(string reason = "Manual")
    {
        if (!isGameReady) return;

        // Cập nhật save data với trạng thái hiện tại
        CurrentSaveData.current_level = CurrentLevel;
        CurrentSaveData.high_score = CurrentScore;
        // Các trường khác (health, gold, inventory...) được cập nhật trực tiếp 
        // bởi các hệ thống game khác thông qua CurrentSaveData

        string json = JsonUtility.ToJson(CurrentSaveData);

        Debug.Log($"[GameManager] Lưu tiến trình ({reason}): Level={CurrentLevel}, Score={CurrentScore}");

        #if !UNITY_EDITOR && UNITY_WEBGL
        SaveGameToWeb(json, CurrentLevel, CurrentScore);
        #else
        Debug.Log("[GameManager] Editor mode - Save data: " + json);
        PlayerPrefs.SetString("debug_save_data", json);
        PlayerPrefs.Save();
        #endif
    }

    /// <summary>
    /// Submit điểm cao lên Leaderboard kèm checksum anti-cheat.
    /// Chỉ gọi khi có sự kiện xác thực được điểm (VD: hoàn thành màn chơi).
    /// </summary>
    public void SubmitHighScore(long score)
    {
        if (string.IsNullOrEmpty(userId) || gameId < 0)
        {
            Debug.LogWarning("[GameManager] Chưa có user_id hoặc game_id. Bỏ qua submit score.");
            return;
        }

        string checksum = GenerateChecksum(userId, gameId, score);

        Debug.Log($"[GameManager] Submit điểm cao: {score}, Checksum: {checksum.Substring(0, 16)}...");

        #if !UNITY_EDITOR && UNITY_WEBGL
        SubmitHighScoreToWeb(score, checksum);
        #else
        Debug.Log($"[GameManager] Editor mode - Score: {score}, Checksum: {checksum}");
        #endif
    }

    /// <summary>
    /// Yêu cầu lấy BXH từ web (kết quả trả về qua OnLeaderboardReceived).
    /// </summary>
    public void FetchLeaderboard(int topN = 10)
    {
        #if !UNITY_EDITOR && UNITY_WEBGL
        RequestLeaderboardFromWeb(topN);
        #else
        Debug.Log($"[GameManager] Editor mode - Yêu cầu Leaderboard top {topN}");
        #endif
    }

    // ========================================================================
    // SINH MÃ CHECKSUM SHA-256 (ANTI-CHEAT)
    // ========================================================================

    /// <summary>
    /// Tạo chuỗi hash SHA-256 từ user_id, game_id, score và secret_salt.
    /// Công thức: SHA256("userId:gameId:score:secretSalt")
    /// Kết quả phải khớp với kết quả tính trên Edge Function.
    /// </summary>
    private string GenerateChecksum(string uid, int gid, long score)
    {
        string rawString = $"{uid}:{gid}:{score}:{secretSalt}";

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(rawString);
            byte[] hash = sha256.ComputeHash(bytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }

    // ========================================================================
    // EVENTS
    // ========================================================================
    
    /// <summary>Event phát ra khi game đã sẵn sàng (đã nhận save data).</summary>
    public event Action OnGameReady;

    /// <summary>Event phát ra khi nhận được dữ liệu Leaderboard từ web.</summary>
    public event Action<string> OnLeaderboardLoaded;
}

// ============================================================================
// CÁC LỚP DỮ LIỆU (DATA CLASSES)
// ============================================================================

/// <summary>
/// Cấu hình game nhận từ ReactJS khi khởi tạo.
/// </summary>
[Serializable]
public class GameConfig
{
    public int game_id;
    public string user_id;
}

/// <summary>
/// Cấu trúc dữ liệu save game.
/// TÙY BIẾN: Thêm/sửa các trường theo nhu cầu của từng game cụ thể.
/// LƯU Ý: Giữ tổng dung lượng JSON < 512KB (theo giới hạn trong bảng games.max_save_size_kb).
/// </summary>
[Serializable]
public class PlayerSaveData
{
    // --- Thông tin tiến trình ---
    public int current_level = 1;
    public long high_score = 0;

    // --- Trạng thái nhân vật ---
    public int health = 100;
    public int max_health = 100;
    public int gold = 0;

    // --- Vị trí ---
    public float position_x = 0f;
    public float position_y = 0f;
    public float position_z = 0f;

    // --- Vật phẩm (Inventory) ---
    public string[] inventory = new string[0];

    // --- Kỹ năng đã mở khóa ---
    public string[] unlocked_skills = new string[0];

    // --- Các cờ trạng thái game (VD: đã xem cutscene, đã mở cổng...) ---
    public string[] flags = new string[0];

    // --- Thời gian chơi tích lũy (giây) ---
    public float total_play_time = 0f;
}
