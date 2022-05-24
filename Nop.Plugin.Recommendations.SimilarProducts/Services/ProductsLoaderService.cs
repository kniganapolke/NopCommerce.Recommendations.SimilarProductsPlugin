using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;
using Microsoft.ML;
using Nop.Data;
using Nop.Plugin.Recommendations.SimilarProducts.Models.ML;
using System.Data.SqlClient;

namespace Nop.Plugin.Recommendations.SimilarProducts.Services
{
    public class ProductsLoaderService : IProductsLoaderService
    {
        public Task<IEnumerable<ProductInputModel>> LoadProducts(MLContext mlContext, DataSettings dataSettings)
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
