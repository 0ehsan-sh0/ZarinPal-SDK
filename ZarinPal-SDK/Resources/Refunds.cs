using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZarinPal.Validators;
using ZarinPal.Models;
using ZarinPal.Interfaces;
using ZarinPal.Enums;

namespace ZarinPal.Resources;

/// <summary>
/// Class representing the Refunds resource for handling refund operations via GraphQL.
/// </summary>
public class Refunds : BaseResource
{
    /// <summary>
    /// Creates an instance of Refunds.
    /// </summary>
    /// <param name="client">The ZarinPal client instance.</param>
    public Refunds(IZarinPalClient client) : base(client)
    {
    }

    /// <summary>
    /// Create a refund request via GraphQL.
    /// </summary>
    /// <param name="data">The refund request data.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The response from the GraphQL API.</returns>
    public async Task<RefundCreateResult> CreateAsync(RefundCreateRequest data, CancellationToken cancellationToken = default)
    {
        // Validate input data
        Validator.ValidateSessionId(data.SessionId);
        Validator.ValidateAmount(data.Amount);
        if (data.Method.HasValue)
        {
            Validator.ValidateMethod(data.Method);
        }
        if (!string.IsNullOrEmpty(data.Reason))
        {
            Validator.ValidateReason(data.Reason!);
        }

        const string query = @"
          mutation AddRefund(
            $session_id: ID!,
            $amount: BigInteger!,
            $description: String,
            $method: InstantPayoutActionTypeEnum,
            $reason: RefundReasonEnum
          ) {
            resource: AddRefund(
              session_id: $session_id,
              amount: $amount,
              description: $description,
              method: $method,
              reason: $reason
            ) {
              terminal_id,
              id,
              amount,
              timeline {
                refund_amount,
                refund_time,
                refund_status
              }
            }
          }
        ";

        var variables = new
        {
            session_id = data.SessionId,
            amount = data.Amount,
            description = data.Description,
            method = data.Method,
            reason = data.Reason,
        };

        var result = await Client.GraphqlAsync<RefundCreateResult>(query, variables, dataPath: "resource", cancellationToken: cancellationToken);
        return result ?? new RefundCreateResult();
    }

    /// <summary>
    /// Retrieve details of a specific refund.
    /// </summary>
    /// <param name="refundId">The ID of the refund to retrieve.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The response containing refund details.</returns>
    public async Task<RefundItem> RetrieveAsync(string refundId, CancellationToken cancellationToken = default)
    {
        const string query = @"
          query GetRefund($id: ID!) {
            refund: GetRefund(id: $id) {
              id,
              amount,
              status,
              created_at,
              description
            }
          }
        ";

        var variables = new
        {
            id = refundId,
        };

        var result = await Client.GraphqlAsync<RefundItem>(query, variables, dataPath: "refund", cancellationToken: cancellationToken);
        return result ?? new RefundItem();
    }

    /// <summary>
    /// List refunds with optional pagination.
    /// </summary>
    /// <param name="data">The listing parameters.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The response containing a list of refunds.</returns>
    public async Task<List<RefundItem>> ListAsync(RefundListRequest data, CancellationToken cancellationToken = default)
    {
        // Validate input data
        Validator.ValidateTerminalId(data.TerminalId);
        if (data.Limit.HasValue)
        {
            Validator.ValidateLimit(data.Limit);
        }
        if (data.Offset.HasValue)
        {
            Validator.ValidateOffset(data.Offset);
        }

        const string query = @"
          query GetRefunds($terminal_id: ID!, $limit: Int, $offset: Int) {
            refunds: GetRefunds(
              terminal_id: $terminal_id,
              limit: $limit,
              offset: $offset
            ) {
              id,
              amount,
              status,
              created_at,
              description
            }
          }
        ";

        var variables = new
        {
            terminal_id = data.TerminalId,
            limit = data.Limit,
            offset = data.Offset,
        };

        var result = await Client.GraphqlAsync<List<RefundItem>>(query, variables, dataPath: "refunds", cancellationToken: cancellationToken);
        return result ?? new List<RefundItem>();
    }
}
