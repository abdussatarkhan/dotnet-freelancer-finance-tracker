using FluentValidation;
using FreelancerTrack.Application.Common.Interfaces;
using FreelancerTrack.Application.Common.Models;
using FreelancerTrack.Application.Features.Expenses.DTOs;
using FreelancerTrack.Domain.Entities;
using FreelancerTrack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FreelancerTrack.Application.Features.Expenses.Commands.LogExpense;

public record LogExpenseCommand(
    Guid? ProjectId,
    string Title,
    string? Vendor,
    ExpenseCategory Category,
    DateTimeOffset DateUtc,
    decimal GrossAmount,
    decimal TaxAmount,
    string Currency,
    PaymentMethod PaymentMethod,
    bool IsBillable) : IRequest<Result<ExpenseDto>>;

public class LogExpenseCommandValidator : AbstractValidator<LogExpenseCommand>
{
    public LogExpenseCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.GrossAmount).GreaterThan(0);
        RuleFor(x => x.TaxAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.DateUtc).NotEmpty();
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.PaymentMethod).IsInEnum();
    }
}

public class LogExpenseCommandHandler : IRequestHandler<LogExpenseCommand, Result<ExpenseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrencyExchangeService _currencyService;

    public LogExpenseCommandHandler(IApplicationDbContext context, ICurrencyExchangeService currencyService)
    {
        _context = context;
        _currencyService = currencyService;
    }

    public async Task<Result<ExpenseDto>> Handle(LogExpenseCommand request, CancellationToken cancellationToken)
    {
        string? projectName = null;
        if (request.ProjectId.HasValue)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == request.ProjectId.Value, cancellationToken);
            if (project == null)
            {
                return Result<ExpenseDto>.Failure($"Project with ID '{request.ProjectId}' was not found.", "PROJECT_NOT_FOUND");
            }
            projectName = project.Name;
        }

        string baseCurrency = _currencyService.SystemBaseCurrency;
        decimal rate = _currencyService.GetExchangeRateToBase(request.Currency, baseCurrency);

        var expense = new Expense
        {
            ProjectId = request.ProjectId,
            Title = request.Title,
            Vendor = request.Vendor,
            Category = request.Category,
            DateUtc = request.DateUtc,
            GrossAmount = request.GrossAmount,
            TaxAmount = request.TaxAmount,
            Currency = request.Currency.ToUpperInvariant(),
            ExchangeRateToBase = rate,
            PaymentMethod = request.PaymentMethod,
            IsBillable = request.IsBillable,
            ReimbursedStatus = request.IsBillable ? ExpenseReimbursedStatus.Unbilled : ExpenseReimbursedStatus.NotBillable
        };

        expense.CalculateBaseAmount();

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ExpenseDto(
            expense.Id,
            expense.ProjectId,
            projectName,
            expense.Title,
            expense.Vendor,
            expense.Category,
            expense.DateUtc,
            expense.GrossAmount,
            expense.TaxAmount,
            expense.Currency,
            expense.ExchangeRateToBase,
            expense.GrossAmountInBaseCurrency,
            expense.PaymentMethod,
            expense.IsBillable,
            expense.ReimbursedStatus,
            expense.InvoiceItemId,
            expense.InvoicedAtUtc,
            new List<ReceiptAttachmentDto>(),
            expense.CreatedAtUtc);

        return Result<ExpenseDto>.Success(dto);
    }
}
