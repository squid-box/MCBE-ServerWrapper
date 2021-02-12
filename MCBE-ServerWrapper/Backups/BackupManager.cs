namespace AhlSoft.BedrockServerWrapper.Backups
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Timers;
    using AhlSoft.BedrockServerWrapper.PlayerManagement;

    /// <summary>
    /// Manages server backups.
    /// </summary>
    public class BackupManager : IDisposable
    {
        private readonly Log _log;
        private readonly Settings _settings;
        private readonly PlayerManager _playerManager;
        private readonly PapyrusCsManager _papyrusCsManager;
        private readonly Timer _scheduledBackupTimer;

        private readonly string _serverRoot;

        private bool _hasUserBeenOnlineSinceLastBackup;

        /// <summary>
        /// Creates a new <see cref="BackupManager"/>.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="settings"></param>
        /// <param name="playerManager"></param>
        /// <param name="serverRoot"></param>
        public BackupManager(Log log, Settings settings, PlayerManager playerManager, string serverRoot)
        {
            _log = log;
            _settings = settings;
            _playerManager = playerManager;
            _papyrusCsManager = new PapyrusCsManager(_settings, log);
            _serverRoot = serverRoot;

            HasBackupBeenInitiated = false;
            _hasUserBeenOnlineSinceLastBackup = false;

            _playerManager.PlayerConnected += (_, __) => _hasUserBeenOnlineSinceLastBackup = true;

            if (_settings.AutomaticUpdatesEnabled)
            {
                _scheduledBackupTimer = new Timer(_settings.AutomaticBackupFrequency * 60 * 1000)
                {
                    AutoReset = true,
                    Enabled = true
                };

                _scheduledBackupTimer.Elapsed += (_, __) =>
                {
                    if (!_hasUserBeenOnlineSinceLastBackup)
                    {
                        _log.Info("Skipping scheduled backup since no user has been online since last time.");
                        return;
                    }

                    ScheduledBackup?.Invoke(this, EventArgs.Empty);
                };
            }
        }

        /// <summary>
        /// Invoked whenever a backup has been completed.
        /// </summary>
        public event EventHandler<BackupCompletedEventArgs> BackupCompleted;

        /// <summary>
        /// Invoked whenever a backup is scheduled to be performed.
        /// </summary>
        public event EventHandler ScheduledBackup;

        /// <summary>
        /// Value indicating whether or not a backup has been initiated.
        /// </summary>
        public bool HasBackupBeenInitiated { get; set; }

        /// <inheritdoc cref="IDisposable" />
        public void Dispose()
        {
            _scheduledBackupTimer?.Dispose();
            GC.SuppressFinalize(this);
        }

        internal void ManualBackup(object sender, BackupReadyEventArgs eventArgs)
        {
            if (HasBackupBeenInitiated)
            {
                Backup(eventArgs.BackupArguments);
            }
        }

        private void Backup(string arguments)
        {
            _log.Info($"Started backup.");
            var start = DateTime.Now;
            var tmpDir = Path.Combine(Path.GetTempPath(), "mcbesw_backup");
            
            if (Directory.Exists(tmpDir))
            {
	            Utils.DeleteDirectory(tmpDir, _log);
            }
            
            Directory.CreateDirectory(tmpDir);
            Directory.CreateDirectory(_settings.BackupFolder);

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

                    BackupCompleted?.Invoke(this, new BackupCompletedEventArgs(string.Empty, TimeSpan.Zero, false));

                    return;
                }
            }

            _log.Info("Compressing backup...");
            var backupName = Path.Combine(_settings.BackupFolder, GetBackupFileName());
            ZipFile.CreateFromDirectory(tmpDir, backupName, CompressionLevel.Optimal, false);

            _papyrusCsManager.GenerateMap(tmpDir);

            BackupCompleted?.Invoke(this, new BackupCompletedEventArgs(backupName, DateTime.Now - start));
            HasBackupBeenInitiated = false;
            _hasUserBeenOnlineSinceLastBackup = _playerManager.UsersOnline != 0;
        }

        private static string GetBackupFileName()
        {
            var now = DateTime.Now;

            return $"backup_{now.Year:0000}{now.Month:00}{now.Day:00}-{now.Hour:00}{now.Minute:00}{now.Second:00}.zip";
        }
    }
}