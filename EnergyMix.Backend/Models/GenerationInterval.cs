using System.Text.Json.Serialization;

namespace EnergyMix.Backend.Models;

public sealed class GenerationInterval
{
    [JsonPropertyName("from")]
    public DateTimeOffset From { get; init; }

    [JsonPropertyName("to")]
    public DateTimeOffset To { get; init; }

    [JsonPropertyName("generationmix")]
    public List<GenerationMixItem> GenerationMix { get; init; } = [];
}