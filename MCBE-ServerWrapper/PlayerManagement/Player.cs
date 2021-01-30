namespace BedrockServerWrapper.PlayerManagement
{
    using System;

    /// <summary>
    /// Represents a player on the server.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Creates a new <see cref="Player"/>.
        /// </summary>
        /// <param name="name">Name of the <see cref="Player"/>.</param>
        /// <param name="xuid">XUID of the <see cref="Player"/>.</param>
        public Player(string name, string xuid)
        {
            Name = name;
            Xuid = xuid;
        }

        /// <summary>
        /// Name of the <see cref="Player"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// XUID of the <see cref="Player"/>.
        /// </summary>
        public string Xuid { get; }

        /// <summary>
        /// Gets a unique hash code for this <see cref="Player"/>.
        /// </summary>
        /// <returns>A unique hash code for this <see cref="Player"/>.</returns>
        public override int GetHashCode()
        {
            return (Name+Xuid).GetHashCode();
        }

        /// <summary>
        /// Checks if this <see cref="Player"/> is equal to another <see cref="Player"/>.
        /// </summary>
        /// <param name="obj"><see cref="Player"/> to compare this <see cref="Player"/> against.</param>
        /// <returns>True if equal, otherwise false.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is Player other)
            {
	            return Name.Equals(other.Name, StringComparison.Ordinal) && Xuid.Equals(other.Xuid, StringComparison.Ordinal);
            }

            return false;
        }

        /// <summary>
        /// Gets a string representation of this <see cref="Player"/>.
        /// </summary>
        /// <returns>The name of this player.</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Determines whether or not two <see cref="Player"/>s are equal.
        /// </summary>
        /// <param name="player1">First <see cref="Player"/>.</param>
        /// <param name="player2">Second <see cref="Player"/>.</param>
        /// <returns>True if <see cref="Player"/>s are equal, otherwise false.</returns>
        public static bool operator ==(Player player1, Player player2)
        {
	        return !(player1 == null) && player1.Equals(player2);
        }

        /// <summary>
        /// Determines whether or not two <see cref="Player"/>s are not equal.
        /// </summary>
        /// <param name="player1">First <see cref="Player"/>.</param>
        /// <param name="player2">Second <see cref="Player"/>.</param>
        /// <returns>True if <see cref="Player"/>s are unequal, otherwise false.</returns>
        public static bool operator !=(Player player1, Player player2)
        {
            return !(player1 is null) && !player1.Equals(player2);
        }
    }
}