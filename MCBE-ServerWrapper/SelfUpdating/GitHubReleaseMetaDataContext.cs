namespace AhlSoft.BedrockServerWrapper.SelfUpdating;

using System.Text.Json.Serialization;

/// <summary>
/// Default context used for serializing/deserializing in trimmed compilation.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(GitHubReleaseMetaData))]
internal partial class GitHubReleaseMetaDataContext : JsonSerializerContext
{
}