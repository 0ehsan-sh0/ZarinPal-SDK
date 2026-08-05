using System.Text.Json.Serialization;

namespace ZarinPal.Models;

/// <summary>
/// Request model for payment inquiry.
/// </summary>
public class InquiryRequest
{
    /// <summary>
    /// Authority string to inquire about.
    /// </summary>
    [JsonPropertyName("authority")]
    public string Authority { get; set; } = string.Empty;
}
