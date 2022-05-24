using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ML;
using Nop.Data;
using Nop.Plugin.Recommendations.SimilarProducts.Models.ML;

namespace Nop.Plugin.Recommendations.SimilarProducts.Services
{
    public interface IProductsLoaderService
    {
        Task<IEnumerable<ProductInputModel>> LoadProducts(MLContext mlContext, DataSettings dataSettings);
    }
}
