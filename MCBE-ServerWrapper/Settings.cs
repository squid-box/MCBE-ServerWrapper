namespace BedrockServerWrapper
{
    using System;
    using System.IO;
    using Newtonsoft.Json;

    public class Settings
    {
        private const string SettingsFile = "mcbe.conf";

        public bool AutomaticUpdatesEnabled { get; set; }

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
        /// Optional command that will be executed after map generation.
        /// </summary>
        public string PapyrusPostRunCommand { get; set; }

        public string BackupFolder { get; set; }

        private Settings()
        {
            // Empty constructor to block public constructor.
        }

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
        /// 
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

            BackupFolder = "Backups";
            PapyrusCsFolder = "PapyrusCs";
            PapyrusOutputFolder = Path.Combine(PapyrusCsFolder, "GeneratedMap");
        }
    }
}