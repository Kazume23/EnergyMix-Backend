namespace EnergyMix.Backend.Services
{
    public class CarbonIntensityService
    {
        private readonly HttpClient _httpClient;

        public CarbonIntensityService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetRawGenerationMixAsync()
        {
            var startDateUtc = DateTime.UtcNow.Date;
            var endDateUtc = startDateUtc.AddDays(1);

            var startDateText = startDateUtc.ToString("yyyy-MM-ddTHH:mmZ");
            var endDateText = endDateUtc.ToString("yyyy-MM-ddTHH:mmZ");

            var generationEndpointUrl = $"generation/{startDateText}/{endDateText}";

            var carbonApiResponse = await _httpClient.GetAsync(generationEndpointUrl);

            carbonApiResponse.EnsureSuccessStatusCode();

            return await carbonApiResponse.Content.ReadAsStringAsync();
        }
    }
}
