# EnergyMix.Backend

Backend API for an internship recruitment task. The application uses the public Carbon Intensity API to fetch UK energy generation mix data and calculate clean energy statistics.

## Technologies

* .NET 10
* ASP.NET Core Web API
* xUnit
* Carbon Intensity API

## Features

The backend exposes two main endpoints required by the task.

### Daily energy mix

```http
GET /api/carbon/daily-mix
```

Fetches generation mix data for three days: today, tomorrow and the day after tomorrow.

The endpoint groups half-hour intervals by date and returns average energy source percentages for each day, including the calculated clean energy percentage.

Clean energy sources are:

```text
biomass
nuclear
hydro
wind
solar
```

Example response:

```json
[
  {
    "date": "2026-06-11",
    "sources": [
      {
        "fuel": "biomass",
        "percentage": 7.46
      },
      {
        "fuel": "wind",
        "percentage": 36.13
      }
    ],
    "cleanEnergyPercentage": 60.35
  }
]
```

### Optimal charging window

```http
GET /api/carbon/optimal-charging-window?hours=4
```

Finds the best charging window for an electric vehicle based on the highest average share of clean energy.

The `hours` parameter must be a full number between 1 and 6.

Since Carbon Intensity API data is provided in half-hour intervals, one hour equals two intervals.

Example response:

```json
{
  "start": "2026-06-13T20:00:00+00:00",
  "end": "2026-06-14T00:00:00+00:00",
  "averageCleanEnergyPercentage": 67.79
}
```

## Running the project

From the repository root:

```bash
dotnet run --project EnergyMix.Backend/EnergyMix.Backend.csproj
```

Swagger is available in development mode and can be used to test the API endpoints.

## Running tests

From the repository root:

```bash
dotnet test
```

The test project covers the main backend calculation logic:

* clean energy percentage calculation
* daily energy mix grouping and averaging
* optimal charging window selection
* validation of allowed charging window length
