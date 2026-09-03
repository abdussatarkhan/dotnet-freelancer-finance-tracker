using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Invoices.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Invoices.Queries.GetInvoiceById;

public record GetInvoiceByIdQuery(Guid Id) : IRequest<Result<InvoiceDto>>;

public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetInvoiceByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Client)
            .Include(i => i.Project)
            .Include(i => i.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        if (invoice == null)
        {
            return Result<InvoiceDto>.Failure($"Invoice with ID '{request.Id}' was not found.", "INVOICE_NOT_FOUND");
        }

        var dto = new InvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.ClientId,
            invoice.Client.Name,
            invoice.ProjectId,
            invoice.Project?.Name,
            invoice.RecurringProfileId,
            invoice.IssueDateUtc,
            invoice.DueDateUtc,
            invoice.TransactionCurrency,
            invoice.BaseCurrency,
            invoice.ExchangeRateToBase,
            invoice.SubTotal,
            invoice.TaxRatePercentage,
            invoice.TaxAmount,
            invoice.DiscountAmount,
            invoice.TotalAmount,
            invoice.TotalAmountInBaseCurrency,
            invoice.AmountPaid,
            invoice.BalanceDue,
            invoice.Status,
            invoice.Notes,
            invoice.PaymentTerms,
            invoice.Items.Select(i => new InvoiceItemDto(
                i.Id,
                i.InvoiceId,
                i.Description,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice,
                i.ItemType,
                i.MilestoneId,
                i.ExpenseId)).ToList(),
            invoice.CreatedAtUtc);

        return Result<InvoiceDto>.Success(dto);
    }
}
