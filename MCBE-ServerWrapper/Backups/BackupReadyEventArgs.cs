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
        /// <param name="anyUserOnline">Whether there are any users currently online.</param>
        public BackupReadyEventArgs(string backupArguments, bool anyUserOnline)
        {
            BackupArguments = backupArguments;
            AnyUserOnline = anyUserOnline;
        }

        /// <summary>
        /// Gets the arguments associated to this backup.
        /// </summary>
        public string BackupArguments { get; }

        /// <summary>
        /// Gets a value indicating whether or not there are any users currently online.
        /// </summary>
        public bool AnyUserOnline { get; }
    }
}