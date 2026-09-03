using FreelancerTrack.Application.Common.Interfaces;

namespace FreelancerTrack.Infrastructure.Services;

public class CurrencyExchangeService : ICurrencyExchangeService
{
    public string SystemBaseCurrency => "USD";

    // Rates relative to USD (1 unit of Foreign Currency = X USD)
    private static readonly Dictionary<string, decimal> ExchangeRates = new(StringComparer.OrdinalIgnoreCase)
    {
        { "USD", 1.000000m },
        { "EUR", 1.085000m }, // 1 EUR = 1.085 USD
        { "GBP", 1.295000m }, // 1 GBP = 1.295 USD
        { "PKR", 0.003600m }, // 1 PKR = 0.0036 USD (approx 278 PKR/USD)
        { "CAD", 0.735000m }, // 1 CAD = 0.735 USD
        { "AUD", 0.655000m }  // 1 AUD = 0.655 USD
    };

    public decimal GetExchangeRateToBase(string fromCurrency, string? baseCurrency = null)
    {
        string targetBase = baseCurrency ?? SystemBaseCurrency;

        if (string.Equals(fromCurrency, targetBase, StringComparison.OrdinalIgnoreCase))
        {
            return 1.000000m;
        }

        if (!ExchangeRates.TryGetValue(fromCurrency, out decimal fromRateInUsd))
        {
            throw new NotSupportedException($"Currency '{fromCurrency}' is not supported in the exchange rate engine.");
        }

        if (string.Equals(targetBase, "USD", StringComparison.OrdinalIgnoreCase))
        {
            return fromRateInUsd;
        }

        if (!ExchangeRates.TryGetValue(targetBase, out decimal baseRateInUsd))
        {
            throw new NotSupportedException($"Target base currency '{targetBase}' is not supported.");
        }

        return Math.Round(fromRateInUsd / baseRateInUsd, 6, MidpointRounding.AwayFromZero);
    }
}
