using System.Text.Json;

namespace ZarinPal.Interfaces;

/// <summary>
/// Interface for ZarinPal client to allow for testing and dependency injection
/// </summary>
public interface IZarinPalClient : IDisposable
{
    /// <summary>
    /// General method for making HTTP requests to ZarinPal's REST API
    /// </summary>
    /// <param name="method">The HTTP method (e.g., 'GET', 'POST')</param>
    /// <param name="url">The endpoint URL relative to the base URL</param>
    /// <param name="data">The request payload</param>
    /// <returns>The response data from the API</returns>
    Task<JsonElement> RequestAsync(string method, string url, object? data = null);

    /// <summary>
    /// General method for making GraphQL requests to ZarinPal's API
    /// </summary>
    /// <param name="query">The GraphQL query string</param>
    /// <param name="variables">An object containing the variables for the GraphQL query</param>
    /// <returns>The response data from the API</returns>
    Task<JsonElement> GraphqlAsync(string query, object? variables = null);

    /// <summary>
    /// Gets the base URL used for API requests
    /// </summary>
    /// <returns>The base URL</returns>
    string GetBaseUrl();
}