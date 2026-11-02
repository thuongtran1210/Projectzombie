#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DefenseSoulPickupValidation
{
    private static TestRunnerApi api;
    public static void Validate()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.InvalidOperationException("Requires Edit Mode.");
        for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new System.InvalidOperationException("Save scene edits first.");
        Directory.CreateDirectory("scratch");File.WriteAllText("scratch/SoulPickupValidation.txt","Running...");
        api=ScriptableObject.CreateInstance<TestRunnerApi>();api.RegisterCallbacks(new Results());
        api.Execute(new ExecutionSettings(new Filter{testMode=TestMode.EditMode,testNames=new[]{"DefenseSoulPickupTests","DefensePoolLifecycleTests",
            "ProjectZombie.Tests.Editor.DefenseWoodDrop3DTests.SpawnWithoutCollector_StillCreatesVisiblePrefab",
            "ProjectZombie.Tests.Editor.DefenseWoodDrop3DTests.WoodDropPool_CanAcquireReleaseAndReuseInstance"}}){runSynchronously=true});
    }
    private sealed class Results:ICallbacks
    {
        public void RunStarted(ITestAdaptor testsToRun){}public void TestStarted(ITestAdaptor test){}public void TestFinished(ITestResultAdaptor result){}
        public void RunFinished(ITestResultAdaptor result)
        {File.WriteAllText("scratch/SoulPickupValidation.xml",result.ToXml().OuterXml);File.WriteAllText("scratch/SoulPickupValidation.txt",result.ResultState+"\nPassed: "+result.PassCount+"\nFailed: "+result.FailCount);api.UnregisterCallbacks(this);UnityEngine.Object.DestroyImmediate(api);api=null;}
    }
}
#endif
