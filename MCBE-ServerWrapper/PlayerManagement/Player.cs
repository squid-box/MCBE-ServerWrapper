﻿namespace MCBE_ServerWrapper.PlayerManagement
{
    /// <summary>
    /// Represents a player on the server.
    /// </summary>
    public struct Player
    {
        /// <summary>
        /// Creeates a new <see cref="Player"/>.
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

            var other = (Player)obj;
            return Name.Equals(other.Name) && Xuid.Equals(other.Xuid);
        }

        /// <summary>
        /// Gets a string representation of this <see cref="Player"/>.
        /// </summary>
        /// <returns>The name of this player.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}