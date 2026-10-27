using System;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.UI.Controls;

public static class DefenseVongxuyenControlChecks
{
    private static int _checks;
    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception(name);
        _checks++;
        Console.WriteLine("PASS " + name);
    }
    private static bool Close(float a, float b) => Math.Abs(a-b) < 0.001f;

    private static void CheckVillageContext()
    {
        var origin = new DefensePoint3(0f, 0f, 0f);
        Check(DefenseVillageInteractionRules.InZone(new DefensePoint3(1.6f, 100f, 0f), origin, 1.6f, true), "Hut includes its 1.6m XZ boundary regardless of actor height");
        Check(!DefenseVillageInteractionRules.InZone(new DefensePoint3(1.6001f, 0f, 0f), origin, 1.6f, true), "Leaving the hut radius closes recruitment access");
        Check(!DefenseVillageInteractionRules.InZone(new DefensePoint3(1.7f, 0f, 0f), origin, 1.7f, false), "Tower zone uses the source strict 1.7m boundary");
        Check(DefenseVillageInteractionRules.InZone(new DefensePoint3(1.699f, 0f, 0f), origin, 1.7f, false), "Tower context opens inside its interaction radius");
        Check(!DefenseVillageInteractionRules.InZone(new DefensePoint3(float.NaN, 0f, 0f), origin, 1.6f, true), "Invalid positions cannot open an interaction zone");
        Check(DefenseVillageInteractionRules.Resolve(false, true, true, 0, 100, 50, 40).Action == DefenseVillageAction.None, "Closed gameplay gate hides all context actions");
        Check(DefenseVillageInteractionRules.Resolve(true, false, false, 0, 100, 50, 40).Action == DefenseVillageAction.None, "Outside every zone no global shop or build button is shown");
        var hut = DefenseVillageInteractionRules.Resolve(true, true, true, 1, 0, 50, 40);
        Check(hut.Action == DefenseVillageAction.Recruit && hut.Enabled, "Hut takes priority over a nearby tower and opening shop costs no wood");
        var poor = DefenseVillageInteractionRules.Resolve(true, false, true, 0, 49, 50, -1);
        Check(poor.Action == DefenseVillageAction.Build && !poor.Enabled && poor.Shortage == 1, "An unaffordable build remains visible with an exact shortage");
        Check(DefenseVillageInteractionRules.Resolve(true, false, true, 0, 50, 50, -1).Enabled, "Exact build balance enables the context action");
        var upgrade = DefenseVillageInteractionRules.Resolve(true, false, true, 1, 40, 50, 40);
        Check(upgrade.Action == DefenseVillageAction.Upgrade && upgrade.Enabled && upgrade.Cost == 40, "Occupied pedestal switches to the actual upgrade price");
        var max = DefenseVillageInteractionRules.Resolve(true, false, true, 3, 1000, 50, -1);
        Check(max.Action == DefenseVillageAction.Maximum && !max.Enabled, "Level-three tower shows maximum level without allowing further purchases");
    }

    public static void Main()
    {
        CheckVillageContext();
        var gesture = new SkillPointerGesture(6f,52f,0d,false,true);
        var press = gesture.Press(1,100f,200f,0d);
        Check(press.Signal == SkillPointerSignal.Aiming && press.AimStarted, "Vongxuyen press opens aim immediately without hold delay");
        Check(gesture.Release(1,100f,200f,0.02d,false).Signal == SkillPointerSignal.TapReleased,
            "Immediate preview preserves quick-tap release policy");
        gesture.Press(1,100f,200f,1d);
        gesture.Tick(4d);
        Check(gesture.Release(1,100f,200f,4.01d,false).Signal == SkillPointerSignal.TapReleased,
            "Long stationary hold still uses tap auto-target instead of zero-distance cast");
        gesture.Press(1,100f,200f,5d);
        gesture.Move(1,105f,200f,5.01d,false);
        Check(!gesture.IsDragging, "Small movement below 6px does not take over the initial facing aim");
        var drag = gesture.Move(1,113f,200f,5.02d,false);
        Check(gesture.IsDragging && Close(drag.Pull,0.25f) && drag.DirectionX == 1f,
            "13px drag is quarter pull on the 52px control radius");
        Check(gesture.Release(1,113f,200f,5.03d,false).Signal == SkillPointerSignal.AimReleased,
            "Drag release requests explicit aimed cast");
        gesture.Press(1,100f,200f,6d);
        drag = gesture.Move(1,204f,200f,6.01d,false);
        Check(drag.Pull == 1f, "Dragging beyond 52px clamps pull without moving the pad origin");
        Check(gesture.Release(2,204f,200f,6.02d,false).Signal == SkillPointerSignal.None && gesture.IsPressed,
            "Another finger cannot end the active thumb pad");
        Check(gesture.Release(1,204f,200f,6.03d,true).Signal == SkillPointerSignal.Cancelled,
            "Circular cancel-zone hit cancels instead of aimed cast");

        var right = new DefensePoint3(1f,0f,0f);
        var up = new DefensePoint3(0f,0f,1f);
        DefenseAimResolver.TryResolveDrag(default,right,up,1f,0f,7f/52f,10f,up,out var aim,1.5f);
        Check(Close(aim.Distance,1.5f), "Short directional drag uses Vongxuyen minimum reach 1.5m");
        DefenseAimResolver.TryResolveDrag(default,right,up,1f,0f,13f/52f,10f,up,out aim,1.5f);
        Check(Close(aim.Target.X,2.5f), "Quarter thumb pull matches Vongxuyen quarter world range");
        DefenseAimResolver.TryResolveDrag(default,right,up,0f,1f,2f,10f,up,out aim,1.5f);
        Check(Close(aim.Distance,10f) && Close(aim.Target.Z,10f), "Screen up maps to camera-relative ground up at maximum range");
        DefenseAimResolver.TryResolveDrag(default,right,up,1f,0f,0.1f,1f,up,out aim,1.5f);
        Check(Close(aim.Distance,1f), "Minimum aim reach never exceeds the authored skill range");
        Check(!DefenseAimResolver.TryResolveDrag(default,right,up,1f,0f,0.1f,10f,up,out aim,float.NaN),
            "Invalid minimum reach cannot produce aim geometry");
        Console.WriteLine("TOTAL " + _checks + " PASS (standalone Vongxuyen control parity, no Unity runtime)");
    }
}
