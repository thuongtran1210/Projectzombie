using UnityEditor;
using UnityEngine;

namespace Projectzombie.Editor.VFXTools
{
    public class PlayerDashVFXAssembler : EditorWindow
    {
        [MenuItem("ProjectZombie/VFX/Setup Player Dash VFX & Prefabs")]
        public static void SetupPlayerDash()
        {
            // 1. Texture Meta
            string dustPath = "Assets/Art/VFX/Dash/TEX_Dash_WindPuff.png";
            TextureImporter dustImporter = AssetImporter.GetAtPath(dustPath) as TextureImporter;
            if (dustImporter != null)
            {
                dustImporter.textureType = TextureImporterType.Sprite;
                dustImporter.spriteImportMode = SpriteImportMode.Single;
                dustImporter.spritePixelsPerUnit = 128;
                dustImporter.filterMode = FilterMode.Bilinear;
                dustImporter.alphaIsTransparency = true;
                dustImporter.sRGBTexture = true;
                EditorUtility.SetDirty(dustImporter);
                dustImporter.SaveAndReimport();
            }

            AssetDatabase.Refresh();
            Sprite dustSprite = AssetDatabase.LoadAssetAtPath<Sprite>(dustPath);

            // 2. Material
            string matPath = "Assets/Art/VFX/Dash/M_Dash_WindPuff.mat";
            Material dustMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (dustMat == null)
            {
                Shader unlit2D = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
                dustMat = new Material(unlit2D);
                if (dustSprite != null) dustMat.mainTexture = dustSprite.texture;
                AssetDatabase.CreateAsset(dustMat, matPath);
            }

            // 3. Create Dash Dust Prefab
            string dustPrefabPath = "Assets/_Prefabs/VFX/VFX_Player_DashDust.prefab";
            GameObject dustObj = new GameObject("VFX_Player_DashDust");
            var ps = dustObj.AddComponent<ParticleSystem>();
            var psRenderer = dustObj.GetComponent<ParticleSystemRenderer>();

            var main = ps.main;
            main.duration = 0.35f;
            main.loop = false;
            main.startLifetime = 0.25f;
            main.startSpeed = 3.5f;
            main.startSize = 0.65f;
            main.startColor = new Color(1f, 1f, 1f, 0.85f);
            main.stopAction = ParticleSystemStopAction.Destroy;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.2f;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 6) });

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 0.5f);
            sizeCurve.AddKey(1f, 1.2f);
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var colorOverLife = ps.colorOverLifetime;
            colorOverLife.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.4f, 0.9f, 1f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLife.color = grad;

            psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            psRenderer.sortingLayerName = "Shadows";
            psRenderer.sortingOrder = 6;
            psRenderer.material = dustMat;

            GameObject dustPrefab = PrefabUtility.SaveAsPrefabAsset(dustObj, dustPrefabPath);
            GameObject.DestroyImmediate(dustObj);

            // 4. Attach PlayerDashVisuals to Player Prefabs
            string[] playerPrefabs = new[]
            {
                "Assets/_Prefabs/Characters/Players/Dao Si.prefab",
                "Assets/_Prefabs/Characters/Players/Thu Sinh.prefab"
            };

            foreach (var pPath in playerPrefabs)
            {
                GameObject pPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(pPath);
                if (pPrefab != null)
                {
                    var dashVisual = pPrefab.GetComponent<ProjectZombie.Features.Player.Visuals.PlayerDashVisuals>();
                    if (dashVisual == null)
                    {
                        dashVisual = pPrefab.AddComponent<ProjectZombie.Features.Player.Visuals.PlayerDashVisuals>();
                    }

                    // Assign serialized dust prefab
                    var so = new SerializedObject(dashVisual);
                    so.FindProperty("_dashDustPrefab").objectReferenceValue = dustPrefab;
                    so.ApplyModifiedProperties();

                    EditorUtility.SetDirty(pPrefab);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DashVFX] ĐÃ TỰ ĐỘNG GẮN VÀ CẤU HÌNH HIỆU ỨNG TÀN ẢNH (GHOST AFTERIMAGE) & BỤI GIÓ LƯỚT CHO TẤT CẢ HERO!");
        }
    }
}
