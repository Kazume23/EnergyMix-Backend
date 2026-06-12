using EnergyMix.Backend.Models;
using System.Net.Http.Json;

namespace EnergyMix.Backend.Services

{
    public sealed class CarbonIntensityService
    {
        private readonly HttpClient _httpClient;

        public CarbonIntensityService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CarbonGenerationResponse> GetGenerationAsync(DateTimeOffset startDateUtc, DateTimeOffset endDateUtc)
        {
            var startDateText = startDateUtc.UtcDateTime.ToString("yyyy-MM-ddTHH:mmZ");
            var endDateText = endDateUtc.UtcDateTime.ToString("yyyy-MM-ddTHH:mmZ");
            
            var generationEndpointUrl = $"generation/{startDateText}/{endDateText}";

            var carbonApiResponse = await _httpClient.GetAsync(generationEndpointUrl);

            carbonApiResponse.EnsureSuccessStatusCode();
            
            var generationResponse = 
                await carbonApiResponse.Content.ReadFromJsonAsync<CarbonGenerationResponse>();

            if (generationResponse is null)
            {
                throw new InvalidOperationException("Carbon Intensity API returned an empty generation response.");
            }

            return generationResponse;
        }
    }
}
