using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Skills.Zones;

namespace ProjectZombie.Features.Player.Skills
{
    /// <summary>
    /// Legacy Kỹ năng Chủ động Đạo Sĩ (chuyển giao sang Thanh Đồng - "Giá Đồng" Mục 3.1.2 GDD v4.0).
    /// Giữ lại để đảm bảo tương thích ngược code cũ.
    /// </summary>
    public class DaoSiSignatureSkill : SignatureSkillBase
    {
        public override float Cooldown => 30.0f;

        private GameObject _zonePrefab;

        public DaoSiSignatureSkill(GameObject zonePrefab = null)
        {
            _zonePrefab = zonePrefab;
        }

        public override void Execute(GameObject playerObj, System.Action<ElementType> onElementSelectedCallback = null)
        {
            if (playerObj == null) return;

            Vector3 spawnPos = playerObj.transform.position;

            if (_zonePrefab != null)
            {
                GameObject zoneObj = Object.Instantiate(_zonePrefab, spawnPos, Quaternion.identity);
                var zoneScript = zoneObj.GetComponent<BatQuaiTranZone>();
                if (zoneScript != null)
                {
                    zoneScript.Initialize(spawnPos, 4.5f, 4.0f);
                }
            }
            else
            {
                // Fallback tạo GameObject động có BatQuaiTranZone & LineRenderer
                GameObject zoneObj = new GameObject("BatQuaiTranZone_Dynamic");
                var lineRenderer = zoneObj.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = new Color(0.9f, 0.8f, 0.5f, 0.6f);
                lineRenderer.endColor = new Color(0.9f, 0.8f, 0.5f, 0.6f);
                lineRenderer.startWidth = 0.08f;
                lineRenderer.endWidth = 0.08f;

                var zoneScript = zoneObj.AddComponent<BatQuaiTranZone>();
                zoneScript.Initialize(spawnPos, 4.5f, 4.0f);
            }
        }
    }
}
