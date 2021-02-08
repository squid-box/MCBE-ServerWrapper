namespace AhlSoft.BedrockServerWrapper
{
    using System;
    using System.IO;
    using AhlSoft.BedrockServerWrapper.Updater;

    /// <summary>
    /// 
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var settings = Settings.Load();
            var log = new Log(settings);

            PrintTitle();

            if (args != null && args.Length < 2)
            {
                var rootPath = args.Length == 0 ? "." : args[0];

                var serverFolder = Path.Combine(rootPath, settings.ServerFolder);

                if (!Utils.ValidateServerFiles(serverFolder))
                {
                    log.Info("Could not find required server files, downloading latest version.");

                    ServerDownloader.GetServerFiles(log, serverFolder);
                }

                if (!Utils.ValidateServerFiles(serverFolder))
                {
                    PrintUsage(ExitCodes.InvalidServerFiles, log);
                    Environment.Exit(ExitCodes.InvalidServerFiles);
                }

                using (var serverProcess = new ServerProcess(serverFolder, log, settings))
                {
	                serverProcess.Start();

	                while (true)
	                {
		                var input = Console.ReadLine();

		                if (string.IsNullOrWhiteSpace(input))
		                {
			                continue;
		                }

		                if (input.Equals("stop", StringComparison.OrdinalIgnoreCase))
		                {
			                break;
		                }

                        if (input.Equals("backup", StringComparison.OrdinalIgnoreCase))
                        {
                            serverProcess.Backup();
                        }
                        else if (input.Equals("update", StringComparison.OrdinalIgnoreCase))
                        {
                            CheckForUpdates(serverProcess, serverFolder, log);
                        }
                        else if (input.Equals("values", StringComparison.OrdinalIgnoreCase))
                        {
                            serverProcess.PrintServerValues();
                        }
                        else
                        {
                            serverProcess.SendInputToProcess(input);
                        }
                    }

	                serverProcess.Stop();
                }

                Environment.Exit(ExitCodes.Ok);
            }
            else
            {
                PrintUsage(ExitCodes.InvalidNumberOfArguments, log);
                Environment.Exit(ExitCodes.InvalidNumberOfArguments);
            }
        }

        private static void PrintTitle()
        {
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine("  Minecraft Bedrock Dedicated Server Wrapper  ");
            Console.WriteLine($"  Version: {Utils.ProgramVersion}");
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine();
        }

        private static void CheckForUpdates(ServerProcess serverProcess, string rootPath, Log log)
        {
            log.Info("Checking for latest Bedrock server version...");
            var latestVersion = ServerDownloader.FindLatestServerVersion(log);

            if (
                latestVersion != null && 
                serverProcess.ServerValues.ContainsKey("ServerVersion") && 
                !string.IsNullOrEmpty(serverProcess.ServerValues["ServerVersion"]))
            {
                if (new Version(serverProcess.ServerValues["ServerVersion"]) < latestVersion)
                {
                    log.Info($"Found new version {latestVersion}, stopping server and updating.");
                    serverProcess.Stop();

                    ServerDownloader.GetServerFiles(log, rootPath);

                    serverProcess.Start();
                }
                else
                {
                    log.Info("Server is up-to-date!");
                }
            }
            else
            {
                log.Error("Unable to determine status version, update check aborted.");
            }
        }

        private static void PrintUsage(int exitCode, Log log)
        {
            if (exitCode == ExitCodes.InvalidNumberOfArguments)
            {
                Console.WriteLine("Usage: start with either:");
                Console.WriteLine(" * Without any arguments, if running in the server directory.");
                Console.WriteLine(" * Without one argument, pointing out the server directory.");
            }
            else if (exitCode == ExitCodes.InvalidServerFiles)
            {
                log.Error("Server files broken / missing, please check and retry.");
            }
        }
    }
}