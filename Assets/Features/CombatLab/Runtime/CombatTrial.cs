using System;

namespace ProjectZombie.Features.CombatLab
{
    /// <summary>Explicit-window measurements; never treats requested damage as HP removed.</summary>
    public sealed class CombatTrial
    {
        public int Attacks { get; private set; }
        public int Hits { get; private set; }
        public double RequestedDamage { get; private set; }
        public double HpRemoved { get; private set; }
        public double Overkill { get; private set; }
        public double IntervalTotal { get; private set; }
        public int IntervalSamples { get; private set; }
        private double _lastAttack = -1d;

        /// <summary>Records gameplay seconds relative to the start of this trial.</summary>
        public void RecordAttack(double seconds)
        {
            RequireNonNegative(seconds);
            if (_lastAttack > seconds) throw new ArgumentOutOfRangeException(nameof(seconds));
            if (_lastAttack >= 0d)
            {
                IntervalTotal += seconds - _lastAttack;
                IntervalSamples++;
            }
            _lastAttack = seconds;
            Attacks++;
        }

        /// <summary>Records accepted damage using HP captured immediately before and after application.</summary>
        public void RecordHit(double requested, double hpBefore, double hpAfter)
        {
            RequireNonNegative(requested);
            RequireNonNegative(hpBefore);
            RequireNonNegative(hpAfter);
            if (requested <= 0d || hpAfter > hpBefore) throw new ArgumentOutOfRangeException(nameof(requested));
            Hits++;
            RequestedDamage += requested;
            HpRemoved += hpBefore - hpAfter;
            // Overkill is only identifiable for lethal hits with positive pre-hit HP.
            if (hpBefore > 0d && hpAfter == 0d) Overkill += Math.Max(0d, requested - hpBefore);
        }

        /// <summary>Includes all idle and movement time in the explicit measurement window.</summary>
        public double Dps(double elapsed)
        {
            RequireNonNegative(elapsed);
            return elapsed > 0d ? HpRemoved / elapsed : 0d;
        }

        /// <summary>Single-target nominal ceiling; windup bounds the pending-attack scheduler.</summary>
        public static double NominalDps(double damage, double interval, double windup)
        {
            RequireNonNegative(damage);
            RequireNonNegative(interval);
            RequireNonNegative(windup);
            if (interval <= 0d || windup <= 0d) throw new ArgumentOutOfRangeException(nameof(interval));
            return damage / Math.Max(interval, windup);
        }

        private static void RequireNonNegative(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d)
                throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
}
