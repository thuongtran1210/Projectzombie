#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectZombie.Core.Architecture;

/// <summary>Allows direct Play from a content scene without adding runtime fallback routes to player builds.</summary>
public static class AppBootstrapEditorSupport
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureEditorBootstrap()
    {
        if (AppCompositionRoot.Instance != null || Object.FindObjectOfType<AppCompositionRoot>(true) != null) return;
        const string PATH = "Assets/Core/Architecture/Assets/AppBootstrap.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PATH);
        if (prefab == null)
        {
            Debug.LogError("Install App Bootstrap before entering Play Mode.");
            return;
        }
        var content = SceneManager.GetActiveScene();
        var bootstrap = SceneManager.CreateScene(AppCompositionRoot.BOOTSTRAP_SCENE);
        // Move an inactive instance before Awake so ownership is established in Bootstrap.
        var staging = new GameObject("Bootstrap Staging");
        staging.SetActive(false);
        SceneManager.MoveGameObjectToScene(staging, bootstrap);
        var instance = Object.Instantiate(prefab, staging.transform);
        instance.transform.SetParent(null);
        instance.SetActive(true);
        Object.Destroy(staging);
        if (content.IsValid() && content.isLoaded)
        {
            SceneManager.SetActiveScene(content);
        }
    }
}
#endif
