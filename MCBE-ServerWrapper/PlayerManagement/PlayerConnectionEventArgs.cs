namespace BedrockServerWrapper.PlayerManagement
{
    using System;

    public class PlayerConnectionEventArgs : EventArgs
    {
        public PlayerConnectionEventArgs(Player player)
        {
            Player = player;
        }

        public Player Player { get; }
    }
}
