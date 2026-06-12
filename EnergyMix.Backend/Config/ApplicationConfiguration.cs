using EnergyMix.Backend.Clients;
using EnergyMix.Backend.Services;

namespace EnergyMix.Backend.Config;

public static class ApplicationConfiguration
{
    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddFrontendCors(builder.Configuration);
        builder.Services.AddCarbonIntensityClient(builder.Configuration);
        builder.Services.AddScoped<CarbonService>();
    }

    public static void UseApplicationPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "EnergyMix API v1");
            });
        }

        app.UseHttpsRedirection();
        app.UseCors("Frontend");
        app.MapControllers();
    }

    private static void AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

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
        services.AddHttpClient<CarbonIntensityClient>(client =>
        {
            var carbonApiBaseUrl = configuration["CarbonIntensityApi:BaseUrl"]
                ?? throw new InvalidOperationException("CarbonIntensityApi:BaseUrl is not configured.");

            client.BaseAddress = new Uri(carbonApiBaseUrl);
        });
    }
}
