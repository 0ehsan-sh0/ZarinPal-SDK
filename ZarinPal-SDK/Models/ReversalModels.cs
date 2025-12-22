using System.Text.Json.Serialization;

namespace ZarinPal.Models;

public class ReversalRequest
{
    [JsonPropertyName("authority")]
    public string Authority { get; set; } = string.Empty;
}
