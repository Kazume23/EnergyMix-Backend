namespace EnergyMix.Backend.Dtos.Responses;

public sealed class OptimalChargingWindowResponseDto
{
    public DateTimeOffset Start { get; init; }
    public DateTimeOffset End { get; init; }
    public decimal AverageCleanEnergyPercentage { get; init; }
    public List<EnergySourceShareResponseDto> Sources { get; init; } = [];
}
