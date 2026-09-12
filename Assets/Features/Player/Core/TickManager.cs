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

        public static TickManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TickManager>();
                    if (_instance == null && Application.isPlaying)
                    {
                        GameObject go = new GameObject("[TickManager]");
                        _instance = go.AddComponent<TickManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
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
