using System;
using FluentAssertions;
using Xunit;
using ZarinPal.Enums;
using ZarinPal.Exceptions;
using ZarinPal.Validators;

namespace ZarinPal.SDK.UnitTests;

public class ValidatorTests
{
    [Theory]
    [InlineData("c1234567-89ab-cdef-0123-456789abcdef")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("ABCDEF01-2345-6789-ABCD-EF0123456789")]
    public void ValidateMerchantId_ValidId_DoesNotThrow(string merchantId)
    {
        var act = () => Validator.ValidateMerchantId(merchantId);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-merchant-id")]
    [InlineData("c1234567-89ab-cdef-0123")]
    [InlineData("c1234567-89ab-cdef-0123-456789abcdef-extra")]
    public void ValidateMerchantId_InvalidId_ThrowsValidationException(string? merchantId)
    {
        var act = () => Validator.ValidateMerchantId(merchantId);
        act.Should().Throw<ValidationException>()
           .WithMessage("*Invalid merchant_id format*");
    }

    [Theory]
    [InlineData("A00000000000000000000000000000000000")]
    [InlineData("S1234567890abcdef1234567890abcdef123")]
    [InlineData("A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6Q7R8")]
    public void ValidateAuthority_ValidAuthority_DoesNotThrow(string authority)
    {
        var act = () => Validator.ValidateAuthority(authority);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("B00000000000000000000000000000000000")]
    [InlineData("A0000000000000000000000000000000000")] // 34 chars after prefix (too short)
    [InlineData("A000000000000000000000000000000000000")] // 36 chars after prefix (too long)
    [InlineData("A0000000000000000000000000000000000!")] // Special chars
    public void ValidateAuthority_InvalidAuthority_ThrowsValidationException(string? authority)
    {
        var act = () => Validator.ValidateAuthority(authority!);
        act.Should().Throw<ValidationException>()
           .WithMessage("*Invalid authority format*");
    }

    [Fact]
    [Obsolete]
    public void ValidateAmount_ValidAmount_DoesNotThrow()
    {
        var act = () => Validator.ValidateAmount(1000);
        act.Should().NotThrow();

        var act2 = () => Validator.ValidateAmount(50000, 500);
        act2.Should().NotThrow();
    }

    [Fact]
    public void ValidateAmount_BelowMinimum_ThrowsValidationException()
    {
        var act = () => Validator.ValidateAmount(999);
        act.Should().Throw<ValidationException>()
           .WithMessage("*Amount must be at least 1000*");

        var actCustom = () => Validator.ValidateAmount(100, 500);
        actCustom.Should().Throw<ValidationException>()
           .WithMessage("*Amount must be at least 500*");
    }

    [Theory]
    [InlineData("https://example.com/callback")]
    [InlineData("http://localhost:3000/api/callback")]
    [InlineData("https://subdomain.domain.co.ir/payment/verify?id=123")]
    public void ValidateCallbackUrl_ValidUrl_DoesNotThrow(string callbackUrl)
    {
        var act = () => Validator.ValidateCallbackUrl(callbackUrl);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("http://")]
    [InlineData("ftp://example.com")]
    [InlineData("just-a-string")]
    public void ValidateCallbackUrl_InvalidUrl_ThrowsValidationException(string? callbackUrl)
    {
        var act = () => Validator.ValidateCallbackUrl(callbackUrl!);
        act.Should().Throw<ValidationException>()
           .WithMessage("*Invalid callback URL format*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("09123456789")]
    [InlineData("09999999999")]
    [InlineData("09000000000")]
    public void ValidateMobile_ValidOrNullMobile_DoesNotThrow(string? mobile)
    {
        var act = () => Validator.ValidateMobile(mobile);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("08123456789")]
    [InlineData("0912345678")] // 10 digits
    [InlineData("091234567890")] // 12 digits
    [InlineData("0912abc5678")]
    public void ValidateMobile_InvalidMobile_ThrowsValidationException(string mobile)
    {
        var act = () => Validator.ValidateMobile(mobile);
        act.Should().Throw<ValidationException>()
           .WithMessage("*Invalid mobile number format*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("user@example.com")]
    [InlineData("name.surname@sub.domain.co.ir")]
    public void ValidateEmail_ValidOrNullEmail_DoesNotThrow(string? email)
    {
        var act = () => Validator.ValidateEmail(email);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@.com")]
    public void ValidateEmail_InvalidEmail_ThrowsValidationException(string email)
    {
        var act = () => Validator.ValidateEmail(email);
        act.Should().Throw<ValidationException>()
           .WithMessage("*Invalid email format*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("IRR")]
    [InlineData("IRT")]
    public void ValidateCurrency_ValidOrNullCurrency_DoesNotThrow(string? currency)
    {
        var act = () => Validator.ValidateCurrency(currency);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("irr")]
    public void ValidateCurrency_InvalidCurrency_ThrowsValidationException(string currency)
    {
        var act = () => Validator.ValidateCurrency(currency);
        act.Should().Throw<ValidationException>()
           .WithMessage("*Invalid currency format*");
    }

    [Fact]
    public void ValidateTerminalId_Valid_DoesNotThrow()
    {
        var act = () => Validator.ValidateTerminalId("term_123");
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ValidateTerminalId_NullOrEmpty_ThrowsValidationException(string? terminalId)
    {
        var act = () => Validator.ValidateTerminalId(terminalId);
        act.Should().Throw<ValidationException>()
           .WithMessage("*Terminal ID is required*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("PAID")]
    [InlineData("VERIFIED")]
    [InlineData("TRASH")]
    [InlineData("ACTIVE")]
    [InlineData("REFUNDED")]
    public void ValidateFilter_ValidOrNull_DoesNotThrow(string? filter)
    {
        var act = () => Validator.ValidateFilter(filter);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateFilter_Invalid_ThrowsValidationException()
    {
        var act = () => Validator.ValidateFilter("UNKNOWN_FILTER");
        act.Should().Throw<ValidationException>()
           .WithMessage("*Invalid filter value*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1)]
    [InlineData(100)]
    public void ValidateLimit_ValidOrNull_DoesNotThrow(int? limit)
    {
        var act = () => Validator.ValidateLimit(limit);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void ValidateLimit_NonPositive_ThrowsValidationException(int limit)
    {
        var act = () => Validator.ValidateLimit(limit);
        act.Should().Throw<ValidationException>()
           .WithMessage("*Limit must be a positive integer*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(50)]
    public void ValidateOffset_ValidOrNull_DoesNotThrow(int? offset)
    {
        var act = () => Validator.ValidateOffset(offset);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateOffset_Negative_ThrowsValidationException()
    {
        var act = () => Validator.ValidateOffset(-1);
        act.Should().Throw<ValidationException>()
           .WithMessage("*Offset must be a non-negative integer*");
    }

    [Fact]
    public void ValidateSessionId_Valid_DoesNotThrow()
    {
        var act = () => Validator.ValidateSessionId("sess_12345");
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ValidateSessionId_NullOrEmpty_ThrowsValidationException(string? sessionId)
    {
        var act = () => Validator.ValidateSessionId(sessionId);
        act.Should().Throw<ValidationException>()
           .WithMessage("*Session ID is required*");
    }

    [Fact]
    public void ValidateMethod_Valid_DoesNotThrow()
    {
        var act = () => Validator.ValidateMethod(RefundMethod.CARD);
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateMethod_Null_ThrowsValidationException()
    {
        var act = () => Validator.ValidateMethod(null);
        act.Should().Throw<ValidationException>()
           .WithMessage("*Method is required*");
    }

    [Theory]
    [InlineData("CUSTOMER_REQUEST")]
    [InlineData("DUPLICATE_TRANSACTION")]
    [InlineData("SUSPICIOUS_TRANSACTION")]
    [InlineData("OTHER")]
    public void ValidateReason_Valid_DoesNotThrow(string reason)
    {
        var act = () => Validator.ValidateReason(reason);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("INVALID_REASON")]
    public void ValidateReason_Invalid_ThrowsValidationException(string? reason)
    {
        var act = () => Validator.ValidateReason(reason!);
        act.Should().Throw<ValidationException>()
           .WithMessage("*Invalid reason*");
    }
}
