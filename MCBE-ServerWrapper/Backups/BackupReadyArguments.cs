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
        public BackupReadyArguments(string backupInfo)
        {
            BackupArguments = backupInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        public string BackupArguments { get; }
    }
}
