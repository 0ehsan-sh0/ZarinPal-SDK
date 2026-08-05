using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZarinPal.Interfaces;

namespace ZarinPal.Extensions;

/// <summary>
/// Service collection extensions for registering ZarinPal SDK services.
/// </summary>
public static class ZarinPalServiceCollectionExtensions
{
    /// <summary>
    /// Registers ZarinPal services with the Dependency Injection container.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configure">A delegate to configure the ZarinPal options.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddZarinPal(
        this IServiceCollection services,
        Action<Config> configure)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        var config = new Config();
        configure(config);

        return services.AddZarinPal(config);
    }

    /// <summary>
    /// Registers ZarinPal services with the Dependency Injection container using a Config instance.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="config">The configured ZarinPal options instance.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddZarinPal(
        this IServiceCollection services,
        Config config)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (config == null) throw new ArgumentNullException(nameof(config));

        services.AddHttpClient("ZarinPalRest", client =>
        {
            var baseUrl = config.Sandbox ? "https://sandbox.zarinpal.com" : "https://payment.zarinpal.com";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = config.Timeout;
            client.DefaultRequestHeaders.UserAgent.ParseAdd(config.UserAgent);
        });

        services.AddHttpClient("ZarinPalGraphql", client =>
        {
            var graphqlBaseUrl = config.Sandbox ? "https://sandbox.zarinpal.com/api/v4/graphql/" : "https://next.zarinpal.com/api/v4/graphql/";
            client.BaseAddress = new Uri(graphqlBaseUrl);
            client.Timeout = config.Timeout;
            client.DefaultRequestHeaders.UserAgent.ParseAdd(config.UserAgent);
            if (!string.IsNullOrEmpty(config.AccessToken))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.AccessToken);
            }
        });

        services.AddSingleton(config);

        services.AddSingleton<ZarinPal>(sp =>
        {
            var cfg = sp.GetRequiredService<Config>();
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var logger = sp.GetService<ILogger<ZarinPal>>();
            return new ZarinPal(cfg, factory, logger);
        });

        services.AddSingleton<IZarinPal>(sp => sp.GetRequiredService<ZarinPal>());
        services.AddSingleton<IZarinPalClient>(sp => sp.GetRequiredService<ZarinPal>());

        return services;
    }
}
