using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace ProjectZombie.Features.UI.DamageText
{
    /// <summary>
    /// Item hiển thị chữ số sát thương World-Space passive (nhận dữ liệu từ Manager).
    /// Tự động bay nẩy và mờ dần, sau đó trả về Pool.
    /// </summary>
    [RequireComponent(typeof(TextMeshPro))]
    public class DamageTextItem : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _textMesh;

        private Action<DamageTextItem> _onDespawn;

        private void Awake()
        {
            if (_textMesh == null)
            {
                _textMesh = GetComponent<TextMeshPro>();
            }
            if (_textMesh != null)
            {
                _textMesh.alignment = TextAlignmentOptions.Center;
                // Ép buộc hiển thị lên Layer WorldUI (hoặc Layer cao hơn Characters/VFX)
                _textMesh.sortingLayerID = SortingLayer.NameToID("UI_World");
                _textMesh.sortingOrder = 1000; // Đảm bảo luôn đè lên VFX và Enemy
            }
        }

        public void Setup(string text, Color color, float fontSize, Vector3 spawnPosition, DamageTextStyleConfig config, Action<DamageTextItem> onDespawn)
        {
            _onDespawn = onDespawn;
            if (_textMesh == null) return;

            _textMesh.alignment = TextAlignmentOptions.Center;
            _textMesh.text = text;
            _textMesh.color = color;
            _textMesh.fontSize = fontSize;

            // Offset ngẫu nhiên để tránh đè chữ
            Vector3 randomOffset = new Vector3(
                UnityEngine.Random.Range(-config.RandomArcSpread, config.RandomArcSpread),
                UnityEngine.Random.Range(0f, config.RandomArcSpread * 0.5f),
                0f
            );

            transform.position = spawnPosition + randomOffset;
            gameObject.SetActive(true);

            StopAllCoroutines();
            StartCoroutine(AnimateRoutine(config));
        }

        private IEnumerator AnimateRoutine(DamageTextStyleConfig config)
        {
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Vector3 startScale = Vector3.one;

            Color initialColor = _textMesh.color;

            while (elapsed < config.Lifetime)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / config.Lifetime);

                // Movement (Float Up)
                transform.position = startPos + Vector3.up * (config.FloatSpeed * elapsed);

                // Scale Curve (Pop effect)
                float scaleMultiplier = config.ScaleCurve.Evaluate(normalizedTime);
                transform.localScale = startScale * scaleMultiplier;

                // Alpha Curve (Fade out)
                float alpha = config.AlphaCurve.Evaluate(normalizedTime);
                Color currentColor = initialColor;
                currentColor.a = alpha;
                _textMesh.color = currentColor;

                yield return null;
            }

            gameObject.SetActive(false);
            _onDespawn?.Invoke(this);
        }
    }
}
