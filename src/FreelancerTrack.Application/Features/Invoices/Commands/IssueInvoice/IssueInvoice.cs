using FluentValidation;
using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Invoices.DTOs;
using FreelancerTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Invoices.Commands.IssueInvoice;

public record IssueInvoiceCommand(Guid InvoiceId) : IRequest<Result<InvoiceDto>>;

public class IssueInvoiceCommandValidator : AbstractValidator<IssueInvoiceCommand>
{
    public IssueInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
    }
}

public class IssueInvoiceCommandHandler : IRequestHandler<IssueInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrencyExchangeService _currencyService;

    public IssueInvoiceCommandHandler(IApplicationDbContext context, ICurrencyExchangeService currencyService)
    {
        _context = context;
        _currencyService = currencyService;
    }

    public async Task<Result<InvoiceDto>> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Client)
            .Include(i => i.Project)
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice == null)
        {
            return Result<InvoiceDto>.Failure($"Invoice with ID '{request.InvoiceId}' was not found.", "INVOICE_NOT_FOUND");
        }

        if (invoice.Status != InvoiceStatus.Draft)
        {
            return Result<InvoiceDto>.Failure($"Only draft invoices can be issued. Current status is {invoice.Status}.", "INVALID_INVOICE_STATE");
        }

        // Freeze historical exchange rate at issue time
        invoice.ExchangeRateToBase = _currencyService.GetExchangeRateToBase(invoice.TransactionCurrency, invoice.BaseCurrency);
        invoice.CalculateTotals();
        invoice.Status = InvoiceStatus.Issued;

        await _context.SaveChangesAsync(cancellationToken);

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
