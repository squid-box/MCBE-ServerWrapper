namespace AhlSoft.BedrockServerWrapper
{
    using System;

    /// <summary>
    /// Application settings provider.
    /// </summary>
    public interface ISettingsProvider
    {
        /// <summary>
        /// Whether or not automatic updates are enabled.
        /// </summary>
        public bool AutomaticBackupEnabled { get; set; }

        /// <summary>
        /// Invoked whenever <see cref="AutomaticBackupEnabled"/> is changed.
        /// </summary>
        public event EventHandler AutomaticBackupEnabledChanged;

        /// <summary>
        /// The number of minutes between automatic backups.
        /// </summary>
        public int AutomaticBackupFrequency { get; set; }

        /// <summary>
        /// Invoked whenever <see cref="AutomaticBackupFrequency"/> is changed.
        /// </summary>
        public event EventHandler AutomaticBackupFrequencyChanged;

        /// <summary>
        /// Folder where backups are placed.
        /// </summary>
        public string BackupFolder { get; set; }

        /// <summary>
        /// Folder where PapyrusCs executables are put.
        /// </summary>
        public string PapyrusFolder { get; set; }

        /// <summary>
        /// Folder where map is saved.
        /// </summary>
        public string PapyrusOutputFolder { get; set; }

        /// <summary>
        /// Optional command that will be executed after map generation.
        /// </summary>
        public string PapyrusPostRunCommand { get; set; }

        /// <summary>
        /// Name of the folder where world (level) data is saved.
        /// </summary>
        public string LevelName { get; set; }

        /// <summary>
        /// Name of the folder where server files are stored.
        /// </summary>
        public string ServerFolder { get; set; }

        /// <summary>
        /// Saves the settings to file.
        /// </summary>
        public void Save();

        /// <summary>
        /// Reset all settings to default.
        /// </summary>
        public void Reset();
    }
}