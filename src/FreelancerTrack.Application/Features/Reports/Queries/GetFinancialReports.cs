using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Reports.DTOs;
using FreelancerTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Reports.Queries;

// 1. Net Cash Flow
public record GetNetCashFlowReportQuery(DateTimeOffset? FromDate = null, DateTimeOffset? ToDate = null) : IRequest<Result<NetCashFlowReportDto>>;

public class GetNetCashFlowReportQueryHandler : IRequestHandler<GetNetCashFlowReportQuery, Result<NetCashFlowReportDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrencyExchangeService _currencyService;

    public GetNetCashFlowReportQueryHandler(IApplicationDbContext context, ICurrencyExchangeService currencyService)
    {
        _context = context;
        _currencyService = currencyService;
    }

    public async Task<Result<NetCashFlowReportDto>> Handle(GetNetCashFlowReportQuery request, CancellationToken cancellationToken)
    {
        var invoiceQuery = _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.PartiallyPaid)
            .AsNoTracking();

        var expenseQuery = _context.Expenses.AsNoTracking();

        if (request.FromDate.HasValue)
        {
            invoiceQuery = invoiceQuery.Where(i => i.IssueDateUtc >= request.FromDate.Value);
            expenseQuery = expenseQuery.Where(e => e.DateUtc >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            invoiceQuery = invoiceQuery.Where(i => i.IssueDateUtc <= request.ToDate.Value);
            expenseQuery = expenseQuery.Where(e => e.DateUtc <= request.ToDate.Value);
        }

        var paidInvoices = await invoiceQuery.ToListAsync(cancellationToken);
        var expenses = await expenseQuery.ToListAsync(cancellationToken);

        decimal totalRevenue = paidInvoices.Sum(i => i.AmountPaid * i.ExchangeRateToBase);
        decimal totalExpenses = expenses.Sum(e => e.GrossAmountInBaseCurrency);
        decimal netCashFlow = totalRevenue - totalExpenses;

        var dto = new NetCashFlowReportDto(
            _currencyService.SystemBaseCurrency,
            Math.Round(totalRevenue, 2),
            Math.Round(totalExpenses, 2),
            Math.Round(netCashFlow, 2),
            paidInvoices.Count,
            expenses.Count);

        return Result<NetCashFlowReportDto>.Success(dto);
    }
}

// 2. Accounts Receivable Aging
public record GetAccountsReceivableAgingQuery : IRequest<Result<AccountsReceivableAgingDto>>;

public class GetAccountsReceivableAgingQueryHandler : IRequestHandler<GetAccountsReceivableAgingQuery, Result<AccountsReceivableAgingDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrencyExchangeService _currencyService;

    public GetAccountsReceivableAgingQueryHandler(
        IApplicationDbContext context,
        IDateTimeProvider dateTimeProvider,
        ICurrencyExchangeService currencyService)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _currencyService = currencyService;
    }

    public async Task<Result<AccountsReceivableAgingDto>> Handle(GetAccountsReceivableAgingQuery request, CancellationToken cancellationToken)
    {
        DateTimeOffset now = _dateTimeProvider.UtcNow;

        var unpaidInvoices = await _context.Invoices
            .Where(i => (i.Status == InvoiceStatus.Issued || i.Status == InvoiceStatus.PartiallyPaid || i.Status == InvoiceStatus.Overdue) && i.BalanceDue > 0)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        decimal current = 0m;
        decimal d1To30 = 0m;
        decimal d31To60 = 0m;
        decimal d61To90 = 0m;
        decimal dOver90 = 0m;

        foreach (var inv in unpaidInvoices)
        {
            decimal balanceInBase = inv.BalanceDue * inv.ExchangeRateToBase;

            if (inv.DueDateUtc >= now)
            {
                current += balanceInBase;
            }
            else
            {
                int daysPastDue = (int)(now - inv.DueDateUtc).TotalDays;
                if (daysPastDue <= 30)
                {
                    d1To30 += balanceInBase;
                }
                else if (daysPastDue <= 60)
                {
                    d31To60 += balanceInBase;
                }
                else if (daysPastDue <= 90)
                {
                    d61To90 += balanceInBase;
                }
                else
                {
                    dOver90 += balanceInBase;
                }
            }
        }

        decimal totalOutstanding = current + d1To30 + d31To60 + d61To90 + dOver90;

        var dto = new AccountsReceivableAgingDto(
            _currencyService.SystemBaseCurrency,
            Math.Round(current, 2),
            Math.Round(d1To30, 2),
            Math.Round(d31To60, 2),
            Math.Round(d61To90, 2),
            Math.Round(dOver90, 2),
            Math.Round(totalOutstanding, 2));

        return Result<AccountsReceivableAgingDto>.Success(dto);
    }
}

// 3. Project Profitability
public record GetProjectProfitabilityQuery : IRequest<Result<ProjectProfitabilityReportDto>>;

public class GetProjectProfitabilityQueryHandler : IRequestHandler<GetProjectProfitabilityQuery, Result<ProjectProfitabilityReportDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrencyExchangeService _currencyService;

    public GetProjectProfitabilityQueryHandler(IApplicationDbContext context, ICurrencyExchangeService currencyService)
    {
        _context = context;
        _currencyService = currencyService;
    }

    public async Task<Result<ProjectProfitabilityReportDto>> Handle(GetProjectProfitabilityQuery request, CancellationToken cancellationToken)
    {
        var projects = await _context.Projects
            .Include(p => p.Client)
            .Include(p => p.Invoices)
            .Include(p => p.Expenses)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var list = new List<ProjectProfitabilityItemDto>();

        foreach (var p in projects)
        {
            // Calculate total invoiced revenue in base currency (excluding voided invoices)
            decimal invoicedRevenue = p.Invoices
                .Where(i => i.Status != InvoiceStatus.Void)
                .Sum(i => i.TotalAmountInBaseCurrency);

            // Calculate total expenses incurred on this project
            decimal projectExpenses = p.Expenses
                .Sum(e => e.GrossAmountInBaseCurrency);

            decimal netProfit = invoicedRevenue - projectExpenses;
            decimal margin = invoicedRevenue > 0
                ? Math.Round((netProfit / invoicedRevenue) * 100m, 2)
                : 0m;

            list.Add(new ProjectProfitabilityItemDto(
                p.Id,
                p.Name,
                p.Client.Name,
                Math.Round(invoicedRevenue, 2),
                Math.Round(projectExpenses, 2),
                Math.Round(netProfit, 2),
                margin));
        }

        var report = new ProjectProfitabilityReportDto(
            _currencyService.SystemBaseCurrency,
            list);

        return Result<ProjectProfitabilityReportDto>.Success(report);
    }
}
