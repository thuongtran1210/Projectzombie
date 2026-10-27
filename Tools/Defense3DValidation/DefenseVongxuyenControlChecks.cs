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

    public static void Main()
    {
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
