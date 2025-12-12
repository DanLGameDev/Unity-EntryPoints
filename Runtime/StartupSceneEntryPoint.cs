using UnityEditor;
using UnityEngine;

namespace DGP.EntryPoints
{
    [CreateAssetMenu(fileName = "StartupSceneLauncher", menuName = "DGP/Startup Scene Launcher")]
    public class StartupSceneEntryPoint : ScriptableObject, IEntryPoint
    {
        [SerializeField] private SceneAsset startupScene;

        public string DisplayName => name;

        public string StartupScenePath
        {
            get {
#if UNITY_EDITOR
                return startupScene != null ? AssetDatabase.GetAssetPath(startupScene) : string.Empty;
#else
                return string.Empty;
#endif
            }
        }

        public virtual void OnEntryPointSelected()
        {
#if UNITY_EDITOR
            if (startupScene == null) {
                Debug.LogWarning($"[{name}] No startup scene assigned.");
                return;
            }
    
            UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene = startupScene;
            Debug.Log($"[{name}] Set playModeStartScene to: {startupScene.name}"); // Add this
#endif
        }

        public virtual void Bootstrap()
        {
            // noop
        }
    }
}