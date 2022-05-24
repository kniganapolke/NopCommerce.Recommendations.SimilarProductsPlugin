using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Recommendations.SimilarProducts.Services
{
    internal class WeightedSimilaritiesCollection
    {
        private List<(double, double)> _collection;

        public WeightedSimilaritiesCollection()
        {
            _collection = new List<(double, double)>();
        }

        public void AddDistance(float distance, double weight)
        {
            _collection.Add((weight, distance));
        }

        public void AddCosineDistance(float[] vector1, float[] vector2, double weight)
        {
            if (!IsVectorZero(vector1) || !IsVectorZero(vector2))
            {
                var distance = MathNet.Numerics.Distance.Cosine(vector1, vector2);
                if (!float.IsNaN(distance))
                    _collection.Add((weight, distance));
            }
        }

        public void AddJaccardDistance(float[] vector1, float[] vector2, double weight)
        {
            if (!IsVectorZero(vector1) || !IsVectorZero(vector2))
            {
                var distance = MathNet.Numerics.Distance.Jaccard(vector1, vector2);
                if (!double.IsNaN(distance))
                    _collection.Add((weight, distance));
            }
        }

        public void AddPearsonDistance(float[] vector1, float[] vector2, double weight)
        {
            if (!IsVectorZero(vector1) || !IsVectorZero(vector2))
            {
                var distance = MathNet.Numerics.Distance.Pearson(vector1.Select(f => (double)f), vector2.Select(f => (double)f));
                if (!double.IsNaN(distance))
                    _collection.Add((weight, distance));
            }
        }

        public double GetWeightedDistance()
        {
            return _collection.Any() ? _collection.Sum(i => i.Item1 * (1 - i.Item2)) / _collection.Sum(i => i.Item1) : 0;
        }

        private static bool IsVectorZero(float[] vector)
        {
            return vector == null || vector.Length == 0 || vector.All(value => value == 0);
        }
    }
}