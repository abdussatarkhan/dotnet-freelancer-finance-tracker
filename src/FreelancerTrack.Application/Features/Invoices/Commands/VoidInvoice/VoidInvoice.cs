using FluentValidation;
using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Invoices.DTOs;
using FreelancerTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Invoices.Commands.VoidInvoice;

public record VoidInvoiceCommand(Guid InvoiceId, string Reason) : IRequest<Result<InvoiceDto>>;

public class VoidInvoiceCommandValidator : AbstractValidator<VoidInvoiceCommand>
{
    public VoidInvoiceCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public class VoidInvoiceCommandHandler : IRequestHandler<VoidInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;

    public VoidInvoiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<InvoiceDto>> Handle(VoidInvoiceCommand request, CancellationToken cancellationToken)
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

        if (invoice.Status == InvoiceStatus.Paid)
        {
            return Result<InvoiceDto>.Failure("Cannot void a paid invoice. Issue a refund or credit note instead.", "CANNOT_VOID_PAID");
        }

        invoice.Status = InvoiceStatus.Void;
        invoice.Notes = string.IsNullOrWhiteSpace(invoice.Notes)
            ? $"Voided Reason: {request.Reason}"
            : $"{invoice.Notes} | Voided Reason: {request.Reason}";

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
