using System;
using Nop.Core;

namespace Nop.Plugin.Recommendations.SimilarProducts.Domains
{
    public class SimilarProductRecord : BaseEntity
    {
        public int ProductId { get; set; }
        public int SimilarProductId { get; set; }
        public double Similarity { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}
