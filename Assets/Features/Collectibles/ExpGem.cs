using UnityEngine;
using ProjectZombie.Features.Player;
using DG.Tweening; // Thêm thư viện DOTween

namespace ProjectZombie.Features.Collectibles
{
    /// <summary>
    /// An experience gem dropped by enemies. It flies towards the player when they are in range.
    /// </summary>
    public class ExpGem : MonoBehaviour
    {
        [SerializeField] private float expAmount = 10f;
        [SerializeField] private float flySpeed = 10f;

        private Transform _targetPlayer;
        private bool _isTriggered = false;
        private bool _isHoming = false; // Trạng thái đang bay vào người chơi

        // Hiệu ứng "nảy" khi vừa sinh ra (tùy chọn)
        private void Start()
        {
            // Cho viên exp nhỏ từ 0 phình lên lúc mới rớt xuống
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        private void Update()
        {
            // Chỉ bay vào người chơi khi đã hoàn thành hiệu ứng văng ra (Homing)
            if (_isHoming && _targetPlayer != null)
            {
                // Fly towards the player
                transform.position = Vector3.MoveTowards(transform.position, _targetPlayer.position, flySpeed * Time.deltaTime);

                // Check distance for collection. Using Vector2 to ignore Z-axis differences in 2D.
                if (Vector2.Distance(transform.position, _targetPlayer.position) < 0.5f)
                {
                    Collect();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!_isTriggered && collision.CompareTag("Player"))
            {
                StartMagnetEffect(collision.transform);
            }
        }
        
        private void OnTriggerEnter(Collider collision)
        {
            if (!_isTriggered && collision.CompareTag("Player"))
            {
                StartMagnetEffect(collision.transform);
            }
        }

        private void StartMagnetEffect(Transform player)
        {
            _isTriggered = true;
            _targetPlayer = player;

            // --- HIỆU ỨNG HÚT CỦA DOTWEEN ---
            
            // 1. Tính hướng văng ra ngược với hướng người chơi (tạo đà)
            Vector3 dirAwayFromPlayer = (transform.position - _targetPlayer.position).normalized;
            // Nếu trùng vị trí, random hướng
            if (dirAwayFromPlayer == Vector3.zero) dirAwayFromPlayer = Random.insideUnitCircle.normalized;
            
            Vector3 jumpPos = transform.position + dirAwayFromPlayer * 1.5f;

            // 2. Di chuyển viên kinh nghiệm văng ra một chút trong 0.25 giây
            transform.DOMove(jumpPos, 0.25f).SetEase(Ease.OutQuad).OnComplete(() => 
            {
                // 3. Sau khi văng ra xong, bắt đầu lao vào người chơi
                _isHoming = true;
                
                // Reset tốc độ bay về 0 rồi cho tăng tốc dần (Gia tốc) bằng DOTween cho mượt
                flySpeed = 0f;
                DOTween.To(() => flySpeed, x => flySpeed = x, 35f, 0.5f).SetEase(Ease.InQuad);
            });
        }

        private void Collect()
        {
            if (_targetPlayer != null)
            {
                var playerExp = _targetPlayer.GetComponent<PlayerExperience>();
                if (playerExp != null)
                {
                    playerExp.AddExp(expAmount);
                }
            }
            
            // Tắt Tween đang chạy trên object này (nếu có) để tránh lỗi bộ nhớ
            transform.DOKill();
            
            // For now, just destroy the object. In a real game, use Object Pooling.
            Destroy(gameObject);
        }
        
        // This can be used to set exp amount based on enemy type
        public void SetExpAmount(float amount)
        {
            expAmount = amount;
        }
    }
}

