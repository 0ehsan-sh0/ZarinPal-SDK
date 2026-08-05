using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZarinPal.Models;
using ZarinPal.Resources;

namespace ZarinPal.Interfaces;

/// <summary>
/// Main interface for ZarinPal SDK operations.
/// </summary>
public interface IZarinPal
{
    /// <summary>
    /// Payments resource.
    /// </summary>
    Payments Payments { get; }

    /// <summary>
    /// Refunds resource.
    /// </summary>
    Refunds Refunds { get; }

    /// <summary>
    /// Transactions resource.
    /// </summary>
    Transactions Transactions { get; }

    /// <summary>
    /// Verifications resource.
    /// </summary>
    Verifications Verifications { get; }

    /// <summary>
    /// Reversals resource.
    /// </summary>
    Reversals Reversals { get; }

    /// <summary>
    /// Unverified payments resource.
    /// </summary>
    Unverified Unverified { get; }

    /// <summary>
    /// Inquiries resource.
    /// </summary>
    Inquiries Inquiries { get; }

    /// <summary>
    /// Gets base URL used for requests.
    /// </summary>
    string GetBaseUrl();

    /// <summary>
    /// Creates a payment request.
    /// </summary>
    Task<PaymentResult> CreateAsync(PaymentRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates transaction fee.
    /// </summary>
    Task<FeeCalculationResult> CalculateFeeAsync(FeeCalculationRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns redirect URL for an authority.
    /// </summary>
    string GetRedirectUrl(string authority);

    /// <summary>
    /// Verifies a payment.
    /// </summary>
    Task<VerifyResult> VerifyAsync(VerificationRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inquires transaction status.
    /// </summary>
    Task<InquiryResult> InquireAsync(InquiryRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reverses a payment transaction.
    /// </summary>
    Task<ReversalResult> ReverseAsync(ReversalRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists transactions via GraphQL.
    /// </summary>
    Task<List<TransactionItem>> ListTransactionsAsync(TransactionListRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists unverified payments.
    /// </summary>
    Task<UnverifiedResult> ListUnverifiedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a refund request via GraphQL.
    /// </summary>
    Task<RefundCreateResult> CreateRefundAsync(RefundCreateRequest data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a refund by ID via GraphQL.
    /// </summary>
    Task<RefundItem> RetrieveRefundAsync(string refundId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists refunds via GraphQL.
    /// </summary>
    Task<List<RefundItem>> ListRefundsAsync(RefundListRequest data, CancellationToken cancellationToken = default);
}
