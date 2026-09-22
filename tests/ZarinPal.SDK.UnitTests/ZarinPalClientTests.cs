using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using ZarinPal.Exceptions;
using ZarinPal.Models;

namespace ZarinPal.SDK.UnitTests;

public class ZarinPalClientTests
{
    [Fact]
    public void Constructor_NullConfig_ThrowsArgumentNullException()
    {
        var act = () => new ZarinPal(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void Constructor_ProductionMode_SetsProductionBaseUrl()
    {
        var config = new Config
        {
            MerchantId = "c1234567-89ab-cdef-0123-456789abcdef",
            Sandbox = false
        };

        using var zarinpal = new ZarinPal(config);

        zarinpal.GetBaseUrl().Should().Be("https://payment.zarinpal.com");
        zarinpal.Payments.Should().NotBeNull();
        zarinpal.Refunds.Should().NotBeNull();
        zarinpal.Transactions.Should().NotBeNull();
        zarinpal.Verifications.Should().NotBeNull();
        zarinpal.Reversals.Should().NotBeNull();
        zarinpal.Unverified.Should().NotBeNull();
        zarinpal.Inquiries.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_SandboxMode_SetsSandboxBaseUrl()
    {
        var config = new Config
        {
            MerchantId = "c1234567-89ab-cdef-0123-456789abcdef",
            Sandbox = true
        };

        using var zarinpal = new ZarinPal(config);

        zarinpal.GetBaseUrl().Should().Be("https://sandbox.zarinpal.com");
    }

    [Fact]
    public void Constructor_InjectedHttpClients_DoesNotDisposeClientsOnDispose()
    {
        var config = new Config { MerchantId = "c1234567-89ab-cdef-0123-456789abcdef" };
        var restClient = new HttpClient { BaseAddress = new Uri("https://payment.zarinpal.com") };
        var graphqlClient = new HttpClient { BaseAddress = new Uri("https://next.zarinpal.com") };

        var zarinpal = new ZarinPal(config, restClient, graphqlClient, NullLogger<ZarinPal>.Instance);
        zarinpal.Dispose();

        // Disposing zarinpal should not dispose externally owned clients
        var actRest = () => restClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://payment.zarinpal.com"));
        actRest.Should().NotThrowAsync<ObjectDisposedException>();

        restClient.Dispose();
        graphqlClient.Dispose();
    }

    [Fact]
    public void GetRedirectUrl_AppendsAuthorityToStartPayEndpoint()
    {
        var config = new Config
        {
            MerchantId = "c1234567-89ab-cdef-0123-456789abcdef",
            Sandbox = true
        };

        using var zarinpal = new ZarinPal(config);
        var authority = "A00000000000000000000000000000000000";

        var url = zarinpal.GetRedirectUrl(authority);

        url.Should().Be($"https://sandbox.zarinpal.com/pg/StartPay/{authority}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid-authority")]
    public void GetRedirectUrl_InvalidAuthority_ThrowsValidationException(string? invalidAuthority)
    {
        var config = new Config { MerchantId = "c1234567-89ab-cdef-0123-456789abcdef", Sandbox = true };
        using var zarinpal = new ZarinPal(config);

        var act = () => zarinpal.GetRedirectUrl(invalidAuthority!);
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public async Task ResourceMethods_NullArguments_ThrowArgumentNullException()
    {
        var config = new Config { MerchantId = "c1234567-89ab-cdef-0123-456789abcdef", Sandbox = true };
        using var zarinpal = new ZarinPal(config);

        await Assert.ThrowsAsync<ArgumentNullException>(() => zarinpal.CreateAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => zarinpal.CalculateFeeAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => zarinpal.VerifyAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => zarinpal.InquireAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => zarinpal.ReverseAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => zarinpal.ListTransactionsAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => zarinpal.CreateRefundAsync(null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() => zarinpal.ListRefundsAsync(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RetrieveRefundAsync_NullOrEmptyId_ThrowsValidationException(string? refundId)
    {
        var config = new Config { MerchantId = "c1234567-89ab-cdef-0123-456789abcdef", Sandbox = true };
        using var zarinpal = new ZarinPal(config);

        var act = () => zarinpal.RetrieveRefundAsync(refundId!);
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Refund ID is required*");
    }

    [Fact]
    public async Task CreateRefundAsync_NullMethod_ThrowsValidationException()
    {
        var config = new Config { MerchantId = "c1234567-89ab-cdef-0123-456789abcdef", Sandbox = true };
        using var zarinpal = new ZarinPal(config);

        var request = new RefundCreateRequest
        {
            SessionId = "sess_001",
            Amount = 5000,
            Method = null
        };

        var act = () => zarinpal.CreateRefundAsync(request);
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Method is required*");
    }
}
