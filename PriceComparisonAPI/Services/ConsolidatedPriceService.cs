using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PriceComparisonAPI.Abstracts;
using PriceComparisonAPI.Models;

namespace PriceComparisonAPI.Services
{
    public class ConsolidatedPriceService : IConsolidatedPriceService
    {
        private readonly ILogger<ConsolidatedPriceService> _logger;
        private readonly IConfiguration _configuration;
        private readonly BaseURLOptions _options;
        private readonly IWebsiteDataService _siteDataService;
        public ConsolidatedPriceService(IConfiguration configuration, ILogger<ConsolidatedPriceService> logger, IWebsiteDataService dataService)
        {
            _configuration = configuration;
            _logger = logger;
            _options = _configuration.GetSection(BaseURLOptions.BaseURLs).Get<BaseURLOptions>();
            _siteDataService = dataService;
            
        }

        public List<ConsolidatedModel> StartService(string query)
        {
            _logger.LogInformation($"Service started at {DateTime.Now} with search query as {query}");
            List<ConsolidatedModel> result = new();
            try
            {
                List<Task> tasks = new();
                tasks.Add(Task.Run(() =>
                {
                    return GetAmazonData(query, _siteDataService.GetAmazonDriver(query));
                }));
                tasks.Add(Task.Run(() =>
                {
                    return GetFlipkartData(query, _siteDataService.GetFlipkartDriver(query));
                }));

                foreach (Task<dynamic> task in tasks)
                {
                    task.Wait();
                    result.Add((ConsolidatedModel)task.Result);
                }
                _logger.LogInformation($"Service ended at {DateTime.Now} with search query as {query}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in StartService: " + ex.Message);
            }
            return result;
            
        }

        private ChromeDriver GetChromeDriver()
        {
            ChromeOptions chromeOptions = new();
            chromeOptions.AddArgument("headless");
            return new ChromeDriver(@"C:\Users\harpr\source\repos\PriceComparisonMain\Drivers", chromeOptions);
        }

        private ConsolidatedModel GetAmazonData(string query, IWebDriver _chromeDriver)
        {
            _logger.LogInformation($"GetAmazonData started!");
            ConsolidatedModel model = new ConsolidatedModel();
            List<Common> result = new List<Common>();
            //string AMZBase = _options.AmazonBaseURL + query;
            //_chromeDriver.Navigate().GoToUrl(AMZBase);
            var list = _chromeDriver.FindElements(By.XPath("/html/body/div[1]/div[2]/div[1]/div[1]/div/span[3]/div[2]/div[@data-component-id]"));
            var count = list.Count;

            IList<string> departments = new List<string>();
            var noOfDepts = _chromeDriver.FindElements(By.CssSelector("#departments ul[class^='a-unordered-list'] li[id^='n/']")).Count;
            var dept1 = _chromeDriver.FindElements(By.CssSelector("#departments ul[class^='a-unordered-list'] li[id^='n/'] span[class='a-list-item'] span")).ToList();
            var dept2 = _chromeDriver.FindElements(By.CssSelector("#filter-n a[id^='n/'] span")).ToList();
            dept1.ForEach(x =>
            {
                if (!string.IsNullOrEmpty(x.Text))
                {
                    departments.Add(x.Text);
                }
            });
            dept2.ForEach(x =>
            {
                if (!string.IsNullOrEmpty(x.Text))
                {
                    departments.Add(x.Text);
                }
            });
            model.Departments = departments;
            _logger.LogInformation($"Recieved {count} products from Amazon for query {query}");
            for (int i = 3; i <= count; i++)
            {
                Common common = new("Amazon");
                try
                {
                    var asin = _chromeDriver.FindElement(By.XPath($"/html/body/div[1]/div[2]/div[1]/div[1]/div/span[3]/div[2]/div[{i}]")).GetAttribute("data-asin");
                    if (asin == null || asin.Length != 10) continue;

                    var price = _chromeDriver.FindElements(By.XPath($"/html/body/div[1]/div[2]/div[1]/div[1]/div/span[3]/div[2]/div[{i}]//span[@class='a-price-whole']")).FirstOrDefault();
                    var symbol = _chromeDriver.FindElements(By.XPath($"/html/body/div[1]/div[2]/div[1]/div[1]/div/span[3]/div[2]/div[{i}]//span[@class='a-price-symbol']")).FirstOrDefault();
                    if (price == null || symbol == null)
                    {
                        _logger.LogInformation($"Price/symbol for asin: {asin} is null");
                        continue;
                    }
                    common.IdentificationInformation.Type = "asin";
                    common.IdentificationInformation.Value = asin;
                    common.ProductPrice.Price = price.Text;
                    if (symbol != null) common.ProductPrice.Symbol = symbol.Text;
                                                                      
                    var link1 = _chromeDriver.FindElements(By.XPath($"/html/body/div[1]/div[2]/div[1]/div[1]/div/span[3]/div[2]/div[{i}]//a[@class='a-link-normal s-no-outline']")).FirstOrDefault();
                    var name1 = _chromeDriver.FindElements(By.XPath($"/html/body/div[1]/div[2]/div[1]/div[1]/div/span[3]/div[2]/div[{i}]//span[@class='a-size-medium a-color-base a-text-normal']")).FirstOrDefault();
                    if (link1 != null)
                    {
                        common.ProductLink = link1.GetAttribute("href");
                    }
                    else
                    {
                        var link2 = _chromeDriver.FindElements(By.XPath($"/html/body/div[1]/div[2]/div[1]/div[1]/div/span[3]/div[2]/div[{i}]//a[@class='a-link-normal s-underline-text s-underline-link-text s-link-style a-text-normal']")).FirstOrDefault();
                        common.ProductLink = link2 == null ? String.Empty : link2.GetAttribute("href");
                    }
                    if (name1 != null)
                    {
                        common.ProductName = name1.Text;
                    }
                    else
                    {
                        var name2 = _chromeDriver.FindElements(By.XPath($"/html/body/div[1]/div[2]/div[1]/div[1]/div/span[3]/div[2]/div[{i}]//span[@class='a-size-base-plus a-color-base a-text-normal']")).FirstOrDefault();
                        common.ProductName = name2 == null ? String.Empty : name2.Text;
                    }

                    var img = _chromeDriver.FindElements(By.XPath($"/html/body/div[1]/div[2]/div[1]/div[1]/div/span[3]/div[2]/div[{i}]//img[@class='s-image']")).FirstOrDefault();
                    if (img != null) common.ProductImageLink = img.GetAttribute("src");

                    var dType = _chromeDriver.FindElements(By.XPath($"/html/body/div[1]/div[2]/div[1]/div[1]/div/span[3]/div[2]/div[{i}]//span[@aria-label='FREE Delivery by Amazon']")).FirstOrDefault();
                    if (dType != null) common.DeliveryInformation.Type = dType.Text;

                    var getItBy = _chromeDriver.FindElements(By.XPath($"/html/body/div[1]/div[2]/div[1]/div[1]/div/span[3]/div[2]/div[{i}]//span[@class='a-color-base a-text-bold']")).FirstOrDefault();
                    if (getItBy != null) common.DeliveryInformation.GetItBy = getItBy.Text;
                                                                       
                    //var rating = _chromeDriver.FindElements(By.XPath($"/html/body/div[1]/div[2]/div[1]/div[1]/div/span[3]/div[2]/div[{i}]/div/div/div/div/div/div/div/div[2]/div/div/div[2]/div/span[1]/span/a/i[1]/span")).FirstOrDefault();
                    //if (rating != null)
                    //{
                    //    common.Rating = rating.Text;
                    //}

                    var reviewCount = _chromeDriver.FindElements(By.XPath($"/html/body/div[1]/div[2]/div[1]/div[1]/div/span[3]/div[2]/div[{i}]//span[@class='a-size-base s-underline-text']")).FirstOrDefault();
                    var reviewLink = _chromeDriver.FindElements(By.XPath($"/html/body/div[1]/div[2]/div[1]/div[1]/div/span[3]/div[2]/div[{i}]//a[@class='a-link-normal s-underline-text s-underline-link-text s-link-style']")).FirstOrDefault();
                    if (reviewCount != null)
                    {
                        common.Reviews.Count = reviewCount.Text + " Reviews";
                    }
                    if (reviewLink != null)
                    {
                        common.Reviews.Link = reviewLink.GetAttribute("href");
                    }
                    result.Add(common);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in Amazon Service: " + ex.Message);
                }
            }
            model.Commons = result;
            return model;
        }

        private ConsolidatedModel GetFlipkartData(string query, ChromeDriver _chromeDriver)
        {
            _logger.LogInformation($"GetFlipkartData started!");
            ConsolidatedModel model = new ConsolidatedModel();
            List<Common> result = new();
            //string FlipkartBaseURL = _options.FlipkartBaseURL + query;
            //_chromeDriver.Navigate().GoToUrl(FlipkartBaseURL);
            var list = _chromeDriver.FindElements(By.XPath("/html/body/div/div/div[3]/div[1]/div[2]//div[@class='_13oc-S']"));
            var count = list.Count;
            IList<string> departments = new List<string>();
            int noOfDept = _chromeDriver.FindElements(By.XPath($"/html/body/div[1]/div/div[3]/div/div[1]/div/div[1]/div/div/section//div[@class='TB_InB']")).Count;
            for (int i = 2; i < noOfDept + 2; i++)
            {
                var dept = _chromeDriver.FindElements(By.XPath($"/html/body/div[1]/div/div[3]/div/div[1]/div/div[1]/div/div/section/div[{i}]//a")).FirstOrDefault();
                if (dept != null)
                {
                    departments.Add(dept.GetAttribute("title"));
                }
            }
            model.Departments = departments;
            //_logger.LogInformation("-------------------------------------------");
            //foreach (var dept in departments)
            //{
            //    _logger.LogInformation(dept);
            //}
            //_logger.LogInformation("-------------------------------------------");
            _logger.LogInformation($"Recieved {count} products from Flipkart for query {query}");
            for (int i = 2; i <= count + 1; i++)
            {
                
                try
                {
                    bool IsAvailable = _chromeDriver.FindElements(By.XPath($"/html/body/div/div/div[3]/div[1]/div[2]/div[{i}]//span[@class='_192laR']")).Count <= 0;
                    if (!IsAvailable) continue;
                    int size = _chromeDriver.FindElements(By.XPath($"/html/body/div/div/div[3]/div[1]/div[2]/div[{i}]//div[@data-id]")).Count;
                    
                    _logger.LogInformation("Children Count: " + size);
                    if (size > 1)
                    {
                        for (int j = 1; j <= size; j++)
                        {
                            Common common = new(query);
                            common.VendorName = "Flipkart";
                            var fsin = _chromeDriver.FindElement(By.XPath($"/html/body/div/div/div[3]/div[1]/div[2]/div[{i}]/div/div[{j}]")).GetAttribute("data-id");
                            if (fsin == null) continue;
                            
                            var name = _chromeDriver.FindElements(By.XPath($"/html/body/div/div/div[3]/div[1]/div[2]/div[{i}]/div/div[{j}]//a[@class='s1Q9rs']")).FirstOrDefault();
                            if (name == null) continue;

                            common.ProductName = name.GetAttribute("title");
                            common.IdentificationInformation.Type = "fsin";
                            common.IdentificationInformation.Value = fsin;

                            var price = _chromeDriver.FindElements(By.XPath($"/html/body/div/div/div[3]/div[1]/div[2]/div[{i}]/div/div[{j}]//div[@class='_30jeq3']")).FirstOrDefault();
                            if (price == null) continue;
                            else
                            {
                                common.ProductPrice.Symbol = price.Text[..1];
                                common.ProductPrice.Price = price.Text[1..];
                            }
                            var link = _chromeDriver.FindElements(By.XPath($"/html/body/div/div/div[3]/div[1]/div[2]/div[{i}]/div/div[{j}]//a[@class='s1Q9rs']")).FirstOrDefault();
                            if (link != null) common.ProductLink = link.GetAttribute("href");
                            
                            var img = _chromeDriver.FindElements(By.XPath($"/html/body/div/div/div[3]/div[1]/div[2]/div[{i}]/div/div[{j}]//img[@class='_396cs4 _3exPp9']")).FirstOrDefault();
                            if (img != null) common.ProductImageLink = img.GetAttribute("src");

                            var reviewcount = _chromeDriver.FindElements(By.XPath($"/html/body/div/div/div[3]/div[1]/div[2]/div[{i}]/div/div[{j}]//span[@class='_2_R_DZ']")).FirstOrDefault();
                            if (reviewcount != null)
                            {
                                common.Reviews.Count = reviewcount.Text.Replace("(", "").Replace(")", "") + " Reviews";
                            }


                            result.Add(common);
                        }
                    }
                    else
                    {
                        Common common = new(query);
                        common.VendorName = "Flipkart";
                        var fsin = _chromeDriver.FindElement(By.XPath($"/html/body/div/div/div[3]/div[1]/div[2]/div[{i}]/div/div")).GetAttribute("data-id");
                        if (fsin == null) continue;
                        common.IdentificationInformation.Type = "fsin";
                        common.IdentificationInformation.Value = fsin;

                        var name = _chromeDriver.FindElements(By.XPath($"/html/body/div/div/div[3]/div[1]/div[2]/div[{i}]/div/div/div/a/div[2]/div[1]/div[1]")).FirstOrDefault();
                        if (name == null) continue;
                        else
                        {
                            var value = name.GetAttribute("title");
                            common.ProductName = string.IsNullOrEmpty(value) ? name.Text : value;
                        }

                        var price = _chromeDriver.FindElements(By.XPath($"/html/body/div/div/div[3]/div[1]/div[2]/div[{i}]/div/div/div/a/div[2]/div[2]/div[1]/div/div[1]")).FirstOrDefault();
                        if (price == null) continue;
                        else
                        {
                            common.ProductPrice.Symbol = price.Text[..1];
                            common.ProductPrice.Price = price.Text[1..];
                        }

                        var link = _chromeDriver.FindElements(By.XPath($"/html/body/div/div/div[3]/div[1]/div[2]/div[{i}]/div/div/div/a")).FirstOrDefault();
                        if (link != null) common.ProductLink = link.GetAttribute("href");
                                                                        
                        var img = _chromeDriver.FindElements(By.XPath($"/html/body/div/div/div[3]/div[1]/div[2]/div[{i}]/div/div/div/a/div[1]/div[1]/div/div/img")).FirstOrDefault();
                        if (img != null) common.ProductImageLink = img.GetAttribute("src");
                                                                          
                        var dType = _chromeDriver.FindElements(By.XPath($"/html/body/div[1]/div/div[3]/div/div[2]/div[{i}]/div/div/div/a/div[2]/div[2]//div[@class='_2Tpdn3']")).FirstOrDefault();
                        if (dType != null)
                        {
                            common.DeliveryInformation.Type = dType.Text;
                        }

                        IList<string> moreInfoList = new List<string>();
                        var moreInfo = _chromeDriver.FindElements(By.XPath($"/html/body/div[1]/div/div[3]/div[1]/div[2]/div[{i}]/div/div/div/a/div[2]/div//div[@class='fMghEO']//ul[@class='_1xgFaf']/li[@class='rgWa7D']")).ToList();
                        moreInfo.ForEach(x =>
                        {
                            var info = x.Text;
                            if (info != null)
                            {
                                moreInfoList.Add(info);
                            }
                        });
                        common.MoreInfo = moreInfoList;

                        var reviewcount = _chromeDriver.FindElements(By.XPath($"/html/body/div[1]/div/div[3]/div[1]/div[2]/div[{i}]//span[@class='_2_R_DZ']/span/span[3]")).FirstOrDefault();
                        if (reviewcount != null)
                        {
                            common.Reviews.Count = reviewcount.Text.Remove(0, 1);
                        }

                        result.Add(common);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in Flipkart Service: " + ex.Message);
                }
                
            }
            model.Commons = result;
            return model;
        }
    }
}
