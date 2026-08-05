using System;

namespace ZarinPal.Exceptions;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Creates a new ValidationException
    /// </summary>
    /// <param name="message">Error message</param>
    public ValidationException(string message) : base(message)
    {
    }
}