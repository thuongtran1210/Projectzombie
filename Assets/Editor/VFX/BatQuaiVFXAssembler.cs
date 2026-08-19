using UnityEditor;
using UnityEngine;

namespace Projectzombie.Editor.VFXTools
{
    public class BatQuaiVFXAssembler : EditorWindow
    {
        [MenuItem("ProjectZombie/VFX/Build Taoist Bat Quai VFX Prefab")]
        public static void BuildBatQuaiPrefab()
        {
            // 1. Configure Textures Meta
            string decalPath = "Assets/Art/Skills/VFX/TEX_BatQuai_GroundDecal.png";
            TextureImporter decalImporter = AssetImporter.GetAtPath(decalPath) as TextureImporter;
            if (decalImporter != null)
            {
                decalImporter.textureType = TextureImporterType.Sprite;
                decalImporter.spriteImportMode = SpriteImportMode.Single;
                decalImporter.spritePixelsPerUnit = 128;
                decalImporter.filterMode = FilterMode.Bilinear;
                decalImporter.alphaIsTransparency = true;
                decalImporter.sRGBTexture = true;
                EditorUtility.SetDirty(decalImporter);
                decalImporter.SaveAndReimport();
            }

            string talismanPath = "Assets/Art/Skills/VFX/TEX_Taoist_Talisman.png";
            TextureImporter talismanImporter = AssetImporter.GetAtPath(talismanPath) as TextureImporter;
            if (talismanImporter != null)
            {
                talismanImporter.textureType = TextureImporterType.Sprite;
                talismanImporter.spriteImportMode = SpriteImportMode.Single;
                talismanImporter.spritePixelsPerUnit = 128;
                talismanImporter.filterMode = FilterMode.Bilinear;
                talismanImporter.alphaIsTransparency = true;
                talismanImporter.sRGBTexture = true;
                EditorUtility.SetDirty(talismanImporter);
                talismanImporter.SaveAndReimport();
            }

            AssetDatabase.Refresh();

            Sprite decalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(decalPath);
            Sprite talismanSprite = AssetDatabase.LoadAssetAtPath<Sprite>(talismanPath);

            // 2. Load or Create Prefab
            string prefabPath = "Assets/_Prefabs/Skills/BatQuaiTranZone.prefab";
            GameObject rootObj = new GameObject("BatQuaiTranZone");
            var zoneScript = rootObj.AddComponent<ProjectZombie.Features.Skills.Zones.BatQuaiTranZone>();

            // Child: Decal Visual (Mặt trận Bát Quái nằm dưới chân nhân vật: Layer 'Shadows')
            GameObject visualObj = new GameObject("DecalVisual");
            visualObj.transform.SetParent(rootObj.transform);
            visualObj.transform.localPosition = Vector3.zero;
            var sr = visualObj.AddComponent<SpriteRenderer>();
            sr.sprite = decalSprite;
            sr.sortingLayerName = "Shadows";
            sr.sortingOrder = 5;
            sr.color = new Color(1f, 1f, 1f, 0.95f);

            // Child: Talisman Orbit Particles
            GameObject psObj = new GameObject("TalismanParticles");
            psObj.transform.SetParent(rootObj.transform);
            psObj.transform.localPosition = Vector3.zero;
            var ps = psObj.AddComponent<ParticleSystem>();
            var psRenderer = psObj.GetComponent<ParticleSystemRenderer>();
            
            // PS Main
            var main = ps.main;
            main.duration = 4.0f;
            main.loop = true;
            main.startLifetime = 1.2f;
            main.startSpeed = 2.5f;
            main.startSize = 0.45f;
            main.maxParticles = 25;

            // PS Shape
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 4.2f;
            shape.radiusThickness = 0.15f;

            // PS Renderer
            psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            psRenderer.sortingLayerName = "Skill";
            psRenderer.sortingOrder = 1;

            // Material for Talisman Particles
            string matPath = "Assets/Art/Skills/VFX/M_Taoist_Talisman.mat";
            Material talismanMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (talismanMat == null)
            {
                Shader unlit2D = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default");
                talismanMat = new Material(unlit2D);
                if (talismanSprite != null) talismanMat.mainTexture = talismanSprite.texture;
                AssetDatabase.CreateAsset(talismanMat, matPath);
            }
            else if (talismanSprite != null)
            {
                talismanMat.mainTexture = talismanSprite.texture;
                EditorUtility.SetDirty(talismanMat);
            }

            psRenderer.material = talismanMat;

            // Save Prefab
            PrefabUtility.SaveAsPrefabAsset(rootObj, prefabPath);
            GameObject.DestroyImmediate(rootObj);

            Debug.Log($"[BatQuaiVFX] ĐÃ TẠO VÀ NÂNG CẤP THÀNH CÔNG PREFAB TẠI {prefabPath}!");
        }
    }
}
