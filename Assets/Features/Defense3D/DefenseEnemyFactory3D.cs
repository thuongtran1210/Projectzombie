using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Legacy-compatible composition helpers; production ownership is supplied by the session factory.</summary>
    public static class DefenseEnemyFactory3D
    {
        public const string DefinitionsFolder = "Defense3D/Enemies/";

        /// <summary>Production path: register ownership while inactive, then enable and configure navigation.</summary>
        internal static DefenseEnemy3D SpawnOwnedSnapshot(DefenseEnemySpawnDefinition definition, Vector3 position, int lane,
            DefenseBasePrototype3D target, IDefenseGameplayContext gameplay, DefenseWoodWallet3D wallet,
            DefenseTargetIndex targets, DefenseRuntimeRoots3D roots, System.Action<DefenseEnemy3D> prepared, bool arenaBoss, bool landed = false)
        {
            if (!arenaBoss && target == null) throw new System.ArgumentNullException(nameof(target));
            if (roots == null || prepared == null) throw new System.ArgumentNullException(nameof(roots));
            var actor = CreateActor(definition,position,gameplay,wallet,targets,roots,prepared,landed);
            try
            {
                if (!arenaBoss)
                {
                    actor.GetComponent<DefenseEnemyBridgeWalker3D>().Configure(lane,target);
                    if (actor.GetComponent<DefenseGroundMovement3D>().HasFailed)
                        throw new System.InvalidOperationException("Enemy route initialization failed: " + definition.EnemyId);
                }
                return actor;
            }
            catch
            {
                ReleaseFailedActor(actor.gameObject);
                throw;
            }
        }

        public static DefenseEnemy3D Spawn(string enemyId, Vector3 position, int lane, DefenseBasePrototype3D target,
            IDefenseGameplayContext gameplay = null, DefenseWoodWallet3D collectorWallet = null, DefenseTargetIndex targets = null)
        {
            var definition = Resources.Load<DefenseEnemyDataSO>(DefinitionsFolder + enemyId);
            if (definition == null)
                throw new System.InvalidOperationException("Missing enemy definition: " + enemyId);
            return Spawn(definition, position, lane, target, gameplay, collectorWallet, targets);
        }

        public static DefenseEnemy3D Spawn(DefenseEnemyDataSO definition, Vector3 position, int lane, DefenseBasePrototype3D target,
            IDefenseGameplayContext gameplay = null, DefenseWoodWallet3D collectorWallet = null, DefenseTargetIndex targets = null)
        {
            return SpawnSnapshot(new DefenseEnemySpawnDefinition(definition), position, lane, target, gameplay, collectorWallet, targets);
        }

        public static DefenseEnemy3D SpawnSnapshot(DefenseEnemySpawnDefinition definition, Vector3 position, int lane,
            DefenseBasePrototype3D target, IDefenseGameplayContext gameplay, DefenseWoodWallet3D collectorWallet, DefenseTargetIndex targets)
        {
            if (target == null) throw new System.ArgumentNullException(nameof(target));
            var actor = CreateActor(definition, position, gameplay, collectorWallet, targets);
            try
            {
                actor.GetComponent<DefenseEnemyBridgeWalker3D>().Configure(lane, target);
                if (actor.GetComponent<DefenseGroundMovement3D>().HasFailed)
                    throw new System.InvalidOperationException("Enemy route initialization failed: " + definition.EnemyId);
                return actor;
            }
            catch
            {
                ReleaseFailedActor(actor.gameObject);
                throw;
            }
        }

        /// <summary>Creates an authored Boss composition without starting a ground route.</summary>
        public static DefenseEnemy3D SpawnArenaBoss(DefenseEnemyDataSO definition, Vector3 position,
            IDefenseGameplayContext gameplay, DefenseWoodWallet3D collectorWallet, DefenseTargetIndex targets = null)
        {
            return SpawnArenaBossSnapshot(new DefenseEnemySpawnDefinition(definition), position, gameplay, collectorWallet, targets);
        }

        public static DefenseEnemy3D SpawnArenaBossSnapshot(DefenseEnemySpawnDefinition definition, Vector3 position,
            IDefenseGameplayContext gameplay, DefenseWoodWallet3D collectorWallet, DefenseTargetIndex targets)
            => CreateActor(definition, position, gameplay, collectorWallet, targets);

        private static DefenseEnemy3D CreateActor(DefenseEnemySpawnDefinition definition, Vector3 position,
            IDefenseGameplayContext gameplay, DefenseWoodWallet3D collectorWallet, DefenseTargetIndex targets,
            DefenseRuntimeRoots3D roots = null, System.Action<DefenseEnemy3D> prepared = null, bool landed = false)
        {
            if (definition == null) throw new System.ArgumentNullException(nameof(definition));
            if (gameplay == null) throw new System.ArgumentNullException(nameof(gameplay), "Spawn requires an explicit Defense session context.");
            if (targets == null) throw new System.ArgumentNullException(nameof(targets));
            if (!ReferenceEquals(gameplay.Targets, targets)) throw new System.ArgumentException("Target index must belong to the injected gameplay session.");
            if (definition.Stats.Locomotion == DefenseEnemyLocomotionType.ParatrooperLanding && !landed)
                throw new System.InvalidOperationException("Landing enemy can only be acquired after an approved landing.");
            var instance = DefenseEnemyPrefab3D.Instantiate(definition.Prefab,roots != null ? roots.Staging : null);
            try
            {
                instance.transform.position = position;
                var adapter = instance.GetComponent<DefenseEnemyBridgeWalker3D>();
                if (adapter == null)
                    throw new System.InvalidOperationException("Enemy prefab requires its bridge composition adapter: " + definition.EnemyId);
                adapter.BindGameplay(gameplay, collectorWallet,roots != null ? roots.Pickups : null, roots != null ? roots.FriendlyTargets : null);
                adapter.Initialize(definition);
                if (definition.Stats.Locomotion == DefenseEnemyLocomotionType.AmphibianWaterToShore)
                {
                    var water = instance.GetComponent<DefenseWaterMovement3D>();
                    if (water == null || roots == null || roots.WaterNavigation == null)
                        throw new System.InvalidOperationException("Water spawn requires authored movement and scene navigation.");
                    water.Bind(gameplay, roots.WaterNavigation);
                }
                if (definition.Stats.Locomotion == DefenseEnemyLocomotionType.AirFlight)
                {
                    var air = instance.GetComponent<DefenseAirMovement3D>();
                    if (air == null || roots == null || roots.AirNavigation == null)
                        throw new System.InvalidOperationException("Air spawn requires authored flight movement and scene volume.");
                    air.Bind(gameplay, roots.AirNavigation, roots.FriendlyTargets);
                }
                var actor = instance.GetComponent<DefenseEnemy3D>();
                if (landed) instance.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;
                actor.BindTargets(targets);
                prepared?.Invoke(actor);
                if (roots != null)
                {
                    instance.transform.SetParent(roots.Actors,true);
                    instance.SetActive(true);
                }
                return actor;
            }
            catch
            {
                ReleaseFailedActor(instance);
                throw;
            }
        }

        private static void ReleaseFailedActor(GameObject instance)
        {
            var actor = instance.GetComponent<DefenseEnemy3D>();
            if (actor != null && actor.RequestRelease(DefenseActorReleaseReason.SpawnFailed)) return;
            instance.SetActive(false);
            Object.Destroy(instance);
        }
    }
}
