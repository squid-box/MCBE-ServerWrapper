namespace BedrockServerWrapper.Backups
{
    using System;
    using System.IO;

    public class BackupManager
    {
        private readonly ServerProcess _serverProcess;

        public BackupManager(ServerProcess serverProcess)
        {
            _serverProcess = serverProcess;
        }

        public static string BackupFolder => @"Backups";

        public void BackupReady(object sender, BackupReadyArguments arguments)
        {
            var tmpFolder = Path.Combine(BackupFolder, "tmp");
            Directory.CreateDirectory(tmpFolder);

            Console.Out.WriteLine("Copying files...");

            foreach (var file in arguments.BackupArguments.Split(','))
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
    }
}
