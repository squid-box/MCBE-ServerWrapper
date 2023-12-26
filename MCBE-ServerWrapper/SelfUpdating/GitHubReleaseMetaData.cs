namespace AhlSoft.BedrockServerWrapper.SelfUpdating;

using System.Text.Json.Serialization;

/// <summary>
/// Incomplete data structure of GitHub release info.
/// </summary>
public class GitHubReleaseMetaData
{
    /// <summary>
    /// The tag name for a release.
    /// </summary>
    [JsonPropertyName("tag_name")]
    public string TagName { get; set; }
}