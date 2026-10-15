using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.EditorTools.Migration
{
    /// <summary>
    /// Công cụ Editor hỗ trợ thiết lập toàn bộ môi trường Sandbox 3D tự động (Zero-risk Setup):
    /// 1. Tạo thư mục và Scene Playground 3D độc lập tại Assets/Scenes/3D_Migration/Scene_3D_Playground.unity
    /// 2. Dựng mặt sàn 3D, vật cản 3D, hệ thống chiếu sáng Directional Light và Camera Top-down Isometric 3D.
    /// 3. Khởi tạo đối tượng Player 3D Prototype để kiểm thử ngay lập tức cơ chế di chuyển và lướt thân pháp X-Z.
    /// 4. Đảm bảo tuân thủ tuyệt đối quy tắc AGENTS.md.
    /// </summary>
    public static class Migration3DSetupTool
    {
        private const string SceneFolderPath = "Assets/Scenes/3D_Migration";
        private const string ScenePath = "Assets/Scenes/3D_Migration/Scene_3D_Playground.unity";

        [MenuItem("Tools/Project Zombie/3D Migration/Build 3D Sandbox Playground Scene")]
        public static void Build3DPlaygroundScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("[3D Migration] Vui lòng tắt Play Mode trước khi tạo Scene 3D Sandbox.");
                return;
            }

            // Đảm bảo thư mục tồn tại
            if (!AssetDatabase.IsValidFolder("Assets/Scenes/3D_Migration"))
            {
                AssetDatabase.CreateFolder("Assets/Scenes", "3D_Migration");
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            try
            {
                // 1. Ánh sáng 3D (Directional Light)
                var lightObj = new GameObject("Directional Light 3D");
                var light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1.0f, 0.96f, 0.90f);
                light.intensity = 1.2f;
                light.shadows = LightShadows.Soft;
                lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

                // 2. Môi trường sàn 3D (Ground Plane)
                var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "Ground_3D_Arena";
                ground.transform.position = Vector3.zero;
                ground.transform.localScale = new Vector3(5f, 1f, 5f); // 50m x 50m
                ground.tag = "Untagged";

                int groundLayer = LayerMask.NameToLayer("Ground");
                if (groundLayer >= 0) ground.layer = groundLayer;

                var groundMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                groundMat.color = new Color(0.22f, 0.28f, 0.25f); // Xanh rêu cổ phong
                ground.GetComponent<MeshRenderer>().sharedMaterial = groundMat;

                // 3. Vật cản 3D (Obstacles & Walls)
                var obstacleParent = new GameObject("[Obstacles]");
                int obstacleLayer = LayerMask.NameToLayer("Obstacle");

                CreateObstacleBox(obstacleParent.transform, "Wall_North", new Vector3(0f, 1.5f, 25f), new Vector3(50f, 3f, 1f), obstacleLayer);
                CreateObstacleBox(obstacleParent.transform, "Wall_South", new Vector3(0f, 1.5f, -25f), new Vector3(50f, 3f, 1f), obstacleLayer);
                CreateObstacleBox(obstacleParent.transform, "Wall_East", new Vector3(25f, 1.5f, 0f), new Vector3(1f, 3f, 50f), obstacleLayer);
                CreateObstacleBox(obstacleParent.transform, "Wall_West", new Vector3(-25f, 1.5f, 0f), new Vector3(1f, 3f, 50f), obstacleLayer);

                // Thêm vài cột đá / tảng đá cản đường ở trung tâm
                CreateObstaclePillar(obstacleParent.transform, "Pillar_Center_Left", new Vector3(-6f, 1.5f, 5f), obstacleLayer);
                CreateObstaclePillar(obstacleParent.transform, "Pillar_Center_Right", new Vector3(6f, 1.5f, -5f), obstacleLayer);

                // 4. Đối tượng Player 3D Prototype
                var playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                playerObj.name = "Player_3D_Prototype";
                playerObj.transform.position = new Vector3(0f, 1.0f, 0f);
                playerObj.tag = "Player";
                int playerLayer = LayerMask.NameToLayer("Player");
                if (playerLayer >= 0) playerObj.layer = playerLayer;

                var playerMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                playerMat.color = new Color(0.15f, 0.65f, 0.95f); // Xanh ngọc phát quang
                playerObj.GetComponent<MeshRenderer>().sharedMaterial = playerMat;

                // Rigidbody 3D
                var rb = playerObj.AddComponent<Rigidbody>();
                rb.mass = 1f;
                rb.drag = 0f;
                rb.angularDrag = 0.05f;
                rb.useGravity = true;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

                // Player Stats & Controller
                var stats = playerObj.AddComponent<PlayerStats>();
                var magnet = playerObj.AddComponent<PlayerMagnetTrigger>();
                var controller = playerObj.AddComponent<PlayerController>();
                var health = playerObj.AddComponent<HealthSystem>();

                // Gắn mũi tên chỉ hướng (Visual Nose) để quan sát góc xoay 3D
                var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
                nose.name = "Visual_Facing_Indicator";
                nose.transform.SetParent(playerObj.transform, false);
                nose.transform.localPosition = new Vector3(0f, 0.4f, 0.5f);
                nose.transform.localScale = new Vector3(0.2f, 0.2f, 0.6f);
                Object.DestroyImmediate(nose.GetComponent<Collider>());
                var noseMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                noseMat.color = Color.yellow;
                nose.GetComponent<MeshRenderer>().sharedMaterial = noseMat;

                // 5. Camera 3D Top-down Isometric
                var camObj = new GameObject("Main Camera 3D");
                var cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                camObj.transform.position = new Vector3(0f, 18f, -13f);
                camObj.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
                cam.fieldOfView = 45f;
                cam.nearClipPlane = 0.3f;
                cam.farClipPlane = 1000f;
                camObj.AddComponent<AudioListener>();

                // Camera follow script đơn giản gắn liền
                var follower = camObj.AddComponent<Camera3DFollower>();
                follower.target = playerObj.transform;
                follower.offset = new Vector3(0f, 18f, -13f);

                // 6. Quái vật Dummy 3D thử nghiệm
                CreateDummyEnemy(new Vector3(4f, 1f, 8f), "Enemy_3D_Dummy_A");
                CreateDummyEnemy(new Vector3(-5f, 1f, 7f), "Enemy_3D_Dummy_B");

                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.Refresh();

                Debug.Log($"<color=#55FF55><b>[3D Migration Thành Công]</b></color> Đã tạo xong Scene Sandbox 3D tại: {ScenePath}!");
                EditorUtility.DisplayDialog("3D Migration", $"Đã tạo thành công Scene Sandbox 3D tại:\n{ScenePath}\n\nBạn có thể mở scene này ngay bây giờ để kiểm thử!", "OK");
            }
            finally
            {
                // Không đóng scene vì muốn người dùng có thể thấy ngay
            }
        }

        private static void CreateObstacleBox(Transform parent, string name, Vector3 pos, Vector3 size, int layer)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.position = pos;
            box.transform.localScale = size;
            if (layer >= 0) box.layer = layer;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.4f, 0.35f, 0.3f);
            box.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private static void CreateObstaclePillar(Transform parent, string name, Vector3 pos, int layer)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = name;
            pillar.transform.SetParent(parent, false);
            pillar.transform.position = pos;
            pillar.transform.localScale = new Vector3(2f, 2f, 2f);
            if (layer >= 0) pillar.layer = layer;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.6f, 0.2f, 0.2f); // Trụ đá phong ấn đỏ
            pillar.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private static void CreateDummyEnemy(Vector3 pos, string name)
        {
            var dummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dummy.name = name;
            dummy.transform.position = pos;
            dummy.tag = "Enemy";

            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0) dummy.layer = enemyLayer;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.9f, 0.2f, 0.2f); // Quái đỏ
            dummy.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var rb = dummy.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            var health = dummy.AddComponent<HealthSystem>();
            // Gán máu tối đa 100
            var serializedHealth = new SerializedObject(health);
            var maxHealthProp = serializedHealth.FindProperty("maxHealth");
            if (maxHealthProp != null)
            {
                maxHealthProp.floatValue = 100f;
                serializedHealth.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }

    /// <summary>
    /// Component hỗ trợ Camera 3D bám mượt mà theo Player trong Scene Playground.
    /// </summary>
    public class Camera3DFollower : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0f, 18f, -13f);
        public float smoothSpeed = 10f;

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }
}
