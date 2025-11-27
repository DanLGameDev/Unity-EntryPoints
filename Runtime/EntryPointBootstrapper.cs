using UnityEngine;

namespace DGP.EntryPoints
{
    /// <summary>
    /// Abstract bootstrapper that provides access to the selected entry point configuration.
    /// Inherit from this class, implement GetDefaultConfiguration() and Bootstrap(), then add 
    /// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] to a static
    /// method that creates an instance and calls Bootstrap().
    /// </summary>
    public abstract class EntryPointBootstrapper
    {
        /// <summary>
        /// Gets the active entry point configuration.
        /// In editor: returns the selected entry point from the toolbar.
        /// In builds: returns the default configuration.
        /// </summary>
        protected IEntryPoint GetActiveConfiguration()
        {
#if UNITY_EDITOR
            var activeConfig = EntryPoints.ActiveEntryPoint;
            if (activeConfig != null)
                return activeConfig;
            
            Debug.LogWarning("[EntryPointBootstrapper] No entry point selected in editor, falling back to default.");
#endif
            return GetDefaultConfiguration();
        }

        /// <summary>
        /// Implement this to return your default configuration for builds.
        /// This is typically loaded from Resources.
        /// </summary>
        /// <returns>The default entry point configuration</returns>
        protected abstract IEntryPoint GetDefaultConfiguration();

        /// <summary>
        /// Implement this to bootstrap your game with the active configuration.
        /// </summary>
        public abstract void Bootstrap();
    }
}