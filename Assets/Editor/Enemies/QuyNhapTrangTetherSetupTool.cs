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

                lineRenderer.positionCount = 14;
                lineRenderer.startWidth = 0.09f;
                lineRenderer.endWidth = 0.09f;
                lineRenderer.useWorldSpace = true;
                lineRenderer.sortingLayerName = "Skill";
                lineRenderer.sortingOrder = 500;
                lineRenderer.enabled = false;

                // Gán Material URP 2D Unlit hoặc Sprite Default để hiển thị rõ màu sắc
                if (lineRenderer.sharedMaterial == null)
                {
                    Material defaultMat = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
                    if (defaultMat == null)
                    {
                        Shader sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
                        if (sh != null) defaultMat = new Material(sh);
                    }
                    if (defaultMat != null) lineRenderer.sharedMaterial = defaultMat;
                }

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
