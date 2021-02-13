namespace AhlSoft.BedrockServerWrapper
{
    using System;
    using System.IO;

    using Newtonsoft.Json;

    /// <inheritdoc cref="ISettingsProvider" />
    public class SettingsProvider : ISettingsProvider, IDisposable
    {
        private bool _automaticBackupEnabled;
        private int _automaticBackupFrequency;
        private const string SettingsFile = "mcbsw.conf";

        private SettingsProvider()
        {
            // Empty constructor to block public constructor.
        }

        /// <summary>
        /// Load settings from file, or create a new file.
        /// </summary>
        /// <returns>A SettingsProvider object.</returns>
        public static ISettingsProvider Load()
        {
            if (File.Exists(SettingsFile))
            {
                return JsonConvert.DeserializeObject<SettingsProvider>(File.ReadAllText(SettingsFile));
            }

            var newSettings = new SettingsProvider();
            newSettings.Reset();
            newSettings.Save();

            return newSettings;
        }

        /// <inheritdoc />
        public bool AutomaticBackupEnabled
        {
            get => _automaticBackupEnabled;

            set
            {
                if (_automaticBackupEnabled != value)
                {
                    _automaticBackupEnabled = value;
                    AutomaticBackupEnabledChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <inheritdoc />
        public event EventHandler AutomaticBackupEnabledChanged;

        /// <inheritdoc />
        public int AutomaticBackupFrequency
        {
            get => _automaticBackupFrequency;
            
            set
            {
                if (_automaticBackupFrequency != value)
                {
                    _automaticBackupFrequency = value;
                    AutomaticBackupFrequencyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <inheritdoc />
        public event EventHandler AutomaticBackupFrequencyChanged;

        /// <inheritdoc />
        public string BackupFolder { get; set; }

        /// <inheritdoc />
        public string LevelName { get; set; }

        /// <inheritdoc />
        public string PapyrusFolder { get; set; }

        /// <inheritdoc />
        public string PapyrusOutputFolder { get; set; }

        /// <inheritdoc />
        public string PapyrusPostRunCommand { get; set; }

        /// <inheritdoc />
        public string ServerFolder { get; set; }

        /// <inheritdoc />
        public void Save()
        {
            File.WriteAllText(SettingsFile, JsonConvert.SerializeObject(this));
        }

        /// <inheritdoc />
        public void Reset()
        {
            AutomaticBackupEnabled = true;
            AutomaticBackupFrequency = 60;

            ServerFolder = "Server";
            BackupFolder = "Backups";
            PapyrusFolder = "PapyrusCs";
            PapyrusOutputFolder = Path.Combine(PapyrusFolder, "GeneratedMap");
        }

        /// <inheritdoc cref="IDisposable" />
        public void Dispose()
        {
            Save();
            GC.SuppressFinalize(this);
        }
    }
}