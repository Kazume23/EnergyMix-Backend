namespace EnergyMix.Backend.Models;

public sealed class DailyEnergyMixResponse
{
    public DateOnly Date { get; init; }
    public List<EnergySourceShareResponse> Sources { get; init; } = [];
    public decimal CleanEnergyPercentage { get; init; }
}   