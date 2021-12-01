namespace AhlSoft.BedrockServerWrapper
{
    using System;
    using System.Linq;
    using System.Net.Http;

    using AhlSoft.BedrockServerWrapper.Logging;
    
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
        /// <param name="log">Logger to use.</param>
        /// <returns>True if an update exists.</returns>
        public (bool updateAvailable, Version remoteVersion, string updateUrl) CheckForUpdate()
        {
            const string baseUrl = "https://artifactory.ahlgren.io/artifactory/api/storage/mcbesw/master";

            try
            {
                var master = JObject.Parse(_httpClient.GetStringAsync(baseUrl).Result);
                var latest = master["children"]?.Children()
                    .OrderBy(c => c["uri"])
                    .Last();

                if (latest?["uri"] == null)
                {
                    throw new Exception("Unable to determine download URL.");
                }

                var remoteVersion = Version.Parse(latest["uri"].Value<string>()?[1..] ?? string.Empty);
                var localVersion = Version.Parse(Utils.ProgramVersion);

                var downloadInfo = JObject.Parse(_httpClient.GetStringAsync($"{baseUrl}/{latest["uri"].Value<string>()}/{(Utils.IsLinux() ? "MCBSW" : "MCBSW.exe")}").Result);

                if (downloadInfo["downloadUri"] == null)
                {
                    throw new Exception("Unable to determine download URL.");
                }

                return (remoteVersion > localVersion, remoteVersion, $"https{downloadInfo["downloadUri"].Value<string>()}");
            }
            catch (Exception e)
            {
                _log.Error($"Couldn't determine latest available version: {e.GetType()} - {e.Message}");
                return (false, null, null);
            }
        }
    }
}
