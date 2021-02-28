namespace AhlSoft.BedrockServerWrapper.PlayerManagement
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using AhlSoft.BedrockServerWrapper.Logging;

    using Newtonsoft.Json;

    /// <inheritdoc cref="IPlayerManager" />
    public class PlayerManager : IPlayerManager
    {
        private const string PlayerTimeLogFile = @"playertime.json";
        private const string PlayerSeenLogFile = @"playerseen.json";
        private readonly ILog _log;

        public PlayerManager(ILog log)
        {
            _log = log;
            _online = new Dictionary<Player, DateTime>();
            _timeLog = LoadTimeLog();
            _lastSeenLog = LoadSeenLog();
        }

        /// <inheritdoc />
        public event EventHandler<PlayerConnectionEventArgs> PlayerConnected;

        /// <inheritdoc />
        public event EventHandler<PlayerConnectionEventArgs> PlayerDisconnected;

        /// <summary>
        /// Collection of currently online <see cref="Player"/> and when they logged in.
        /// </summary>
        private readonly Dictionary<Player, DateTime> _online;

        private readonly Dictionary<string, DateTime> _lastSeenLog;

        /// <summary>
        /// Log of how many minutes <see cref="Player"/>s have played so far.
        /// </summary>
        private readonly Dictionary<string, int> _timeLog;

        /// <inheritdoc />
        public int GetPlayedMinutes(Player player)
        {
            if (!_timeLog.ContainsKey(player.Name))
            {
                return -1;
            }

            return _timeLog[player.Name];
        }

        /// <inheritdoc />
        public DateTime GetLastSeen(Player player)
        {
            return !_lastSeenLog.ContainsKey(player.Name) ? 
                DateTime.MinValue : 
                _lastSeenLog[player.Name];
        }

        /// <inheritdoc />
        public void PlayerLeft(Player player)
        {
            if (!_online.ContainsKey(player))
            {
                _log.Warning($"Player \"{player}\" was not logged in!");
                return;
            }

            PlayerDisconnected?.Invoke(this, new PlayerConnectionEventArgs(player));

            if (!_timeLog.ContainsKey(player.Name))
            {
                _timeLog.Add(player.Name, 0);
            }

            _lastSeenLog[player.Name] = DateTime.Now;

            _timeLog[player.Name] += Convert.ToInt32(Math.Ceiling((DateTime.UtcNow - _online[player]).TotalMinutes));
            _online.Remove(player);

            SaveTimeLog();
            SaveSeenLog();
        }

        /// <inheritdoc />
        public void PlayerJoined(Player player)
        {
            if (_online.ContainsKey(player))
            {
                _log.Warning($"Player \"{player}\" already logged in!");
                _online.Remove(player);
            }

            _online.Add(player, DateTime.UtcNow);

            PlayerConnected?.Invoke(this, new PlayerConnectionEventArgs(player));
        }

        /// <inheritdoc />
        public int UsersOnline => _online.Count;

        private Dictionary<string, int> LoadTimeLog()
        {
            if (File.Exists(PlayerTimeLogFile))
            {
                _log.Info("Loading player time log from file.");
                return JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(PlayerTimeLogFile));
            }

            _log.Info("No player time log found, creating new.");
            return new Dictionary<string, int>();
        }

        private Dictionary<string, DateTime> LoadSeenLog()
        {
            if (File.Exists(PlayerSeenLogFile))
            {
                _log.Info("Loading player seen log from file.");
                return JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(File.ReadAllText(PlayerSeenLogFile));
            }

            _log.Info("No player seen log found, creating new.");
            return new Dictionary<string, DateTime>();
        }

        private void SaveTimeLog()
        {
            File.WriteAllText(PlayerTimeLogFile, JsonConvert.SerializeObject(_timeLog, Formatting.Indented));
        }

        private void SaveSeenLog()
        {
            File.WriteAllText(PlayerSeenLogFile, JsonConvert.SerializeObject(_lastSeenLog, Formatting.Indented));
        }
    }
}