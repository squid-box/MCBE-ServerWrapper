namespace AhlSoft.BedrockServerWrapper.Backups
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;

    using AhlSoft.BedrockServerWrapper.PlayerManagement;

    /// <summary>
    /// Manages server backups.
    /// </summary>
    public class BackupManager
    {
        private readonly Settings _settings;
        private readonly PapyrusCsManager _papyrusCsManager;
        private readonly Log _log;

        private readonly string _serverRoot;

        private bool _hasUserBeenOnlineSinceLastBackup;

        /// <summary>
        /// Creates a new <see cref="BackupManager"/>.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="settings"></param>
        /// <param name="serverRoot"></param>
        public BackupManager(Log log, Settings settings, string serverRoot)
        {
            _log = log;
            _settings = settings;
            _papyrusCsManager = new PapyrusCsManager(_settings, log);
            _serverRoot = serverRoot;

            HasBackupBeenInitiated = false;
            _hasUserBeenOnlineSinceLastBackup = false;
        }

        /// <summary>
        /// Invoked whenever a backup has been completed.
        /// </summary>
        public event EventHandler<BackupCompletedEventArgs> BackupCompleted;

        /// <summary>
        /// Value indicating whether or not a backup has been initiated.
        /// </summary>
        public bool HasBackupBeenInitiated { get; set; }

        internal void PlayerJoined(object sender, PlayerConnectionEventArgs args)
        {
            _hasUserBeenOnlineSinceLastBackup = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        internal void RunScheduledBackup(string arguments)
        {
            if (_hasUserBeenOnlineSinceLastBackup)
            {
                Backup(arguments, false);
            }
            else
            {
                _log.Info("Skipped scheduled backup, no users have been online.");
            }
        }

        internal void ManualBackup(object sender, BackupReadyEventArgs eventArgs)
        {
            if (HasBackupBeenInitiated)
            {
                Backup(eventArgs.BackupArguments, true);
            }
        }

        private void Backup(string arguments, bool manual)
        {
            _log.Info($"Started {(manual ? "manual" : "scheduled")} backup.");
            var start = DateTime.Now;
            var tmpDir = Path.Combine(Path.GetTempPath(), "mcbesw_backup");
            
            if (Directory.Exists(tmpDir))
            {
	            Utils.DeleteDirectory(tmpDir, _log);
            }
            
            Directory.CreateDirectory(tmpDir);

            _log.Info("Copying files...");

            foreach (var file in arguments.Split(','))
            {
                try
                {
                    var fileTmp = file.Trim().Split(':');
                    var fileName = Path.Combine(_serverRoot, "worlds", fileTmp[0]);
                    var fileSize = Convert.ToInt32(fileTmp[1], CultureInfo.InvariantCulture);

                    _log.Info($" - Copying {fileName}...");
                    Utils.CopyFile(fileName, Path.Combine(tmpDir, fileName), fileSize);
                }
                catch (Exception e)
                {
                    _log.Error($"Backup failed: {e.GetType()}: {e.Message}");
                    Utils.DeleteDirectory(tmpDir, _log);
                    HasBackupBeenInitiated = false;

                    BackupCompleted?.Invoke(this, new BackupCompletedEventArgs(string.Empty, manual, TimeSpan.Zero, false));

                    return;
                }
            }

            _log.Info("Compressing backup...");
            var backupName = Path.Combine(_settings.BackupFolder, GetBackupFileName());
            ZipFile.CreateFromDirectory(tmpDir, backupName, CompressionLevel.Optimal, false);

            _papyrusCsManager.GenerateMap(tmpDir);

            BackupCompleted?.Invoke(this, new BackupCompletedEventArgs(backupName, manual, DateTime.Now - start));
            HasBackupBeenInitiated = false;
        }

        private static string GetBackupFileName()
        {
            var now = DateTime.Now;

            return $"backup_{now.Year:0000}{now.Month:00}{now.Day:00}-{now.Hour:00}{now.Minute:00}{now.Second:00}.zip";
        }
    }
}