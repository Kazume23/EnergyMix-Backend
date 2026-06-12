using EnergyMix.Backend.Dtos.CarbonApi;
using System.Net.Http.Json;

namespace EnergyMix.Backend.Clients;

public sealed class CarbonIntensityClient
{
    private readonly HttpClient _httpClient;

    public CarbonIntensityClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CarbonGenerationResponseDto> GetGenerationAsync(
        DateTimeOffset startDateUtc,
        DateTimeOffset endDateUtc)
    {
        var startDateText = startDateUtc.UtcDateTime.ToString("yyyy-MM-ddTHH:mmZ");
        var endDateText = endDateUtc.UtcDateTime.ToString("yyyy-MM-ddTHH:mmZ");
        var generationEndpointUrl = $"generation/{startDateText}/{endDateText}";

        var carbonApiResponse = await _httpClient.GetAsync(generationEndpointUrl);

        carbonApiResponse.EnsureSuccessStatusCode();

        var generationResponse =
            await carbonApiResponse.Content.ReadFromJsonAsync<CarbonGenerationResponseDto>();

        if (generationResponse is null)
        {
            throw new InvalidOperationException("Carbon Intensity API returned an empty generation response.");
        }

        return generationResponse;
    }
}
