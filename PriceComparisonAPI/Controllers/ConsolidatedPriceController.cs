using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PriceComparisonAPI.Abstracts;
using PriceComparisonAPI.Models;

namespace PriceComparisonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsolidatedPriceController : ControllerBase
    {
        private readonly IConsolidatedPriceService _priceService;
        private readonly ILogger<ConsolidatedPriceController> _logger;

        public ConsolidatedPriceController(IConsolidatedPriceService priceService, ILogger<ConsolidatedPriceController> logger)
        {
            _priceService = priceService;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]/{query}")]
        public List<ConsolidatedModel> Get(string query)
        {
            return _priceService.StartService(query);
        }
    }
}
