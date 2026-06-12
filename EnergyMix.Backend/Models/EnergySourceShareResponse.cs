namespace EnergyMix.Backend.Models;

public sealed class EnergySourceShareResponse
{
    public string Fuel { get; init; } = string.Empty;
    public decimal Percentage { get; init; }
}