using UnityEngine;
using UnityEditor;
using System.IO;

namespace ProjectZombie.Editor.VFX
{
    public static class CharacterVFXBuilder
    {
        private const string PREFAB_DIR = "Assets/VFX/SkillLibrary/Prefabs";
        private const string MAT_DIR = "Assets/VFX/SkillLibrary/Materials";
        private const string TEX_PATH = "Assets/Art/Skills/Tex_ThuSinh_InkSlash.png";

        [MenuItem("Tools/VFX Generator/Build Thu Sinh Ink Slash VFX", false, 1)]
        public static void BuildThuSinhInkSlash()
        {
            if (!Directory.Exists(PREFAB_DIR)) Directory.CreateDirectory(PREFAB_DIR);
            if (!Directory.Exists(MAT_DIR)) Directory.CreateDirectory(MAT_DIR);

            // 1. Cấu hình Import cho Texture
            TextureImporter importer = AssetImporter.GetAtPath(TEX_PATH) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_PATH);

            // 2. Tạo Material URP Additive / AlphaBlend
            string matPath = $"{MAT_DIR}/MAT_ThuSinh_InkSlash.mat";
            Shader shader = Shader.Find("ProjectZombie/VFX/Slash_Additive");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            if (mat != null)
            {
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                if (mat.HasProperty("_CoreColor")) mat.SetColor("_CoreColor", new Color(2.5f, 2.2f, 1.2f, 1.0f));
                if (mat.HasProperty("_EdgeColor")) mat.SetColor("_EdgeColor", new Color(0.95f, 0.75f, 0.2f, 1.0f));
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", new Color(2.0f, 1.6f, 0.4f, 1.0f));
                EditorUtility.SetDirty(mat);
            }

            // 3. Xây dựng Prefab Particle System
            string prefabPath = $"{PREFAB_DIR}/VFX_ThuSinh_InkSlash.prefab";
            GameObject rootObj = new GameObject("VFX_ThuSinh_InkSlash");

            // Root Particle
            ParticleSystem ps = rootObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.25f;
            main.loop = false;
            main.startLifetime = 0.22f;
            main.startSpeed = 0f;
            main.startSize = 3.2f;
            main.startRotation3D = false;
            main.startRotation = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.playOnAwake = true;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 1) });

            var shape = ps.shape;
            shape.enabled = false;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var sizeLife = ps.sizeOverLifetime;
            sizeLife.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 0.7f);
            sizeCurve.AddKey(0.4f, 1.15f);
            sizeCurve.AddKey(1f, 1.0f);
            sizeLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var psRenderer = rootObj.GetComponent<ParticleSystemRenderer>();
            psRenderer.material = mat;
            psRenderer.sortingLayerName = "Skill";
            psRenderer.sortingOrder = 10;
            psRenderer.alignment = ParticleSystemRenderSpace.Local;

            // Thêm lớp Bụi mực vàng văng ra (Gold Ink Splatters)
            GameObject sparksObj = new GameObject("Gold_Ink_Splatters");
            sparksObj.transform.SetParent(rootObj.transform, false);

            ParticleSystem sparksPS = sparksObj.AddComponent<ParticleSystem>();
            var sMain = sparksPS.main;
            sMain.duration = 0.3f;
            sMain.loop = false;
            sMain.startLifetime = 0.35f;
            sMain.startSpeed = 4.5f;
            sMain.startSize = 0.15f;
            sMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var sEmission = sparksPS.emission;
            sEmission.rateOverTime = 0;
            sEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.02f, 12) });

            var sShape = sparksPS.shape;
            sShape.enabled = true;
            sShape.shapeType = ParticleSystemShapeType.Circle;
            sShape.arc = 120f;
            sShape.radius = 1.2f;

            var sRenderer = sparksObj.GetComponent<ParticleSystemRenderer>();
            Material sparksMat = AssetDatabase.LoadAssetAtPath<Material>($"{MAT_DIR}/MAT_HitSparks_Additive.mat");
            if (sparksMat == null) sparksMat = mat;
            sRenderer.material = sparksMat;
            sRenderer.sortingLayerName = "Skill";
            sRenderer.sortingOrder = 12;

            // Lưu Prefab
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(rootObj, prefabPath);
            GameObject.DestroyImmediate(rootObj);

            // 4. Gán tự động vào CharacterDataSO (Chuẩn mới)
            var thuSinhSO = AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.Player.CharacterDataSO>("Assets/_Data/Characters/Hero_ThuSinh.asset");
            if (thuSinhSO != null)
            {
                thuSinhSO.basicAttackConfig.slashVfxPrefab = savedPrefab;
                thuSinhSO.basicAttackConfig.attackName = "Bút Lệnh Phán Tà";
                thuSinhSO.basicAttackConfig.meleeAreaSize = new Vector2(3.2f, 2.5f);
                thuSinhSO.basicAttackConfig.meleeOffset = 1.2f;
                EditorUtility.SetDirty(thuSinhSO);
                AssetDatabase.SaveAssets();
                Debug.Log($"[CharacterVFXBuilder] Đã gán thành công {prefabPath} cho nhân vật Thư Sinh!");
            }

            AssetDatabase.Refresh();
            Debug.Log($"[CharacterVFXBuilder] Hoàn tất dựng VFX Thư Sinh tại: {prefabPath}");
        }

        private const string DAOSI_SLASH_TEX = "Assets/Art/Skills/Tex_DaoSi_SwordSlash.png";

        [MenuItem("Tools/VFX Generator/Build Dao Si Sword Slash VFX", false, 2)]
        public static void BuildDaoSiSwordSlash()
        {
            if (!Directory.Exists(PREFAB_DIR)) Directory.CreateDirectory(PREFAB_DIR);
            if (!Directory.Exists(MAT_DIR)) Directory.CreateDirectory(MAT_DIR);

            // 1. Cấu hình Import cho Texture
            TextureImporter importer = AssetImporter.GetAtPath(DAOSI_SLASH_TEX) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(DAOSI_SLASH_TEX);

            // 2. Tạo Material URP Additive Kiếm Khí Tiên Đạo
            string matPath = $"{MAT_DIR}/MAT_DaoSi_SwordSlash.mat";
            Shader shader = Shader.Find("ProjectZombie/VFX/Slash_Additive");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            if (mat != null)
            {
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                if (mat.HasProperty("_CoreColor")) mat.SetColor("_CoreColor", new Color(1.8f, 2.5f, 2.2f, 1.0f));
                if (mat.HasProperty("_EdgeColor")) mat.SetColor("_EdgeColor", new Color(0.2f, 0.9f, 0.7f, 1.0f));
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", new Color(0.3f, 2.2f, 1.6f, 1.0f));
                EditorUtility.SetDirty(mat);
            }

            // 3. Xây dựng Prefab Particle System
            string prefabPath = $"{PREFAB_DIR}/VFX_DaoSi_SwordSlash.prefab";
            GameObject rootObj = new GameObject("VFX_DaoSi_SwordSlash");

            // Root Particle (Vệt Kiếm Khí Xanh Ngọc)
            ParticleSystem ps = rootObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.22f;
            main.loop = false;
            main.startLifetime = 0.20f;
            main.startSpeed = 0f;
            main.startSize = 3.3f;
            main.startRotation3D = false;
            main.startRotation = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.playOnAwake = true;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 1) });

            var shape = ps.shape;
            shape.enabled = false;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.65f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var sizeLife = ps.sizeOverLifetime;
            sizeLife.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 0.65f);
            sizeCurve.AddKey(0.4f, 1.2f);
            sizeCurve.AddKey(1f, 1.0f);
            sizeLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var psRenderer = rootObj.GetComponent<ParticleSystemRenderer>();
            psRenderer.material = mat;
            psRenderer.sortingLayerName = "Skill";
            psRenderer.sortingOrder = 10;
            psRenderer.alignment = ParticleSystemRenderSpace.Local;

            // Thêm lớp Tia Sét Tiên Đạo Văng Ra (Jade Lightning Sparks)
            GameObject sparksObj = new GameObject("Jade_Lightning_Sparks");
            sparksObj.transform.SetParent(rootObj.transform, false);

            ParticleSystem sparksPS = sparksObj.AddComponent<ParticleSystem>();
            var sMain = sparksPS.main;
            sMain.duration = 0.3f;
            sMain.loop = false;
            sMain.startLifetime = 0.35f;
            sMain.startSpeed = 5.0f;
            sMain.startSize = 0.18f;
            sMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var sEmission = sparksPS.emission;
            sEmission.rateOverTime = 0;
            sEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.02f, 14) });

            var sShape = sparksPS.shape;
            sShape.enabled = true;
            sShape.shapeType = ParticleSystemShapeType.Circle;
            sShape.arc = 130f;
            sShape.radius = 1.3f;

            var sRenderer = sparksObj.GetComponent<ParticleSystemRenderer>();
            Material sparksMat = AssetDatabase.LoadAssetAtPath<Material>($"{MAT_DIR}/MAT_HitSparks_Additive.mat");
            if (sparksMat == null) sparksMat = mat;
            sRenderer.material = sparksMat;
            sRenderer.sortingLayerName = "Skill";
            sRenderer.sortingOrder = 12;

            // Lưu Prefab
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(rootObj, prefabPath);
            GameObject.DestroyImmediate(rootObj);

            // 4. Gán tự động vào CharacterDataSO (Chuẩn mới) cho Đạo Sĩ
            var daoSiSO = AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.Player.CharacterDataSO>("Assets/_Data/Characters/Hero_DaoSi.asset");
            if (daoSiSO != null)
            {
                daoSiSO.basicAttackConfig.attackType = ProjectZombie.Features.Player.CharacterAttackType.MeleeSlash;
                daoSiSO.basicAttackConfig.slashVfxPrefab = savedPrefab;
                daoSiSO.basicAttackConfig.projectilePrefab = null;
                daoSiSO.basicAttackConfig.attackName = "Tiên Đạo Kiếm Khí";
                daoSiSO.basicAttackConfig.meleeAreaSize = new Vector2(3.3f, 2.4f);
                daoSiSO.basicAttackConfig.meleeOffset = 1.25f;
                daoSiSO.basicAttackConfig.baseAttackSpeed = 2.0f;
                EditorUtility.SetDirty(daoSiSO);
                AssetDatabase.SaveAssets();
                Debug.Log($"[CharacterVFXBuilder] Đã gán thành công {prefabPath} cho nhân vật Đạo Sĩ!");
            }

            AssetDatabase.Refresh();
            Debug.Log($"[CharacterVFXBuilder] Hoàn tất dựng VFX Đạo Sĩ tại: {prefabPath}");
        }

        private const string THANHDONG_WAVE_TEX = "Assets/Art/Skills/Tex_ThanhDong_AirWave.png";
        private const string THANHDONG_SPARK_TEX = "Assets/Art/Skills/Tex_ThanhDong_PetalSpark.png";

        [MenuItem("Tools/VFX Generator/Build Thanh Dong Air Wave VFX", false, 3)]
        public static void BuildThanhDongAirWave()
        {
            if (!Directory.Exists(PREFAB_DIR)) Directory.CreateDirectory(PREFAB_DIR);
            if (!Directory.Exists(MAT_DIR)) Directory.CreateDirectory(MAT_DIR);

            // 1. Cấu hình Import cho 2 Texture
            TextureImporter waveImporter = AssetImporter.GetAtPath(THANHDONG_WAVE_TEX) as TextureImporter;
            if (waveImporter != null)
            {
                waveImporter.textureType = TextureImporterType.Sprite;
                waveImporter.spriteImportMode = SpriteImportMode.Single;
                waveImporter.alphaIsTransparency = true;
                waveImporter.mipmapEnabled = false;
                waveImporter.wrapMode = TextureWrapMode.Clamp;
                waveImporter.filterMode = FilterMode.Bilinear;
                waveImporter.SaveAndReimport();
            }

            TextureImporter sparkImporter = AssetImporter.GetAtPath(THANHDONG_SPARK_TEX) as TextureImporter;
            if (sparkImporter != null)
            {
                sparkImporter.textureType = TextureImporterType.Default;
                sparkImporter.alphaIsTransparency = true;
                sparkImporter.mipmapEnabled = false;
                sparkImporter.wrapMode = TextureWrapMode.Clamp;
                sparkImporter.filterMode = FilterMode.Bilinear;
                sparkImporter.SaveAndReimport();
            }

            Texture2D waveTex = AssetDatabase.LoadAssetAtPath<Texture2D>(THANHDONG_WAVE_TEX);
            Texture2D sparkTex = AssetDatabase.LoadAssetAtPath<Texture2D>(THANHDONG_SPARK_TEX);

            // 2. Tạo Material URP Additive Sóng Khí Linh Lực
            Shader shader = Shader.Find("ProjectZombie/VFX/Slash_Additive");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

            // Mat Wave
            string waveMatPath = $"{MAT_DIR}/MAT_ThanhDong_AirWave.mat";
            Material waveMat = AssetDatabase.LoadAssetAtPath<Material>(waveMatPath);
            if (waveMat == null)
            {
                waveMat = new Material(shader);
                AssetDatabase.CreateAsset(waveMat, waveMatPath);
            }
            if (waveMat != null)
            {
                if (waveMat.HasProperty("_MainTex")) waveMat.SetTexture("_MainTex", waveTex);
                if (waveMat.HasProperty("_BaseMap")) waveMat.SetTexture("_BaseMap", waveTex);
                if (waveMat.HasProperty("_CoreColor")) waveMat.SetColor("_CoreColor", new Color(2.5f, 2.0f, 2.2f, 1.0f));
                if (waveMat.HasProperty("_EdgeColor")) waveMat.SetColor("_EdgeColor", new Color(1.0f, 0.35f, 0.55f, 1.0f)); // Hồng phấn Tứ Phủ
                if (waveMat.HasProperty("_EmissionColor")) waveMat.SetColor("_EmissionColor", new Color(2.2f, 0.6f, 1.0f, 1.0f));
                EditorUtility.SetDirty(waveMat);
            }

            // Mat Sparkles
            string sparkMatPath = $"{MAT_DIR}/MAT_ThanhDong_PetalSpark.mat";
            Material sparkMat = AssetDatabase.LoadAssetAtPath<Material>(sparkMatPath);
            if (sparkMat == null)
            {
                sparkMat = new Material(shader);
                AssetDatabase.CreateAsset(sparkMat, sparkMatPath);
            }
            if (sparkMat != null)
            {
                if (sparkMat.HasProperty("_MainTex")) sparkMat.SetTexture("_MainTex", sparkTex);
                if (sparkMat.HasProperty("_BaseMap")) sparkMat.SetTexture("_BaseMap", sparkTex);
                if (sparkMat.HasProperty("_CoreColor")) sparkMat.SetColor("_CoreColor", new Color(2.2f, 2.2f, 2.5f, 1.0f));
                if (sparkMat.HasProperty("_EdgeColor")) sparkMat.SetColor("_EdgeColor", new Color(1.0f, 0.45f, 0.65f, 1.0f));
                EditorUtility.SetDirty(sparkMat);
            }

            // 3. Xây dựng Prefab Projectile Sóng Khí Xuyên Quái
            string prefabPath = $"{PREFAB_DIR}/Projectile_ThanhDong_AirWave.prefab";
            GameObject projObj = new GameObject("Projectile_ThanhDong_AirWave");

            var rb = projObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Dùng CapsuleCollider2D dạng đứng ôm trọn bề ngang vệt sóng mềm mại (0.22m dày x 1.05m rộng)
            var col = projObj.AddComponent<CapsuleCollider2D>();
            col.isTrigger = true;
            col.direction = CapsuleDirection2D.Vertical;
            col.size = new Vector2(0.22f, 1.05f);

            var simpleProj = projObj.AddComponent<ProjectZombie.Features.Projectiles.SimpleProjectile>();
            simpleProj.SetPiercing(true, 4); // Xuyên qua 4 quái vật trên đường bay

            // 1. Thân Đạn: Dùng SpriteRenderer đơn lẻ (Chỉ có DUY NHẤT 1 làn sóng khí sắc nét)
            Sprite waveSprite = AssetDatabase.LoadAssetAtPath<Sprite>(THANHDONG_WAVE_TEX);
            if (waveSprite == null)
            {
                // Đảm bảo TextureImporter là Sprite
                TextureImporter importer = AssetImporter.GetAtPath(THANHDONG_WAVE_TEX) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();
                }
                waveSprite = AssetDatabase.LoadAssetAtPath<Sprite>(THANHDONG_WAVE_TEX);
            }

            var sr = projObj.AddComponent<SpriteRenderer>();
            sr.sprite = waveSprite;
            sr.material = waveMat;
            sr.sortingLayerName = "Skill";
            sr.sortingOrder = 10;
            projObj.transform.localScale = Vector3.one; // Giữ nguyên tỉ lệ 1:1 chuẩn GameObject

            // Trail Particle (Chùm Bụi Hoa / Linh Lực Rơi Rụng Phía Sau)
            GameObject trailObj = new GameObject("Petal_Sparks_Trail");
            trailObj.transform.SetParent(projObj.transform, false);

            ParticleSystem psTrail = trailObj.AddComponent<ParticleSystem>();
            var tMain = psTrail.main;
            tMain.duration = 1.0f;
            tMain.loop = true;
            tMain.startLifetime = 0.4f;
            tMain.startSpeed = 0f;
            tMain.startSize = 0.35f;
            tMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var tEmission = psTrail.emission;
            tEmission.rateOverTime = 25;

            var tSize = psTrail.sizeOverLifetime;
            tSize.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 1f);
            curve.AddKey(1f, 0.2f);
            tSize.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var tRenderer = trailObj.GetComponent<ParticleSystemRenderer>();
            tRenderer.material = sparkMat;
            tRenderer.sortingLayerName = "Skill";
            tRenderer.sortingOrder = 9;
            tRenderer.alignment = ParticleSystemRenderSpace.Facing;

            // Lưu Prefab
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(projObj, prefabPath);
            GameObject.DestroyImmediate(projObj);

            // 4. Gán tự động vào CharacterDataSO (Chuẩn mới) cho Thanh Đồng
            var thanhDongSO = AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.Player.CharacterDataSO>("Assets/_Data/Characters/Hero_ThanhDong.asset");
            if (thanhDongSO != null)
            {
                thanhDongSO.basicAttackConfig.attackType = ProjectZombie.Features.Player.CharacterAttackType.RangedProjectile;
                thanhDongSO.basicAttackConfig.projectilePrefab = savedPrefab;
                thanhDongSO.basicAttackConfig.slashVfxPrefab = null;
                thanhDongSO.basicAttackConfig.attackName = "Linh Lụa Tứ Phủ";
                thanhDongSO.basicAttackConfig.projectileSpeed = 8.0f; // Bay chậm rãi mềm mại
                thanhDongSO.basicAttackConfig.projectileLifetime = 1.4f;
                thanhDongSO.basicAttackConfig.projectileCount = 1;
                thanhDongSO.basicAttackConfig.spreadAngle = 0f;
                thanhDongSO.basicAttackConfig.baseAttackSpeed = 1.8f;
                EditorUtility.SetDirty(thanhDongSO);
                AssetDatabase.SaveAssets();
                Debug.Log($"[CharacterVFXBuilder] Đã gán thành công {prefabPath} cho nhân vật Thanh Đồng!");
            }

            AssetDatabase.Refresh();
            Debug.Log($"[CharacterVFXBuilder] Hoàn tất dựng VFX Thanh Đồng tại: {prefabPath}");
        }

        private const string ANSI_SLASH_TEX = "Assets/Art/Skills/Tex_AnSi_EarthImpactSlash.png";

        [MenuItem("Tools/VFX Generator/Build An Si Earth Impact Slash VFX", false, 4)]
        public static void BuildAnSiEarthImpactSlash()
        {
            if (!Directory.Exists(PREFAB_DIR)) Directory.CreateDirectory(PREFAB_DIR);
            if (!Directory.Exists(MAT_DIR)) Directory.CreateDirectory(MAT_DIR);

            // 1. Cấu hình Import cho Texture
            TextureImporter importer = AssetImporter.GetAtPath(ANSI_SLASH_TEX) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(ANSI_SLASH_TEX);

            // 2. Tạo Material URP Additive Thạch Thể Chấn Địa
            string matPath = $"{MAT_DIR}/MAT_AnSi_EarthImpactSlash.mat";
            Shader shader = Shader.Find("ProjectZombie/VFX/Slash_Additive");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            if (mat != null)
            {
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                if (mat.HasProperty("_CoreColor")) mat.SetColor("_CoreColor", new Color(2.5f, 2.0f, 1.0f, 1.0f));
                if (mat.HasProperty("_EdgeColor")) mat.SetColor("_EdgeColor", new Color(0.85f, 0.55f, 0.2f, 1.0f));
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", new Color(2.0f, 1.2f, 0.3f, 1.0f));
                EditorUtility.SetDirty(mat);
            }

            // 3. Xây dựng Prefab Particle System
            string prefabPath = $"{PREFAB_DIR}/VFX_AnSi_EarthImpactSlash.prefab";
            GameObject rootObj = new GameObject("VFX_AnSi_EarthImpactSlash");

            // Root Particle (Vệt chém nứt đất đá)
            ParticleSystem ps = rootObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.28f;
            main.loop = false;
            main.startLifetime = 0.24f;
            main.startSpeed = 0f;
            main.startSize = 3.6f;
            main.startRotation3D = false;
            main.startRotation = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.playOnAwake = true;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 1) });

            var shape = ps.shape;
            shape.enabled = false;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.65f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var sizeLife = ps.sizeOverLifetime;
            sizeLife.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 0.65f);
            sizeCurve.AddKey(0.4f, 1.2f);
            sizeCurve.AddKey(1f, 1.0f);
            sizeLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var psRenderer = rootObj.GetComponent<ParticleSystemRenderer>();
            psRenderer.material = mat;
            psRenderer.sortingLayerName = "Skill";
            psRenderer.sortingOrder = 10;
            psRenderer.alignment = ParticleSystemRenderSpace.Local;

            // Thêm lớp Bụi Sỏi Văng Ra (Earth Shards Splatters)
            GameObject sparksObj = new GameObject("Earth_Shards");
            sparksObj.transform.SetParent(rootObj.transform, false);

            ParticleSystem sparksPS = sparksObj.AddComponent<ParticleSystem>();
            var sMain = sparksPS.main;
            sMain.duration = 0.35f;
            sMain.loop = false;
            sMain.startLifetime = 0.4f;
            sMain.startSpeed = 5.5f;
            sMain.startSize = 0.22f;
            sMain.simulationSpace = ParticleSystemSimulationSpace.World;

            var sEmission = sparksPS.emission;
            sEmission.rateOverTime = 0;
            sEmission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.02f, 16) });

            var sShape = sparksPS.shape;
            sShape.enabled = true;
            sShape.shapeType = ParticleSystemShapeType.Circle;
            sShape.arc = 140f;
            sShape.radius = 1.4f;

            var sRenderer = sparksObj.GetComponent<ParticleSystemRenderer>();
            Material sparksMat = AssetDatabase.LoadAssetAtPath<Material>($"{MAT_DIR}/MAT_HitSparks_Additive.mat");
            if (sparksMat == null) sparksMat = mat;
            sRenderer.material = sparksMat;
            sRenderer.sortingLayerName = "Skill";
            sRenderer.sortingOrder = 12;

            // Lưu Prefab
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(rootObj, prefabPath);
            GameObject.DestroyImmediate(rootObj);

            // 4. Gán tự động vào CharacterDataSO (Chuẩn mới) cho Ẩn Sĩ
            var anSiSO = AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.Player.CharacterDataSO>("Assets/_Data/Characters/Hero_AnSi.asset");
            if (anSiSO != null)
            {
                anSiSO.basicAttackConfig.attackType = ProjectZombie.Features.Player.CharacterAttackType.MeleeSlash;
                anSiSO.basicAttackConfig.slashVfxPrefab = savedPrefab;
                anSiSO.basicAttackConfig.attackName = "Thạch Quyền Phá Địa";
                anSiSO.basicAttackConfig.meleeAreaSize = new Vector2(3.6f, 2.7f);
                anSiSO.basicAttackConfig.meleeOffset = 1.35f;
                anSiSO.basicAttackConfig.baseAttackSpeed = 1.6f;
                anSiSO.basicAttackConfig.knockbackForce = 6.0f; // Ẩn sĩ đấm siêu đầm
                EditorUtility.SetDirty(anSiSO);
                AssetDatabase.SaveAssets();
                Debug.Log($"[CharacterVFXBuilder] Đã gán thành công {prefabPath} cho nhân vật Ẩn Sĩ!");
            }

            AssetDatabase.Refresh();
            Debug.Log($"[CharacterVFXBuilder] Hoàn tất dựng VFX Ẩn Sĩ tại: {prefabPath}");
        }

        [MenuItem("Tools/VFX Generator/🎨 Auto Assign Character Attack Icons", false, 5)]
        public static void AssignAllAttackIcons()
        {
            string[] iconPaths = new string[]
            {
                "Assets/Art/UI/Skills/Icon_Atk_ThuSinh_Brush.png",
                "Assets/Art/UI/Skills/Icon_Atk_DaoSi_Sword.png",
                "Assets/Art/UI/Skills/Icon_Atk_ThanhDong_Torch.png",
                "Assets/Art/UI/Skills/Icon_Atk_AnSi_Fist.png",
                "Assets/Art/UI/HUD/Tex_Attack_Aim_Arrow.png",
                "Assets/Art/UI/HUD/Tex_Attack_Aim_Arc_Reticle.png"
            };

            foreach (var path in iconPaths)
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.SaveAndReimport();
                }
            }

            // Gán trực tiếp vào các CharacterDataSO độc lập
            var soThuSinh = AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.Player.CharacterDataSO>("Assets/_Data/Characters/Hero_ThuSinh.asset");
            if (soThuSinh != null) { soThuSinh.basicAttackConfig.attackIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Skills/Icon_Atk_ThuSinh_Brush.png"); EditorUtility.SetDirty(soThuSinh); }

            var soDaoSi = AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.Player.CharacterDataSO>("Assets/_Data/Characters/Hero_DaoSi.asset");
            if (soDaoSi != null) { soDaoSi.basicAttackConfig.attackIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Skills/Icon_Atk_DaoSi_Sword.png"); EditorUtility.SetDirty(soDaoSi); }

            var soThanhDong = AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.Player.CharacterDataSO>("Assets/_Data/Characters/Hero_ThanhDong.asset");
            if (soThanhDong != null) { soThanhDong.basicAttackConfig.attackIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Skills/Icon_Atk_ThanhDong_Torch.png"); EditorUtility.SetDirty(soThanhDong); }

            var soAnSi = AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.Player.CharacterDataSO>("Assets/_Data/Characters/Hero_AnSi.asset");
            if (soAnSi != null) { soAnSi.basicAttackConfig.attackIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Skills/Icon_Atk_AnSi_Fist.png"); EditorUtility.SetDirty(soAnSi); }

            AssetDatabase.SaveAssets();
            Debug.Log("[CharacterVFXBuilder] 🎨 Đã gán thành công 4 Attack Icon cho cả 4 nhân vật!");
        }

        [MenuItem("Tools/VFX Generator/⚡ Build All 4 Character Basic Attack VFX (1-Click)", false, 0)]
        public static void BuildAllCharacterVFX()
        {
            BuildThuSinhInkSlash();
            BuildDaoSiSwordSlash();
            BuildThanhDongAirWave();
            BuildAnSiEarthImpactSlash();
            AssignAllAttackIcons();
            Debug.Log("[CharacterVFXBuilder] 🚀 ĐÃ HOÀN TẤT DỰNG & LIÊN KẾT BỘ 4 VFX & ICON TƯỚNG 1-CLICK!");
        }
    }
}
