using ProjectZombie.Features.Enemies;
using UnityEngine;

namespace ProjectZombie.Features.Elements
{
    /// <summary>Runtime composition root for the configured Hỏa Hoạn spread behavior.</summary>
    public sealed class FireSpreadManager : MonoBehaviour
    {
        private static FireSpreadManager _instance;
        private FireSpreadService _service;

        public static void SpreadFrom(Enemy source, float burnDamage)
        {
            if (source == null)
                return;

            FireSpreadManager manager = Instance;
            manager?._service?.SpreadFrom(source, burnDamage);
        }

        private static FireSpreadManager Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = FindObjectOfType<FireSpreadManager>();
                if (_instance == null)
                    _instance = new GameObject("[FireSpreadManager]").AddComponent<FireSpreadManager>();
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            FireSpreadSettings settings = FireSpreadSettings.Runtime;
            if (settings == null)
            {
                Debug.LogError("Missing Resources/FireSpreadSettings asset; Hỏa Hoạn spread is disabled.", this);
                enabled = false;
                return;
            }

            _service = new FireSpreadService(settings);
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
