using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using ZarinPal.Enums;
using ZarinPal.Models;
using ZarinPal.SDK.IntegrationTests.Helpers;

namespace ZarinPal.SDK.E2ETests;

public class MockedSandboxE2ETests
{
    [Fact]
    public async Task Scenario1_CompletePaymentAndVerificationCycle()
    {
        var handler = new MockHttpMessageHandler();

        // 1. Payment Request Response
        handler.EnqueueResponse(HttpStatusCode.OK, @"{
            ""data"": {
                ""code"": 100,
                ""message"": ""Success"",
                ""authority"": ""A00000000000000000000000000000000000""
            }
        }");

        // 2. Verification Response
        handler.EnqueueResponse(HttpStatusCode.OK, @"{
            ""data"": {
                ""code"": 100,
                ""message"": ""Verified"",
                ""ref_id"": 99887766,
                ""card_pan"": ""603799******1111"",
                ""card_hash"": ""hash123""
            }
        }");

        var restClient = new HttpClient(handler) { BaseAddress = new Uri("https://sandbox.zarinpal.com") };
        var config = new Config
        {
            MerchantId = "c1234567-89ab-cdef-0123-456789abcdef",
            Sandbox = true
        };
        using var client = new ZarinPal(config, restClient, restClient);

        // Step 1: Create Payment
        var paymentRequest = new PaymentRequest
        {
            Amount = 15000,
            CallbackUrl = "https://myshop.com/payment/callback",
            Description = "Order #2001",
            Mobile = "09123456789"
        };
        var paymentResult = await client.Payments.CreateAsync(paymentRequest);

        paymentResult.Code.Should().Be(100);
        paymentResult.Authority.Should().Be("A00000000000000000000000000000000000");

        // Step 2: Generate StartPay Redirect URL
        var redirectUrl = client.GetRedirectUrl(paymentResult.Authority);
        redirectUrl.Should().Be("https://sandbox.zarinpal.com/pg/StartPay/A00000000000000000000000000000000000");

        // Step 3: Verify Payment after user returns
        var verifyRequest = new VerificationRequest
        {
            Amount = 15000,
            Authority = paymentResult.Authority
        };
        var verifyResult = await client.VerifyAsync(verifyRequest);

        verifyResult.Code.Should().Be(100);
        verifyResult.RefId.Should().Be(99887766);
        verifyResult.CardPan.Should().Be("603799******1111");
    }

    [Fact]
    public async Task Scenario2_UnverifiedInspectionInquiryAndReversalCycle()
    {
        var handler = new MockHttpMessageHandler();

        // 1. Unverified list response
        handler.EnqueueResponse(HttpStatusCode.OK, @"{
            ""data"": {
                ""code"": 100,
                ""message"": ""Success"",
                ""authorities"": [
                    {
                        ""authority"": ""A00000000000000000000000000000000005"",
                        ""amount"": 50000,
                        ""callback_url"": ""https://myshop.com/cb""
                    }
                ]
            }
        }");

        // 2. Inquiry response
        handler.EnqueueResponse(HttpStatusCode.OK, @"{
            ""data"": {
                ""code"": 100,
                ""message"": ""Inquiry status"",
                ""authority"": ""A00000000000000000000000000000000005"",
                ""amount"": 50000,
                ""status"": ""PENDING""
            }
        }");

        // 3. Reversal response
        handler.EnqueueResponse(HttpStatusCode.OK, @"{
            ""data"": {
                ""code"": 100,
                ""message"": ""Reversed""
            }
        }");

        var restClient = new HttpClient(handler) { BaseAddress = new Uri("https://sandbox.zarinpal.com") };
        var config = new Config
        {
            MerchantId = "c1234567-89ab-cdef-0123-456789abcdef",
            Sandbox = true
        };
        using var client = new ZarinPal(config, restClient, restClient);

        // Step 1: Fetch Unverified Payments
        var unverifiedResult = await client.ListUnverifiedAsync();
        unverifiedResult.Authorities.Should().HaveCount(1);
        var unverifiedAuth = unverifiedResult.Authorities![0].Authority;

        // Step 2: Inquire status
        var inquiryResult = await client.InquireAsync(new InquiryRequest { Authority = unverifiedAuth });
        inquiryResult.Status.Should().Be("PENDING");

        // Step 3: Reverse transaction
        var reversalResult = await client.ReverseAsync(new ReversalRequest { Authority = unverifiedAuth });
        reversalResult.Code.Should().Be(100);
        reversalResult.Message.Should().Be("Reversed");
    }

    [Fact]
    public async Task Scenario3_FeeCalculationAndGraphqlRefundCycle()
    {
        var handler = new MockHttpMessageHandler();

        // 1. Fee calculation response
        handler.EnqueueResponse(HttpStatusCode.OK, @"{
            ""data"": {
                ""fee"": 500,
                ""fee_type"": ""Merchant""
            }
        }");

        // 2. Refund creation response
        handler.EnqueueResponse(HttpStatusCode.OK, @"{
            ""data"": {
                ""resource"": {
                    ""id"": ""ref_777"",
                    ""terminal_id"": ""term_10"",
                    ""amount"": 10000,
                    ""timeline"": [
                        {
                            ""refund_amount"": 10000,
                            ""refund_time"": ""2026-08-05T15:00:00Z"",
                            ""refund_status"": ""PROCESSING""
                        }
                    ]
                }
            }
        }");

        // 3. Refund retrieval response
        handler.EnqueueResponse(HttpStatusCode.OK, @"{
            ""data"": {
                ""refund"": {
                    ""id"": ""ref_777"",
                    ""amount"": 10000,
                    ""status"": ""COMPLETED"",
                    ""created_at"": ""2026-08-05T15:00:00Z"",
                    ""description"": ""Refund test""
                }
            }
        }");

        var restClient = new HttpClient(handler) { BaseAddress = new Uri("https://sandbox.zarinpal.com") };
        var config = new Config
        {
            MerchantId = "c1234567-89ab-cdef-0123-456789abcdef",
            Sandbox = true,
            AccessToken = "valid_token"
        };
        using var client = new ZarinPal(config, restClient, restClient);

        // Step 1: Calculate Fee
        var fee = await client.CalculateFeeAsync(new FeeCalculationRequest { Amount = 10000, Currency = "IRT" });
        fee.Fee.Should().Be(500);

        // Step 2: Request Refund via GraphQL
        var refundCreate = await client.CreateRefundAsync(new RefundCreateRequest
        {
            SessionId = "sess_999",
            Amount = 10000,
            Method = RefundMethod.CARD,
            Reason = "CUSTOMER_REQUEST"
        });
        refundCreate.Id.Should().Be("ref_777");

        // Step 3: Retrieve Refund Details
        var refundItem = await client.RetrieveRefundAsync(refundCreate.Id);
        refundItem.Id.Should().Be("ref_777");
        refundItem.Status.Should().Be("COMPLETED");
    }
}
