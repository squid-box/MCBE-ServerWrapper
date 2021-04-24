namespace AhlSoft.BedrockServerWrapper.Settings
{
    using System;
    using System.IO;

    using Newtonsoft.Json;

    /// <inheritdoc cref="ISettingsProvider" />
    public class SettingsProvider : ISettingsProvider, IDisposable
    {
        private bool _automaticBackupEnabled;
        private int _automaticBackupFrequency;
        private string _papyrusFolder;
        private string _papyrusOutputFolder;
        private string _serverFolder;
        private string _backupFolder;
        private int _numberOfBackups;
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

            ISettingsProvider newSettings = new SettingsProvider();
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
                if (_automaticBackupFrequency != value && value > 0)
                {
                    _automaticBackupFrequency = value;
                    AutomaticBackupFrequencyChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <inheritdoc />
        public event EventHandler AutomaticBackupFrequencyChanged;

        /// <inheritdoc />
        public string BackupFolder
        {
            get => _backupFolder;
            set => _backupFolder = !string.IsNullOrWhiteSpace(value) ? value : "Backups";
        }

        /// <inheritdoc />
        public int NumberOfBackups
        {
            get => _numberOfBackups;

            set
            {
                if (value >= 0)
                {
                    _numberOfBackups = value;
                }
            }
        }

        /// <inheritdoc />
        public bool PapyrusEnabled { get; set; }

        /// <inheritdoc />
        public string LevelName { get; set; }

        /// <inheritdoc />
        public string PapyrusFolder
        {
            get => _papyrusFolder;
            set => _papyrusFolder = !string.IsNullOrWhiteSpace(value) ? value : "PapyrusCs";
        }

        /// <inheritdoc />
        public string PapyrusOutputFolder
        {
            get => _papyrusOutputFolder;
            set => _papyrusOutputFolder = !string.IsNullOrWhiteSpace(value) ? value : Path.Combine(PapyrusFolder, "GeneratedMap");
        }

        /// <inheritdoc />
        public string PapyrusPostRunCommand { get; set; }

        /// <inheritdoc />
        public string ServerFolder
        {
            get => _serverFolder;
            set => _serverFolder = !string.IsNullOrWhiteSpace(value) ? value : "Server";
        }

        /// <inheritdoc />
        public void Save()
        {
            File.WriteAllText(SettingsFile, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        /// <inheritdoc />
        public void Reset()
        {
            AutomaticBackupEnabled = true;
            AutomaticBackupFrequency = 60;

            NumberOfBackups = 7;

            ServerFolder = "Server";
            BackupFolder = "Backups";
            PapyrusEnabled = true;
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