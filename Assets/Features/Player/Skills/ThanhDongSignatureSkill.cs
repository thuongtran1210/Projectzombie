using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.YinYang;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// Kỹ năng Chủ động Thanh Đồng: "Giá Đồng Tứ Phủ" (Mục 3.1.2 GDD 3.0).
    /// Bán kính hiệu lực: 4.5m, Thời lượng 5s, Cooldown 30s.
    /// Thỉnh nhập Thánh Tứ Phủ (Thiên, Nhạc, Thoải, Địa), ban buff hào quang 4 cõi:
    /// - Thiên Phủ (Hỏa): +30% Sát thương bộc phát
    /// - Nhạc Phủ (Mộc): +40% Tốc độ di chuyển & đẩy lùi quái
    /// - Thoải Phủ (Thủy): -25% Hồi chiêu toàn bộ vũ khí & làm chậm quái
    /// - Địa Phủ (Thổ): Giảm 50% sát thương nhận vào
    /// </summary>
    public class ThanhDongSignatureSkill : SignatureSkillBase
    {
        public override float Cooldown => 30.0f;

        private GameObject _auraPrefab;

        public ThanhDongSignatureSkill(GameObject auraPrefab = null)
        {
            _auraPrefab = auraPrefab;
        }

        public override void Execute(GameObject playerObj, System.Action<ElementType> onElementSelectedCallback = null)
        {
            if (playerObj == null) return;

            // Kích hoạt trạng thái Thánh Giáng Ngự trên tracker nếu có
            if (playerObj.TryGetComponent<Mechanics.ThanhDongPossessionTracker>(out var possessionTracker))
            {
                possessionTracker.TriggerPossession();
            }

            // Callback chọn cõi Tứ Phủ đại diện (Nhạc Phủ - Mộc Bổn Mệnh)
            ElementType selectedTuPhuRealm = ElementType.Moc;
            onElementSelectedCallback?.Invoke(selectedTuPhuRealm);

            Vector3 spawnPos = playerObj.transform.position;

            if (_auraPrefab != null)
            {
                Object.Instantiate(_auraPrefab, spawnPos, Quaternion.identity, playerObj.transform);
            }
        }
    }
}
