using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Recommendations.SimilarProducts.Models;
using Nop.Plugin.Recommendations.SimilarProducts.Services;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Recommendations.SimilarProducts.Components
{
    [ViewComponent(Name = "SimilarProducts")]
    public class SimilarProductsViewComponent : NopViewComponent
    {
        private readonly ISimilarProductsService _simProdService;
        private readonly IAclService _aclService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IFeaturesConfigurationService _configurationService;

        public SimilarProductsViewComponent(
            ISimilarProductsService simProdService,
            IAclService aclService,
            IProductModelFactory productModelFactory,
            IProductService productService,            
            IStoreMappingService storeMappingService,
            IFeaturesConfigurationService configurationService)
        {
            _simProdService = simProdService;
            _aclService = aclService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeMappingService = storeMappingService;
            _configurationService = configurationService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var productId = (int)HttpContext.Request.RouteValues["productid"];

            var settings = await _configurationService.GetAsync();

            var simProducts = (await _simProdService.GetSimilarProductsAsync(productId, settings.NumOfSimilarProductsToDiscover))
                                .OrderByDescending(p => p.Similarity)
                                .ToDictionary(p => p.ProductId);

            var productIds = simProducts.Keys.ToArray();

            //load products
            var products = await (await _productService.GetProductsByIdsAsync(productIds))
            //ACL and store mapping
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => _productService.ProductIsAvailable(p))
            //visible individually
            .Where(p => p.VisibleIndividually)
            .Take(settings.NumOfSimilarProductsToDisplay)
            .ToListAsync();

            if (!products.Any())
                return Content(string.Empty);

            var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, true, true, null))
                .Select(m => new SimilarProductOverviewModel() { Product = m, Similarity = simProducts[m.Id].Similarity })
                .OrderByDescending(m => m.Similarity)
                .ToList();

            return View("~/Plugins/Recommendations.SimilarProducts/Views/SimilarProducts.cshtml", model);
        }
    }
}
