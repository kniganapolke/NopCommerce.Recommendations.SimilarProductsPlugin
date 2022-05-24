using System;
using System.ComponentModel;

namespace Nop.Plugin.Recommendations.SimilarProducts.Models
{
    [Flags]
    public enum ProductFeaturesEnum
    {
        [Description("admin.catalog.products.fields.producttype")]
        ProductTypeId = 1,

        [Description("admin.catalog.products.fields.associatedtoproductname")]
        ParentGroupedProductId = 2,

        [Description("admin.catalog.products.fields.vendor")]
        VendorId = 4,

        [Description("admin.catalog.products.fields.price")]
        Price = 8,

        [Description("admin.catalog.products.fields.requireotherproducts")]
        RequireOtherProducts = 16,

        [Description("admin.catalog.products.fields.isdownload")]
        IsDownload = 32,

        [Description("admin.catalog.products.fields.isrecurring")]
        IsRecurring = 64,

        [Description("admin.catalog.products.fields.name")]
        Name = 128,

        [Description("admin.catalog.products.fields.shortdescription")]
        ShortDescription = 256,

        [Description("admin.catalog.products.fields.fulldescription")]
        FullDescription = 512,

        [Description("admin.catalog.products.fields.metakeywords")]
        MetaKeywords = 1024,

        [Description("admin.catalog.products.fields.metatitle")]
        MetaTitle = 2048,

        [Description("admin.catalog.products.fields.categories")]
        Categories = 4096,

        [Description("admin.catalog.products.fields.manufacturers")]
        Manufacturers = 8192,

        [Description("admin.catalog.products.productattributes")]
        ProductAttributes = 16384,

        [Description("admin.catalog.products.fields.weight")]
        Weight = 32768,

        [Description("admin.catalog.products.fields.height")]
        Height = 65536,

        [Description("admin.catalog.products.fields.length")]
        Length = 131072,

        [Description("admin.catalog.products.fields.width")]
        Width = 262144,

        [Description("admin.catalog.products.fields.producttags")]
        ProductTags = 524288,
    }
}
