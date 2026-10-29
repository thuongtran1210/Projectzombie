using System;
using ProjectZombie.Features.Defense3D;

public static class DefenseGroundAggroChecks
{
    private static int _checks;
    private static void Check(bool value,string name)
    {
        if (!value) throw new Exception(name);
        _checks++;
        Console.WriteLine("PASS " + name);
    }
    private static DefenseGroundNavigationRules Rules() => new DefenseGroundNavigationRules(4.45f,8.05f,8.8f,6.25f,3.8f,
        -10.5f,-0.5f,10.5f,4.2f,7f,0.75f,0.25f,3f);

    public static void Main()
    {
        var rules=Rules();
        var actualBase=new DefensePoint3(-22.41f,1.3f,0f);
        var buffer=new DefensePoint3[4];
        for (int lane=0;lane<3;lane++)
        {
            Check(rules.TryWriteRoute(new DefensePoint3(25f,1f,0f),actualBase,lane,buffer,out var count) && count==4 &&
                buffer[3].X==actualBase.X && buffer[3].Y==actualBase.Y,
                "East lane "+lane+" ends at actual Base rather than obsolete x=-12");
        }
        Check(rules.TryWriteRoute(new DefensePoint3(-5f,1f,4f),actualBase,0,buffer,out int westCount) && westCount==1 && buffer[0].X==-22.41f,
            "West spawn routes directly to Base instead of crossing back to East");
        Check(!rules.TryWriteRoute(new DefensePoint3(6.25f,0f,0f),actualBase,1,buffer,out _),
            "New Ground spawn in the water domain is rejected");
        Check(rules.TryWriteRoute(new DefensePoint3(6.25f,1f,-0.5f),actualBase,1,buffer,out int resumed,true) && resumed==2 && buffer[0].X==3.8f,
            "Committed actor already on its bridge can resume toward West without teleporting to East entry");
        Check(!rules.TryWriteRoute(new DefensePoint3(6.25f,1f,6f),actualBase,1,buffer,out _,true),
            "Bridge resume cannot authorize arbitrary water coordinates");
        Check(!rules.TryWriteRoute(new DefensePoint3(25f,1f,0f),actualBase,5,buffer,out _) &&
            !rules.TryWriteRoute(new DefensePoint3(float.NaN,0f,0f),actualBase,1,buffer,out _),
            "Invalid lane and non-finite origins fail closed");
        Check(!rules.TryWriteRoute(new DefensePoint3(25f,1f,0f),actualBase,1,new DefensePoint3[2],out int insufficient) && insufficient==0,
            "Small route buffer cannot silently truncate bridge waypoints");
        Check(!rules.SameBank(new DefensePoint3(4f,0f,0f),new DefensePoint3(8.1f,0f,0f)) &&
            !rules.SameBank(new DefensePoint3(6f,0f,0f),new DefensePoint3(6.5f,0f,0f)),
            "Ground aggro never treats opposite banks or the water strip as shared ground");
        Check(rules.SameBank(new DefensePoint3(-10f,0f,0f),new DefensePoint3(4f,9f,0f)),
            "Same-bank classification is horizontal and independent of body height");

        var policy=new DefenseGroundAggroPolicy(rules);
        Check(policy.Select(0f,4.3f,true,true,false)==DefenseGroundFocus.Base,"Hero outside acquire range does not distract a marching enemy");
        Check(policy.Select(1f,4.1f,true,true,false)==DefenseGroundFocus.Hero,"Reachable nearby Hero acquires Ground aggro");
        Check(policy.Select(1.1f,5f,true,true,false)==DefenseGroundFocus.Hero,"Retain range prevents target flapping at the acquire boundary");
        Check(policy.Select(1.2f,8f,true,true,false)==DefenseGroundFocus.Hero,"Minimum target lock avoids immediate switchback");
        Check(policy.Select(2f,8f,true,true,false)==DefenseGroundFocus.Base,"Enemy returns to Base when Hero leaves retain range after lock");
        Check(policy.Select(3f,6f,true,true,true)==DefenseGroundFocus.Hero,"Recent Hero damage can provoke aggro within the extended range");
        Check(policy.Select(3.1f,2f,false,true,true)==DefenseGroundFocus.Base,"Down or inactive Hero releases aggro immediately despite lock");
        policy.Select(4f,2f,true,true,false);
        Check(policy.Select(4.1f,2f,true,false,false)==DefenseGroundFocus.Base,"Lost navigation or opposite-bank Hero cannot keep an aggro lock");
        Check(policy.Select(5f,float.NaN,true,true,false)==DefenseGroundFocus.Base,"Invalid target geometry cannot acquire aggro");
        bool invalid=false;
        try { new DefenseGroundNavigationRules(9f,8f,10f,6f,3f,-10f,0f,10f,4f,7f,1f,0.25f,3f); }
        catch (ArgumentOutOfRangeException) { invalid=true; }
        Check(invalid,"Navigation snapshot rejects inverted bank bounds");
        Console.WriteLine("TOTAL " + _checks + " PASS (standalone Ground route/aggro, no Unity runtime)");
    }
}
