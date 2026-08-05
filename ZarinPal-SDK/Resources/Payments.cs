using System.Threading;
using System.Threading.Tasks;
using ZarinPal.Validators;
using ZarinPal.Models;
using ZarinPal.Interfaces;

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
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The response from the API containing payment authority.</returns>
    public async Task<PaymentResult> CreateAsync(PaymentRequest data, CancellationToken cancellationToken = default)
    {
        // Validate input data
        Validator.ValidateAmount(data.Amount);
        Validator.ValidateCallbackUrl(data.CallbackUrl);
        Validator.ValidateMobile(data.Mobile);
        Validator.ValidateEmail(data.Email);

        // Make the API request
        var result = await Client.RequestAsync<PaymentResult>("POST", _endpoint, data, cancellationToken);
        return result ?? new PaymentResult();
    }

    /// <summary>
    /// Calculate the transaction fee before creating a payment request.
    /// </summary>
    /// <param name="data">The fee calculation request data.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The fee calculation response from the API.</returns>
    public async Task<FeeCalculationResult> FeeCalculationAsync(FeeCalculationRequest data, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(data.MerchantId))
        {
            Validator.ValidateMerchantId(data.MerchantId);
        }
        Validator.ValidateAmount(data.Amount);
        Validator.ValidateCurrency(data.Currency);

        var result = await Client.RequestAsync<FeeCalculationResult>("POST", "/pg/v4/payment/feeCalculation.json", data, cancellationToken);
        return result ?? new FeeCalculationResult();
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
