using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ZarinPal.Interfaces;

/// <summary>
/// Interface for ZarinPal client to allow for testing and dependency injection
/// </summary>
public interface IZarinPalClient : IDisposable
{
    /// <summary>
    /// General method for making HTTP requests to ZarinPal's REST API.
    /// </summary>
    /// <param name="method">The HTTP method (e.g., 'GET', 'POST')</param>
    /// <param name="url">The endpoint URL relative to the base URL</param>
    /// <param name="data">The request payload</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The raw JSON response from the API</returns>
    Task<JsonElement> RequestAsync(string method, string url, object? data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// General method for making HTTP requests to ZarinPal's REST API with typed response.
    /// </summary>
    /// <typeparam name="T">Target response model type</typeparam>
    /// <param name="method">The HTTP method (e.g., 'GET', 'POST')</param>
    /// <param name="url">The endpoint URL relative to the base URL</param>
    /// <param name="data">The request payload</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The deserialized response data model</returns>
    Task<T?> RequestAsync<T>(string method, string url, object? data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// General method for making GraphQL requests to ZarinPal's API.
    /// </summary>
    /// <param name="query">The GraphQL query string</param>
    /// <param name="variables">An object containing the variables for the GraphQL query</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The raw JSON response from the API</returns>
    Task<JsonElement> GraphqlAsync(string query, object? variables = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// General method for making GraphQL requests to ZarinPal's API with typed response.
    /// </summary>
    /// <typeparam name="T">Target response model type</typeparam>
    /// <param name="query">The GraphQL query string</param>
    /// <param name="variables">An object containing the variables for the GraphQL query</param>
    /// <param name="dataPath">Property name or path within response data to deserialize</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The deserialized response data model</returns>
    Task<T?> GraphqlAsync<T>(string query, object? variables = null, string? dataPath = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the base URL used for API requests
    /// </summary>
    /// <returns>The base URL</returns>
    string GetBaseUrl();
}