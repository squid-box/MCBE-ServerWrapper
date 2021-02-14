namespace AhlSoft.BedrockServerWrapper
{
    using System;

    using AhlSoft.BedrockServerWrapper.Backups;
    using AhlSoft.BedrockServerWrapper.Logging;
    using AhlSoft.BedrockServerWrapper.PapyrusCs;
    using AhlSoft.BedrockServerWrapper.PlayerManagement;
    using AhlSoft.BedrockServerWrapper.Settings;
    using AhlSoft.BedrockServerWrapper.Server;

    using Autofac;

    /// <summary>
    /// Entry point for program.
    /// </summary>
    public static class Program
    {
        private static IContainer Container { get; set; }

        private static ILog Log { get; set; }

        /// <summary>
        /// Entry point for program.
        /// </summary>
        public static void Main()
        {
            try
            {
                var builder = new ContainerBuilder();
                builder.RegisterType<Log>().As<ILog>().SingleInstance();
                builder.Register(_ => SettingsProvider.Load()).As<ISettingsProvider>().SingleInstance();
                builder.RegisterType<PapyrusCsManager>().As<IPapyrusCsManager>().SingleInstance();
                builder.RegisterType<PlayerManager>().As<IPlayerManager>().SingleInstance();
                builder.RegisterType<BackupManager>().As<IBackupManager>().SingleInstance();
                builder.RegisterType<ServerProcess>().As<IServerProcess>().SingleInstance();
                Container = builder.Build();

                Log = Container.Resolve<ILog>();
                var settings = Container.Resolve<ISettingsProvider>();

                PrintTitle();

                if (!Utils.ValidateServerFiles(settings.ServerFolder))
                {
                    Log.Info("Could not find required server files, downloading latest version.");

                    ServerDownloader.GetServerFiles(Log, settings.ServerFolder);

                    if (!Utils.ValidateServerFiles(settings.ServerFolder))
                    {
                        Log.Error("Server files broken / missing, please check and retry.");
                        Environment.Exit(ExitCodes.InvalidServerFiles);
                    }
                }

                using (var serverProcess = Container.Resolve<IServerProcess>())
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

                        if (input.Equals("update", StringComparison.OrdinalIgnoreCase))
                        {
                            CheckForUpdates(serverProcess, settings.ServerFolder);
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
            catch (Exception e)
            {
                var message = $"Unhandled exception. {e.GetType()}: {e.Message}";

                if (Log != null)
                {
                    Log?.Error(message);
                }
                else
                {
                    Console.Error.WriteLine(message);
                }
                
                Environment.Exit(ExitCodes.UnknownCrash);
            }
        }

        private static void PrintTitle()
        {
            Log?.Info("----------------------------------------------");
            Log?.Info("  Minecraft Bedrock Dedicated Server Wrapper  ");
            Log?.Info($"  Version: {Utils.ProgramVersion}");
            Log?.Info("----------------------------------------------");
            Log?.Info("");
        }

        private static void CheckForUpdates(IServerProcess serverProcess, string rootPath)
        {
            Log?.Info("Checking for latest Bedrock server version...");
            var latestVersion = ServerDownloader.FindLatestServerVersion(Log);

            if (
                latestVersion != null && 
                serverProcess.ServerValues.ContainsKey("ServerVersion") && 
                !string.IsNullOrEmpty(serverProcess.ServerValues["ServerVersion"]))
            {
                if (new Version(serverProcess.ServerValues["ServerVersion"]) < latestVersion)
                {
                    Log?.Info($"Found new version {latestVersion}, stopping server and updating.");
                    serverProcess.Stop();

                    ServerDownloader.GetServerFiles(Log, rootPath);

                    serverProcess.Start();
                }
                else
                {
                    Log?.Info("Server is up-to-date!");
                }
            }
            else
            {
                Log?.Error("Unable to determine status version, update check aborted.");
            }
        }
    }
}