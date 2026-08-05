using System.Text.Json.Serialization;
using ZarinPal.Enums;

namespace ZarinPal.Models;

/// <summary>
/// Request model for creating a refund via GraphQL.
/// </summary>
public class RefundCreateRequest
{
    /// <summary>
    /// Session ID of transaction to refund.
    /// </summary>
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Amount to refund.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Refund description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Refund payout method.
    /// </summary>
    [JsonPropertyName("method")]
    public RefundMethod? Method { get; set; }

    /// <summary>
    /// Reason for refund.
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

/// <summary>
/// Request model for listing refunds via GraphQL.
/// </summary>
public class RefundListRequest
{
    /// <summary>
    /// Terminal ID.
    /// </summary>
    [JsonPropertyName("terminalId")]
    public string TerminalId { get; set; } = string.Empty;

    /// <summary>
    /// Pagination limit.
    /// </summary>
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    /// <summary>
    /// Pagination offset.
    /// </summary>
    [JsonPropertyName("offset")]
    public int? Offset { get; set; }
}
