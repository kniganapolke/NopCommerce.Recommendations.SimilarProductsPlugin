using Nop.Data.Configuration;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;
using Nop.Plugin.Recommendations.SimilarProducts.Models;

namespace Nop.Plugin.Recommendations.SimilarProducts.Services
{
    public interface ISimilarProductsDiscoveryService
    {
        /// <summary>
        /// Get similar products for the specified product
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<SimilarProduct>> GetSimilarProductsAsync(int productId, int take);

        /// <summary>
        /// Prepare similar products data for all products and save it to database
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        Task TrainModelAndSaveSimilarProductsAsync(FeaturesConfigurationRecord settings, DataConfig appSettings);
    }
}