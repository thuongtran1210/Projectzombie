using UnityEngine;
using ProjectZombie.Features.Projectiles.Data;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Projectiles.Core;
using ProjectZombie.Features.Projectiles.Behaviors;
using System.Collections.Generic;

namespace ProjectZombie.Features.Projectiles.Components
{
    [RequireComponent(typeof(ProjectileMovement))]
    [RequireComponent(typeof(ProjectileCollision))]
    [RequireComponent(typeof(ProjectileLifetime))]
    [RequireComponent(typeof(ProjectileVFXListener))]
    public class ProjectileController : MonoBehaviour
    {
        public ProjectileData Data { get; private set; }
        public GameObject Owner { get; private set; }
        public DamageContext Damage { get; private set; }
        public ProjectileRuntimeState State;
        public Vector2 CurrentDirection { get; set; }

        private ProjectilePool _pool;
        private IProjectileBehavior[] _behaviors;
        private bool _isBehaviorsResolved;

        private ProjectileMovement _movement;
        private ProjectileCollision _collision;
        private ProjectileLifetime _lifetime;

        private void Awake()
        {
            _movement = GetComponent<ProjectileMovement>();
            _collision = GetComponent<ProjectileCollision>();
            _lifetime = GetComponent<ProjectileLifetime>();
        }

        private ProjectileData _cachedData;

        public void Initialize(ProjectileData data, Vector2 direction, GameObject owner, DamageContext damage, ProjectilePool pool, int generation = 0)
        {
            Data = data;
            CurrentDirection = direction.normalized;
            Owner = owner;
            Damage = damage;
            _pool = pool;

            State.Reset(generation, transform.position);

            // Resolve and cache behaviors if not resolved yet or if data changed
            if (!_isBehaviorsResolved || _cachedData != data)
            {
                _cachedData = data;
                ResolveBehaviors();
            }

            _movement.Initialize(this);
            _collision.Initialize(this);
            _lifetime.Initialize(this);

            if (_behaviors != null)
            {
                for (int i = 0; i < _behaviors.Length; i++)
                {
                    _behaviors[i].OnSpawn();
                }
            }

            ProjectileSystem.Instance.EventDispatcher.RaiseSpawned(this);
        }

        private void ResolveBehaviors()
        {
            var behaviorsList = new System.Collections.Generic.List<IProjectileBehavior>();

            if (Data != null && Data.Behaviors != null && Data.Behaviors.Count > 0)
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
            _isBehaviorsResolved = true;
        }

        public IReadOnlyList<IProjectileBehavior> Behaviors => _behaviors;

        public T GetBehavior<T>() where T : class, IProjectileBehavior
        {
            if (_behaviors == null) return null;
            for (int i = 0; i < _behaviors.Length; i++)
            {
                if (_behaviors[i] is T match) return match;
            }
            return null;
        }

        private void Update()
        {
            State.DistanceTraveled = Vector2.Distance(State.SpawnPosition, transform.position);

            if (_lifetime != null)
            {
                _lifetime.OnUpdate();
            }

            if (_behaviors != null)
            {
                for (int i = 0; i < _behaviors.Length; i++)
                {
                    _behaviors[i].OnUpdate();
                }
            }
        }

        public void HandleHit(ProjectileEventContext context)
        {
            State.HitCount++;
            ProjectileSystem.Instance.EventDispatcher.RaiseHit(context);

            bool requireDespawn = false;
            bool keepAlive = false;

            if (_behaviors != null)
            {
                for (int i = 0; i < _behaviors.Length; i++)
                {
                    BehaviorHitResult result = _behaviors[i].OnHit(context);
                    if (result == BehaviorHitResult.RequireDespawn)
                    {
                        requireDespawn = true;
                    }
                    else if (result == BehaviorHitResult.KeepAlive)
                    {
                        keepAlive = true;
                    }
                }
            }

            if (requireDespawn || !keepAlive)
            {
                Despawn();
            }
        }

        public void HandleExpiration()
        {
            ProjectileSystem.Instance.EventDispatcher.RaiseExpired(this);
            Despawn();
        }

        public void Despawn()
        {
            if (_behaviors != null)
            {
                for (int i = 0; i < _behaviors.Length; i++)
                {
                    _behaviors[i].OnDespawn();
                }
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
