using UnityEngine;
using System;

namespace ProjectZombie.Core
{
    public class TickManager : MonoBehaviour
    {
        public static event Action OnTick;
        
        private static TickManager _instance;
        private float tickInterval = 0.5f;
        private float _nextTickTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("[TickManager]");
                _instance = go.AddComponent<TickManager>();
                DontDestroyOnLoad(go);
            }
        }

        private void Update()
        {
            if (Time.time >= _nextTickTime)
            {
                _nextTickTime = Time.time + tickInterval;
                OnTick?.Invoke();
            }
        }
    }
}
