using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Data;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;
using Nop.Plugin.Recommendations.SimilarProducts.Models;
using Nop.Plugin.Recommendations.SimilarProducts.Services;
using Nop.Services.Logging;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Recommendations.Controllers
{
    public class RecommendationsSimilarProductsController : BasePluginController
    {
        private readonly IFeaturesConfigurationService _configurationService;
        private readonly ISimilarProductsDiscoveryService _similarProductsService;
        private readonly ILogger _logger;

        public RecommendationsSimilarProductsController(
            IFeaturesConfigurationService configurationService,
            ISimilarProductsDiscoveryService similarProductsService,
            ILogger logger)
        {
            _configurationService = configurationService;
            _similarProductsService = similarProductsService;
            _logger = logger;
        }

        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin] //confirms access to the admin panel
        [Area(AreaNames.Admin)] //specifies the area containing a controller or action
        public async Task<IActionResult> Configure()
        {
            var settings = await _configurationService.GetAsync();

            var model = settings is null ?
                new ConfigurationModel() :
                new ConfigurationModel(settings);

            return View("~/Plugins/Recommendations.SimilarProducts/Views/Configure.cshtml", model);
        }

        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            var settings = (await _configurationService.GetAsync()) ?? new FeaturesConfigurationRecord();

            settings.ProductFeaturesEnabled = model.GetFeaturesAsSumOfFlags();
            settings.MinAcceptedValueOfSimilarity = model.MinAcceptedValueOfSimilarity;
            settings.NumOfSimilarProductsToDiscover = model.NumOfSimilarProductsToDiscover;
            settings.NumOfSimilarProductsToDisplay = model.NumOfSimilarProductsToDisplay;

            await _configurationService.AddOrUpdateAsync(settings);

            return View("~/Plugins/Recommendations.SimilarProducts/Views/Configure.cshtml", model);
        }

        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> TrainModel()
        {
            var pluginSettings = await _configurationService.GetAsync();

            var model = pluginSettings is null ?
                new ConfigurationModel() :
                new ConfigurationModel(pluginSettings);

            var appSettings = await DataSettingsManager.LoadSettingsAsync();

            try
            {
                await _similarProductsService.TrainModelAndSaveSimilarProductsAsync(pluginSettings, appSettings);
            }
            catch(InvalidOperationException ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                model.DisplayConfigurationTip = true;
            }

            return View("~/Plugins/Recommendations.SimilarProducts/Views/Configure.cshtml", model);
        }
    }
}
