using OpenQA.Selenium.Chrome;

namespace PriceComparisonAPI.Abstracts
{
    public interface IWebsiteDataService
    {
        dynamic GetAmazonDriver(string query);
        dynamic GetFlipkartDriver(string query);
    }
}
