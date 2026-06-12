using System.Text.Json.Serialization;

namespace EnergyMix.Backend.Dtos.CarbonApi;

public sealed class CarbonGenerationResponseDto
{
    [JsonPropertyName("data")]
    public List<GenerationIntervalDto> Data { get; init; } = [];
}
