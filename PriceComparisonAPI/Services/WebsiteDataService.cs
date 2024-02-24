using OpenQA.Selenium.Chrome;
using PriceComparisonAPI.Abstracts;
using PriceComparisonAPI.Models;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;

namespace PriceComparisonAPI.Services
{
    public class WebsiteDataService : IWebsiteDataService
    {
        private readonly IWebDriver amazonDriver;
        private readonly IWebDriver flipkartDriver;
        private readonly Object amazonLocker;
        private readonly Object flipkartLocker;
        private readonly IConfiguration _configuration;
        private readonly BaseURLOptions _options;
        private readonly static StreamWriter writer = new StreamWriter(@"C:\Users\harpr\source\repos\PriceComparisonMain\Logs\logs.txt", true);
        
        public WebsiteDataService(IConfiguration configuration, IHostApplicationLifetime applicationLifetime)
        {
            _configuration = configuration;
            _options = _configuration.GetSection(BaseURLOptions.BaseURLs).Get<BaseURLOptions>();
            ChromeOptions chromeOptions = new();
            chromeOptions.AddArgument("headless");
            chromeOptions.AddArgument("no-sandbox");
            amazonDriver = new ChromeDriver(@"Drivers\", chromeOptions);
            flipkartDriver = new ChromeDriver(@"Drivers\", chromeOptions);
            //amazonDriver = new RemoteWebDriver(new Uri("http://localhost:4448"), chromeOptions.ToCapabilities(), TimeSpan.FromSeconds(180));
            //flipkartDriver = new RemoteWebDriver(new Uri("http://localhost:4444"), chromeOptions);
            amazonLocker = new Object();
            flipkartLocker = new Object();
            writer.AutoFlush = true;

            applicationLifetime.ApplicationStopping.Register(OnShutDown);
        }

        public void OnShutDown()
        {
            amazonDriver.Dispose();
            flipkartDriver.Dispose();
        }

        public dynamic GetAmazonDriver(string query)
        {
            lock(amazonLocker)
            {

                writer.WriteLine($"Getting Amazon Driver at {DateTime.Now} for query {query}");
                amazonDriver.Navigate().GoToUrl(_options.AmazonBaseURL + query);
                writer.WriteLine($"Returning Amazon Driver at {DateTime.Now} for query {query}");
                return amazonDriver;
                
            }
        }

        public dynamic GetFlipkartDriver(string query)
        {
            lock(flipkartLocker)
            {

                writer.WriteLine($"Getting Flipkart Driver at {DateTime.Now} for query {query}");
                flipkartDriver.Navigate().GoToUrl(_options.FlipkartBaseURL + query);
                writer.WriteLine($"Returning Flipkart Driver at {DateTime.Now} for query {query}");
                return flipkartDriver;
                
            }
        }
    }
}
