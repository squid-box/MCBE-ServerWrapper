namespace AhlSoft.BedrockServerWrapper.Settings;

using System.Text.Json.Serialization;

/// <summary>
/// Default context used for serializing/deserializing in trimmed compilation.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SettingsProvider))]
internal partial class SettingsProviderContext : JsonSerializerContext
{
}