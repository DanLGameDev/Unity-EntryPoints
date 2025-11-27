namespace DGP.EntryPoints
{
    /// <summary>
    /// Provides access to the currently active launch configuration
    /// </summary>
    public static class EntryPoints
    {
        private static IEntryPoint activeEntryPoint;
        
        /// <summary>
        /// Gets the currently active launch configuration
        /// </summary>
        public static IEntryPoint ActiveEntryPoint
        {
            get => activeEntryPoint;
            set => activeEntryPoint = value;
        }
    }
}