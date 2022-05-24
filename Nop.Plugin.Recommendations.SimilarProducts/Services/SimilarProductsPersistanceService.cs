using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;
using Nop.Plugin.Recommendations.SimilarProducts.Models;

namespace Nop.Plugin.Recommendations.SimilarProducts.Services
{
    public class SimilarProductsPersistanceService : ISimilarProductsPersistanceService
    {
        #region Fields

        private readonly IRepository<SimilarProductRecord> _similarProductRecordRepository;

        #endregion Fields

        #region Ctor

        public SimilarProductsPersistanceService(IRepository<SimilarProductRecord> similarroductRecordRepository)
        {
            _similarProductRecordRepository = similarroductRecordRepository;
        }

        #endregion Ctor

        public async Task SaveSimilarProducts(IEnumerable<SimilarProductRecord> records)
        {
            await _similarProductRecordRepository.TruncateAsync(resetIdentity: true);

            var createdOnUtc = DateTime.UtcNow;
            foreach (var record in records)
            {
                record.CreatedOnUtc = createdOnUtc;
                await _similarProductRecordRepository.InsertAsync(record);
            }
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
    }
}
