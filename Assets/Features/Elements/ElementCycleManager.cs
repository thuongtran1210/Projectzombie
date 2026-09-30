using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Weapons;

namespace ProjectZombie.Features.Elements
{
    public struct ElementHitEntry
    {
        public ElementType element;
        public float timestamp;
        public WeaponBase weapon;
        public bool isVirtualHit;
        public ElementHitEntry(ElementType element, float timestamp, WeaponBase weapon, bool isVirtualHit = false)
        {
            this.element = element;
            this.timestamp = timestamp;
            this.weapon = weapon;
            this.isVirtualHit = isVirtualHit;
        }
    }
    /// <summary>Tracks ordered elemental hits and proc cooldown independently for each owner.</summary>
    public class ElementCycleManager : MonoBehaviour
    {
        public static ElementCycleManager Instance { get; private set; }
        [SerializeField] private float _windowTimeSeconds = 3f;
        [SerializeField] private float _procCooldownSeconds = 3f;
        private sealed class PlayerCycle
        {
            public ElementHitEntry Latest;
            public bool HasHit;
            public float LastProc = float.NegativeInfinity;
        }
        private readonly Dictionary<GameObject, PlayerCycle> _players = new Dictionary<GameObject, PlayerCycle>();
        private readonly List<GameObject> _expiredOwners = new List<GameObject>();
        public event System.Action<ElementType, ElementType, WeaponBase> OnElementSynergyTriggered;
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }
        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            _players.Clear();
        }
        private void LateUpdate()
        {
            _expiredOwners.Clear();
            foreach (var entry in _players)
                if (entry.Key == null) _expiredOwners.Add(entry.Key);
            foreach (var owner in _expiredOwners) _players.Remove(owner);
        }
        private PlayerCycle GetCycle(GameObject owner)
        {
            if (owner == null) return null;
            if (owner.GetComponentInParent<ICharacterStats>() is Component character)
                owner = character.gameObject;
            if (!_players.TryGetValue(owner, out var cycle))
            {
                cycle = new PlayerCycle();
                _players.Add(owner, cycle);
            }
            return cycle;
        }
        /// <summary>Primes one owner's next hit; a successful virtual pair bypasses their normal proc cooldown.</summary>
        public void PushVirtualElementHit(ElementType element, GameObject owner)
        {
            var cycle = GetCycle(owner);
            if (cycle == null || element == ElementType.None) return;
            cycle.Latest = new ElementHitEntry(element, Time.time, null, true);
            cycle.HasHit = true;
        }
        /// <summary>Records an accepted hit. Unidentified sources never enter a shared fallback buffer.</summary>
        public void RegisterHit(ElementType hitElement, WeaponBase weapon, GameObject owner = null)
        {
            if (weapon != null) owner = weapon.OwnerGameObject;
            var cycle = GetCycle(owner);
            if (cycle == null || hitElement == ElementType.None) return;
            float now = Time.time;
            var previous = cycle.Latest;
            bool proc = cycle.HasHit && now - previous.timestamp <= _windowTimeSeconds
                && IsGenerationPair(previous.element, hitElement)
                && (previous.isVirtualHit || now - cycle.LastProc >= _procCooldownSeconds);
            // Replace before callbacks: a virtual hit is consumed exactly once.
            cycle.Latest = new ElementHitEntry(hitElement, now, weapon);
            cycle.HasHit = true;
            if (!proc) return;
            if (!previous.isVirtualHit) cycle.LastProc = now;
            if (weapon != null) weapon.ReduceRelicSkillCooldown(0.2f);
            global::ProjectZombie.Core.Audio.AudioService.Current?.PlayElementalReaction(owner.transform.position);
            OnElementSynergyTriggered?.Invoke(previous.element, hitElement, weapon);
        }
        public bool IsGenerationPair(ElementType first, ElementType second)
            => ElementSynergyRules.IsElementGenerative(first, second);
    }
}
