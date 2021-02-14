namespace AhlSoft.BedrockServerWrapper.Server
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;

    using AhlSoft.BedrockServerWrapper.Logging;

    /// <summary>
    /// Utility class for finding and downloading server data.
    /// </summary>
    public static class ServerDownloader
    {
        private static readonly Uri ServerDownloadPage = new Uri("https://www.minecraft.net/en-us/download/server/bedrock/");

        /// <summary>
        /// Gets appropriate server files and puts them in a given path.
        /// </summary>
        /// <param name="log">Logger to use.</param>
        /// <param name="rootPath">Path to download files to.</param>
        public static void GetServerFiles(ILog log, string rootPath)
        {
            if (Utils.IsLinux())
            {
                if (!DownloadAndUnpackLinuxServer(log, rootPath))
                {
                    log?.Error("Failed to download server, shutting down.");
                    Environment.Exit(ExitCodes.InvalidServerFiles);
                }
            }
            else
            {
                if (!DownloadAndUnpackWindowsServer(log, rootPath))
                {
                    log?.Error("Failed to download server, shutting down.");
                    Environment.Exit(ExitCodes.InvalidServerFiles);
                }
            }
        }

        /// <summary>
        /// Determines the latest version of the server available for the current system.
        /// </summary>
        /// <returns>The latest version of the server available.</returns>
        public static Version FindLatestServerVersion(ILog log)
        {
	        return Utils.IsLinux() ? FindLatestLinuxServerVersion(log) : FindLatestWindowsServerVersion(log);
        }

        private static Version FindLatestWindowsServerVersion(ILog log)
        {
            return FindCurrentVersion(log, @"https://minecraft.azureedge.net/bin-win/bedrock-server-(.*).zip");
        }
        private static Version FindLatestLinuxServerVersion(ILog log)
        {
            return FindCurrentVersion(log, @"https://minecraft.azureedge.net/bin-linux/bedrock-server-(.*).zip");
        }

        private static bool DownloadAndUnpackWindowsServer(ILog log, string targetDirectory)
        {
            log?.Info("Attempting to download Windows server files...");

            try
            {
                var serverZip = FindDownloadUrl(@"(https://minecraft.azureedge.net/bin-win/bedrock-server-.*.zip)");
                return DownloadAndUnzipPackage(log, serverZip, targetDirectory);
            }
            catch (Exception e)
            {
                log?.Error($"Couldn't get latest Windows server: {e.GetType()} : {e.Message}");
                return false;
            }
        }

        private static bool DownloadAndUnpackLinuxServer(ILog log, string targetDirectory)
        {
            log?.Info("Attempting to download Linux server files...");

            try
            {
                var serverZip = FindDownloadUrl(@"(https://minecraft.azureedge.net/bin-linux/bedrock-server-.*.zip)");
                return DownloadAndUnzipPackage(log, serverZip, targetDirectory);
            }
            catch (Exception e)
            {
                log?.Error($"Couldn't get latest Linux server: {e.GetType()} : {e.Message}");
                return false;
            }
        }

        private static Version FindCurrentVersion(ILog log, string regexPattern)
        {
            try
            {
                using var client = new WebClient();
                var downloadPageSource = client.DownloadString(ServerDownloadPage);
                return new Version(Regex.Match(downloadPageSource, regexPattern).Groups[1].Value);
            }
            catch (Exception e)
            {
                log?.Error($"Couldn't determine latest server version:\n{e.GetType()} : {e.Message}");
                return null;
            }
        }

        private static bool DownloadAndUnzipPackage(ILog log, Uri packageUrl, string targetDirectory)
        {
            var protectedFiles = new[]
            {
                Path.Combine(targetDirectory, "server.properties"),
                Path.Combine(targetDirectory, "permissions.json"),
                Path.Combine(targetDirectory, "whitelist.json")
            };

            try
            {
                using var client = new WebClient();

                var done = false;
                var lastProgress = 0;

                client.DownloadProgressChanged += (s, a) => { lastProgress = a.ProgressPercentage; };
                client.DownloadFileCompleted += (s, a) => { done = true; };

                var filename = Path.GetTempFileName();
                var tempBackupDir = Path.Combine(Path.GetTempPath(), "mcbesw_protectedfiles");
                client.DownloadFileAsync(packageUrl, filename);

                while (!done)
                {
                    log?.Info($"Progress: {lastProgress}%");
                    Thread.Sleep(1000);
                }

                log?.Info("Download complete.");

                Directory.CreateDirectory(targetDirectory);
                Directory.CreateDirectory(tempBackupDir);

                using (var zip = ZipFile.OpenRead(filename))
                {
                    log?.Info($"Unzipping {zip.Entries.Count} files...");

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

                log?.Info("Unzip complete.");

                Utils.DeleteDirectory(tempBackupDir, log);

                return true;
            }
            catch (Exception e)
            {
                log?.Error($"Couldn't get file \"{packageUrl}\": {e.GetType()} : {e.Message}");
                return false;
            }
        }

        private static Uri FindDownloadUrl(string regexPattern)
        {
            using var client = new WebClient();
            var downloadPageSource = client.DownloadString(ServerDownloadPage);
            return new Uri(Regex.Match(downloadPageSource, regexPattern).Groups[1].Value);
        }
    }
}