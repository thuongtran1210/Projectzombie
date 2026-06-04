using System.Collections.Generic;
using TikTokBridge.Core;
using TikTokBridge.Logic;
using TikTokBridge.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectZombie.Features.DebugUI
{
    public class DebugEventOverlay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI logTextTemplate; 
        [SerializeField] private Transform logContainer;

        [Header("Settings")]
        [SerializeField] private float logDisplayTime = 3f;

        // Lưu trữ event hiện tại để Hủy đăng ký khi cần thiết
        private ICommandDispatcher _dispatcher;

        public void Construct(ICommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            // Subscribe events
            _dispatcher.OnLikeReceived += HandleLike;
            _dispatcher.OnFollowReceived += HandleFollow;
            _dispatcher.OnGiftReceived += HandleGift;
            _dispatcher.OnSpawnEnemy += HandleSpawnEnemy;
            _dispatcher.OnSpawnBoss += HandleSpawnBoss;
        }

        private void OnDestroy()
        {
            // Quan trọng: Phải unsubscribe khi object bị hủy để tránh memory leak
            if (_dispatcher != null)
            {
                _dispatcher.OnLikeReceived -= HandleLike;
                _dispatcher.OnFollowReceived -= HandleFollow;
                _dispatcher.OnGiftReceived -= HandleGift;
                _dispatcher.OnSpawnEnemy -= HandleSpawnEnemy;
                _dispatcher.OnSpawnBoss -= HandleSpawnBoss;
            }
        }

        private void HandleLike(GameCommandPayload payload)
        {
            PrintLog($"<color=blue>[LIKE_RECEIVED]</color> {payload.user} liked the stream!");
        }

        private void HandleFollow(GameCommandPayload payload)
        {
            PrintLog($"<color=yellow>[FOLLOW_RECEIVED]</color> {payload.user} started following!");
        }

        private void HandleGift(GameCommandPayload payload)
        {
            PrintLog($"<color=red>[GIFT_RECEIVED]</color> {payload.user} sent a gift!");
        }

        private void HandleSpawnEnemy(GameCommandPayload payload)
        {
            PrintLog($"<color=orange>[SPAWN_ENEMY]</color> Spawning {payload.amount}x {payload.enemy} by {payload.user}!");
        }

        private void HandleSpawnBoss(GameCommandPayload payload)
        {
            PrintLog($"<color=purple>[SPAWN_BOSS]</color> Spawning BOSS {payload.enemy} by {payload.user}!");
        }

        private void PrintLog(string message)
        {
            if (logTextTemplate == null || logContainer == null) return;

            TextMeshProUGUI newLog = Instantiate(logTextTemplate, logContainer);
            newLog.text = message;
            newLog.gameObject.SetActive(true);

            // Tự hủy log sau vài giây
            Destroy(newLog.gameObject, logDisplayTime);
        }
    }
}
