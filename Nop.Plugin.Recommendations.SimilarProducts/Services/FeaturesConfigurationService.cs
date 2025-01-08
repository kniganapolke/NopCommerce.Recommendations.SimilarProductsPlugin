using Nop.Data;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;
using Nop.Plugin.Recommendations.SimilarProducts.Models;

namespace Nop.Plugin.Recommendations.SimilarProducts.Services
{
    public class FeaturesConfigurationService : IFeaturesConfigurationService
    {
        private readonly int _numOfSimilarProductsToDiscover = 10;
        private readonly int _numOfSimilarProductsToDisplay = 4;
        private readonly double _minAcceptedValueOfSimilarity = 0.75;

        private readonly IRepository<FeaturesConfigurationRecord> _featuresConfigurationRecordRepository;

        public FeaturesConfigurationService(
            IRepository<FeaturesConfigurationRecord> featuresConfigurationRecordRepository)
        {
            _featuresConfigurationRecordRepository = featuresConfigurationRecordRepository;
        }

        /// <summary>
        /// Logs the specified record.
        /// </summary>
        /// <param name="record">The record.</param>
        public async Task AddOrUpdateAsync(FeaturesConfigurationRecord record)
        {
            if (record is null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            if(record.Id == 0)
            {
                record.CreatedOnUtc = DateTime.UtcNow;
                await _featuresConfigurationRecordRepository.InsertAsync(record);
            }
            else
            {
                record.UpdatedOnUtc = DateTime.UtcNow;
                await _featuresConfigurationRecordRepository.UpdateAsync(record);
            }            
        }

        public async Task<FeaturesConfigurationRecord> GetAsync()
        {
            var r = await _featuresConfigurationRecordRepository.GetAllAsync(q => q);

            return r.FirstOrDefault();
        }

        public async Task SaveDefaultConfigurationAsync()
        {
            var record = new FeaturesConfigurationRecord()
            {
                NumOfSimilarProductsToDiscover = _numOfSimilarProductsToDiscover,
                NumOfSimilarProductsToDisplay = _numOfSimilarProductsToDisplay,
                MinAcceptedValueOfSimilarity = _minAcceptedValueOfSimilarity,
                ProductFeaturesEnabled = new ConfigurationModel().GetSumOfFeaturesAllEnabled()
            };

            await AddOrUpdateAsync(record);
        }
    }
}
