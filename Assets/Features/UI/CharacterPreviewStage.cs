using UnityEngine;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Sân khấu độc lập render nhân vật thời gian thực qua Camera con và RenderTexture,
    /// cho phép phát Animation Attack / Idle / Cast trực tiếp lên UI (RawImage).
    /// </summary>
    public class CharacterPreviewStage : MonoBehaviour
    {
        public static CharacterPreviewStage Instance { get; private set; }

        [Header("Render Texture Setup")]
        [SerializeField] private Camera _previewCamera;
        [SerializeField] private RenderTexture _renderTexture;
        [SerializeField] private Transform _modelSpawnPoint;

        [Header("Animation Loop Settings")]
        [SerializeField] private string _primaryAnimState = "Attack";
        [SerializeField] private string _idleAnimState = "Idle";
        [SerializeField] private bool _loopAttack = true;
        [SerializeField] private float _attackInterval = 2.5f;

        private GameObject _currentModelInstance;
        private Animator _currentAnimator;
        private float _timer;

        public RenderTexture PreviewTexture => _renderTexture;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            SetupCameraAndTexture();
        }

        private void SetupCameraAndTexture()
        {
            if (_renderTexture == null)
            {
                _renderTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32)
                {
                    name = "RT_CharacterPreview",
                    antiAliasing = 2,
                    filterMode = FilterMode.Bilinear,
                    useMipMap = false
                };
                _renderTexture.Create();
            }

            if (_previewCamera == null)
            {
                _previewCamera = GetComponentInChildren<Camera>();
                if (_previewCamera == null)
                {
                    GameObject camObj = new GameObject("Camera_Preview");
                    camObj.transform.SetParent(transform, false);
                    camObj.transform.localPosition = new Vector3(0, 0.8f, -10f);
                    _previewCamera = camObj.AddComponent<Camera>();
                }
            }

            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = new Color(0, 0, 0, 0); // Nền trong suốt 100%
            _previewCamera.orthographic = true;
            _previewCamera.orthographicSize = 1.6f;
            _previewCamera.nearClipPlane = 0.1f;
            _previewCamera.farClipPlane = 50f;
            _previewCamera.targetTexture = _renderTexture;

            if (_modelSpawnPoint == null)
            {
                Transform sp = transform.Find("ModelSpawnPoint");
                if (sp == null)
                {
                    GameObject spObj = new GameObject("ModelSpawnPoint");
                    spObj.transform.SetParent(transform, false);
                    spObj.transform.localPosition = Vector3.zero;
                    _modelSpawnPoint = spObj.transform;
                }
                else
                {
                    _modelSpawnPoint = sp;
                }
            }
        }

        public void DisplayCharacter(GameObject characterPrefab, string targetAnimation = null)
        {
            ClearCurrentModel();
            if (characterPrefab == null) return;

            // Tìm con Visual hoặc SpriteRenderer của Prefab mẫu để sinh độc lập
            Transform visualSource = characterPrefab.transform.Find("Visual");
            GameObject instance = null;

            if (visualSource != null)
            {
                instance = Instantiate(visualSource.gameObject, _modelSpawnPoint);
                instance.name = "Preview_Visual";
            }
            else
            {
                instance = Instantiate(characterPrefab, _modelSpawnPoint);
                instance.name = "Preview_Model";
                DisableGameplayComponents(instance);
            }

            instance.tag = "Untagged";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            _currentModelInstance = instance;
            _currentAnimator = instance.GetComponentInChildren<Animator>();

            if (_currentAnimator != null)
            {
                _currentAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
                string animToPlay = !string.IsNullOrEmpty(targetAnimation) ? targetAnimation : _idleAnimState;
                PlayAnimation(animToPlay);
            }
        }

        public void PlayIdleAnimation()
        {
            PlayAnimation(_idleAnimState);
        }

        public void PlayAnimation(string stateName)
        {
            if (_currentAnimator == null || string.IsNullOrEmpty(stateName)) return;

            if (_currentAnimator.HasState(0, Animator.StringToHash(stateName)))
            {
                _currentAnimator.Play(stateName, 0, 0f);
            }
            else if (_currentAnimator.HasState(0, Animator.StringToHash("Attack_1")))
            {
                _currentAnimator.Play("Attack_1", 0, 0f);
            }
        }

        private void Update()
        {
            if (_loopAttack && _currentAnimator != null)
            {
                _timer += Time.unscaledDeltaTime;
                if (_timer >= _attackInterval)
                {
                    _timer = 0f;
                    PlayAnimation(_primaryAnimState);
                }
            }
        }

        private void DisableGameplayComponents(GameObject root)
        {
            // Tắt Rigidbody và Colliders
            var rb = root.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;

            var colliders = root.GetComponentsInChildren<Collider2D>(true);
            foreach (var col in colliders) col.enabled = false;

            // Tắt các MonoBehaviour Gameplay (PlayerController, Health, WeaponManager...)
            var behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var b in behaviours)
            {
                if (b != null)
                {
                    b.enabled = false;
                }
            }

            // Đảm bảo SpriteRenderer và Animator con luôn bật
            var renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var r in renderers)
            {
                r.enabled = true;
            }

            var animators = root.GetComponentsInChildren<Animator>(true);
            foreach (var a in animators)
            {
                a.enabled = true;
            }
        }

        public void ClearCurrentModel()
        {
            if (_currentModelInstance != null)
            {
                Destroy(_currentModelInstance);
                _currentModelInstance = null;
                _currentAnimator = null;
            }
            _timer = 0f;
        }

        private void OnDestroy()
        {
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
            }
        }
    }
}
