namespace AhlSoft.BedrockServerWrapper.Server;

using System;
using System.Collections.Generic;

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

    /// <summary>
    /// Starts the server process.
    /// </summary>
    public void Start();

    /// <summary>
    /// Stops the server process.
    /// </summary>
    public void Stop();
}
