namespace AhlSoft.BedrockServerWrapper.Server;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

using AhlSoft.BedrockServerWrapper.Backups;
using AhlSoft.BedrockServerWrapper.Logging;
using AhlSoft.BedrockServerWrapper.PlayerManagement;
using AhlSoft.BedrockServerWrapper.Settings;

using Spectre.Console;

/// <inheritdoc cref="IServerProcess" />
public class ServerProcess : IServerProcess
{
    private readonly Process _serverProcess;
    private readonly IPlayerManager _playerManager;
    private readonly IBackupManager _backupManager;
    private readonly ILog _log;
    private readonly ISettingsProvider _settingsProvider;

    private CancellationTokenSource _cancellationTokenSource;
    private DateTime _serverStarting;

    public ServerProcess(ILog log, ISettingsProvider settingsProvider, IPlayerManager playerManager, IBackupManager backupManager)
    {
        ServerValues = new Dictionary<string, string>();

        _settingsProvider = settingsProvider;
        _log = log;

        _serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(_settingsProvider.ServerFolder, ServerExecutable),
                WorkingDirectory = settingsProvider.ServerFolder,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            }
        };

        if (Utils.IsLinux())
        {
            _serverProcess.StartInfo.EnvironmentVariables.Add("LD_LIBRARY_PATH", _settingsProvider.ServerFolder);
        }

        _playerManager = playerManager;
        
        _backupManager = backupManager;
        _backupManager.BackupCompleted += BackupCompleted;
        _backupManager.ScheduledBackup += (_, __) => Backup();
    }

    /// <summary>
    /// Gets the filename of the server executable.
    /// </summary>
    public static string ServerExecutable => Utils.IsLinux() ? "bedrock_server" : "bedrock_server.exe";

    /// <inheritdoc />
    public Dictionary<string, string> ServerValues { get; }

    /// <inheritdoc />
    public bool IsRunning
    {
        get
        {
            try
            {
                Process.GetProcessById(_serverProcess.Id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    /// <inheritdoc />
    public void SendInputToProcess(string input)
    {
        if (!IsRunning)
        {
            _log.Error("Unable to send input to process: not running.");
            return;
        }

        if (input.Equals("values", StringComparison.OrdinalIgnoreCase))
        {
            PrintServerValues();
        }
        else if (input.Equals("backup", StringComparison.OrdinalIgnoreCase))
        {
            Backup();
        }
        else if (input.StartsWith("autobackup", StringComparison.OrdinalIgnoreCase))
        {
            HandleAutoBackupInput(input);
        }
        else if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
        {
            Help();
        }
        else
        {
            _serverProcess.StandardInput.WriteLine(input);
        }
    }

    /// <inheritdoc />
    public void Start()
    {
        if (!IsRunning && Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ServerExecutable)).Length > 0)
        {
            throw new InvalidOperationException("Server process is already running, can't start again.");
        }

        _serverProcess.Start();

        while (!IsRunning)
        {
            Thread.Sleep(100);
        }

        _serverProcess.OutputDataReceived += ReceivedStandardOutput;
        _serverProcess.ErrorDataReceived += ReceivedErrorOutput;

        _serverProcess.BeginOutputReadLine();
        _serverProcess.BeginErrorReadLine();
        _serverProcess.StandardInput.AutoFlush = true;
    }

    /// <inheritdoc />
    public void Stop()
    {
        SendInputToProcess("stop");
        _log.Info("Shutting down server...");

        _serverProcess.WaitForExit(5000);

        if (!_serverProcess.HasExited)
        {
            _log.Error("Could not exit server process. Killing.");
            _serverProcess.Kill();
        }

        _serverProcess.CancelOutputRead();
        _serverProcess.CancelErrorRead();
        _serverProcess.OutputDataReceived -= ReceivedStandardOutput;
        _serverProcess.ErrorDataReceived -= ReceivedErrorOutput;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Say(string message)
    {
        SendInputToProcess($"say {message}");
    }

    private void PrintServerValues()
    {
        // Create a table
        var table = new Table();

        // Add some columns
        table.AddColumn("Name");
        table.AddColumn("Value");

        var alternating = true;

        // Add some rows
        foreach (var (name, value) in ServerValues)
        {
            table.AddRow($"[{(alternating ? "white" : "grey84")}]{name}[/]", $"[{(alternating ? "white" : "grey84")}]{value}[/]");
            alternating = !alternating;
        }

        // Render the table to the console
        AnsiConsole.Write(table);
    }

    private void Backup()
    {
        _backupManager.HasBackupBeenInitiated = true;
        SendInputToProcess("save hold");
    }

    private void ReceivedStandardOutput(object sender, DataReceivedEventArgs e)
    {
        if (e?.Data == null)
        {
            return;
        }

        if (e.Data.Contains("Starting Server"))
        {
            _serverStarting = DateTime.Now;
        }

        if (e.Data.Contains("Server started"))
        {
            if (_serverStarting != DateTime.MinValue)
            {
                _log.Info($"Server started in {(DateTime.Now - _serverStarting).TotalMilliseconds} ms.", "white");
                return;
            }
        }

        if (e.Data.Contains("Saving..."))
        {
            _log.Info("Backup started...", "grey");
            Say("Backup started.");

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            var thread = new Thread(() =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    SendInputToProcess("save query");
                    Thread.Sleep(1000);
                }
            });
            thread.Start();

            return;
        }

        if (e.Data.Contains("level.dat"))
        {
            _cancellationTokenSource.Cancel();
            _backupManager.Backup(e.Data);
            return;
        }

        if (e.Data.Contains("Player connected"))
        {
            var playerData = Regex.Match(e.Data, @".* Player connected: (.*), xuid: (.*)");
            var player = new Player(playerData.Groups[1].Value, playerData.Groups[2].Value);
            _playerManager.PlayerJoined(player);

            var timePlayed = _playerManager.GetPlayedMinutes(player);

            var thread = new Thread(() =>
            {
                Thread.Sleep(5000);

                Say(timePlayed == -1
                    ? $"Welcome {player}!"
                    : $"Welcome back {player}, you've played {Utils.TimePlayedConversion(timePlayed)}, we last saw you {_playerManager.GetLastSeen(player):yyyy-MM-dd}.");
            });
            thread.Start();
        }

        if (e.Data.Contains("Player disconnected"))
        {
            var playerData = Regex.Match(e.Data, @".* Player disconnected: (.*), xuid: (.*)");
            var player = new Player(playerData.Groups[1].Value, playerData.Groups[2].Value);
            _playerManager.PlayerLeft(player);

            Say($"Goodbye {player}!");
        }

        if (e.Data.Contains("Difficulty: ") && !ServerValues.ContainsKey("Difficulty"))
        {
            ServerValues.Add("Difficulty", Regex.Match(e.Data, @".*Difficulty: \d (.*)").Groups[1].Value);
        }

        if (e.Data.Contains("Game mode: ") && !ServerValues.ContainsKey("GameMode"))
        {
            ServerValues.Add("GameMode", Regex.Match(e.Data, @".*Game mode: \d (.*)").Groups[1].Value);
        }

        if (e.Data.Contains("Level Name: ") && !ServerValues.ContainsKey("LevelName"))
        {
            _settingsProvider.LevelName = Regex.Match(e.Data, @".*Level Name: (.*)").Groups[1].Value;
        }

        if (e.Data.Contains("Version") && !ServerValues.ContainsKey("ServerVersion"))
        {
            ServerValues["ServerVersion"] = Regex.Match(e.Data, @".*Version (\d+\.\d+\.\d+\.\d+)").Groups[1].Value;
        }

        if (e.Data.Contains("IPv4 supported") && !ServerValues.ContainsKey("IpV4Port"))
        {
            ServerValues["IpV4Port"] = Regex.Match(e.Data, @".*port: (\d*)").Groups[1].Value;
        }

        if (e.Data.Contains("IPv6 supported") && !ServerValues.ContainsKey("IpV6Port"))
        {
            ServerValues["IpV6Port"] = Regex.Match(e.Data, @".*port: (\d*)").Groups[1].Value;
        }

        _log.Info(e.Data.Replace("┬º2", string.Empty), "white");
    }

    private void ReceivedErrorOutput(object sender, DataReceivedEventArgs e)
    {
        _log.Error(e.Data);
    }

    private static void Help()
    {
        AnsiConsole.MarkupLine("[underline]MCBE-SW commands:[/]");
        AnsiConsole.MarkupLine("* [cornflowerblue]autobackup[/] : Allows you to change automatic backup settings.");
        AnsiConsole.MarkupLine("* [cornflowerblue]backup[/] : Performs a backup.");
        AnsiConsole.MarkupLine("* [cornflowerblue]licensing[/] : Prints license information.");
        AnsiConsole.MarkupLine("* [cornflowerblue]stop[/] : Stops the server and shuts down MCBE-SW.");
        AnsiConsole.MarkupLine("* [cornflowerblue]update[/] : Checks for new MCBE server version, and updates if available.");
        AnsiConsole.MarkupLine("");
        AnsiConsole.MarkupLine("Use \"[cornflowerblue]help <number>[/]\" for MCBE help pages");
    }

    private void BackupCompleted(object sender, BackupCompletedEventArgs args)
    {
        SendInputToProcess("save resume");
        var backupFile = args.BackupFile;

        if (string.IsNullOrEmpty(backupFile))
        {
            Say("Backup failed to create a file..?");
            return;
        }

        Say(args.Successful
            ? $"Backup completed: {Path.GetFileName(backupFile)} ({new FileInfo(backupFile).Length / 1024 / 1024}MB), completed in {args.BackupDuration.TotalSeconds}s."
            : "Backup failed.");
    }

    private void HandleAutoBackupInput(string input)
    {
        const string errorMessage = "[cornflowerblue]autobackup[/] accepts the following options: [cornflowerblue]enable[/], [cornflowerblue]disable[/], [cornflowerblue]frequency <minutes>[/].";

        var splitTemp = input.Split(' ');

        if (splitTemp.Length < 2)
        {
            AnsiConsole.MarkupLine(errorMessage);
            return;
        }

        switch (splitTemp[1])
        {
            case "enable":
                _settingsProvider.AutomaticBackupEnabled = true;
                break;
            case "disable":
                _settingsProvider.AutomaticBackupEnabled = false;
                break;
            case "frequency":
                if (splitTemp.Length != 3)
                {
                    AnsiConsole.MarkupLine("[cornflowerblue]autobackup frequency[/] requires an argument for the number of minutes between backups.");
                    return;
                }

                try
                {
                    _settingsProvider.AutomaticBackupFrequency = Convert.ToInt32(splitTemp[2]);
                }
                catch (Exception e)
                {
                    AnsiConsole.MarkupLine($"Could not convert \"{splitTemp[2]}\" to an integer. {e.GetType()}: {e.Message}");
                }

                break;
            default:
                AnsiConsole.MarkupLine(errorMessage);
                break;
        }
    }

    /// <summary>
    /// Dispose all resources.
    /// </summary>
    /// <param name="disposing">Whether or not we're disposing.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
            _settingsProvider.Save();

            _backupManager.BackupCompleted -= BackupCompleted;

            _cancellationTokenSource?.Dispose();
            _serverProcess?.Dispose();
			}
    }
}
