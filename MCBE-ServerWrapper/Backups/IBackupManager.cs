namespace AhlSoft.BedrockServerWrapper.Backups;

using System;

/// <summary>
/// Manages server backups.
/// </summary>
public interface IBackupManager
{
    /// <summary>
    /// Invoked whenever a backup has been completed.
    /// </summary>
    public event EventHandler<BackupCompletedEventArgs> BackupCompleted;

    /// <summary>
    /// Invoked whenever a backup is scheduled to be performed.
    /// </summary>
    public event EventHandler ScheduledBackup;

    /// <summary>
    /// Gets or sets a value indicating whether or not a backup has been initiated.
    /// </summary>
    public bool HasBackupBeenInitiated { get; set; }

    /// <summary>
    /// Performs a backup, if the backup is ready.
    /// </summary>
    /// <param name="backupArguments">Arguments for the backup to be performed.</param>
    public void Backup(string backupArguments);
}
