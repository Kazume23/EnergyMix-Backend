# EnergyMix Backend

Backend API for the EnergyMix recruitment task. It fetches UK electricity generation data from the public Carbon Intensity API and calculates daily energy mix averages and the cleanest electric vehicle charging window.

This repository contains the .NET backend. The frontend application is maintained in a separate repository.

## Related Repositories

* Backend API: https://github.com/Kazume23/EnergyMix-Backend
* Frontend: https://github.com/Kazume23/EnergyMix-Frontend

## Deployed Application

* Backend API: https://energymix-backend-wsuq.onrender.com
* Frontend: https://energymix-frontend-hju7.onrender.com

## Features

* Fetches UK generation mix data from the Carbon Intensity API.
* Processes half-hour generation intervals.
* Calculates average daily generation mix for today, tomorrow, and the day after tomorrow.
* Calculates clean energy percentage using the task definition.
* Finds the optimal EV charging window for a duration from 1 to 6 full hours.
* Uses RFC 7807 Problem Details for error responses.
* Adds memory caching, retry handling, request logging, rate limiting, health checks, and timeout protection for external HTTP calls.
* Provides unit tests for calculation, service, controller, and utility logic.
* Includes Docker support for deployment.
* Includes a GitHub Actions CI workflow.

## Technology Stack

* .NET 10
* ASP.NET Core Web API
* xUnit
* Docker

## External API

Data source:

```text
https://api.carbonintensity.org.uk/
```

Used endpoint:

```http
GET /generation/{from}/{to}
```

The Carbon Intensity API returns generation mix data in 30-minute intervals.

Clean energy sources for this task:

```text
biomass
nuclear
hydro
wind
solar
```

## API Endpoints

### Daily Energy Mix

```http
GET /api/carbon/daily-mix
```

Fetches generation data for three days: today, tomorrow, and the day after tomorrow. The backend groups half-hour intervals by date and returns average source shares for each day.

Example response:

```json
[
  {
    "date": "2026-06-12",
    "sources": [
      {
        "fuel": "wind",
        "percentage": 32.45
      },
      {
        "fuel": "gas",
        "percentage": 21.18
      }
    ],
    "cleanEnergyPercentage": 58.72
  }
]
```

### Optimal Charging Window

```http
GET /api/carbon/optimal-charging-window?hours=4
```

Finds the time window with the highest average clean energy share. The `hours` query parameter must be a full number between `1` and `6`.

Because source data uses 30-minute intervals:

```text
1 hour = 2 intervals
3 hours = 6 intervals
6 hours = 12 intervals
```

Example response:

```json
{
  "start": "2026-06-13T01:00:00+00:00",
  "end": "2026-06-13T05:00:00+00:00",
  "averageCleanEnergyPercentage": 63.41,
  "sources": [
    {
      "fuel": "wind",
      "percentage": 41.22
    },
    {
      "fuel": "gas",
      "percentage": 19.34
    }
  ]
}
```

Example validation error:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Hours": [
      "Hours must be between 1 and 6."
    ]
  }
}
```

### Health Check

```http
GET /health
```

Returns the ASP.NET Core health check status and is also used by the Docker `HEALTHCHECK`.

## Configuration

The backend uses regular ASP.NET Core configuration. Shared settings live in `appsettings.json`, while environment-specific host and CORS values live in `appsettings.Development.json` and `appsettings.Production.json`.

The backend allows frontend requests through CORS. Allowed frontend origins are configured under `Cors:AllowedOrigins`.

Production host and CORS defaults are stored in `appsettings.Production.json`:

```json
{
  "AllowedHosts": "energymix-backend-wsuq.onrender.com",
  "Cors": {
    "AllowedOrigins": [
      "https://energymix-frontend-hju7.onrender.com"
    ]
  }
}
```

Local development origins are stored in `appsettings.Development.json`:

```json
{
  "AllowedHosts": "localhost;127.0.0.1",
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "http://localhost:5174"
    ]
  }
}
```

CORS origins should be written without a trailing slash, for example `https://energymix-frontend-hju7.onrender.com`. The application also trims trailing slashes defensively when building the CORS policy.

The same values can also be overridden in Render environment variables, for example:

```text
ASPNETCORE_ENVIRONMENT=Production
AllowedHosts=energymix-backend-wsuq.onrender.com
Cors__AllowedOrigins__0=https://energymix-frontend-hju7.onrender.com
```

External Carbon Intensity API settings are configured under `CarbonIntensityApi`:

```json
{
  "CarbonIntensityApi": {
    "BaseUrl": "https://api.carbonintensity.org.uk/",
    "TotalTimeoutSeconds": 45,
    "AttemptTimeoutSeconds": 20,
    "RetryCount": 1,
    "RetryDelayMilliseconds": 1000,
    "CircuitBreakerSamplingDurationSeconds": 60
  }
}
```

The outgoing Carbon Intensity API client uses:

* `AttemptTimeoutSeconds` for one HTTP attempt
* `RetryCount` and `RetryDelayMilliseconds` for transient retries
* `TotalTimeoutSeconds` as the maximum time budget for the full request, including retries
* `CircuitBreakerSamplingDurationSeconds` for the resilience circuit breaker sampling window

If the frontend uses its own request timeout, it should be at least as long as the backend total timeout. Otherwise the browser can abort a request before the backend finishes retrying.

## Production Behavior

The application is prepared for deployment behind a reverse proxy such as Render:

* `ForwardedHeaders` is enabled so the app can read proxy headers.
* HTTPS redirection is only enabled in development because Render terminates HTTPS before forwarding traffic to the container.
* Unhandled exceptions are converted to RFC 7807 Problem Details responses.
* External Carbon Intensity API failures return `502 Bad Gateway`.
* External request timeouts return `504 Gateway Timeout`.
* Missing generation data returns `503 Service Unavailable`.
* Validation errors return `400 Bad Request`.
* Public endpoints are rate limited to `60` requests per minute per remote IP.
* Carbon Intensity generation responses are cached for `30` minutes.

## Running Locally

Restore dependencies:

```bash
dotnet restore
```

Build the solution:

```bash
dotnet build
```

Run the API:

```bash
dotnet run --project EnergyMix.Backend/EnergyMix.Backend.csproj
```

Swagger UI is available in development at:

```text
https://localhost:7008/swagger
```

The HTTP profile also uses:

```text
http://localhost:5200
```

## Tests

Run unit tests:

```bash
dotnet test
```

Run the same build and test style as CI:

```bash
dotnet build --configuration Release
dotnet test --no-build --configuration Release
```

The test project covers calculation logic, including:

* clean energy percentage calculation
* daily energy mix aggregation
* optimal charging window selection
* average source share calculation
* Carbon service orchestration and caching
* controller success responses

## CI

GitHub Actions restores dependencies, builds the backend in Release mode, and runs the test project.

## Docker

Build the Docker image:

```bash
docker build -t energymix-backend .
```

Run the container:

```bash
docker run -p 8080:8080 energymix-backend
```

The container exposes the API on port `8080`.

## Project Structure

```text
EnergyMix.Backend/
  Clients/       External API clients
  Config/        Application service and middleware configuration
  Constants/     Shared business constants
  Controllers/   HTTP endpoints
  Dtos/          Request, external API, and backend response DTOs
  Exceptions/    Application-specific exceptions
  Services/      Business orchestration
  Utilities/     Calculation and shared helper logic
    ChargingWindowCalculator/
    CleanEnergyCalculator/
    EnergyMixCalculator/
    EnergySourceShareCalculator/

EnergyMix.Backend.Tests/
  Controllers/   Unit tests for controller responses
  Helpers/       Shared test data builders
  Services/      Unit tests for service orchestration
  Utilities/     Unit tests for calculation and utility logic
```

## Notes

Time values are handled in UTC because the Carbon Intensity API returns UTC timestamps.

The backend intentionally uses a simple structure suitable for a small recruitment task while keeping the calculation logic separated and covered by unit tests.
