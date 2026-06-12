using EnergyMix.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient<CarbonIntensityService>(client =>
{
    var carbonApiBaseUrl = builder.Configuration["CarbonIntensityApi:BaseUrl"]
        ?? throw new InvalidOperationException("CarbonIntensityApi:BaseUrl is not configured.");

    client.BaseAddress = new Uri(carbonApiBaseUrl);
});

builder.Services.AddSingleton<CleanEnergyCalculator>();
builder.Services.AddSingleton<EnergyMixCalculator>();
builder.Services.AddSingleton<ChargingWindowCalculator>();
builder.Services.AddScoped<CarbonService>();

var app = builder.Build();

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

app.Run();
