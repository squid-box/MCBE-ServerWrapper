namespace AhlSoft.BedrockServerWrapper.PlayerManagement;

using System.Collections.Generic;
using System;
using System.Text.Json.Serialization;

/// <summary>
/// Default context used for serializing/deserializing in trimmed compilation.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, DateTime>))]
internal partial class PlayerSeenLogContext : JsonSerializerContext
{
}