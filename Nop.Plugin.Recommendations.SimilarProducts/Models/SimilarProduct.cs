using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Recommendations.SimilarProducts.Models
{
    public class SimilarProduct
    {
        public int ProductId { get; set; }

        public double Similarity { get; set; }
    }
}
