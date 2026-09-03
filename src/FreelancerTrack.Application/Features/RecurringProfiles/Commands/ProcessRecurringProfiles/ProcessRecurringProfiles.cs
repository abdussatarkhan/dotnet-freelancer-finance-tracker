using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Domain.Entities;
using FreelancerTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.RecurringProfiles.Commands.ProcessRecurringProfiles;

public record ProcessRecurringProfilesCommand : IRequest<Result<ProcessRecurringProfilesResult>>;

public record ProcessRecurringProfilesResult(int ProcessedProfilesCount, List<Guid> GeneratedInvoiceIds);

public class ProcessRecurringProfilesCommandHandler : IRequestHandler<ProcessRecurringProfilesCommand, Result<ProcessRecurringProfilesResult>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrencyExchangeService _currencyService;

    public ProcessRecurringProfilesCommandHandler(
        IApplicationDbContext context,
        IDateTimeProvider dateTimeProvider,
        ICurrencyExchangeService currencyService)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _currencyService = currencyService;
    }

    public async Task<Result<ProcessRecurringProfilesResult>> Handle(ProcessRecurringProfilesCommand request, CancellationToken cancellationToken)
    {
        DateTimeOffset now = _dateTimeProvider.UtcNow;

        var dueProfiles = await _context.RecurringProfiles
            .Include(rp => rp.Client)
            .Where(rp => rp.IsActive && rp.NextRunDateUtc <= now)
            .ToListAsync(cancellationToken);

        if (dueProfiles.Count == 0)
        {
            return Result<ProcessRecurringProfilesResult>.Success(new ProcessRecurringProfilesResult(0, new List<Guid>()));
        }

        var generatedInvoiceIds = new List<Guid>();
        string baseCurrency = _currencyService.SystemBaseCurrency;

        foreach (var profile in dueProfiles)
        {
            decimal rate = _currencyService.GetExchangeRateToBase(profile.Currency, baseCurrency);

            string yearMonth = now.ToString("yyyyMM");
            int countForMonth = await _context.Invoices
                .CountAsync(i => i.InvoiceNumber.StartsWith($"INV-{yearMonth}"), cancellationToken);
            string invoiceNumber = $"INV-{yearMonth}-{(countForMonth + 1 + generatedInvoiceIds.Count):D4}";

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                ClientId = profile.ClientId,
                ProjectId = profile.ProjectId,
                RecurringProfileId = profile.Id,
                IssueDateUtc = now,
                DueDateUtc = now.AddDays(profile.PaymentDueDays),
                TransactionCurrency = profile.Currency,
                BaseCurrency = baseCurrency,
                ExchangeRateToBase = rate,
                TaxRatePercentage = 0m, // Retainer base rate
                DiscountAmount = 0m,
                Status = InvoiceStatus.Issued,
                Notes = $"Auto-generated invoice from Recurring Profile: {profile.Title}",
                PaymentTerms = $"Net {profile.PaymentDueDays} days"
            };

            var item = new InvoiceItem
            {
                InvoiceId = invoice.Id,
                Description = $"Retainer Billing: {profile.Title} ({profile.Schedule})",
                Quantity = 1m,
                UnitPrice = profile.RetainerAmount,
                ItemType = InvoiceItemType.Retainer
            };
            item.RecalculateTotal();
            invoice.Items.Add(item);

            invoice.CalculateTotals();

            _context.Invoices.Add(invoice);
            generatedInvoiceIds.Add(invoice.Id);

            // Advance schedule for next cycle
            profile.AdvanceSchedule();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<ProcessRecurringProfilesResult>.Success(
            new ProcessRecurringProfilesResult(dueProfiles.Count, generatedInvoiceIds));
    }
}
