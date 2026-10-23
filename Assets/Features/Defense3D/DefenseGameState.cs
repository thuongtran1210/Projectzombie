using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Authoritative gameplay state for G1 (U3D-003). Pure C#; no Unity scene references.</summary>
    public enum DefenseGamePhase
    {
        Ready,
        Playing,
        Paused,
        LevelUp,
        Overtime,
        Win,
        Loss
    }

    /// <summary>
    /// Controls legal transitions. Terminal results are irreversible until an explicit Reset.
    /// Base loss is evaluated before victory to enforce D-03/A same-tick priority.
    /// This class does not advance time, spawn objects, or alter MonoBehaviours.
    /// </summary>
    public sealed class DefenseGameState
    {
        public DefenseGamePhase Phase { get; private set; } = DefenseGamePhase.Ready;
        public DefenseGamePhase ResumePhase { get; private set; } = DefenseGamePhase.Playing;
        public event Action<DefenseGamePhase, DefenseGamePhase> Changed;

        public bool IsTerminal => Phase == DefenseGamePhase.Win || Phase == DefenseGamePhase.Loss;
        public bool AdvancesGameplay => Phase == DefenseGamePhase.Playing || Phase == DefenseGamePhase.Overtime;

        public bool TryStart() => Transition(DefenseGamePhase.Playing);
        public bool TryPause() => Transition(DefenseGamePhase.Paused);
        public bool TryOpenLevelUp() => Transition(DefenseGamePhase.LevelUp);
        public bool TryResume() => Transition(ResumePhase);

        /// <summary>
        /// Evaluate authoritative results once after same-tick base/boss updates.
        /// BaseHP zero always wins over a simultaneous boss kill or timeout.
        /// A boss kill before 600 gameplay seconds cannot end the game.
        /// </summary>
        public void Resolve(float baseHp, bool yakshaDefeated, float gameplaySeconds)
        {
            if (IsTerminal || Phase == DefenseGamePhase.Ready) return;
            if (baseHp <= 0f)
            {
                Transition(DefenseGamePhase.Loss);
                return;
            }

            if (gameplaySeconds < 600f) return;
            if (yakshaDefeated)
            {
                Transition(DefenseGamePhase.Win);
                return;
            }

            if (Phase == DefenseGamePhase.Playing)
                Transition(DefenseGamePhase.Overtime);
            // If paused/modal at the threshold, the clock should not advance;
            // the caller re-evaluates after resuming.
        }

        public void Reset()
        {
            var previous = Phase;
            Phase = DefenseGamePhase.Ready;
            ResumePhase = DefenseGamePhase.Playing;
            if (previous != Phase) Changed?.Invoke(previous, Phase);
        }

        private bool Transition(DefenseGamePhase target)
        {
            if (IsTerminal || Phase == target) return false;
            bool allowed;
            switch (Phase)
            {
                case DefenseGamePhase.Ready:
                    allowed = target == DefenseGamePhase.Playing;
                    break;
                case DefenseGamePhase.Playing:
                case DefenseGamePhase.Overtime:
                    allowed = target == DefenseGamePhase.Paused ||
                              target == DefenseGamePhase.LevelUp ||
                              (Phase == DefenseGamePhase.Playing && target == DefenseGamePhase.Overtime) ||
                              target == DefenseGamePhase.Win ||
                              target == DefenseGamePhase.Loss;
                    break;
                case DefenseGamePhase.Paused:
                case DefenseGamePhase.LevelUp:
                    allowed = target == ResumePhase || target == DefenseGamePhase.Loss;
                    break;
                default:
                    return false;
            }
            if (!allowed) return false;
            var previous = Phase;
            if (target == DefenseGamePhase.Paused || target == DefenseGamePhase.LevelUp)
                ResumePhase = Phase;
            Phase = target;
            Changed?.Invoke(previous, target);
            return true;
        }
    }
}
