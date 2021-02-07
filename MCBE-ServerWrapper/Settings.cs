namespace AhlSoft.BedrockServerWrapper
{
    using System;
    using System.IO;

    using Newtonsoft.Json;

    /// <summary>
    /// Application settings.
    /// </summary>
    public class Settings
    {
        private const string SettingsFile = "mcbsw.conf";

        /// <summary>
        /// Whether or not automatic updates are enabled.
        /// </summary>
        public bool AutomaticUpdatesEnabled { get; set; }

        /// <summary>
        /// The number of minutes between automatic updates.
        /// </summary>
        public int AutomaticBackupFrequency { get; set; }

        /// <summary>
        /// Folder where PapyrusCs executables are put.
        /// </summary>
        public string PapyrusCsFolder { get; set; }

        /// <summary>
        /// Folder where map is saved.
        /// </summary>
        public string PapyrusOutputFolder { get; set; }

        /// <summary>
        /// Name of the folder where world (level) data is saved.
        /// </summary>
        public string LevelName { get; set; }

        /// <summary>
        /// Optional command that will be executed after map generation.
        /// </summary>
        public string PapyrusPostRunCommand { get; set; }

        /// <summary>
        /// Folder where backups are placed.
        /// </summary>
        public string BackupFolder { get; set; }

        /// <summary>
        /// File where log is saved.
        /// </summary>
        public string LogFilePath { get; set; }

        public string ServerFolder { get; set; }

        private Settings()
        {
            // Empty constructor to block public constructor.
        }
        
        /// <summary>
        /// Load settings from file, or create a new file.
        /// </summary>
        /// <returns>A Settings object.</returns>
        public static Settings Load()
        {
            if (File.Exists(SettingsFile))
            {
                Console.WriteLine($"Loading settings from \"{SettingsFile}\".");
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsFile));
            }

            var newSettings = new Settings();
            newSettings.Reset();
            newSettings.Save();

            return newSettings;
        }

        /// <summary>
        /// Saves the settings to file.
        /// </summary>
        public void Save()
        {
            File.WriteAllText(SettingsFile, JsonConvert.SerializeObject(this));
        }

        /// <summary>
        /// Reset all settings to default.
        /// </summary>
        public void Reset()
        {
            AutomaticUpdatesEnabled = false;
            AutomaticBackupFrequency = 12;

            ServerFolder = "Server";
            BackupFolder = "Backups";
            PapyrusCsFolder = "PapyrusCs";
            PapyrusOutputFolder = Path.Combine(PapyrusCsFolder, "GeneratedMap");
            LogFilePath = "mcbsw.log";
        }
    }
}