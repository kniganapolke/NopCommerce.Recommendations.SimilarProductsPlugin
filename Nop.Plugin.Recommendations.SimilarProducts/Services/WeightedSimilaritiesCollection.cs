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

        public void AddEqualityDistance(float[] vector1, float[] vector2, double weight, bool ignoreZeros)
        {
            if (!IsVectorZero(vector1) || !IsVectorZero(vector2))
            {
                float sum = 0f;
                int count = 0;
                for(var i = 0; i < vector1.Length; i++)
                {
                    if (ignoreZeros && vector1[i] == 0 && vector2[i] == 0)
                        continue;

                    sum += vector1[i] == vector2[i] ? 0 : 1;
                    count++;
                }

                var distance = sum / count;

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