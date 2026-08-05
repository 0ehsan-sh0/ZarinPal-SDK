using System.Threading.Tasks;
using ZarinPal.Validators;
using ZarinPal.Models;
using ZarinPal.Interfaces;
using System.Text.Json;

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
    /// <returns>A promise resolving to the list of transactions.</returns>
    public async Task<JsonElement> ListAsync(TransactionListRequest data)
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

        // Make the GraphQL request
        return await Client.GraphqlAsync(query, variables);
    }
}
