#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
public sealed class DefenseSkillBuildValidation : IPreprocessBuildWithReport
{
    public int callbackOrder=>-100;
    public void OnPreprocessBuild(BuildReport report)
    {
        try{DefenseSkillDataMigration.Validate();}
        catch(System.Exception error){throw new BuildFailedException("Invalid skill data: "+error.Message);}
    }
}
#endif
