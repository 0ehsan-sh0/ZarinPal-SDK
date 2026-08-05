using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using ZarinPal.Enums;
using ZarinPal.Models;
using ZarinPal.SDK.IntegrationTests.Helpers;

namespace ZarinPal.SDK.IntegrationTests;

public class GraphqlResourceIntegrationTests
{
    private readonly MockHttpMessageHandler _handler;
    private readonly ZarinPal _client;

    public GraphqlResourceIntegrationTests()
    {
        _handler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(_handler)
        {
            BaseAddress = new Uri("https://sandbox.zarinpal.com/api/v4/graphql/")
        };
        var config = new Config
        {
            MerchantId = "c1234567-89ab-cdef-0123-456789abcdef",
            Sandbox = true,
            AccessToken = "test_access_token"
        };
        _client = new ZarinPal(config, httpClient, httpClient);
    }

    [Fact]
    public async Task Refunds_CreateAsync_SendsGraphqlMutationAndParsesResult()
    {
        var responseJson = @"{
            ""data"": {
                ""resource"": {
                    ""id"": ""ref_123"",
                    ""terminal_id"": ""term_99"",
                    ""amount"": 5000,
                    ""timeline"": [
                        {
                            ""refund_amount"": 5000,
                            ""refund_time"": ""2026-08-05T12:00:00Z"",
                            ""refund_status"": ""SUCCESS""
                        }
                    ]
                }
            }
        }";
        _handler.EnqueueResponse(HttpStatusCode.OK, responseJson);

        var request = new RefundCreateRequest
        {
            SessionId = "sess_001",
            Amount = 5000,
            Description = "Customer requested refund",
            Method = RefundMethod.CARD,
            Reason = "CUSTOMER_REQUEST"
        };

        var result = await _client.Refunds.CreateAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().Be("ref_123");
        result.TerminalId.Should().Be("term_99");
        result.Amount.Should().Be(5000);

        _handler.Requests.Should().HaveCount(1);
        _handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        _handler.RequestBodies[0].Should().Contain("AddRefund");
        _handler.RequestBodies[0].Should().Contain("sess_001");
    }

    [Fact]
    public async Task Refunds_RetrieveAsync_SendsGraphqlQueryAndParsesResult()
    {
        var responseJson = @"{
            ""data"": {
                ""refund"": {
                    ""id"": ""ref_123"",
                    ""amount"": 5000,
                    ""status"": ""COMPLETED"",
                    ""created_at"": ""2026-08-05T12:00:00Z"",
                    ""description"": ""Refund 123""
                }
            }
        }";
        _handler.EnqueueResponse(HttpStatusCode.OK, responseJson);

        var result = await _client.Refunds.RetrieveAsync("ref_123");

        result.Should().NotBeNull();
        result.Id.Should().Be("ref_123");
        result.Amount.Should().Be(5000);
        result.Status.Should().Be("COMPLETED");

        _handler.RequestBodies[0].Should().Contain("GetRefund");
        _handler.RequestBodies[0].Should().Contain("ref_123");
    }

    [Fact]
    public async Task Refunds_ListAsync_SendsGraphqlQueryAndParsesList()
    {
        var responseJson = @"{
            ""data"": {
                ""refunds"": [
                    {
                        ""id"": ""ref_001"",
                        ""amount"": 1000,
                        ""status"": ""COMPLETED"",
                        ""created_at"": ""2026-08-05T10:00:00Z"",
                        ""description"": ""Refund 1""
                    },
                    {
                        ""id"": ""ref_002"",
                        ""amount"": 2000,
                        ""status"": ""PENDING"",
                        ""created_at"": ""2026-08-05T11:00:00Z"",
                        ""description"": ""Refund 2""
                    }
                ]
            }
        }";
        _handler.EnqueueResponse(HttpStatusCode.OK, responseJson);

        var request = new RefundListRequest
        {
            TerminalId = "term_001",
            Limit = 10,
            Offset = 0
        };

        var result = await _client.Refunds.ListAsync(request);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("ref_001");
        result[1].Id.Should().Be("ref_002");

        _handler.RequestBodies[0].Should().Contain("GetRefunds");
        _handler.RequestBodies[0].Should().Contain("term_001");
    }

    [Fact]
    public async Task Transactions_ListAsync_SendsGraphqlQueryAndParsesList()
    {
        var responseJson = @"{
            ""data"": {
                ""transactions"": [
                    {
                        ""id"": ""tx_001"",
                        ""status"": ""VERIFIED"",
                        ""amount"": 15000,
                        ""description"": ""Tx 1"",
                        ""created_at"": ""2026-08-05T09:00:00Z""
                    }
                ]
            }
        }";
        _handler.EnqueueResponse(HttpStatusCode.OK, responseJson);

        var request = new TransactionListRequest
        {
            TerminalId = "term_001",
            Filter = "VERIFIED",
            Limit = 5,
            Offset = 0
        };

        var result = await _client.Transactions.ListAsync(request);

        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("tx_001");
        result[0].Status.Should().Be("VERIFIED");

        _handler.RequestBodies[0].Should().Contain("GetTransactions");
        _handler.RequestBodies[0].Should().Contain("VERIFIED");
    }
}
