using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;

namespace Nop.Plugin.Recommendations.SimilarProducts.Models
{
    public class ConfigurationModel
    {
        [BindProperty]
        public List<ProductFeatureConfiguration> Features { get; set; }

        [Required]
        [Range(1, 30, ErrorMessage = "Value should fall in range [1, 30]")]
        public int NumOfSimilarProductsToDiscover { get; set; }

        [Required]
        [Range(1, 30, ErrorMessage = "Value should fall in range [1, 30]")]
        public int NumOfSimilarProductsToDisplay { get; set; }

        [Required]
        [Range(0.1, 1, ErrorMessage = "Value should fall in range [0.1, 1]")]
        public double MinAcceptedValueOfSimilarity { get; set; }

        public ConfigurationModel()
        {
            Features = Enum.GetValues<ProductFeaturesEnum>()
                .Select(f => new ProductFeatureConfiguration(f, false))
                .ToList();
        }

        public ConfigurationModel(ProductFeaturesEnum enabledFeatures)
        {
            Features = Enum.GetValues<ProductFeaturesEnum>()
                .Select(f => new ProductFeatureConfiguration(f, (enabledFeatures & f) > 0))
                .ToList();
        }

        public ConfigurationModel(FeaturesConfigurationRecord record)
            :this((ProductFeaturesEnum)record.ProductFeaturesEnabled)
        {
            NumOfSimilarProductsToDiscover = record.NumOfSimilarProductsToDiscover;
            NumOfSimilarProductsToDisplay = record.NumOfSimilarProductsToDisplay;
            MinAcceptedValueOfSimilarity = record.MinAcceptedValueOfSimilarity;
        }

        public long GetFeaturesAsSumOfFlags()
        {
            ProductFeaturesEnum sum = 0;
            var features = Features.Where(f => f.Enabled).Select(f => f.Feature);
            foreach(var f in features)
            {
                sum |= f;
            }

            return (long)sum;
        }

        public long GetSumOfFeaturesAllEnabled()
        {
            ProductFeaturesEnum sum = 0;

            foreach (var f in Features.Select(f => f.Feature))
            {
                sum |= f;
            }

            return (long)sum;
        }
    }
}
