using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Invoices.DTOs;
using FreelancerTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Invoices.Queries.GetInvoices;

public record GetInvoicesQuery(
    Guid? ClientId = null,
    Guid? ProjectId = null,
    InvoiceStatus? Status = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null) : IRequest<Result<IReadOnlyList<InvoiceDto>>>;

public class GetInvoicesQueryHandler : IRequestHandler<GetInvoicesQuery, Result<IReadOnlyList<InvoiceDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetInvoicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<InvoiceDto>>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Invoices
            .Include(i => i.Client)
            .Include(i => i.Project)
            .Include(i => i.Items)
            .AsNoTracking();

        if (request.ClientId.HasValue)
        {
            query = query.Where(i => i.ClientId == request.ClientId.Value);
        }

        if (request.ProjectId.HasValue)
        {
            query = query.Where(i => i.ProjectId == request.ProjectId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(i => i.Status == request.Status.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(i => i.IssueDateUtc >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(i => i.IssueDateUtc <= request.ToDate.Value);
        }

        var invoices = await query
            .OrderByDescending(i => i.IssueDateUtc)
            .Select(i => new InvoiceDto(
                i.Id,
                i.InvoiceNumber,
                i.ClientId,
                i.Client.Name,
                i.ProjectId,
                i.Project != null ? i.Project.Name : null,
                i.RecurringProfileId,
                i.IssueDateUtc,
                i.DueDateUtc,
                i.TransactionCurrency,
                i.BaseCurrency,
                i.ExchangeRateToBase,
                i.SubTotal,
                i.TaxRatePercentage,
                i.TaxAmount,
                i.DiscountAmount,
                i.TotalAmount,
                i.TotalAmountInBaseCurrency,
                i.AmountPaid,
                i.BalanceDue,
                i.Status,
                i.Notes,
                i.PaymentTerms,
                i.Items.Select(item => new InvoiceItemDto(
                    item.Id,
                    item.InvoiceId,
                    item.Description,
                    item.Quantity,
                    item.UnitPrice,
                    item.TotalPrice,
                    item.ItemType,
                    item.MilestoneId,
                    item.ExpenseId)).ToList(),
                i.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<InvoiceDto>>.Success(invoices);
    }
}
