namespace FreelancerTrack.Application.Common.Interfaces;

public interface ICurrencyExchangeService
{
    string SystemBaseCurrency { get; }
    decimal GetExchangeRateToBase(string fromCurrency, string? baseCurrency = null);
}
