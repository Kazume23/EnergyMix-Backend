namespace EnergyMix.Backend.Models;

public sealed class OptimalChargingWindowResponse
{
    public DateTimeOffset Start { get; init; }
    public DateTimeOffset End { get; init; }
    public decimal AverageCleanEnergyPercentage { get; init; }
}