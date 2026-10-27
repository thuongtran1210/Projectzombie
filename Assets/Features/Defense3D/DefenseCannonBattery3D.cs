using System;
using UnityEngine;
using UnityEngine.AI;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Authored three-cannon battery and West landing validator. Projectiles are fixed authored views.</summary>
    public sealed class DefenseCannonBattery3D : MonoBehaviour
    {
        [SerializeField] private DefenseGhostCannon3D[] _cannons;
        [SerializeField] private DefenseEnemyDataSO _landingEnemy;
        [SerializeField] private LayerMask _groundMask = 4096;
        [SerializeField] private float _westBankMaximumX = 4.45f;
        [SerializeField] private float _sampleRadius = 0.3f;
        [SerializeField] private LayerMask _landingBlockers = 4105;
        [SerializeField] private float _landingRadius = 0.45f;
        [SerializeField] private float _landingLowerCenter = 0.5f;
        [SerializeField] private float _landingUpperCenter = 1.5f;
        private NavMeshPath _path;
        private DefenseEnemySpawnDefinition _snapshot;
        public DefenseEnemySpawnDefinition LandingEnemy => _snapshot;
        private void Awake() => _path = new NavMeshPath();
        public void ValidateBindings()
        {
            if (!DefenseDamageRules.IsFinite(_westBankMaximumX) || !DefenseDamageRules.IsFinite(_sampleRadius) || _sampleRadius <= 0f ||
                !DefenseDamageRules.IsFinite(_landingRadius) || _landingRadius <= 0f ||
                !DefenseDamageRules.IsFinite(_landingLowerCenter) || _landingLowerCenter <= _landingRadius ||
                !DefenseDamageRules.IsFinite(_landingUpperCenter) || _landingUpperCenter < _landingLowerCenter ||
                _groundMask.value == 0 || _landingBlockers.value == 0)
                throw new InvalidOperationException("Invalid landing geometry settings.");
            if (_cannons == null || _cannons.Length != 3 || _landingEnemy == null ||
                _landingEnemy.LocomotionType != DefenseEnemyLocomotionType.ParatrooperLanding)
                throw new InvalidOperationException("Assign three Ghost Cannons and the landing enemy definition.");
            for (int i = 0; i < _cannons.Length; i++)
            {
                if (_cannons[i] == null || _cannons[i].gameObject.scene != gameObject.scene)
                    throw new InvalidOperationException("Cannon must belong to the battery scene.");
                _cannons[i].ValidateBindings();
                for (int j = 0; j < i; j++)
                    if (_cannons[j] == _cannons[i] || _cannons[j].CannonId == _cannons[i].CannonId)
                        throw new InvalidOperationException("Cannon instances and IDs must be distinct.");
            }
        }
        public void Bind(IDefenseGameplayContext gameplay, DefenseSpawnRuntime spawns)
        {
            ValidateBindings();
            _snapshot = new DefenseEnemySpawnDefinition(_landingEnemy);
            foreach (var cannon in _cannons) cannon.Bind(gameplay, spawns);
        }
        public void Tick()
        {
            foreach (var cannon in _cannons) cannon.Tick();
        }
        public bool TryResolveLanding(Vector3 requested, Vector3 target, out Vector3 position)
        {
            position = default;
            if (_path == null || requested.x > _westBankMaximumX || !new DefensePoint3(requested.x, requested.y, requested.z).IsFinite ||
                !Physics.Raycast(requested + Vector3.up * 2f, Vector3.down, out var ground, 4f, _groundMask, QueryTriggerInteraction.Ignore) ||
                !NavMesh.SamplePosition(ground.point, out var hit, _sampleRadius, NavMesh.AllAreas) || hit.position.x > _westBankMaximumX ||
                !NavMesh.SamplePosition(target, out var destination, 2f, NavMesh.AllAreas) ||
                !NavMesh.CalculatePath(hit.position, destination.position, NavMesh.AllAreas, _path) || _path.status != NavMeshPathStatus.PathComplete)
                return false;
            if (Physics.CheckCapsule(hit.position + Vector3.up * _landingLowerCenter, hit.position + Vector3.up * _landingUpperCenter,
                _landingRadius, _landingBlockers, QueryTriggerInteraction.Ignore)) return false;
            position = hit.position;
            return true;
        }
    }
}
