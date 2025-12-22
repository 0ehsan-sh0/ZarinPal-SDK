using ZarinPal.Validators;
using ZarinPal.Models;
using ZarinPal.Interfaces;
using System.Text.Json;

namespace ZarinPal.Resources;

/// <summary>
/// Class representing the Verifications resource for verifying payments.
/// </summary>
public class Verifications : BaseResource
{
    private readonly string _endpoint = "/pg/v4/payment/verify.json";

    /// <summary>
    /// Creates an instance of Verifications.
    /// </summary>
    /// <param name="client">The ZarinPal client instance.</param>
    public Verifications(IZarinPalClient client) : base(client)
    {
    }

    /// <summary>
    /// Verify a payment transaction.
    /// </summary>
    /// <param name="data">The verification data.</param>
    /// <returns>A promise resolving to the verification result.</returns>
    public async Task<JsonElement> VerifyAsync(VerificationRequest data)
    {
        // Validate input data
        Validator.ValidateAmount(data.Amount);
        Validator.ValidateAuthority(data.Authority);

        // Make the API request
        return await Client.RequestAsync("POST", _endpoint, data);
    }
}
