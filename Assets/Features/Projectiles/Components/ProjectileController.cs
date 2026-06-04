using UnityEngine;
using ProjectZombie.Features.Projectiles.Data;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Projectiles.Core;
using ProjectZombie.Features.Projectiles.Behaviors;

namespace ProjectZombie.Features.Projectiles.Components
{
    [RequireComponent(typeof(ProjectileMovement))]
    [RequireComponent(typeof(ProjectileCollision))]
    [RequireComponent(typeof(ProjectileLifetime))]
    public class ProjectileController : MonoBehaviour
    {
        public ProjectileData Data { get; private set; }
        public GameObject Owner { get; private set; }
        public DamageContext Damage { get; private set; }
        public ProjectileRuntimeState State { get; private set; }
        public Vector2 CurrentDirection { get; set; }

        private ProjectilePool _pool;
        private IProjectileBehavior[] _behaviors;

        private ProjectileMovement _movement;
        private ProjectileCollision _collision;
        private ProjectileLifetime _lifetime;

        private void Awake()
        {
            _movement = GetComponent<ProjectileMovement>();
            _collision = GetComponent<ProjectileCollision>();
            _lifetime = GetComponent<ProjectileLifetime>();
        }

        public void Initialize(ProjectileData data, Vector2 direction, GameObject owner, DamageContext damage, ProjectilePool pool, int generation = 0)
        {
            Data = data;
            CurrentDirection = direction.normalized;
            Owner = owner;
            Damage = damage;
            _pool = pool;

            State = new ProjectileRuntimeState(generation);
            State.SpawnPosition = transform.position;

            // Resolve and Sort behaviors based on data
            ResolveBehaviors();

            _movement.Initialize(this);
            _collision.Initialize(this);
            _lifetime.Initialize(this);

            foreach (var behavior in _behaviors)
            {
                behavior.OnSpawn();
            }

            ProjectileSystem.Instance.EventDispatcher.RaiseSpawned(this);
        }

        private void ResolveBehaviors()
        {
            var behaviorsList = new System.Collections.Generic.List<IProjectileBehavior>();

            if (Data.Behaviors != null && Data.Behaviors.Count > 0)
            {
                // Sort behavior data by ExecutionOrder
                var sortedData = new System.Collections.Generic.List<ProjectileBehaviorData>(Data.Behaviors);
                sortedData.Sort((a, b) => {
                    if (a == null || b == null) return 0;
                    return a.ExecutionOrder.CompareTo(b.ExecutionOrder);
                });

                foreach (var behaviorData in sortedData)
                {
                    if (behaviorData != null)
                    {
                        behaviorsList.Add(behaviorData.CreateBehavior(this));
                    }
                }
            }
            else
            {
                behaviorsList.Add(new StraightBehavior(this));
            }
            
            _behaviors = behaviorsList.ToArray();
        }

        private void Update()
        {
            if (State != null)
            {
                State.DistanceTraveled = Vector2.Distance(State.SpawnPosition, transform.position);
            }

            foreach (var behavior in _behaviors)
            {
                behavior.OnUpdate();
            }
        }

        public void HandleHit(ProjectileEventContext context)
        {
            State.HitCount++;
            ProjectileSystem.Instance.EventDispatcher.RaiseHit(context);

            bool shouldDespawn = true;
            foreach (var behavior in _behaviors)
            {
                if (!behavior.OnHit(context))
                {
                    shouldDespawn = false;
                }
            }

            if (shouldDespawn)
            {
                Despawn();
            }
        }

        public void HandleExpiration()
        {
            ProjectileSystem.Instance.EventDispatcher.RaiseExpired(this);
            Despawn();
        }

        private void Despawn()
        {
            foreach (var behavior in _behaviors)
            {
                behavior.OnDespawn();
            }
            
            ProjectileSystem.Instance.EventDispatcher.RaiseDespawned(this);
            
            if (_pool != null)
            {
                _pool.Return(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
