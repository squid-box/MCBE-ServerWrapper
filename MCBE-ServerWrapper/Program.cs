namespace AhlSoft.BedrockServerWrapper
{
    using System;

    using AhlSoft.BedrockServerWrapper.Backups;
    using AhlSoft.BedrockServerWrapper.Logging;
    using AhlSoft.BedrockServerWrapper.PapyrusCs;
    using AhlSoft.BedrockServerWrapper.PlayerManagement;
    using AhlSoft.BedrockServerWrapper.Updater;

    using Autofac;

    /// <summary>
    /// Entry point for program.
    /// </summary>
    public static class Program
    {
        private static IContainer Container { get; set; }

        /// <summary>
        /// Entry point for program.
        /// </summary>
        public static void Main()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Log>().As<ILog>().SingleInstance();
            builder.Register(_ => SettingsProvider.Load()).As<ISettingsProvider>().SingleInstance();
            builder.RegisterType<PapyrusCsManager>().As<IPapyrusCsManager>().SingleInstance();
            builder.RegisterType<PlayerManager>().As<IPlayerManager>().SingleInstance();
            builder.RegisterType<BackupManager>().As<IBackupManager>().SingleInstance();
            builder.RegisterType<ServerProcess>().As<IServerProcess>().SingleInstance();
            Container = builder.Build();
            
            var log = Container.Resolve<ILog>();
            var settings = Container.Resolve<ISettingsProvider>();

            PrintTitle(log);

            if (!Utils.ValidateServerFiles(settings.ServerFolder))
            {
                log.Info("Could not find required server files, downloading latest version.");

                ServerDownloader.GetServerFiles(log, settings.ServerFolder);

                if (!Utils.ValidateServerFiles(settings.ServerFolder))
                {
                    log.Error("Server files broken / missing, please check and retry.");
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
                        CheckForUpdates(serverProcess, settings.ServerFolder, log);
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

        private static void PrintTitle(ILog log)
        {
            log.Info("----------------------------------------------");
            log.Info("  Minecraft Bedrock Dedicated Server Wrapper  ");
            log.Info($"  Version: {Utils.ProgramVersion}");
            log.Info("----------------------------------------------");
            log.Info("");
        }

        private static void CheckForUpdates(IServerProcess serverProcess, string rootPath, ILog log)
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
    }
}