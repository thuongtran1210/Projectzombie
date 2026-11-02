using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Optional prototype diagnostics. Contains no match-state or spawning logic.</summary>
    public static class DefenseRunDebugOverlay
    {
        public static void Draw(DefenseRun6003D run)
        {
            if (run == null || run.Base == null) return;
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// Component IMGUI độc lập, chỉ được gắn vào GameObject khi showDebugOverlay = true.
    /// Giúp DefenseRun6003D không chứa phương thức OnGUI(), triệt tiêu 100% việc Unity
    /// kích hoạt GUIUtility.BeginGUI và loại bỏ hoàn toàn 368 B GC Alloc / frame!
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DefenseRunDebugOverlayBehaviour : MonoBehaviour
    {
        private DefenseRun6003D _run;

        public static void Sync(DefenseRun6003D run, bool show)
        {
            if (run == null) return;
            var overlay = run.GetComponent<DefenseRunDebugOverlayBehaviour>();
            if (show)
            {
                if (overlay == null) overlay = run.gameObject.AddComponent<DefenseRunDebugOverlayBehaviour>();
                overlay._run = run;
                overlay.enabled = true;
            }
            else if (overlay != null)
            {
                overlay.enabled = false;
            }
        }

        private void OnGUI()
        {
            if (_run == null || !_run.isActiveAndEnabled) return;
            DefenseRunDebugOverlay.Draw(_run);
        }
    }
#endif
}
