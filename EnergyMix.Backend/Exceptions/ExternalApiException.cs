using System.Net;

namespace EnergyMix.Backend.Exceptions;

public sealed class ExternalApiException : Exception
{
    public ExternalApiException(
        string message,
        HttpStatusCode statusCode,
        string responseBody)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public HttpStatusCode StatusCode { get; }
    public string ResponseBody { get; }
}
