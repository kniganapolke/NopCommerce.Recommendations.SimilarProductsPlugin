using System.Threading.Tasks;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;

namespace Nop.Plugin.Recommendations.SimilarProducts.Services
{
    public interface IFeaturesConfigurationService
    {
        /// <summary>
        /// Save configuration
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        Task AddOrUpdateAsync(FeaturesConfigurationRecord record);

        /// <summary>
        /// Get configuration
        /// </summary>
        /// <returns></returns>
        Task<FeaturesConfigurationRecord> GetAsync();

        /// <summary>
        /// Save default configuration
        /// </summary>
        /// <returns></returns>
        Task SaveDefaultConfigurationAsync();
    }
}
