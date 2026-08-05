using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using ZarinPal.Interfaces;
using ZarinPal.Resources;
using ZarinPal.Exceptions;
using ZarinPal.Models;
using System.Net;

namespace ZarinPal;

/// <summary>
/// Main class for interacting with ZarinPal APIs.
/// Provides access to various resources such as payments, refunds, transactions, etc.
/// </summary>
public class ZarinPal : IZarinPal,IZarinPalClient
{
    public Payments Payments { get; }
    public Refunds Refunds { get; }
    public Transactions Transactions { get; }
    public Verifications Verifications { get; }
    public Reversals Reversals { get; }
    public Unverified Unverified { get; }
    public Inquiries Inquiries { get; }

    private Config Config { get; }
    private HttpClient HttpClient { get; }
    private HttpClient GraphqlClient { get; }
    private string BaseUrl { get; }

    /// <summary>
    /// Creates an instance of ZarinPal.
    /// </summary>
    /// <param name="config">The configuration object.</param>
    public ZarinPal(Config config)
    {
        Config = config;

        BaseUrl = Config.Sandbox
            ? "https://sandbox.zarinpal.com"
            : "https://payment.zarinpal.com";

        HttpClient = new HttpClient()
        {
            BaseAddress = new Uri(BaseUrl)
        };
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ZarinPalSdk/v1 (.NET)");
        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var graphqlBaseUrl = Config.Sandbox
            ? "https://sandbox.zarinpal.com/api/v4/graphql/"
            : "https://next.zarinpal.com/api/v4/graphql/";

        GraphqlClient = new HttpClient()
        {
            BaseAddress = new Uri(graphqlBaseUrl)
        };
        GraphqlClient.DefaultRequestHeaders.UserAgent.ParseAdd("ZarinPalSdk/v1 (.NET)");
        GraphqlClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

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
    /// General method for making HTTP requests to ZarinPal's REST API.
    /// </summary>
    public async Task<JsonElement> RequestAsync(string method, string url, object? data = null)
    {
        var jsonData = new Dictionary<string, object?>();
        if (!string.IsNullOrEmpty(Config.MerchantId))
        {
            jsonData["merchant_id"] = Config.MerchantId;
        }

        if (data != null)
        {
            var serializedData = JsonSerializer.Serialize(data);
            var dataDict = JsonSerializer.Deserialize<Dictionary<string, object?>>(serializedData);
            if (dataDict != null)
            {
                foreach (var item in dataDict)
                {
                    if (item.Key == "merchant_id")
                    {
                        var valStr = item.Value?.ToString();
                        if (!string.IsNullOrEmpty(valStr))
                        {
                            jsonData["merchant_id"] = valStr;
                        }
                    }
                    else
                    {
                        jsonData[item.Key] = item.Value;
                    }
                }
            }
        }

        var json = JsonSerializer.Serialize(jsonData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(new HttpMethod(method), url)
        {
            Content = content
        };

        var response = await HttpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        return ParseAndValidateResponse(responseContent, response.StatusCode, isGraphql: false);
    }

    /// <summary>
    /// General method for making GraphQL requests to ZarinPal's API.
    /// </summary>
    public async Task<JsonElement> GraphqlAsync(string query, object? variables = null)
    {
        var requestData = new { query, variables };
        var json = JsonSerializer.Serialize(requestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await GraphqlClient.PostAsync("", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        return ParseAndValidateResponse(responseContent, response.StatusCode, isGraphql: true);
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

    public string GetBaseUrl() => BaseUrl;

    /// <summary>
    /// Convenience method for creating a payment request.
    /// Equivalent to <see cref="Payments.CreateAsync(PaymentRequest)"/>.
    /// </summary>
    public Task<JsonElement> CreateAsync(PaymentRequest data)
    {
        return Payments.CreateAsync(data);
    }

    /// <summary>
    /// Convenience method for calculating the transaction fee.
    /// Equivalent to <see cref="Payments.FeeCalculationAsync(FeeCalculationRequest)"/>.
    /// </summary>
    public Task<JsonElement> CalculateFeeAsync(FeeCalculationRequest data)
    {
        return Payments.FeeCalculationAsync(data);
    }

    /// <summary>
    /// Convenience method for getting the payment redirect URL.
    /// Equivalent to <see cref="Payments.GetRedirectUrl(string)"/>.
    /// </summary>
    public string GetRedirectUrl(string authority)
    {
        return Payments.GetRedirectUrl(authority);
    }

    /// <summary>
    /// Convenience method for verifying a payment transaction.
    /// Equivalent to <see cref="Verifications.VerifyAsync(VerificationRequest)"/>.
    /// </summary>
    public Task<JsonElement> VerifyAsync(VerificationRequest data)
    {
        return Verifications.VerifyAsync(data);
    }

    /// <summary>
    /// Convenience method for inquiring about a transaction's status.
    /// Equivalent to <see cref="Inquiries.InquireAsync(InquiryRequest)"/>.
    /// </summary>
    public Task<JsonElement> InquireAsync(InquiryRequest data)
    {
        return Inquiries.InquireAsync(data);
    }

    /// <summary>
    /// Convenience method for reversing a transaction.
    /// Equivalent to <see cref="Reversals.ReverseAsync(ReversalRequest)"/>.
    /// </summary>
    public Task<JsonElement> ReverseAsync(ReversalRequest data)
    {
        return Reversals.ReverseAsync(data);
    }

    /// <summary>
    /// Convenience method for listing transactions.
    /// Equivalent to <see cref="Transactions.ListAsync(TransactionListRequest)"/>.
    /// </summary>
    public Task<JsonElement> ListTransactionsAsync(TransactionListRequest data)
    {
        return Transactions.ListAsync(data);
    }

    /// <summary>
    /// Convenience method for listing unverified payments.
    /// Equivalent to <see cref="Unverified.ListAsync"/>.
    /// </summary>
    public Task<JsonElement> ListUnverifiedAsync()
    {
        return Unverified.ListAsync();
    }

    /// <summary>
    /// Convenience method for creating a refund request.
    /// Equivalent to <see cref="Refunds.CreateAsync(RefundCreateRequest)"/>.
    /// </summary>
    public Task<JsonElement> CreateRefundAsync(RefundCreateRequest data)
    {
        return Refunds.CreateAsync(data);
    }

    /// <summary>
    /// Convenience method for retrieving a specific refund.
    /// Equivalent to <see cref="Refunds.RetrieveAsync(string)"/>.
    /// </summary>
    public Task<JsonElement> RetrieveRefundAsync(string refundId)
    {
        return Refunds.RetrieveAsync(refundId);
    }

    /// <summary>
    /// Convenience method for listing refunds.
    /// Equivalent to <see cref="Refunds.ListAsync(RefundListRequest)"/>.
    /// </summary>
    public Task<JsonElement> ListRefundsAsync(RefundListRequest data)
    {
        return Refunds.ListAsync(data);
    }

    public void Dispose()
    {
        HttpClient.Dispose();
        GraphqlClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
