namespace BedrockServerWrapper.PlayerManagement
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    public class PlayerManager
    {
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
        private readonly Dictionary<Player, int> _timeLog;

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
            if (!_timeLog.ContainsKey(player))
            {
                return -1;
            }

            return _timeLog[player];
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

            if (!_timeLog.ContainsKey(player))
            {
                _timeLog.Add(player, 0);
            }

            _timeLog[player] += Convert.ToInt32(Math.Ceiling((DateTime.UtcNow - _online[player]).TotalMinutes));
            _online.Remove(player);
        }

        private Dictionary<Player, int> LoadTimeLog()
        {
            // TODO: Add persistence between sessions.
            return new Dictionary<Player, int>();
        }
    }
}
