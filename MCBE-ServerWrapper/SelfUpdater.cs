namespace AhlSoft.BedrockServerWrapper;

using System;
using System.Net.Http;

using AhlSoft.BedrockServerWrapper.Logging;

/// <summary>
/// Provides logic to find info about remote versions.
/// </summary>
public class SelfUpdater
{
    private readonly ILog _log;
    private readonly HttpClient _httpClient;

    public SelfUpdater(ILog log, HttpClient httpClient)
    {
        _log = log;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Check if there's a newer version.
    /// </summary>
    /// <returns>True if an update exists.</returns>
    public (bool updateAvailable, Version remoteVersion, string updateUrl) CheckForUpdate()
    {
        // TODO: Change to GitHub releases

        _log.Warning("Unable to check for newer version of MCBSW.");

        return (false, null, null);
    }
}
