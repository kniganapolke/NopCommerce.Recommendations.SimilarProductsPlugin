using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Recommendations.SimilarProducts.Services;

namespace Nop.Plugin.Recommendations.SimilarProducts.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="appSettings">App settings</param>
        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<IFeaturesConfigurationService, FeaturesConfigurationService>();
            services.AddScoped<ISimilarProductsService, SimilarProductsDiscoveryService>();
            services.AddScoped<IProductsLoaderService, ProductsLoaderService>();
            services.AddScoped<ISimilarProductsPersistanceService, SimilarProductsPersistanceService>();
        }

        public int Order => 1;
    }
}
