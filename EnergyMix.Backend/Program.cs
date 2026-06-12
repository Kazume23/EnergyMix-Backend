using EnergyMix.Backend.Config;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

app.UseApplicationPipeline();

app.Run();
