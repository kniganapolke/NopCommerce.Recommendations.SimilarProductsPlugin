using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Text;
using Nop.Data;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;
using Nop.Plugin.Recommendations.SimilarProducts.Models;
using Nop.Plugin.Recommendations.SimilarProducts.Models.ML;

namespace Nop.Plugin.Recommendations.SimilarProducts.Services
{
    public class SimilarProductsService : ISimilarProductsService
    {
        #region Fields

        private readonly IRepository<SimilarProductRecord> _similarProductRecordRepository;

        #endregion Fields

        #region Ctor

        public SimilarProductsService(IRepository<SimilarProductRecord> similarroductRecordRepository)
        {
            _similarProductRecordRepository = similarroductRecordRepository;
        }

        #endregion Ctor

        #region Public Methods

        public async Task TrainModelAndSaveSimilarProductsAsync(FeaturesConfigurationRecord settings)
        {
            if(settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var mlContext = new MLContext();

            var predictionEngine = await TrainModel(mlContext, settings);

            var data = await LoadProducts(mlContext);

            var input = mlContext.Data.CreateEnumerable<ProductInputModel>(data, false);
            var products = input.ToDictionary(p => p.Id, p => p);
            var transformedProducts = input.ToDictionary(p => p.Id, p => predictionEngine.Predict(p));

            var records = new List<SimilarProductRecord>();

            foreach(var productId in products.Keys)
            {
                var product = transformedProducts.Single(p => p.Key == productId).Value;
                var similarities = transformedProducts
                    .Where(p => p.Key != productId) // Do not compare with product itself
                    .ToDictionary(p => p.Key, p => Similarity(product, p.Value, settings))
                    .Where(p => p.Value >= settings.MinAcceptedValueOfSimilarity)
                    .OrderByDescending(pair => pair.Value)
                    .Take(settings.NumOfSimilarProductsToDiscover).Select(pair => 
                    new SimilarProductRecord() 
                    { 
                        ProductId = productId, 
                        SimilarProductId = pair.Key, 
                        Similarity = pair.Value
                    });
                
                records.AddRange(similarities);
            }

            await SaveSimilarProducts(records);
        }

        public async Task<IEnumerable<SimilarProduct>> GetSimilarProductsAsync(int productId, int take)
        {
            var similarProducts = await _similarProductRecordRepository.GetAllAsync(
                    query => query.Where(r => r.ProductId == productId)
                    .OrderByDescending(r => r.Similarity)
                    .Take(take));

            var result = similarProducts.Select(p => 
                new SimilarProduct() 
                { 
                    ProductId = p.SimilarProductId,
                    Similarity = p.Similarity
                });

            return result;
        }

        #endregion Public Methods

        #region Private Methods

        private double Similarity(ProductOutputModel p1, ProductOutputModel p2, FeaturesConfigurationRecord settings)
        {
            var weightedDistances = new WeightedSimilaritiesCollection();

            if(IsFeatureEnabled(settings, ProductFeaturesEnum.Name))
            {
                weightedDistances.AddCosineSimilarity(p1.NameFeaturized, p2.NameFeaturized, weight: 1);
            }

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.ShortDescription))
            {
                weightedDistances.AddCosineSimilarity(p1.ShortDescriptionFeaturized, p2.ShortDescriptionFeaturized, weight: 1);
            }

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.FullDescription))
            {
                weightedDistances.AddCosineSimilarity(p1.FullDescriptionFeaturized, p2.FullDescriptionFeaturized, weight: 1);
            }

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.MetaTitle))
            {
                weightedDistances.AddCosineSimilarity(p1.MetaTitleFeaturized, p2.MetaTitleFeaturized, weight: 1);
            }

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.MetaKeywords))
            {
                weightedDistances.AddCosineSimilarity(p1.MetaKeywordsFeaturized, p2.MetaKeywordsFeaturized, weight: 1);
            }

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.Categories))
            {
                weightedDistances.AddPearsonSimilarity(p1.Categories, p2.Categories, weight: 1);
            }

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.Manufacturers))
            {
                weightedDistances.AddPearsonSimilarity(p1.Manufacturers, p2.Manufacturers, weight: 1);
            }

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.ProductAttributes))
            {
                weightedDistances.AddPearsonSimilarity(p1.ProductAttributes, p2.ProductAttributes, weight: 1);
            }

            weightedDistances.AddPearsonSimilarity(p1.MiscFeatures, p2.MiscFeatures, weight: 1);
                        
            weightedDistances.AddPearsonSimilarity(p1.PhysicalCaracteristics, p2.PhysicalCaracteristics, weight: 1);

            var weightedDistance = weightedDistances.GetWeightedDistance();

            return weightedDistance;
        }

        private async Task<PredictionEngine<ProductInputModel, ProductOutputModel>> TrainModel(MLContext mlContext, FeaturesConfigurationRecord settings)
        {
            var task = await Task.Run(() => {

                var options = new TextFeaturizingEstimator.Options()
                {
                    CaseMode = TextNormalizingEstimator.CaseMode.Lower,
                    KeepDiacritics = false,
                    KeepPunctuations = false
                };

                Func<string, float[]> parseStringIntoArray = (input) =>
                    input?.Split(',').Select(v =>
                    {
                        _ = float.TryParse(v, out var r);
                        return r;
                    }).ToArray();

                Action<ProductInputModel, ProductOutputModel> mapping = (input, output) =>
                {
                    output.Id = input.Id;

                    var miscFeatures = new List<float>();
                    var physicalCaracteristics = new List<float>();

                    if (IsFeatureEnabled(settings, ProductFeaturesEnum.ProductTypeId))
                        miscFeatures.Add(input.ProductTypeId);

                    if (IsFeatureEnabled(settings, ProductFeaturesEnum.ParentGroupedProductId))
                        miscFeatures.Add(input.ParentGroupedProductId);

                    if (IsFeatureEnabled(settings, ProductFeaturesEnum.VendorId))
                        miscFeatures.Add(input.VendorId);

                    if (IsFeatureEnabled(settings, ProductFeaturesEnum.Price))
                        miscFeatures.Add(input.Price);

                    if (IsFeatureEnabled(settings, ProductFeaturesEnum.RequireOtherProducts))
                        miscFeatures.Add(Convert.ToSingle(input.RequireOtherProducts));

                    if (IsFeatureEnabled(settings, ProductFeaturesEnum.IsDownload))
                        miscFeatures.Add(Convert.ToSingle(input.IsDownload));

                    if (IsFeatureEnabled(settings, ProductFeaturesEnum.IsRecurring))
                        miscFeatures.Add(Convert.ToSingle(input.IsRecurring));

                    if (IsFeatureEnabled(settings, ProductFeaturesEnum.Weight))
                        physicalCaracteristics.Add(input.Weight);

                    if (IsFeatureEnabled(settings, ProductFeaturesEnum.Width))
                        physicalCaracteristics.Add(input.Width);

                    if (IsFeatureEnabled(settings, ProductFeaturesEnum.Height))
                        physicalCaracteristics.Add(input.Height);

                    if (IsFeatureEnabled(settings, ProductFeaturesEnum.Length))
                        physicalCaracteristics.Add(input.Length);

                    output.MiscFeatures = miscFeatures.ToArray();
                    output.PhysicalCaracteristics = physicalCaracteristics.ToArray();

                    output.Categories = IsFeatureEnabled(settings, ProductFeaturesEnum.Categories) ? parseStringIntoArray(input.Categories) : new float[0];
                    output.Manufacturers = IsFeatureEnabled(settings, ProductFeaturesEnum.Manufacturers) ? parseStringIntoArray(input.Manufacturers) : new float[0];
                    output.ProductAttributes = IsFeatureEnabled(settings, ProductFeaturesEnum.ProductAttributes) ? parseStringIntoArray(input.ProductAttributes) : new float[0];
                };

                var pipeline =
                    mlContext.Transforms.CustomMapping<ProductInputModel, ProductOutputModel>(mapping, "ProductCustomMapping")

                    .Append(mlContext.Transforms.Text.NormalizeText("Name", null, keepDiacritics: false, keepPunctuations: false, keepNumbers: false))
                    .Append(mlContext.Transforms.Text.NormalizeText("MetaTitle", null, keepDiacritics: false, keepPunctuations: false, keepNumbers: false))
                    .Append(mlContext.Transforms.Text.NormalizeText("ShortDescription", null, keepDiacritics: false, keepPunctuations: false, keepNumbers: false))
                    .Append(mlContext.Transforms.Text.NormalizeText("FullDescription", null, keepDiacritics: false, keepPunctuations: false, keepNumbers: false))
                    .Append(mlContext.Transforms.Text.NormalizeText("MetaKeywords", null, keepDiacritics: false, keepPunctuations: false, keepNumbers: false))

                    .Append(mlContext.Transforms.Text.TokenizeIntoWords("TokensName", "Name"))
                    .Append(mlContext.Transforms.Text.TokenizeIntoWords("TokensMetaTitle", "MetaTitle"))
                    .Append(mlContext.Transforms.Text.TokenizeIntoWords("TokensShortDescription", "ShortDescription"))
                    .Append(mlContext.Transforms.Text.TokenizeIntoWords("TokensFullDescription", "FullDescription"))
                    .Append(mlContext.Transforms.Text.TokenizeIntoWords("TokensMetaKeywords", "MetaKeywords"))

                    .Append(mlContext.Transforms.Text.ApplyWordEmbedding("NameFeaturized", "TokensName", WordEmbeddingEstimator.PretrainedModelKind.GloVe50D))
                    .Append(mlContext.Transforms.Text.ApplyWordEmbedding("MetaTitleFeaturized", "TokensMetaTitle", WordEmbeddingEstimator.PretrainedModelKind.GloVe50D))
                    .Append(mlContext.Transforms.Text.ApplyWordEmbedding("ShortDescriptionFeaturized", "TokensShortDescription", WordEmbeddingEstimator.PretrainedModelKind.GloVe50D))
                    .Append(mlContext.Transforms.Text.ApplyWordEmbedding("FullDescriptionFeaturized", "TokensFullDescription", WordEmbeddingEstimator.PretrainedModelKind.GloVe50D))
                    .Append(mlContext.Transforms.Text.ApplyWordEmbedding("MetaKeywordsFeaturized", "TokensMetaKeywords", WordEmbeddingEstimator.PretrainedModelKind.GloVe50D))
                ;

                var emptydata = mlContext.Data.LoadFromEnumerable(new List<ProductInputModel>());
                var transformer = pipeline.Fit(emptydata);

                var predictionEngine = mlContext.Model.CreatePredictionEngine<ProductInputModel, ProductOutputModel>(transformer);

                return predictionEngine;
            });

            return task;
        }

        private async Task<IDataView> LoadProducts(MLContext mlContext)
        {
            var dataSettings = DataSettingsManager.LoadSettings();
            var connectionString = dataSettings.ConnectionString;
            var sqlStatement = "";

            using (var sr = new StreamReader("Plugins/Recommendations.SimilarProducts/SqlScripts/load-products.sql"))
            {
                sqlStatement = await sr.ReadToEndAsync();
            }

            var task = await Task.Run(() => {
                var dbSource = new DatabaseSource(SqlClientFactory.Instance, connectionString, sqlStatement);
                DatabaseLoader loader = mlContext.Data.CreateDatabaseLoader<ProductInputModel>();
                IDataView data = loader.Load(dbSource);
                return data;
            });

            return task;
        }

        private async Task SaveSimilarProducts(IEnumerable<SimilarProductRecord> records)
        {
            await _similarProductRecordRepository.TruncateAsync(resetIdentity: true);

            var createdOnUtc = DateTime.UtcNow;
            foreach (var record in records)
            {
                record.CreatedOnUtc = createdOnUtc;
                await _similarProductRecordRepository.InsertAsync(record);
            }
        }

        private bool IsFeatureEnabled(FeaturesConfigurationRecord settings, ProductFeaturesEnum feature)
        {
            return ((ProductFeaturesEnum)settings.ProductFeaturesEnabled & feature) > 0;
        }

        #endregion Private Methods
    }
}
