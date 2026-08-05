using System.Threading;
using System.Threading.Tasks;
using ZarinPal.Interfaces;
using ZarinPal.Models;

namespace ZarinPal.Resources;

/// <summary>
/// Class representing the Unverified resource for fetching unverified payments.
/// </summary>
public class Unverified : BaseResource
{
    private readonly string _endpoint = "/pg/v4/payment/unVerified.json";

    /// <summary>
    /// Creates an instance of Unverified.
    /// </summary>
    /// <param name="client">The ZarinPal client instance.</param>
    public Unverified(IZarinPalClient client) : base(client)
    {
    }

    /// <summary>
    /// Retrieve a list of unverified payments.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The unverified payments result.</returns>
    public async Task<UnverifiedResult> ListAsync(CancellationToken cancellationToken = default)
    {
        // Make the API request
        var result = await Client.RequestAsync<UnverifiedResult>("POST", _endpoint, new { }, cancellationToken);
        return result ?? new UnverifiedResult();
    }
}
