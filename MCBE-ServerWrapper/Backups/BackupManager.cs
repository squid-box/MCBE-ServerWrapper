namespace BedrockServerWrapper.Backups
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;

    using PlayerManagement;

    /// <summary>
    /// Manages server backups.
    /// </summary>
    public class BackupManager
    {
        private readonly Settings _settings;
        private readonly PapyrusCsController _papyrusCsController;
        private readonly Log _log;

        private bool _hasUserBeenOnlineSinceLastBackup;

        /// <summary>
        /// Creates a new <see cref="BackupManager"/>.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="settings"></param>
        public BackupManager(Log log, Settings settings)
        {
            _log = log;
            _settings = settings;
            _papyrusCsController = new PapyrusCsController(_settings, log);

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
            var tmpDir = Path.Combine(_settings.BackupFolder, "tmp");
            
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
                    var fileName = Path.Combine("worlds", fileTmp[0]);
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

            _papyrusCsController.GenerateMap(Path.Combine(tmpDir, "worlds", _settings.LevelName));

            Utils.DeleteDirectory(tmpDir, _log);
            
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