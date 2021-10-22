namespace Nop.Plugin.Recommendations.SimilarProducts.Models
{
    public class ProductFeatureConfiguration
    {
        public ProductFeaturesEnum Feature { get; set; }
        public bool Enabled { get; set; }

        public ProductFeatureConfiguration()
        {                
        }

        public ProductFeatureConfiguration(ProductFeaturesEnum feature, bool enabled)
        {
            Feature = feature;
            Enabled = enabled;
        }
    }
}
