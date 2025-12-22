using System.Text.Json.Serialization;

namespace ZarinPal.Models;

public class VerificationRequest
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("authority")]
    public string Authority { get; set; } = string.Empty;
}
