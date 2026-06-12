FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["EnergyMix.Backend/EnergyMix.Backend.csproj", "EnergyMix.Backend/"]
COPY ["EnergyMix.Backend.Tests/EnergyMix.Backend.Tests.csproj", "EnergyMix.Backend.Tests/"]

RUN dotnet restore "EnergyMix.Backend/EnergyMix.Backend.csproj"

COPY . .

RUN dotnet publish "EnergyMix.Backend/EnergyMix.Backend.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "EnergyMix.Backend.dll"]
