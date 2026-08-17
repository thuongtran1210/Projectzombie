using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.YinYang
{
    public struct ElementHitEntry
    {
        public ElementType element;
        public float timestamp;
        public WeaponBase weapon;
        public bool isVirtualHit;

        public ElementHitEntry(ElementType element, float timestamp, WeaponBase weapon, bool isVirtualHit = false)
        {
            this.element = element;
            this.timestamp = timestamp;
            this.weapon = weapon;
            this.isVirtualHit = isVirtualHit;
        }
    }

    /// <summary>
    /// Quản lý Vòng Tương Sinh (v4.2 Revised & v4.0 Signature Skill Integration).
    /// Buffer queue recentElementHits (tối đa 3s) và kích hoạt giảm 20% Cooldown cho vũ khí hit 2 khi khớp tương sinh.
    /// Vòng tương sinh: Kim -> Thủy -> Mộc -> Hỏa -> Thổ -> Kim.
    /// </summary>
    public class ElementCycleManager : MonoBehaviour
    {
        public static ElementCycleManager Instance { get; private set; }

        [Header("Cycle Settings")]
        [Tooltip("Cửa sổ thời gian tối đa để lưu hit (giây).")]
        [SerializeField] private float _windowTimeSeconds = 3.0f;

        [Tooltip("Cooldown giữa các lần proc Tương Sinh (giây) - tối đa 1 proc / 3s cho MVP.")]
        [SerializeField] private float _procCooldownSeconds = 3.0f;

        [Header("Audio Feedback")]
        [SerializeField] private AudioClip _procSFX;

        private readonly Queue<ElementHitEntry> _recentElementHits = new Queue<ElementHitEntry>();
        private float _lastProcTimestamp = -999f;

        /// <summary>
        /// Sự kiện phát ra khi kích hoạt thành công Vòng Tương Sinh (Element 1 -> Element 2 -> Weapon được giảm Cooldown).
        /// </summary>
        public event System.Action<ElementType, ElementType, WeaponBase> OnElementSynergyTriggered;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        /// <summary>
        /// Đẩy 1 phần tử ảo (Virtual Element Hit) từ Signature Skill Phán Quyết Tiền Định (Thư Sinh) vào Queue.
        /// </summary>
        public void PushVirtualElementHit(ElementType element)
        {
            if (element == ElementType.None) return;

            var virtualHit = new ElementHitEntry(element, Time.time, null, isVirtualHit: true);

            if (_recentElementHits.Count >= 3)
            {
                _recentElementHits.Dequeue();
            }
            _recentElementHits.Enqueue(virtualHit);
        }

        /// <summary>
        /// Ghi nhận 1 đòn đánh sát thương thuộc tính Ngũ Hành từ vũ khí.
        /// </summary>
        public void RegisterHit(ElementType hitElement, WeaponBase weapon)
        {
            if (hitElement == ElementType.None) return;

            float now = Time.time;

            // Clean up queue: loại bỏ các hit quá 3s
            while (_recentElementHits.Count > 0 && (now - _recentElementHits.Peek().timestamp) > _windowTimeSeconds)
            {
                _recentElementHits.Dequeue();
            }

            var newHit = new ElementHitEntry(hitElement, now, weapon);

            // Kiểm tra proc với hit ảo hoặc hit thật gần nhất
            if (_recentElementHits.Count > 0)
            {
                ElementHitEntry lastHit = GetLatestHit();

                // Nếu hit gần nhất là Hit Ảo từ Signature Skill (Thư Sinh):
                if (lastHit.isVirtualHit)
                {
                    if (IsGenerationPair(lastHit.element, newHit.element))
                    {
                        // Proc lập tức 20% CDR cho vũ khí hit thật mà không tốn cooldown 3s toàn cục
                        if (weapon != null)
                        {
                            weapon.ReduceCurrentCooldown(0.20f);
                        }
                        OnElementSynergyTriggered?.Invoke(lastHit.element, newHit.element, weapon);
                        // Xóa hit ảo đã proc khỏi queue
                        _recentElementHits.Dequeue();
                    }
                }
                else if ((now - _lastProcTimestamp) >= _procCooldownSeconds)
                {
                    if (IsGenerationPair(lastHit.element, newHit.element))
                    {
                        ProcElementGeneration(lastHit, newHit);
                    }
                }
            }

            // Thêm vào queue (giữ tối đa 3 phần tử)
            if (_recentElementHits.Count >= 3)
            {
                _recentElementHits.Dequeue();
            }
            _recentElementHits.Enqueue(newHit);
        }

        private ElementHitEntry GetLatestHit()
        {
            ElementHitEntry latest = default;
            foreach (var h in _recentElementHits)
            {
                latest = h;
            }
            return latest;
        }

        /// <summary>
        /// Kiểm tra cặp Tương Sinh: Kim sinh Thủy -> Thủy sinh Mộc -> Mộc sinh Hỏa -> Hỏa sinh Thổ -> Thổ sinh Kim.
        /// </summary>
        public bool IsGenerationPair(ElementType e1, ElementType e2)
        {
            return (e1 == ElementType.Kim && e2 == ElementType.Thuy) ||
                   (e1 == ElementType.Thuy && e2 == ElementType.Moc) ||
                   (e1 == ElementType.Moc && e2 == ElementType.Hoa) ||
                   (e1 == ElementType.Hoa && e2 == ElementType.Tho) ||
                   (e1 == ElementType.Tho && e2 == ElementType.Kim);
        }

        private void ProcElementGeneration(ElementHitEntry hit1, ElementHitEntry hit2)
        {
            _lastProcTimestamp = Time.time;

            // 1. Giảm 20% Cooldown của vũ khí vừa phát ra hit thứ 2
            if (hit2.weapon != null)
            {
                hit2.weapon.ReduceCurrentCooldown(0.20f);
            }

            // 2. SFX Ting Feedback
            if (_procSFX != null)
            {
                AudioSource.PlayClipAtPoint(_procSFX, transform.position);
            }

            // 3. Event Notification cho UI HUD / VFX
            OnElementSynergyTriggered?.Invoke(hit1.element, hit2.element, hit2.weapon);

            // 4. Log & Visual UI feedback
            Debug.Log($"<color=#00FFCC>[VÒNG TƯƠNG SINH PROC]</color> {hit1.element} sinh {hit2.element}! Vũ khí '{hit2.weapon?.weaponId}' được giảm 20% Cooldown!");
        }
    }
}
