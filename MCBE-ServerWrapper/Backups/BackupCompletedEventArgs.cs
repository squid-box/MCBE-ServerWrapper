namespace BedrockServerWrapper.Backups
{
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
        /// <param name="manualBackup">Whether this was a manual backup or not.</param>
        /// <param name="backupDuration">The time it took to perform the backup.</param>
        public BackupCompletedEventArgs(string backupFile, bool manualBackup, TimeSpan backupDuration)
        {
            BackupFile = backupFile;
            ManualBackup = manualBackup;
            BackupDuration = backupDuration;
        }

        /// <summary>
        /// Gets a value indicating whether or not this was a manual backup.
        /// </summary>
        public bool ManualBackup { get; }

        /// <summary>
        /// Gets the time it took to perform the backup.
        /// </summary>
        public TimeSpan BackupDuration { get; }

        /// <summary>
        /// Gets the path to the backup file.
        /// </summary>
        public string BackupFile { get; }
    }
}