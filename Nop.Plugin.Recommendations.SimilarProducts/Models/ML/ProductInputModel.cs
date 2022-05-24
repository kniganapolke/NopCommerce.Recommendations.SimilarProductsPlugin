namespace Nop.Plugin.Recommendations.SimilarProducts.Models.ML
{
    public class ProductInputModel
    {
        public int Id { get; set; }
        public float ProductTypeId { get; set; }
        public float ParentGroupedProductId { get; set; }
        public float VendorId { get; set; }
        public float Price { get; set; }
        public bool RequireOtherProducts { get; set; }
        public bool IsDownload { get; set; }
        public bool IsRecurring { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaTitle { get; set; }
        public string Categories { get; set; }
        public string Manufacturers { get; set; }
        public string ProductAttributes { get; set; }
        public string ProductTags { get; set; }
        public float Weight { get; set; }
        public float Height { get; set; }
        public float Length { get; set; }
        public float Width { get; set; }

    }
}
