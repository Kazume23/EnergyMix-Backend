namespace EnergyMix.Backend.Dtos.Responses;

public sealed class EnergySourceShareResponseDto
{
    public string Fuel { get; init; } = string.Empty;
    public decimal Percentage { get; init; }
}
