using EnergyMix.Backend.Clients;
using EnergyMix.Backend.Dtos.CarbonApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly.Timeout;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace EnergyMix.Backend.Tests.Clients;

public class CarbonIntensityClientResilienceTests
{
    [Fact]
    public async Task GetGenerationAsync_TimesOutAttemptAndRetries_WhenCarbonApiDoesNotRespond()
    {
        var delayingHandler = new DelayingHttpMessageHandler(TimeSpan.FromSeconds(10));
        await using var serviceProvider = CreateServiceProvider(delayingHandler);
        var carbonIntensityClient = serviceProvider.GetRequiredService<ICarbonIntensityClient>();

        var stopwatch = Stopwatch.StartNew();

        await Assert.ThrowsAsync<TimeoutRejectedException>(() =>
            carbonIntensityClient.GetGenerationAsync(
                new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 6, 16, 0, 0, 0, TimeSpan.Zero)));

        stopwatch.Stop();

        Assert.Equal(2, delayingHandler.RequestCount);
        Assert.InRange(stopwatch.Elapsed, TimeSpan.FromMilliseconds(1500), TimeSpan.FromSeconds(5));
    }

    private static ServiceProvider CreateServiceProvider(HttpMessageHandler httpMessageHandler)
    {
        var services = new ServiceCollection();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
        });

        services.AddHttpClient<ICarbonIntensityClient, CarbonIntensityClient>(client =>
        {
            client.BaseAddress = new Uri("https://carbon-intensity.test/");
            client.Timeout = Timeout.InfiniteTimeSpan;
        })
        .ConfigurePrimaryHttpMessageHandler(() => httpMessageHandler)
        .AddStandardResilienceHandler(options =>
        {
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(5);
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(1);
            options.Retry.MaxRetryAttempts = 1;
            options.Retry.Delay = TimeSpan.FromMilliseconds(10);
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(3);
        });

        return services.BuildServiceProvider(validateScopes: true);
    }

    private sealed class DelayingHttpMessageHandler : HttpMessageHandler
    {
        private readonly TimeSpan _delay;
        private int _requestCount;

        public DelayingHttpMessageHandler(TimeSpan delay)
        {
            _delay = delay;
        }

        public int RequestCount => _requestCount;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _requestCount);

            await Task.Delay(_delay, cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new CarbonGenerationResponseDto { Data = [] })
            };
        }
    }
}
