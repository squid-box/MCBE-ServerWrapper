namespace AhlSoft.BedrockServerWrapper.Server
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading;

    using AhlSoft.BedrockServerWrapper.Logging;

    /// <summary>
    /// Utility class for finding and downloading server data.
    /// </summary>
    public class ServerDownloader
    {
        private readonly Uri ServerDownloadPage = new("https://www.minecraft.net/en-us/download/server/bedrock/");
        private readonly ILog _log;
        private readonly HttpClient _httpClient;
        
        public ServerDownloader(ILog log, HttpClient httpClient)
        {
            _log = log;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Gets appropriate server files and puts them in a given path.
        /// </summary>
        /// <param name="log">Logger to use.</param>
        /// <param name="rootPath">Path to download files to.</param>
        public void GetServerFiles(string rootPath)
        {
            if (Utils.IsLinux())
            {
                if (!DownloadAndUnpackLinuxServer(rootPath))
                {
                    _log?.Error("Failed to download server, shutting down.");
                    Environment.Exit(ExitCodes.InvalidServerFiles);
                }
            }
            else
            {
                if (!DownloadAndUnpackWindowsServer(rootPath))
                {
                    _log?.Error("Failed to download server, shutting down.");
                    Environment.Exit(ExitCodes.InvalidServerFiles);
                }
            }
        }

        /// <summary>
        /// Determines the latest version of the server available for the current system.
        /// </summary>
        /// <returns>The latest version of the server available.</returns>
        public Version FindLatestServerVersion()
        {
	        return Utils.IsLinux() ? FindLatestLinuxServerVersion() : FindLatestWindowsServerVersion();
        }

        private Version FindLatestWindowsServerVersion()
        {
            return FindCurrentVersion(@"https://minecraft.azureedge.net/bin-win/bedrock-server-(.*).zip");
        }
        private Version FindLatestLinuxServerVersion()
        {
            return FindCurrentVersion(@"https://minecraft.azureedge.net/bin-linux/bedrock-server-(.*).zip");
        }

        private bool DownloadAndUnpackWindowsServer(string targetDirectory)
        {
            _log?.Info("Attempting to download Windows server files...");

            try
            {
                var serverZip = FindDownloadUrl(@"(https://minecraft.azureedge.net/bin-win/bedrock-server-.*.zip)");
                return DownloadAndUnzipPackage(serverZip, targetDirectory);
            }
            catch (Exception e)
            {
                _log?.Error($"Couldn't get latest Windows server.");
                _log?.Exception(e);

                return false;
            }
        }

        private bool DownloadAndUnpackLinuxServer(string targetDirectory)
        {
            _log?.Info("Attempting to download Linux server files...");

            try
            {
                var serverZip = FindDownloadUrl(@"(https://minecraft.azureedge.net/bin-linux/bedrock-server-.*.zip)");
                return DownloadAndUnzipPackage(serverZip, targetDirectory);
            }
            catch (Exception e)
            {
                _log?.Error($"Couldn't get latest Linux server.");
                _log?.Exception(e);

                return false;
            }
        }

        private Version FindCurrentVersion(string regexPattern)
        {
            try
            {
                var downloadPageSource = _httpClient.GetStringAsync(ServerDownloadPage).Result;

                return new Version(Regex.Match(downloadPageSource, regexPattern).Groups[1].Value);
            }
            catch (Exception e)
            {
                _log?.Error($"Couldn't determine latest server version:");
                _log?.Exception(e);

                return null;
            }
        }

        private bool DownloadAndUnzipPackage(Uri packageUrl, string targetDirectory)
        {
            var protectedFiles = new[]
            {
                Path.Combine(targetDirectory, "server.properties"),
                Path.Combine(targetDirectory, "permissions.json"),
                Path.Combine(targetDirectory, "whitelist.json")
            };

            try
            {
                using var downloadStream = _httpClient.GetStreamAsync(packageUrl);

                while (!downloadStream.IsCompleted)
                {
                    _log.Info("Downloading...");
                    Thread.Sleep(1000);
                }

                var filename = Path.GetTempFileName();
                var tempBackupDir = Path.Combine(Path.GetTempPath(), "mcbesw_protectedFiles");

                using var fileStream = new FileStream(filename, FileMode.CreateNew);
                downloadStream.Result.CopyTo(fileStream);

                _log?.Info("Download complete.");

                Directory.CreateDirectory(targetDirectory);
                Directory.CreateDirectory(tempBackupDir);

                using (var zip = ZipFile.OpenRead(filename))
                {
                    _log?.Info($"Unzipping {zip.Entries.Count} files...");

                    foreach (var entry in zip.Entries)
                    {
                        var destination = Path.Combine(targetDirectory, entry.FullName);

                        if (protectedFiles.Contains(destination) && File.Exists(destination))
                        {
                            continue;
                        }

                        var destinationDirectory = Path.GetDirectoryName(destination);

                        if (destinationDirectory == null)
                        {
                            _log?.Error($"Couldn't determine directory of path \"{destination}\".");
                            return false;
                        }

                        Directory.CreateDirectory(destinationDirectory);

                        // If the entry is a directory (not a file), don't try to extract it.
                        if (Directory.Exists(destination))
                        {
                            continue;
                        }

                        entry.ExtractToFile(destination, true);
                    }
                }

                _log?.Info("Unzip complete.");

                Utils.DeleteDirectory(tempBackupDir, _log);

                if (Utils.IsLinux())
                {
                    if (!Utils.MakeExecutable(Path.Combine(targetDirectory, ServerProcess.ServerExecutable), _log))
                    {
                        _log?.Error("Could not make server program executable.");

                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                _log?.Error($"Couldn't get file \"{packageUrl}\".");
                _log?.Exception(e);

                return false;
            }
        }

        private Uri FindDownloadUrl(string regexPattern)
        {
            var downloadPageSource = _httpClient.GetStringAsync(ServerDownloadPage).Result;

            return new Uri(Regex.Match(downloadPageSource, regexPattern).Groups[1].Value);
        }
    }
}