using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Core.Events
{
    /// <summary>
    /// Sự kiện phát ra khi kẻ địch bị tiêu diệt.
    /// </summary>
    public struct EnemyDiedEvent
    {
        public GameObject EnemyGameObject;
        public Vector3 Position;
        public int ExpReward;
        public ElementType Element;

        public EnemyDiedEvent(GameObject enemyGameObject, Vector3 position, int expReward, ElementType element)
        {
            EnemyGameObject = enemyGameObject;
            Position = position;
            ExpReward = expReward;
            Element = element;
        }
    }

    /// <summary>
    /// Sự kiện phát ra khi Player lên cấp.
    /// </summary>
    public struct PlayerLevelUpEvent
    {
        public int NewLevel;

        public PlayerLevelUpEvent(int newLevel)
        {
            NewLevel = newLevel;
        }
    }

    /// <summary>
    /// Sự kiện phát ra khi gây sát thương thành công.
    /// </summary>
    public struct DamageDealtEvent
    {
        public Vector3 HitPosition;
        public DamageData DamageData;
        public GameObject Target;

        public DamageDealtEvent(Vector3 hitPosition, DamageData damageData, GameObject target)
        {
            HitPosition = hitPosition;
            DamageData = damageData;
            Target = target;
        }
    }

    /// <summary>
    /// Sự kiện phát ra khi nhặt ngọc kinh nghiệm.
    /// </summary>
    public struct ExpCollectedEvent
    {
        public int Amount;
        public Vector3 Position;

        public ExpCollectedEvent(int amount, Vector3 position)
        {
            Amount = amount;
            Position = position;
        }
    }

    /// <summary>
    /// Sự kiện thay đổi trạng thái Game.
    /// </summary>
    public struct GameStateChangedEvent
    {
        public GameState PreviousState;
        public GameState NewState;

        public GameStateChangedEvent(GameState previousState, GameState newState)
        {
            PreviousState = previousState;
            NewState = newState;
        }
    }
}
