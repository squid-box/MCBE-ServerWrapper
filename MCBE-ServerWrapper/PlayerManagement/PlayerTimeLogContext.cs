namespace AhlSoft.BedrockServerWrapper.PlayerManagement;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Default context used for serializing/deserializing in trimmed compilation.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, int>))]
internal partial class PlayerTimeLogContext : JsonSerializerContext
{
}