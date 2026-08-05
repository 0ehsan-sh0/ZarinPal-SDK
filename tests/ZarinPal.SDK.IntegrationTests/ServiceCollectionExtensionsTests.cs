using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ZarinPal.Extensions;
using ZarinPal.Interfaces;

namespace ZarinPal.SDK.IntegrationTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddZarinPal_WithConfig_RegistersServicesCorrectly()
    {
        var services = new ServiceCollection();
        var config = new Config
        {
            MerchantId = "c1234567-89ab-cdef-0123-456789abcdef",
            Sandbox = true,
            AccessToken = "test_token"
        };

        services.AddZarinPal(config);

        var provider = services.BuildServiceProvider();

        var zarinpalInstance = provider.GetService<ZarinPal>();
        zarinpalInstance.Should().NotBeNull();

        var iZarinPal = provider.GetService<IZarinPal>();
        iZarinPal.Should().BeSameAs(zarinpalInstance);

        var iZarinPalClient = provider.GetService<IZarinPalClient>();
        iZarinPalClient.Should().BeSameAs(zarinpalInstance);
    }

    [Fact]
    public void AddZarinPal_WithAction_ConfiguresAndRegistersServices()
    {
        var services = new ServiceCollection();

        services.AddZarinPal(options =>
        {
            options.MerchantId = "c1234567-89ab-cdef-0123-456789abcdef";
            options.Sandbox = false;
        });

        var provider = services.BuildServiceProvider();

        var zarinpalInstance = provider.GetService<IZarinPal>();
        zarinpalInstance.Should().NotBeNull();
    }
}
