namespace AhlSoft.BedrockServerWrapper.SelfUpdating;

using System;
using System.Net.Http;
using System.Text.Json;
using AhlSoft.BedrockServerWrapper.Logging;
using System.Text.RegularExpressions;

/// <summary>
/// Provides logic to find info about remote versions.
/// </summary>
public class SelfUpdater
{
    private readonly ILog _log;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Creates a new <see cref="SelfUpdater" />.
    /// </summary>
    /// <param name="log">The log.</param>
    /// <param name="httpClient">The HTTP client.</param>
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
        var target = Utils.IsLinux() ? "_linux-x64.zip" : "_win-x64.zip";

        var latestReleaseJson = _httpClient.GetStringAsync(@"https://api.github.com/repos/squid-box/MCBE-ServerWrapper/releases/latest").Result;

        var match = Regex.Match(latestReleaseJson, $"^.*browser_download_url.*(https://.*{target})\".*$");

        if (!match.Success)
        {
            _log.Warning("Could not find the latest release of MCBSW.");
            return (false, null, null);
        }

        try
        {
            var release = JsonSerializer.Deserialize(latestReleaseJson, GitHubReleaseMetaDataContext.Default.GitHubReleaseMetaData);

            if (!string.IsNullOrEmpty(release?.TagName))
            {
                var remoteVersion = Version.Parse(release.TagName[1..]);

                return (remoteVersion > Version.Parse(Utils.ProgramVersion), remoteVersion, match.Groups[1].Value);
            }

            return (false, null, match.Groups[1].Value);
        }
        catch (Exception exception)
        {
            _log.Warning("Could not find the latest release of MCBSW.");
            _log.Warning($"{exception.GetType()} : {exception.Message}", logToConsole: false);

            return (false, null, null);
        }
    }
}
