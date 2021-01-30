namespace BedrockServerWrapper
{
    using System;
    using Updater;

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
            PrintTitle();

            if (args != null && args.Length < 2)
            {
                var rootPath = args.Length == 0 ? "." : args[0];

                if (!Utils.ValidateServerFiles(rootPath))
                {
                    Console.Out.WriteLine("Could not find required server files, downloading latest version.");

                    ServerDownloader.GetServerFiles(rootPath);
                }

                if (!Utils.ValidateServerFiles(rootPath))
                {
                    PrintUsage(ExitCodes.InvalidServerFiles);
                    Environment.Exit(ExitCodes.InvalidServerFiles);
                }

                using (var serverProcess = new ServerProcess(rootPath))
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
		                else if (input.Equals("backup", StringComparison.OrdinalIgnoreCase))
		                {
                            serverProcess.Backup();
		                }
		                else if (input.Equals("update", StringComparison.OrdinalIgnoreCase))
		                {
			                CheckForUpdates(serverProcess, rootPath);
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
                PrintUsage(ExitCodes.InvalidNumberOfArguments);
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

        private static void CheckForUpdates(ServerProcess serverProcess, string rootPath)
        {
            Console.Out.WriteLine("Checking for latest Bedrock server version...");
            var latestVersion = ServerDownloader.FindLatestServerVersion();

            if (
                latestVersion != null && 
                serverProcess.ServerValues.ContainsKey("ServerVersion") && 
                !string.IsNullOrEmpty(serverProcess.ServerValues["ServerVersion"]))
            {
                if (new Version(serverProcess.ServerValues["ServerVersion"]) < latestVersion)
                {
                    Console.Out.WriteLine($"Found new version {latestVersion}, stopping server and updating.");
                    serverProcess.Stop();

                    ServerDownloader.GetServerFiles(rootPath);

                    serverProcess.Start();
                }
                else
                {
                    Console.Out.WriteLine("Server is up-to-date!");
                }
            }
            else
            {
                Console.Error.WriteLine("Unable to determine status version, update check aborted.");
            }
        }

        private static void PrintUsage(int exitCode)
        {
            if (exitCode == ExitCodes.InvalidNumberOfArguments)
            {
                Console.WriteLine("Usage: start with either:");
                Console.WriteLine(" * Without any arguments, if running in the server directory.");
                Console.WriteLine(" * Without one argument, pointing out the server directory.");
            }
            else if (exitCode == ExitCodes.InvalidServerFiles)
            {
                Console.WriteLine("Server files broken / missing, please check and retry.");
            }
        }
    }
}