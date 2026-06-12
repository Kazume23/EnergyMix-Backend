using System.ComponentModel.DataAnnotations;

namespace EnergyMix.Backend.Dtos.Requests;

public sealed class OptimalChargingWindowQueryDto
{
    [Range(1, 6, ErrorMessage = "Hours must be between 1 and 6.")]
    public int Hours { get; init; }
}
