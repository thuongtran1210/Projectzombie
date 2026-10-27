using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Coordinates a match frame and its results using separately owned spawn and encounter services.</summary>
    public sealed class DefenseMatchRuntime : IDisposable
    {
        private readonly DefenseSession _session;
        private readonly DefenseBasePrototype3D _base;
        private readonly DefenseHeroHealth3D _hero;
        private readonly DefenseTowerBuildPrototype3D _towers;
        private readonly DefenseSpawnRuntime _spawns;
        private readonly DefenseYakshaEncounter3D _encounter;
        private bool _resultPublished;
        private bool _disposed;

        public event Action<bool> MatchEnded;

        public DefenseMatchRuntime(DefenseSession session, DefenseBasePrototype3D defenseBase,
            DefenseHeroHealth3D hero, DefenseTowerBuildPrototype3D towers,
            DefenseSpawnRuntime spawns, DefenseYakshaEncounter3D encounter)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _base = defenseBase != null ? defenseBase : throw new ArgumentNullException(nameof(defenseBase));
            _hero = hero != null ? hero : throw new ArgumentNullException(nameof(hero));
            _towers = towers;
            _spawns = spawns ?? throw new ArgumentNullException(nameof(spawns));
            _encounter = encounter ?? throw new ArgumentNullException(nameof(encounter));
            _base.BaseDestroyed += OnBaseDestroyed;
        }

        public void Tick(float deltaSeconds)
        {
            if (_disposed || _session.IsDisposed) return;
            if (_session.IsTerminal)
            {
                _hero.AdvanceGameplayTime(0f, true);
                PublishResult();
                return;
            }
            float accepted = _session.Tick(deltaSeconds, _base.CurrentHP, _encounter.IsDefeated);
            _hero.AdvanceGameplayTime(accepted, _session.IsTerminal);
            PublishResult();
            if (!_session.AdvancesGameplay) return;
            _spawns.Tick();
            _encounter.Tick();
            _session.Tick(0f, _base.CurrentHP, _encounter.IsDefeated);
            _hero.AdvanceGameplayTime(0f, _session.IsTerminal);
            PublishResult();
        }

        /// <summary>Used by both restart and session shutdown; safe to call again.</summary>
        public void Cleanup()
        {
            if (!_disposed) _spawns.Cleanup();
        }

        public void Reset()
        {
            if (_disposed) return;
            _resultPublished = false;
            if (_base != null) _base.ResetBase();
            if (_hero != null) _hero.ResetHero();
            if (_towers != null) _towers.ResetTowers();
        }

        public void Dispose()
        {
            if (_disposed) return;
            if (_base != null) _base.BaseDestroyed -= OnBaseDestroyed;
            try { _spawns.Dispose(); }
            finally
            {
                _encounter.Dispose();
                _disposed = true;
                MatchEnded = null;
            }
        }

        private void OnBaseDestroyed()
        {
            if (_disposed || _session.IsDisposed || _session.IsTerminal) return;
            _session.Tick(0f, 0f, _encounter.IsDefeated);
            _hero.AdvanceGameplayTime(0f, true);
            PublishResult();
        }

        private void PublishResult()
        {
            if (_resultPublished || !_session.IsTerminal) return;
            _resultPublished = true;
            bool won = _session.Phase == DefenseGamePhase.Win;
            if (won) _base.LockDamageAfterWin();
            _encounter.StopCombat();
            Debug.Log(won ? "[TREBRAVE Defense3D] WIN: 600s + Yaksha defeated, Base alive."
                : "[TREBRAVE Defense3D] LOSS: BaseHP=0 (priority over WIN).");
            MatchEnded?.Invoke(won);
        }
    }
}
