using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ZarinPal.Interfaces;
using ZarinPal.Resources;
using ZarinPal.Exceptions;
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

    public void Dispose()
    {
        HttpClient.Dispose();
        GraphqlClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
