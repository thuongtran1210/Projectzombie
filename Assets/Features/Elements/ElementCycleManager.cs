using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.Elements
{
    /// <summary>Owner-local reward contract, independent of player input and UI implementations.</summary>
    public interface IElementSynergyReceiver
    {
        void ApplyElementSynergyReward();
        void ResetElementSynergyReward();
    }

    public struct ElementHitEntry
    {
        public ElementType element;
        public float timestamp;
        public WeaponBase weapon;
        public bool isVirtualHit;
        public ElementHitSource source;
        public ulong attackId;

        public ElementHitEntry(ElementType element, float timestamp, WeaponBase weapon,
            ElementHitSource source, ulong attackId)
        {
            this.element = element;
            this.timestamp = timestamp;
            this.weapon = weapon;
            this.source = source;
            this.attackId = attackId;
            isVirtualHit = source == ElementHitSource.VirtualHero;
        }
    }

    /// <summary>GDD 6.2: an owner's basic/virtual hero hit primes their next generative relic hit.</summary>
    public class ElementCycleManager : MonoBehaviour
    {
        public static ElementCycleManager Instance { get; private set; }
        private sealed class PlayerCycle
        {
            public ElementHitEntry Lead;
            public bool HasLead;
            public float LastProc = float.NegativeInfinity;
            public IElementSynergyReceiver Receiver;
            // Kept for the run, so even long-lived/piercing projectiles cannot register twice.
            public readonly HashSet<ulong> SeenAttacks = new HashSet<ulong>();
        }

        private readonly Dictionary<GameObject, PlayerCycle> _players = new Dictionary<GameObject, PlayerCycle>();
        private readonly List<GameObject> _expiredOwners = new List<GameObject>();
        private GameStateManager _gameState;
        private GameState _previousState;

        public event System.Action<ElementType, ElementType, WeaponBase> OnElementSynergyTriggered;
        public event System.Action<GameObject, ElementHitEntry> OnElementHitRecorded;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void OnEnable() => BindGameState();
        private void Start() => BindGameState();

        private void BindGameState()
        {
            if (_gameState != null || GameStateManager.Instance == null) return;
            _gameState = GameStateManager.Instance;
            _previousState = _gameState.CurrentState;
            _gameState.OnStateChanged += HandleGameStateChanged;
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.MainMenu || state == GameState.GameOver ||
                (state == GameState.Playing && (_previousState == GameState.MainMenu || _previousState == GameState.GameOver)))
                ResetRunState();
            _previousState = state;
        }

        private void OnDisable()
        {
            if (_gameState != null) _gameState.OnStateChanged -= HandleGameStateChanged;
            _gameState = null;
            ResetRunState();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void LateUpdate()
        {
            _expiredOwners.Clear();
            foreach (var entry in _players)
                if (entry.Key == null) _expiredOwners.Add(entry.Key);
            foreach (var owner in _expiredOwners) _players.Remove(owner);
        }

        private static GameObject ResolveOwner(GameObject owner)
        {
            if (owner != null && owner.GetComponentInParent<ICharacterStats>() is Component character)
                return character.gameObject;
            return owner;
        }

        private PlayerCycle GetCycle(GameObject owner)
        {
            if (owner == null) return null;
            if (!_players.TryGetValue(owner, out var cycle))
            {
                cycle = new PlayerCycle { Receiver = owner.GetComponent<IElementSynergyReceiver>() };
                _players.Add(owner, cycle);
            }
            return cycle;
        }

        /// <summary>Clears leads, proc cooldowns, deduplication and temporary rewards between runs.</summary>
        public void ResetRunState()
        {
            foreach (var entry in _players)
                if (entry.Key != null) entry.Value.Receiver?.ResetElementSynergyReward();
            _players.Clear();
        }

        /// <summary>Clears one owner when despawned or reset without affecting teammates.</summary>
        public void ResetOwner(GameObject owner)
        {
            owner = ResolveOwner(owner);
            if (owner == null || !_players.TryGetValue(owner, out var cycle)) return;
            cycle.Receiver?.ResetElementSynergyReward();
            _players.Remove(owner);
        }

        /// <summary>Scholar privilege: one virtual lead may bypass the proc cooldown, using identical rewards.</summary>
        public void PushVirtualElementHit(ElementType element, GameObject owner)
        {
            if (element == ElementType.None) return;
            owner = ResolveOwner(owner);
            var cycle = GetCycle(owner);
            if (cycle == null) return;
            cycle.Lead = new ElementHitEntry(element, Time.time, null, ElementHitSource.VirtualHero, ElementSynergyRules.NextAttackId());
            cycle.HasLead = true;
            OnElementHitRecorded?.Invoke(owner, cycle.Lead);
        }

        /// <summary>Only accepted hero-basic hits lead; only relic hits complete. Target identity is irrelevant.</summary>
        public void RegisterHit(ElementType hitElement, WeaponBase weapon, GameObject owner = null,
            ElementHitSource source = ElementHitSource.Unknown, ulong attackId = 0)
        {
            if (hitElement == ElementType.None || attackId == 0) return;
            if (weapon != null) owner = weapon.OwnerGameObject;
            if (source != ElementHitSource.HeroBasicAttack && source != ElementHitSource.Relic) return;
            if (source == ElementHitSource.Relic && (weapon == null || weapon.isPrimaryActiveWeapon)) return;
            owner = ResolveOwner(owner);
            var cycle = GetCycle(owner);
            if (cycle == null || !cycle.SeenAttacks.Add(attackId)) return;

            float now = Time.time;
            var incoming = new ElementHitEntry(hitElement, now, weapon, source, attackId);
            if (source == ElementHitSource.HeroBasicAttack)
            {
                cycle.Lead = incoming;
                cycle.HasLead = true;
                OnElementHitRecorded?.Invoke(owner, incoming);
                return;
            }

            var lead = cycle.Lead;
            bool proc = cycle.HasLead && now - lead.timestamp <= ElementSynergyRules.HIT_WINDOW_SECONDS
                && IsGenerationPair(lead.element, hitElement)
                && (lead.isVirtualHit || now - cycle.LastProc >= ElementSynergyRules.PROC_COOLDOWN_SECONDS);
            if (proc)
            {
                // Consume before feedback: nested on-hit effects cannot reuse the same lead.
                cycle.HasLead = false;
                cycle.LastProc = now;
                cycle.Receiver?.ApplyElementSynergyReward();
                global::ProjectZombie.Core.Audio.AudioService.Current?.PlayElementalReaction(owner.transform.position);
                OnElementSynergyTriggered?.Invoke(lead.element, hitElement, weapon);
            }
            OnElementHitRecorded?.Invoke(owner, incoming);
        }

        public bool IsGenerationPair(ElementType first, ElementType second)
            => ElementSynergyRules.IsElementGenerative(first, second);
    }
}
