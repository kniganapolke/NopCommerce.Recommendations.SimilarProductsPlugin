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
            var dataSettings = await DataSettingsManager.LoadSettingsAsync();
            var data = await LoadProducts(mlContext, dataSettings);
            var input = mlContext.Data.CreateEnumerable<ProductInputModel>(data, false);

            var predictionEngine = await TrainTextModel(mlContext, settings);            
            
            var transformedTextProducts = predictionEngine != null ?
                input.ToDictionary(p => p.Id, p => {
                var pr = predictionEngine.Predict(p);
                return pr;
            }) : new Dictionary<int, ProductTextOutputModel>();


            var outputModels = input.Select(i => { 
                var model = ToProductOutputModel(i, settings);
                if (transformedTextProducts.ContainsKey(model.Id))
                {
                    var textModel = transformedTextProducts[model.Id];
                    model.NameFeaturized = textModel.NameFeaturized;
                    model.ShortDescriptionFeaturized = textModel.ShortDescriptionFeaturized;
                    model.FullDescriptionFeaturized = textModel.FullDescriptionFeaturized;
                    model.MetaKeywordsFeaturized = textModel.MetaKeywordsFeaturized;
                    model.MetaTitleFeaturized = textModel.MetaTitleFeaturized;
                }

                return model;
            });

            var records = new List<SimilarProductRecord>();

            var products = outputModels.ToDictionary(p => p.Id, p => p);
            foreach (var product in products.Values)
            {
                var similarities = outputModels
                    .Where(p => p.Id != product.Id) // Do not compare with product itself
                    .ToDictionary(p => p.Id, p => Similarity(product, p, settings))
                    .Where(p => p.Value >= settings.MinAcceptedValueOfSimilarity)
                    .OrderByDescending(pair => pair.Value)
                    .Take(settings.NumOfSimilarProductsToDiscover).Select(pair => 
                    new SimilarProductRecord() 
                    { 
                        ProductId = product.Id, 
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
                weightedDistances.AddCosineDistance(p1.NameFeaturized, p2.NameFeaturized, weight: 1);
            }

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.ShortDescription))
            {
                weightedDistances.AddCosineDistance(p1.ShortDescriptionFeaturized, p2.ShortDescriptionFeaturized, weight: 1);
            }

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.FullDescription))
            {
                weightedDistances.AddCosineDistance(p1.FullDescriptionFeaturized, p2.FullDescriptionFeaturized, weight: 1);
            }

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.MetaTitle))
            {
                weightedDistances.AddCosineDistance(p1.MetaTitleFeaturized, p2.MetaTitleFeaturized, weight: 1);
            }

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.MetaKeywords))
            {
                weightedDistances.AddCosineDistance(p1.MetaKeywordsFeaturized, p2.MetaKeywordsFeaturized, weight: 1);
            }

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.Categories))
            {
                weightedDistances.AddPearsonDistance(p1.Categories, p2.Categories, weight: 1);
            }

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.Manufacturers))
            {
                weightedDistances.AddPearsonDistance(p1.Manufacturers, p2.Manufacturers, weight: 1);
            }

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.ProductAttributes))
            {
                weightedDistances.AddPearsonDistance(p1.ProductAttributes, p2.ProductAttributes, weight: 1);
            }

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.Price))
            {
                if(p1.Price > 0 && p2.Price > 0)
                {
                    var distance = 1 - Math.Min(p1.Price, p2.Price) / Math.Max(p1.Price, p2.Price);
                    weightedDistances.AddDistance(distance, weight: 1);
                }
            }

            if (p1.MiscFeatures != null && p1.MiscFeatures.Length > 0 
                && p2.MiscFeatures != null && p2.MiscFeatures.Length > 0)
            {
                weightedDistances.AddPearsonDistance(p1.MiscFeatures, p2.MiscFeatures, weight: 1);
            }

            if (p1.PhysicalCaracteristics != null && p1.PhysicalCaracteristics.Length > 0 
                && p2.PhysicalCaracteristics != null && p2.PhysicalCaracteristics.Length > 0)
            {
                weightedDistances.AddPearsonDistance(p1.PhysicalCaracteristics, p2.PhysicalCaracteristics, weight: 1);
            }
            
            var weightedDistance = weightedDistances.GetWeightedDistance();

            return weightedDistance;
        }

        private Task<PredictionEngine<ProductInputModel, ProductTextOutputModel>> TrainTextModel(MLContext mlContext, FeaturesConfigurationRecord settings)
        {
            if (!IsFeatureEnabled(settings, ProductFeaturesEnum.Name) && !IsFeatureEnabled(settings, ProductFeaturesEnum.ShortDescription)
                && !IsFeatureEnabled(settings, ProductFeaturesEnum.FullDescription) && !IsFeatureEnabled(settings, ProductFeaturesEnum.MetaKeywords)
                && !IsFeatureEnabled(settings, ProductFeaturesEnum.MetaTitle))
                return null;

            return Task.Run(() => {

                var pipeline = mlContext.Transforms.Text.NormalizeText("Name", keepPunctuations: false, keepNumbers: false)
                .Append(mlContext.Transforms.Text.NormalizeText("ShortDescription", keepPunctuations: false, keepNumbers: false))
                .Append(mlContext.Transforms.Text.NormalizeText("FullDescription", keepPunctuations: false, keepNumbers: false))
                .Append(mlContext.Transforms.Text.NormalizeText("MetaKeywords", keepPunctuations: false, keepNumbers: false))
                .Append(mlContext.Transforms.Text.NormalizeText("MetaTitle", keepPunctuations: false, keepNumbers: false))
                .Append(mlContext.Transforms.Text.TokenizeIntoWords("TokensName", "Name"))
                .Append(mlContext.Transforms.Text.TokenizeIntoWords("TokensShortDescription", "ShortDescription"))
                .Append(mlContext.Transforms.Text.TokenizeIntoWords("TokensFullDescription", "FullDescription"))
                .Append(mlContext.Transforms.Text.TokenizeIntoWords("TokensMetaKeywords", "MetaKeywords"))
                .Append(mlContext.Transforms.Text.TokenizeIntoWords("TokensMetaTitle", "MetaTitle"))
                .Append(mlContext.Transforms.Text.ApplyWordEmbedding("NameFeaturized", "TokensName", WordEmbeddingEstimator.PretrainedModelKind.GloVe50D))
                .Append(mlContext.Transforms.Text.ApplyWordEmbedding("ShortDescriptionFeaturized", "TokensShortDescription", WordEmbeddingEstimator.PretrainedModelKind.GloVe50D))
                .Append(mlContext.Transforms.Text.ApplyWordEmbedding("FullDescriptionFeaturized", "TokensFullDescription", WordEmbeddingEstimator.PretrainedModelKind.GloVe50D))
                .Append(mlContext.Transforms.Text.ApplyWordEmbedding("MetaKeywordsFeaturized", "TokensMetaKeywords", WordEmbeddingEstimator.PretrainedModelKind.GloVe50D))
                .Append(mlContext.Transforms.Text.ApplyWordEmbedding("MetaTitleFeaturized", "TokensMetaTitle", WordEmbeddingEstimator.PretrainedModelKind.GloVe50D));

                if (pipeline == null)
                {
                    return null;
                }

                var emptydata = mlContext.Data.LoadFromEnumerable(new List<ProductInputModel>());
                var transformer = pipeline.Fit(emptydata);
                var predictionEngine = mlContext.Model.CreatePredictionEngine<ProductInputModel, ProductTextOutputModel>(transformer);

                return predictionEngine;

            });            
        }

        private ProductOutputModel ToProductOutputModel(ProductInputModel input, FeaturesConfigurationRecord settings)
        {
            Func<string, float[]> parseStringIntoArray = (input) =>
                input?.Split(',').Select(v =>
                {
                    _ = float.TryParse(v, out var r);
                    return r;
                }).ToArray();

            var output = new ProductOutputModel();

            output.Id = input.Id;

            var miscFeatures = new List<float>();
            var physicalCaracteristics = new List<float>();

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.ProductTypeId))
                miscFeatures.Add(input.ProductTypeId);

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.ParentGroupedProductId))
                miscFeatures.Add(input.ParentGroupedProductId);

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.VendorId))
                miscFeatures.Add(input.VendorId);

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

            if (IsFeatureEnabled(settings, ProductFeaturesEnum.Price))
                output.Price = input.Price;

            output.MiscFeatures = miscFeatures.ToArray();
            output.PhysicalCaracteristics = physicalCaracteristics.ToArray();

            output.Categories = IsFeatureEnabled(settings, ProductFeaturesEnum.Categories) ? parseStringIntoArray(input.Categories) : new float[0];
            output.Manufacturers = IsFeatureEnabled(settings, ProductFeaturesEnum.Manufacturers) ? parseStringIntoArray(input.Manufacturers) : new float[0];
            output.ProductAttributes = IsFeatureEnabled(settings, ProductFeaturesEnum.ProductAttributes) ? parseStringIntoArray(input.ProductAttributes) : new float[0];

            return output;
        }

        private Task<IDataView> LoadProducts(MLContext mlContext, DataSettings dataSettings)
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

                return data;

            });            
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
