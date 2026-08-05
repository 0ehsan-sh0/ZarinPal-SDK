using System.Text.Json.Serialization;

namespace ZarinPal.Models;

/// <summary>
/// Request model for creating a payment.
/// </summary>
public class PaymentRequest
{
    /// <summary>
    /// Payment amount in IRR/Toman.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Callback URL after payment attempt.
    /// </summary>
    [JsonPropertyName("callback_url")]
    public string CallbackUrl { get; set; } = string.Empty;

    /// <summary>
    /// Description for payment.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Optional mobile number of payer.
    /// </summary>
    [JsonPropertyName("mobile")]
    public string? Mobile { get; set; }

    /// <summary>
    /// Optional email address of payer.
    /// </summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>
    /// Optional metadata payload.
    /// </summary>
    [JsonPropertyName("metadata")]
    public object? Metadata { get; set; }
}

/// <summary>
/// Request model for calculating transaction fee.
/// </summary>
public class FeeCalculationRequest
{
    /// <summary>
    /// Optional merchant ID.
    /// </summary>
    [JsonPropertyName("merchant_id")]
    public string? MerchantId { get; set; }

    /// <summary>
    /// Amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency (IRR or IRT).
    /// </summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }
}
