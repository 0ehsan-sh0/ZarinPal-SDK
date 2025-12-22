namespace ZarinPal;

/// <summary>
/// Configuration for ZarinPal SDK
/// </summary>
public class Config
{
    /// <summary>
    /// Your merchant ID provided by ZarinPal.
    /// </summary>
    public string? MerchantId { get; set; }

    /// <summary>
    /// Access token for authentication (used for GraphQL requests).
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Whether to use the sandbox environment.
    /// </summary>
    public bool Sandbox { get; set; } = false;
}
