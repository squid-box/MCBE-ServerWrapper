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
        /// <param name="player"><see cref="Player"/> that has logged in.</param>
        public void PlayerLoggedIn(Player player)
        {
            if (_online.ContainsKey(player))
            {
                Console.Error.WriteLine($"Player \"{player}\" already logged in!");
                _online.Remove(player);
            }

            _online.Add(player, DateTime.UtcNow);
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
        public void PlayerLoggedOut(Player player)
        {
            if (!_online.ContainsKey(player))
            {
                Console.Error.WriteLine($"Player \"{player}\" was not logged in!");
                return;
            }

            if (!_timeLog.ContainsKey(player.Name))
            {
                _timeLog.Add(player.Name, 0);
            }

            _timeLog[player.Name] += Convert.ToInt32(Math.Ceiling((DateTime.UtcNow - _online[player]).TotalMinutes));
            _online.Remove(player);

            SaveTimeLog();
        }

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
