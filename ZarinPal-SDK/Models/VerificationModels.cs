using System.Text.Json.Serialization;

namespace ZarinPal.Models;

/// <summary>
/// Model for requesting payment verification.
/// </summary>
public class VerificationRequest
{
    /// <summary>
    /// Expected payment amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Authority string returned from payment creation.
    /// </summary>
    [JsonPropertyName("authority")]
    public string Authority { get; set; } = string.Empty;
}
