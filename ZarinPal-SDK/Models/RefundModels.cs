using System.Text.Json.Serialization;

namespace ZarinPal.Models;

public class RefundCreateRequest
{
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

public class RefundListRequest
{
    [JsonPropertyName("terminalId")]
    public string TerminalId { get; set; } = string.Empty;

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    [JsonPropertyName("offset")]
    public int? Offset { get; set; }
}
