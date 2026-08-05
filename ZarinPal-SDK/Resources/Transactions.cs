using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZarinPal.Validators;
using ZarinPal.Models;
using ZarinPal.Interfaces;

namespace ZarinPal.Resources;

/// <summary>
/// Class representing the Transactions resource for fetching transaction information via GraphQL.
/// </summary>
public class Transactions : BaseResource
{
    /// <summary>
    /// Creates an instance of Transactions.
    /// </summary>
    /// <param name="client">The ZarinPal client instance.</param>
    public Transactions(IZarinPalClient client) : base(client)
    {
    }

    /// <summary>
    /// Retrieve a list of transactions via GraphQL.
    /// </summary>
    /// <param name="data">The transaction query parameters.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of transaction items.</returns>
    public async Task<List<TransactionItem>> ListAsync(TransactionListRequest data, CancellationToken cancellationToken = default)
    {
        // Validate input data
        Validator.ValidateTerminalId(data.TerminalId);
        if (!string.IsNullOrEmpty(data.Filter))
        {
            Validator.ValidateFilter(data.Filter);
        }
        if (data.Limit.HasValue)
        {
            Validator.ValidateLimit(data.Limit);
        }
        if (data.Offset.HasValue)
        {
            Validator.ValidateOffset(data.Offset);
        }

        const string query = @"
          query GetTransactions($terminal_id: ID!, $filter: String, $limit: Int, $offset: Int) {
            transactions: GetTransactions(
              terminal_id: $terminal_id,
              filter: $filter,
              limit: $limit,
              offset: $offset
            ) {
              id,
              status,
              amount,
              description,
              created_at
            }
          }
        ";

        var variables = new
        {
            terminal_id = data.TerminalId,
            filter = data.Filter,
            limit = data.Limit,
            offset = data.Offset,
        };

        var result = await Client.GraphqlAsync<List<TransactionItem>>(query, variables, dataPath: "transactions", cancellationToken: cancellationToken);
        return result ?? new List<TransactionItem>();
    }
}
