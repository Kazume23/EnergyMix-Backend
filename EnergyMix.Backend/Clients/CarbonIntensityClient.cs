using EnergyMix.Backend.Dtos.CarbonApi;
using EnergyMix.Backend.Exceptions;
using System.Net.Http.Json;

namespace EnergyMix.Backend.Clients;

public sealed class CarbonIntensityClient : ICarbonIntensityClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CarbonIntensityClient> _logger;

    public CarbonIntensityClient(
        HttpClient httpClient,
        ILogger<CarbonIntensityClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CarbonGenerationResponseDto> GetGenerationAsync(
        DateTimeOffset startDateUtc,
        DateTimeOffset endDateUtc,
        CancellationToken cancellationToken = default)
    {
        var startDateText = startDateUtc.UtcDateTime.ToString("yyyy-MM-ddTHH:mmZ");
        var endDateText = endDateUtc.UtcDateTime.ToString("yyyy-MM-ddTHH:mmZ");
        var generationEndpointUrl = $"generation/{startDateText}/{endDateText}";

        using var carbonApiResponse = await _httpClient.GetAsync(generationEndpointUrl, cancellationToken);

        if (!carbonApiResponse.IsSuccessStatusCode)
        {
            var responseBody = await carbonApiResponse.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogWarning(
                "Carbon Intensity API returned {StatusCode}: {ResponseBody}",
                carbonApiResponse.StatusCode,
                responseBody);

            throw new ExternalApiException(
                "Carbon Intensity API returned an unsuccessful response.",
                carbonApiResponse.StatusCode,
                responseBody);
        }

        var generationResponse =
            await carbonApiResponse.Content.ReadFromJsonAsync<CarbonGenerationResponseDto>(
                cancellationToken);

        if (generationResponse is null)
        {
            throw new ExternalApiException(
                "Carbon Intensity API returned an empty generation response.",
                carbonApiResponse.StatusCode,
                string.Empty);
        }

        return generationResponse;
    }
}
