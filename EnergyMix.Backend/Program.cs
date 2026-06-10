using EnergyMix.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<CarbonIntensityService>(client =>
{
    client.BaseAddress = new Uri("https://api.carbonintensity.org.uk/");
});

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

app.MapControllers();

app.Run();