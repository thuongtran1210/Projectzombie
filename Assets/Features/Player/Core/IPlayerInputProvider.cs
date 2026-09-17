using System;
using UnityEngine;

namespace ProjectZombie.Features.Player.Input
{
    /// <summary>
    /// Hợp đồng cung cấp Input cho nhân vật (Quy chuẩn 13.2 AGENTS.md).
    /// Cho phép PlayerController nhận lệnh từ cả Local InputReader (Phím/Joystick) lẫn NetworkInputBridge (Photon Fusion).
    /// </summary>
    public interface IPlayerInputProvider
    {
        /// <summary>
        /// Vector di chuyển (Normalized hoặc Analog Magnitude [0..1]).
        /// </summary>
        Vector2 MovementInput { get; }

        /// <summary>
        /// Trạng thái khóa Input (khi choáng, cutscene, hoặc mở menu).
        /// </summary>
        bool IsInputBlocked { get; set; }

        /// <summary>
        /// Sự kiện phát ra khi kích hoạt Lướt / Dash.
        /// </summary>
        event Action OnDashTriggered;

        /// <summary>
        /// Sự kiện phát ra khi kích hoạt Tấn công cơ bản / Combo.
        /// </summary>
        event Action OnAttackTriggered;

        /// <summary>
        /// Sự kiện phát ra khi kích hoạt Tuyệt kỹ (Signature Ultimate).
        /// </summary>
        event Action OnSignatureSkillTriggered;

        /// <summary>
        /// Sự kiện phát ra khi kích hoạt Pháp bảo Hộ thân (Relic Active Skill).
        /// </summary>
        event Action OnRelicSkillTriggered;
    }
}
