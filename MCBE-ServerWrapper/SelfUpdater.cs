namespace AhlSoft.BedrockServerWrapper;

using System;
using System.IO.Compression;
using System.IO;
using System.Net.Http;

using AhlSoft.BedrockServerWrapper.Logging;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json.Linq;

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
        var target = Utils.IsLinux() ? "-linux64.zip" : "-win64.zip";

        var latestReleaseJson = _httpClient.GetStringAsync(@"https://api.github.com/repos/squid-box/MCBE-ServerWrapper/releases/latest").Result;

        var match = Regex.Match(latestReleaseJson, $"^.*browser_download_url.*(https://.*{target})\".*$");

        if (!match.Success)
        {
            _log.Warning("Could not find the latest release of MCBSW.");
            return (false, null, null);
        }

        try
        {
            dynamic release = JObject.Parse(latestReleaseJson);

            var tag = release.tag_name as string;
            var remoteVersion = Version.Parse(tag.AsSpan(1));

            if (remoteVersion != null)
            {
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
