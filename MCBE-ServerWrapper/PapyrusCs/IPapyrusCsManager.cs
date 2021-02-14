namespace AhlSoft.BedrockServerWrapper.PapyrusCs
{
    /// <summary>
    /// Manages integration with PapyrusCs.
    /// </summary>
    public interface IPapyrusCsManager
    {

        /// <summary>
        /// Generates map(s) for the world in the <paramref name="tempFolder"/> folder.
        /// </summary>
        /// <param name="tempFolder">Folder containing the world to generate map(s) from.</param>
        /// <remarks>Ensure the world is not loaded/locked when generating map.</remarks>
        public void GenerateMap(string tempFolder);
    }
}