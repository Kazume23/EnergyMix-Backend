using EnergyMix.Backend.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMix.Backend.Config;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(
        IHostEnvironment hostEnvironment,
        ILogger<GlobalExceptionHandler> logger,
        IProblemDetailsService problemDetailsService)
    {
        _hostEnvironment = hostEnvironment;
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = CreateProblemDetails(httpContext, exception);

        _logger.LogError(
            exception,
            "Request failed with status code {StatusCode}: {Title}",
            problemDetails.Status,
            problemDetails.Title);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private ProblemDetails CreateProblemDetails(HttpContext httpContext, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            ExternalApiException => (StatusCodes.Status502BadGateway, "External API request failed."),
            InsufficientGenerationDataException => (StatusCodes.Status503ServiceUnavailable, "Generation data is not available."),
            HttpRequestException => (StatusCodes.Status502BadGateway, "External API request failed."),
            TaskCanceledException => (StatusCodes.Status504GatewayTimeout, "External API request timed out."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = _hostEnvironment.IsDevelopment() ? exception.Message : null,
            Instance = httpContext.Request.Path
        };
    }
}
