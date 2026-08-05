using System.Threading;
using System.Threading.Tasks;
using ZarinPal.Constants;
using ZarinPal.Validators;
using ZarinPal.Models;
using ZarinPal.Interfaces;

namespace ZarinPal.Resources;

/// <summary>
/// Class representing the Reversals resource for reversing transactions.
/// </summary>
public class Reversals : BaseResource
{
    /// <summary>
    /// Creates an instance of Reversals.
    /// </summary>
    /// <param name="client">The ZarinPal client instance.</param>
    public Reversals(IZarinPalClient client) : base(client)
    {
    }

    /// <summary>
    /// Reverse a transaction.
    /// </summary>
    /// <param name="data">The reversal request data.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The response from the API.</returns>
    public async Task<ReversalResult> ReverseAsync(ReversalRequest data, CancellationToken cancellationToken = default)
    {
        // Validate input data
        Validator.ValidateAuthority(data.Authority);

        // Make the API request
        var result = await Client.RequestAsync<ReversalResult>("POST", Endpoints.Reverse, data, cancellationToken);
        return result ?? new ReversalResult();
    }
}
