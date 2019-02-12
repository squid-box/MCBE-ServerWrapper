namespace BedrockServerWrapper.Backups
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public class BackupReadyArguments : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="backupInfo"></param>
        /// <param name="anyUserOnline"></param>
        public BackupReadyArguments(string backupInfo, bool anyUserOnline)
        {
            BackupArguments = backupInfo;
            AnyUserOnline = anyUserOnline;
        }

        /// <summary>
        /// 
        /// </summary>
        public string BackupArguments { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool AnyUserOnline { get; }
    }
}
