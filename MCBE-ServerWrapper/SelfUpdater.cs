namespace AhlSoft.BedrockServerWrapper
{
    using System;
    using System.Linq;
    using System.Net;

    using AhlSoft.BedrockServerWrapper.Logging;
    
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Provides logic to find info about remote versions.
    /// </summary>
    public static class SelfUpdater
    {
        /// <summary>
        /// Check if there's a newer version.
        /// </summary>
        /// <param name="log">Logger to use.</param>
        /// <returns>True if an update exists.</returns>
        public static (bool updateAvailable, Version remoteVersion, string updateUrl) CheckForUpdate(ILog log)
        {
            const string baseUrl = "https://artifactory.ahlgren.io/artifactory/api/storage/mcbesw/master";

            try
            {
                using var wc = new WebClient();

                var master = JObject.Parse(wc.DownloadString(baseUrl));
                var latest = master["children"]?.Children()
                    .OrderBy(c => c["uri"])
                    .Last();

                if (latest?["uri"] == null)
                {
                    throw new Exception("Unable to determine download URL.");
                }

                var remoteVersion = Version.Parse(latest["uri"].Value<string>()?[1..] ?? string.Empty);
                var localVersion = Version.Parse(Utils.ProgramVersion);

                var downloadInfo = JObject.Parse(wc.DownloadString($"{baseUrl}/{latest["uri"].Value<string>()}/{(Utils.IsLinux() ? "MCBSW" : "MCBSW.exe")}"));

                if (downloadInfo["downloadUri"] == null)
                {
                    throw new Exception("Unable to determine download URL.");
                }

                return (remoteVersion > localVersion, remoteVersion, $"https{downloadInfo["downloadUri"].Value<string>()}");
            }
            catch (Exception e)
            {
                log.Error($"Couldn't determine latest available version: {e.GetType()} - {e.Message}");
                return (false, null, null);
            }
        }
    }
}
