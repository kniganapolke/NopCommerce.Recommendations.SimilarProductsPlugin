using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;

namespace Nop.Plugin.Recommendations.SimilarProducts.Mapping.Builders
{
    public class SimilarProductRecordBuilder : NopEntityBuilder<SimilarProductRecord>
    {
        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SimilarProductRecord.Id)).AsInt32().Identity().PrimaryKey()
                .WithColumn(nameof(SimilarProductRecord.ProductId)).AsInt32().ForeignKey(nameof(Product), nameof(Product.Id)).Indexed()
                .WithColumn(nameof(SimilarProductRecord.SimilarProductId)).AsInt32().ForeignKey(nameof(Product), nameof(Product.Id))
                .WithColumn(nameof(SimilarProductRecord.Similarity)).AsDouble()
                .WithColumn(nameof(FeaturesConfigurationRecord.CreatedOnUtc)).AsDateTime();
        }
    }
}
