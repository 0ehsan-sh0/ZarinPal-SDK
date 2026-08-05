using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using ZarinPal.Enums;
using ZarinPal.Models;

namespace ZarinPal.SDK.UnitTests;

public class ModelSerializationTests
{
    [Fact]
    public void PaymentRequest_Serialization_IncludesExpectedProperties()
    {
        var request = new PaymentRequest
        {
            Amount = 50000,
            CallbackUrl = "https://example.com/callback",
            Description = "Test Order",
            Mobile = "09123456789",
            Email = "test@example.com"
        };

        var json = JsonSerializer.Serialize(request);

        json.Should().Contain("\"amount\":50000");
        json.Should().Contain("\"callback_url\":\"https://example.com/callback\"");
        json.Should().Contain("\"description\":\"Test Order\"");
        json.Should().Contain("\"mobile\":\"09123456789\"");
        json.Should().Contain("\"email\":\"test@example.com\"");
    }

    [Fact]
    public void PaymentResult_Deserialization_MapsFieldsCorrectly()
    {
        var json = @"{
            ""code"": 100,
            ""message"": ""Operation successful"",
            ""authority"": ""A00000000000000000000000000000000000"",
            ""fee_type"": ""Merchant"",
            ""fee"": 500,
            ""wages"": [
                { ""iban"": ""IR111111111111111111111111"", ""amount"": 1000, ""description"": ""Fee"" }
            ]
        }";

        var result = JsonSerializer.Deserialize<PaymentResult>(json);

        result.Should().NotBeNull();
        result!.Code.Should().Be(100);
        result.Message.Should().Be("Operation successful");
        result.Authority.Should().Be("A00000000000000000000000000000000000");
        result.FeeType.Should().Be("Merchant");
        result.Fee.Should().Be(500);
        result.Wages.Should().HaveCount(1);
        result.Wages![0].Iban.Should().Be("IR111111111111111111111111");
    }

    [Fact]
    public void VerifyResult_Deserialization_MapsFieldsCorrectly()
    {
        var json = @"{
            ""code"": 100,
            ""message"": ""Verified"",
            ""ref_id"": 123456789,
            ""card_pan"": ""502229******1234"",
            ""card_hash"": ""hash123"",
            ""fee_type"": ""Merchant"",
            ""fee"": 200
        }";

        var result = JsonSerializer.Deserialize<VerifyResult>(json);

        result.Should().NotBeNull();
        result!.Code.Should().Be(100);
        result.Message.Should().Be("Verified");
        result.RefId.Should().Be(123456789);
        result.CardPan.Should().Be("502229******1234");
        result.CardHash.Should().Be("hash123");
        result.FeeType.Should().Be("Merchant");
        result.Fee.Should().Be(200);
    }

    [Fact]
    public void FeeCalculationResult_Deserialization_MapsFieldsCorrectly()
    {
        var json = @"{
            ""fee"": 1500,
            ""fee_type"": ""Customer""
        }";

        var result = JsonSerializer.Deserialize<FeeCalculationResult>(json);

        result.Should().NotBeNull();
        result!.Fee.Should().Be(1500);
        result.FeeType.Should().Be("Customer");
    }

    [Fact]
    public void InquiryResult_Deserialization_MapsFieldsCorrectly()
    {
        var json = @"{
            ""code"": 100,
            ""message"": ""Inquiry successful"",
            ""authority"": ""A00000000000000000000000000000000000"",
            ""amount"": 10000,
            ""status"": ""PAID"",
            ""ref_id"": 987654321,
            ""card_pan"": ""603799******4321""
        }";

        var result = JsonSerializer.Deserialize<InquiryResult>(json);

        result.Should().NotBeNull();
        result!.Code.Should().Be(100);
        result.Amount.Should().Be(10000);
        result.Status.Should().Be("PAID");
        result.RefId.Should().Be(987654321);
        result.CardPan.Should().Be("603799******4321");
    }

    [Fact]
    public void UnverifiedResult_Deserialization_MapsFieldsCorrectly()
    {
        var json = @"{
            ""code"": 100,
            ""message"": ""Success"",
            ""authorities"": [
                {
                    ""authority"": ""A00000000000000000000000000000000001"",
                    ""amount"": 25000,
                    ""callback_url"": ""https://example.com/cb"",
                    ""date"": ""2026-08-05""
                }
            ]
        }";

        var result = JsonSerializer.Deserialize<UnverifiedResult>(json);

        result.Should().NotBeNull();
        result!.Code.Should().Be(100);
        result.Authorities.Should().HaveCount(1);
        result.Authorities![0].Authority.Should().Be("A00000000000000000000000000000000001");
        result.Authorities[0].Amount.Should().Be(25000);
    }

    [Fact]
    public void RefundCreateResult_Deserialization_MapsFieldsCorrectly()
    {
        var json = @"{
            ""id"": ""ref_001"",
            ""terminal_id"": ""term_100"",
            ""amount"": 5000,
            ""timeline"": [
                {
                    ""refund_amount"": 5000,
                    ""refund_time"": ""2026-08-05T12:00:00Z"",
                    ""refund_status"": ""COMPLETED""
                }
            ]
        }";

        var result = JsonSerializer.Deserialize<RefundCreateResult>(json);

        result.Should().NotBeNull();
        result!.Id.Should().Be("ref_001");
        result.TerminalId.Should().Be("term_100");
        result.Amount.Should().Be(5000);
        result.Timeline.Should().HaveCount(1);
        result.Timeline![0].RefundStatus.Should().Be("COMPLETED");
    }

    [Fact]
    public void TransactionItem_Deserialization_MapsFieldsCorrectly()
    {
        var json = @"{
            ""id"": ""tx_999"",
            ""status"": ""VERIFIED"",
            ""amount"": 75000,
            ""description"": ""Order payment"",
            ""created_at"": ""2026-08-05T10:00:00Z""
        }";

        var result = JsonSerializer.Deserialize<TransactionItem>(json);

        result.Should().NotBeNull();
        result!.Id.Should().Be("tx_999");
        result.Status.Should().Be("VERIFIED");
        result.Amount.Should().Be(75000);
        result.Description.Should().Be("Order payment");
        result.CreatedAt.Should().Be("2026-08-05T10:00:00Z");
    }
}
