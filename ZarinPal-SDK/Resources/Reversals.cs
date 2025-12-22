using ZarinPal.Validators;
using ZarinPal.Models;
using ZarinPal.Interfaces;
using System.Text.Json;

namespace ZarinPal.Resources;

/// <summary>
/// Class representing the Reversals resource for reversing transactions.
/// </summary>
public class Reversals : BaseResource
{
    private readonly string _endpoint = "/pg/v4/payment/reverse.json";

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
    /// <returns>The response from the API.</returns>
    public async Task<JsonElement> ReverseAsync(ReversalRequest data)
    {
        // Validate input data
        Validator.ValidateAuthority(data.Authority);

        // Make the API request
        return await Client.RequestAsync("POST", _endpoint, data);
    }
}
