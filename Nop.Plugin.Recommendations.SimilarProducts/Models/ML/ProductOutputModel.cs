namespace Nop.Plugin.Recommendations.SimilarProducts.Models.ML
{
    public class ProductOutputModel
    {
        public int Id { get; set; }
        public float[] MiscFeatures { get; set; }
        public float[] NameFeaturized { get; set; }
        public float[] ShortDescriptionFeaturized { get; set; }
        public float[] FullDescriptionFeaturized { get; set; }
        public float[] MetaKeywordsFeaturized { get; set; }
        public float[] MetaTitleFeaturized { get; set; }
        public float[] Categories { get; set; }
        public float[] Manufacturers { get; set; }
        public float[] ProductAttributes { get; set; }
        public float[] PhysicalCaracteristics { get; set; }
    }
}
