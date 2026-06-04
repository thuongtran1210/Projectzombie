using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Quản lý danh sách các thẻ bị động (Passives) mà người chơi đã nhặt.
    /// Dùng để kiểm tra điều kiện tiến hóa (Evolution) hoặc tương tác khác.
    /// </summary>
    public class PlayerPassives : MonoBehaviour
    {
        private List<string> _activePassives = new List<string>();

        public IReadOnlyList<string> ActivePassives => _activePassives;

        public void AddPassive(string passiveId)
        {
            if (string.IsNullOrEmpty(passiveId)) return;
            
            if (!_activePassives.Contains(passiveId))
            {
                _activePassives.Add(passiveId);
                Debug.Log($"[PlayerPassives] Added new passive: {passiveId}");
            }
        }

        public bool HasPassive(string passiveId)
        {
            if (string.IsNullOrEmpty(passiveId)) return false;
            return _activePassives.Contains(passiveId);
        }
    }
}
