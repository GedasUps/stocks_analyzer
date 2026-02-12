namespace RentVest.Web.Models
{
    public class SimulationViewModel
    {
        // --- INPUTS ---
        public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-1);
        public decimal RentAmount { get; set; } = 1000;
        public string Currency { get; set; } = "EUR";
        public string RiskLevel { get; set; } = "Moderate";

        // --- OUTPUTS ---
        public SimulationResult? Results { get; set; }
        public string? ErrorMessage { get; set; }
    }

    // Matches RecommendationResponseDto from API
    public class SimulationResult
    {
        public decimal TotalInvestedUSD { get; set; }
        public decimal TotalValueAfterMonthUSD { get; set; }
        public decimal ProfitOrLossUSD { get; set; }
        public decimal ProfitOrLossPercentage { get; set; }

        public List<PortfolioItem> Portfolio { get; set; } = new();
        public List<NewsItem> RelatedNews { get; set; } = new();
    }

    public class PortfolioItem
    {
        public string Symbol { get; set; }
        public string Type { get; set; }
        public decimal AllocationPercentage { get; set; }
        public decimal AmountInvestedUSD { get; set; }
        public decimal Price { get; set; } // Start Price
        public decimal Profit { get; set; }
        public decimal AveragePrice { get; set; }
    }

    public class NewsItem
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Source { get; set; }
    }
}