#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DefenseMapCollisionValidation
{
    private static TestRunnerApi api;
    [MenuItem("Tools/VFX Generator/Validate Map Collision")]
    public static void Validate()
    {
        if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Requires Edit Mode.");
        for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new System.InvalidOperationException("Save scene edits before native test execution.");
        Directory.CreateDirectory("scratch");File.WriteAllText("scratch/MapCollisionValidation.txt","Running...");
        api=ScriptableObject.CreateInstance<TestRunnerApi>();api.RegisterCallbacks(new Results());
        api.Execute(new ExecutionSettings(new Filter{testMode=TestMode.EditMode,testNames=new[]{"DefenseHeroGroundSafetyTests","DefenseDashMotorTests","DefenseDashFeedbackTests","Migration3DTraversalAndTargetingTests"}}){runSynchronously=true});
    }
    private sealed class Results:ICallbacks
    {
        public void RunStarted(ITestAdaptor testsToRun){}
        public void TestStarted(ITestAdaptor test){}
        public void TestFinished(ITestResultAdaptor result){}
        public void RunFinished(ITestResultAdaptor result)
        {File.WriteAllText("scratch/MapCollisionValidation.xml",result.ToXml().OuterXml);File.WriteAllText("scratch/MapCollisionValidation.txt",result.ResultState+"\nPassed: "+result.PassCount+"\nFailed: "+result.FailCount);api.UnregisterCallbacks(this);UnityEngine.Object.DestroyImmediate(api);api=null;}
    }
}
#endif

