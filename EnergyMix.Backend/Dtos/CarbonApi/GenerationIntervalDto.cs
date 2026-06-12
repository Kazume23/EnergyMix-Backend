using System.Text.Json.Serialization;

namespace EnergyMix.Backend.Dtos.CarbonApi;

public sealed class GenerationIntervalDto
{
    [JsonPropertyName("from")]
    public DateTimeOffset From { get; init; }

    [JsonPropertyName("to")]
    public DateTimeOffset To { get; init; }

    [JsonPropertyName("generationmix")]
    public List<GenerationMixItemDto> GenerationMix { get; init; } = [];
}
