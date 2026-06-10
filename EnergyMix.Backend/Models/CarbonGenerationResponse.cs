using EnergyMix.Backend.Models;
using System.Text.Json.Serialization;

namespace Energy.Mix.Backend.Models;
public class CarbonGenerationResponse
{
    [JsonPropertyName("data")]
    public List<GenerationInterval> Data { get; init; } = [];

}
