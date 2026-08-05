using System.Text.Json.Serialization;

namespace ZarinPal.Models;

/// <summary>
/// Model for requesting a transaction reversal.
/// </summary>
public class ReversalRequest
{
    /// <summary>
    /// Authority string of transaction to reverse.
    /// </summary>
    [JsonPropertyName("authority")]
    public string Authority { get; set; } = string.Empty;
}
