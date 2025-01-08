using Microsoft.ML;
using Nop.Data.Configuration;
using Nop.Plugin.Recommendations.SimilarProducts.Models.ML;

namespace Nop.Plugin.Recommendations.SimilarProducts.Services
{
    public interface IProductsLoaderService
    {
        Task<IEnumerable<ProductInputModel>> LoadProducts(MLContext mlContext, DataConfig dataSettings);
    }
}
