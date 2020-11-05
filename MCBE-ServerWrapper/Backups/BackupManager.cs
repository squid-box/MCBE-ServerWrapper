namespace BedrockServerWrapper.Backups
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;

    using PlayerManagement;

    /// <summary>
    /// Manages server backups.
    /// </summary>
    public class BackupManager
    {
        private bool _hasUserBeenOnlineSinceLastBackup;

        /// <summary>
        /// Creates a new <see cref="BackupManager"/>.
        /// </summary>
        public BackupManager()
        {
            _hasUserBeenOnlineSinceLastBackup = false;
        }

        /// <summary>
        /// Invoked whenever a backup has been completed.
        /// </summary>
        public event EventHandler<BackupCompletedEventArgs> BackupCompleted;

        /// <summary>
        /// Gets the path to the backup folder.
        /// </summary>
        public static string BackupFolder => @"Backups";

        internal void PlayerJoined(object sender, PlayerConnectionEventArgs args)
        {
            _hasUserBeenOnlineSinceLastBackup = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        internal void RunScheduledBackup(string arguments)
        {
            if (_hasUserBeenOnlineSinceLastBackup)
            {
                Backup(arguments, false);
            }
            else
            {
                Console.Out.WriteLine("Skipped scheduled backup, no users have been online.");
            }
        }

        internal void ManualBackup(object sender, BackupReadyEventArgs eventArgs)
        {
            Backup(eventArgs.BackupArguments, true);
        }

        private void Backup(string arguments, bool manual)
        {
            var start = DateTime.Now;
            var tmpDir = Path.Combine(BackupFolder, "tmp");
            if (Directory.Exists(tmpDir))
            {
	            Utils.DeleteDirectory(tmpDir);
            }
            Directory.CreateDirectory(tmpDir);

            Console.Out.WriteLine("Copying files...");

            foreach (var file in arguments.Split(','))
            {
                var fileTmp = file.Trim().Split(':');
                var fileName = Path.Combine("worlds", fileTmp[0]);
                var fileSize = Convert.ToInt32(fileTmp[1], CultureInfo.InvariantCulture);

                Console.Out.WriteLine($" - Copying {fileName}...");
                Utils.CopyFile(fileName, Path.Combine(tmpDir, fileName), fileSize);
            }

            Console.Out.WriteLine("Compressing backup...");
            var backupName = Path.Combine(BackupFolder, GetBackupFileName());
            ZipFile.CreateFromDirectory(tmpDir, backupName, CompressionLevel.Optimal, false);

            Utils.DeleteDirectory(tmpDir);
            
            BackupCompleted?.Invoke(this, new BackupCompletedEventArgs(backupName, manual, DateTime.Now - start));
        }

        private static string GetBackupFileName()
        {
            var now = DateTime.Now;

            return $"backup_{now.Year:0000}{now.Month:00}{now.Day:00}-{now.Hour:00}{now.Minute:00}{now.Second:00}.zip";
        }
    }
}