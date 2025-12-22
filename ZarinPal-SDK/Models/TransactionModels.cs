using System.Text.Json.Serialization;

namespace ZarinPal.Models;

public class TransactionListRequest
{
    [JsonPropertyName("terminalId")]
    public string TerminalId { get; set; } = string.Empty;

    [JsonPropertyName("filter")]
    public string? Filter { get; set; }

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    [JsonPropertyName("offset")]
    public int? Offset { get; set; }
}
