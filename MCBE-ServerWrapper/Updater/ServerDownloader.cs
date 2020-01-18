namespace BedrockServerWrapper.Updater
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;

    public static class ServerDownloader
    {
        private static readonly Uri ServerDownloadPage = new Uri("https://www.minecraft.net/en-us/download/server/bedrock/");

        public static void GetServerFiles(string rootPath)
        {
            if (Utils.IsLinux())
            {
                if (!DownloadAndUnpackLinuxServer(rootPath))
                {
                    Console.Error.WriteLine("Failed to download server, shutting down.");
                    Environment.Exit(ExitCodes.InvalidServerFiles);
                }
            }
            else
            {
                if (!DownloadAndUnpackWindowsServer(rootPath))
                {
                    Console.Error.WriteLine("Failed to download server, shutting down.");
                    Environment.Exit(ExitCodes.InvalidServerFiles);
                }
            }
        }

        public static Version FindLatestServerVersion()
        {
            if (Utils.IsLinux())
            {
                return FindLatestLinuxServerVersion();
            }
            else
            {
                return FindLatestWindowsServerVersion();
            }
        }

        public static Version FindLatestWindowsServerVersion()
        {
            return FindCurrentVersion(@"https://minecraft.azureedge.net/bin-win/bedrock-server-(.*).zip");
        }

        public static Version FindLatestLinuxServerVersion()
        {
            return FindCurrentVersion(@"https://minecraft.azureedge.net/bin-linux/bedrock-server-(.*).zip");
        }

        public static bool DownloadAndUnpackWindowsServer(string targetDirectory)
        {
            Console.Out.WriteLine("Attempting to download Windows server files...");

            try
            {
                var serverZip = FindDownloadUrl(@"(https://minecraft.azureedge.net/bin-win/bedrock-server-.*.zip)");
                return DownloadAndUnzipPackage(serverZip, targetDirectory);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Couldn't get latest Windows server: {e.GetType()} : {e.Message}");
                return false;
            }
        }

        public static bool DownloadAndUnpackLinuxServer(string targetDirectory)
        {
            Console.Out.WriteLine("Attempting to download Linux server files...");

            try
            {
                var serverZip = FindDownloadUrl(@"(https://minecraft.azureedge.net/bin-linux/bedrock-server-.*.zip)");
                return DownloadAndUnzipPackage(serverZip, targetDirectory);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Couldn't get latest Linux server: {e.GetType()} : {e.Message}");
                return false;
            }
        }

        private static Version FindCurrentVersion(string regexPattern)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var downloadPageSource = client.DownloadString(ServerDownloadPage);
                    return new Version(Regex.Match(downloadPageSource, regexPattern).Groups[1].Value);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Couldn't determine latest server version:\n{e.GetType()} : {e.Message}");
                return null;
            }
        }

        private static bool DownloadAndUnzipPackage(Uri packageUrl, string targetDirectory)
        {
            var protectedFiles = new[]
            {
                Path.Combine(targetDirectory, "server.properties"),
                Path.Combine(targetDirectory, "permissions.json"),
                Path.Combine(targetDirectory, "whitelist.json")
            };

            try
            {
                using (var client = new WebClient())
                {
                    var done = false;
                    var lastProgress = 0;

                    client.DownloadProgressChanged += (s, a) => { lastProgress = a.ProgressPercentage; };
                    client.DownloadFileCompleted += (s, a) => { done = true; };

                    var filename = Path.GetTempFileName();
                    var tempBackupDir = Path.Combine(Path.GetTempPath(), "mcbesw_protectedfiles");
                    client.DownloadFileAsync(packageUrl, filename);

                    while (!done)
                    {
                        Console.Out.WriteLine($"Progress: {lastProgress}%");
                        Thread.Sleep(1000);
                    }

                    Console.Out.WriteLine("Download complete.");

                    Directory.CreateDirectory(targetDirectory);
                    Directory.CreateDirectory(tempBackupDir);

                    using (var zip = ZipFile.OpenRead(filename))
                    {
                        Console.Out.WriteLine($"Unzipping {zip.Entries.Count} files...");

                        foreach (var entry in zip.Entries)
                        {
                            var destination = Path.Combine(targetDirectory, entry.FullName);

                            if (protectedFiles.Contains(destination) && File.Exists(destination))
                            {
                                continue;
                            }

                            Directory.CreateDirectory(Path.GetDirectoryName(destination));

                            // If the entry is a directory (not a file), don't try to extract it.
                            if (Directory.Exists(destination))
                            {
                                continue;
                            }

                            entry.ExtractToFile(destination, true);
                        }
                    }

                    Console.Out.WriteLine("Unzip complete.");

                    Directory.Delete(tempBackupDir, true);

                    return true;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Couldn't get file \"{packageUrl}\": {e.GetType()} : {e.Message}");
                return false;
            }
        }

        private static Uri FindDownloadUrl(string regexPattern)
        {
            using (var client = new WebClient())
            {
                var downloadPageSource = client.DownloadString(ServerDownloadPage);
                return new Uri(Regex.Match(downloadPageSource, regexPattern).Groups[1].Value);
            }
        }
    }
}
