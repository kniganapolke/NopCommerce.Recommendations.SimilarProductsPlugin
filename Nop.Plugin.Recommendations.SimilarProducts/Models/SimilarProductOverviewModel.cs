using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Recommendations.SimilarProducts.Models
{
    public class SimilarProductOverviewModel
    {
        public ProductOverviewModel Product { get; set; }

        public double Similarity { get; set; }
    }
}
