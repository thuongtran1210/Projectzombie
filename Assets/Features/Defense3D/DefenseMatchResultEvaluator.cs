using System;

namespace ProjectZombie.Features.Defense3D
{
    public struct DefenseMatchSummary
    {
        public bool IsWin;
        public float ElapsedTime;
        public bool YakshaDefeated;
        public float BaseRemainingHp;
        public int TotalKills;
        public int TowerCount;

        public DefenseMatchSummary(bool isWin, float elapsed, bool yakshaDead, float baseHp, int kills, int towers)
        {
            IsWin = isWin;
            ElapsedTime = elapsed;
            YakshaDefeated = yakshaDead;
            BaseRemainingHp = baseHp;
            TotalKills = kills;
            TowerCount = towers;
        }
    }

    /// <summary>
    /// U3D-022: Authoritative match result evaluator according to D-03/A and D-04/A.
    /// Invariants:
    /// - BaseHP <= 0 is ALWAYS LOSS, evaluated ahead of boss defeat even on the exact same tick.
    /// - Defeating Yaksha before 600s does NOT trigger WIN; defense must continue until 600s.
    /// - Reaching 600s while Yaksha is alive enters OVERTIME.
    /// - WIN requires: BaseHP > 0 && ElapsedTime >= 600s && YakshaDefeated.
    /// </summary>
    public static class DefenseMatchResultEvaluator
    {
        public static DefenseGamePhase EvaluatePhase(float baseHp, bool yakshaDefeated, float elapsedSeconds, DefenseGamePhase currentPhase)
        {
            // Terminal LOSS priority
            if (baseHp <= 0f)
            {
                return DefenseGamePhase.Loss;
            }

            // Cannot win or enter overtime before 600s
            if (elapsedSeconds < 600f)
            {
                return currentPhase == DefenseGamePhase.Ready ? DefenseGamePhase.Ready : DefenseGamePhase.Playing;
            }

            // At or past 600s
            if (yakshaDefeated)
            {
                return DefenseGamePhase.Win;
            }

            return DefenseGamePhase.Overtime;
        }
    }
}
