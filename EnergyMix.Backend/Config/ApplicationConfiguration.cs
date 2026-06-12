using EnergyMix.Backend.Clients;
using EnergyMix.Backend.Services;
using EnergyMix.Backend.Utilities;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Http.Resilience;
using System.Threading.RateLimiting;

namespace EnergyMix.Backend.Config;

public static class ApplicationConfiguration
{
    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddOpenApi();
        builder.Services.AddHttpLogging();
        builder.Services.AddMemoryCache();
        builder.Services.AddHealthChecks();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.Configure<CarbonIntensityApiOptions>(
            builder.Configuration.GetSection(CarbonIntensityApiOptions.SectionName));
        builder.Services.ConfigureForwardedHeaders();
        builder.Services.AddCarbonRateLimiter();
        builder.Services.AddFrontendCors(builder.Configuration);
        builder.Services.AddCarbonIntensityClient(builder.Configuration);
        builder.Services.AddSingleton<ICleanEnergyCalculator, CleanEnergyCalculator>();
        builder.Services.AddSingleton<IEnergySourceShareCalculator, EnergySourceShareCalculator>();
        builder.Services.AddSingleton<IEnergyMixCalculator, EnergyMixCalculator>();
        builder.Services.AddSingleton<IChargingWindowCalculator, ChargingWindowCalculator>();
        builder.Services.AddScoped<ICarbonService, CarbonService>();
    }

    public static void UseApplicationPipeline(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseForwardedHeaders();
        app.UseHttpLogging();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "EnergyMix API v1");
            });

            app.UseHttpsRedirection();
        }

        app.UseCors("Frontend");
        app.UseRateLimiter();
        app.MapControllers();
        app.MapHealthChecks("/health");
    }

    private static void AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = GetAllowedOrigins(configuration);

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    private static void AddCarbonIntensityClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<ICarbonIntensityClient, CarbonIntensityClient>(client =>
        {
            var carbonApiBaseUrl = configuration[$"{CarbonIntensityApiOptions.SectionName}:BaseUrl"]
                ?? throw new InvalidOperationException("CarbonIntensityApi:BaseUrl is not configured.");

            client.BaseAddress = new Uri(carbonApiBaseUrl);
            client.Timeout = Timeout.InfiniteTimeSpan;
        })
        .AddStandardResilienceHandler(options =>
        {
            var totalTimeoutSeconds = configuration.GetValue(
                $"{CarbonIntensityApiOptions.SectionName}:TotalTimeoutSeconds",
                30);
            var attemptTimeoutSeconds = configuration.GetValue(
                $"{CarbonIntensityApiOptions.SectionName}:AttemptTimeoutSeconds",
                8);
            var retryCount = configuration.GetValue(
                $"{CarbonIntensityApiOptions.SectionName}:RetryCount",
                2);
            var retryDelayMilliseconds = configuration.GetValue(
                $"{CarbonIntensityApiOptions.SectionName}:RetryDelayMilliseconds",
                500);

            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(totalTimeoutSeconds);
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(attemptTimeoutSeconds);
            options.Retry.MaxRetryAttempts = retryCount;
            options.Retry.Delay = TimeSpan.FromMilliseconds(retryDelayMilliseconds);
        });
    }

    private static void ConfigureForwardedHeaders(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto;

            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });
    }

    private static void AddCarbonRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    });
            });
        });
    }

    private static string[] GetAllowedOrigins(IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        return allowedOrigins
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim().TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
