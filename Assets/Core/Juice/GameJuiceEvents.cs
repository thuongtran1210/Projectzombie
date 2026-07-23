using System;

namespace ProjectZombie.Core.Juice
{
    /// <summary>
    /// Event Hub tập trung cho các hiệu ứng Game Feel (Rung màn hình, khựng hình Hit Stop).
    /// Giúp Decouple hoàn toàn giữa hệ thống Vũ khí và Camera / Time Manager.
    /// </summary>
    public static class GameJuiceEvents
    {
        public static event Action<float, float> OnCameraShakeRequested;
        public static event Action<float> OnHitStopRequested;

        public static void RequestCameraShake(float duration, float magnitude)
        {
            OnCameraShakeRequested?.Invoke(duration, magnitude);
        }

        public static void RequestHitStop(float duration)
        {
            OnHitStopRequested?.Invoke(duration);
        }
    }
}
