namespace BedrockServerWrapper
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Text.RegularExpressions;

    public class PapyrusCsController
    {
        private readonly Settings _settings;
        private readonly Log _log;

        public PapyrusCsController(Settings settings, Log log)
        {
            _settings = settings;
            _log = log;
        }

        internal string PapyrusCsExecutable => Path.Combine(_settings.PapyrusCsFolder, Utils.IsLinux() ? "PapyrusCs" : "PapyrusCs.exe");

        public bool IsPapyrusCsAvailable => File.Exists(PapyrusCsExecutable);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Ensure the world is not loaded/locked when generating map.</remarks>
        public void GenerateMap(string worldFolder)
        {
            _log.Info("Map generation starting.");

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
            
            if (!string.IsNullOrEmpty(_settings.PapyrusPostRunCommand))
            {
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = Utils.IsLinux() ? "/bin/bash" : "cmd.exe",
                        Arguments = $"{(Utils.IsLinux() ? "-c" : "/C")} \"{_settings.PapyrusPostRunCommand}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    process.Start();
                    process.WaitForExit();

                    _log.Info($"PostRunCommand executed with {process.ExitCode}.");
                }
            }
        }

        private void GenerateDimensionMap(string worldFolder, int dimension)
        {
            _log.Info($"Generating map for dimension {dimension}.");

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = PapyrusCsExecutable,
                    Arguments = $"-w \"{worldFolder}\" -o \"{_settings.PapyrusOutputFolder}\" --dim {dimension} --htmlfile index.html",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                process.Start();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    if (process.ExitCode == -532462766)
                    {
                        _log.Warning("Couldn't generate map for non-existent dimension.");
                    }
                    else
                    {
                        _log.Error($"Map generation failed with exit code {process.ExitCode}.");
                    }
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

            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent", "MCBE-ServerWrapper");
                var latestReleaseJson = client.DownloadString(@"https://api.github.com/repos/mjungnickel18/papyruscs/releases/latest");

                var match = Regex.Match(latestReleaseJson, $"^.*browser_download_url.*(https://.*{target})\".*$");
                 
                if (!match.Success)
                {
                    _log.Error("Could not find the latest release of PapyrusCs.");
                    return;
                }

                client.DownloadFile(match.Groups[1].Value, tempFile);

                if (Directory.Exists(_settings.PapyrusCsFolder))
                {
                    Directory.Delete(_settings.PapyrusCsFolder, true);
                }

                ZipFile.ExtractToDirectory(tempFile, _settings.PapyrusCsFolder);

                File.Delete(tempFile);
            }

            if (Utils.IsLinux())
            {
                _log.Info("Making PapyrusCs executable.");
                using (var process = new Process())
                {
                    var chmod = $"chmod +x '{PapyrusCsExecutable}'";
                    
                    process.StartInfo = new ProcessStartInfo
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{chmod}\""
                    };

                    process.Start();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        _log.Error($"\"{chmod}\" failed with exit code {process.ExitCode}.");
                    }
                };
            }
        }
    }
}
