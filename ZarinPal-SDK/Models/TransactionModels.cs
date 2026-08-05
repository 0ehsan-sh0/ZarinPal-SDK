using System.Text.Json.Serialization;

namespace ZarinPal.Models;

/// <summary>
/// Model for querying transaction list via GraphQL.
/// </summary>
public class TransactionListRequest
{
    /// <summary>
    /// Terminal ID.
    /// </summary>
    [JsonPropertyName("terminalId")]
    public string TerminalId { get; set; } = string.Empty;

    /// <summary>
    /// Optional transaction filter (e.g. PAID, VERIFIED).
    /// </summary>
    [JsonPropertyName("filter")]
    public string? Filter { get; set; }

    /// <summary>
    /// Maximum number of records to return.
    /// </summary>
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    /// <summary>
    /// Offset for pagination.
    /// </summary>
    [JsonPropertyName("offset")]
    public int? Offset { get; set; }
}
