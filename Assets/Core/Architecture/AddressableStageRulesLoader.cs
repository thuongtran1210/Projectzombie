using System;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Maps;

namespace ProjectZombie.Core.Architecture
{
    /// <summary>Rules assets belong to the active match, not the application catalog.</summary>
    public interface IStageRulesLoader
    {
        Task<StageSpawnConfig> LoadAsync(StageDefinitionSO stage);
        void Release();
    }

    public sealed class AddressableStageRulesLoader : IStageRulesLoader
    {
        private AsyncOperationHandle<StageSpawnConfig> _handle;
        private bool _owned;
        public async Task<StageSpawnConfig> LoadAsync(StageDefinitionSO stage)
        {
            if (_owned) throw new InvalidOperationException("Release the previous match rules first.");
            if (stage.DefenseBlueprint != null) return null;
            _handle = Addressables.LoadAssetAsync<StageSpawnConfig>(stage.SpawnRules);
            _owned = true;
            try
            {
                var rules = await _handle.Task;
                if (_handle.Status != AsyncOperationStatus.Succeeded || rules == null)
                    throw new InvalidOperationException("Unable to load stage rules.");
                rules.Compile();
                return rules;
            }
            catch { Release(); throw; }
        }
        public void Release()
        {
            if (_owned && _handle.IsValid()) Addressables.Release(_handle);
            _owned = false;
        }
    }
}
