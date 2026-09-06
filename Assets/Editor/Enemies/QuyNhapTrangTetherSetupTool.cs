#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Enemies.Special;
using ProjectZombie.Features.Enemies.Visuals;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Tool tự động gắn và cấu hình cơ chế Dây Xích Oán Khí (EnemyTetherLink) vào Prefab Quỷ Nhập Tràng (E_QUYNHAPTRANG).
    /// Đảm bảo Idempotency (chạy nhiều lần không trùng lặp) và độc lập (No Singleton).
    /// </summary>
    public static class QuyNhapTrangTetherSetupTool
    {
        [MenuItem("Tools/Vong Xuyen/Enemies/Setup Quy Nhap Trang Tether Mechanic", priority = 30)]
        public static void SetupTetherMechanic()
        {
            string prefabPath = "Assets/_Prefabs/Characters/Enemies/E_QUYNHAPTRANG.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogError($"[QuyNhapTrangTetherSetupTool] Không tìm thấy Prefab tại: {prefabPath}");
                return;
            }

            // Mở Prefab Contents để chỉnh sửa an toàn
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            try
            {
                // 1. Gắn EnemyTetherLink vào Root
                var tetherLink = prefabRoot.GetComponent<EnemyTetherLink>();
                if (tetherLink == null)
                {
                    tetherLink = prefabRoot.AddComponent<EnemyTetherLink>();
                }

                // 2. Tạo GameObject con TetherBeam_Visual nếu chưa có
                Transform beamTrans = prefabRoot.transform.Find("TetherBeam_Visual");
                if (beamTrans == null)
                {
                    GameObject beamObj = new GameObject("TetherBeam_Visual");
                    beamObj.transform.SetParent(prefabRoot.transform, false);
                    beamTrans = beamObj.transform;
                }

                var beamVisual = beamTrans.GetComponent<TetherBeamVisual>();
                if (beamVisual == null)
                {
                    beamVisual = beamTrans.gameObject.AddComponent<TetherBeamVisual>();
                }

                var lineRenderer = beamTrans.GetComponent<LineRenderer>();
                if (lineRenderer == null)
                {
                    lineRenderer = beamTrans.gameObject.AddComponent<LineRenderer>();
                }

                lineRenderer.positionCount = 2;
                lineRenderer.startWidth = 0.18f;
                lineRenderer.endWidth = 0.18f;
                lineRenderer.useWorldSpace = true;
                lineRenderer.sortingLayerName = "VFX_World";
                lineRenderer.sortingOrder = 100;
                lineRenderer.enabled = false;

                // Lưu thay đổi vào Prefab
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                Debug.Log("<color=#4DEEEA><b>[QuyNhapTrangTetherSetupTool]</b> Đã cấu hình thành công cơ chế Dây Xích Oán Khí (EnemyTetherLink) vào E_QUYNHAPTRANG.prefab!</color>");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }
    }
}
#endif
