﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Recommendations.SimilarProducts.Domains;
using Nop.Plugin.Recommendations.SimilarProducts.Models;
using Nop.Plugin.Recommendations.SimilarProducts.Services;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Recommendations.Controllers
{
    public class RecommendationsSimilarProductsController : BasePluginController
    {
        private readonly IFeaturesConfigurationService _configurationService;
        private readonly ISimilarProductsService _similarProductsService;

        public RecommendationsSimilarProductsController(
            IFeaturesConfigurationService configurationService,
            ISimilarProductsService similarProductsService)
        {
            _configurationService = configurationService;
            _similarProductsService = similarProductsService;
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

            return await Task.FromResult((IActionResult)View("~/Plugins/Recommendations.SimilarProducts/Views/Configure.cshtml", model));
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

            return await Task.FromResult((IActionResult)View("~/Plugins/Recommendations.SimilarProducts/Views/Configure.cshtml", model));
        }

        [AutoValidateAntiforgeryToken]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> TrainModel()
        {
            var settings = await _configurationService.GetAsync();

            var model = settings is null ?
                new ConfigurationModel() :
                new ConfigurationModel(settings);

            await _similarProductsService.TrainModelAndSaveSimilarProductsAsync(settings);

            return await Task.FromResult((IActionResult)View("~/Plugins/Recommendations.SimilarProducts/Views/Configure.cshtml", model));
        }
    }
}