using System;
using Nop.Core;

namespace Nop.Plugin.Recommendations.SimilarProducts.Domains
{
    public class FeaturesConfigurationRecord : BaseEntity
    {
        /// <summary>
        /// Sum of bit flags
        /// </summary>
        public long ProductFeaturesEnabled { get; set; }

        public int NumOfSimilarProductsToDiscover { get; set; }

        public int NumOfSimilarProductsToDisplay { get; set; }

        public double MinAcceptedValueOfSimilarity { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime? UpdatedOnUtc { get; set; }
    }
}
