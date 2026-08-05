using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ZarinPal.Models;

/// <summary>
/// Response model for payment creation request.
/// </summary>
public class PaymentResult
{
    /// <summary>
    /// Response status code (e.g. 100 for success, 101 for already verified).
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// Message associated with the response code.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Payment authority string to redirect the user.
    /// </summary>
    [JsonPropertyName("authority")]
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Fee calculation type if returned by API.
    /// </summary>
    [JsonPropertyName("fee_type")]
    public string? FeeType { get; set; }

    /// <summary>
    /// Fee amount deducted for this transaction.
    /// </summary>
    [JsonPropertyName("fee")]
    public decimal? Fee { get; set; }

    /// <summary>
    /// List of wages attached to this payment.
    /// </summary>
    [JsonPropertyName("wages")]
    public List<Wage>? Wages { get; set; }
}

/// <summary>
/// Response model for fee calculation request.
/// </summary>
public class FeeCalculationResult
{
    /// <summary>
    /// Transaction fee amount.
    /// </summary>
    [JsonPropertyName("fee")]
    public decimal Fee { get; set; }

    /// <summary>
    /// Fee type classification.
    /// </summary>
    [JsonPropertyName("fee_type")]
    public string? FeeType { get; set; }
}

/// <summary>
/// Response model for payment verification.
/// </summary>
public class VerifyResult
{
    /// <summary>
    /// Response status code (100 = success, 101 = verified before).
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// Message associated with the status code.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Unique reference ID for the completed payment.
    /// </summary>
    [JsonPropertyName("ref_id")]
    public long? RefId { get; set; }

    /// <summary>
    /// Masked card PAN used for payment.
    /// </summary>
    [JsonPropertyName("card_pan")]
    public string? CardPan { get; set; }

    /// <summary>
    /// Hash of the card PAN used for payment.
    /// </summary>
    [JsonPropertyName("card_hash")]
    public string? CardHash { get; set; }

    /// <summary>
    /// Fee type for the verified payment.
    /// </summary>
    [JsonPropertyName("fee_type")]
    public string? FeeType { get; set; }

    /// <summary>
    /// Fee amount for the verified payment.
    /// </summary>
    [JsonPropertyName("fee")]
    public decimal? Fee { get; set; }

    /// <summary>
    /// Wages split for the transaction.
    /// </summary>
    [JsonPropertyName("wages")]
    public List<Wage>? Wages { get; set; }
}

/// <summary>
/// Response model for payment inquiry.
/// </summary>
public class InquiryResult
{
    /// <summary>
    /// Response code from inquiry.
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// Message associated with the response code.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Authority string inquired about.
    /// </summary>
    [JsonPropertyName("authority")]
    public string? Authority { get; set; }

    /// <summary>
    /// Transaction amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    /// <summary>
    /// Transaction status.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Reference ID if transaction was successful.
    /// </summary>
    [JsonPropertyName("ref_id")]
    public long? RefId { get; set; }

    /// <summary>
    /// Masked card PAN.
    /// </summary>
    [JsonPropertyName("card_pan")]
    public string? CardPan { get; set; }

    /// <summary>
    /// Card PAN hash.
    /// </summary>
    [JsonPropertyName("card_hash")]
    public string? CardHash { get; set; }

    /// <summary>
    /// Fee type.
    /// </summary>
    [JsonPropertyName("fee_type")]
    public string? FeeType { get; set; }

    /// <summary>
    /// Fee amount.
    /// </summary>
    [JsonPropertyName("fee")]
    public decimal? Fee { get; set; }

    /// <summary>
    /// List of wages associated with the transaction.
    /// </summary>
    [JsonPropertyName("wages")]
    public List<Wage>? Wages { get; set; }
}

/// <summary>
/// Response model for payment reversal.
/// </summary>
public class ReversalResult
{
    /// <summary>
    /// Response code (100 = success).
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// Message associated with response code.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Response model for unverified payments list.
/// </summary>
public class UnverifiedResult
{
    /// <summary>
    /// Response status code.
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// Message associated with response code.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// List of unverified authorities.
    /// </summary>
    [JsonPropertyName("authorities")]
    public List<UnverifiedAuthorityItem>? Authorities { get; set; }
}

/// <summary>
/// Item in unverified payments list.
/// </summary>
public class UnverifiedAuthorityItem
{
    /// <summary>
    /// Authority string.
    /// </summary>
    [JsonPropertyName("authority")]
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Transaction amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Callback URL registered for payment.
    /// </summary>
    [JsonPropertyName("callback_url")]
    public string? CallbackUrl { get; set; }

    /// <summary>
    /// Referer header/domain.
    /// </summary>
    [JsonPropertyName("referer")]
    public string? Referer { get; set; }

    /// <summary>
    /// Date timestamp string.
    /// </summary>
    [JsonPropertyName("date")]
    public string? Date { get; set; }
}

/// <summary>
/// Response model for creating a refund via GraphQL.
/// </summary>
public class RefundCreateResult
{
    /// <summary>
    /// Refund ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Terminal ID.
    /// </summary>
    [JsonPropertyName("terminal_id")]
    public string? TerminalId { get; set; }

    /// <summary>
    /// Refund amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Refund timeline entries.
    /// </summary>
    [JsonPropertyName("timeline")]
    public List<RefundTimelineItem>? Timeline { get; set; }
}

/// <summary>
/// Refund timeline entry.
/// </summary>
public class RefundTimelineItem
{
    /// <summary>
    /// Refund amount for this timeline entry.
    /// </summary>
    [JsonPropertyName("refund_amount")]
    public decimal RefundAmount { get; set; }

    /// <summary>
    /// Timestamp of refund timeline entry.
    /// </summary>
    [JsonPropertyName("refund_time")]
    public string? RefundTime { get; set; }

    /// <summary>
    /// Status of refund timeline entry.
    /// </summary>
    [JsonPropertyName("refund_status")]
    public string? RefundStatus { get; set; }
}

/// <summary>
/// Response model for refund details via GraphQL.
/// </summary>
public class RefundItem
{
    /// <summary>
    /// Refund ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Refund amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Refund status.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Creation timestamp string.
    /// </summary>
    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    /// <summary>
    /// Refund description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// Response model for transaction details via GraphQL.
/// </summary>
public class TransactionItem
{
    /// <summary>
    /// Transaction ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Transaction status.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Transaction amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Transaction description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Transaction creation timestamp string.
    /// </summary>
    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }
}
