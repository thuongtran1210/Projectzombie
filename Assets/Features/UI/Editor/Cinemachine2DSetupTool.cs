#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Cinemachine;
using ProjectZombie.Features.Arena;

namespace ProjectZombie.Features.CameraTools.Editor
{
    /// <summary>
    /// Editor Tool tự động cấu hình Cinemachine 2D Camera cho Scene:
    /// - Gỡ bỏ PixelPerfectCamera lỗi thời khỏi Main Camera.
    /// - Gắn CinemachineBrain vào Main Camera.
    /// - Tạo CinemachineVirtualCamera 2D với cấu hình Framing Transposer mượt mà (Lookahead, Damping).
    /// - Đồng bộ tham chiếu với CameraFollow component.
    /// </summary>
    public static class Cinemachine2DSetupTool
    {
        [MenuItem("ProjectZombie/Camera/⚡ Setup Cinemachine 2D Camera (1-Click)", priority = 10)]
        [MenuItem("Tools/ProjectZombie/Camera/Setup Cinemachine 2D Camera", priority = 10)]
        public static void SetupCinemachine2D()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                mainCam = Object.FindObjectOfType<Camera>();
            }

            if (mainCam == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy Main Camera trong Scene!", "OK");
                return;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Setup Cinemachine 2D Camera");
            int group = Undo.GetCurrentGroup();

            // 1. Gỡ bỏ PixelPerfectCamera nếu có
            var pixelPerfect = mainCam.GetComponent<UnityEngine.U2D.PixelPerfectCamera>();
            if (pixelPerfect != null)
            {
                Undo.DestroyObjectImmediate(pixelPerfect);
                Debug.Log("<color=yellow>[Cinemachine 2D]</color> Đã gỡ bỏ PixelPerfectCamera khỏi Main Camera.");
            }

            // 2. Gắn CinemachineBrain vào Main Camera
            var brain = mainCam.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                brain = Undo.AddComponent<CinemachineBrain>(mainCam.gameObject);
                brain.m_ShowDebugText = false;
                brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 0.5f);
                brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
                Debug.Log("<color=green>[Cinemachine 2D]</color> Đã thêm CinemachineBrain vào Main Camera.");
            }

            // 3. Tìm hoặc Tạo CinemachineVirtualCamera 2D
            GameObject vcamObj = GameObject.Find("CM_VirtualCamera_2D");
            CinemachineVirtualCamera vcam = null;

            if (vcamObj == null)
            {
                vcamObj = new GameObject("CM_VirtualCamera_2D");
                Undo.RegisterCreatedObjectUndo(vcamObj, "Create CM_VirtualCamera_2D");
                vcam = vcamObj.AddComponent<CinemachineVirtualCamera>();
            }
            else
            {
                vcam = vcamObj.GetComponent<CinemachineVirtualCamera>();
                if (vcam == null) vcam = vcamObj.AddComponent<CinemachineVirtualCamera>();
            }

            // 4. Cấu hình Lens & Body (Framing Transposer chuẩn 2D Action)
            vcam.m_Lens.Orthographic = true;
            vcam.m_Lens.OrthographicSize = 5.5f;
            vcam.m_Lens.NearClipPlane = 0.1f;
            vcam.m_Lens.FarClipPlane = 5000f;

            // Cấu hình Body = Framing Transposer
            var transposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (transposer == null)
            {
                transposer = vcam.AddCinemachineComponent<CinemachineFramingTransposer>();
            }

            transposer.m_TrackedObjectOffset = Vector3.zero;
            transposer.m_LookaheadTime = 0.15f; // Dự đoán hướng di chuyển 0.15s
            transposer.m_LookaheadSmoothing = 5f;
            transposer.m_LookaheadIgnoreY = false;

            // Damping (độ êm ái)
            transposer.m_XDamping = 0.5f;
            transposer.m_YDamping = 0.5f;
            transposer.m_ZDamping = 0f;

            // Dead Zone (vùng đứng yên không dịch camera)
            transposer.m_DeadZoneWidth = 0.05f;
            transposer.m_DeadZoneHeight = 0.05f;

            // Soft Zone (vùng camera bắt đầu bám theo)
            transposer.m_SoftZoneWidth = 0.6f;
            transposer.m_SoftZoneHeight = 0.6f;
            transposer.m_BiasX = 0f;
            transposer.m_BiasY = 0f;

            // 5. Thêm CinemachineImpulseListener & ImpulseSource cho rung lắc
            var impulseListener = vcamObj.GetComponent<CinemachineImpulseListener>();
            if (impulseListener == null) vcamObj.AddComponent<CinemachineImpulseListener>();

            var impulseSource = vcamObj.GetComponent<CinemachineImpulseSource>();
            if (impulseSource == null) impulseSource = vcamObj.AddComponent<CinemachineImpulseSource>();

            // 6. Gắn hoặc cập nhật CameraFollow component trên Main Camera
            var cameraFollow = mainCam.GetComponent<CameraFollow>();
            if (cameraFollow == null) cameraFollow = Undo.AddComponent<CameraFollow>(mainCam.gameObject);

            SerializedObject cfSO = new SerializedObject(cameraFollow);
            var vcamProp = cfSO.FindProperty("_virtualCamera");
            if (vcamProp != null) vcamProp.objectReferenceValue = vcam;
            var impulseProp = cfSO.FindProperty("_impulseSource");
            if (impulseProp != null) impulseProp.objectReferenceValue = impulseSource;
            cfSO.ApplyModifiedProperties();

            // 7. Tự động liên kết Player nếu Player đã có trong Scene
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                vcam.Follow = player.transform;
                vcam.LookAt = player.transform;
                cameraFollow.SetTarget(player.transform);
                Debug.Log($"<color=green>[Cinemachine 2D]</color> Đã tự động bám theo Player: {player.name}");
            }

            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = vcamObj;

            EditorUtility.DisplayDialog("Thành công", "Đã nâng cấp hệ thống Camera sang Cinemachine 2D hoàn chỉnh!\n\n- Gỡ bỏ PixelPerfectCamera\n- Cấu hình Framing Transposer & Lookahead\n- Tích hợp Screen Shake Impulse", "OK");
            Debug.Log("<color=green>[Cinemachine 2D]</color> Cấu hình Camera hoàn tất thành công!");
        }
    }
}
#endif
