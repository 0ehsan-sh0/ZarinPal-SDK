using System.Text.Json.Serialization;

namespace ZarinPal.Models;

/// <summary>
/// Information about a wage split item.
/// </summary>
public class Wage
{
    /// <summary>
    /// IBAN string for wage payout destination.
    /// </summary>
    [JsonPropertyName("iban")]
    public string Iban { get; set; } = string.Empty;

    /// <summary>
    /// Wage amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Wage description.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
