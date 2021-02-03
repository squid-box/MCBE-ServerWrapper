﻿namespace BedrockServerWrapper.Backups
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
        private readonly Settings _settings;
        private readonly PapyrusCsController _papyrusCsController;
        private bool _hasUserBeenOnlineSinceLastBackup;

        /// <summary>
        /// Creates a new <see cref="BackupManager"/>.
        /// </summary>
        public BackupManager(Settings settings, PapyrusCsController papyrusCsController)
        {
            _settings = settings;
            _papyrusCsController = papyrusCsController;

            HasBackupBeenInitiated = false;
            _hasUserBeenOnlineSinceLastBackup = false;
        }

        /// <summary>
        /// Invoked whenever a backup has been completed.
        /// </summary>
        public event EventHandler<BackupCompletedEventArgs> BackupCompleted;

        /// <summary>
        /// Value indicating whether or not a backup has been initiated.
        /// </summary>
        public bool HasBackupBeenInitiated { get; set; }

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
            if (HasBackupBeenInitiated)
            {
                Backup(eventArgs.BackupArguments, true);
            }
        }

        private void Backup(string arguments, bool manual)
        {
            var start = DateTime.Now;
            var tmpDir = Path.Combine(_settings.BackupFolder, "tmp");
            
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
            var backupName = Path.Combine(_settings.BackupFolder, GetBackupFileName());
            ZipFile.CreateFromDirectory(tmpDir, backupName, CompressionLevel.Optimal, false);

            // TODO: Ugly hardcoded, need to dynamically find this.
            _papyrusCsController.GenerateMap(Path.Combine(tmpDir, "worlds", "world"));

            Utils.DeleteDirectory(tmpDir);
            
            BackupCompleted?.Invoke(this, new BackupCompletedEventArgs(backupName, manual, DateTime.Now - start));
            HasBackupBeenInitiated = false;
        }

        private static string GetBackupFileName()
        {
            var now = DateTime.Now;

            return $"backup_{now.Year:0000}{now.Month:00}{now.Day:00}-{now.Hour:00}{now.Minute:00}{now.Second:00}.zip";
        }
    }
}