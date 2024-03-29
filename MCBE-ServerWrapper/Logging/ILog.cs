﻿namespace AhlSoft.BedrockServerWrapper.Logging;

using System;

/// <summary>
/// Logging class.
/// </summary>
public interface ILog
{
    /// <summary>
    /// Log a message at the Info level.
    /// </summary>
    /// <param name="message">Message to log.</param>
    /// <param name="color">Optional color styling override.</param>
    /// <param name="logToConsole">Whether or not message gets logged to console.</param>
    public void Info(string message, string color = "green", bool logToConsole = true);

    /// <summary>
    /// Log a message at the Warning level.
    /// </summary>
    /// <param name="message">Message to log.</param>
    /// <param name="color">Optional color styling override.</param>
    /// <param name="logToConsole">Whether or not message gets logged to console.</param>
    public void Warning(string message, string color = "yellow", bool logToConsole = true);

    /// <summary>
    /// Log a message at the Error level.
    /// </summary>
    /// <param name="message">Message to log.</param>
    /// <param name="color">Optional color styling override.</param>
    /// <param name="logToConsole">Whether or not message gets logged to console.</param>
    public void Error(string message, string color = "red", bool logToConsole = true);

    /// <summary>
    /// Log an Exception (and any inner Exceptions).
    /// </summary>
    /// <param name="exception">Exception to log.</param>
    /// <param name="color">Optional color styling override.</param>
    /// <param name="logToConsole">Whether or not message gets logged to console.</param>
    public void Exception(Exception exception, string color = "maroon", bool logToConsole = true);
}
