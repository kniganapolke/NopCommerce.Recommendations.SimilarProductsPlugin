using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;

namespace Nop.Plugin.Recommendations.SimilarProducts.Migrations
{
    [NopMigration("2021/10/10 08:41:55:1687541", "Nop.Plugin.Recommendations.SimilarProducts schema NEW", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            Create.TableFor<FeaturesConfigurationRecord>();
            Create.TableFor<SimilarProductRecord>();
        }
    }
}
