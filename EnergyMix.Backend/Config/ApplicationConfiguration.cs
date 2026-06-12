using EnergyMix.Backend.Clients;
using EnergyMix.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMix.Backend.Config;

public static class ApplicationConfiguration
{
    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
                options.InvalidModelStateResponseFactory = CreateValidationErrorResponse);

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
        services.AddHttpClient<CarbonIntensityClient>(client =>
        {
            var carbonApiBaseUrl = configuration["CarbonIntensityApi:BaseUrl"]
                ?? throw new InvalidOperationException("CarbonIntensityApi:BaseUrl is not configured.");

            client.BaseAddress = new Uri(carbonApiBaseUrl);
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

    private static IActionResult CreateValidationErrorResponse(ActionContext context)
    {
        var message = context.ModelState.Values
            .SelectMany(modelStateEntry => modelStateEntry.Errors)
            .Select(modelError => modelError.ErrorMessage)
            .FirstOrDefault() ?? "The request is invalid.";

        return new BadRequestObjectResult(new { message });
    }
}
