using System;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace ProjectZombie.Core.Architecture
{
    /// <summary>Owns one content scene; the Bootstrap scene is never owned or unloaded here.</summary>
    public interface IContentSceneLoader
    {
        Scene Current { get; }
        Task<Scene> LoadAsync(AssetReference reference);
        Task UnloadAsync();
    }

    /// <summary>Pairs every scene load with an unload and retains the exact operation handle.</summary>
    public sealed class AddressableSceneLoader : IContentSceneLoader
    {
        private AsyncOperationHandle<SceneInstance> _handle;
        private bool _ownsHandle;
        public Scene Current { get; private set; }

        public AddressableSceneLoader(Scene existingContent)
        {
            Current = existingContent;
        }

        public async Task<Scene> LoadAsync(AssetReference reference)
        {
            if (Current.IsValid() && Current.isLoaded) throw new InvalidOperationException("Unload content before loading another scene.");
            if (reference == null || !reference.RuntimeKeyIsValid()) throw new ArgumentException("Missing scene reference.");
            var handle = Addressables.LoadSceneAsync(reference, LoadSceneMode.Additive, true);
            try
            {
                var instance = await handle.Task;
                if (handle.Status != AsyncOperationStatus.Succeeded) throw new InvalidOperationException("Scene load failed.");
                _handle = handle;
                _ownsHandle = true;
                Current = instance.Scene;
                SceneManager.SetActiveScene(Current);
                return Current;
            }
            catch
            {
                if (handle.IsValid()) Addressables.Release(handle);
                throw;
            }
        }

        public async Task UnloadAsync()
        {
            if (_ownsHandle)
            {
                await Addressables.UnloadSceneAsync(_handle).Task;
                _ownsHandle = false;
            }
            else if (Current.IsValid() && Current.isLoaded)
            {
                var operation = SceneManager.UnloadSceneAsync(Current);
                if (operation != null) while (!operation.isDone) await Task.Yield();
            }
            Current = default;
        }
    }
}
