using System.Collections.Generic;
using UnityEngine;

namespace Core.Audio
{
    /// <summary>
    /// Quản lý Object Pool cho AudioSource GameObjects để triệt tiêu 100% GC Alloc khi phát SFX dồn dập.
    /// </summary>
    public class AudioSourcePool
    {
        private readonly Transform _parentTransform;
        private readonly List<AudioSource> _pool = new List<AudioSource>();
        private readonly int _initialCapacity;

        public AudioSourcePool(Transform parent, int initialCapacity = 16)
        {
            _parentTransform = parent;
            _initialCapacity = initialCapacity;

            for (int i = 0; i < _initialCapacity; i++)
            {
                CreateNewAudioSource();
            }
        }

        private AudioSource CreateNewAudioSource()
        {
            var go = new GameObject($"Pooled_AudioSource_{_pool.Count}");
            go.transform.SetParent(_parentTransform);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            go.SetActive(false);
            _pool.Add(source);
            return source;
        }

        public AudioSource Get()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                if (!_pool[i].gameObject.activeSelf)
                {
                    _pool[i].gameObject.SetActive(true);
                    return _pool[i];
                }
            }

            // Nếu pool hết slot, tự mở rộng
            var newSource = CreateNewAudioSource();
            newSource.gameObject.SetActive(true);
            return newSource;
        }

        public void Release(AudioSource source)
        {
            if (source == null) return;
            source.stop();
            source.clip = null;
            source.gameObject.SetActive(false);
        }

        public void ReleaseAll()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                if (_pool[i] != null && _pool[i].gameObject.activeSelf)
                {
                    _pool[i].Stop();
                    _pool[i].clip = null;
                    _pool[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
