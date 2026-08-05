using System.Threading;
using System.Threading.Tasks;
using ZarinPal.Constants;
using ZarinPal.Validators;
using ZarinPal.Models;
using ZarinPal.Interfaces;

namespace ZarinPal.Resources;

/// <summary>
/// Class representing the Verifications resource for verifying payments.
/// </summary>
public class Verifications : BaseResource
{
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
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The verification result.</returns>
    public async Task<VerifyResult> VerifyAsync(VerificationRequest data, CancellationToken cancellationToken = default)
    {
        // Validate input data
        Validator.ValidateAmount(data.Amount);
        Validator.ValidateAuthority(data.Authority);

        // Make the API request
        var result = await Client.RequestAsync<VerifyResult>("POST", Endpoints.Verify, data, cancellationToken);
        return result ?? new VerifyResult();
    }
}
