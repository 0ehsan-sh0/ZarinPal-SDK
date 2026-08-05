using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZarinPal.Exceptions;
using ZarinPal.Interfaces;
using ZarinPal.Models;
using ZarinPal.Resources;

namespace ZarinPal;

/// <summary>
/// Main class for interacting with ZarinPal APIs.
/// Provides access to various resources such as payments, refunds, transactions, etc.
/// </summary>
public class ZarinPal : IZarinPal, IZarinPalClient
{
    /// <summary>
    /// Gets the payments resource instance.
    /// </summary>
    public Payments Payments { get; }

    /// <summary>
    /// Gets the refunds resource instance.
    /// </summary>
    public Refunds Refunds { get; }

    /// <summary>
    /// Gets the transactions resource instance.
    /// </summary>
    public Transactions Transactions { get; }

    /// <summary>
    /// Gets the verifications resource instance.
    /// </summary>
    public Verifications Verifications { get; }

    /// <summary>
    /// Gets the reversals resource instance.
    /// </summary>
    public Reversals Reversals { get; }

    /// <summary>
    /// Gets the unverified resource instance.
    /// </summary>
    public Unverified Unverified { get; }

    /// <summary>
    /// Gets the inquiries resource instance.
    /// </summary>
    public Inquiries Inquiries { get; }

    private Config Config { get; }
    private HttpClient HttpClient { get; }
    private HttpClient GraphqlClient { get; }
    private readonly bool _ownsHttpClients;
    private readonly ILogger<ZarinPal>? _logger;
    private string BaseUrl { get; }

    /// <summary>
    /// Creates an instance of ZarinPal.
    /// </summary>
    /// <param name="config">The configuration object.</param>
    public ZarinPal(Config config)
        : this(config, (ILogger<ZarinPal>?)null)
    {
    }

    /// <summary>
    /// Creates an instance of ZarinPal with optional logger.
    /// </summary>
    /// <param name="config">The configuration object.</param>
    /// <param name="logger">Optional logger instance.</param>
    public ZarinPal(Config config, ILogger<ZarinPal>? logger)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger;
        _ownsHttpClients = true;

        BaseUrl = Config.Sandbox
            ? "https://sandbox.zarinpal.com"
            : "https://payment.zarinpal.com";

        HttpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = Config.Timeout
        };
        ConfigureHttpClientHeaders(HttpClient, Config);

        var graphqlBaseUrl = Config.Sandbox
            ? "https://sandbox.zarinpal.com/api/v4/graphql/"
            : "https://next.zarinpal.com/api/v4/graphql/";

        GraphqlClient = new HttpClient
        {
            BaseAddress = new Uri(graphqlBaseUrl),
            Timeout = Config.Timeout
        };
        ConfigureHttpClientHeaders(GraphqlClient, Config);

        if (!string.IsNullOrEmpty(Config.AccessToken))
        {
            GraphqlClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Config.AccessToken);
        }

        Payments = new Payments(this);
        Refunds = new Refunds(this);
        Transactions = new Transactions(this);
        Verifications = new Verifications(this);
        Reversals = new Reversals(this);
        Unverified = new Unverified(this);
        Inquiries = new Inquiries(this);
    }

    /// <summary>
    /// Creates an instance of ZarinPal with injected HttpClients.
    /// </summary>
    public ZarinPal(Config config, HttpClient restClient, HttpClient graphqlClient, ILogger<ZarinPal>? logger = null)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));
        HttpClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        GraphqlClient = graphqlClient ?? throw new ArgumentNullException(nameof(graphqlClient));
        _logger = logger;
        _ownsHttpClients = false;

        BaseUrl = Config.Sandbox
            ? "https://sandbox.zarinpal.com"
            : "https://payment.zarinpal.com";

        Payments = new Payments(this);
        Refunds = new Refunds(this);
        Transactions = new Transactions(this);
        Verifications = new Verifications(this);
        Reversals = new Reversals(this);
        Unverified = new Unverified(this);
        Inquiries = new Inquiries(this);
    }

    /// <summary>
    /// Creates an instance of ZarinPal using IHttpClientFactory.
    /// </summary>
    public ZarinPal(Config config, IHttpClientFactory httpClientFactory, ILogger<ZarinPal>? logger = null)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));
        if (httpClientFactory == null) throw new ArgumentNullException(nameof(httpClientFactory));

        _logger = logger;
        _ownsHttpClients = false;

        BaseUrl = Config.Sandbox
            ? "https://sandbox.zarinpal.com"
            : "https://payment.zarinpal.com";

        HttpClient = httpClientFactory.CreateClient("ZarinPalRest");
        if (HttpClient.BaseAddress == null)
        {
            HttpClient.BaseAddress = new Uri(BaseUrl);
        }

        GraphqlClient = httpClientFactory.CreateClient("ZarinPalGraphql");
        if (GraphqlClient.BaseAddress == null)
        {
            var graphqlBaseUrl = Config.Sandbox
                ? "https://sandbox.zarinpal.com/api/v4/graphql/"
                : "https://next.zarinpal.com/api/v4/graphql/";
            GraphqlClient.BaseAddress = new Uri(graphqlBaseUrl);
        }

        Payments = new Payments(this);
        Refunds = new Refunds(this);
        Transactions = new Transactions(this);
        Verifications = new Verifications(this);
        Reversals = new Reversals(this);
        Unverified = new Unverified(this);
        Inquiries = new Inquiries(this);
    }

    private static void ConfigureHttpClientHeaders(HttpClient client, Config config)
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd(config.UserAgent);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// General method for making HTTP requests to ZarinPal's REST API.
    /// </summary>
    public async Task<JsonElement> RequestAsync(string method, string url, object? data = null, CancellationToken cancellationToken = default)
    {
        JsonObject jsonObject;
        if (data != null)
        {
            var node = JsonSerializer.SerializeToNode(data);
            jsonObject = node as JsonObject ?? new JsonObject();
        }
        else
        {
            jsonObject = new JsonObject();
        }

        if (!string.IsNullOrEmpty(Config.MerchantId) && !jsonObject.ContainsKey("merchant_id"))
        {
            jsonObject["merchant_id"] = Config.MerchantId;
        }

        var json = jsonObject.ToJsonString();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(new HttpMethod(method), url)
        {
            Content = content
        };

        _logger?.LogDebug("Sending REST request to {Method} {Url}", method, url);
        var response = await HttpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync();

        return ParseAndValidateResponse(responseContent, response.StatusCode, isGraphql: false);
    }

    /// <summary>
    /// General method for making HTTP requests to ZarinPal's REST API with typed response.
    /// </summary>
    public async Task<T?> RequestAsync<T>(string method, string url, object? data = null, CancellationToken cancellationToken = default)
    {
        var root = await RequestAsync(method, url, data, cancellationToken);
        if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Object)
        {
            return JsonSerializer.Deserialize<T>(dataProp.GetRawText());
        }
        return JsonSerializer.Deserialize<T>(root.GetRawText());
    }

    /// <summary>
    /// General method for making GraphQL requests to ZarinPal's API.
    /// </summary>
    public async Task<JsonElement> GraphqlAsync(string query, object? variables = null, CancellationToken cancellationToken = default)
    {
        var requestData = new { query, variables };
        var json = JsonSerializer.Serialize(requestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger?.LogDebug("Sending GraphQL request");
        var response = await GraphqlClient.PostAsync("", content, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync();

        return ParseAndValidateResponse(responseContent, response.StatusCode, isGraphql: true);
    }

    /// <summary>
    /// General method for making GraphQL requests to ZarinPal's API with typed response.
    /// </summary>
    public async Task<T?> GraphqlAsync<T>(string query, object? variables = null, string? dataPath = null, CancellationToken cancellationToken = default)
    {
        var root = await GraphqlAsync(query, variables, cancellationToken);
        if (root.TryGetProperty("data", out var dataProp))
        {
            if (!string.IsNullOrEmpty(dataPath) && dataProp.TryGetProperty(dataPath!, out var targetProp))
            {
                return JsonSerializer.Deserialize<T>(targetProp.GetRawText());
            }
            return JsonSerializer.Deserialize<T>(dataProp.GetRawText());
        }
        return JsonSerializer.Deserialize<T>(root.GetRawText());
    }

    private static JsonElement ParseAndValidateResponse(string responseContent, HttpStatusCode statusCode, bool isGraphql)
    {
        if (string.IsNullOrWhiteSpace(responseContent))
        {
            throw new ResponseException("API response body was empty.", statusCode);
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(responseContent);
        }
        catch (JsonException ex)
        {
            throw new ResponseException($"Failed to parse API response JSON: {ex.Message}", statusCode);
        }

        var root = doc.RootElement;

        if ((int)statusCode < 200 || (int)statusCode >= 300)
        {
            var errorMsg = ExtractErrorMessage(root) ?? $"API request failed with status code {statusCode}: {responseContent}";
            throw new ResponseException(errorMsg, statusCode);
        }

        if (isGraphql)
        {
            if (root.TryGetProperty("errors", out var errorsProp) &&
                errorsProp.ValueKind != JsonValueKind.Null &&
                errorsProp.ValueKind != JsonValueKind.Undefined)
            {
                if (errorsProp.ValueKind == JsonValueKind.Array && errorsProp.GetArrayLength() > 0)
                {
                    var errorMessages = new List<string>();
                    foreach (var err in errorsProp.EnumerateArray())
                    {
                        if (err.ValueKind == JsonValueKind.Object && err.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == JsonValueKind.String)
                        {
                            var msg = msgProp.GetString();
                            if (!string.IsNullOrEmpty(msg))
                            {
                                errorMessages.Add(msg!);
                            }
                        }
                        else
                        {
                            errorMessages.Add(err.ToString());
                        }
                    }
                    var combinedMsg = errorMessages.Count > 0 ? string.Join("; ", errorMessages) : errorsProp.ToString();
                    throw new ResponseException($"GraphQL error: {combinedMsg}", statusCode);
                }
                else if (errorsProp.ValueKind == JsonValueKind.Object)
                {
                    var msg = errorsProp.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : errorsProp.ToString();
                    throw new ResponseException($"GraphQL error: {msg}", statusCode);
                }
            }
        }
        else
        {
            if (root.TryGetProperty("errors", out var errorsProp) &&
                errorsProp.ValueKind != JsonValueKind.Null &&
                errorsProp.ValueKind != JsonValueKind.Undefined)
            {
                if (errorsProp.ValueKind == JsonValueKind.Array && errorsProp.GetArrayLength() > 0)
                {
                    var firstErr = errorsProp[0];
                    int code = -1;
                    string message = "REST API Error";
                    if (firstErr.ValueKind == JsonValueKind.Object)
                    {
                        if (firstErr.TryGetProperty("code", out var cProp) && cProp.ValueKind == JsonValueKind.Number)
                        {
                            code = cProp.GetInt32();
                        }
                        if (firstErr.TryGetProperty("message", out var mProp) && mProp.ValueKind == JsonValueKind.String)
                        {
                            message = mProp.GetString() ?? message;
                        }
                    }
                    else if (firstErr.ValueKind == JsonValueKind.String)
                    {
                        message = firstErr.GetString() ?? message;
                    }
                    throw new ZarinPalApiException(code, message, statusCode);
                }
                else if (errorsProp.ValueKind == JsonValueKind.Object)
                {
                    int code = -1;
                    string message = "REST API Error";
                    if (errorsProp.TryGetProperty("code", out var cProp) && cProp.ValueKind == JsonValueKind.Number)
                    {
                        code = cProp.GetInt32();
                    }
                    if (errorsProp.TryGetProperty("message", out var mProp) && mProp.ValueKind == JsonValueKind.String)
                    {
                        message = mProp.GetString() ?? message;
                    }
                    throw new ZarinPalApiException(code, message, statusCode);
                }
            }

            if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Object)
            {
                if (dataProp.TryGetProperty("code", out var codeProp) && codeProp.ValueKind == JsonValueKind.Number)
                {
                    int code = codeProp.GetInt32();
                    if (code != 100 && code != 101)
                    {
                        string message = dataProp.TryGetProperty("message", out var msgProp) && msgProp.ValueKind == JsonValueKind.String
                            ? msgProp.GetString() ?? $"API returned error code {code}."
                            : $"API returned error code {code}.";
                        throw new ZarinPalApiException(code, message, statusCode);
                    }
                }
            }
        }

        return root.Clone();
    }

    private static string? ExtractErrorMessage(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Object)
        {
            if (root.TryGetProperty("errors", out var errorsProp))
            {
                if (errorsProp.ValueKind == JsonValueKind.Array && errorsProp.GetArrayLength() > 0)
                {
                    var first = errorsProp[0];
                    if (first.ValueKind == JsonValueKind.Object && first.TryGetProperty("message", out var m))
                        return m.GetString();
                    return first.ToString();
                }
                if (errorsProp.ValueKind == JsonValueKind.Object && errorsProp.TryGetProperty("message", out var msg))
                {
                    return msg.GetString();
                }
            }
            if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Object)
            {
                if (dataProp.TryGetProperty("message", out var msg))
                {
                    return msg.GetString();
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the base URL configured for REST API calls.
    /// </summary>
    public string GetBaseUrl() => BaseUrl;

    /// <summary>
    /// Creates a payment request.
    /// </summary>
    public Task<PaymentResult> CreateAsync(PaymentRequest data, CancellationToken cancellationToken = default)
    {
        return Payments.CreateAsync(data, cancellationToken);
    }

    /// <summary>
    /// Calculates transaction fee.
    /// </summary>
    public Task<FeeCalculationResult> CalculateFeeAsync(FeeCalculationRequest data, CancellationToken cancellationToken = default)
    {
        return Payments.FeeCalculationAsync(data, cancellationToken);
    }

    /// <summary>
    /// Returns the redirect URL for a payment authority.
    /// </summary>
    public string GetRedirectUrl(string authority)
    {
        return Payments.GetRedirectUrl(authority);
    }

    /// <summary>
    /// Verifies a payment transaction.
    /// </summary>
    public Task<VerifyResult> VerifyAsync(VerificationRequest data, CancellationToken cancellationToken = default)
    {
        return Verifications.VerifyAsync(data, cancellationToken);
    }

    /// <summary>
    /// Inquires transaction status.
    /// </summary>
    public Task<InquiryResult> InquireAsync(InquiryRequest data, CancellationToken cancellationToken = default)
    {
        return Inquiries.InquireAsync(data, cancellationToken);
    }

    /// <summary>
    /// Reverses a payment transaction.
    /// </summary>
    public Task<ReversalResult> ReverseAsync(ReversalRequest data, CancellationToken cancellationToken = default)
    {
        return Reversals.ReverseAsync(data, cancellationToken);
    }

    /// <summary>
    /// Lists transactions.
    /// </summary>
    public Task<List<TransactionItem>> ListTransactionsAsync(TransactionListRequest data, CancellationToken cancellationToken = default)
    {
        return Transactions.ListAsync(data, cancellationToken);
    }

    /// <summary>
    /// Lists unverified payments.
    /// </summary>
    public Task<UnverifiedResult> ListUnverifiedAsync(CancellationToken cancellationToken = default)
    {
        return Unverified.ListAsync(cancellationToken);
    }

    /// <summary>
    /// Creates a refund request via GraphQL.
    /// </summary>
    public Task<RefundCreateResult> CreateRefundAsync(RefundCreateRequest data, CancellationToken cancellationToken = default)
    {
        return Refunds.CreateAsync(data, cancellationToken);
    }

    /// <summary>
    /// Retrieves a specific refund by ID via GraphQL.
    /// </summary>
    public Task<RefundItem> RetrieveRefundAsync(string refundId, CancellationToken cancellationToken = default)
    {
        return Refunds.RetrieveAsync(refundId, cancellationToken);
    }

    /// <summary>
    /// Lists refunds via GraphQL.
    /// </summary>
    public Task<List<RefundItem>> ListRefundsAsync(RefundListRequest data, CancellationToken cancellationToken = default)
    {
        return Refunds.ListAsync(data, cancellationToken);
    }

    /// <summary>
    /// Disposes HTTP clients if owned by this instance.
    /// </summary>
    public void Dispose()
    {
        if (_ownsHttpClients)
        {
            HttpClient.Dispose();
            GraphqlClient.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
