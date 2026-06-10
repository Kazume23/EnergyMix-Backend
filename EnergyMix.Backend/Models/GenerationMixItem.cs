using System.Text.Json.Serialization;

namespace EnergyMix.Backend.Models;

public class GenerationMixItem
{
    [JsonPropertyName("fuel")]
    public string Fuel { get; init; } = string.Empty;

    [JsonPropertyName("perc")]
    public decimal Percentage { get; init; }
}