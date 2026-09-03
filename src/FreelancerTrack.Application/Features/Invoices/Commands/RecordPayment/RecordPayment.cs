using FluentValidation;
using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Invoices.DTOs;
using FreelancerTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Invoices.Commands.RecordPayment;

public record RecordInvoicePaymentCommand(Guid InvoiceId, decimal PaymentAmount) : IRequest<Result<InvoiceDto>>;

public class RecordInvoicePaymentCommandValidator : AbstractValidator<RecordInvoicePaymentCommand>
{
    public RecordInvoicePaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.PaymentAmount).GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
    }
}

public class RecordInvoicePaymentCommandHandler : IRequestHandler<RecordInvoicePaymentCommand, Result<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;

    public RecordInvoicePaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<InvoiceDto>> Handle(RecordInvoicePaymentCommand request, CancellationToken cancellationToken)
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

        if (invoice.Status == InvoiceStatus.Void)
        {
            return Result<InvoiceDto>.Failure("Cannot record payment against a voided invoice.", "INVOICE_VOID");
        }

        if (invoice.Status == InvoiceStatus.Draft)
        {
            return Result<InvoiceDto>.Failure("Cannot record payment against a draft invoice. Please issue it first.", "INVOICE_NOT_ISSUED");
        }

        if (request.PaymentAmount > invoice.BalanceDue)
        {
            return Result<InvoiceDto>.Failure($"Payment amount of {request.PaymentAmount} exceeds remaining balance of {invoice.BalanceDue}.", "OVERPAYMENT_NOT_ALLOWED");
        }

        invoice.RecordPayment(request.PaymentAmount);

        // If invoice is fully paid, mark any associated milestones as Paid
        if (invoice.Status == InvoiceStatus.Paid)
        {
            var milestoneIds = invoice.Items
                .Where(it => it.MilestoneId.HasValue)
                .Select(it => it.MilestoneId!.Value)
                .ToList();

            if (milestoneIds.Count != 0)
            {
                var milestones = await _context.Milestones
                    .Where(m => milestoneIds.Contains(m.Id))
                    .ToListAsync(cancellationToken);

                foreach (var milestone in milestones)
                {
                    milestone.MarkPaid();
                }
            }
        }

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
