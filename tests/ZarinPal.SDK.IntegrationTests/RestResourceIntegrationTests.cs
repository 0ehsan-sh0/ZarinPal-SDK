using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using ZarinPal.Constants;
using ZarinPal.Models;
using ZarinPal.SDK.IntegrationTests.Helpers;

namespace ZarinPal.SDK.IntegrationTests;

public class RestResourceIntegrationTests
{
    private readonly MockHttpMessageHandler _handler;
    private readonly ZarinPal _client;

    public RestResourceIntegrationTests()
    {
        _handler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(_handler)
        {
            BaseAddress = new Uri("https://sandbox.zarinpal.com")
        };
        var config = new Config
        {
            MerchantId = "c1234567-89ab-cdef-0123-456789abcdef",
            Sandbox = true
        };
        _client = new ZarinPal(config, httpClient, httpClient);
    }

    [Fact]
    public async Task Payments_CreateAsync_SendsPostAndParsesResult()
    {
        var responseJson = @"{
            ""data"": {
                ""code"": 100,
                ""message"": ""Success"",
                ""authority"": ""A00000000000000000000000000000000000"",
                ""fee_type"": ""Merchant"",
                ""fee"": 100
            }
        }";
        _handler.EnqueueResponse(HttpStatusCode.OK, responseJson);

        var request = new PaymentRequest
        {
            Amount = 10000,
            CallbackUrl = "https://example.com/callback",
            Description = "Order #101",
            Mobile = "09123456789"
        };

        var result = await _client.Payments.CreateAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(100);
        result.Authority.Should().Be("A00000000000000000000000000000000000");

        _handler.Requests.Should().HaveCount(1);
        _handler.Requests[0].Method.Should().Be(HttpMethod.Post);
        _handler.Requests[0].RequestUri!.AbsolutePath.Should().Be(Endpoints.PaymentRequest);
        _handler.RequestBodies[0].Should().Contain("\"merchant_id\":\"c1234567-89ab-cdef-0123-456789abcdef\"");
        _handler.RequestBodies[0].Should().Contain("\"amount\":10000");
    }

    [Fact]
    public async Task Payments_FeeCalculationAsync_SendsPostAndParsesResult()
    {
        var responseJson = @"{
            ""data"": {
                ""fee"": 250,
                ""fee_type"": ""Customer""
            }
        }";
        _handler.EnqueueResponse(HttpStatusCode.OK, responseJson);

        var request = new FeeCalculationRequest
        {
            Amount = 50000,
            Currency = "IRR"
        };

        var result = await _client.Payments.FeeCalculationAsync(request);

        result.Should().NotBeNull();
        result.Fee.Should().Be(250);
        result.FeeType.Should().Be("Customer");

        _handler.Requests[0].RequestUri!.AbsolutePath.Should().Be(Endpoints.FeeCalculation);
    }

    [Fact]
    public async Task Verifications_VerifyAsync_SendsPostAndParsesResult()
    {
        var responseJson = @"{
            ""data"": {
                ""code"": 100,
                ""message"": ""Verified"",
                ""ref_id"": 123456,
                ""card_pan"": ""502229******1234"",
                ""card_hash"": ""hash"",
                ""fee_type"": ""Merchant"",
                ""fee"": 100
            }
        }";
        _handler.EnqueueResponse(HttpStatusCode.OK, responseJson);

        var request = new VerificationRequest
        {
            Amount = 10000,
            Authority = "A00000000000000000000000000000000000"
        };

        var result = await _client.Verifications.VerifyAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(100);
        result.RefId.Should().Be(123456);

        _handler.Requests[0].RequestUri!.AbsolutePath.Should().Be(Endpoints.Verify);
    }

    [Fact]
    public async Task Inquiries_InquireAsync_SendsPostAndParsesResult()
    {
        var responseJson = @"{
            ""data"": {
                ""code"": 100,
                ""message"": ""Inquiry status"",
                ""authority"": ""A00000000000000000000000000000000000"",
                ""amount"": 10000,
                ""status"": ""PAID"",
                ""ref_id"": 123456
            }
        }";
        _handler.EnqueueResponse(HttpStatusCode.OK, responseJson);

        var request = new InquiryRequest
        {
            Authority = "A00000000000000000000000000000000000"
        };

        var result = await _client.Inquiries.InquireAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(100);
        result.Status.Should().Be("PAID");
        result.RefId.Should().Be(123456);

        _handler.Requests[0].RequestUri!.AbsolutePath.Should().Be(Endpoints.Inquiry);
    }

    [Fact]
    public async Task Reversals_ReverseAsync_SendsPostAndParsesResult()
    {
        var responseJson = @"{
            ""data"": {
                ""code"": 100,
                ""message"": ""Reversed successfully""
            }
        }";
        _handler.EnqueueResponse(HttpStatusCode.OK, responseJson);

        var request = new ReversalRequest
        {
            Authority = "A00000000000000000000000000000000000"
        };

        var result = await _client.Reversals.ReverseAsync(request);

        result.Should().NotBeNull();
        result.Code.Should().Be(100);
        result.Message.Should().Be("Reversed successfully");

        _handler.Requests[0].RequestUri!.AbsolutePath.Should().Be(Endpoints.Reverse);
    }

    [Fact]
    public async Task Unverified_ListAsync_SendsPostAndParsesResult()
    {
        var responseJson = @"{
            ""data"": {
                ""code"": 100,
                ""message"": ""Success"",
                ""authorities"": [
                    {
                        ""authority"": ""A00000000000000000000000000000000001"",
                        ""amount"": 20000,
                        ""callback_url"": ""https://example.com/cb""
                    }
                ]
            }
        }";
        _handler.EnqueueResponse(HttpStatusCode.OK, responseJson);

        var result = await _client.Unverified.ListAsync();

        result.Should().NotBeNull();
        result.Code.Should().Be(100);
        result.Authorities.Should().HaveCount(1);
        result.Authorities![0].Authority.Should().Be("A00000000000000000000000000000000001");

        _handler.Requests[0].RequestUri!.AbsolutePath.Should().Be(Endpoints.Unverified);
    }
}
