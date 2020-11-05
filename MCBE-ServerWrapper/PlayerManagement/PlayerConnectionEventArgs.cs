namespace BedrockServerWrapper.PlayerManagement
{
    using System;

    /// <summary>
    /// Information about a player connecting.
    /// </summary>
    public class PlayerConnectionEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new <see cref="PlayerConnectionEventArgs"/>.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> that has conected.</param>
        public PlayerConnectionEventArgs(Player player)
        {
            Player = player;
        }

        /// <summary>
        /// The <see cref="Player"/> that has connected.
        /// </summary>
        public Player Player { get; }
    }
}