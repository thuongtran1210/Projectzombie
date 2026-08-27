using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// View hiển thị thụ động cho Màn hình Kết quả Game Over / Chiến thắng.
    /// Không chứa logic game, chỉ nhận dữ liệu hiển thị từ Presenter.
    /// </summary>
    public class GameOverScreenView : MonoBehaviour
    {
        [Header("Backdrop & Panel Root")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Image backdropDim;

        [Header("Title Banner")]
        [SerializeField] private Image bannerImage;
        [SerializeField] private TMP_Text titleText;

        [Header("Run Stats Display (Value texts)")]
        [SerializeField] private TMP_Text timeAliveText;
        [SerializeField] private TMP_Text killCountText;
        [SerializeField] private TMP_Text maxLevelText;
        [SerializeField] private TMP_Text damageDealtText;

        [Header("Currency Meta")]
        [SerializeField] private TMP_Text currencyEarnedText;
        [SerializeField] private Image currencyIcon;

        [Header("Buttons")]
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button mainMenuButton;

        // Sự kiện gửi lên Presenter khi bấm nút
        public event Action OnPlayAgainClicked;
        public event Action OnMainMenuClicked;

        [Header("Transition")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeInDuration = 0.4f;

        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            EnsureReferences();

            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Ẩn tất cả visual ban đầu
            SetAllChildrenActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (playAgainButton != null)
            {
                playAgainButton.onClick.RemoveAllListeners();
                playAgainButton.onClick.AddListener(() => OnPlayAgainClicked?.Invoke());
            }
            
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(() => OnMainMenuClicked?.Invoke());
            }

            // Đảm bảo Animator hoạt động khi Time.timeScale = 0 (Game Over pause)
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
        }

        private void EnsureReferences()
        {
            if (backdropDim == null)
            {
                backdropDim = transform.Find("Backdrop_Dim")?.GetComponent<Image>();
            }

            if (panel == null || panel == gameObject)
            {
                Transform bg = transform.Find("Background_Panel") ?? transform.Find("Panel_GameOver");
                if (bg != null) panel = bg.gameObject;
            }

            if (bannerImage == null)
            {
                bannerImage = transform.Find("Background_Panel/Banner_Title")?.GetComponent<Image>()
                           ?? transform.Find("Panel_GameOver/Banner_Title")?.GetComponent<Image>();
            }

            if (playAgainButton == null)
            {
                playAgainButton = transform.Find("Background_Panel/PlayAgain_Button")?.GetComponent<Button>()
                               ?? transform.Find("PlayAgain_Button")?.GetComponent<Button>()
                               ?? GetComponentInChildren<Button>(true);
            }

            if (mainMenuButton == null)
            {
                mainMenuButton = transform.Find("Background_Panel/MainMenu_Button")?.GetComponent<Button>()
                              ?? transform.Find("MainMenu_Button")?.GetComponent<Button>();
            }

            if (titleText == null)
            {
                titleText = transform.Find("Background_Panel/Banner_Title/Title_Text")?.GetComponent<TMP_Text>()
                         ?? transform.Find("Title_Text")?.GetComponent<TMP_Text>()
                         ?? transform.Find("Background_Panel/Title_Text")?.GetComponent<TMP_Text>();
            }
        }

        private void SetAllChildrenActive(bool isActive)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(isActive);
            }
        }

        public void SetActive(bool isActive)
        {
            EnsureReferences();

            // Bật toàn bộ các thành phần con trong Hierarchy (Backdrop, Background_Panel, Buttons...)
            SetAllChildrenActive(isActive);

            if (panel != null && panel != gameObject)
            {
                panel.SetActive(isActive);
            }

            if (playAgainButton != null) playAgainButton.gameObject.SetActive(isActive);
            if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(isActive);

            if (canvasGroup != null)
            {
                canvasGroup.interactable = isActive;
                canvasGroup.blocksRaycasts = isActive;

                if (isActive)
                {
                    if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
                    _fadeCoroutine = StartCoroutine(FadeInCoroutine());
                }
                else
                {
                    canvasGroup.alpha = 0f;
                }
            }
        }

        private System.Collections.IEnumerator FadeInCoroutine()
        {
            canvasGroup.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
            _fadeCoroutine = null;
        }

        public void SetBanner(Sprite bannerSprite)
        {
            if (bannerImage != null && bannerSprite != null)
            {
                bannerImage.sprite = bannerSprite;
            }
        }

        public void SetTitle(string text, Color color)
        {
            if (titleText != null)
            {
                titleText.text = text;
                titleText.color = color;
            }
        }

        public void SetTimeAlive(string text)
        {
            if (timeAliveText != null) timeAliveText.text = text;
        }

        public void SetKillCount(string text)
        {
            if (killCountText != null) killCountText.text = text;
        }

        public void SetMaxLevel(string text)
        {
            if (maxLevelText != null) maxLevelText.text = text;
        }

        public void SetDamageDealt(string text)
        {
            if (damageDealtText != null) damageDealtText.text = text;
        }

        public void SetCurrency(string text)
        {
            if (currencyEarnedText != null) currencyEarnedText.text = text;
        }
    }
}
