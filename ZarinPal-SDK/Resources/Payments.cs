using ZarinPal.Validators;
using ZarinPal.Models;
using ZarinPal.Interfaces;
using System.Text.Json;

namespace ZarinPal.Resources;

/// <summary>
/// Class representing the Payments resource for creating payment requests.
/// </summary>
public class Payments : BaseResource
{
    private readonly string _endpoint = "/pg/v4/payment/request.json";
    private readonly string _startPayUrl = "/pg/StartPay/";

    /// <summary>
    /// Creates an instance of Payments.
    /// </summary>
    /// <param name="client">The ZarinPal client instance.</param>
    public Payments(IZarinPalClient client) : base(client)
    {
    }

    /// <summary>
    /// Create a payment request.
    /// </summary>
    /// <param name="data">The payment request data.</param>
    /// <returns>The response from the API.</returns>
    public async Task<JsonElement> CreateAsync(PaymentRequest data)
    {
        // Validate input data
        Validator.ValidateAmount(data.Amount);
        Validator.ValidateCallbackUrl(data.CallbackUrl);
        Validator.ValidateMobile(data.Mobile);
        Validator.ValidateEmail(data.Email);

        // Make the API request
        return await Client.RequestAsync("POST", _endpoint, data);
    }

    /// <summary>
    /// Calculate the transaction fee before creating a payment request.
    /// </summary>
    /// <param name="data">The fee calculation request data.</param>
    /// <returns>The response from the API.</returns>
    public async Task<JsonElement> FeeCalculationAsync(FeeCalculationRequest data)
    {
        Validator.ValidateMerchantId(data.MerchantId);
        Validator.ValidateAmount(data.Amount);
        Validator.ValidateCurrency(data.Currency);

        return await Client.RequestAsync("POST", "/pg/v4/payment/feeCalculation.json", data);
    }

    /// <summary>
    /// Get the redirect URL for the payment.
    /// </summary>
    /// <param name="authority">The authority code returned from create request.</param>
    /// <returns>The full redirect URL.</returns>
    public string GetRedirectUrl(string authority)
    {
        var baseUrl = Client.GetBaseUrl();
        return $"{baseUrl}{_startPayUrl}{authority}";
    }
}
