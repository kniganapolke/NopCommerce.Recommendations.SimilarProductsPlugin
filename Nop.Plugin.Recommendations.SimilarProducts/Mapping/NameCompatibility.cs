using System;
using System.Collections.Generic;
using Nop.Data.Mapping;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;

namespace Nop.Plugin.Recommendations.SimilarProducts.Mapping
{
    public partial class NameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>()
        {
            { typeof(FeaturesConfigurationRecord), "Recommendations_SimilarProducts_Configuration" },
            { typeof(SimilarProductRecord), "Recommendations_SimilarProducts_Similarities" },
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>()
        {
        };
    }
}