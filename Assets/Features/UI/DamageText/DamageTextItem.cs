using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace ProjectZombie.Features.UI.DamageText
{
    /// <summary>Hiển thị và chạy hoạt ảnh chữ; báo hoàn thành để chủ sở hữu thu hồi về pool.</summary>
    [RequireComponent(typeof(TextMeshPro))]
    public class DamageTextItem : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _textMesh;
        private void Awake() { if (_textMesh == null) _textMesh = GetComponent<TextMeshPro>(); }
        public void StopPresentation()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
            transform.localScale = Vector3.one;
        }
        public void Setup(string text, Color color, float fontSize, Vector3 position, DamageTextStyleConfig config,
            long token, Action<DamageTextItem, long> completed, Camera camera, bool faceCamera,
            string sortingLayer, int sortingOrder, Func<bool> canAnimate)
        {
            StopAllCoroutines();
            if (_textMesh == null) _textMesh = GetComponent<TextMeshPro>();
            if (_textMesh == null) throw new InvalidOperationException("Combat text requires TextMeshPro.");
            _textMesh.SetText(text); _textMesh.color = color; _textMesh.fontSize = fontSize;
            _textMesh.alignment = TextAlignmentOptions.Center;
            _textMesh.sortingLayerID = SortingLayer.NameToID(sortingLayer);
            _textMesh.sortingOrder = sortingOrder;
            var right = faceCamera ? camera.transform.right : Vector3.right;
            var up = faceCamera ? camera.transform.up : Vector3.up;
            transform.position = position + right * UnityEngine.Random.Range(-config.RandomArcSpread, config.RandomArcSpread)
                + up * UnityEngine.Random.Range(0f, config.RandomArcSpread * .5f);
            transform.rotation = faceCamera ? camera.transform.rotation : Quaternion.identity;
            transform.localScale = Vector3.one;
            gameObject.SetActive(true);
            StartCoroutine(Animate(config, token, completed, camera, faceCamera, canAnimate, color));
        }
        private IEnumerator Animate(DamageTextStyleConfig config, long token, Action<DamageTextItem, long> completed,
            Camera camera, bool faceCamera, Func<bool> canAnimate, Color initialColor)
        {
            float elapsed = 0f;
            float lifetime = Mathf.Max(.01f, config.Lifetime);
            Vector3 origin = transform.position;
            while (elapsed < lifetime)
            {
                // Tạm dừng hoạt ảnh theo trạng thái trận đấu; coroutine chỉ tồn tại khi item đang hiển thị.
                if (canAnimate != null && !canAnimate()) { yield return null; continue; }
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / lifetime);
                var up = faceCamera && camera != null ? camera.transform.up : Vector3.up;
                if (faceCamera && camera != null) transform.rotation = camera.transform.rotation;
                transform.position = origin + up * (config.FloatSpeed * elapsed);
                transform.localScale = Vector3.one * (config.ScaleCurve != null ? config.ScaleCurve.Evaluate(t) : 1f);
                var color = initialColor;
                color.a *= config.AlphaCurve != null ? config.AlphaCurve.Evaluate(t) : 1f - t;
                _textMesh.color = color;
                yield return null;
            }
            completed(this, token);
        }
    }
}
