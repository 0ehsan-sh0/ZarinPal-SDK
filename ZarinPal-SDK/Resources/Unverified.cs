using ZarinPal.Interfaces;
using System.Text.Json;

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
    /// <returns>A promise resolving to the list of unverified payments.</returns>
    public async Task<JsonElement> ListAsync()
    {
        // Make the API request
        return await Client.RequestAsync("POST", _endpoint, new { });
    }
}
