using FluentValidation;
using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Invoices.DTOs;
using FreelancerTrack.Domain.Entities;
using FreelancerTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Invoices.Commands.CreateDraftInvoice;

public record CreateInvoiceItemRequest(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    InvoiceItemType ItemType,
    Guid? MilestoneId = null,
    Guid? ExpenseId = null);

public record CreateDraftInvoiceCommand(
    Guid ClientId,
    Guid? ProjectId,
    DateTimeOffset IssueDateUtc,
    DateTimeOffset DueDateUtc,
    string TransactionCurrency,
    decimal TaxRatePercentage,
    decimal DiscountAmount,
    string? Notes,
    string? PaymentTerms,
    List<CreateInvoiceItemRequest> Items) : IRequest<Result<InvoiceDto>>;

public class CreateDraftInvoiceCommandValidator : AbstractValidator<CreateDraftInvoiceCommand>
{
    public CreateDraftInvoiceCommandValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.DueDateUtc).GreaterThan(x => x.IssueDateUtc).WithMessage("Due date must be after issue date.");
        RuleFor(x => x.TransactionCurrency).NotEmpty().Length(3);
        RuleFor(x => x.TaxRatePercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one invoice line item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description).NotEmpty().MaximumLength(500);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public class CreateDraftInvoiceCommandHandler : IRequestHandler<CreateDraftInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrencyExchangeService _currencyService;

    public CreateDraftInvoiceCommandHandler(IApplicationDbContext context, ICurrencyExchangeService currencyService)
    {
        _context = context;
        _currencyService = currencyService;
    }

    public async Task<Result<InvoiceDto>> Handle(CreateDraftInvoiceCommand request, CancellationToken cancellationToken)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == request.ClientId, cancellationToken);
        if (client == null)
        {
            return Result<InvoiceDto>.Failure($"Client with ID '{request.ClientId}' was not found.", "CLIENT_NOT_FOUND");
        }

        string baseCurrency = _currencyService.SystemBaseCurrency;
        decimal exchangeRate = _currencyService.GetExchangeRateToBase(request.TransactionCurrency, baseCurrency);

        // Generate sequential invoice number (e.g. INV-YYYYMM-XXXX)
        string yearMonth = request.IssueDateUtc.ToString("yyyyMM");
        int countForMonth = await _context.Invoices
            .CountAsync(i => i.InvoiceNumber.StartsWith($"INV-{yearMonth}"), cancellationToken);
        string invoiceNumber = $"INV-{yearMonth}-{(countForMonth + 1):D4}";

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            ClientId = request.ClientId,
            ProjectId = request.ProjectId,
            IssueDateUtc = request.IssueDateUtc,
            DueDateUtc = request.DueDateUtc,
            TransactionCurrency = request.TransactionCurrency.ToUpperInvariant(),
            BaseCurrency = baseCurrency,
            ExchangeRateToBase = exchangeRate,
            TaxRatePercentage = request.TaxRatePercentage,
            DiscountAmount = request.DiscountAmount,
            Status = InvoiceStatus.Draft,
            Notes = request.Notes,
            PaymentTerms = request.PaymentTerms
        };

        foreach (var reqItem in request.Items)
        {
            var item = new InvoiceItem
            {
                InvoiceId = invoice.Id,
                Description = reqItem.Description,
                Quantity = reqItem.Quantity,
                UnitPrice = reqItem.UnitPrice,
                ItemType = reqItem.ItemType,
                MilestoneId = reqItem.MilestoneId,
                ExpenseId = reqItem.ExpenseId
            };
            item.RecalculateTotal();
            invoice.Items.Add(item);

            // If linked to milestone, update milestone status
            if (reqItem.MilestoneId.HasValue)
            {
                var milestone = await _context.Milestones.FirstOrDefaultAsync(m => m.Id == reqItem.MilestoneId.Value, cancellationToken);
                if (milestone != null)
                {
                    milestone.MarkBilled(item.Id);
                }
            }

            // If linked to billable expense, update expense status
            if (reqItem.ExpenseId.HasValue)
            {
                var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == reqItem.ExpenseId.Value, cancellationToken);
                if (expense != null)
                {
                    expense.LinkToInvoice(item.Id);
                }
            }
        }

        invoice.CalculateTotals();

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new InvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.ClientId,
            client.Name,
            invoice.ProjectId,
            null,
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
