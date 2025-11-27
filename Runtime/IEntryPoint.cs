namespace DGP.EntryPoints
{
    public interface IEntryPoint
    {
        /// <summary>
        /// The name to display in the editor dropdown
        /// </summary>
        string DisplayName { get; }
        
        /// <summary>
        /// Called when this configuration is selected in the editor
        /// </summary>
        void OnEntryPointSelected();

        /// <summary>
        /// Called to bootstrap the application with this configuration
        /// </summary>
        void Bootstrap();
    }
}