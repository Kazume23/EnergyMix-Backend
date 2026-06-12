namespace EnergyMix.Backend.Dtos.Responses;

public sealed class DailyEnergyMixResponseDto
{
    public DateOnly Date { get; init; }
    public List<EnergySourceShareResponseDto> Sources { get; init; } = [];
    public decimal CleanEnergyPercentage { get; init; }
}
