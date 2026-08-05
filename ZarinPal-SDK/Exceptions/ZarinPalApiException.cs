using System.Net;

namespace ZarinPal.Exceptions;

/// <summary>
/// Exception thrown when ZarinPal API returns a business logic error code (e.g. data.code != 100).
/// </summary>
public class ZarinPalApiException : ResponseException
{
    /// <summary>
    /// Gets the error code returned by ZarinPal.
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// Creates a new instance of <see cref="ZarinPalApiException"/>.
    /// </summary>
    /// <param name="code">The ZarinPal error code.</param>
    /// <param name="message">The error message.</param>
    public ZarinPalApiException(int code, string message) : base(message)
    {
        Code = code;
    }

    /// <summary>
    /// Creates a new instance of <see cref="ZarinPalApiException"/> with an HTTP status code.
    /// </summary>
    /// <param name="code">The ZarinPal error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    public ZarinPalApiException(int code, string message, HttpStatusCode statusCode) : base(message, statusCode)
    {
        Code = code;
    }
}
