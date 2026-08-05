using System.Threading;
using System.Threading.Tasks;
using ZarinPal.Validators;
using ZarinPal.Models;
using ZarinPal.Interfaces;

namespace ZarinPal.Resources;

/// <summary>
/// Class representing the Inquiries resource for checking transaction status.
/// </summary>
public class Inquiries : BaseResource
{
    private readonly string _endpoint = "/pg/v4/payment/inquiry.json";

    /// <summary>
    /// Creates an instance of Inquiries.
    /// </summary>
    /// <param name="client">The ZarinPal client instance.</param>
    public Inquiries(IZarinPalClient client) : base(client)
    {
    }

    /// <summary>
    /// Inquire about the status of a transaction.
    /// </summary>
    /// <param name="data">The inquiry data.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The inquiry result from the API.</returns>
    public async Task<InquiryResult> InquireAsync(InquiryRequest data, CancellationToken cancellationToken = default)
    {
        // Validate input data
        Validator.ValidateAuthority(data.Authority);

        // Make the API request
        var result = await Client.RequestAsync<InquiryResult>("POST", _endpoint, data, cancellationToken);
        return result ?? new InquiryResult();
    }
}
