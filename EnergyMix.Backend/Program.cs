using EnergyMix.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var frontendUrl = builder.Configuration["FrontendUrl"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var allowedOrigins = new List<string>
        {
            "http://localhost:5173",
            "http://localhost:5174"
        };

        if (!string.IsNullOrWhiteSpace(frontendUrl))
        {
            allowedOrigins.Add(frontendUrl);
        }

        policy
            .WithOrigins(allowedOrigins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient<CarbonIntensityService>(client =>
{
    client.BaseAddress = new Uri("https://api.carbonintensity.org.uk/");
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
