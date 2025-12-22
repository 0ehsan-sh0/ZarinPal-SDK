using System.Net;

namespace ZarinPal.Exceptions;

/// <summary>
/// Exception thrown when API responses contain errors
/// </summary>
public class ResponseException : Exception
{
    /// <summary>
    /// HTTP status code of the response
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Creates a new ResponseException
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="statusCode">HTTP status code</param>
    public ResponseException(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public ResponseException(string message) : base(message)
    {
        StatusCode = HttpStatusCode.InternalServerError;
    }

    /// <summary>
    /// Gets the status code
    /// </summary>
    /// <returns>HTTP status code</returns>
    public HttpStatusCode GetStatusCode()
    {
        return StatusCode;
    }
}
