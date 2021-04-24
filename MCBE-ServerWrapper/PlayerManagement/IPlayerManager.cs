namespace AhlSoft.BedrockServerWrapper.PlayerManagement
{
    using System;

    /// <summary>
    /// Manages player activity.
    /// </summary>
    public interface IPlayerManager
    {
        /// <summary>
        /// Invoked whenever a player joins the game.
        /// </summary>
        public event EventHandler<PlayerConnectionEventArgs> PlayerConnected;

        /// <summary>
        /// Gets number of minutes a <see cref="Player"/> has spent on this server.
        /// </summary>
        /// <param name="player"><see cref="Player"/> to check played time for.</param>
        /// <returns>Number of minutes the <see cref="Player"/> has spent on this server.</returns>
        public int GetPlayedMinutes(Player player);

        /// <summary>
        /// Gets the last time a given player was seen on the server.
        /// </summary>
        /// <param name="player"><see cref="Player"/> to check last activity for.</param>
        /// <returns>The timestamp of when the player last logged out.</returns>
        public DateTime GetLastSeen(Player player);

        /// <summary>
        /// Called when a <see cref="Player"/> logs out.
        /// </summary>
        public void PlayerLeft(Player player);

        /// <summary>
        /// Called when a <see cref="Player"/> logs in.
        /// </summary>
        public void PlayerJoined(Player player);

        /// <summary>
        /// Gets the number of currently online users.
        /// </summary>
        public int UsersOnline { get; }
    }
}