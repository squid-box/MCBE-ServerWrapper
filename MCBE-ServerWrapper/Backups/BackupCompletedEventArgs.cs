namespace AhlSoft.BedrockServerWrapper.Backups;

using System;

/// <summary>
/// Represents information related to a completed backup.
/// </summary>
public class BackupCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Creates a new <see cref="BackupCompletedEventArgs"/>.
    /// </summary>
    /// <param name="backupFile">Path to the file of the backup.</param>
    /// <param name="backupDuration">The time it took to perform the backup.</param>
    /// <param name="successful">Whether this backup was successful or not.</param>
    public BackupCompletedEventArgs(string backupFile, TimeSpan backupDuration, bool successful = true)
    {
        BackupFile = backupFile;
        BackupDuration = backupDuration;
        Successful = successful;
    }

    /// <summary>
    /// Gets the time it took to perform the backup.
    /// </summary>
    public TimeSpan BackupDuration { get; }

    /// <summary>
    /// Gets the path to the backup file.
    /// </summary>
    public string BackupFile { get; }

    /// <summary>
    /// Gets a value indicating whether or not the backup was successful.
    /// </summary>
    public bool Successful { get; }
}
