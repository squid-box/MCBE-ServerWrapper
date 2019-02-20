namespace BedrockServerWrapper.Backups
{
    using System;
    using System.IO;
    using System.IO.Compression;

    using PlayerManagement;

    public class BackupManager
    {
        private bool _hasUserBeenOnlineSinceLastBackup;

        public BackupManager()
        {
            _hasUserBeenOnlineSinceLastBackup = false;
        }

        public event EventHandler<BackupCompletedArguments> BackupCompleted;

        public static string BackupFolder => @"Backups";

        public void PlayerJoined(object sender, PlayerConnectionEventArgs args)
        {
            _hasUserBeenOnlineSinceLastBackup = true;
        }

        public void RunScheduledBackup(string arguments)
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

        public void ManualBackup(object sender, BackupReadyArguments arguments)
        {
            Backup(arguments.BackupArguments, true);
        }

        private void Backup(string arguments, bool manual)
        {
            var start = DateTime.Now;
            var tmpDir = Path.Combine(BackupFolder, "tmp");
            if (Directory.Exists(tmpDir))
            {
                DeleteDirectory(tmpDir);
            }
            Directory.CreateDirectory(tmpDir);

            Console.Out.WriteLine("Copying files...");

            foreach (var file in arguments.Split(','))
            {
                var fileTmp = file.Trim().Split(':');
                var fileName = Path.Combine("worlds", fileTmp[0]);
                var fileSize = Convert.ToInt32(fileTmp[1]);

                Console.Out.WriteLine($" - Copying {fileName}...");
                CopyFile(fileName, Path.Combine(tmpDir, fileName), fileSize);
            }

            Console.Out.WriteLine("Compressing backup...");
            var backupName = Path.Combine(BackupFolder, GetBackupFileName());
            ZipFile.CreateFromDirectory(tmpDir, backupName, CompressionLevel.Optimal, false);

            DeleteDirectory(tmpDir);
            
            BackupCompleted?.Invoke(this, new BackupCompletedArguments(backupName, manual, DateTime.Now - start));
        }

        private static bool DeleteDirectory(string dir)
        {
            Console.Out.WriteLine("Removing temporary files.");
            try
            {
                Directory.Delete(dir, true);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't delete temporary files/folder: {e.GetType()} - {e.Message}");
                return false;
            }
        }

        private static void CopyFile(string source, string destination, int bytesToRead)
        {
            var buffer = new byte[bytesToRead];

            using (var reader = new BinaryReader(new FileStream(source, FileMode.Open)))
            {
                reader.Read(buffer, 0, bytesToRead);
            }

            File.WriteAllBytes(destination, buffer);
        }

        private static string GetBackupFileName()
        {
            var now = DateTime.Now;

            return $"backup_{now.Year:0000}{now.Month:00}{now.Day:00}-{now.Hour:00}{now.Minute:00}{now.Second:00}.zip";
        }
    }
}
