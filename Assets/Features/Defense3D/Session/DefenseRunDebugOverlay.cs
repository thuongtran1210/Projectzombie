using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Optional prototype diagnostics. Contains no match-state or spawning logic.</summary>
    public static class DefenseRunDebugOverlay
    {
        public static void Draw(DefenseRun6003D run)
        {
            if (run.Base == null) return;
            GUI.Box(new Rect(16, 16, 355, 158), "TREBRAVE Defense 3D");
            GUI.Label(new Rect(28, 43, 330, 22), "Time: " + Mathf.FloorToInt(run.ElapsedTime) + "s / 600s | Events: " + run.ProcessedEventsCount + "/" + run.TotalEventsCount);
            GUI.Label(new Rect(28, 66, 330, 22), "Base HP: " + Mathf.CeilToInt(run.Base.CurrentHP) + "/" + Mathf.CeilToInt(run.Base.MaxHP));
            GUI.Label(new Rect(28, 89, 330, 22), run.IsEnded ? (run.IsWon ? "WIN" : "LOSS") :
                run.IsOvertime ? "OVERTIME" : run.BossSpawned ? (run.AssaultStarted ? "Yaksha: ASSAULT_BASE" : "Yaksha: ARENA_COMBAT") : "Defend the West Bank");
            if (run.Yaksha != null && !run.Yaksha.IsDead)
                GUI.Label(new Rect(28, 112, 330, 22), "Yaksha HP: " + Mathf.CeilToInt(run.Yaksha.CurrentHP));
            if (run.HeroHealth != null)
                GUI.Label(new Rect(28, 134, 330, 22), "Hero HP: " + Mathf.CeilToInt(run.HeroHealth.CurrentHP));
            if (!run.IsEnded) return;
            float x = (Screen.width - 420f) * 0.5f;
            float y = (Screen.height - 180f) * 0.5f;
            GUI.Box(new Rect(x, y, 420f, 180f), run.IsWon ? "VICTORY" : "DEFEAT");
            if (GUI.Button(new Rect(x + 135f, y + 110f, 150f, 40f), "Restart Match")) run.RestartRun();
        }
    }
}
