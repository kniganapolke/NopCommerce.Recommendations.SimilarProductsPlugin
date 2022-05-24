namespace Nop.Plugin.Recommendations.SimilarProducts.Models.ML
{
    public class ProductTextOutputModel
    {
        public float[] NameFeaturized { get; set; }
        public float[] ShortDescriptionFeaturized { get; set; }
        public float[] FullDescriptionFeaturized { get; set; }
        public float[] MetaKeywordsFeaturized { get; set; }
        public float[] MetaTitleFeaturized { get; set; }
    }
}
