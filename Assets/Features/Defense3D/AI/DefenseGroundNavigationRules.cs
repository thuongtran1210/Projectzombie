using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Immutable map and aggro settings captured from an authored navigation profile.</summary>
    public sealed class DefenseGroundNavigationRules
    {
        public float WestEdge { get; }
        public float EastEdge { get; }
        public float EastEntry { get; }
        public float BridgeCenter { get; }
        public float WestExit { get; }
        private readonly float[] _lanes;
        public float AcquireRange { get; }
        public float RetainRange { get; }
        public float TargetLockSeconds { get; }
        public float RepathSeconds { get; }
        public float ProvokedSeconds { get; }
        public float BridgeHalfWidth { get; }

        public DefenseGroundNavigationRules(float westEdge,float eastEdge,float eastEntry,float center,float westExit,
            float northZ,float middleZ,float southZ,float acquireRange,float retainRange,float lockSeconds,float repathSeconds,float provokedSeconds,
            float bridgeHalfWidth=1.4f)
        {
            float[] values={westEdge,eastEdge,eastEntry,center,westExit,northZ,middleZ,southZ,acquireRange,retainRange,lockSeconds,repathSeconds,provokedSeconds,bridgeHalfWidth};
            foreach (float value in values) if (!DefenseDamageRules.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(westEdge));
            if (westEdge>=eastEdge || westExit>westEdge || eastEntry<eastEdge || center<=westEdge || center>=eastEdge ||
                acquireRange<=0f || retainRange<acquireRange || lockSeconds<0f || repathSeconds<=0f || provokedSeconds<0f || bridgeHalfWidth<=0f)
                throw new ArgumentOutOfRangeException(nameof(acquireRange));
            WestEdge=westEdge; EastEdge=eastEdge; EastEntry=eastEntry; BridgeCenter=center; WestExit=westExit;
            _lanes=new[]{northZ,middleZ,southZ}; AcquireRange=acquireRange; RetainRange=retainRange;
            TargetLockSeconds=lockSeconds; RepathSeconds=repathSeconds; ProvokedSeconds=provokedSeconds;
            BridgeHalfWidth=bridgeHalfWidth;
        }

        public bool SameBank(DefensePoint3 a,DefensePoint3 b) => a.IsFinite && b.IsFinite &&
            ((a.X<=WestEdge && b.X<=WestEdge) || (a.X>=EastEdge && b.X>=EastEdge));

        public bool TryWriteRoute(DefensePoint3 origin,DefensePoint3 destination,int lane,DefensePoint3[] buffer,out int count,
            bool allowExistingBridge = false)
        {
            count=0;
            if (!origin.IsFinite || !destination.IsFinite || lane<0 || lane>=_lanes.Length || destination.X>WestEdge) return false;
            if (buffer==null) return false;
            if (origin.X<=WestEdge)
            {
                if (buffer.Length<1) return false;
                buffer[0]=destination; count=1; return true;
            }
            if (origin.X<EastEdge)
            {
                // Resume a committed lane route; this never authorizes a new ground spawn in water.
                if (!allowExistingBridge || Math.Abs(origin.Z-_lanes[lane])>BridgeHalfWidth || buffer.Length<2) return false;
                buffer[0]=new DefensePoint3(WestExit,0f,_lanes[lane]); buffer[1]=destination; count=2; return true;
            }
            if (buffer.Length<4) return false;
            float z=_lanes[lane];
            buffer[0]=new DefensePoint3(EastEntry,0f,z); buffer[1]=new DefensePoint3(BridgeCenter,0f,z);
            buffer[2]=new DefensePoint3(WestExit,0f,z); buffer[3]=destination; count=4;
            return true;
        }
    }

    public enum DefenseGroundFocus { Base, Hero }

    /// <summary>Engine-free hysteresis policy; navigation remains responsible for path feasibility.</summary>
    public sealed class DefenseGroundAggroPolicy
    {
        private readonly DefenseGroundNavigationRules _rules;
        private float _lockUntil;
        public DefenseGroundFocus Focus { get; private set; }
        public DefenseGroundAggroPolicy(DefenseGroundNavigationRules rules) => _rules=rules ?? throw new ArgumentNullException(nameof(rules));

        public DefenseGroundFocus Select(float gameplayTime,float heroDistance,bool heroAvailable,bool reachable,bool provoked)
        {
            if (!DefenseDamageRules.IsFinite(gameplayTime) || !DefenseDamageRules.IsFinite(heroDistance) || gameplayTime<0f || heroDistance<0f ||
                !heroAvailable || !reachable) { Focus=DefenseGroundFocus.Base; _lockUntil=0f; return Focus; }
            if (Focus==DefenseGroundFocus.Hero && (heroDistance<=_rules.RetainRange || gameplayTime<_lockUntil)) return Focus;
            float acquire=provoked ? _rules.RetainRange : _rules.AcquireRange;
            if (heroDistance<=acquire)
            {
                if (Focus!=DefenseGroundFocus.Hero) _lockUntil=gameplayTime+_rules.TargetLockSeconds;
                Focus=DefenseGroundFocus.Hero;
            }
            else Focus=DefenseGroundFocus.Base;
            return Focus;
        }
    }
}
