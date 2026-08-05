using System.Net.Http.Headers;
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
public class ZarinPal : IZarinPalClient
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

        GraphqlClient = new HttpClient()
        {
            BaseAddress = new Uri("https://next.zarinpal.com/api/v4/graphql/")
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
                    jsonData[item.Key] = item.Value;
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

        if (!response.IsSuccessStatusCode)
        {
            throw new ResponseException($"API request failed with status code {response.StatusCode}: {responseContent}", response.StatusCode);
        }

        return JsonSerializer.Deserialize<JsonElement>(responseContent);
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

        if (!response.IsSuccessStatusCode)
        {
            throw new ResponseException($"GraphQL request failed with status code {response.StatusCode}: {responseContent}", response.StatusCode);
        }

        return JsonSerializer.Deserialize<JsonElement>(responseContent);
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
