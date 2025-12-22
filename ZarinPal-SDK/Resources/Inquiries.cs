using ZarinPal.Validators;
using ZarinPal.Models;
using ZarinPal.Interfaces;
using System.Text.Json;

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
    /// <returns>The response from the API.</returns>
    public async Task<JsonElement> InquireAsync(InquiryRequest data)
    {
        // Validate input data
        Validator.ValidateAuthority(data.Authority);

        // Make the API request
        return await Client.RequestAsync("POST", _endpoint, data);
    }
}
