namespace BedrockServerWrapper.Backups
{
    using System;
    using System.IO;

    using PlayerManagement;

    public class BackupManager
    {
        private readonly ServerProcess _serverProcess;

        private bool _hasUserBeenOnlineSinceLastBackup;

        public BackupManager(ServerProcess serverProcess)
        {
            _serverProcess = serverProcess;
            _hasUserBeenOnlineSinceLastBackup = false;
        }

        public static string BackupFolder => @"Backups";

        public void PlayerJoined(object sender, PlayerConnectionEventArgs args)
        {
            _hasUserBeenOnlineSinceLastBackup = true;
        }

        public void RunScheduledBackup(string arguments)
        {
            if (_hasUserBeenOnlineSinceLastBackup)
            {
                Backup(arguments);
            }
            else
            {
                Console.Out.WriteLine("Skipped scheduled backup, no users have been online.");
            }
        }

        public void ManualBackup(object sender, BackupReadyArguments arguments)
        {
            Backup(arguments.BackupArguments);
        }

        private void Backup(string arguments)
        {
            var tmpFolder = Path.Combine(BackupFolder, "tmp");
            Directory.CreateDirectory(tmpFolder);

            Console.Out.WriteLine("Copying files...");

            foreach (var file in arguments.Split(','))
            {
                var fileTmp = file.Trim().Split(':');
                var fileName = Path.Combine("worlds", fileTmp[0]);
                var fileSize = Convert.ToInt32(fileTmp[1]);

                Console.Out.WriteLine($" - Copying {fileName}...");
                CopyFile(fileName, Path.Combine(tmpFolder, Path.GetFileName(fileName)), fileSize);
            }

            Console.Out.WriteLine("Compressing backup...");
            // TODO: Compress backups, and organize them.
            //Directory.Move(tmpFolder, "Backup");

            Console.Out.WriteLine("Removing temporary files.");

            // Tell Server to resume saving (backup complete).
            _serverProcess.SendInputToProcess("save resume");
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
