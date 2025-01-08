using Microsoft.Data.SqlClient;
using Microsoft.ML;
using Microsoft.ML.Data;
using Nop.Data.Configuration;
using Nop.Plugin.Recommendations.SimilarProducts.Models.ML;

namespace Nop.Plugin.Recommendations.SimilarProducts.Services
{
    public class ProductsLoaderService : IProductsLoaderService
    {
        public Task<IEnumerable<ProductInputModel>> LoadProducts(MLContext mlContext, DataConfig dataSettings)
        {
            return Task.Run(() =>
            {
                var connectionString = dataSettings.ConnectionString;
                var sqlStatement = "";

                using (var sr = new StreamReader("Plugins/Recommendations.SimilarProducts/SqlScripts/load-products.sql"))
                {
                    sqlStatement = sr.ReadToEnd();
                }

                var dbSource = new DatabaseSource(SqlClientFactory.Instance, connectionString, sqlStatement);
                DatabaseLoader loader = mlContext.Data.CreateDatabaseLoader<ProductInputModel>();
                IDataView data = loader.Load(dbSource);

                var input = mlContext.Data.CreateEnumerable<ProductInputModel>(data, false);

                return input;

            });
        }
    }
}
