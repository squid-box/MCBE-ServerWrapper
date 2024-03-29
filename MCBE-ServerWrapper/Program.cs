﻿namespace AhlSoft.BedrockServerWrapper;

using System;
using System.Net.Http;

using AhlSoft.BedrockServerWrapper.Backups;
using AhlSoft.BedrockServerWrapper.Logging;
using AhlSoft.BedrockServerWrapper.PapyrusCs;
using AhlSoft.BedrockServerWrapper.PlayerManagement;
using AhlSoft.BedrockServerWrapper.SelfUpdating;
using AhlSoft.BedrockServerWrapper.Settings;
using AhlSoft.BedrockServerWrapper.Server;
using Autofac;
using Spectre.Console;

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
    /// <param name="args">Arguments passed to application</param>
    public static void Main(string[] args)
    {
        if (args.Length == 1 &&
            (args[0].Equals("-v") ||
            args[0].Equals("/v") ||
            args[0].Equals("--version")))
        {
            Console.Out.WriteLine(Utils.ProgramVersion);
            Environment.Exit(ExitCodes.Ok);
        }

        try
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Log>().As<ILog>().SingleInstance();
            builder.Register(_ => SettingsProvider.Load()).As<ISettingsProvider>().SingleInstance();
            builder.RegisterType<PapyrusCsManager>().As<IPapyrusCsManager>().SingleInstance();
            builder.RegisterType<HttpClient>().SingleInstance();
            builder.RegisterType<ServerDownloader>().SingleInstance();
            builder.RegisterType<SelfUpdater>().SingleInstance();
            builder.RegisterType<PlayerManager>().As<IPlayerManager>().SingleInstance();
            builder.RegisterType<BackupManager>().As<IBackupManager>().SingleInstance();
            builder.RegisterType<ServerProcess>().As<IServerProcess>().SingleInstance();
            Container = builder.Build();

            Log = Container.Resolve<ILog>();
            var serverDownloader = Container.Resolve<ServerDownloader>();
            var settings = Container.Resolve<ISettingsProvider>();

            PrintTitle();

            var (updateAvailable, updateVersion, updateUrl) = Container.Resolve<SelfUpdater>().CheckForUpdate();

            if (updateAvailable)
            {
                Log.Info($"MCBSW can be updated to {updateVersion}.");
                Log.Info($"Download available at: \"{updateUrl}\".");
            }

            if (!Utils.ValidateServerFiles(settings.ServerFolder))
            {
                Log.Info("Could not find required server files, downloading latest version.");

                serverDownloader.GetServerFiles(settings.ServerFolder);

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

                    input = input.Trim();

                    if (input.Equals("stop", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    if (input.Equals("update", StringComparison.OrdinalIgnoreCase))
                    {
                        CheckForUpdates(serverProcess, serverDownloader, settings.ServerFolder);
                    }
                    else if (input.Equals("licensing", StringComparison.OrdinalIgnoreCase))
                    {
                        Licenses.PrintLicenses();
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
        catch (Exception exception)
        {
            var message = $"Unhandled exception. {exception.GetType()}: {exception.Message}";

            Log?.Error(message, "red", false);
            AnsiConsole.WriteException(exception);

            try
            {
                var serverProcess = Container.Resolve<IServerProcess>();
                
                if (serverProcess.IsRunning)
                {
                    serverProcess.Stop();
                }
            }
            catch (Exception f)
            {
                Log?.Warning($"Could not kill server process: {f.GetType()} - {f.Message}");
            }

            Environment.Exit(ExitCodes.UnknownCrash);
        }
    }

    private static void PrintTitle()
    {
        var titleRule = new Rule("[lightseagreen]Minecraft Bedrock Dedicated Server Wrapper[/]")
        {
            Justification = Justify.Center,
            Style = Style.Parse("lightseagreen")
        };

        AnsiConsole.Write(titleRule);
        Log?.Info($"Starting version: {Utils.ProgramVersion}");
    }

    private static void CheckForUpdates(IServerProcess serverProcess, ServerDownloader serverDownloader, string rootPath)
    {
        Log?.Info("Checking for latest Bedrock server version...");
        var latestVersion = serverDownloader.FindLatestServerVersion();

        if (
            latestVersion != null && 
            serverProcess.ServerValues.ContainsKey("ServerVersion") && 
            !string.IsNullOrEmpty(serverProcess.ServerValues["ServerVersion"]))
        {
            if (new Version(serverProcess.ServerValues["ServerVersion"]) < latestVersion)
            {
                Log?.Info($"Found new version {latestVersion}, stopping server and updating.");
                serverProcess.Stop();

                serverDownloader.GetServerFiles(rootPath);

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
