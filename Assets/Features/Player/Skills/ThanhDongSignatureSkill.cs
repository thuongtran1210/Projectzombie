using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.YinYang;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// Kỹ năng Chủ động Thanh Đồng: "Giá Đồng" (Hầu Đồng Tứ Phủ — Mục 3.1.2 GDD v4.0).
    /// Bán kính hiệu lực: 4.5m, Thời lượng 5s, Cooldown 30s.
    /// Thỉnh nhập Thánh Tứ Phủ (Thiên, Nhạc, Thoải, Địa), ban buff hào quang thuộc tính & ép cân bằng Âm Dương về 50.
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

            // Thỉnh nhập cõi Tứ Phủ: Nếu không chọn, mặc định thỉnh nhập Nhạc Phủ (Mộc) hoặc Hỏa
            ElementType selectedTuPhuRealm = ElementType.Moc;
            onElementSelectedCallback?.Invoke(selectedTuPhuRealm);

            // Ép cân bằng Cán cân Âm Dương về 50 (Thái Cực) trong 5s thời gian Giá Đồng
            if (YinYangManager.Instance != null)
            {
                YinYangManager.Instance.SetTemporaryNeutralOverride(5.0f, 50.0f);
            }

            Vector3 spawnPos = playerObj.transform.position;

            if (_auraPrefab != null)
            {
                Object.Instantiate(_auraPrefab, spawnPos, Quaternion.identity, playerObj.transform);
            }
            else
            {
                // Fallback tạo hiệu ứng hào quang Giá Đồng Tứ Phủ
                GameObject auraObj = new GameObject("GiaDongTuPhuAura_Dynamic");
                auraObj.transform.SetParent(playerObj.transform);
                auraObj.transform.localPosition = Vector3.zero;

                var lineRenderer = auraObj.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

                // Phối màu Tứ Phủ đại diện (Nhạc Phủ - Xanh Mộc #4C7A3D)
                Color tuPhuColor = new Color(0.3f, 0.48f, 0.24f, 0.8f);
                lineRenderer.startColor = tuPhuColor;
                lineRenderer.endColor = tuPhuColor;
                lineRenderer.startWidth = 0.1f;
                lineRenderer.endWidth = 0.1f;
                lineRenderer.useWorldSpace = false;

                // Vẽ vòng tròn Hào Quang Tứ Phủ
                int steps = 24;
                lineRenderer.positionCount = steps + 1;
                float radius = 4.5f;
                for (int i = 0; i <= steps; i++)
                {
                    float angle = i * (Mathf.PI * 2.0f / steps);
                    lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0));
                }

                Object.Destroy(auraObj, 5.0f);
            }
        }
    }
}
