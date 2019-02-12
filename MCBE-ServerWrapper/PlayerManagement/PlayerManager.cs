namespace BedrockServerWrapper.PlayerManagement
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;

    /// <summary>
    /// 
    /// </summary>
    public class PlayerManager
    {
        private const string PlayerLogFile = @"playerlog.json";

        public PlayerManager()
        {
            _online = new Dictionary<Player, DateTime>();
            _timeLog = LoadTimeLog();
        }

        /// <summary>
        /// Collection of currently online <see cref="Player"/> and when they logged in.
        /// </summary>
        private readonly Dictionary<Player, DateTime> _online;

        /// <summary>
        /// Log of how many minutes <see cref="Player"/>s have played so far.
        /// </summary>
        private readonly Dictionary<string, int> _timeLog;

        /// <summary>
        /// Called when a <see cref="Player"/> logs in.
        /// </summary>
        public void PlayerConnected(object sender, PlayerConnectionEventArgs args)
        {
            if (_online.ContainsKey(args.Player))
            {
                Console.Error.WriteLine($"Player \"{args.Player}\" already logged in!");
                _online.Remove(args.Player);
            }

            _online.Add(args.Player, DateTime.UtcNow);
        }

        /// <summary>
        /// Gets number of minutes a <see cref="Player"/> has spent on this server.
        /// </summary>
        /// <param name="player"><see cref="Player"/> to check played time for.</param>
        /// <returns>Number of minutes the <see cref="Player"/> has spent on this server.</returns>
        public int GetPlayedMinutes(Player player)
        {
            if (!_timeLog.ContainsKey(player.Name))
            {
                return -1;
            }

            return _timeLog[player.Name];
        }

        /// <summary>
        /// Called when a <see cref="Player"/> logs out.
        /// </summary>
        /// <param name="player"><see cref="Player"/> that logs out.</param>
        public void PlayerDisconnected(object sender, PlayerConnectionEventArgs args)
        {
            if (!_online.ContainsKey(args.Player))
            {
                Console.Error.WriteLine($"Player \"{args.Player}\" was not logged in!");
                return;
            }

            if (!_timeLog.ContainsKey(args.Player.Name))
            {
                _timeLog.Add(args.Player.Name, 0);
            }

            _timeLog[args.Player.Name] += Convert.ToInt32(Math.Ceiling((DateTime.UtcNow - _online[args.Player]).TotalMinutes));
            _online.Remove(args.Player);

            SaveTimeLog();
        }

        /// <summary>
        /// Gets the number of currently online users.
        /// </summary>
        public int UsersOnline => _online.Count;

        private Dictionary<string, int> LoadTimeLog()
        {
            if (File.Exists(PlayerLogFile))
            {
                Console.Out.WriteLine("Loading player log from file.");
                return JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(PlayerLogFile));
            }
            else
            {
                Console.Out.WriteLine("No player log found, creating new.");
                return new Dictionary<string, int>();
            }
            
        }

        private void SaveTimeLog()
        {
            File.WriteAllText(PlayerLogFile, JsonConvert.SerializeObject(_timeLog));
        }
    }
}
