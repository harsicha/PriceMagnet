namespace PriceComparisonAPI.Models
{
    public class ConsolidatedModel
    {
        public ConsolidatedModel()
        {
            Departments = new List<string>();
        }
        public IList<string> Departments { get; set; }
        public IList<Common> Commons { get; set; }
    }

    public class Common
    {
        public Common(string vendor)
        {
            DeliveryInformation = new DeliveryInformation();
            IdentificationInformation = new Identification();
            ProductPrice = new PriceModel();
            VendorName = vendor;
            MoreInfo = new List<string>();
            Reviews = new Reviews();
        }
        public Identification IdentificationInformation { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public PriceModel ProductPrice { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductLink { get; set; } = string.Empty;
        public string ProductImageLink { get; set; } = string.Empty;
        public string ExchangeMaxValue { get; set; } = string.Empty;
        public DeliveryInformation DeliveryInformation { get; set; }
        public IList<string> MoreInfo { get; set; }
        //public string Rating { get; set; }
        public Reviews Reviews { get; set; }
    }

    public class DeliveryInformation
    {
        public string Type { get; set; } = String.Empty;
        public string GetItBy { get; set; } = String.Empty;
    }
    public class Identification
    {
        // Like asin, fsin, etc
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public class PriceModel
    {
        public string Symbol { get; set; }
        public string Price { get; set; }
    }

    public class Reviews
    {
        public string Link { get; set; }
        public string Count { get; set; }
    }
}
