using UnityEngine;
using TikTokBridge.Core;
using TikTokBridge.Models;

namespace ProjectZombie.Features.Player
{
    [RequireComponent(typeof(PlayerStats))]
    public class StreamerPassive : MonoBehaviour
    {
        private PlayerStats _playerStats;
        private ICommandDispatcher _dispatcher;

        private int _totalLikes = 0;
        private int _currentMultiplierStep = 0;
        private const int MAX_MULTIPLIER_STEPS = 30; // Max 30%

        private void Awake()
        {
            _playerStats = GetComponent<PlayerStats>();
        }

        // Dependency Injection từ Bootstrapper
        public void Construct(ICommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _dispatcher.OnLikeReceived += HandleLikeReceived;
        }

        private void HandleLikeReceived(GameCommandPayload cmd)
        {
            int likes = 1;
            
            // TikTok gửi nhiều like cùng lúc trong trường likeCount (nếu cấu hình)
            if (cmd.additionalData != null && cmd.additionalData["likeCount"] != null)
            {
                likes = cmd.additionalData["likeCount"].ToObject<int>();
            }

            _totalLikes += likes;
            
            // Tính số bước multiplier hiện tại (mỗi 100 like = 1 bước)
            int calculatedSteps = _totalLikes / 100;

            if (calculatedSteps > _currentMultiplierStep)
            {
                int stepsToAdd = calculatedSteps - _currentMultiplierStep;
                
                // Chỉ tăng tối đa 30% (30 bước)
                for (int i = 0; i < stepsToAdd; i++)
                {
                    if (_currentMultiplierStep < MAX_MULTIPLIER_STEPS)
                    {
                        _currentMultiplierStep++;
                        _playerStats.AddDamageMultiplier(0.01f); // +1% Damage
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (_dispatcher != null)
            {
                _dispatcher.OnLikeReceived -= HandleLikeReceived;
            }
        }
    }
}
