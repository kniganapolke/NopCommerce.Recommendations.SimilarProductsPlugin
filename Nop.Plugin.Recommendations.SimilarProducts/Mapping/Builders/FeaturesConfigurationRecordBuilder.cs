using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;

namespace Nop.Plugin.Recommendations.SimilarProducts.Mapping.Builders
{
    public class FeaturesConfigurationRecordBuilder : NopEntityBuilder<FeaturesConfigurationRecord>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(FeaturesConfigurationRecord.Id)).AsInt32().Identity().PrimaryKey()
                .WithColumn(nameof(FeaturesConfigurationRecord.ProductFeaturesEnabled)).AsInt64()
                .WithColumn(nameof(FeaturesConfigurationRecord.NumOfSimilarProductsToDiscover)).AsInt32()
                .WithColumn(nameof(FeaturesConfigurationRecord.NumOfSimilarProductsToDisplay)).AsInt32()
                .WithColumn(nameof(FeaturesConfigurationRecord.MinAcceptedValueOfSimilarity)).AsDouble()
                .WithColumn(nameof(FeaturesConfigurationRecord.CreatedOnUtc)).AsDateTime()
                .WithColumn(nameof(FeaturesConfigurationRecord.UpdatedOnUtc)).AsDateTime().Nullable();
        }

        #endregion
    }
}