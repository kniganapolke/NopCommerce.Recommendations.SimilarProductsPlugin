using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;
using Nop.Plugin.Recommendations.SimilarProducts.Models;

namespace Nop.Plugin.Recommendations.SimilarProducts.Services
{
    public interface ISimilarProductsPersistanceService
    {
        Task SaveSimilarProducts(IEnumerable<SimilarProductRecord> records);

        Task<IEnumerable<SimilarProduct>> GetSimilarProductsAsync(int productId, int take);
    }
}