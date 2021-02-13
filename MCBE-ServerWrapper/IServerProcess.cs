namespace AhlSoft.BedrockServerWrapper
{
    using System;
    using System.Collections.Generic;
    using AhlSoft.BedrockServerWrapper.Backups;

    /// <summary>
    /// Wraps the Bedrock server process.
    /// </summary>
    public interface IServerProcess : IDisposable
    {
        /// <summary>
        /// Contains any properties and values related to the server.
        /// </summary>
        public Dictionary<string, string> ServerValues { get; }

        public bool IsRunning { get; }

        public void SendInputToProcess(string input);

        public void Say(string message);

        /// <summary>
        /// Initiates a backup.
        /// </summary>
        public void Backup();

        public void Start();

        public void Stop();

        public void PrintServerValues();
    }
}