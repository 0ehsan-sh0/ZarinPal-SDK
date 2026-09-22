using System.Text.Json.Serialization;

namespace ZarinPal.Enums;

/// <summary>
/// Refund method type for instant payouts.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RefundMethod
{
    /// <summary>
    /// PAYA bank transfer.
    /// </summary>
    PAYA,

    /// <summary>
    /// Card-to-card transfer.
    /// </summary>
    CARD
}
