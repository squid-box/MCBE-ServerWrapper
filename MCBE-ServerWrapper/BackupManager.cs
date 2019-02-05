namespace MCBE_ServerWrapper
{
    using System.IO;

    public class BackupManager
    {
        public BackupManager()
        {

        }

        public static string BackupFolder => @"Backups";

        public void ReceiveInput(string input)
        {
        
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
