using System;
using Microsoft.Extensions.DependencyInjection;
using ZarinPal.Interfaces;

namespace ZarinPal.Extensions
{
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
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var config = new Config();
            configure(config);

            services.AddSingleton(config);

            // Facade (برای Controller)
            services.AddSingleton<IZarinPal, ZarinPal>();

            // Client داخلی (برای Resourceها)
            services.AddSingleton<IZarinPalClient>(
                sp => (ZarinPal)sp.GetRequiredService<IZarinPal>()
            );

            return services;
        }
    }
}
