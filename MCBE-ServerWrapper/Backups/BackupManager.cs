namespace AhlSoft.BedrockServerWrapper.Backups;

using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Timers;

using AhlSoft.BedrockServerWrapper.Logging;
using AhlSoft.BedrockServerWrapper.PapyrusCs;
using AhlSoft.BedrockServerWrapper.PlayerManagement;
using AhlSoft.BedrockServerWrapper.Settings;

/// <inheritdoc cref="IBackupManager" />
public class BackupManager : IBackupManager, IDisposable
{
    private readonly ILog _log;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IPlayerManager _playerManager;
    private readonly IPapyrusCsManager _papyrusCsManager;

    private Timer _scheduledBackupTimer;
    private bool _hasUserBeenOnlineSinceLastBackup;

    /// <summary>
    /// Creates a new <see cref="BackupManager"/>.
    /// </summary>
    /// <param name="log">Logger to use.</param>
    /// <param name="settingsProvider">SettingsProvider to use.</param>
    /// <param name="playerManager">PlayerManager to use.</param>
    /// <param name="papyrusCsManager">PapyrusCsManager to use.</param>
    public BackupManager(ILog log, ISettingsProvider settingsProvider, IPlayerManager playerManager, IPapyrusCsManager papyrusCsManager)
    {
        _log = log;
        _settingsProvider = settingsProvider;
        _playerManager = playerManager;
        _papyrusCsManager = papyrusCsManager;

        HasBackupBeenInitiated = false;
        _hasUserBeenOnlineSinceLastBackup = false;

        _playerManager.PlayerConnected += (_, __) => _hasUserBeenOnlineSinceLastBackup = true;

        BuildScheduledBackupTimer();

        _settingsProvider.AutomaticBackupEnabledChanged += (_, __) => BuildScheduledBackupTimer();
        _settingsProvider.AutomaticBackupFrequencyChanged += (_, __) => BuildScheduledBackupTimer();
    }

    /// <inheritdoc />
    public event EventHandler<BackupCompletedEventArgs> BackupCompleted;

    /// <inheritdoc />
    public event EventHandler ScheduledBackup;

    /// <inheritdoc />
    public bool HasBackupBeenInitiated { get; set; }

    /// <inheritdoc cref="IDisposable" />
    public void Dispose()
    {
        _scheduledBackupTimer?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void Backup(string backupArguments)
    {
        if (!HasBackupBeenInitiated)
        {
            _log.Warning("Backup hasn't been initiated, aborting.");
            return;
        }

        _log.Info("Started backup.");
        var start = DateTime.Now;
        var tmpDir = Path.Combine(Path.GetTempPath(), "mcbesw_backup");
        
        if (Directory.Exists(tmpDir))
        {
	            Utils.DeleteDirectory(tmpDir, _log);
        }
        
        Directory.CreateDirectory(tmpDir);
        Directory.CreateDirectory(_settingsProvider.BackupFolder);

        _log.Info("Copying files...");

        foreach (var file in backupArguments.Split(','))
        {
            try
            {
                var fileTmp = file.Trim().Split(':');
                var fileName = Path.Combine(_settingsProvider.ServerFolder, "worlds", fileTmp[0]);
                var fileSize = Convert.ToInt32(fileTmp[1], CultureInfo.InvariantCulture);

                _log.Info($" - Copying {fileName}...");
                Utils.CopyFile(_log, fileName, Path.Combine(tmpDir, fileName), fileSize);
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
        var backupName = Path.Combine(_settingsProvider.BackupFolder, GenerateBackupFileName());
        ZipFile.CreateFromDirectory(tmpDir, backupName, CompressionLevel.Optimal, false);

        _papyrusCsManager.GenerateMap(tmpDir);

        CleanOldBackups();

        _log.Info($"Backup saved to \"{backupName}\".");
        BackupCompleted?.Invoke(this, new BackupCompletedEventArgs(backupName, DateTime.Now - start));
        HasBackupBeenInitiated = false;
        _hasUserBeenOnlineSinceLastBackup = _playerManager.UsersOnline != 0;
    }

    private void ScheduledBackupTimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        if (!_hasUserBeenOnlineSinceLastBackup)
        {
            // Skipping scheduled backup, no user has been online since last time.
            return;
        }

        ScheduledBackup?.Invoke(this, EventArgs.Empty);
    }

    private void BuildScheduledBackupTimer()
    {
        if (_settingsProvider.AutomaticBackupEnabled)
        {
            _log.Info($"Automatic backups are enabled, running every {_settingsProvider.AutomaticBackupFrequency} minute(s).");

            if (_scheduledBackupTimer != null)
            {
                _scheduledBackupTimer.Elapsed -= ScheduledBackupTimerOnElapsed;
                _scheduledBackupTimer.Dispose();
            }

            _scheduledBackupTimer = new Timer(_settingsProvider.AutomaticBackupFrequency * 60 * 1000)
            {
                AutoReset = true,
                Enabled = true
            };

            _scheduledBackupTimer.Elapsed += ScheduledBackupTimerOnElapsed;
        }
        else
        {
            _log.Info("Automatic backups are disabled.");

            if (_scheduledBackupTimer != null)
            {
                _scheduledBackupTimer.Elapsed -= ScheduledBackupTimerOnElapsed;
                _scheduledBackupTimer.Dispose();
            }
        }
    }

    private void CleanOldBackups()
    {
        if (_settingsProvider.NumberOfBackups == 0)
        {
            // Skip cleanup if setting is "disabled".
            return;
        }

        var filesToDelete = new DirectoryInfo(_settingsProvider.BackupFolder)
            .GetFiles("backup_*.zip")
            .OrderByDescending(f => f.LastWriteTime)
            .Select(f => Path.Combine(_settingsProvider.BackupFolder, f.Name))
            .Skip(_settingsProvider.NumberOfBackups + 1);

        foreach (var file in filesToDelete)
        {
            try
            {
                _log.Info($"Deleting backup file \"{file}\".");
                File.Delete(file);
            }
            catch (Exception e)
            {
                _log.Warning($"Could not delete file: {e.GetType()} - {e.Message}");
            }
            
        }
    }

    private static string GenerateBackupFileName()
    {
        var now = DateTime.Now;

        return $"backup_{now.Year:0000}{now.Month:00}{now.Day:00}-{now.Hour:00}{now.Minute:00}{now.Second:00}.zip";
    }
}
