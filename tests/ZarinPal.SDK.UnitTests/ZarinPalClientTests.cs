using System;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
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
}
