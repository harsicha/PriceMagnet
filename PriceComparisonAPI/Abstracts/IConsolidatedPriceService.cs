using PriceComparisonAPI.Models;

namespace PriceComparisonAPI.Abstracts
{
    public interface IConsolidatedPriceService
    {
        List<ConsolidatedModel> StartService(string query);
    }
}
