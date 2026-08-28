#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Editor.VFXTools
{
    /// <summary>
    /// Editor Tool tự động cấu hình Texture, Material và Lắp ráp Multi-layer VFX Prefabs 
    /// cho toàn bộ hệ thống Pháp Bảo / Vũ Khí trong Project Zombie.
    /// Menu: ProjectZombie > VFX > Assemble All Weapons VFX
    /// </summary>
    public static class WeaponVFXAssembler
    {
        private const string TEXTURE_DIR = "Assets/Art/Weapons/VFX";
        private const string MATERIAL_DIR = "Assets/VFX/SkillLibrary/Materials";
        private const string PREFAB_DIR = "Assets/VFX/SkillLibrary/Prefabs";
        private const string WEAPONS_PREFAB_DIR = "Assets/_Prefabs/Weapons";

        [MenuItem("ProjectZombie/VFX/Assemble All Weapons VFX")]
        public static void AssembleAllWeaponsVFX()
        {
            EnsureDirectories();

            // 1. Cấu hình Texture Importers
            ConfigureTextures();

            // 2. Lắp ráp từng VFX Prefab
            BuildPotSuctionVFX();
            BuildChickenBroomSmashVFX();
            BuildThachSanhArrowVFX();
            BuildSleepingMatVFX();
            BuildFoxFlameStreamVFX();
            BuildWaterLightningChainVFX();

            // 3. Tự động liên kết VFX vào các Weapon Prefabs tương ứng
            WireWeaponPrefabs();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[WeaponVFXAssembler] Đã lắp ráp và liên kết hoàn thiện toàn bộ VFX Prefabs cho hệ thống Pháp Bảo / Vũ Khí thành công!");
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Art/Weapons")) AssetDatabase.CreateFolder("Assets/Art", "Weapons");
            if (!AssetDatabase.IsValidFolder(TEXTURE_DIR)) AssetDatabase.CreateFolder("Assets/Art/Weapons", "VFX");
            if (!AssetDatabase.IsValidFolder("Assets/VFX/SkillLibrary")) AssetDatabase.CreateFolder("Assets/VFX", "SkillLibrary");
            if (!AssetDatabase.IsValidFolder(MATERIAL_DIR)) AssetDatabase.CreateFolder("Assets/VFX/SkillLibrary", "Materials");
            if (!AssetDatabase.IsValidFolder(PREFAB_DIR)) AssetDatabase.CreateFolder("Assets/VFX/SkillLibrary", "Prefabs");
        }

        private static void ConfigureTextures()
        {
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new string[] { TEXTURE_DIR });
            foreach (string guid in textureGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    bool changed = false;
                    if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
                    if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; changed = true; }
                    if (!importer.alphaIsTransparency) { importer.alphaIsTransparency = true; changed = true; }
                    if (importer.wrapMode != TextureWrapMode.Clamp) { importer.wrapMode = TextureWrapMode.Clamp; changed = true; }
                    if (importer.filterMode != FilterMode.Bilinear) { importer.filterMode = FilterMode.Bilinear; changed = true; }

                    if (changed)
                    {
                        EditorUtility.SetDirty(importer);
                        importer.SaveAndReimport();
                    }
                }
            }
            AssetDatabase.Refresh();
        }

        private static Material GetOrCreateMaterial(string matName, string textureName, bool isAdditive)
        {
            string matPath = $"{MATERIAL_DIR}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TEXTURE_DIR}/{textureName}");

            if (mat == null)
            {
                Shader shader = isAdditive 
                    ? (Shader.Find("ProjectZombie/VFX/Slash_Additive") ?? Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default"))
                    : (Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") ?? Shader.Find("Sprites/Default"));

                mat = new Material(shader);
                if (tex != null) mat.mainTexture = tex;
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                if (tex != null && mat.mainTexture != tex)
                {
                    mat.mainTexture = tex;
                    EditorUtility.SetDirty(mat);
                }
            }
            return mat;
        }

        // =========================================================================
        // 1. W_POT: Nồi Cơm Thạch Sanh VFX (Vortex + Suction + Rice Embers)
        // =========================================================================
        private static void BuildPotSuctionVFX()
        {
            Material matVortex = GetOrCreateMaterial("M_Pot_Suction_Vortex", "Tex_Pot_Suction_Vortex.png", true);
            Material matRice = GetOrCreateMaterial("M_Rice_Collectible", "Tex_Rice_Collectible.png", false);

            GameObject root = new GameObject("VFX_Relic_Pot_Suction");
            
            // Layer 1: Suction Vortex
            var psVortex = root.AddComponent<ParticleSystem>();
            var psrVortex = root.GetComponent<ParticleSystemRenderer>();
            psrVortex.material = matVortex;
            psrVortex.sortingLayerName = "Skill";
            psrVortex.sortingOrder = 5;

            var mainV = psVortex.main;
            mainV.duration = 1.0f;
            mainV.loop = false;
            mainV.startLifetime = 0.8f;
            mainV.startSpeed = 0f;
            mainV.startSize = 3.8f;
            mainV.startRotation = new ParticleSystem.MinMaxCurve(0, 360f * Mathf.Deg2Rad);
            mainV.startColor = new Color(1f, 0.85f, 0.4f, 0.9f);

            var rotV = psVortex.rotationOverLifetime;
            rotV.enabled = true;
            rotV.z = new ParticleSystem.MinMaxCurve(720f * Mathf.Deg2Rad);

            var sizeV = psVortex.sizeOverLifetime;
            sizeV.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0.2f);
            curve.AddKey(0.4f, 1.0f);
            curve.AddKey(1.0f, 0.0f);
            sizeV.size = new ParticleSystem.MinMaxCurve(1.0f, curve);

            // Layer 2: Flying Rice Orbs (Hạt gạo / cơm nắm xoáy vào tâm)
            GameObject childRice = new GameObject("Rice_Sparks");
            childRice.transform.SetParent(root.transform);
            childRice.transform.localPosition = Vector3.zero;

            var psRice = childRice.AddComponent<ParticleSystem>();
            var psrRice = childRice.GetComponent<ParticleSystemRenderer>();
            psrRice.material = matRice;
            psrRice.sortingLayerName = "Skill";
            psrRice.sortingOrder = 6;

            var mainR = psRice.main;
            mainR.duration = 1.0f;
            mainR.loop = false;
            mainR.startLifetime = 0.6f;
            mainR.startSpeed = -4.0f; // Hút về tâm
            mainR.startSize = 0.35f;

            var shapeR = psRice.shape;
            shapeR.shapeType = ParticleSystemShapeType.Circle;
            shapeR.radius = 2.5f;

            var emissionR = psRice.emission;
            emissionR.rateOverTime = 0;
            emissionR.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.05f, 15) });

            SavePrefab(root, $"{PREFAB_DIR}/VFX_Relic_Pot_Suction.prefab");
        }

        // =========================================================================
        // 2. R008: Chổi Lông Gà Gia Truyền (Smash + Feather Burst + Cracked Earth)
        // =========================================================================
        private static void BuildChickenBroomSmashVFX()
        {
            Material matBroom = GetOrCreateMaterial("M_ChickenBroom_Giant", "Tex_ChickenBroom_Giant.png", false);
            Material matFeather = GetOrCreateMaterial("M_Feather_Burst", "Tex_Feather_Burst.png", false);
            Material matCrack = GetOrCreateMaterial("M_Ground_Cracked", "Tex_Ground_Cracked_Shockwave.png", true);

            GameObject root = new GameObject("VFX_Relic_ChickenBroom_Smash");

            // Layer 1: Ground Crack Decal
            var psCrack = root.AddComponent<ParticleSystem>();
            var psrCrack = root.GetComponent<ParticleSystemRenderer>();
            psrCrack.material = matCrack;
            psrCrack.sortingLayerName = "Shadows";
            psrCrack.sortingOrder = 5;

            var mainC = psCrack.main;
            mainC.duration = 1.2f;
            mainC.loop = false;
            mainC.startLifetime = 0.9f;
            mainC.startSpeed = 0f;
            mainC.startSize = 4.5f;
            mainC.startColor = new Color(1f, 0.9f, 0.4f, 1f);

            var colOverLifeC = psCrack.colorOverLifetime;
            colOverLifeC.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) }
            );
            colOverLifeC.color = grad;

            // Layer 2: Giant Broom Stamp (Sprite nhấp nháy từ trên giáng xuống)
            GameObject childBroom = new GameObject("Giant_Broom_Impact");
            childBroom.transform.SetParent(root.transform);
            childBroom.transform.localPosition = new Vector3(0, 0.5f, 0);

            var psBroom = childBroom.AddComponent<ParticleSystem>();
            var psrBroom = childBroom.GetComponent<ParticleSystemRenderer>();
            psrBroom.material = matBroom;
            psrBroom.sortingLayerName = "Skill";
            psrBroom.sortingOrder = 10;

            var mainB = psBroom.main;
            mainB.duration = 0.5f;
            mainB.loop = false;
            mainB.startLifetime = 0.4f;
            mainB.startSpeed = 0f;
            mainB.startSize = 3.2f;

            var emissionB = psBroom.emission;
            emissionB.rateOverTime = 0;
            emissionB.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 1) });

            // Layer 3: Feather Burst 360 độ
            GameObject childFeather = new GameObject("Feathers_Explosion");
            childFeather.transform.SetParent(root.transform);
            childFeather.transform.localPosition = Vector3.zero;

            var psFeather = childFeather.AddComponent<ParticleSystem>();
            var psrFeather = childFeather.GetComponent<ParticleSystemRenderer>();
            psrFeather.material = matFeather;
            psrFeather.sortingLayerName = "Skill";
            psrFeather.sortingOrder = 11;

            var mainF = psFeather.main;
            mainF.duration = 1.0f;
            mainF.loop = false;
            mainF.startLifetime = 0.7f;
            mainF.startSpeed = 6.0f;
            mainF.startSize = 0.65f;
            mainF.startRotation = new ParticleSystem.MinMaxCurve(0, 360f * Mathf.Deg2Rad);

            var shapeF = psFeather.shape;
            shapeF.shapeType = ParticleSystemShapeType.Circle;
            shapeF.radius = 0.4f;

            var emissionF = psFeather.emission;
            emissionF.rateOverTime = 0;
            emissionF.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.05f, 24) });

            var rotF = psFeather.rotationOverLifetime;
            rotF.enabled = true;
            rotF.z = new ParticleSystem.MinMaxCurve(-360f * Mathf.Deg2Rad, 360f * Mathf.Deg2Rad);

            SavePrefab(root, $"{PREFAB_DIR}/VFX_Relic_ChickenBroom_Smash.prefab");
        }

        // =========================================================================
        // 3. W007: Cung Thạch Sanh (Piercing Arrow Trail + Sonic Cone)
        // =========================================================================
        private static void BuildThachSanhArrowVFX()
        {
            Material matTrail = GetOrCreateMaterial("M_ThachSanh_Arrow_Trail", "Tex_ThachSanh_Arrow_Trail.png", true);
            Material matCone = GetOrCreateMaterial("M_ThachSanh_Pierce_Shockwave", "Tex_ThachSanh_Pierce_Shockwave.png", true);

            GameObject root = new GameObject("VFX_W007_ThachSanhArrow");

            // Layer 1: Golden Energy Trail Beam
            var psTrail = root.AddComponent<ParticleSystem>();
            var psrTrail = root.GetComponent<ParticleSystemRenderer>();
            psrTrail.material = matTrail;
            psrTrail.sortingLayerName = "Skill";
            psrTrail.sortingOrder = 8;

            var mainT = psTrail.main;
            mainT.duration = 0.5f;
            mainT.loop = true;
            mainT.startLifetime = 0.25f;
            mainT.startSpeed = 0f;
            mainT.startSize = 2.0f;
            mainT.startColor = new Color(1f, 0.85f, 0.2f, 1f);

            var emissionT = psTrail.emission;
            emissionT.rateOverDistance = 8f;

            // Layer 2: Sonic Piercing Waves
            GameObject childCone = new GameObject("Piercing_Rings");
            childCone.transform.SetParent(root.transform);
            childCone.transform.localPosition = Vector3.zero;

            var psCone = childCone.AddComponent<ParticleSystem>();
            var psrCone = childCone.GetComponent<ParticleSystemRenderer>();
            psrCone.material = matCone;
            psrCone.sortingLayerName = "Skill";
            psrCone.sortingOrder = 9;

            var mainC = psCone.main;
            mainC.duration = 0.5f;
            mainC.loop = true;
            mainC.startLifetime = 0.2f;
            mainC.startSpeed = 1.0f;
            mainC.startSize = 1.2f;

            var emissionC = psCone.emission;
            emissionC.rateOverDistance = 4f;

            SavePrefab(root, $"{PREFAB_DIR}/VFX_W007_ThachSanhArrow.prefab");
        }

        // =========================================================================
        // 4. R007: Chiếu Trải Hoàng Tuyền (Sleep Zzz + Slide Wind)
        // =========================================================================
        private static void BuildSleepingMatVFX()
        {
            Material matZzz = GetOrCreateMaterial("M_Sleep_Zzz_Comic", "Tex_Sleep_Zzz_Comic.png", false);
            Material matWind = GetOrCreateMaterial("M_Mat_Slide_Wind", "Tex_Mat_Slide_Wind.png", true);

            GameObject root = new GameObject("VFX_Relic_SleepingMat_Decal");

            // Layer 1: Floating Zzz on sleep zone
            var psZzz = root.AddComponent<ParticleSystem>();
            var psrZzz = root.GetComponent<ParticleSystemRenderer>();
            psrZzz.material = matZzz;
            psrZzz.sortingLayerName = "Skill";
            psrZzz.sortingOrder = 10;

            var mainZ = psZzz.main;
            mainZ.duration = 4.0f;
            mainZ.loop = true;
            mainZ.startLifetime = 1.2f;
            mainZ.startSpeed = 1.2f;
            mainZ.startSize = 0.6f;

            var shapeZ = psZzz.shape;
            shapeZ.shapeType = ParticleSystemShapeType.Box;
            shapeZ.scale = new Vector3(2.5f, 1.5f, 0.1f);

            // Layer 2: Wind Slide speed
            GameObject childWind = new GameObject("Mat_Slide_Wind");
            childWind.transform.SetParent(root.transform);
            childWind.transform.localPosition = Vector3.zero;

            var psWind = childWind.AddComponent<ParticleSystem>();
            var psrWind = childWind.GetComponent<ParticleSystemRenderer>();
            psrWind.material = matWind;
            psrWind.sortingLayerName = "Skill";
            psrWind.sortingOrder = 6;

            var mainW = psWind.main;
            mainW.duration = 2.0f;
            mainW.loop = true;
            mainW.startLifetime = 0.5f;
            mainW.startSpeed = 3.0f;
            mainW.startSize = 1.0f;

            SavePrefab(root, $"{PREFAB_DIR}/VFX_Relic_SleepingMat_Decal.prefab");
        }

        // =========================================================================
        // 5. W008 & W009: Đao Cửu Vĩ (Lửa Cáo) & Trượng Long Vương (Sét Nước)
        // =========================================================================
        private static void BuildFoxFlameStreamVFX()
        {
            Material matFlame = GetOrCreateMaterial("M_FoxFlame_Stream", "Tex_FoxFlame_Stream.png", true);
            GameObject root = new GameObject("VFX_W008_FoxFlameStream");

            var ps = root.AddComponent<ParticleSystem>();
            var psr = root.GetComponent<ParticleSystemRenderer>();
            psr.material = matFlame;
            psr.sortingLayerName = "Skill";
            psr.sortingOrder = 8;

            var main = ps.main;
            main.duration = 1.5f;
            main.loop = true;
            main.startLifetime = 0.5f;
            main.startSpeed = 7.0f;
            main.startSize = 1.6f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.radius = 0.2f;

            SavePrefab(root, $"{PREFAB_DIR}/VFX_W008_FoxFlameStream.prefab");
        }

        private static void BuildWaterLightningChainVFX()
        {
            Material matWaterBolt = GetOrCreateMaterial("M_WaterLightning_Chain", "Tex_WaterLightning_Chain.png", true);
            GameObject root = new GameObject("VFX_W009_LightningChain");

            var ps = root.AddComponent<ParticleSystem>();
            var psr = root.GetComponent<ParticleSystemRenderer>();
            psr.material = matWaterBolt;
            psr.sortingLayerName = "Skill";
            psr.sortingOrder = 9;

            var main = ps.main;
            main.duration = 0.6f;
            main.loop = false;
            main.startLifetime = 0.35f;
            main.startSpeed = 0f;
            main.startSize = 2.4f;

            SavePrefab(root, $"{PREFAB_DIR}/VFX_W009_LightningChain.prefab");
        }

        // =========================================================================
        // 6. Liên kết Prefabs
        // =========================================================================
        private static void WireWeaponPrefabs()
        {
            // 1. W_POT
            SetPrefabComponentField($"{WEAPONS_PREFAB_DIR}/Weapon_W_POT.prefab", "potVfxPrefab", $"{PREFAB_DIR}/VFX_Relic_Pot_Suction.prefab");

            // 2. W_PIPE
            SetPrefabComponentField($"{WEAPONS_PREFAB_DIR}/Weapon_W_PIPE.prefab", "smokeVfxPrefab", $"{PREFAB_DIR}/VFX_Relic_Pipe_DragonSmoke.prefab");

            // 3. W_SLIPPER
            SetPrefabComponentField($"{WEAPONS_PREFAB_DIR}/Weapon_W_SLIPPER.prefab", "whirlwindVfxPrefab", $"{PREFAB_DIR}/VFX_Relic_Slipper_Whirlwind.prefab");

            // 4. R008
            SetPrefabComponentField($"{WEAPONS_PREFAB_DIR}/Weapon_R008.prefab", "relicVfxPrefab", $"{PREFAB_DIR}/VFX_Relic_ChickenBroom_Smash.prefab");

            // 5. R007
            SetPrefabComponentField($"{WEAPONS_PREFAB_DIR}/Weapon_R007.prefab", "matVfxPrefab", $"{PREFAB_DIR}/VFX_Relic_SleepingMat_Decal.prefab");
        }

        private static void SetPrefabComponentField(string prefabPath, string fieldName, string vfxPrefabPath)
        {
            if (!File.Exists(prefabPath)) return;
            GameObject vfxObj = AssetDatabase.LoadAssetAtPath<GameObject>(vfxPrefabPath);
            if (vfxObj == null) return;

            using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                GameObject root = scope.prefabContentsRoot;
                SerializedObject so = new SerializedObject(root.GetComponent<MonoBehaviour>());
                var prop = so.FindProperty(fieldName);
                if (prop != null)
                {
                    prop.objectReferenceValue = vfxObj;
                    so.ApplyModifiedProperties();
                    Debug.Log($"[WeaponVFXAssembler] Đã gán '{vfxPrefabPath}' vào '{fieldName}' của '{prefabPath}'");
                }
            }
        }

        private static void SavePrefab(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            GameObject.DestroyImmediate(go);
            Debug.Log($"[WeaponVFXAssembler] Created/Updated Prefab at: {path}");
        }
    }
}
#endif
