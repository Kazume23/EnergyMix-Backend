using System.Text.Json.Serialization;

namespace EnergyMix.Backend.Models;
public sealed class CarbonGenerationResponse
{
    [JsonPropertyName("data")]
    public List<GenerationInterval> Data { get; init; } = [];

}
