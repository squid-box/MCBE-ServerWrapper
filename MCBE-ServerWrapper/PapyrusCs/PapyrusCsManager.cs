namespace AhlSoft.BedrockServerWrapper.PapyrusCs
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using AhlSoft.BedrockServerWrapper.Logging;
    using AhlSoft.BedrockServerWrapper.Settings;

    /// <inheritdoc cref="IPapyrusCsManager" />
    public class PapyrusCsManager : IPapyrusCsManager
    {
        private readonly ISettingsProvider _settingsProvider;
        private readonly ILog _log;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates a new <see cref="PapyrusCsManager" />.
        /// </summary>
        /// <param name="settingsProvider">SettingsProvider.</param>
        /// <param name="log">Logger.</param>
        public PapyrusCsManager(ISettingsProvider settingsProvider, ILog log, HttpClient httpClient)
        {
            _settingsProvider = settingsProvider;
            _log = log;
            _httpClient = httpClient;
        }

        private string PapyrusCsExecutable => Path.Combine(_settingsProvider.PapyrusFolder, Utils.IsLinux() ? "PapyrusCs" : "PapyrusCs.exe");
        
        /// <summary>
        /// Gets a value indicating whether or not PapyrusCs is installed and available.
        /// </summary>
        private bool IsPapyrusCsAvailable => File.Exists(PapyrusCsExecutable);

        /// <inheritdoc />
        public void GenerateMap(string tempFolder)
        {
            if (!_settingsProvider.PapyrusEnabled)
            {
                return;
            }

            _log.Info("Map generation starting.");

            var worldFolder = Path.Combine(tempFolder, _settingsProvider.ServerFolder, "worlds", _settingsProvider.LevelName);

            if (!Directory.Exists(worldFolder))
            {
                _log.Error("World folder could not be found.");
                return;
            }

            if (!IsPapyrusCsAvailable)
            {
                try
                {
                    InstallPapyrusCs();
                }
                catch (Exception e)
                {
                    _log.Error($"Could not install Papyrus. Exception: {e.GetType()}: {e.Message}");
                    return;
                }
            }

            GenerateDimensionMap(worldFolder, 0);
            GenerateDimensionMap(worldFolder, 1);
            GenerateDimensionMap(worldFolder, 2);

            _log.Info("Map generation done.");

            try
            {
                Utils.DeleteDirectory(tempFolder, _log);
            }
            catch (Exception e)
            {
                _log.Error($"Couldn't delete temporary files. {e.GetType()}: {e.Message}");
            }

            if (!string.IsNullOrEmpty(_settingsProvider.PapyrusPostRunCommand))
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = Utils.IsLinux() ? "/bin/bash" : "cmd.exe",
                        Arguments = $"{(Utils.IsLinux() ? "-c" : "/C")} \"{_settingsProvider.PapyrusPostRunCommand}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                _log.Info($"Map generation PostRunCommand executed with {process.ExitCode}.");
            }
        }

        private void GenerateDimensionMap(string worldFolder, int dimension)
        {
            _log.Info($"Generating map for dimension {dimension}.");

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = PapyrusCsExecutable,
                    Arguments = $"-w \"{worldFolder}\" -o \"{_settingsProvider.PapyrusOutputFolder}\" --dim {dimension} --htmlfile index.html",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                if (process.ExitCode == -532462766 || process.ExitCode == 134)
                {
                    _log.Warning("Couldn't generate map for non-existent dimension.");
                }
                else
                {
                    _log.Error($"Map generation failed with exit code {process.ExitCode}.");
                }
            }
        }

        private void InstallPapyrusCs()
        {
            if (IsPapyrusCsAvailable)
            {
                _log.Warning("PapyrusCs already installed.");
                return;
            }

            var target = Utils.IsLinux() ? "-linux64.zip" : "-win64.zip";
            var tempFile = Path.GetTempFileName();

            var latestReleaseJson = _httpClient.GetStringAsync(@"https://api.github.com/repos/mjungnickel18/papyruscs/releases/latest").Result;

            var match = Regex.Match(latestReleaseJson, $"^.*browser_download_url.*(https://.*{target})\".*$");

            if (!match.Success)
            {
                _log.Error("Could not find the latest release of PapyrusCs.");
                return;
            }

            using var downloadStream = _httpClient.GetStreamAsync(match.Groups[1].Value);

            while (!downloadStream.IsCompleted)
            {
                _log.Info("Downloading PapyrusCs...");
                Thread.Sleep(1000);
            }

            using (var fileStream = new FileStream(tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                downloadStream.Result.CopyTo(fileStream);
            }
            
            Utils.DeleteDirectory(_settingsProvider.PapyrusFolder, _log);
            ZipFile.ExtractToDirectory(tempFile, _settingsProvider.PapyrusFolder);
            File.Delete(tempFile);

            if (Utils.IsLinux())
            {
                Utils.MakeExecutable(PapyrusCsExecutable, _log);
            }
        }
    }
}