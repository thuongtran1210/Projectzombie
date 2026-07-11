// ============================================================================
// FILE: SupabaseBridge.jslib
// VỊ TRÍ: Assets/Plugins/WebGL/SupabaseBridge.jslib (trong Unity Project)
// MÔ TẢ: Cầu nối Javascript cho phép Unity WebGL giao tiếp hai chiều với
//         ReactJS Web App. Mọi tương tác mạng (save/load) đều đi qua đây
//         thay vì gọi trực tiếp API từ Unity.
// ============================================================================

mergeInto(LibraryManager.library, {

  // -------------------------------------------------------------------------
  // HÀM 1: NotifyGameReady
  // Gọi khi Unity đã load xong scene và sẵn sàng nhận dữ liệu save cũ.
  // ReactJS sẽ phản hồi bằng cách gọi SendMessage("GameManager", "LoadSaveData", json)
  // -------------------------------------------------------------------------
  NotifyGameReady: function () {
    if (typeof window.dispatchReactEvent === 'function') {
      window.dispatchReactEvent("GameReady");
    } else {
      console.warn("[SupabaseBridge] window.dispatchReactEvent chưa được đăng ký bởi ReactJS.");
    }
  },

  // -------------------------------------------------------------------------
  // HÀM 2: SaveGameToWeb
  // Gửi dữ liệu save game từ Unity ra ReactJS để đồng bộ lên Supabase.
  // @param jsonDataStr: Chuỗi JSON chứa toàn bộ trạng thái game (save_data)
  // @param level:       Màn chơi hiện tại (integer)
  // @param score:       Điểm số hiện tại (long/integer)
  // -------------------------------------------------------------------------
  SaveGameToWeb: function (jsonDataStr, level, score) {
    if (typeof window.dispatchReactSave === 'function') {
      window.dispatchReactSave(
        UTF8ToString(jsonDataStr),
        level,
        score
      );
    } else {
      console.warn("[SupabaseBridge] window.dispatchReactSave chưa được đăng ký bởi ReactJS.");
    }
  },

  // -------------------------------------------------------------------------
  // HÀM 3: SubmitHighScoreToWeb
  // Gửi riêng điểm cao kèm checksum SHA-256 để ReactJS chuyển tiếp tới
  // Edge Function /submit-score xác thực chống gian lận.
  // @param score:       Điểm số cần submit (long/integer)
  // @param checksumStr: Chuỗi hash SHA-256 được tính trong C# WebAssembly
  // -------------------------------------------------------------------------
  SubmitHighScoreToWeb: function (score, checksumStr) {
    if (typeof window.dispatchReactSubmitScore === 'function') {
      window.dispatchReactSubmitScore(
        score,
        UTF8ToString(checksumStr)
      );
    } else {
      console.warn("[SupabaseBridge] window.dispatchReactSubmitScore chưa được đăng ký bởi ReactJS.");
    }
  },

  // -------------------------------------------------------------------------
  // HÀM 4: RequestLeaderboardFromWeb
  // Yêu cầu ReactJS truy vấn BXH từ Supabase và gửi kết quả trở lại Unity
  // thông qua SendMessage("GameManager", "OnLeaderboardReceived", json)
  // @param topN: Số lượng vị trí BXH muốn lấy (ví dụ: 10, 50)
  // -------------------------------------------------------------------------
  RequestLeaderboardFromWeb: function (topN) {
    if (typeof window.dispatchReactLeaderboard === 'function') {
      window.dispatchReactLeaderboard(topN);
    } else {
      console.warn("[SupabaseBridge] window.dispatchReactLeaderboard chưa được đăng ký.");
    }
  }

});
