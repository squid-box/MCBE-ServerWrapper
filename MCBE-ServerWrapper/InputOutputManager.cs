namespace AhlSoft.BedrockServerWrapper
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading;

    using AhlSoft.BedrockServerWrapper.Backups;
    using AhlSoft.BedrockServerWrapper.PlayerManagement;

    /// <summary>
    /// 
    /// </summary>
    public class InputOutputManager : IDisposable
    {
        private readonly ServerProcess _serverProcess;
        private readonly PlayerManager _playerManager;
        private readonly Log _log;
        private readonly Settings _settings;

        private CancellationTokenSource _cancellationTokenSource;
        private DateTime _serverStarting;
        
        /// <summary>
        /// Invoked when a backup is ready to be copied.
        /// </summary>
        public event EventHandler<BackupReadyEventArgs> BackupReady;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="serverProcess"></param>
        /// <param name="log"></param>
        /// <param name="playerManager"></param>
        public InputOutputManager(Log log, Settings settings, ServerProcess serverProcess, PlayerManager playerManager)
        {
            _log = log;
            _settings = settings;
            _serverProcess = serverProcess;
            _playerManager = playerManager;
            _serverStarting = DateTime.MinValue;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        internal void ReceivedStandardOutput(object sender, DataReceivedEventArgs e)
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
                    _log.Info($"Server started in {(DateTime.Now - _serverStarting).TotalMilliseconds} ms.");
                    return;
                }
            }

            if (e.Data.Contains("Saving..."))
            {
                _log.Info("Backup started...");
                _serverProcess.Say("Backup started.");

                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
                
                BackupReady += (s, a) =>
                {
                    _cancellationTokenSource.Cancel();
                };

                var thread = new Thread(() =>
                {
                    while (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        _serverProcess.SendInputToProcess("save query");
                        Thread.Sleep(1000);
                    }
                });
                thread.Start();

                return;
            }
            
            if (e.Data.Contains("level.dat"))
            {
                _cancellationTokenSource.Cancel();
                BackupReady?.Invoke(this, new BackupReadyEventArgs(e.Data, _playerManager.UsersOnline > 0));
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

                    _serverProcess.Say(timePlayed == -1
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
                
                _serverProcess.Say($"Goodbye {player}!");
            }

            if (e.Data.Contains("Difficulty: ") && !_serverProcess.ServerValues.ContainsKey("Difficulty"))
            {
                _serverProcess.ServerValues.Add("Difficulty", Regex.Match(e.Data, @".*Difficulty: \d (.*)").Groups[1].Value);
            }

            if (e.Data.Contains("Game mode: ") && !_serverProcess.ServerValues.ContainsKey("GameMode"))
            {
                _serverProcess.ServerValues.Add("GameMode", Regex.Match(e.Data, @".*Game mode: \d (.*)").Groups[1].Value);
            }

            if (e.Data.Contains("Level Name: ") && !_serverProcess.ServerValues.ContainsKey("LevelName"))
            {
                _settings.LevelName = Regex.Match(e.Data, @".*Level Name: (.*)").Groups[1].Value;
            }

            if (e.Data.Contains("Version") && !_serverProcess.ServerValues.ContainsKey("ServerVersion"))
            {
                _serverProcess.ServerValues["ServerVersion"] = Regex.Match(e.Data, @".*Version (\d+\.\d+\.\d+\.\d+)").Groups[1].Value;
            }

            if (e.Data.Contains("IPv4 supported") && !_serverProcess.ServerValues.ContainsKey("IpV4Port"))
            {
                _serverProcess.ServerValues["IpV4Port"] = Regex.Match(e.Data, @".*port: (\d*)").Groups[1].Value;
            }

            if (e.Data.Contains("IPv6 supported") && !_serverProcess.ServerValues.ContainsKey("IpV6Port"))
            {
                _serverProcess.ServerValues["IpV6Port"] = Regex.Match(e.Data, @".*port: (\d*)").Groups[1].Value;
            }

            _log.Info(e.Data);
        }

        internal void ReceivedErrorOutput(object sender, DataReceivedEventArgs e)
        {
            _log.Error(e.Data);
        }

        internal void BackupCompleted(object sender, BackupCompletedEventArgs args)
        {
            _serverProcess.SendInputToProcess("save resume");

            _serverProcess.Say(args.Successful
                ? $"Backup completed: {Path.GetFileName(args.BackupFile)} ({new FileInfo(args.BackupFile).Length / 1024 / 1024}MB), completed in {args.BackupDuration.TotalSeconds}s."
                : "Backup failed.");
        }

        /// <inheritdoc />
        public void Dispose()
        {
	        Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose all resources.
        /// </summary>
        /// <param name="disposing">Whether or not we're disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
	        if (disposing)
	        {
		        _cancellationTokenSource?.Dispose();
	        }
        }
    }
}