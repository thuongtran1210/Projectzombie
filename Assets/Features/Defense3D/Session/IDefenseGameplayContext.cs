using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Read-only time and execution gate for one Defense session.</summary>
    public interface IDefenseGameplayContext
    {
        bool AdvancesGameplay { get; }
        float ElapsedSeconds { get; }
        int RunGeneration { get; }
        object SessionIdentity { get; }
        IDefenseTargetQuery Targets { get; }
        IDefenseStatModifiers Stats { get; }
        event Action<bool> GameplayGateChanged;
        event Action RunCleanup;
        event Action RunReset;
    }

    /// <summary>Commands allowed at the session boundary; consumers do not mutate the FSM directly.</summary>
    public interface IDefenseSessionCommands
    {
        bool Pause();
        bool OpenLevelUp();
        bool Resume();
        void Restart();
    }
}
