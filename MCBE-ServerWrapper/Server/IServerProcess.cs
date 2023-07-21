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

    /// <summary>
    /// Gets a value indicating whether the underlying server process is running or not.
    /// </summary>
    public bool IsRunning { get; }

    /// <summary>
    /// Sends given input to the underlying server process.
    /// </summary>
    /// <param name="input">Input to send.</param>
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
