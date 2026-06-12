using EnergyMix.Backend.Constants;
using System.ComponentModel.DataAnnotations;

namespace EnergyMix.Backend.Dtos.Requests;

public sealed class OptimalChargingWindowQueryDto
{
    [Range(EnergyMixConstants.MinChargingHours, EnergyMixConstants.MaxChargingHours, ErrorMessage = "Hours must be between 1 and 6.")]
    public int Hours { get; init; }
}
