using Energy.Mix.Backend.Models;
using EnergyMix.Backend.Models;
using System.Net.Http.Json;

namespace EnergyMix.Backend.Services

{
    public class CarbonIntensityService
    {
        private readonly HttpClient _httpClient;

        public CarbonIntensityService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CarbonGenerationResponse> GetRawGenerationMixAsync(DateTimeOffset startDateUtc, DateTimeOffset endDateUtc)
        {
            var startDateText = startDateUtc.UtcDateTime.ToString("yyyy-MM-ddTHH:mmZ");
            var endDateText = endDateUtc.UtcDateTime.ToString("yyyy-MM-ddTHH:mmZ");
            
            var generationEndpointUrl = $"generation/{startDateText}/{endDateText}";

            var carbonApiResponse = await _httpClient.GetAsync(generationEndpointUrl);

            carbonApiResponse.EnsureSuccessStatusCode();
            
            var generationResponse = 
                await carbonApiResponse.Content.ReadFromJsonAsync<CarbonGenerationResponse>();
            return generationResponse
                ?? throw new InvalidOperationException("Carbon Intensity API returned an empty generation response.");
        }
    }
}
