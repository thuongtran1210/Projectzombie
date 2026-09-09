using System.Collections;
using UnityEngine;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Sân khấu độc lập render nhân vật thời gian thực qua Camera con và RenderTexture,
    /// cho phép phát Animation Attack / Idle / Cast trực tiếp lên UI (RawImage).
    /// Tối ưu 60 FPS không khựng, tự động chuyển đổi mượt mà giữa Chu kỳ Tấn công và Thở Đứng Yên (Idle).
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
        [SerializeField] private float _attackInterval = 3.2f;

        [Header("Chibi Game Feel")]
        [SerializeField] private bool _enableBreathingFloat = true;
        [SerializeField] private float _breathingSpeed = 3.0f;
        [SerializeField] private float _breathingAmount = 0.035f;

        private GameObject _currentModelInstance;
        private Animator _currentAnimator;
        private float _timer;
        private bool _isPlayingAttack = false;

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
                    antiAliasing = 4,
                    filterMode = FilterMode.Bilinear,
                    useMipMap = false,
                    hideFlags = HideFlags.DontSave
                };
            }

            if (_previewCamera == null)
            {
                _previewCamera = GetComponentInChildren<Camera>();
                if (_previewCamera == null)
                {
                    GameObject camObj = new GameObject("Camera_Preview");
                    camObj.transform.SetParent(transform, false);
                    camObj.transform.localPosition = new Vector3(0, 0.45f, -10f);
                    _previewCamera = camObj.AddComponent<Camera>();
                }
            }

            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = new Color(0, 0, 0, 0); // Nền trong suốt 100%
            _previewCamera.orthographic = true;
            _previewCamera.orthographicSize = 1.45f;
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

        private float _currentAttackDuration = 0.5f;

        public void DisplayCharacter(GameObject characterPrefab, string targetAnimation = null)
        {
            SetupCameraAndTexture();
            ClearCurrentModel();
            if (characterPrefab == null || _modelSpawnPoint == null) return;

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
            instance.transform.SetParent(_modelSpawnPoint, false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            _currentModelInstance = instance;
            _currentAnimator = instance.GetComponentInChildren<Animator>();

            if (_currentAnimator != null)
            {
                // Luôn cập nhật Animator không phụ thuộc vào Main Camera hay Time.timeScale
                _currentAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
                _currentAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                _currentAnimator.applyRootMotion = false;

                // Tính thời lượng thực của clip Attack để transition mượt mà
                CalculateAttackDuration();

                string animToPlay = !string.IsNullOrEmpty(targetAnimation) ? targetAnimation : _idleAnimState;
                if (_currentAnimator.runtimeAnimatorController != null && _currentAnimator.isActiveAndEnabled)
                {
                    if (animToPlay.ToLower().Contains("attack"))
                    {
                        PlayAttackAnimation();
                    }
                    else
                    {
                        PlayIdleAnimation();
                    }
                }
            }
        }

        private void CalculateAttackDuration()
        {
            _currentAttackDuration = 0.45f;
            if (_currentAnimator != null && _currentAnimator.runtimeAnimatorController != null)
            {
                var clips = _currentAnimator.runtimeAnimatorController.animationClips;
                if (clips != null)
                {
                    foreach (var clip in clips)
                    {
                        if (clip != null && clip.name.ToLower().Contains("attack"))
                        {
                            _currentAttackDuration = Mathf.Max(0.25f, clip.length);
                            break;
                        }
                    }
                }
            }
        }

        public void PlayIdleAnimation()
        {
            _isPlayingAttack = false;
            _timer = 0f;
            PlayState(_idleAnimState);
        }

        public void PlayAttackAnimation()
        {
            _isPlayingAttack = true;
            _timer = 0f;
            PlayState(_primaryAnimState);
        }

        private void PlayState(string stateName)
        {
            if (_currentAnimator == null || !_currentAnimator.isActiveAndEnabled || _currentAnimator.runtimeAnimatorController == null || string.IsNullOrEmpty(stateName)) return;

            int stateHash = Animator.StringToHash(stateName);
            if (_currentAnimator.HasState(0, stateHash))
            {
                _currentAnimator.Play(stateHash, 0, 0f);
                _currentAnimator.Update(0f);
            }
            else if (stateName == _primaryAnimState && _currentAnimator.HasState(0, Animator.StringToHash("Attack_1")))
            {
                _currentAnimator.Play("Attack_1", 0, 0f);
                _currentAnimator.Update(0f);
            }
            else if (stateName == _idleAnimState && _currentAnimator.HasState(0, Animator.StringToHash("Default")))
            {
                _currentAnimator.Play("Default", 0, 0f);
                _currentAnimator.Update(0f);
            }
        }

        private void Update()
        {
            // 1. Nhấp nhô thở Chibi tự nhiên tạo cảm giác nhân vật sống động
            if (_enableBreathingFloat && _modelSpawnPoint != null)
            {
                float floatY = Mathf.Sin(Time.unscaledTime * _breathingSpeed) * _breathingAmount;
                _modelSpawnPoint.localPosition = new Vector3(0f, floatY, 0f);
            }

            if (_currentAnimator == null) return;

            // 2. Quản lý chuyển động mượt mà giữa Attack và Idle dựa theo thời gian thực (UnscaledTime)
            if (_isPlayingAttack)
            {
                _timer += Time.unscaledDeltaTime;
                // Khi thời gian đánh kết thúc chuẩn theo độ dài clip animation -> chuyển sang Idle mượt mà
                if (_timer >= _currentAttackDuration)
                {
                    PlayIdleAnimation();
                }
            }
            else if (_loopAttack)
            {
                _timer += Time.unscaledDeltaTime;
                if (_timer >= _attackInterval)
                {
                    PlayAttackAnimation();
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

            // Đảm bảo SpriteRenderer và Animator con luôn bật và render đẹp
            var renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var r in renderers)
            {
                r.enabled = true;
                r.sortingOrder = 1;
            }

            var animators = root.GetComponentsInChildren<Animator>(true);
            foreach (var a in animators)
            {
                a.enabled = true;
                a.updateMode = AnimatorUpdateMode.UnscaledTime;
                a.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }
        }

        public void ClearCurrentModel()
        {
            if (_currentModelInstance != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_currentModelInstance);
                }
                else
                {
                    DestroyImmediate(_currentModelInstance);
                }
                _currentModelInstance = null;
                _currentAnimator = null;
            }

            // Dọn dẹp sạch sẽ các GameObject con còn sót lại trong _modelSpawnPoint
            if (_modelSpawnPoint != null)
            {
                for (int i = _modelSpawnPoint.childCount - 1; i >= 0; i--)
                {
                    var child = _modelSpawnPoint.GetChild(i).gameObject;
                    if (Application.isPlaying) Destroy(child);
                    else DestroyImmediate(child);
                }
            }

            _timer = 0f;
            _isPlayingAttack = false;
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
