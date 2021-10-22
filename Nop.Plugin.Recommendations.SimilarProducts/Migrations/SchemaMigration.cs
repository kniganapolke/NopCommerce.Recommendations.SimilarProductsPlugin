using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;

namespace Nop.Plugin.Recommendations.SimilarProducts.Migrations
{
    [SkipMigrationOnUpdate]
    [NopMigration("2021/10/10 08:41:55:1687541", "Nop.Plugin.Recommendations.SimilarProducts schema NEW")]
    public class SchemaMigration : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            _migrationManager.BuildTable<FeaturesConfigurationRecord>(Create);
            _migrationManager.BuildTable<SimilarProductRecord>(Create);
        }
    }
}
