#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.DebugTools
{
    /// <summary>
    /// Component hỗ trợ Game Designer / QA test nhanh hệ thống Thẻ Nâng Cấp khi đang Play Mode:
    /// - Cơ chế Ngẫu Nhiên: Kích hoạt Level Up ngẫu nhiên mở UI in-game (Phím F2).
    /// - Cơ chế Chỉ Định: Ép ngay Đại Lõi (F1), Ép 5 Thần Binh Thuật, Ép bất kỳ Thẻ Upgrade nào vào Player.
    /// </summary>
    [AddComponentMenu("ProjectZombie/Debug/Runtime Upgrade Debug Controller")]
    public class RuntimeUpgradeDebugController : MonoBehaviour
    {
        [Header("Editor Controls")]
        [SerializeField] private bool _showOnScreenGUI = false;

        [Header("Chỉ Định Ép Thẻ Trực Tiếp")]
        [SerializeField] private MythicArchetype _forceArchetype = MythicArchetype.PhuDongThienUy;
        [SerializeField] private UpgradeData _targetUpgradeToInject;

        private void Update()
        {
            // Phím tắt F1: Bật/Tắt Menu Cheat Nâng Cấp On-Screen
            if (Input.GetKeyDown(KeyCode.F1))
            {
                _showOnScreenGUI = !_showOnScreenGUI;
            }

            // Phím tắt F2: Kích hoạt Level Up Ngẫu Nhiên (+1 Level) mở bảng 3 thẻ in-game
            if (Input.GetKeyDown(KeyCode.F2))
            {
                TriggerRandomLevelUp();
            }
        }

        /// <summary>
        /// Kích hoạt cơ chế Ngẫu Nhiên: Tăng 1 Level và mở UI chọn thẻ in-game.
        /// </summary>
        [ContextMenu("🎲 Kích Hoạt Random Level Up (F2)")]
        public void TriggerRandomLevelUp()
        {
            var exp = FindObjectOfType<PlayerExperience>();
            if (exp != null)
            {
                exp.AddExp(exp.MaxExp - exp.CurrentExp + 0.1f);
                Debug.Log("<color=#00FF88>[RuntimeDebug]</color> Đã kích hoạt Level Up ngẫu nhiên qua PlayerExperience!");
            }
            else
            {
                Debug.LogWarning("[RuntimeDebug] Không tìm thấy PlayerExperience trong Scene!");
            }
        }

        /// <summary>
        /// Kích hoạt cơ chế Chỉ Định: Trang bị ngay 1 trong 5 Đại Lõi và Prefab Runtime cho Player.
        /// </summary>
        [ContextMenu("👑 Chỉ Định Trang Bị Đại Lõi")]
        public void ForceEquipArchetype()
        {
            var player = PlayerProvider.HasPlayer ? PlayerProvider.PlayerGameObject : GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[RuntimeDebug] Không tìm thấy Player trong Scene!");
                return;
            }

            var mythicMgr = player.GetComponent<PlayerMythicManager>();
            if (mythicMgr == null)
            {
                mythicMgr = player.AddComponent<PlayerMythicManager>();
            }

            // Nạp Core UpgradeData tương ứng
            string coreId = _forceArchetype switch
            {
                MythicArchetype.PhuDongThienUy => "CORE_PHUDONG",
                MythicArchetype.KimQuyThanCo => "CORE_KIMQUY",
                MythicArchetype.TanVienSonThanh => "CORE_TANVIEN",
                MythicArchetype.ThuyBaCuongNo => "CORE_THUYBA",
                MythicArchetype.LongTienHuyetMach => "CORE_LONGTIEN",
                _ => null
            };

            if (string.IsNullOrEmpty(coreId)) return;

            var coreData = Resources.Load<MythicCoreUpgradeData>($"Upgrades/Mythic/{coreId}");
#if UNITY_EDITOR
            if (coreData == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets($"{coreId} t:MythicCoreUpgradeData");
                if (guids.Length > 0)
                {
                    coreData = UnityEditor.AssetDatabase.LoadAssetAtPath<MythicCoreUpgradeData>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }
#endif
            if (coreData != null)
            {
                mythicMgr.EquipMythicCore(coreData);
                Debug.Log($"<color=#FFD700>[RuntimeDebug] Đã chỉ định trang bị Đại Lõi: {coreData.mythicTitle} ({_forceArchetype})</color>");
            }
        }

        /// <summary>
        /// Kích hoạt cơ chế Chỉ Định: Ép ngay 1 thẻ nâng cấp chỉ định vào Player.
        /// </summary>
        [ContextMenu("🎯 Ép Thẻ Đang Chọn Vào Player")]
        public void InjectTargetUpgrade()
        {
            if (_targetUpgradeToInject == null)
            {
                Debug.LogWarning("[RuntimeDebug] Chưa chọn thẻ _targetUpgradeToInject trong Inspector!");
                return;
            }

            var player = PlayerProvider.HasPlayer ? PlayerProvider.PlayerGameObject : GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[RuntimeDebug] Không tìm thấy Player trong Scene!");
                return;
            }

            var context = PlayerContext.Create(player);
            _targetUpgradeToInject.ApplyUpgrade(context);
            Debug.Log($"<color=#00E5FF>[RuntimeDebug] Đã ép thẻ {_targetUpgradeToInject.id} ({_targetUpgradeToInject.upgradeName}) vào Player!</color>");
        }

        private void OnGUI()
        {
            if (!_showOnScreenGUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 320, 260), "🎮 RUNTIME UPGRADE DEBUG (F1)", GUI.skin.window);

            GUILayout.Label("<b>1. Cơ Chế Ngẫu Nhiên:</b>", new GUIStyle(GUI.skin.label) { richText = true });
            if (GUILayout.Button("🎲 Kích Hoạt Random Level Up (F2)", GUILayout.Height(28)))
            {
                TriggerRandomLevelUp();
            }

            GUILayout.Space(6);

            GUILayout.Label("<b>2. Cơ Chế Chỉ Định Đại Lõi:</b>", new GUIStyle(GUI.skin.label) { richText = true });
            _forceArchetype = (MythicArchetype)UnityEditor.EditorGUILayout.EnumPopup(_forceArchetype);
            if (GUILayout.Button("👑 Ép Kích Hoạt Lõi Này", GUILayout.Height(28)))
            {
                ForceEquipArchetype();
            }

            GUILayout.Space(6);
            if (GUILayout.Button("Đóng Menu Cheat (F1)", GUILayout.Height(24)))
            {
                _showOnScreenGUI = false;
            }

            GUILayout.EndArea();
        }
    }
}
#endif
