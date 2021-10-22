using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Recommendations.SimilarProducts.Services;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Plugins;
using NopTasks = Nop.Services.Tasks;
using Nop.Web.Framework.Infrastructure;
using Nop.Plugin.Recommendations.SimilarProducts.ScheduleTasks;
using Nop.Services.Localization;

namespace Nop.Plugin.Recommendations.SimilarProducts
{
    public class SimilarProductsPlugin : BasePlugin, IMiscPlugin, IWidgetPlugin
    {
        #region Fields

        private IWebHelper _webHelper;
        private readonly IFeaturesConfigurationService _configurationService;
        private readonly NopTasks.IScheduleTaskService _scheduleTaskService;
        private readonly ILocalizationService _localizationService;

        #endregion Fields

        #region Properties

        public bool HideInWidgetList => false;

        #endregion Properties

        #region Ctor

        public SimilarProductsPlugin(
            IFeaturesConfigurationService configurationService,
            NopTasks.IScheduleTaskService scheduleTaskService,
            ILocalizationService localizationService,
            IWebHelper webHelper)
            :base()
        {
            _configurationService = configurationService;
            _scheduleTaskService = scheduleTaskService;
            _localizationService = localizationService;
            _webHelper = webHelper;
        }

        #endregion Ctor

        #region Methods

        public override async Task InstallAsync()
        {
            await InsertLocaleStringResources();
            await InsertScheduleTasks();
            await _configurationService.SaveDefaultConfigurationAsync();

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Recommendations.SimilarProducts");
            await DeleteScheduleTasks();
            await base.UninstallAsync();
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/RecommendationsSimilarProducts/Configure";
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.ProductDetailsBottom });
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "SimilarProducts";
        }

        #endregion Methods

        #region Private Methods

        private async Task InsertScheduleTasks()
        {
            var scheduleTask = new Core.Domain.Tasks.ScheduleTask()
            {
                Name = DiscoverSimilarProductsScheduleTask.Name,
                Type = DiscoverSimilarProductsScheduleTask.Type,
                Seconds = DiscoverSimilarProductsScheduleTask.Seconds
            };

            await _scheduleTaskService.InsertTaskAsync(scheduleTask);
        }

        private async Task DeleteScheduleTasks()
        {
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Recommendations.SimilarProducts");
            var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(DiscoverSimilarProductsScheduleTask.Type);
            await _scheduleTaskService.DeleteTaskAsync(scheduleTask);
        }

        private async Task InsertLocaleStringResources()
        {
            await _localizationService.AddLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Recommendations.SimilarProducts.SimilarProducts"] = "Similar Products",
                ["Plugins.Recommendations.SimilarProducts.CheckProductProperties"] = "Check product's properties to participate in comparison",
                ["Plugins.Recommendations.SimilarProducts.NumberOfSimilarProductsToFind"] = "Number of similar products to find",
                ["Plugins.Recommendations.SimilarProducts.NumberOfSimilarProductsToDisplay"] = "Number of similar products to display",
                ["Plugins.Recommendations.SimilarProducts.MinValueOfSimilarity"] = "Minimal value of similarity that is accepted",
                ["Plugins.Recommendations.SimilarProducts.TrainModel"] = "Train Model"
            });
        }

        #endregion #region Private Methods
    }
}
