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
* Provides unit tests for calculation logic.
* Includes Docker support for deployment.

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
  "averageCleanEnergyPercentage": 63.41
}
```

Example validation error:

```json
{
  "message": "Hours must be between 1 and 6."
}
```

## Configuration

The backend allows frontend requests through CORS.

Local frontend origins are configured by default:

```text
http://localhost:5173
http://localhost:5174
```

For deployed environments, configure the deployed frontend URL:

```json
{
  "FrontendUrl": "https://your-frontend-url.com"
}
```

Current production frontend URL:

```json
{
  "FrontendUrl": "https://energymix-frontend-hju7.onrender.com"
}
```

<<<<<<< HEAD
=======
For production deployment, set `AllowedHosts` to the deployed backend host, for example through the `ASPNETCORE_ALLOWEDHOSTS` environment variable. Do not use `*` for a public deployment.

>>>>>>> 9bd99652ecae5ef5ade7bbad4bae3cadd9a47f0b
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

The test project covers calculation logic, including:

* clean energy percentage calculation
* daily energy mix aggregation
* optimal charging window selection

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
  Controllers/   HTTP endpoints
  Models/        API and response models
  Services/      Carbon API client and calculation logic

EnergyMix.Backend.Tests/
  Services/      Unit tests for calculation services
```

## Notes

Time values are handled in UTC because the Carbon Intensity API returns UTC timestamps.

The backend intentionally uses a simple structure suitable for a small recruitment task while keeping the calculation logic separated and covered by unit tests.
