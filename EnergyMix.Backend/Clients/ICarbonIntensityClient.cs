using EnergyMix.Backend.Dtos.CarbonApi;

namespace EnergyMix.Backend.Clients;

public interface ICarbonIntensityClient
{
    Task<CarbonGenerationResponseDto> GetGenerationAsync(
        DateTimeOffset startDateUtc,
        DateTimeOffset endDateUtc,
        CancellationToken cancellationToken = default);
}
