using Nop.Data;
using Nop.Plugin.Recommendations.SimilarProducts.Services;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Recommendations.SimilarProducts.ScheduleTasks
{
    public class DiscoverSimilarProductsScheduleTask : IScheduleTask
    {
        private readonly ISimilarProductsDiscoveryService _similarProductsService;
        private readonly IFeaturesConfigurationService _configurationService;

        public static string Name = "Discover similar products (Similar Products Plugin)";
        public static string Type = "Nop.Plugin.Recommendations.SimilarProducts.ScheduleTasks.DiscoverSimilarProductsScheduleTask, Nop.Plugin.Recommendations.SimilarProducts";
        public static int Seconds = 604800;

        public DiscoverSimilarProductsScheduleTask(
            ISimilarProductsDiscoveryService similarProductsService,
            IFeaturesConfigurationService configurationService)
        {
            _similarProductsService = similarProductsService;
            _configurationService = configurationService;
        }

        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            var pluginSettings = await _configurationService.GetAsync();
            var appSettings = DataSettingsManager.LoadSettings();
            await _similarProductsService.TrainModelAndSaveSimilarProductsAsync(pluginSettings, appSettings);
        }
    }
}
