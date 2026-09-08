using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.UI.Controls.Customization
{
    /// <summary>
    /// Data model đại diện cho thông số bố cục của một nút điều khiển di động đơn lẻ.
    /// </summary>
    [Serializable]
    public class ControlButtonLayoutData
    {
        public string controlId;            // Định danh duy nhất: "Btn_Attack", "Btn_Dash", "Btn_SignatureSkill", "Btn_RelicSkill", "DynamicVirtualJoystick"
        public Vector2 anchoredPosition;   // Tọa độ tương đối theo Canvas chuẩn
        public float scale = 1.0f;         // Tỉ lệ phóng to/thu nhỏ (0.7f - 1.5f)
        public float opacity = 1.0f;       // Độ trong suốt (0.3f - 1.0f)

        public ControlButtonLayoutData() { }

        public ControlButtonLayoutData(string id, Vector2 pos, float scaleVal, float opacityVal)
        {
            controlId = id;
            anchoredPosition = pos;
            scale = scaleVal;
            opacity = opacityVal;
        }
    }

    /// <summary>
    /// Profile tổng thể chứa thông số cấu hình của toàn bộ cụm phím điều khiển.
    /// Hỗ trợ serialize sang JSON để lưu trữ cục bộ vào PlayerPrefs.
    /// </summary>
    [Serializable]
    public class MobileLayoutProfile
    {
        public int version = 1;
        public List<ControlButtonLayoutData> controls = new List<ControlButtonLayoutData>();

        public ControlButtonLayoutData FindControl(string id)
        {
            if (controls == null) return null;
            return controls.Find(c => c != null && c.controlId == id);
        }

        public void SetControl(string id, Vector2 pos, float scale, float opacity)
        {
            if (controls == null) controls = new List<ControlButtonLayoutData>();
            var existing = FindControl(id);
            if (existing != null)
            {
                existing.anchoredPosition = pos;
                existing.scale = scale;
                existing.opacity = opacity;
            }
            else
            {
                controls.Add(new ControlButtonLayoutData(id, pos, scale, opacity));
            }
        }
    }
}
