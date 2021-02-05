namespace BedrockServerWrapper.PlayerManagement
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    /// <summary>
    /// Manages player activity.
    /// </summary>
    public class PlayerManager
    {
        private const string PlayerTimeLogFile = @"playertime.json";
        private const string PlayerSeenLogFile = @"playerseen.json";
        private readonly Log _log;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        public PlayerManager(Log log)
        {
            _log = log;
            _online = new Dictionary<Player, DateTime>();
            _timeLog = LoadTimeLog();
            _lastSeenLog = LoadSeenLog();
        }

        /// <summary>
        /// Collection of currently online <see cref="Player"/> and when they logged in.
        /// </summary>
        private readonly Dictionary<Player, DateTime> _online;

        private readonly Dictionary<string, DateTime> _lastSeenLog;

        /// <summary>
        /// Log of how many minutes <see cref="Player"/>s have played so far.
        /// </summary>
        private readonly Dictionary<string, int> _timeLog;

        /// <summary>
        /// Called when a <see cref="Player"/> logs in.
        /// </summary>
        internal void PlayerConnected(object sender, PlayerConnectionEventArgs args)
        {
            if (_online.ContainsKey(args.Player))
            {
                _log.Warning($"Player \"{args.Player}\" already logged in!");
                _online.Remove(args.Player);
            }

            _online.Add(args.Player, DateTime.UtcNow);
        }

        /// <summary>
        /// Gets number of minutes a <see cref="Player"/> has spent on this server.
        /// </summary>
        /// <param name="player"><see cref="Player"/> to check played time for.</param>
        /// <returns>Number of minutes the <see cref="Player"/> has spent on this server.</returns>
        internal int GetPlayedMinutes(Player player)
        {
            if (!_timeLog.ContainsKey(player.Name))
            {
                return -1;
            }

            return _timeLog[player.Name];
        }

        internal DateTime GetLastSeen(Player player)
        {
            if (!_lastSeenLog.ContainsKey(player.Name))
            {
                return DateTime.MinValue;
            }

            return _lastSeenLog[player.Name];
        }

        /// <summary>
        /// Called when a <see cref="Player"/> logs out.
        /// </summary>
        internal void PlayerDisconnected(object sender, PlayerConnectionEventArgs args)
        {
            if (!_online.ContainsKey(args.Player))
            {
                _log.Warning($"Player \"{args.Player}\" was not logged in!");
                return;
            }

            if (!_timeLog.ContainsKey(args.Player.Name))
            {
                _timeLog.Add(args.Player.Name, 0);
            }

            _lastSeenLog[args.Player.Name] = DateTime.Now;

            _timeLog[args.Player.Name] += Convert.ToInt32(Math.Ceiling((DateTime.UtcNow - _online[args.Player]).TotalMinutes));
            _online.Remove(args.Player);

            SaveTimeLog();
            SaveSeenLog();
        }

        /// <summary>
        /// Gets the number of currently online users.
        /// </summary>
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
            File.WriteAllText(PlayerTimeLogFile, JsonConvert.SerializeObject(_timeLog));
        }

        private void SaveSeenLog()
        {
            File.WriteAllText(PlayerSeenLogFile, JsonConvert.SerializeObject(_lastSeenLog));
        }
    }
}