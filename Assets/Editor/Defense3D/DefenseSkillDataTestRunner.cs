#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class DefenseSkillDataTestRunner
{
    private static TestRunnerApi api;
    public static void Run()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.InvalidOperationException("Requires Edit Mode.");
        for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new System.InvalidOperationException("Save scene edits first.");
        Directory.CreateDirectory("scratch");api=ScriptableObject.CreateInstance<TestRunnerApi>();api.RegisterCallbacks(new Results());
        api.Execute(new ExecutionSettings(new Filter{testMode=TestMode.EditMode,testNames=new[]{"DefenseSkillDataTests","DefenseSignatureInterruptionTests","DefenseDashMotorTests",
            "ProjectZombie.Tests.Editor.DefenseLoadoutCatalogAndAdapterTests.ApplyToHeroKit_AutoLoadsRelicWhenNull"}}){runSynchronously=true});
    }
    private sealed class Results:ICallbacks
    {
        public void RunStarted(ITestAdaptor testsToRun){}public void TestStarted(ITestAdaptor test){}public void TestFinished(ITestResultAdaptor result){}
        public void RunFinished(ITestResultAdaptor result)
        {File.WriteAllText("scratch/SkillDataTests.xml",result.ToXml().OuterXml);File.WriteAllText("scratch/SkillDataTests.txt",result.ResultState+"\nPassed: "+result.PassCount+"\nFailed: "+result.FailCount);api.UnregisterCallbacks(this);UnityEngine.Object.DestroyImmediate(api);api=null;}
    }
}
#endif
