namespace BedrockServerWrapper
{
    using System;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using System.Threading;

    using Backups;
    using PlayerManagement;

    /// <summary>
    /// 
    /// </summary>
    public class InputOutputManager
    {
        private readonly ServerProcess _serverProcess;
        private readonly PlayerManager _playerManager;
        private readonly ConsoleColor _defaultConsoleColor;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private DateTime _serverStarting;

        #region Events
        
        /// <summary>
        /// Invoked when a backup is ready to be copied.
        /// </summary>
        public event EventHandler<BackupReadyArguments> BackupReady;

        /// <summary>
        /// Invoked when a backup is complete.
        /// </summary>
        public event EventHandler BackupComplete;


        public event EventHandler<PlayerConnectionEventArgs> PlayerJoined;

        public event EventHandler<PlayerConnectionEventArgs> PlayerDisconnected;

        #endregion

        public InputOutputManager(ServerProcess serverProcess)
        {
            _serverProcess = serverProcess;
            _playerManager = new PlayerManager();
            _serverStarting = DateTime.MinValue;
            _defaultConsoleColor = Console.ForegroundColor;
            _cancellationTokenSource = new CancellationTokenSource();

            PlayerJoined += _playerManager.PlayerConnected;
            PlayerDisconnected += _playerManager.PlayerDisconnected;
        }

        public void ReceivedStandardOutput(object sender, DataReceivedEventArgs e)
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
                    Console.WriteLine($"Server started in {(DateTime.Now - _serverStarting).TotalMilliseconds} ms.");
                    return;
                }
            }

            if (e.Data.Contains("Saving..."))
            {
                Console.WriteLine("Backup started...");
                _serverProcess.Say("Backup started.");

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
                BackupReady?.Invoke(this, new BackupReadyArguments(e.Data, _playerManager.UsersOnline > 0));
                return;
            }

            if (e.Data.Contains("Changes to the level are resumed."))
            {
                Console.Out.WriteLine("Backup completed.");
                _serverProcess.Say("Backup completed.");
                _cancellationTokenSource.Cancel();
                BackupComplete?.Invoke(this, null);
                return;
            }

            if (e.Data.Contains("Player connected"))
            {
                var playerData = Regex.Match(e.Data, @".* Player connected: (.*), xuid: (.*)");
                var player = new Player(playerData.Groups[1].Value, playerData.Groups[2].Value);
                PlayerJoined?.Invoke(this, new PlayerConnectionEventArgs(player));
                
                var timePlayed = _playerManager.GetPlayedMinutes(player);

                var thread = new Thread(() =>
                {
                    Thread.Sleep(5000);

                    _serverProcess.Say(timePlayed == -1
                        ? $"Welcome {player}!"
                        : $"Welcome back {player}, you've played {timePlayed} minutes so far.");
                });
                thread.Start();
            }

            if (e.Data.Contains("Player disconnected"))
            {
                var playerData = Regex.Match(e.Data, @".* Player disconnected: (.*), xuid: (.*)");
                var player = new Player(playerData.Groups[1].Value, playerData.Groups[2].Value);
                PlayerDisconnected?.Invoke(this, new PlayerConnectionEventArgs(player));
                
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
                _serverProcess.ServerValues.Add("LevelName", Regex.Match(e.Data, @".*Level Name: (.*)").Groups[1].Value);
            }

            if (e.Data.Contains("Version") && !_serverProcess.ServerValues.ContainsKey("ServerVersion"))
            {
                _serverProcess.ServerValues["ServerVersion"] = Regex.Match(e.Data, @".*Version (\d\.\d\.\d\.\d)").Groups[1].Value;
            }

            if (e.Data.Contains("IPv4 supported") && !_serverProcess.ServerValues.ContainsKey("IpV4Port"))
            {
                _serverProcess.ServerValues["IpV4Port"] = Regex.Match(e.Data, @".*port: (\d*)").Groups[1].Value;
            }

            if (e.Data.Contains("IPv6 supported") && !_serverProcess.ServerValues.ContainsKey("IpV6Port"))
            {
                _serverProcess.ServerValues["IpV6Port"] = Regex.Match(e.Data, @".*port: (\d*)").Groups[1].Value;
            }

            Console.WriteLine(e.Data);
        }

        public void ReceivedErrorOutput(object sender, DataReceivedEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Data);
            Console.ForegroundColor = _defaultConsoleColor;
        }
    }
}
