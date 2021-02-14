namespace AhlSoft.BedrockServerWrapper.Backups
{
    using System;

    /// <summary>
    /// Information related to it being possible to run a backup.
    /// </summary>
    public class BackupReadyEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new <see cref="BackupReadyEventArgs"/>.
        /// </summary>
        /// <param name="backupArguments">Arguments for the backup to be performed.</param>
        public BackupReadyEventArgs(string backupArguments)
        {
            BackupArguments = backupArguments;
        }

        /// <summary>
        /// Gets the arguments associated to this backup.
        /// </summary>
        public string BackupArguments { get; }
    }
}