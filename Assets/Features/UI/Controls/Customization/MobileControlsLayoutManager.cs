using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.UI.Controls.Customization
{
    /// <summary>
    /// Service trung tâm quản lý Load, Save, Apply và Reset bố cục nút bấm di động từ PlayerPrefs.
    /// Hoạt động độc lập, Zero GC Alloc trong runtime chiến đấu.
    /// </summary>
    public class MobileControlsLayoutManager : MonoBehaviour
    {
        public static MobileControlsLayoutManager Instance { get; private set; }

        private const string PREFS_KEY = "PROJECT_ZOMBIE_MOBILE_LAYOUT_PROFILE_V1";

        private readonly List<CustomizableControlButton> _registeredControls = new List<CustomizableControlButton>();
        private MobileLayoutProfile _cachedProfile;

        public IReadOnlyList<CustomizableControlButton> RegisteredControls => _registeredControls;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            ScanAndRegisterControls();
            LoadAndApplyAll();
        }

        public void ScanAndRegisterControls()
        {
            _registeredControls.Clear();
            var found = FindObjectsOfType<CustomizableControlButton>(true);
            if (found != null)
            {
                _registeredControls.AddRange(found);
            }
        }

        public void RegisterControl(CustomizableControlButton control)
        {
            if (control != null && !_registeredControls.Contains(control))
            {
                _registeredControls.Add(control);
                
                // Nếu đã có profile trong cache thì áp dụng ngay cho nút mới
                if (_cachedProfile != null)
                {
                    var data = _cachedProfile.FindControl(control.ControlId);
                    if (data != null)
                    {
                        control.ApplyLayout(data);
                    }
                }
            }
        }

        public void UnregisterControl(CustomizableControlButton control)
        {
            if (control != null && _registeredControls.Contains(control))
            {
                _registeredControls.Remove(control);
            }
        }

        public void LoadAndApplyAll()
        {
            _cachedProfile = LoadProfileFromStorage();
            if (_cachedProfile == null || _cachedProfile.controls == null || _cachedProfile.controls.Count == 0)
            {
                return;
            }

            foreach (var control in _registeredControls)
            {
                if (control == null) continue;
                var data = _cachedProfile.FindControl(control.ControlId);
                if (data != null)
                {
                    control.ApplyLayout(data);
                }
            }
        }

        public void SaveCurrentLayout()
        {
            var profile = new MobileLayoutProfile();
            foreach (var control in _registeredControls)
            {
                if (control == null) continue;
                profile.controls.Add(control.ExportCurrentLayout());
            }

            _cachedProfile = profile;
            string json = JsonUtility.ToJson(profile);
            PlayerPrefs.SetString(PREFS_KEY, json);
            PlayerPrefs.Save();

            Debug.Log($"<color=#00FF88>[MobileControlsLayoutManager]</color> Đã lưu thiết lập bố cục phím thành công ({profile.controls.Count} nút)!");
        }

        public void ResetToDefault()
        {
            PlayerPrefs.DeleteKey(PREFS_KEY);
            PlayerPrefs.Save();
            _cachedProfile = null;

            foreach (var control in _registeredControls)
            {
                if (control != null)
                {
                    control.ResetToDefault();
                }
            }

            Debug.Log("<color=#FFD700>[MobileControlsLayoutManager]</color> Đã khôi phục bố cục phím về mặc định!");
        }

        private MobileLayoutProfile LoadProfileFromStorage()
        {
            if (!PlayerPrefs.HasKey(PREFS_KEY)) return null;

            string json = PlayerPrefs.GetString(PREFS_KEY, string.Empty);
            if (string.IsNullOrEmpty(json)) return null;

            try
            {
                return JsonUtility.FromJson<MobileLayoutProfile>(json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MobileControlsLayoutManager] Lỗi đọc JSON profile: {ex.Message}");
                return null;
            }
        }
    }
}
