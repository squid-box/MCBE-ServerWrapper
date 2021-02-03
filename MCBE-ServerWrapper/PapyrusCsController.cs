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

        public PapyrusCsController(Settings settings)
        {
            _settings = settings;
        }

        internal string PapyrusCsExecutable => Path.Combine(_settings.PapyrusCsFolder, Utils.IsLinux() ? "PapyrusCs" : "PapyrusCs.exe");

        public bool IsPapyrusCsAvailable => File.Exists(PapyrusCsExecutable);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Ensure the world is not loaded/locked when generating map.</remarks>
        public void GenerateMap(string worldFolder)
        {
            Console.WriteLine("Map generation starting.");

            if (!Directory.Exists(worldFolder))
            {
                Console.Error.WriteLine("World folder could not be found.");
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
                    Console.Error.WriteLine($"Could not install Papyrus. Exception: {e.GetType()}: {e.Message}");
                    return;
                }
            }

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = PapyrusCsExecutable,
                    Arguments = $"-w \"{worldFolder}\" -o \"{_settings.PapyrusOutputFolder}\" --htmlfile index.html",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process.Start();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    Console.Error.WriteLine($"Map generation failed with exit code {process.ExitCode}.");
                    return;
                }
            }

            Console.WriteLine("Map generation done.");
            Console.WriteLine($"PostRunCommand: {_settings.PapyrusPostRunCommand}");

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

                    Console.Out.WriteLine($"PostRunCommand executed with {process.ExitCode}.");
                }
            }
        }

        private void InstallPapyrusCs()
        {
            if (IsPapyrusCsAvailable)
            {
                Console.Error.WriteLine("PapyrusCs already installed.");
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
                    Console.Error.WriteLine("Could not find the latest release of PapyrusCs.");
                    return;
                }

                var url = match.Groups[1].Value;

                Console.WriteLine($"Downloading {url} to {tempFile}.");
                client.DownloadFile(url, tempFile);

                if (Directory.Exists(_settings.PapyrusCsFolder))
                {
                    Directory.Delete(_settings.PapyrusCsFolder, true);
                }

                ZipFile.ExtractToDirectory(tempFile, _settings.PapyrusCsFolder);

                File.Delete(tempFile);
            }

            if (Utils.IsLinux())
            {
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        FileName = "/bin/bash",
                        Arguments = $"-c \"chmod +x '{PapyrusCsExecutable}'\""
                    };

                    process.Start();
                    process.WaitForExit();
                };
            }
        }
    }
}
