# EnergyMix.Backend

Backend API for an internship assignment focused on the UK electricity generation mix and the best EV charging window based on clean energy share.

The application uses the public Carbon Intensity API to fetch half-hour generation mix intervals, then calculates:

- average daily generation mix for today, tomorrow, and the day after tomorrow
- the best charging window in the next two days for a user-provided duration

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- xUnit for unit tests
- Docker for deployment

## External API

Data source:

```text
https://api.carbonintensity.org.uk/
```

Used endpoint:

```text
GET /generation/{from}/{to}
```

The API returns generation mix data in 30-minute intervals.

For this assignment, clean energy is defined as the sum of:

```text
biomass, nuclear, hydro, wind, solar
```

## Endpoints

### Daily energy mix

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

### Optimal charging window

```http
GET /api/carbon/optimal-charging-window?hours=4
```

Finds the time window with the highest average clean energy share. The `hours` query parameter must be a full number between `1` and `6`.

Because Carbon Intensity data uses 30-minute intervals:

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

The test project focuses on calculation logic:

- clean energy percentage calculation
- daily energy mix aggregation
- optimal charging window selection

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

- Time values are handled in UTC because the Carbon Intensity API returns UTC timestamps.
- The frontend is planned as a separate repository.
- The backend intentionally uses a simple structure suitable for a small internship assignment.
