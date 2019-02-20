namespace BedrockServerWrapper.Backups
{
    using System;

    public class BackupCompletedArguments : EventArgs
    {
        public BackupCompletedArguments(string backupFile, bool manualBackup, TimeSpan backupDuration)
        {
            BackupFile = backupFile;
            ManualBackup = manualBackup;
            BackupDuration = backupDuration;
        }

        public bool ManualBackup { get; }

        public TimeSpan BackupDuration { get; }

        public string BackupFile { get; }
    }
}
