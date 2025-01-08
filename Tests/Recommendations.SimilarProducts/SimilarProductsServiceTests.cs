using FluentAssertions;
using Microsoft.ML;
using Moq;
using Nop.Data.Configuration;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;
using Nop.Plugin.Recommendations.SimilarProducts.Models;
using Nop.Plugin.Recommendations.SimilarProducts.Models.ML;
using Nop.Plugin.Recommendations.SimilarProducts.Services;
using NUnit.Framework;

namespace Nop.Plugin.Recommendations.SimilarProducts.Tests
{
    [TestFixture]
    public class SimilarProductsServiceTests
    {
        List<SimilarProductRecord> _persistedSimilarities;

        [SetUp]
        public void Setup()
        {
            _persistedSimilarities = null;
        }

        [Test]
        public async Task NameSimilarityTestAsync()
        {
            var products = GetNameSimilarityTestProducts();
            var productsMap = products.ToDictionary(p => p.Id);

            var productsLoaderServiceMock = GetProductsLoaderServiceMock(products);
            var similarityPersistanceService = GetSimilarProductsPersistanceServiceMock();
            var service = new SimilarProductsDiscoveryService(productsLoaderServiceMock.Object, similarityPersistanceService.Object);

            var settings = GetFeaturesConfig(productsMap.Count, new[] { ProductFeaturesEnum.Name });

            await service.TrainModelAndSaveSimilarProductsAsync(settings, new DataConfig());

            _persistedSimilarities.Should().NotBeNullOrEmpty("Similarities were not generated / saved.");

            foreach(var product in products)
            {
                TestContext.Out.WriteLine($"Similarities for product #{product.Id} ({product.Name})");

                var simProducts1 = _persistedSimilarities.Where(s => s.ProductId == product.Id).OrderByDescending(s => s.Similarity);
                foreach (var sp in simProducts1)
                {
                    if(product.Id == sp.SimilarProductId)
                    {
                        // Check that a product is similar to itself
                        sp.Similarity.Should().BeGreaterThanOrEqualTo(0.99, "A product is not similar to itself.");
                    }

                    TestContext.Out.WriteLine($"{sp.Similarity} #{sp.SimilarProductId} ({productsMap[sp.SimilarProductId].Name})");
                }
            }

            // Check that products 1 and 5 are the least similar
            var leastSimilarProduct = _persistedSimilarities.Where(s => s.ProductId == 1).OrderBy(s => s.Similarity).First();
            leastSimilarProduct.SimilarProductId.Should().Be(5, $"Products #1 ({productsMap[1].Name}) and #5 ({productsMap[5].Name}) are not least similar.");
        }

        [Test]
        public async Task TwoSimilarProductsAllFeaturesTestAsync()
        {
            var products = GetTwoSimilarTestProducts();
            var productsMap = products.ToDictionary(p => p.Id);

            var productsLoaderServiceMock = GetProductsLoaderServiceMock(products);
            var similarityPersistanceService = GetSimilarProductsPersistanceServiceMock();
            var service = new SimilarProductsDiscoveryService(productsLoaderServiceMock.Object, similarityPersistanceService.Object);

            var settings = GetFeaturesConfig(productsMap.Count, (ProductFeaturesEnum[])Enum.GetValues(typeof(ProductFeaturesEnum)));

            await service.TrainModelAndSaveSimilarProductsAsync(settings, new DataConfig());

            _persistedSimilarities.Should().NotBeNullOrEmpty("Similarities were not generated / saved.");

            foreach (var product in products)
            {
                TestContext.Out.WriteLine($"Similarities for product #{product.Id} ({product.Name})");

                var simProducts1 = _persistedSimilarities.Where(s => s.ProductId == product.Id).OrderByDescending(s => s.Similarity);
                foreach (var sp in simProducts1)
                {
                    if (product.Id == sp.SimilarProductId)
                    {
                        // Check that a product is similar to itself
                        sp.Similarity.Should().BeGreaterThanOrEqualTo(0.99, "A product is not similar to itself.");
                    }
                    else
                    {
                        // Check that different products have high similarity
                        sp.Similarity.Should().BeGreaterThanOrEqualTo(0.95, "Products are expected to have >= 0.95 similarity.");
                    }

                    TestContext.Out.WriteLine($"{sp.Similarity} #{sp.SimilarProductId} ({productsMap[sp.SimilarProductId].Name})");
                }
            }
        }

        [Test]
        public async Task TwoNotSimilarProductsAllFeaturesTest()
        {
            var products = GetTwoNotSimilarTestProducts();
            var productsMap = products.ToDictionary(p => p.Id);

            var productsLoaderServiceMock = GetProductsLoaderServiceMock(products);
            var similarityPersistanceService = GetSimilarProductsPersistanceServiceMock();
            var service = new SimilarProductsDiscoveryService(productsLoaderServiceMock.Object, similarityPersistanceService.Object);

            var settings = GetFeaturesConfig(productsMap.Count, (ProductFeaturesEnum[])Enum.GetValues(typeof(ProductFeaturesEnum)));

            await service.TrainModelAndSaveSimilarProductsAsync(settings, new DataConfig());

            _persistedSimilarities.Should().NotBeNullOrEmpty("Similarities were not generated / saved.");

            foreach (var product in products)
            {
                TestContext.Out.WriteLine($"Similarities for product #{product.Id} ({product.Name})");

                var simProducts1 = _persistedSimilarities.Where(s => s.ProductId == product.Id).OrderByDescending(s => s.Similarity);
                foreach (var sp in simProducts1)
                {
                    if (product.Id == sp.SimilarProductId)
                    {
                        // Check that a product is similar to itself
                        sp.Similarity.Should().BeGreaterThanOrEqualTo(0.99, "A product is not similar to itself.");
                    }
                    else
                    {
                        // Check that different products have low similarity
                        sp.Similarity.Should().BeLessThanOrEqualTo(0.55, "Products are expected to have <= 0.25 similarity.");
                    }

                    TestContext.Out.WriteLine($"{sp.Similarity} #{sp.SimilarProductId} ({productsMap[sp.SimilarProductId].Name})");
                }
            }
        }

        #region Private Methods

        private IEnumerable<ProductInputModel> GetNameSimilarityTestProducts()
        {
            yield return new ProductInputModel
            {
                Id = 1,
                Name = "Fruit Salad"
            };

            yield return new ProductInputModel
            {
                Id = 2,
                Name = "best fruit salad"
            };

            yield return new ProductInputModel
            {
                Id = 3,
                Name = "fruit salad"
            };

            yield return new ProductInputModel
            {
                Id = 4,
                Name = "Fruit basket"
            };

            yield return new ProductInputModel
            {
                Id = 5,
                Name = "airplane ticket"
            };
        }

        private IEnumerable<ProductInputModel> GetTwoSimilarTestProducts()
        {
            yield return new ProductInputModel
            {
                Id = 1,
                Name = "Product 1",
                FullDescription = "Product 1 of first and second category",
                MetaKeywords = "A,B,C",
                MetaTitle = "Product 1",
                ShortDescription = "Product 1",
                Manufacturers = "3,4,5",                
                ProductAttributes = "8,9,10",
                Categories = "1,2",
                ProductTypeId = 2,                
                VendorId = 3,
                ParentGroupedProductId = 45,
                Price = 10,                
                Height = 3,
                Length = 3,
                Weight = 3,
                Width = 3,
                IsDownload = false,
                IsRecurring = false,
                RequireOtherProducts = false,
            };

            yield return new ProductInputModel
            {
                Id = 2,
                Name = "Product 2",
                FullDescription = "Product 2 of first and second category",
                MetaKeywords = "A,B,C",
                MetaTitle = "Product 2",
                ShortDescription = "Product 2",
                Manufacturers = "3,4,5",
                ProductAttributes = "8,9,10",
                Categories = "1,2",
                ProductTypeId = 2,
                VendorId = 3,
                ParentGroupedProductId = 45,
                Price = 12,
                Height = 3.5f,
                Length = 3.5f,
                Weight = 3.1f,
                Width = 3.5f,
                IsDownload = false,
                IsRecurring = false,
                RequireOtherProducts = false,
            };
        }

        private IEnumerable<ProductInputModel> GetTwoNotSimilarTestProducts()
        {
            yield return new ProductInputModel
            {
                Id = 1,
                Name = "Product 1",
                FullDescription = "Product 1 of first and second category",
                MetaKeywords = "A,B,C",
                MetaTitle = "Product 1",
                ShortDescription = "Product 1",
                Manufacturers = "3,4,5",
                ProductAttributes = "8,9,10",
                Categories = "1,2",
                ProductTypeId = 2,
                VendorId = 3,
                ParentGroupedProductId = 45,
                Price = 10,
                Height = 3,
                Length = 3,
                Weight = 3,
                Width = 3,
                IsDownload = false,
                IsRecurring = false,
                RequireOtherProducts = false,
            };

            yield return new ProductInputModel
            {
                Id = 2,
                Name = "Digital Service 2",
                FullDescription = "Digital service 2 for your business",
                MetaKeywords = "one,two",
                MetaTitle = "Digital Service 2",
                ShortDescription = "Digital Service 2",
                Manufacturers = "12",
                ProductAttributes = "99,100",
                Categories = "8",
                ProductTypeId = 90,
                VendorId = 30,
                ParentGroupedProductId = 0,
                Price = 120,
                Height = 0,
                Length = 0,
                Weight = 0,
                Width = 0,
                IsDownload = true,
                IsRecurring = true,
                RequireOtherProducts = true,
            };
        }

        private FeaturesConfigurationRecord GetFeaturesConfig(int productsCount, params ProductFeaturesEnum[] enabledFeatures)
        {
            var config = new ConfigurationModel
            {
                Features = enabledFeatures.Select(f => new ProductFeatureConfiguration(f, true)).ToList()
            };

            var settings = new FeaturesConfigurationRecord
            {
                MinAcceptedValueOfSimilarity = 0,
                NumOfSimilarProductsToDiscover = productsCount,
                NumOfSimilarProductsToDisplay = productsCount,
                ProductFeaturesEnabled = config.GetFeaturesAsSumOfFlags()
            };

            return settings;
        }

        private Mock<IProductsLoaderService> GetProductsLoaderServiceMock(IEnumerable<ProductInputModel> products)
        {
            var mock = new Mock<IProductsLoaderService>();

            mock.Setup(m => m.LoadProducts(It.IsAny<MLContext>(), It.IsAny<DataConfig>()))
                .Returns(Task.FromResult(products));

            return mock;
        }

        private Mock<ISimilarProductsPersistanceService> GetSimilarProductsPersistanceServiceMock()
        {
            var mock = new Mock<ISimilarProductsPersistanceService>();

            mock.Setup(s => s.SaveSimilarProducts(It.IsAny<IEnumerable<SimilarProductRecord>>()))
                .Callback<IEnumerable<SimilarProductRecord>>((records) => { _persistedSimilarities = records.ToList(); });

            return mock;
        }

        #endregion Private Methods
    }
}
